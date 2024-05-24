using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
	{
		SceneManager.LoadSceneAsync("LobbyScene");
	}
	public void QuitGame()
	{
		Application.Quit();
	}
	public void ShowSettings()
	{
		Application.Quit();
	}
}
