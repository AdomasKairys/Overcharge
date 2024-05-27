using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class CoolingSystem : NetworkBehaviour
{
    public float coolRate = 0.5f;

	SFXTrigger sfxTrigger;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}

	private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
        {
            PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
            PlayerState otherState = otherStateController.GetState();
            if (IsServer && otherState == PlayerState.Runner)
            {
                if(otherStateController.currCharge.Value > 0)
                {
					otherStateController.currCharge.Value -= coolRate * Time.deltaTime;
					sfxTrigger.PlaySFX_CanStop("coolingStation", 0f, 0f, true);
				}
				sfxTrigger.StopSFX("coolingStation");
			}		
		}
    }

}
