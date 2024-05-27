using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;

public class PlayerReady : NetworkBehaviour
{
    public static PlayerReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;
    private Dictionary<ulong, bool> playerReadyDictionary;

    public event EventHandler OnAllPlayersReadyChanged;

    /// <summary>
    /// Bool that determines if all players in the lobby are ready.
    /// Initialized as false and only updated in ServerRpc method so it can only become true on the host (server).
    /// Used by the start button to determine if the button can be enabled or not.
    /// </summary>
    public bool AllPlayersReady { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        AllPlayersReady = false;
        InitializeReadyDictionaryServerRpc();
        base.OnNetworkSpawn();
    }

    // Get the existing player ready data
    [ServerRpc(RequireOwnership = false)]
    private void InitializeReadyDictionaryServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!IsServer) return;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            bool ready = IsPlayerReady(clientId);
            SetPlayerReadyClientRPC(ready, clientId, clientRpcParams);
        }
    }

    public void SetPlayerReadyState(bool ready)
    {
        SetPlayerReadyServerRPC(ready);
    }

    public void StartGame()
    {
        StartGameServerRpc();
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        if (!IsServer) return;

        if (AllPlayersReady)
        {
            //GameLobby.Instance.DeleteLobby();
            SceneLoader.LoadScene(SceneLoader.Scene.map1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRPC(bool ready, ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRPC(ready, serverRpcParams.Receive.SenderClientId);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = ready;

        if (!ready && AllPlayersReady)
        {
            AllPlayersReady = false;
            OnAllPlayersReadyChanged.Invoke(this, new EventArgs());
            return;
        }
         
        if(ready && !AllPlayersReady)
        {
            bool allPlayersReady = true;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!IsPlayerReady(clientId))
                {
                    allPlayersReady = false;
                    break;
                }
            }
            
            if (allPlayersReady)
            {
                AllPlayersReady = true;
                OnAllPlayersReadyChanged.Invoke(this, new EventArgs());
            }
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRPC(bool ready, ulong clientId, ClientRpcParams clientRpcParams = default)
    {
        playerReadyDictionary[clientId] = ready;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId) => playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
}
