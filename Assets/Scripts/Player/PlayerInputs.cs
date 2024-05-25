using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class PlayerInputs
{
    public InputAction MoveAction { get; private set; }

    public InputAction JumpAction { get; private set; }

    public InputAction DashAction { get; private set; }

    public InputAction UsePickup { get; private set; }

    public InputAction UsePrimaryEquipment { get; private set; }

    public InputAction UseSecondaryEquipment { get; private set; }

    public InputAction UpwardsWallRun { get; private set; }

    public InputAction DownwardWallRun { get; private set; }

    public PlayerInputs()
    {
        // Retrieve and setup the actions
        PlayerInputActions playerInputActions = new PlayerInputActions();
        MoveAction = playerInputActions.Player.Move;
        JumpAction = playerInputActions.Player.Jump;
        DashAction = playerInputActions.Player.Dash;
        UsePickup = playerInputActions.Player.UsePickup;
        UsePrimaryEquipment = playerInputActions.Player.UsePrimary;
        UseSecondaryEquipment = playerInputActions.Player.UseSecondary;
        UpwardsWallRun = playerInputActions.Player.UpwardsWallRun;
        DownwardWallRun = playerInputActions.Player.DownwardsWallRun;
    }

    public void Enable()
    {
        MoveAction.Enable();
        JumpAction.Enable();
        DashAction.Enable();
        UsePickup.Enable();
        UsePrimaryEquipment.Enable();
        UseSecondaryEquipment.Enable();
        UpwardsWallRun.Enable();
        DownwardWallRun.Enable();
    }

    public void Disable()
    {
        MoveAction.Disable();
        JumpAction.Disable();
        DashAction.Disable();
        UsePickup.Disable();
        UsePrimaryEquipment.Disable();
        UseSecondaryEquipment.Disable();
        UpwardsWallRun.Disable();
        DownwardWallRun.Disable();
    }
}
