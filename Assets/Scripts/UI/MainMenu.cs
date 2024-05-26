using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	private Canvas menu;

	[SerializeField] 
	private GameObject soundoptions;

	[SerializeField] 
	private Slider musicVolumeSlider;

	[SerializeField]
	private Slider effectsVolumeSlider;

	[SerializeField]
	private MixerController mixerController;

	private void Start()
	{
		UpdateSliders();
		soundoptions.SetActive(false);
	}

	public void PlayGame()
	{
		SceneManager.LoadSceneAsync("LobbyScene");
	}
	public void QuitGame()
	{
		Application.Quit();
	}

	private void UpdateSliders() 
	{
		musicVolumeSlider.SetValueWithoutNotify(mixerController.MusicVolume);
		effectsVolumeSlider.SetValueWithoutNotify(mixerController.EffectsVolume);
	}

	public void SoundOptions()
	{
		soundoptions.SetActive(true);
		SetAllInactiveExceptOne(menu,soundoptions);
	}

	public void Done()
	{
		soundoptions.SetActive(false);
		SetAllActiveExceptOne(menu,soundoptions);
	}

	void SetAllInactiveExceptOne(Canvas canvas, GameObject objectToKeepActive)
	{
		// Iterate through all child objects of the canvas
		foreach (Transform child in canvas.transform)
		{
			// Check if the current child object is not the one to keep active
			if (child.gameObject != objectToKeepActive)
			{
				// Set the current child object inactive
				child.gameObject.SetActive(false);
			}
		}

		// Set the object to keep active, active
		if (objectToKeepActive != null)
		{
			objectToKeepActive.SetActive(true);
		}
	}

	void SetAllActiveExceptOne(Canvas canvas, GameObject objectToKeepInactive)
	{
		// Iterate through all child objects of the canvas
		foreach (Transform child in canvas.transform)
		{
			// Set all objects active
			child.gameObject.SetActive(true);
		}

		// Deactivate the specified object
		if (objectToKeepInactive != null)
		{
			objectToKeepInactive.SetActive(false);
		}
	}
}
