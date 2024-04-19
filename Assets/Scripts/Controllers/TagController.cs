using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TagController : NetworkBehaviour
{
    private float tagCooldown = 0.5f;

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
        if (other.CompareTag("TagTrigger") && IsOwner) // reduntant check for future proofing
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
                ChangeStateServerRPC(gameObject.GetComponentInParent<NetworkObject>(), otherStateController.gameObject.GetComponentInParent<NetworkObject>());

                thisMovementController.PushFrom(other.gameObject.GetComponentInParent<Transform>().position, 75f);
                Debug.Log(transform.parent.gameObject.name + " tagged " + other.gameObject.name + " State changed to " + thisStateController.GetState());
            }
        }
    }
    [ServerRpc]
    private void ChangeStateServerRPC(NetworkObjectReference thisObjectRef, NetworkObjectReference otherObjectRef)
    {
        if (!thisObjectRef.TryGet(out NetworkObject thisObject))
            return;
        if (!otherObjectRef.TryGet(out NetworkObject otherObject))
            return;

        var temp = thisObject.GetComponentInChildren<PlayerStateController>().currState.Value;
        thisObject.GetComponentInChildren<PlayerStateController>().SetState(otherObject.GetComponentInChildren<PlayerStateController>().currState.Value);
        otherObject.GetComponentInChildren<PlayerStateController>().SetState(temp);
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
