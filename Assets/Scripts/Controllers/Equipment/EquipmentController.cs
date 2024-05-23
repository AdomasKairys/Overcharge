using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public abstract class EquipmentController : NetworkBehaviour
{
    protected bool _initialized = false;
    
    protected InputAction _useAction;

    public virtual void Initialize(InputAction useAction)
    {
        _useAction = useAction;
        _useAction.Enable();
        _initialized = true;
    }

    public override void OnNetworkDespawn()
    {
        _useAction.Disable();
        base.OnNetworkDespawn();
    }
}

