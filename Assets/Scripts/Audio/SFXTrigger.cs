using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXTrigger : MonoBehaviour
{
	private string sfxName;	  // Name of the SFX to play

	void Start()
	{

	}

	public void PlaySFX(string sfxName)
	{
		if (SFXManager.Instance != null)
		{
			SFXManager.Instance.PlaySFX(sfxName);
		}
	}
}
