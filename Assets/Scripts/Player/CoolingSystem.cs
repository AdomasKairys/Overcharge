using System.Collections;
using Unity.Netcode;
using UnityEngine;

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
    private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
		{
			PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
			PlayerState otherState = otherStateController.GetState();
			if (otherState == PlayerState.Runner && otherStateController.currCharge.Value > 0 && !isSoundPlaying)
			{
				sfxTrigger.PlaySFX_CanStop("coolingStation1", false);
				StartCoroutine(PlayEffectLoop());
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
            if (otherState == PlayerState.Runner)
            {
				if (otherStateController.currCharge.Value > 0.1f)
				{
					if (!isSoundPlaying) //play the audio when the player is on the stattion when he switcher states
					{
						sfxTrigger.PlaySFX_CanStop("coolingStation1", false);
						StartCoroutine(PlayEffectLoop());
						isSoundPlaying = true;
					}
					if (IsServer)
					{
						Debug.Log("reduce");
						otherStateController.currCharge.Value -= coolRate * Time.deltaTime;
					}
				}
				else //stopping sound when charge is less than 0.1f, there will be audio cut off when one player finishes cooling down
				{
					StopCoolingSound();
                }
			}
			else if (otherState == PlayerState.Chaser && isSoundPlaying && playerOnStationAmm == 1) //if the only player statnding on the platform is chaser stop the audiot if its player (can still be playing if player swithc states while on the platform
			{
				StopCoolingSound();
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
				StopCoolingSound();
            }
				
        }	
	}

	private IEnumerator PlayEffectLoop()
	{
		yield return new WaitForSeconds(1.2f);
		sfxTrigger.PlaySFX_CanStop("coolingStation2", true);
	}

	private void StopCoolingSound()
	{
        StopAllCoroutines();
        sfxTrigger.StopSFX("coolingStation1");
        sfxTrigger.StopSFX("coolingStation2");
        isSoundPlaying = false;
    }
}
