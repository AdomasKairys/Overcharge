using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayer : NetworkBehaviour
{

    public const int MAX_PLAYER_AMOUNT = 4;
    public const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static GameMultiplayer Instance { get; private set; }
    public int NetworkManager_Server_OnClientConnectCallback { get; private set; }

    public event EventHandler OnTryinToJoinGame;
    public event EventHandler OnFailToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private List<Color> playerColors;

    private NetworkList<PlayerData> playerDataNetworkList = null;
    private string playerName;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName"+UnityEngine.Random.Range(10,1000));

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }


    public string GetPlayerName() => playerName;
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartClient()
    {
        OnTryinToJoinGame?.Invoke(this, EventArgs.Empty);
        RemoveNetworkManagerCallbacks();
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRPC(GetPlayerName());
        SetPlayerIdServerRPC(AuthenticationService.Instance.PlayerId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRPC(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRPC(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }
    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        RemoveNetworkManagerCallbacks();
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Host_OnClientDisconnectCallback;
        Debug.Log(playerDataNetworkList.Count);
        NetworkManager.Singleton.StartHost();
        Debug.Log(playerDataNetworkList.Count);


    }
    private void RemoveNetworkManagerCallbacks()
    {

        var netMan = NetworkManager.Singleton;
        if (netMan != null)
        {
            netMan.ConnectionApprovalCallback -= NetworkManager_ConnectionApprovalCallback;
            netMan.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
            netMan.OnClientDisconnectCallback -= NetworkManager_Host_OnClientDisconnectCallback;
            netMan.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnectCallback;
            netMan.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnectedCallback;
        }
    }

    private void NetworkManager_Host_OnClientDisconnectCallback(ulong clientId)
    {
        for (int i = 0; i <playerDataNetworkList.Count; i++) { 
            PlayerData playerData = playerDataNetworkList[i];
            if(playerData.clientId == clientId)
                playerDataNetworkList.RemoveAt(i);
        }
    }
    public void Shutdown(ulong clientId)
    {
        if(clientId ==  NetworkManager.ServerClientId)
        {
            playerDataNetworkList.Clear();
            Debug.Log(playerDataNetworkList.Count);
        }
        NetworkManager.Singleton.Shutdown();

    }
    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Host_OnClientDisconnectCallback(clientId);
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { clientId = clientId, colorId = GetFirstUnusedColorId() });
        SetPlayerNameServerRPC(GetPlayerName());
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != SceneLoader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }
        connectionApprovalResponse.Approved = true;
    }
    public bool IsPlayerIndexConnected(int playerIndex) => playerIndex < playerDataNetworkList.Count;
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for(int i = 0; i< playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
                return i;
        }
        return -1;
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }
        return default;
    }
    public PlayerData GetPlayerData() => GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    public PlayerData GetPlayerDataFrompLayerIndex(int playerIndex) => playerDataNetworkList[playerIndex];
    public Color GetPlayerColor(int colorId) => playerColors[colorId];

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRPC(colorId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRPC(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId))
        {
            return;
        }

        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.colorId = colorId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if(playerData.colorId == colorId) return false;
        }
        return true;
    }
    private int GetFirstUnusedColorId()
    {
        for (int i = 0; i< playerColors.Count; i++) 
        { 
            if(IsColorAvailable(i)) return i;
        }
        return -1;
    }

    #region Equipment management

    public void ChangePlayerEquipment(int primaryEquipmentId, int secondaryEquipmentId)
    {
        ChangePlayerEquipmentServerRpc(primaryEquipmentId, secondaryEquipmentId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerEquipmentServerRpc(int primaryEquipmentId, int secondaryEquipmentId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        if (playerDataIndex == -1) return;

        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.primaryEquipment = (EquipmentType)primaryEquipmentId;
        playerData.secondaryEquipment = (EquipmentType)secondaryEquipmentId;

        playerDataNetworkList[playerDataIndex] = playerData;
    }

    #endregion
}
