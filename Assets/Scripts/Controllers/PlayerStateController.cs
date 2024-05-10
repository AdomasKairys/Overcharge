using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using System.Linq;


// Define the possible player states


public class PlayerStateController : NetworkBehaviour
{
    [Header("Player State")]
    public NetworkVariable<float> currCharge = new NetworkVariable<float>(0.0f); // Current value of charge the player has
    public float chargeRate = 1.0f; // The rate in which the palyer's charge increases
    public float overcharge = 100.0f; // The maximum value of charge at which the player dies

    public UnityEvent onPlayerDeath;

    public NetworkObject netObj;
    //[Header("Tagging")]
    //public GameObject tagTrigger;

    // Update is called once per frame
    void Update()
    {
        
        // Change the player state if Enter is presed (for debugging)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SetStateServerRPC(netObj);
        }

        if (GetState() == PlayerState.Chaser)
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
    private void SetStateServerRPC(NetworkObjectReference netObjRef)
    {
        if (!netObjRef.TryGet(out NetworkObject networkObject))
            return;

        var currState = GameMultiplayer.Instance.GetPlayerDataFromClientId(networkObject.OwnerClientId);

        if (currState.playerState == PlayerState.Runner)
        {
            GameMultiplayer.Instance.ChangePlayerState(networkObject.OwnerClientId, PlayerState.Chaser);
        }
        else
        {
            GameMultiplayer.Instance.ChangePlayerState(networkObject.OwnerClientId, PlayerState.Runner);
        }
    }
    public void SetState(PlayerState newState)
    {
        GameMultiplayer.Instance.ChangePlayerState(netObj.OwnerClientId, newState);
    }

    // Method for getting the state of the player
    public PlayerState GetState()
    {
        return GameMultiplayer.Instance.GetPlayerDataFromClientId(netObj.OwnerClientId).playerState;
    }

    private void Die()
    {
        DieServerRPC(netObj); // deactivate the player object
    }
    [ServerRpc]
    private void DieServerRPC(NetworkObjectReference netObjRef)
    {
        if (!netObjRef.TryGet(out NetworkObject networkObject))
            return;
        DieClientRPC(netObjRef);
        GameMultiplayer.Instance.ChangePlayerState(networkObject.OwnerClientId, PlayerState.Dead);
        GameManager.Instance.SetRandomPlayerChaser();

    }
    [ClientRpc]
    private void DieClientRPC(NetworkObjectReference netObjRef)
    {
        if (!netObjRef.TryGet(out NetworkObject networkObject))
            return;
        onPlayerDeath.Invoke();
        var player = networkObject.transform.Find("Player");
        player.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Reset player's health or other states as necessary
        RespawnServerRPC(netObj);
    }
    [ServerRpc]
    private void RespawnServerRPC(NetworkObjectReference pc)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        player.GetComponent<PlayerStateController>().SetState(PlayerState.Runner);
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
