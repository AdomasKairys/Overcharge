using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText, nameText, scoreText;
	private static ulong playerClient;
	private static string playerName;


	public void Initialize(string name, string status)
    {
        nameText.text = name;
		statusText.text = status;
        scoreText.text = "0";
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetStatus(string status)
    {
        statusText.text = status;
    }

	// Get information for the Scoreboard
	public static void SetClient(ulong client)
	{
		playerClient = client;
	}

	public static ulong GetClient()
	{
		return playerClient;
	}

	public static void SetName(string name)
	{
		playerName = name;
	}

	public static string GetName()
	{
		return playerName;
	}

}
