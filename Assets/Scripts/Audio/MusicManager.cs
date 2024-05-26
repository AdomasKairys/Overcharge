using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	private static MusicManager instance = null;
	private AudioSource audioSource;
	private bool wasMusicStopped = false;

	void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this.gameObject);

		audioSource = GetComponent<AudioSource>();
	}

	void Update()
	{
		ResumeMusic();
	}

	public void PlayMusic(AudioClip music)
	{
		if (audioSource.clip != music)
		{
			audioSource.clip = music;
			audioSource.Play();
		}
	}

	public void StopMusic()
	{
		audioSource.Stop();
		wasMusicStopped = true;
	}

	public void ResumeMusic()
	{
		if (!audioSource.isPlaying && wasMusicStopped)
		{
			audioSource.Play();
			wasMusicStopped = false;
		}
	}
}
