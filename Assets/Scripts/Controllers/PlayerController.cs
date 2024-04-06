using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerController : NetworkBehaviour
{
    public PlayerMovement pm;
    public Climbing cl;
    public Swinging sw;
    public WallRunning wr;
    public CinemachineFreeLook fl;
    public Dashing ds;
    public GameObject ui;
    public GameObject predictionPoint;
    public PlayerVisual playerVisual;
    public PlayerStateController psc;
    public TagController tc;
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            fl.Priority = 0;
            pm.enabled = false;
            cl.enabled = false;
            sw.enabled = false;
            wr.enabled = false;
            ds.enabled = false;
            psc.enabled = false;
            tc.enabled = false;
            ui.SetActive(false);
            predictionPoint.GetComponent<MeshRenderer>().enabled = false;
        }
        else
        {
            fl.Priority = 10;
        }
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }
    private void Update()
    {
        if (GameManager.Instance.IsGamePlaying())
        {

        }
    }

}
