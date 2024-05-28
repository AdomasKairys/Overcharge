using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSFXList", menuName = "Audio/SFXList")]
public class SFXList : ScriptableObject
{
	public List<SFX> sfxList;
}

