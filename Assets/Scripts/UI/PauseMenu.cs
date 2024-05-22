using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : NetworkBehaviour
{
    public static bool isPaused = false;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private MonoBehaviour[] player;
    public GameObject[] NotPauseMenuUI;
    public ThirdPersonCam thirdPersonCam;

    private void Start()
    {
        player = player.Where(x => x.enabled).ToArray();
        NotPauseMenuUI = NotPauseMenuUI.Where(x => x.activeSelf).ToArray();
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
        thirdPersonCam.UnfreezeCamera();
        pauseMenu.SetActive(false);
        isPaused = false;
        setTo(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Pause()
	{
        thirdPersonCam.FreezeCamera();
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
        foreach (MonoBehaviour mb in player)
        {
            mb.enabled = to;
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
