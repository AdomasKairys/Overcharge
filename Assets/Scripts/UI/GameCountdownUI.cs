using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private MonoBehaviour[] player;
    [SerializeField] private GameObject[] UI;
    [SerializeField] private NetworkObject networkObject;

	SFXTrigger sfxTrigger;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}

	void Start()
    {
        player = player.Where(x => x.enabled).ToArray();
        UI = UI.Where(x => x.activeSelf).ToArray();
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        Debug.Log("State Changed" + GameManager.Instance.IsCountdownToStartActive());

        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Show();
			sfxTrigger.PlaySFX("countDown");
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
    private void OnDestroy()
    {
        GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
    }
}
