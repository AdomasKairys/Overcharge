using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject Keybinds;
    public void PlayGame()
	{
		SceneManager.LoadSceneAsync("LobbyScene");
	}
	public void QuitGame()
	{
		Application.Quit();
	}
	public void KeybindsOn()
	{
		Keybinds.SetActive(true);
	}
	public void KeybindsOff()
	{
		Keybinds.SetActive(false);
	}
}
