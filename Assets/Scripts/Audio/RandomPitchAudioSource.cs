using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Audio;

[RequireComponent(typeof(AudioSource))]
public class RandomPitchAudioSource : MonoBehaviour
{
    [Range(0f, 5f)]
    [SerializeField]
    private float minPitch = 0.5f;

	[Range(0f, 5f)]
	[SerializeField]
	private float maxPitch = 1.1f;

	[SerializeField]
	private bool playOnStart;

    private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}

	private void Start()
    {
        if(playOnStart)
		{
			Play();
		}
    }

	public void Play()
	{
		audioSource.pitch = Random.Range(minPitch, maxPitch);
		audioSource.Play();
	}
}
