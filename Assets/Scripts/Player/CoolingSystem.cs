using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class CoolingSystem : NetworkBehaviour
{
    public float coolRate = 0.5f;

	private SFXTrigger sfxTrigger;

	private bool isSoundPlaying = false;
	private int playerOnStationAmm = 0;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}
    private void Update()
    {
		Debug.Log(playerOnStationAmm);
    }
    private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
		{
			PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
			PlayerState otherState = otherStateController.GetState();
			if (otherState == PlayerState.Runner && otherStateController.currCharge.Value > 0 && !isSoundPlaying)
			{
				//sfxTrigger.PlaySFX("coolingStation1");
				sfxTrigger.PlaySFX_CanStop("coolingStation1", false);
				StartCoroutine(PlayEffectLoop());
				//sfxTrigger.PlaySFX_CanStop("coolingStation2", true);
				isSoundPlaying = true;
            }
            playerOnStationAmm++;
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
				if (otherStateController.currCharge.Value > 0.1f)
				{
					if (!isSoundPlaying) //play the audio when the player is on the stattion when he switcher states
					{
                        sfxTrigger.PlaySFX_CanStop("coolingStation1", false);
                        StartCoroutine(PlayEffectLoop());
                        isSoundPlaying = true;
                    }
					otherStateController.currCharge.Value -= coolRate * Time.deltaTime;
				}
				else //stopping sound when charge is less than 0.1f, there will be audio cut off when one player finishes cooling down
				{
					StopAllCoroutines();
					sfxTrigger.StopSFX("coolingStation1");
					sfxTrigger.StopSFX("coolingStation2");
					isSoundPlaying = false;
				}
			}		
		}
    }

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
		{
            playerOnStationAmm--;
            if (isSoundPlaying && playerOnStationAmm == 0) //Stop the audio when every player is off the platform and the audio is still playing
			{
				StopAllCoroutines();
				sfxTrigger.StopSFX("coolingStation1");
				sfxTrigger.StopSFX("coolingStation2");
				isSoundPlaying = false;

            }
				
        }	
	}

	private IEnumerator PlayEffectLoop()
	{
		yield return new WaitForSeconds(1.6f);
		sfxTrigger.PlaySFX_CanStop("coolingStation2", true);
	}
}
