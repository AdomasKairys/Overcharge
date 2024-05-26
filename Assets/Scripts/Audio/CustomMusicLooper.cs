using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMusicLooper : MonoBehaviour
{
	public float loopStartTime = 20f; // Default start time for the loop
	public float loopEndTime = 40f; // Default end time for the loop
	public bool loopEnabled = true; // Flag to indicate if looping is enabled

	public AudioSource audioSource;

	public void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
	}

	public void Start()
	{
		// Optional: Start playing the audio immediately upon scene start
		PlayMusic();
	}

	public void Update()
	{
		// Check if looping is enabled and the playback time is beyond the loop end time
		if (audioSource != null && loopEnabled && audioSource.isPlaying && audioSource.time >= loopEndTime)
		{
			// Reset the playback time to the loop start time
			audioSource.time = loopStartTime;
		}
	}

	public void PlayMusic()
	{
		// Play the audio clip from the beginning
		audioSource.Play();
	}

	public void StopMusic()
	{
		// Stop the audio playback
		audioSource.Stop();
	}

	public void SetLoopPoints(float startTime, float endTime)
	{
		// Set the loop start and end times
		loopStartTime = startTime;
		loopEndTime = endTime;
	}

	public void SetLoopEnabled(bool enabled)
	{
		// Set whether looping is enabled or not
		loopEnabled = enabled;
	}

	public void SetAudioClip(AudioClip music)
	{
		// Set the audio clip to be played
		audioSource.clip = music;
	}
}
