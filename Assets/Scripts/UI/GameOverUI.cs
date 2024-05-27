using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;   
    private void Awake()
    {
        // TODO: this should be done only by the owner but this isn't a network behaviour
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsGameOver())
        {
            text.text = GameManager.Instance.GetWinner().Value.playerName.ToString();
            Show();

            // Do this here instead of on destroy to ensure that GameManager isn't destroyed first
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
