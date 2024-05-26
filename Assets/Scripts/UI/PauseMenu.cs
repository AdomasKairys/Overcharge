using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : NetworkBehaviour
{
	[SerializeField]
	private GameObject menu;

	[SerializeField]
	private GameObject soundoptions;

	[SerializeField]
	private Slider musicVolumeSlider;

	[SerializeField]
	private Slider effectsVolumeSlider;

	[SerializeField]
	private MixerController mixerController;

	public static bool isPaused = false;
    [SerializeField] private GameObject pauseMenu;
    public GameObject[] NotPauseMenuUI;
    private void Start()
    {
        pauseMenu.SetActive(false);
		UpdateSliders();
		soundoptions.SetActive(false);
	}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
			if (isPaused)
			{
                Resume();
			}
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
	{
        pauseMenu.SetActive(false);
        isPaused = false;
        setTo(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Pause()
	{
        pauseMenu.SetActive(true);
        isPaused = true;
        setTo(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    void setTo(bool to)//add all ui to hide when paused
	{
        foreach(GameObject o in NotPauseMenuUI)
		{
            o.SetActive(to);
        }
        //NotPauseMenuUI.SetActive(to);
    }

    public void LoadMenu()
    {
        GameMultiplayer.Instance.Shutdown(NetworkManager.Singleton.LocalClientId);
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }
    public void Quit()
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
		SetAllInactiveExceptOne(menu, soundoptions);
	}

	public void Done()
	{
		soundoptions.SetActive(false);
		SetAllActiveExceptOne(menu, soundoptions);
	}

	void SetAllInactiveExceptOne(GameObject canvas, GameObject objectToKeepActive)
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

	void SetAllActiveExceptOne(GameObject canvas, GameObject objectToKeepInactive)
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
