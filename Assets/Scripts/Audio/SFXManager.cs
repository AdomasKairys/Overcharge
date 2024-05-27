using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SFXManager : MonoBehaviour
{
	public static SFXManager Instance { get; private set; }

	private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
	private Dictionary<string, AudioSource> audioSourceMap = new Dictionary<string, AudioSource>();
	public AudioMixerGroup effects;
	private AudioSource effectSource;
	private AudioSource canStop;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			effectSource = GetComponent<AudioSource>();
			if (effectSource == null)
			{
				effectSource = gameObject.AddComponent<AudioSource>();
			}
		}
		else
		{
			Destroy(gameObject);
		}
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		var buttons = FindObjectsOfType<Button>(true);
		foreach (var button in buttons)
		{
			if (button.gameObject.tag == "Color")
			{
				button.onClick.AddListener(() =>
				{
					PlaySFX("colorChange");
				});
			}
			else 
			{
				button.onClick.AddListener(() =>
				{
					PlaySFX("button2");
				});		
			}
		}
	}

	System.Text.StringBuilder sb = new System.Text.StringBuilder();
	public void RegisterSFX(string sfxName, AudioClip sfxClip)
	{
		if (!sfxDictionary.ContainsKey(sfxName))
		{
			sfxDictionary.Add(sfxName, sfxClip);
			Debug.LogWarning($"I got '{sfxName}' effect");
		}
		// Iterate through the dictionary and append each key-value pair to the string builder
		foreach (KeyValuePair<string, AudioClip> kvp in sfxDictionary)
		{
			sb.AppendLine($"Key: {kvp.Key}, Value: {kvp.Value.name}");
		}

		// Log the concatenated string
		Debug.LogWarning(sb.ToString());
	}

	public void PlaySFX(string sfxName)
	{
		if (sfxDictionary.TryGetValue(sfxName, out AudioClip sfxClip))
		{
			effectSource.PlayOneShot(sfxClip);
			Debug.LogWarning($"I'm playing the '{sfxName}' music clip");
		}
		else
		{
			Debug.LogWarning($"SFX '{sfxName}' not found!");
		}
	}

	public void PlaySFX_CanStop(string sfxName, float min, float max, bool loop)
	{
		if (sfxDictionary.TryGetValue(sfxName, out AudioClip sfxClip))
		{
			if (!audioSourceMap.ContainsKey(sfxName))
			{
				AudioSource newSource = gameObject.AddComponent<AudioSource>();
				newSource.outputAudioMixerGroup = effects;
				audioSourceMap.Add(sfxName, newSource);
			}

			AudioSource audioSource = audioSourceMap[sfxName];
			
			if (loop)										//Does the effect need to loop?
			{
				if ((min != 0f && max != 0f) || max != 0f)  //Does the effect need to be looped in a certain time?
				{ 
					if (audioSource.time >= max)            //Has the effect passed the end time to start looping?
					{
						audioSource.clip = sfxClip;
						audioSource.Play();
						audioSource.time = min;
						audioSource.loop = true;
					}
				}
				else                                        //So that the effect can loop
				{
					audioSource.clip = sfxClip;
					audioSource.Play();
					audioSource.loop = true;
				}
			}
			if (min == 0f && max != 0f)                     //Does the effect need to continue to a certain time without looping?
			{
				if (audioSource.time <= max)                //Has the effect reached the end time before stopping?
				{
					audioSource.clip = sfxClip;
					audioSource.Play();
				}
				audioSource.Stop();
			}
			else                                            //Just play the effect
			{
				audioSource.clip = sfxClip;
				audioSource.Play();
			}
			Debug.LogWarning($"I'm playing the '{sfxName}' music clip");
		}
		else
		{
			Debug.LogWarning($"SFX '{sfxName}' not found!");
		}
	}

	public void StopSFX(string sfxName)
	{
		if (audioSourceMap.ContainsKey(sfxName))
		{
			audioSourceMap[sfxName].Stop();
			audioSourceMap[sfxName].loop = false;
		}
	}
}

