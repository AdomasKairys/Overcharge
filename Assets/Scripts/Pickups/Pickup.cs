using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Component on the pickup prefab that handles the detection of collisions with player and disabling
/// </summary>
public class Pickup : NetworkBehaviour
{
    [SerializeField]
    private GameObject _pickupBlock;

    [SerializeField]
    private Collider _trigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("A player has collided wit da pickup");
            RequestDisableServerRpc();
            var playerInventoryController = other.gameObject.GetComponent<InventoryController>();
            if (playerInventoryController != null)
            {
                playerInventoryController.HandlePickup();
            }
            else
            {
                Debug.Log("Inventory controller was null");
            }
        }
    }

    private void Disable()
    {
        _pickupBlock.SetActive(false);
        _trigger.enabled = false;
        StartCoroutine(Reenable());
    }

    [ClientRpc]
    private void DisableClientRpc()
    {
        if (IsOwner) return; // Prevent the owner from running this again if they're also the server.

        Disable();
    }

    [ServerRpc]
    public void RequestDisableServerRpc()
    {
        if (!IsServer) return;

        Disable();
        DisableClientRpc();
    }

    private IEnumerator Reenable()
    {
        yield return new WaitForSeconds(7.0f);
        _pickupBlock.SetActive(true);
        _trigger.enabled = true;
    }
}
