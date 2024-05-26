using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText, nameText;
	private static ulong playerClient;
	private static string playerName;


	public void Initialize(string name, string status)
    {
        nameText.text = name;
		statusText.text = status;
    }

}
