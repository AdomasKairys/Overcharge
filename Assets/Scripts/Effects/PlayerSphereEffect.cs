using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSphereEffect : MonoBehaviour
{
    [SerializeField] private PlayerStateController playerStateController;
    [SerializeField] private GameObject playerSphere;
    [SerializeField] private ParticleSystem chaserEffect;

    public Material material;

    private void Awake()
    {
        material = new Material(playerSphere.GetComponent<MeshRenderer>().material);
        playerSphere.GetComponent<MeshRenderer>().material = material;
        chaserEffect.Stop();
        chaserEffect.Clear();
    }
    // Start is called before the first frame update
    private void Start()
    {
        UpdateColor();
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdateColor();
    }
    void UpdateColor()
    {
        if (playerStateController.GetState() == PlayerState.Chaser)
        {
            chaserEffect.Play();
            SetMaterialColor(Color.red, new Color(0.529f, 0.153f, 0.267f));

        }
        else if (playerStateController.GetState() == PlayerState.Runner)
        {
            chaserEffect.Stop();
            chaserEffect.Clear();
            SetMaterialColor(Color.blue, new Color(0.54f, 0.41f, 0.69f));

        }
    }
    void SetMaterialColor(Color rimColor, Color color)
    {
        material.SetColor("_RimColor", rimColor);
        material.SetColor("_Color", color);
    }
    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
