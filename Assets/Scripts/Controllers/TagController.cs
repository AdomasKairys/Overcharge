using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TagController : NetworkBehaviour
{
    private float tagCooldown = 0.5f;

    [SerializeField] private PlayerStateController thisStateController;
    [SerializeField] private PlayerMovement thisMovementController;
    private NetworkVariable<bool> blocked = new NetworkVariable<bool>(false); // whether the tagging functionality for this player is blocked

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(transform.parent.gameObject.name + ": " + thisStateController);
    }
  

    private void OnTriggerEnter(Collider other)
    {
        // Trigger objects on the player should have the tagTrigger layer so only collisions inbetween tag triggers are registered here
        if (other.CompareTag("TagTrigger") && IsServer) // reduntant check for future proofing
        {
            PlayerState thisState = thisStateController.GetState();
            PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
            PlayerMovement otherMovementController = other.gameObject.GetComponentInParent<PlayerMovement>();
            TagController otherTagController = other.GetComponentInParent<TagController>();
            PlayerState otherState = otherStateController.GetState();
            //bool otherBlocked = otherTagController.IsBlocked();
            // Tag control is performed only when this player is an unblocked chaser
            if(!blocked.Value && otherState != thisState)
            {
                // If the tagged player is an unblocked runner
                Block();
                otherTagController.Block();
                ChangeStateServerRPC(gameObject.GetComponentInParent<NetworkObject>(), otherStateController.gameObject.GetComponentInParent<NetworkObject>());

                //thisMovementController.PushFrom(other.gameObject.GetComponentInParent<Transform>().position, 75f);

                thisMovementController.UniversalKnockback(other.transform.position, 75f, thisStateController.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId, true);
                otherMovementController.UniversalKnockback(transform.position, 75f, otherStateController.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId, true);



                Debug.Log(transform.parent.gameObject.name + " tagged " + other.gameObject.name + " State changed to " + thisStateController.GetState());
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangeStateServerRPC(NetworkObjectReference thisObjectRef, NetworkObjectReference otherObjectRef)
    {
        if (!thisObjectRef.TryGet(out NetworkObject thisObject))
            return;
        if (!otherObjectRef.TryGet(out NetworkObject otherObject))
            return;

        var temp = thisObject.GetComponentInChildren<PlayerStateController>().GetState();
        thisObject.GetComponentInChildren<PlayerStateController>().SetState(otherObject.GetComponentInChildren<PlayerStateController>().GetState());
        otherObject.GetComponentInChildren<PlayerStateController>().SetState(temp);
    }
    public bool IsBlocked()
    {
        return blocked.Value;
    }

    public void Block()
    {
        blocked.Value = true;
        StartCoroutine(UnblockAfterCooldown());
    }

    IEnumerator UnblockAfterCooldown()
    {
        yield return new WaitForSeconds(tagCooldown); // Wait for the cooldown time.
        blocked.Value = false; // Unblock the player.
    }
}
