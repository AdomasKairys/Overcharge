using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MixerController : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;

	[SerializeField]
	private string masterVolumeParameter = "MasterVolume";

	[SerializeField]
	private string musicVolumeParameter = "MusicVolume";

	[SerializeField]
	private string effectsVolumeParameter = "EffectsVolume";

    private const string MusicVolumeKey = "MusicVolume";
    private const string EffectsVolumeKey = "EffectsVolume";

    private const float MinVolume = 0.0001f;

    public float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
	public float EffectsVolume => PlayerPrefs.GetFloat(EffectsVolumeKey, 1f);

	// Start is called before the first frame update
	private void Start()
    {
        SetMusicVolume(MusicVolume);
        SetEffectsVolume(EffectsVolume);
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume(masterVolumeParameter, volume);
    }

	public void SetMusicVolume(float volume)
	{
		SetVolume(musicVolumeParameter, volume);
        SaveMusicVolume(volume);
	}

	public void SetEffectsVolume(float volume)
	{
		SetVolume(effectsVolumeParameter, volume);
		SaveEffectsVolume(volume);
	}

	private void SetVolume(string parameter, float volume)
	{
		var mixerVolume = volume <= MinVolume ? -80 : Mathf.Log(volume) * 20;
	}

	private static void SaveMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

	private static void SaveEffectsVolume(float volume)
	{
		PlayerPrefs.SetFloat(EffectsVolumeKey, volume);
	}
}
