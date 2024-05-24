using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DashTrail : NetworkBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerStateController playerStateController;
    [SerializeField] private GameObject playerVisual;
    [SerializeField] private NetworkObject networkObject;

    public float activeTime = 0.5f;

    [Header("Mesh Releted")]
    public float meshRefreshRate = 0.005f;
    public float meshDestroyDelay = 0.25f;


    [Header("Shader Releted")]
    public Material material;


    private bool isTrailActive;
    private Color trailColor;

    void Update()
    {
        if (playerMovement.isDashing && !isTrailActive)
        {
            isTrailActive = true;
            trailColor = playerStateController.GetState() == PlayerState.Runner ? new Color(0, 0.26f*4, 0.75f*4) : new Color(0.75f*4, 0, 0.16f*4);
            StartTrail();
        }
    }
    private void StartTrail()
    {
        StartCoroutine(ActivateTrail(meshRefreshRate, meshDestroyDelay, material, playerVisual, trailColor,activeTime));
        StartTrailServerRPC(meshRefreshRate, meshDestroyDelay, networkObject, trailColor, activeTime);
    }
    [ServerRpc]
    private void StartTrailServerRPC(float meshRefreshRate, float meshDestroyDelay, NetworkObjectReference target, Color trailColor, float timeActive, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != serverRpcParams.Receive.SenderClientId).ToList()
            }
        };
        StartTrailClientRPC(meshRefreshRate, meshDestroyDelay, target, trailColor, timeActive, clientRpcParams);
    }
    [ClientRpc]
    private void StartTrailClientRPC(float meshRefreshRate, float meshDestroyDelay, NetworkObjectReference target, Color trailColor, float timeActive, ClientRpcParams clientRpcParams = default)
    {
        if (!target.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        var material = player.GetComponent<DashTrail>().material;
        var playerVisual = player.Find("PlayerVisual").gameObject;
        StartCoroutine(ActivateTrail(meshRefreshRate, meshDestroyDelay, material, playerVisual, trailColor, timeActive));
    }
    IEnumerator ActivateTrail(float meshRefreshRate, float meshDestroyDelay, Material material, GameObject playerVisual, Color trailColor, float timeActive)
    {
        while(timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            GameObject gObj = new GameObject();
            gObj.transform.SetPositionAndRotation(playerVisual.transform.position, playerVisual.transform.rotation);
            MeshRenderer meshRenderer = gObj.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = gObj.AddComponent<MeshFilter>();
            Mesh mesh = playerVisual.GetComponent<MeshFilter>().mesh;
            material.color = trailColor;
            meshFilter.mesh = mesh;
            meshRenderer.material = material;

            Destroy(gObj, meshDestroyDelay);

            yield return new WaitForSeconds(meshRefreshRate);
        }
        isTrailActive = false;
    }
}
