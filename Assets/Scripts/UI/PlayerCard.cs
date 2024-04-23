using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText, nameText, scoreText;

    public void Initialize(string name)
    {
        nameText.text = name;
        rankText.text = "1";
        scoreText.text = "0";
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetRank(int rank)
    {
        rankText.text = rank.ToString();
    }
}
