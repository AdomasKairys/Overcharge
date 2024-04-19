using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class InventoryController : NetworkBehaviour
{
    [SerializeField]
    private PlayerMovement _playerMovement;

    public PickupType currentPickup = PickupType.None;

    private int currentPickupUses = 0;

    private float currentPickupCooldown = 0f;

    public bool canUseCurrentPickup = false;

    public bool pickingUp = false;

    private float pickingUpDelay = 3f;

 
    void Update()
    {
        if(!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(currentPickup != PickupType.None && canUseCurrentPickup)
            {
                UseCurrentPickUp();
            }
        }
    }

    /// <summary>
    /// Handles the picking up of a pickup block, called by the pickup object
    /// </summary>
    public void HandlePickup()
    {
        if (!IsOwner) return;

        if (!pickingUp)
        {
            pickingUp = true;
            currentPickup = PickupType.None;
            RequestNewPickupServerRpc((int)OwnerClientId);
        }
    }

    [ServerRpc]
    private void RequestNewPickupServerRpc(int clientId)
    {
        if (!IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { (ulong)clientId }
            }
        };

        Debug.Log("Somebody asked for a new pickup");

        int randPickupIndex = UnityEngine.Random.Range(2, 3);
        GetNewPickupClientRpc(randPickupIndex, clientRpcParams);
    }

    [ClientRpc]
    private void GetNewPickupClientRpc(int pickupIndex, ClientRpcParams clientRpcParams = default)
    {
        if(!IsOwner) return;

        StartCoroutine(Pickup(pickupIndex));
    }

    /// <summary>
    /// Method that handles picking up a new pickup
    /// </summary>
    private IEnumerator Pickup(int pickupIndex)
    {
        // Waits the delay
        yield return new WaitForSeconds(pickingUpDelay);

        switch (pickupIndex)
        {
            case 1:
                currentPickup = PickupType.SpeedBoost; 
                currentPickupUses = 3; 
                currentPickupCooldown = 1.5f; 
                break;
            case 2:
                currentPickup = PickupType.GravityBomb;
                currentPickupUses = 1;
                currentPickupCooldown = 0f;
                break;
        }

        pickingUp = false;
        canUseCurrentPickup = true;

        Debug.Log("A new pickup picked up: " + currentPickup.ToString());
    }
    
    // TODO: cia irgi reikia isskirstyti i client / server
    /// <summary>
    /// Uses the current pickup and removes it from the inventory if it has no more uses
    /// </summary>
    private void UseCurrentPickUp()
    {
        if(currentPickup == PickupType.None)
        {
            return;
        }

        canUseCurrentPickup = false;
        currentPickupUses -= 1;

        switch (currentPickup)
        {
            case PickupType.SpeedBoost:
                RequestUseSpeedBoostServerRpc((int)OwnerClientId); break;
            case PickupType.GravityBomb:
                RequestUseGravityBombServerRpc((int)OwnerClientId); break;
        }

        if(currentPickupUses <= 0)
        {
            currentPickup = PickupType.None;
        }
        else
        {
            StartCoroutine(WaitPickupCooldown(currentPickupCooldown));
        }
    }

    [ServerRpc]
    private void RequestUseSpeedBoostServerRpc(int clientId)
    {
        if (!IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { (ulong)clientId }
            }
        };

        UseSpeedBoostClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void UseSpeedBoostClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        if (currentPickupUses > 0)
        {
            Debug.Log("Speed boost used");
            _playerMovement.StartCoroutine(_playerMovement.UseSpeedBoost(2, 2));
        }
    }

    [ServerRpc]
    private void RequestUseGravityBombServerRpc(int clientId)
    {
        if (!IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { (ulong)clientId }
            }
        };

        Debug.Log("Somebody requested to use the gravity bomb");

        UseGravityBombClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void UseGravityBombClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        Debug.Log("Gravity bomb used");
    }

    private IEnumerator WaitPickupCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canUseCurrentPickup = true;
    }

    public enum PickupType
    {
        None = 0,
        SpeedBoost = 1,
        GravityBomb = 2
    }
}
