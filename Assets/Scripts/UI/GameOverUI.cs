using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;   
    private void Awake()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        // TODO: remove the logs
        Debug.Log("State change was called in game over UI");
        if (GameManager.Instance.IsGameOver())
        {
            Debug.Log("State is game over");
            text.text = GameManager.Instance.GetWinner().Value.playerName.ToString();
            Show();
        }
        else
        {
            Debug.Log("State is not game over");
        }
        // I'm pretty sure this isn't necessary as there won't be a scenario where the game becomes no longer over
        //else
        //{
        //    Hide();
        //}
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
    }
}
