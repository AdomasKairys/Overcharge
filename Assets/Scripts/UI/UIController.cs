using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    [Header("Velocity Text Box")]
    public GameObject velocityText;

    [Header("Player Death Menu")]
    public GameObject deathMenu;

    [Header("Pick Up Inventory")]
    [SerializeField] private TextMeshProUGUI pickupName;
    [SerializeField] private Image pickupImage;
    [SerializeField] private Sprite[] pickupSprites;
    [SerializeField] private Sprite noPickupSprite;
    [SerializeField] private GameObject cooldownPickupImage;
    [SerializeField] private InventoryController inventoryController;
    private bool shufflePickups = true;


    TextMeshProUGUI textMesh_playerState;
    TextMeshProUGUI textMesh_velocity;

    // Start is called before the first frame update
    void Start()
    {
        deathMenu.gameObject.SetActive(false);
        textMesh_velocity = velocityText.GetComponent<TextMeshProUGUI>();
        textMesh_playerState = playerStateText.GetComponent<TextMeshProUGUI>();
        if (player != null)
        {
            // Retrieve the player state information
            playerStateController = player.GetComponent<PlayerStateController>();
            // Subscribe to the player death event
            playerStateController.onPlayerDeath.AddListener(ShowDeathMenu);
        }

        // Setup the charge bar
        playerChargeBarSlider = playerChargeBar.GetComponent<Slider>();
        playerChargeBarSlider.minValue = 0;
        playerChargeBarSlider.maxValue = playerStateController.overcharge;
        playerChargeBarSlider.value = playerStateController.currCharge.Value;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the player state information
        if(playerStateController.currState.Value == PlayerState.Chaser)
        {
            playerChargeBarSlider.value = playerStateController.currCharge.Value;
        }        

        // Update the text boxes
        textMesh_velocity.text = player.GetComponent<Rigidbody>().velocity.magnitude.ToString();
        textMesh_playerState.text = playerStateController.currState.Value.ToString();

        UpdatePickup();
    }

    private void ShowDeathMenu()
    {
        playerStateText.SetActive(false);
        playerChargeBar.SetActive(false);
        velocityText.SetActive(false);
        deathMenu.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void OnRespawnButtonClicked()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        deathMenu.SetActive(false);
        playerStateText.SetActive(true);
        playerChargeBarSlider.value = 0;
        playerChargeBar.SetActive(true);
        velocityText.SetActive(true);
        playerStateController.Respawn();
    }

    /// <summary>
    /// Updates the inventory pickup slot text and sprite
    /// </summary>
    private void UpdatePickup()
    {
        if(inventoryController.currentPickup == InventoryController.PickupType.None)
        {
            cooldownPickupImage.SetActive(false);
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
                switch (inventoryController.currentPickup)
                {
                    case InventoryController.PickupType.SpeedBoost:
                        pickupImage.sprite = pickupSprites[0]; break;
                    case InventoryController.PickupType.GravityBomb:
                        pickupImage.sprite = pickupSprites[1]; break;
                }
                pickupName.text = inventoryController.currentPickup.ToString();
                cooldownPickupImage.SetActive(false);
            }
            else
            {
                // TODO: same here
                cooldownPickupImage.SetActive(true);
            }
        }
    }

    private void ShufflePickups()
    {
        pickupImage.sprite = pickupSprites[UnityEngine.Random.Range(0, pickupSprites.Length)];
        shufflePickups = true;
    }
}
