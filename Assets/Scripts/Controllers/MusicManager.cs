using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
	private static MusicManager instance = null;
	private AudioSource audioSource;
	private AudioClip currentMusic;
	private bool wasMusicStopped = false;
	private float loopStartTime = 0f;
	private float loopEndTime = 0f;
	private bool loopEnabled = false;

	[System.Serializable]
	public struct SceneMusic
	{
		public string sceneName;
		public AudioClip musicClip;
		public float loopStartTime;
		public float loopEndTime;
		public bool loopEnabled;
	}

	public SceneMusic[] sceneMusicSettings;

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
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void Update()
	{
		if (loopEnabled && audioSource.isPlaying && audioSource.time >= loopEndTime)
		{
			audioSource.time = loopStartTime;
		}
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		foreach (var sceneMusic in sceneMusicSettings)
		{
			if (scene.name == sceneMusic.sceneName)
			{
				PlayMusic(sceneMusic.musicClip);
				SetLoopPoints(sceneMusic.loopStartTime, sceneMusic.loopEndTime);
				SetLoopEnabled(sceneMusic.loopEnabled);
				return;
			}
		}
	}

	public void PlayMusic(AudioClip music)
	{
		if (audioSource.clip != music)
		{
			audioSource.clip = music;
			audioSource.Play();
			currentMusic = music;
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

	public void SetLoopPoints(float startTime, float endTime)
	{
		loopStartTime = startTime;
		loopEndTime = endTime;
	}

	public void SetLoopEnabled(bool enabled)
	{
		loopEnabled = enabled;
	}
}
