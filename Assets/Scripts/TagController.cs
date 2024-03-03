using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagController : MonoBehaviour
{
    [Header("Tag cooldown in seconds")]
    public float tagCooldown = 2.0f;
    
    private PlayerStateController thisStateController;
    private bool blocked; // whether the tagging functionality for this player is blocked

    // Start is called before the first frame update
    void Start()
    {
        thisStateController = GetComponentInParent<PlayerStateController>();
        blocked = false;
        Debug.Log(transform.parent.gameObject.name + ": " + thisStateController);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Trigger objects on the player should have the tagTrigger layer so only collisions inbetween tag triggers are registered here
        if (other.CompareTag("TagTrigger")) // reduntant check for future proofing
        {
            PlayerState thisState = thisStateController.GetState();
            // Tag control is performed only when this player is an unblocked chaser
            if(!blocked && thisState == PlayerState.Chaser)
            {
                PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
                TagController otherTagController = other.GetComponentInParent<TagController>();
                PlayerState otherState = otherStateController.GetState();
                bool otherBlocked = otherTagController.IsBlocked();
                // If the tagged player is an unblocked runner
                if (!otherBlocked && otherState == PlayerState.Runner)
                {
                    Block();
                    otherTagController.Block();
                    thisStateController.SetState(PlayerState.Runner);
                    otherStateController.SetState(PlayerState.Chaser);
                    Debug.Log(transform.parent.gameObject.name + " tagged " + other.gameObject.name + " State changed to " + thisStateController.GetState());
                }
            }
        }
    }

    public bool IsBlocked()
    {
        return blocked;
    }

    public void Block()
    {
        blocked = true;
        StartCoroutine(UnblockAfterCooldown());
    }

    IEnumerator UnblockAfterCooldown()
    {
        yield return new WaitForSeconds(tagCooldown); // Wait for the cooldown time.
        blocked = false; // Unblock the player.
    }
}
