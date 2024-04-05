using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

// Define the possible player states
public enum PlayerState
{
    Chaser,
    Runner,
    Dead
}

public class PlayerStateController : NetworkBehaviour
{
    [Header("Player State")]
    public PlayerState currState = PlayerState.Runner; // Current state of the player, default is Runner TODO: later make private
    public float currCharge = 0.0f; // Current value of charge the player has
    public float chargeRate = 1.0f; // The rate in which the palyer's charge increases
    public float overcharge = 100.0f; // The maximum value of charge at which the player dies

    public UnityEvent onPlayerDeath;

    public NetworkObject pc;
    //[Header("Tagging")]
    //public GameObject tagTrigger;


    // Update is called once per frame
    void Update()
    {
        // Change the player state if Enter is presed (for debugging)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(currState == PlayerState.Runner)
            {
                SetStateServerRPC(pc, PlayerState.Chaser);
            }
            else
            {
                SetStateServerRPC(pc, PlayerState.Runner);
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

    [ServerRpc]
    private void SetStateServerRPC(NetworkObjectReference pc, PlayerState newState)
    {
        SetStateClientRPC(pc, newState);
    }
    [ClientRpc]
    private void SetStateClientRPC(NetworkObjectReference pc, PlayerState newState)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        player.GetComponent<PlayerStateController>().currState = newState;
    }

    // Method for getting the state of the player
    public PlayerState GetState()
    {
        return currState;
    }

    private void Die()
    {
        DieServerRPC(pc); // deactivate the player object
    }
    [ServerRpc]
    private void DieServerRPC(NetworkObjectReference pc)
    {
        DieClientRPC(pc);
    }
    [ClientRpc]
    private void DieClientRPC(NetworkObjectReference pc)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        onPlayerDeath.Invoke();
        var player = networkObject.transform.Find("Player");
        player.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Reset player's health or other states as necessary
        RespawnServerRPC(pc);
    }
    [ServerRpc]
    private void RespawnServerRPC(NetworkObjectReference pc)
    {
        RespawnClientRPC(pc);
    }
    [ClientRpc]
    private void RespawnClientRPC(NetworkObjectReference pc)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        player.position = new Vector3(0, 0, 0);
        player.gameObject.SetActive(true);
        player.GetComponent<PlayerStateController>().currState = PlayerState.Runner;
        player.GetComponent<PlayerStateController>().currCharge = 0.0f;
    }
}
