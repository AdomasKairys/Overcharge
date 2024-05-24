using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    //could be change to a list of monobehaviours/networkbehaviours
    public PlayerMovement pm;//keybinds update
    public DashTrail dt;
    public ProjectileController prjc;//keybinds update
    public Climbing cl;//keybinds update
    public Swinging sw;//keybinds update
    public WallRunning wr;//keybinds update
    public CinemachineFreeLook fl;
    public Dashing ds;//keybinds update
    public GameObject ui;
    public PlayerVisual playerVisual;
    public PlayerStateController psc;
    public TagController tc;
    public InventoryController inventoryController;

    private KeyCode primEquipKey = KeyCode.Mouse0;
    private KeyCode secEquipKey = KeyCode.Mouse1;
    void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        sw.enabled = false;
        prjc.enabled = false;

        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));

        updateAllKeybinds();
        // Enable equipment based on selection
        switch (playerData.primaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.UseKey = primEquipKey;
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.UseKey = primEquipKey;
                break;
        }
        switch (playerData.secondaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.UseKey = secEquipKey;
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.UseKey = secEquipKey;
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
            dt.enabled = false;
            prjc.enabled = false;
            inventoryController.enabled = false;
            ui.SetActive(false);
            Destroy(ui); //throws an error but if removed everything brakes
        }
        else
        {
            fl.Priority = 10;
        }
    }
    public void updateAllKeybinds()
	{
        updateKeybinds();
        pm.updateKeybinds();
        wr.updateKeybinds();
        cl.updateKeybinds();
        ds.updateKeybinds();
    }
    public void updateKeybinds()
    {
        if (PlayerPrefs.HasKey("primEquipKey"))
        {
            string keyString = PlayerPrefs.GetString("primEquipKey");
            primEquipKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString);
        }
        if (PlayerPrefs.HasKey("secEquipKey"))
        {
            string keyString = PlayerPrefs.GetString("secEquipKey");
            secEquipKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString);
        }
    }
}
