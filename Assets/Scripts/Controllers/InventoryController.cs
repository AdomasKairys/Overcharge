using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : NetworkBehaviour
{
    private PlayerInputActions _playerInputActions;

    private InputAction _usePickupAction;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private PlayerStateController _playerStateController;

    public PickupType currentPickup = PickupType.None;

    private int currentPickupUses = 0;

    private float currentPickupCooldown = 0f;

    public bool canUseCurrentPickup = false;

    public bool pickingUp = false;

    private float pickingUpDelay = 3.6f;

	SFXTrigger sfxTrigger;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}

	public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _playerInputActions = new PlayerInputActions();

            _usePickupAction = _playerInputActions.Player.UsePickup;
            _usePickupAction.performed += OnUsePickup;
            _usePickupAction.Enable();
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _usePickupAction.performed -= OnUsePickup;
            _usePickupAction.Disable();
        }

        base.OnNetworkDespawn();
    }

    private void OnUsePickup(InputAction.CallbackContext context)
    {
        if (currentPickup != PickupType.None && canUseCurrentPickup)
        {
            UseCurrentPickUp();
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

        int randPickupIndex = UnityEngine.Random.Range(1, 3);
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
		sfxTrigger.PlaySFX("itemPickUp");
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
            default:
                currentPickup = PickupType.None;
                break;
        }

        pickingUp = false;
        canUseCurrentPickup = true;

        Debug.Log("A new pickup picked up: " + currentPickup.ToString());
    }
    
    /// <summary>
    /// Uses the current pickup and removes it from the inventory if it has no more uses
    /// </summary>
    private void UseCurrentPickUp()
    {
        if (!IsOwner) return;

        if (currentPickup == PickupType.None)
        {
            return;
        }

        canUseCurrentPickup = false;
        currentPickupUses -= 1;

        switch (currentPickup)
        {
            case PickupType.SpeedBoost:
                RequestUseSpeedBoostServerRpc((int)OwnerClientId);
				sfxTrigger.PlaySFX("speedBoost");
				break;
            case PickupType.GravityBomb:
                //Debug.Log("Client " + OwnerClientId + " will request to use gravity bomb");
                RequestUseGravityBombServerRpc(gameObject.transform.position, _playerStateController.GetState() == PlayerState.Chaser, (int)OwnerClientId);
				sfxTrigger.PlaySFX("gravityBomb");
				break;
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
    private void RequestUseGravityBombServerRpc(Vector3 userPosition, bool chaser, int clientId)
    {
        if (!IsServer) return;

        //Debug.Log("Client " + clientId + " requested to use the gravity bomb at position " + userPosition);

        if (!IsServer) return;

        // Iterate through all connected clients
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if ((int)client.ClientId == clientId)
            {

            }
            else
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    var inventoryController = playerObject.transform.Find("Player").GetComponent<InventoryController>();
                    if (inventoryController != null)
                    {
                        // Call the ClientRpc on the InventoryController that the client owns
                        inventoryController.HandleGravityBombClientRpc(userPosition, chaser);
                    }
                    else
                    {
                        Debug.LogError("InventoryController not found on the player object.");
                    }
                }
            }
        }
    }

    [ClientRpc]
    private void HandleGravityBombClientRpc(Vector3 bombUserPosition, bool chaser)
    {
        if(!IsOwner) return;

        //Debug.Log("Client " + OwnerClientId + " had its ClientRpc called");

        float pushForce = 100f;
        float effectiveRange = 8.0f;

        // Calculate distance from the current player to the bomb user
        float distance = Vector3.Distance(transform.position, bombUserPosition);

        Debug.Log("Client " + OwnerClientId + " distance to bomber: " + distance);

        if (distance <= effectiveRange)
        {
            if (chaser)
            {
                //Debug.Log("Client " + OwnerClientId + " uses its PushTo method");
                // Push other players towards the player who used the gravity bomb
                _playerMovement.PushTo(bombUserPosition, pushForce);
            }
            else
            {
                //Debug.Log("Client " + OwnerClientId + " uses its PushFrom method");
                // Push other players away from the player who used the gravity bomb
                _playerMovement.PushFrom(bombUserPosition, pushForce);
            }
        }
        else
        {
            Debug.Log("Client " + OwnerClientId + " is too far away from bomber");
        }
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
