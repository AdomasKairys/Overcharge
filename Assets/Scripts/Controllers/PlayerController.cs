using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    //could be change to a list of monobehaviours/networkbehaviours
    public PlayerMovement pm;
    public PlayerSphereEffect psf;
    public DashTrail dt;
    public ProjectileController prjc;
    public Climbing cl;
    public Swinging sw;
    public WallRunning wr;
    public CinemachineFreeLook fl;
    public Dashing ds;
    public GameObject ui;
    public PlayerVisual playerVisual;
    public PlayerStateController psc;
    public TagController tc;
    public InventoryController inventoryController;

    void Start()
    {
        // Initially disable all equipment
        sw.enabled = false;
        prjc.enabled = false;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));

        // Enable equipment based on selection
        switch (playerData.primaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.UseKey = KeyCode.Mouse0;
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.UseKey = KeyCode.Mouse0;
                break;
        }
        switch (playerData.secondaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.UseKey = KeyCode.Mouse1;
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.UseKey = KeyCode.Mouse1;
                break;
        }
        ui.GetComponent<UIController>().SetEquipment(playerData.primaryEquipment, playerData.secondaryEquipment);
        if (!IsOwner)
        {
            pm.gameObject.layer = LayerMask.NameToLayer("otherPlayer");
            fl.Priority = 0;
            pm.enabled = false;
            cl.enabled = false;
            sw.enabled = false;
            wr.enabled = false;
            ds.enabled = false;
            psc.enabled = false;
            tc.enabled = false;
            dt.enabled = false;
            prjc.enabled = false;
            psf.enabled = false;
            inventoryController.enabled = false;
            ui.SetActive(false);
        }
        else
        {
            fl.Priority = 10;
        }
        
    }

}
