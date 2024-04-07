using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        GameMultiplayer.Instance.OnTryinToJoinGame += GameManager_OnTryinToJoinGame;
        GameMultiplayer.Instance.OnFailToJoinGame += GameManager_OnFailToJoinGame;

        Hide();
    }

    private void GameManager_OnFailToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnTryinToJoinGame(object sender, System.EventArgs e)
    {
        Show();
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
        GameMultiplayer.Instance.OnTryinToJoinGame -= GameManager_OnTryinToJoinGame;
        GameMultiplayer.Instance.OnFailToJoinGame -= GameManager_OnFailToJoinGame;
    }
}
