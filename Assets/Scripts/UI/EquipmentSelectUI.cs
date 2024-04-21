using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSelectUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _primaryEquipmentDropdown;
    private int _previousPrimaryValue = 0;

    [SerializeField] private TMP_Dropdown _secondaryEquipmentDropdown;
    private int _previousSecondaryValue = 0;

    private void Awake()
    {
        _primaryEquipmentDropdown.onValueChanged.AddListener(PrimaryChanged);

        _secondaryEquipmentDropdown.onValueChanged.AddListener(SecondaryChanged);
    }

    private void PrimaryChanged(int value)
    {
        if (value == _secondaryEquipmentDropdown.value && value != 0)
        {
            // Switch equipment if the same item was selected for both slots
            _primaryEquipmentDropdown.value = _previousSecondaryValue;
            _secondaryEquipmentDropdown.value = _previousPrimaryValue;

            GameMultiplayer.Instance.ChangePlayerEquipment(_primaryEquipmentDropdown.value, _secondaryEquipmentDropdown.value);
        }
        else
        {
            GameMultiplayer.Instance.ChangePlayerEquipment(value, _secondaryEquipmentDropdown.value);
        }

        // Update the previous values after handling the change
        _previousPrimaryValue = _primaryEquipmentDropdown.value;
        _previousSecondaryValue = _secondaryEquipmentDropdown.value;
    }

    private void SecondaryChanged(int value)
    {
        if (value == _primaryEquipmentDropdown.value && value != 0)
        {
            // Switch equipment if the same item was selected for both slots
            _secondaryEquipmentDropdown.value = _previousPrimaryValue;
            _primaryEquipmentDropdown.value = _previousSecondaryValue;

            GameMultiplayer.Instance.ChangePlayerEquipment(_primaryEquipmentDropdown.value, _secondaryEquipmentDropdown.value);
        }
        else
        {
            GameMultiplayer.Instance.ChangePlayerEquipment(_primaryEquipmentDropdown.value, value);
        }

        // Update the previous values after handling the change
        _previousPrimaryValue = _primaryEquipmentDropdown.value;
        _previousSecondaryValue = _secondaryEquipmentDropdown.value;
    }

    private void OnDestroy()
    {
        _primaryEquipmentDropdown.onValueChanged.RemoveListener(PrimaryChanged);
        _secondaryEquipmentDropdown.onValueChanged.RemoveListener(SecondaryChanged);
    }
}
