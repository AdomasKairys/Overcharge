using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;


public class CharacterSelect : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    public void Ready()
    {
        PlayerReady.Instance.SetPlayerReady();
    }
    public void MainMenu()
    {
        GameLobby.Instance.LeaveLobby();
        GameMultiplayer.Instance.Shutdown(NetworkManager.Singleton.LocalClientId);
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }

    private void Start()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();

        lobbyNameText.text = "Loby Name: " + lobby.Name;
        lobbyCodeText.text = "Loby Code: " + lobby.LobbyCode;

    }
}
