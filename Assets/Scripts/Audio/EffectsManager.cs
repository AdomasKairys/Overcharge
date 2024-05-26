using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips for Effects")]
	public AudioClip rocketShoot;
	public AudioClip rocketBoom;

	public AudioClip hookHit;
	public AudioClip hookLaunch;

	public AudioClip countDown;

	public AudioClip itemPickUp;
	public AudioClip itemSpeedBoost;
	public AudioClip itemGravityBomb;

	public AudioClip colorChange;
	public AudioClip button1;
	public AudioClip button2;

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
