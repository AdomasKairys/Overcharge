using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Define the possible player states
public enum PlayerState
{
    Chaser,
    Runner,
    Dead
}

public class PlayerStateController : MonoBehaviour
{
    [Header("Player State")]
    public PlayerState currState = PlayerState.Runner; // Current state of the player, default is Runner TODO: later make private
    public float currCharge = 0.0f; // Current value of charge the player has
    public float chargeRate = 1.0f; // The rate in which the palyer's charge increases
    public float overcharge = 100.0f; // The maximum value of charge at which the player dies

    public UnityEvent onPlayerDeath;

    //[Header("Tagging")]
    //public GameObject tagTrigger;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Change the player state if Enter is presed (for debugging)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(currState == PlayerState.Runner)
            {
                SetState(PlayerState.Chaser);
            }
            else
            {
                SetState(PlayerState.Runner);
            }
        }

        if (currState == PlayerState.Chaser)
        {
            if(currCharge >= overcharge)
            {
                Die();
            }
            // Increase charge for the chaser
            currCharge += chargeRate * Time.deltaTime;
        }
    }

    // Method for setting the state of the player
    public void SetState(PlayerState newState)
    {
        currState = newState;
    }

    // Method for getting the state of the player
    public PlayerState GetState()
    {
        return currState;
    }

    private void Die()
    {
        onPlayerDeath.Invoke();
        this.gameObject.SetActive(false); // deactivate the player object
    }

    public void Respawn()
    {
        // Reset player's health or other states as necessary
        currState = PlayerState.Runner;
        currCharge = 0.0f;
        this.transform.position = new Vector3(0, 0, 0);
        this.gameObject.SetActive(true);
    }
}
