using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : NetworkBehaviour
{
    public static bool isPaused = false;
    [SerializeField] private GameObject pauseMenu;
    public GameObject[] NotPauseMenuUI;
    private void Start()
    {
        pauseMenu.SetActive(false);
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
        GameMultiplayer.Instance.Shutdown();
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }
    public void Quit()
	{
        Application.Quit();
    }
}
