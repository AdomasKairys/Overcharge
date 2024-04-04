using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Camera cam;


    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        playAgainButton.onClick.AddListener(() => { SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString()); });
        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientIds)
    {
        if(clientIds == NetworkManager.ServerClientId)
        {
            Show();
        }
    }

    private void Show()
    {
        cam.enabled = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        cam.enabled = false;
        gameObject.SetActive(false);
    }
}
