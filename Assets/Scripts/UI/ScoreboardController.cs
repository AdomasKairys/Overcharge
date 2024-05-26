using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class ScoreboardController : MonoBehaviour
{
	//[Header("Scoreboard")]
	[SerializeField] private GameObject scoreboard;
	//[SerializeField] private PlayerCard playCardPrefab;
	//[SerializeField] private Transform playerCardParent;

	//private Dictionary<ulong, PlayerCard> _playerCards = new Dictionary<ulong, PlayerCard>();
	//[SerializeField] private Transform playerPrefab;

	// Start is called before the first frame update
	void Start()
    {
		scoreboard.SetActive(false);
		if (GameManager.Instance != null)
		{
			//GameManager.PlayerJoined(PlayerCard.GetClient(), PlayerCard.GetName(), "Runner");
			//GameMultiplayer.Instance.ScoreboardAdd(clientIdFixedString);
			GameMultiplayer.ScoreboardAdd(PlayerCard.GetClient());
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
}
