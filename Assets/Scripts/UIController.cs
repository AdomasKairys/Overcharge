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
            Debug.Log("The controler is: " + playerStateController);
        }

        // Setup the charge bar
        playerChargeBarSlider = playerChargeBar.GetComponent<Slider>();
        playerChargeBarSlider.minValue = 0;
        playerChargeBarSlider.maxValue = playerStateController.overcharge;
        playerChargeBarSlider.value = playerStateController.currCharge;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the player state information
        if(playerStateController.currState == PlayerState.Chaser)
        {
            playerChargeBarSlider.value = playerStateController.currCharge;
        }        

        // Update the text boxes
        textMesh_velocity.text = player.GetComponent<Rigidbody>().velocity.magnitude.ToString();
        textMesh_playerState.text = playerStateController.currState.ToString();
    }
}
