using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class ScoreboardController : MonoBehaviour
{
	[Header("Scoreboard")]
	[SerializeField] private GameObject scoreboard;
	[SerializeField] private Transform scoreboardListContainer;
	[SerializeField] private Transform scoreboardTemplate;

	void Awake()
	{
		GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
	}

	void Start()
    {
		scoreboard.SetActive(false);
		if (GameManager.Instance != null)
		{
			foreach (PlayerData playerData in GameMultiplayer.Instance.GetPlayerList())
			{
				Transform playerCard = Instantiate(scoreboardTemplate, scoreboardListContainer);
				playerCard.gameObject.SetActive(true);
				playerCard.GetComponent<PlayerCard>().Initialize(playerData.playerName.ToString(), playerData.playerState.ToString());
			}
		}
		else
        {
            Debug.LogError("GameManager instance is null!");
        }
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.CapsLock))
		{
			scoreboard.SetActive(true);
		}
		if (Input.GetKeyUp(KeyCode.CapsLock))
		{
			scoreboard.SetActive(false);
		}
	}

	private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
	{
		for (int i = 0; i < scoreboardListContainer.childCount; i++)
		{
			Destroy(scoreboardListContainer.GetChild(i).gameObject);
		}

		foreach (PlayerData playerData in GameMultiplayer.Instance.GetPlayerList())
		{
			Transform playerCard = Instantiate(scoreboardTemplate, scoreboardListContainer);
			playerCard.gameObject.SetActive(true);
			playerCard.GetComponent<PlayerCard>().Initialize(playerData.playerName.ToString(), playerData.playerState.ToString());
		}
	}

	private void OnDestroy()
	{
		GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
	}
}
