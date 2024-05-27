using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSFX", menuName = "Audio/SFX")]
public class SFX : ScriptableObject
{
	public string sfxName;
	public AudioClip audioClip;
	//public float loopStartTime = 0f;
	//public float loopEndTime = 0f;
	//public bool loopEnabled = false;

}

