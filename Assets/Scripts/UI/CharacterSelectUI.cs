using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System;


public class CharacterSelect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _unreadyButton;

    [SerializeField] private Button _startButton;

    private bool _isHost;

    private void Start()
    {
        Lobby lobby = GameLobby.Instance.GetLobby();

        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        if (_startButton == null) return;

        _isHost = GameLobby.Instance.IsLobbyHost();
        if(_isHost)
        {
            _startButton.gameObject.SetActive(true);
            _startButton.enabled = false;
            _startButton.onClick.AddListener(StartGame);
            PlayerReady.Instance.OnAllPlayersReadyChanged += Instance_OnAllPlayersReadyChanged;
        }
        else
        {
            _startButton.gameObject.SetActive(false);
        }

        if (_readyButton == null || _unreadyButton == null) return;

        _readyButton.gameObject.SetActive(true);
        _unreadyButton.gameObject.SetActive(false);

        _readyButton.onClick.AddListener(Ready);
        _unreadyButton.onClick.AddListener(Unready);
    }

    private void Instance_OnAllPlayersReadyChanged(object sender, EventArgs e)
    {
        if (PlayerReady.Instance.AllPlayersReady)
        {
            _startButton.enabled = true;
        }
        else
        {
            _startButton.enabled = false;
        }
    }

    public void Ready()
    {
        PlayerReady.Instance.SetPlayerReadyState(true);
        _readyButton.gameObject.SetActive(false);
        _unreadyButton.gameObject.SetActive(true);
    }

    private void Unready()
    {
        PlayerReady.Instance.SetPlayerReadyState(false);
        _readyButton.gameObject.SetActive(true);
        _unreadyButton.gameObject.SetActive(false);
    }

    private void StartGame()
    {
        if (_isHost) PlayerReady.Instance.StartGame();
    }

    public void MainMenu()
    {
        GameLobby.Instance.LeaveLobby();
        GameMultiplayer.Instance.Shutdown();
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }

    private void OnDestroy()
    {
        if (_isHost && _startButton != null)
        {
            _startButton.onClick.RemoveListener(StartGame);
            PlayerReady.Instance.OnAllPlayersReadyChanged -= Instance_OnAllPlayersReadyChanged;
        }

        if (_readyButton == null || _unreadyButton == null) return;

        _readyButton.onClick.RemoveListener(Ready);
        _unreadyButton.onClick.RemoveListener(Unready);
    }
}
