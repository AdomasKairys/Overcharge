using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VelocityAudioSource : MonoBehaviour
{
	[Range(0f, 5f)]
	[SerializeField]
	private float minPitch = 0.9f;

	[Range(0f, 5f)]
	[SerializeField]
	private float maxPitch = 1.5f;

	[Range(0f, 10f)]
	[SerializeField]
	private float maxSpeed = 3f;

	[SerializeField]
	private Rigidbody target;

	private Vector3 previousTargetPosition;
	private Vector3 targetVelocity;

	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
	}


	private void FixedUpdate()
	{
		if (target == null)
		{
			return;
		}
		UpdateVelocity();
		UpdatePreviousTargetPosition();
	}

	private void Update()
	{
		if (target == null)
		{
			return;
		}
		UpdateAudioSourcePitch();
	}

	private void UpdateAudioSourcePitch()
	{
		var clampedSpeed = Mathf.Clamp(targetVelocity.magnitude, 0, maxSpeed);
		var pitchPercentage = clampedSpeed / maxSpeed;
		var pitch = Mathf.Lerp(minPitch, maxPitch, pitchPercentage);

		audioSource.pitch = pitch;		
	}

	private void UpdateVelocity()
	{
		var targetPosition = target.position;
		targetVelocity = (targetPosition - previousTargetPosition) / Time.deltaTime;

	}

	private void UpdatePreviousTargetPosition()
	{
		previousTargetPosition = target.position;
	}


}
