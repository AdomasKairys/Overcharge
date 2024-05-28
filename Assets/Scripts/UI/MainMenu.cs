using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject Options;
	public GameObject Keybinds;
	public GameObject Audio;

	public void PlayGame()
	{
		SceneManager.LoadSceneAsync("LobbyScene");
	}
	public void QuitGame()
	{
		Application.Quit();
	}

	public void OptionsOn()
	{
		Options.SetActive(true);
	}
	public void OptionsOff()
	{
		Options.SetActive(false);
	}

	public void KeybindsOn()
	{
		Keybinds.SetActive(true);
	}
	public void KeybindsOff()
	{
		Keybinds.SetActive(false);
	}

	public void AudioOn()
	{
		Audio.SetActive(true);
	}
	public void AudioOff()
	{
		Audio.SetActive(false);
	}
}
