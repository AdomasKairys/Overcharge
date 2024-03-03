using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagController : MonoBehaviour
{
    private PlayerStateController thisStateController;

    // Start is called before the first frame update
    void Start()
    {
        thisStateController = GetComponentInParent<PlayerStateController>();
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
            // Tag control is performed only when this player is a chaser
            if(thisState == PlayerState.Chaser)
            {
                PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
                PlayerState otherState = otherStateController.GetState();
                // If the tagged player is a runner
                if (otherState == PlayerState.Runner)
                {
                    thisStateController.SetState(PlayerState.Runner);
                    otherStateController.SetState(PlayerState.Chaser);
                    Debug.Log(transform.parent.gameObject.name + " tagged " + other.gameObject.name + " State changed to " + thisStateController.GetState());
                }
            }
        }
    }
}
