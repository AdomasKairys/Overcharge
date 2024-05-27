using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXTrigger : MonoBehaviour
{
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

	public void PlaySFX_CanStop(string sfxName, bool loop)
	{
		if (SFXManager.Instance != null)
		{
			SFXManager.Instance.PlaySFX_CanStop(sfxName, loop);
		}
	}

	public void StopSFX(string sfxName)
	{
		if (SFXManager.Instance != null)
		{
			SFXManager.Instance.StopSFX(sfxName);
		}
	}
}
