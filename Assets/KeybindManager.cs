using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

public class KeybindManager : MonoBehaviour
{
    public PlayerInputActions playerInputActions;
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
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
        UpdateBindingDisplay();
        rebindUpButton.onClick.AddListener(() => StartRebinding(playerInputActions.Player.Move, "up"));
        rebindDownButton.onClick.AddListener(() => StartRebinding(playerInputActions.Player.Move, "down"));
        rebindLeftButton.onClick.AddListener(() => StartRebinding(playerInputActions.Player.Move, "left"));
        rebindRightButton.onClick.AddListener(() => StartRebinding(playerInputActions.Player.Move, "right"));
    }

    private void OnDisable()
    {
        rebindingOperation?.Dispose();
    }

    private void UpdateBindingDisplay()
    {
        int upBindingIndex = FindBindingIndex(playerInputActions.Player.Move, "up");
        int downBindingIndex = FindBindingIndex(playerInputActions.Player.Move, "down");
        int leftBindingIndex = FindBindingIndex(playerInputActions.Player.Move, "left");
        int rightBindingIndex = FindBindingIndex(playerInputActions.Player.Move, "right");
        upBindingText.text = InputControlPath.ToHumanReadableString(playerInputActions.Player.Move.bindings[upBindingIndex].effectivePath);
        downBindingText.text = InputControlPath.ToHumanReadableString(playerInputActions.Player.Move.bindings[downBindingIndex].effectivePath);
        leftBindingText.text = InputControlPath.ToHumanReadableString(playerInputActions.Player.Move.bindings[leftBindingIndex].effectivePath);
        rightBindingText.text = InputControlPath.ToHumanReadableString(playerInputActions.Player.Move.bindings[rightBindingIndex].effectivePath);
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
        var rebinds = playerInputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        PlayerPrefs.Save();
    }

    public void LoadBindings()
    {
        var rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
        {
            playerInputActions.LoadBindingOverridesFromJson(rebinds);
            UpdateBindingDisplay();
        }
    }
}
