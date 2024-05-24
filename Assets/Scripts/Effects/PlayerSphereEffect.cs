using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSphereEffect : NetworkBehaviour
{
    [SerializeField] private PlayerStateController playerStateController;
    [SerializeField] private GameObject playerSphere;
    [SerializeField] private ParticleSystem chaserEffect;
    [SerializeField] private NetworkObject networkObject;

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
        SetColor();
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        SetColor();
    }
    void SetColor()
    {
        if (playerStateController.GetState() == PlayerState.Chaser)
        {
            chaserEffect.Play();
            SetColorServerRPC(networkObject, Color.red, new Color(0.529f, 0.153f, 0.267f));

        }
        else if (playerStateController.GetState() == PlayerState.Runner)
        {
            chaserEffect.Stop();
            chaserEffect.Clear();
            SetColorServerRPC(networkObject, Color.blue, new Color(0.54f, 0.41f, 0.69f));

        }
    }
    [ServerRpc]
    void SetColorServerRPC(NetworkObjectReference target, Color rimColor, Color color)
    {
        SetColorClientRPC(target, rimColor, color);
    }
    [ClientRpc]
    void SetColorClientRPC(NetworkObjectReference target, Color rimColor, Color color)
    {
        if (!target.TryGet(out NetworkObject networkObject))
            return;

        var player = networkObject.transform.Find("Player");
        player.GetComponent<PlayerSphereEffect>().material.SetColor("_RimColor", rimColor);
        player.GetComponent<PlayerSphereEffect>().material.SetColor("_Color", color);

    }
    private new void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
