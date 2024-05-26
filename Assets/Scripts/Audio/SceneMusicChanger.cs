using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicChanger : MonoBehaviour
{

	public AudioClip newMusic; // Reference to the new AudioClip
	public bool CMLEnabled = true;

	void Start()
	{
		MusicManager musicManager = FindObjectOfType<MusicManager>();
		if (musicManager != null && CMLEnabled == false)
		{
			musicManager.PlayMusic(newMusic);
		}
		else if (musicManager != null && CMLEnabled == true)
		{
			musicManager.StopMusic();
		}
		else
		{
			Debug.LogError("No MusicManager found in the scene.");
		}
	}
}
