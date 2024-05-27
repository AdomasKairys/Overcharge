using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : NetworkBehaviour
{
    //private PlayerInputActions _playerInputActions;
    private PlayerInputs _playerInputActionsPlayer;

    private InputAction _usePickupAction;

    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private PlayerStateController _playerStateController;

    [SerializeField]
    private GravityBombEffect _gravityBombEffect;

    [SerializeField]
    private SpeedBoostEffect _speedBoostEffect;

    public PickupType currentPickup = PickupType.None;

    private int currentPickupUses = 0;

    private float currentPickupCooldown = 0f;

    public float GetCurrentPickupCooldown() => currentPickupCooldown;

    private float remainingCooldown = 0f;

    public float GetRemainingCooldown() => remainingCooldown;

    public bool canUseCurrentPickup = false;

    public bool pickingUp = false;

    private float pickingUpDelay = 3f;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _playerInputActionsPlayer = GameSettings.Instance.playerInputs;

            _usePickupAction = _playerInputActionsPlayer.UsePickup;
            _usePickupAction.performed += OnUsePickup;
        }
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _usePickupAction.performed -= OnUsePickup;
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
                PlaySpeedBoostParticles();
                RequestUseSpeedBoostServerRpc();
                break;
            case PickupType.GravityBomb:
                PlayGravityBombParticles();
                RequestUseGravityBombServerRpc(gameObject.transform.position, _playerStateController.GetState() == PlayerState.Chaser); 
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
    private void RequestUseSpeedBoostServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        // Fire particles for everyone
        HandleSpeedBoostParticlesClientRpc();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        UseSpeedBoostClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void UseSpeedBoostClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        _playerMovement.StartCoroutine(_playerMovement.UseSpeedBoost(2, 2));
    }

    [ClientRpc]
    private void HandleSpeedBoostParticlesClientRpc()
    {
        if (!IsOwner) PlaySpeedBoostParticles();
    }

    private void PlaySpeedBoostParticles()
    {
        _speedBoostEffect.PlayParticles(2f);
    }

    [ServerRpc]
    private void RequestUseGravityBombServerRpc(Vector3 userPosition, bool chaser, ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        // Tell everyone to fire particles
        HandleGravityBombParticlesClientRpc();

        ulong bomberId = serverRpcParams.Receive.SenderClientId;

        // Iterate through all connected clients
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if(client.ClientId != bomberId)
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

        float pushForce = 100f;
        float effectiveRange = 8.0f;

        // Calculate distance from the current player to the bomb user
        float distance = Vector3.Distance(transform.position, bombUserPosition);

        if (distance <= effectiveRange)
        {
            if (chaser)
            {
                _playerMovement.UniversalKnockback(bombUserPosition, -pushForce, OwnerClientId);
            }
            else
            {
                _playerMovement.UniversalKnockback(bombUserPosition, pushForce, OwnerClientId);
            }
        }
        else
        {
            Debug.Log("Client " + OwnerClientId + " is too far away from bomber");
        }
    }

    [ClientRpc]
    private void HandleGravityBombParticlesClientRpc()
    {
        if (!IsOwner) PlayGravityBombParticles(); 
    }

    private void PlayGravityBombParticles()
    {
        _gravityBombEffect.PlayParticles();
    }

    private IEnumerator WaitPickupCooldown(float cooldown)
    {
        remainingCooldown = cooldown;

        while (remainingCooldown > 0)
        {
            yield return null; // Wait for the next frame
            remainingCooldown -= Time.deltaTime;
        }

        canUseCurrentPickup = true;
    }

    public enum PickupType
    {
        None = 0,
        SpeedBoost = 1,
        GravityBomb = 2
    }
}
