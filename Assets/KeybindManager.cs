using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class KeybindManager : MonoBehaviour
{
    //move (wasd)
    [Header("Move")]
    public Button rebindUpButton;
    public Button rebindDownButton;
    public Button rebindLeftButton;
    public Button rebindRightButton;
    public TMP_Text upBindingText;
    public TMP_Text downBindingText;
    public TMP_Text leftBindingText;
    public TMP_Text rightBindingText;
    //Jump
    [Header("Jump")]
    public Button rebindJumpButton;
    public TMP_Text jumpBindingText;
    //Primary Secondary
    [Header("Primary Secondary")]
    public Button rebindPrimaryButton;
    public Button rebindSecondaryButton;
    public TMP_Text primaryBindingText;
    public TMP_Text secondaryBindingText;
    //Dash
    [Header("Dash and upwards wall run")]
    public Button rebindDashButton;
    public TMP_Text dashBindingText;
    public Button rebindUpwardsWallRunButton;
    public TMP_Text upwardsWallRunBindingText;
    //Use pickup
    [Header("UsePickup")]
    public Button rebindUsePickupButton;
    public TMP_Text usePickupBindingText;
    //Downwards wallRun
    [Header("Downwards wall run")]
    public Button rebindDownwardsWallRunButton;
    public TMP_Text downwardsWallRunBindingText;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void Start()
    {
        UpdateBindingDisplay();
        //Move
        rebindUpButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "up"));
        rebindDownButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "down"));
        rebindLeftButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "left"));
        rebindRightButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "right"));
        //Jump
        rebindJumpButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.JumpAction, null));
        //Primary Secondary
        rebindPrimaryButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.UsePrimaryEquipment, null,1));
        rebindSecondaryButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.UseSecondaryEquipment, null));
        //Dash and up wall run
        rebindDashButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.DashAction, null));
        rebindUpwardsWallRunButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.UpwardsWallRun, null));
        //use pickup
        rebindUsePickupButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.UsePickup, null));
        //downward wall run
        rebindDownwardsWallRunButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.DownwardWallRun, null));
    }

    private void OnDisable()
    {
        rebindingOperation?.Dispose();
    }

    private void UpdateBindingDisplay()
    {
        int upBindingIndex = FindBindingIndex(GameSettings.Instance.playerInputs.MoveAction, "up");
        int downBindingIndex = FindBindingIndex(GameSettings.Instance.playerInputs.MoveAction, "down");
        int leftBindingIndex = FindBindingIndex(GameSettings.Instance.playerInputs.MoveAction, "left");
        int rightBindingIndex = FindBindingIndex(GameSettings.Instance.playerInputs.MoveAction, "right");
        
        upBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.MoveAction.bindings[upBindingIndex].effectivePath);
        downBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.MoveAction.bindings[downBindingIndex].effectivePath);
        leftBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.MoveAction.bindings[leftBindingIndex].effectivePath);
        rightBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.MoveAction.bindings[rightBindingIndex].effectivePath);


        int jumpBindingIndex = 0;
        jumpBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.JumpAction.bindings[jumpBindingIndex].effectivePath);

        int primaryBindingIndex = 1;
        primaryBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UsePrimaryEquipment.bindings[primaryBindingIndex].effectivePath);
        int secondaryBindingIndex = 0;
        secondaryBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UseSecondaryEquipment.bindings[secondaryBindingIndex].effectivePath);

        int dashBindingIndex = 0;
        dashBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.DashAction.bindings[dashBindingIndex].effectivePath);
        int upwardsWallRunBindingIndex = 0;
        upwardsWallRunBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UpwardsWallRun.bindings[upwardsWallRunBindingIndex].effectivePath);
        int usePickupBindingIndex = 0;
        usePickupBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UsePickup.bindings[usePickupBindingIndex].effectivePath);
        int downwardsWallRunBindingIndex = 0;
        downwardsWallRunBindingText.text = InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.DownwardWallRun.bindings[downwardsWallRunBindingIndex].effectivePath);
    }

    private int FindBindingIndex(InputAction action, string compositePart)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (action.bindings[i].isPartOfComposite && action.bindings[i].name == compositePart)
            {
                return i;
            }
        }
        return -1;
    }

    private void StartRebinding(InputAction action, string compositePart, int index=0)
    {
        int bindingIndex = compositePart != null ? FindBindingIndex(action, compositePart) : index;

        if (bindingIndex == -1 && compositePart != null)
        {
            Debug.LogError($"Binding for {compositePart} not found.");
            return;
        }

        action.Disable();
        rebindingOperation?.Dispose();

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindingComplete(action, compositePart))
            .Start();
    }

    private void RebindingComplete(InputAction action, string compositePart)
    {
        action.Enable();
        UpdateBindingDisplay();
        rebindingOperation?.Dispose();
    }

    public void SaveBindings()
    {
        var rebinds = GameSettings.Instance.playerInputs.MoveAction.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        PlayerPrefs.Save();
    }

    public void LoadBindings()
    {
        var rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
        {
            GameSettings.Instance.playerInputs.MoveAction.LoadBindingOverridesFromJson(rebinds);
            UpdateBindingDisplay();
        }
    }
}
