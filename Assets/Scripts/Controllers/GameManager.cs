using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        Debug.Log(IsServer);
        if (IsServer)
            SetRandomPlayerChaser();
    }
    private void Update()
    {
        if (!IsServer)
            return;

        switch (state.Value)
        {
            case State.WaitingToStart:
                state.Value = State.CountdownToStart;
                break;
            case State.CountdownToStart:
                countDownToStartTimer.Value -= Time.deltaTime;
                if(countDownToStartTimer.Value < 0 ) {
                    state.Value = State.GamePlaying; 
                }
                break;
            case State.GamePlaying:
                if(GameMultiplayer.Instance.IsGameOver())
                    state.Value = State.GameOver;
                break;
            case State.GameOver:
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }
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
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.position = new Vector3(UnityEngine.Random.value*10, playerTransform.position.y, UnityEngine.Random.value * 10);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
    public bool IsGamePlaying() => state.Value == State.GamePlaying;
    public bool IsCountdownToStartActive() => state.Value == State.CountdownToStart;
    public bool IsGameOver() => state.Value == State.GameOver;

    public float GetCountdownToStartTimer() => countDownToStartTimer.Value;

}
