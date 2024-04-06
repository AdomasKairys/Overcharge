using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
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
    public NetworkVariable<PlayerState> currState = new NetworkVariable<PlayerState>(PlayerState.Runner); // Current state of the player, default is Runner TODO: later make private
    public NetworkVariable<float> currCharge = new NetworkVariable<float>(0.0f); // Current value of charge the player has
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
            SetStateServerRPC(gameObject.GetComponentInParent<NetworkObject>());
        }

        if (currState.Value == PlayerState.Chaser)
        {
            ChangeChargeValueServerRPC(gameObject.GetComponentInParent<NetworkObject>());

            if(currCharge.Value >= overcharge)
            {
                Die();
            }
            // Increase charge for the chaser
        }
    }
    [ServerRpc]
    private void ChangeChargeValueServerRPC(NetworkObjectReference target)
    {
        if (!target.TryGet(out NetworkObject targetObject))
            return;

        targetObject.GetComponentInChildren<PlayerStateController>().currCharge.Value += chargeRate * Time.deltaTime;
    }
    // Method for setting the state of the player
    [ServerRpc]
    private void SetStateServerRPC(NetworkObjectReference target)
    {
        if (!target.TryGet(out NetworkObject targetObject))
            return;

        var currState = targetObject.GetComponentInChildren<PlayerStateController>().currState.Value;

        if (currState == PlayerState.Runner)
        {
            targetObject.GetComponentInChildren<PlayerStateController>().currState.Value = PlayerState.Chaser;
        }
        else
        {
            targetObject.GetComponentInChildren<PlayerStateController>().currState.Value = PlayerState.Runner;
        }
    }
    public void SetState(PlayerState newState)
    {
        currState.Value = newState;
    }

    // Method for getting the state of the player
    public PlayerState GetState()
    {
        return currState.Value;
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
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        player.GetComponent<PlayerStateController>().currState.Value = PlayerState.Runner;
        player.GetComponent<PlayerStateController>().currCharge.Value = 0.0f;
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
    }
}
