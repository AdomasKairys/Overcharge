using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject PauseMenuUI;
    public GameObject[] NotPauseMenuUI;
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
        PauseMenuUI.SetActive(false);
        isPaused = false;
        setTo(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Pause()
	{
        PauseMenuUI.SetActive(true);
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
        SceneManager.LoadSceneAsync(0);
    }
    public void Quit()
	{
        Application.Quit();
    }
    public void Restart()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
