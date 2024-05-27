using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips for Effects")]
	[Header("Audio Clips for Rocket")]
	public AudioClip rocketShoot;
	public AudioClip rocketBoom;

	[Header("Audio Clips for Grappling Hook")]
	public AudioClip hookHit;
	public AudioClip hookLaunch;

	[Header("Audio Clip for CountDown")]
	public AudioClip countDown;

	[Header("Audio Clips for Pick Ups")]
	public AudioClip itemPickUp;
	public AudioClip itemSpeedBoost;
	public AudioClip itemGravityBomb;

	public AudioClip switchState;
	public AudioClip death;
	public AudioClip cooling;
	public AudioClip sprint;

	// Start is called before the first frame update
	private void Start()
    {
        
    }

    // Update is called once per frame
    public void PlaySFX(AudioClip clip)
    {
       SFXSource.PlayOneShot(clip); 
    }
}
