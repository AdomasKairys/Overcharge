using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;
    private PlayerStateController playerStateController;

    [Header("Player State Text Box")]
    public GameObject playerStateText;

    [Header("Player Charge Bar")]
    public GameObject playerChargeBar;
    private Slider playerChargeBarSlider;
    public GameObject sliderColor;
    [Header("Velocity Text Box")]
    public GameObject velocityText;

    [Header("Crosshair image")]
    [SerializeField] private Image crosshair;

    [Header("Pick Up Inventory")]
    [SerializeField] private TextMeshProUGUI pickupName;
    [SerializeField] private Image pickupImage;
    [SerializeField] private Sprite[] pickupSprites;
    [SerializeField] private Sprite noPickupSprite;
    [SerializeField] private Image cooldownPickupImage;
    [SerializeField] private InventoryController inventoryController;
    private bool shufflePickups = true;

    [Header("Equipment Inventory")]
    [SerializeField] private TextMeshProUGUI _primaryText;
    [SerializeField] private TextMeshProUGUI _secondaryText;
    public TextMeshProUGUI primaryKeybind;
    public TextMeshProUGUI secondaryKeybind;
    public TextMeshProUGUI pickupKeybind;

    TextMeshProUGUI textMesh_playerState;
    TextMeshProUGUI textMesh_velocity;

    // Start is called before the first frame update
    void Start()
    {
        textMesh_velocity = velocityText.GetComponent<TextMeshProUGUI>();
        textMesh_playerState = playerStateText.GetComponent<TextMeshProUGUI>();
        if (player != null)
        {
            // Retrieve the player state information
            playerStateController = player.GetComponent<PlayerStateController>();
            // Subscribe to the player death event
            playerStateController.OnPlayerDeath += PlayerStateController_OnPlayerDeath; ;
        }

        // Setup the charge bar
        playerChargeBarSlider = playerChargeBar.GetComponent<Slider>();
        playerChargeBarSlider.minValue = 0;
        playerChargeBarSlider.maxValue = playerStateController.overcharge;
        playerChargeBarSlider.value = playerStateController.currCharge.Value;

        sliderColor.GetComponent<Image>().color = Color.blue;

        primaryKeybind.text= InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UsePrimaryEquipment.bindings[1].effectivePath);
        secondaryKeybind.text= InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UseSecondaryEquipment.bindings[0].effectivePath);
        pickupKeybind.text= InputControlPath.ToHumanReadableString(GameSettings.Instance.playerInputs.UsePickup.bindings[0].effectivePath);

        cooldownPickupImage.fillAmount = 0f;
    }

    private void PlayerStateController_OnPlayerDeath(object sender, EventArgs e)
    {
        playerStateText.SetActive(false);
        playerChargeBar.SetActive(false);
        velocityText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Update the player state information
        if (playerStateController.GetState() == PlayerState.Chaser)
        {
            playerChargeBarSlider.value = playerStateController.currCharge.Value;
            sliderColor.GetComponent<Image>().color = Color.red;
        }
		else
		{
            sliderColor.GetComponent<Image>().color = Color.blue;
        }
        // Update the text boxes
        textMesh_velocity.text = player.GetComponent<Rigidbody>().velocity.magnitude.ToString();
        textMesh_playerState.text = playerStateController.GetState().ToString();
        UpdateCrosshair();
        UpdatePickup();
    }

    public void SetEquipment(EquipmentType primary, EquipmentType secondary)
    {
        switch (primary)
        {
            case EquipmentType.GrapplingHook:
                _primaryText.text = "Grappling hook";
                break;
            case EquipmentType.RocketLauncher:
                _primaryText.text = "Rocket launcher";
                break;
        }

        switch (secondary)
        {
            case EquipmentType.GrapplingHook:
                _secondaryText.text = "Grappling hook";
                break;
            case EquipmentType.RocketLauncher:
                _secondaryText.text = "Rocket launcher";
                break;
        }
    }

    public void OnRespawnButtonClicked()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerStateText.SetActive(true);
        playerChargeBarSlider.value = 0;
        playerChargeBar.SetActive(true);
        velocityText.SetActive(true);
        playerStateController.Respawn();
    }
    private void UpdateCrosshair()
    {
        crosshair.transform.position = Input.mousePosition;
    }

    /// <summary>
    /// Updates the inventory pickup slot text and sprite
    /// </summary>
    private void UpdatePickup()
    {
        if(inventoryController.currentPickup == InventoryController.PickupType.None)
        {
            cooldownPickupImage.fillAmount = 0f;
            if (inventoryController.pickingUp)
            {
                pickupName.text = "...";
                if (shufflePickups)
                {
                    Invoke("ShufflePickups", 0.1f);
                    shufflePickups = false;
                }    
            }
            else
            {
                // TODO: not the most effiecent solution to update this every frame
                pickupImage.sprite = noPickupSprite;
                pickupName.text = "None";
            }
        }
        else
        {
            if (inventoryController.canUseCurrentPickup)
            {
                // TODO: not the most effiecent solution to update this every frame
                cooldownPickupImage.fillAmount = 0f;
                switch (inventoryController.currentPickup)
                {
                    case InventoryController.PickupType.SpeedBoost:
                        pickupImage.sprite = pickupSprites[0]; pickupName.text = "Speed boost"; break;
                    case InventoryController.PickupType.GravityBomb:
                        pickupImage.sprite = pickupSprites[1]; pickupName.text = "Gravity bomb"; break;
                }
            }
            else
            {
                cooldownPickupImage.fillAmount = inventoryController.GetRemainingCooldown() / inventoryController.GetCurrentPickupCooldown();
            }
        }
    }

    private void ShufflePickups()
    {
        pickupImage.sprite = pickupSprites[UnityEngine.Random.Range(0, pickupSprites.Length)];
        shufflePickups = true;
    }
}
