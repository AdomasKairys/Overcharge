using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public PlayerMovement pm;
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
    public PlayerInput pi;

    private PlayerInputActions _playerInputActions;

    private InputAction _usePrimaryAction;
    private InputAction _useSecondaryAction;

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
            prjc.enabled = false;
            pi.enabled = false;
            inventoryController.enabled = false;
            ui.SetActive(false);
        }
        else
        {
            fl.Priority = 10;

            // Initially disable all equipment
            sw.enabled = false;
            prjc.enabled = false;

            _playerInputActions = new PlayerInputActions();

            _usePrimaryAction = _playerInputActions.Player.UsePrimary;
            _useSecondaryAction = _playerInputActions.Player.UseSecondary;
        }
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));

        // Enable equipment based on selection
        switch(playerData.primaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.Initialize(_usePrimaryAction);
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.Initialize(_usePrimaryAction);
                break;
        }
        switch (playerData.secondaryEquipment)
        {
            case EquipmentType.None:
                break;
            case EquipmentType.GrapplingHook:
                sw.enabled = true;
                sw.Initialize(_useSecondaryAction);
                break;
            case EquipmentType.RocketLauncher:
                prjc.enabled = true;
                prjc.Initialize(_useSecondaryAction);
                break;
        }
        ui.GetComponent<UIController>().SetEquipment(playerData.primaryEquipment, playerData.secondaryEquipment);
    }
    private void Update()
    {
        if (GameManager.Instance.IsGamePlaying())
        {

        }
    }

}
