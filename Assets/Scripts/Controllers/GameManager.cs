using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : NetworkBehaviour
{
	[SerializeField] private Transform playerPrefab;

	public static GameManager Instance { get; private set; }

	[Header("Scoreboard")]
	[SerializeField] private GameObject scoreboard;
	[SerializeField] private PlayerCard playCardPrefab;
	[SerializeField] private Transform playerCardParent;

	private Dictionary<ulong, PlayerCard> _playerCards = new Dictionary<ulong, PlayerCard>();

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

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
    public bool IsGamePlaying() => state.Value == State.GamePlaying;
    public bool IsCountdownToStartActive() => state.Value == State.CountdownToStart;
    public float GetCountdownToStartTimer() => countDownToStartTimer.Value;

	public static void PlayerJoined(ulong clientID, string name, string status)
	{
		PlayerCard newCard = Instantiate(GameManager.Instance.playCardPrefab, Instance.playerCardParent);
		Instance._playerCards.Add(clientID, newCard);
		newCard.Initialize(name.ToString(), status.ToString());
	}

	//public static void PlayerLeft(ulong clientID)
	//{
	//	if (Instance._playerCards.TryGetValue(clientID, out PlayerCard playerCard))
	//	{
	//		Destroy(playerCard.gameObject);
	//		Instance._playerCards.Remove(clientID);
	//	}
	//}
}
