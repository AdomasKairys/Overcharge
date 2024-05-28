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
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Show();
			sfxTrigger.PlaySFX("countDown");
        }
        else if (GameManager.Instance.IsGamePlaying())
        { 
            Hide();

            // Do this here instead of on destroy to ensure that GameManager isn't destroyed first
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
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
            // An error is encountered here as if a game object has been destroyed
            // this could also be causing the network object index out of bounds error
            if (o != null)
            {
                o.SetActive(to);
            }
        }
        foreach (MonoBehaviour mb in player)
        {
            mb.enabled = to;
        }
    }
}
