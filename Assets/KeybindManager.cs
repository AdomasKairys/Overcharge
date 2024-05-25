using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class KeybindManager : MonoBehaviour
{
    public Button rebindUpButton;
    public Button rebindDownButton;
    public Button rebindLeftButton;
    public Button rebindRightButton;
    public TMP_Text upBindingText;
    public TMP_Text downBindingText;
    public TMP_Text leftBindingText;
    public TMP_Text rightBindingText;

    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private void Start()
    {
        UpdateBindingDisplay();
        rebindUpButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "up"));
        rebindDownButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "down"));
        rebindLeftButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "left"));
        rebindRightButton.onClick.AddListener(() => StartRebinding(GameSettings.Instance.playerInputs.MoveAction, "right"));
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

    private void StartRebinding(InputAction action, string compositePart)
    {
        int bindingIndex = FindBindingIndex(action, compositePart);
        if (bindingIndex == -1)
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
