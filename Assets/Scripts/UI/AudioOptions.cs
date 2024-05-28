using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioOptions : MonoBehaviour
{
    [SerializeField]
    private Slider musicVolumeSlider;

	[SerializeField]
	private Slider effectsVolumeSlider;

    [SerializeField]
    private MixerController mixerController;

	private void Start()
    {
        UpdateSliders();
	}

	private void UpdateSliders()
    {
        musicVolumeSlider.SetValueWithoutNotify(mixerController.MusicVolume);
		effectsVolumeSlider.SetValueWithoutNotify(mixerController.EffectsVolume);

        Debug.Log("Idiot");
	}
}
