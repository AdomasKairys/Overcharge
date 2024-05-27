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

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
		{
			PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
			PlayerState otherState = otherStateController.GetState();
			if (otherState == PlayerState.Runner)
			{
				//sfxTrigger.PlaySFX("coolingStation1");
				sfxTrigger.PlaySFX_CanStop("coolingStation1", false);
				StartCoroutine(PlayEffectLoop());
				//sfxTrigger.PlaySFX_CanStop("coolingStation2", true);
			}
		}
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
				}			
			}		
		}
    }

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
		{
			StopAllCoroutines();
			sfxTrigger.StopSFX("coolingStation1");
			sfxTrigger.StopSFX("coolingStation2");
		}	
	}

	private IEnumerator PlayEffectLoop()
	{
		yield return new WaitForSeconds(1.6f);
		sfxTrigger.PlaySFX_CanStop("coolingStation2", true);
	}
}
