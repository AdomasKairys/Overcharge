using TMPro;
using UnityEngine;

public class GameCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private MonoBehaviour[] player;
    [SerializeField] private GameObject[] UI;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
    private void Update()
    {
        countdownText.text = Mathf.Ceil(GameManager.Instance.GetCountdownToStartTimer()).ToString();
    }
    private void Show()
    {
        SetTo(false);
        
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        SetTo(true);
        
        gameObject.SetActive(false);
    }
    void SetTo(bool to)
    {
        foreach (GameObject o in UI)
        {
            o.SetActive(to);
        }
        foreach (MonoBehaviour mb in player)
        {
            mb.enabled = to;
        }
    }
}
