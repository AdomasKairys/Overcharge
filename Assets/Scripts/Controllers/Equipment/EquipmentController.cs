using Unity.Netcode;
using UnityEngine.InputSystem;

public enum EquipmentSlot
{
    Primary = 0, 
    Secondary = 1
}

public enum EquipmentType
{
    None = 0,
    GrapplingHook = 1,
    RocketLauncher = 2
}

public abstract class EquipmentController : NetworkBehaviour
{
    protected bool _initialized = false;

    protected PlayerInputs _playerInputs;

    protected EquipmentSlot _slot;

    protected InputAction _useAction;

    protected float _useCooldown = 0f;

    public virtual void Initialize(EquipmentSlot slot, PlayerInputs playerInputs)
    {
        _slot = slot;
        _playerInputs = playerInputs;

        if(slot == EquipmentSlot.Primary)
        {
            _useAction = _playerInputs.UsePrimaryEquipment;
        }
        else
        {
            _useAction = _playerInputs.UseSecondaryEquipment;
        }
    }

    public float GetCooldownTimer() => _useCooldown;
}

