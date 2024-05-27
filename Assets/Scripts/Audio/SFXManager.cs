using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
	public static SFXManager Instance { get; private set; }

	private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
	private AudioSource audioSource;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void RegisterSFX(string sfxName, AudioClip sfxClip)
	{
		if (!sfxDictionary.ContainsKey(sfxName))
		{
			sfxDictionary.Add(sfxName, sfxClip);
			Debug.LogWarning($"I got '{sfxName}' effect");
		}
	}

	public void PlaySFX(string sfxName)
	{
		if (sfxDictionary.TryGetValue(sfxName, out AudioClip sfxClip))
		{
			audioSource.PlayOneShot(sfxClip);
			Debug.LogWarning($"I'm playing the '{sfxName}' music clip");
		}
		else
		{
			Debug.LogWarning($"SFX '{sfxName}' not found!");
		}
	}
}

