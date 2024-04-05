using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TagController : MonoBehaviour
{
    [Header("Tag cooldown in seconds")]
    public float tagCooldown = 2.0f;

    [SerializeField] private PlayerStateController thisStateController;
    [SerializeField] private PlayerMovement thisMovementController;
    private bool blocked; // whether the tagging functionality for this player is blocked

    // Start is called before the first frame update
    void Start()
    {
        blocked = false;
        Debug.Log(transform.parent.gameObject.name + ": " + thisStateController);
    }
  

    private void OnTriggerEnter(Collider other)
    {
        // Trigger objects on the player should have the tagTrigger layer so only collisions inbetween tag triggers are registered here
        if (other.CompareTag("TagTrigger")) // reduntant check for future proofing
        {
            PlayerState thisState = thisStateController.GetState();
            PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
            //TagController otherTagController = other.GetComponentInParent<TagController>();
            PlayerState otherState = otherStateController.GetState();
            //bool otherBlocked = otherTagController.IsBlocked();
            // Tag control is performed only when this player is an unblocked chaser
            if(!blocked && otherState != thisState)
            {
                // If the tagged player is an unblocked runner
                Block();
                //otherTagController.Block();

                if (thisState == PlayerState.Runner)
                    ChangeStateServerRCP(other.gameObject.GetComponentInParent<NetworkObject>(), gameObject.GetComponentInParent<NetworkObject>());
                else
                    ChangeStateServerRCP(gameObject.GetComponentInParent<NetworkObject>(), other.gameObject.GetComponentInParent<NetworkObject>());

                thisMovementController.PushAwayFromTagged(other.gameObject.GetComponentInParent<Rigidbody>().transform.position);
                Debug.Log(transform.parent.gameObject.name + " tagged " + other.gameObject.name + " State changed to " + thisStateController.GetState());
            }
        }
    }
    [ServerRpc]
    private void ChangeStateServerRCP(NetworkObjectReference chaser, NetworkObjectReference runner)
    {
        ChangeStateClientRCP(chaser, runner);
    }
    [ClientRpc]
    private void ChangeStateClientRCP(NetworkObjectReference chaser, NetworkObjectReference runner) 
    {
        if (!chaser.TryGet(out NetworkObject chaserObject))
            return;
        if (!runner.TryGet(out NetworkObject runnerObject))
            return;

        if (gameObject.GetComponentInParent<NetworkObject>() == chaserObject)
        {
            Debug.Log("OK");
            chaserObject.GetComponentInChildren<PlayerStateController>().SetState(PlayerState.Runner);
        }
        else
            runnerObject.GetComponentInChildren<PlayerStateController>().SetState(PlayerState.Chaser);

    }
    public bool IsBlocked()
    {
        return blocked;
    }

    public void Block()
    {
        blocked = true;
        thisMovementController.isDashing = true;
        StartCoroutine(UnblockAfterCooldown());
    }

    IEnumerator UnblockAfterCooldown()
    {
        yield return new WaitForSeconds(tagCooldown); // Wait for the cooldown time.
        blocked = false; // Unblock the player.
        thisMovementController.isDashing = false;
    }
}
