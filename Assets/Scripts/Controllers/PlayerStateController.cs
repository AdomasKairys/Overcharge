using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using System.Linq;
using System;


// Define the possible player states


public class PlayerStateController : NetworkBehaviour
{
    [Header("Player State")]
    public NetworkVariable<float> currCharge = new NetworkVariable<float>(0.0f); // Current value of charge the player has
    public float chargeRate = 0.25f; // The rate in which the palyer's charge increases
    public float overcharge = 100.0f; // The maximum value of charge at which the player dies

    public event EventHandler OnPlayerDeath;
    private bool isEffectPlaying = false;

	SFXTrigger sfxTrigger;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}

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
            ChangeChargeValueServerRPC(netObj);
			if (currCharge.Value >= overcharge)
			{
				Die();
			}		
            //Increase charge for the chaser
        }

		if (GetState() != PlayerState.Dead)
		{
			if (currCharge.Value >= overcharge / 2 && !isEffectPlaying)
			{
				sfxTrigger.PlaySFX_CanStop("charge", true);
                isEffectPlaying = true;
			}
			else
			{
				sfxTrigger.StopSFX("charge");
				isEffectPlaying = false;
			}
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
        sfxTrigger.PlaySFX("death"); //ChargeExplodes AudioClip

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
        OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        var player = networkObject.transform.Find("Player");
        player.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Reset player's health or other states as necessary
    }
}
