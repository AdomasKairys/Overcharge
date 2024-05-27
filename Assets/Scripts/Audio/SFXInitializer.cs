using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXInitializer : MonoBehaviour
{
	public SFXList sfxList;

	void Start()
	{
		foreach (SFX sfx in sfxList.sfxList)
		{
			SFXManager.Instance.RegisterSFX(sfx.sfxName, sfx.audioClip);
			Debug.LogWarning($"I'm working on '{sfx.sfxName}' effect");
		}
	}
}

