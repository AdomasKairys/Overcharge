using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Transform playerPrefab;

    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private NetworkVariable<float> countDownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> countdownToEndTimer = new NetworkVariable<float>(8f);

    private List<Vector3> spawnPositions = new List<Vector3>();
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
        base.OnNetworkSpawn();
    }

    private void LateUpdate()
    {
        if (!IsServer)
            return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                // Make sure all players' player prefabs have been spawned
                if (NetworkManager.SpawnManager.SpawnedObjectsList.Where(no => no.IsPlayerObject).Count() == GameMultiplayer.Instance.GetPlayerCount())
                {
                    Debug.Log("GameManager: all players spawned, swithing to CountdownToStart");
                    state.Value = State.CountdownToStart;
                }
                break;
            case State.CountdownToStart:
                countDownToStartTimer.Value -= Time.deltaTime;
                if(countDownToStartTimer.Value < 0 ) {
                    Debug.Log("GameManager: countdown finished, swithing to GamePlaying");
                    state.Value = State.GamePlaying; 
                }
                break;
            case State.GamePlaying:
                if (false && GameMultiplayer.Instance.IsGameOver())
                {
                    Debug.Log("GameManager: one player (or none) left, swithing to GameOver");
                    state.Value = State.GameOver;
                }                    
                break;
            case State.GameOver:
                countdownToEndTimer.Value -= Time.deltaTime;
                if(countdownToEndTimer.Value < 0)
                {
                    Debug.Log("GameManager: game is over, loading in lobby");
                    //DespawnPlayers();
                    SceneLoader.LoadScene(SceneLoader.Scene.CharacterSelectScene);
                }                
                break;
        }
    }

    //// Used to manually despawn all player objects
    //private void DespawnPlayers()
    //{
    //    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    //    {
    //        var playerObject = client.PlayerObject;
    //        if (playerObject != null && playerObject.IsSpawned)
    //        {
    //            Debug.Log("Found player object belonging to " + client);
    //            playerObject.Despawn();
    //            // Remove the player object from client to prevent automatic spawning on load
    //            client.PlayerObject = null;
    //        }
    //        GameMultiplayer.Instance.ResetPlayerData();
    //    }
    //}

    public void SetRandomPlayerChaser()
    {
        var rand = new System.Random();
        var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        if(!GameMultiplayer.Instance.IsGameOver())
            GameMultiplayer.Instance.ChangePlayerState(clientIds[rand.Next(0, clientIds.Count)], PlayerState.Chaser);
    }
    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if(!IsServer) return;

        //handle spawning better this is just so that player dont clip through the map
        //float sign = 1f;
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        bool[] occupied = new bool[spawnPoints.Length];
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            /*
            sign *= ((clientId + 1) % 3) == 0 ? -1f : sign; 
            var positionX = sign * ((clientId + 1) % 2);
            var positionZ = sign * ((clientId + 2) % 2);

            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.position = new Vector3(
                20 * positionX,
                playerTransform.position.y,
                20 * positionZ
                );
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            */
            bool searching = true;
            int index = UnityEngine.Random.Range(0, spawnPoints.Length - 1);

            while (searching){
				if (index >= spawnPoints.Length)
				{
                    index = 0;
				}
				if (occupied[index])
				{
                    index += 1;
                    if (index >= spawnPoints.Length)
                    {
                        index = 0;
                    }
                }
				else
				{
                    occupied[index] = true;
                    searching = false;
				}
			}
            Transform playerTransform = Instantiate(playerPrefab, spawnPoints[index].transform);
            //playerTransform.position = spawnPoints[index].transform.position;
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        SetRandomPlayerChaser();
    }
    public PlayerData? GetWinner()
    {
        if(GameMultiplayer.Instance.IsGameOver())
        {
            return GameMultiplayer.Instance.GetAlivePlayers().First();
        }
        return null;
    }
    public bool IsGamePlaying() => state.Value == State.GamePlaying;
    public bool IsCountdownToStartActive() => state.Value == State.CountdownToStart;
    public bool IsGameOver() => state.Value == State.GameOver;

    public float GetCountdownToStartTimer() => countDownToStartTimer.Value;


    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            Debug.Log("GameManager: despawning");
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
            state.Value = State.WaitingToStart;
        }
        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        Instance = null;
        Debug.Log("GameManager: destroyed");
        base.OnDestroy();
    }
}
