using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
    [SerializeField] private bool isCameraSeparate = true;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Camera cam;


    private void Start()
    {
        Debug.Log(NetworkManager.ServerClientId + " " + NetworkManager.Singleton.LocalClientId);

        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent; 
        playAgainButton.onClick.AddListener(() => { SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString()); });
        Hide();
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    {
        Debug.Log(NetworkManager.ServerClientId + " " + arg2.ClientId);

        if (arg2.EventType == ConnectionEvent.ClientDisconnected && arg2.ClientId == NetworkManager.ServerClientId)
        {
            Show();
        }
    }

    private void NetworkManager_OnServerStopped(bool obj)
    {
        Show();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log(NetworkManager.ServerClientId + " " + clientId);
        if (clientId == NetworkManager.ServerClientId)
        {
            Show();
        }
    }

    private void Show()
    {
        if(isCameraSeparate)
            cam.enabled = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        if (isCameraSeparate)
            cam.enabled = false;
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            // NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
            // NetworkManager.Singleton.OnServerStopped -= NetworkManager_OnServerStopped;
            NetworkManager.Singleton.OnConnectionEvent -= NetworkManager_OnConnectionEvent;
        }
    }
}
