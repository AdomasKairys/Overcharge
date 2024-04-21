using Unity.Netcode;
using UnityEngine;


public abstract class EquipmentController : NetworkBehaviour
{
    [Header("Input key for equipment")]
    [SerializeField]
    private KeyCode _useKey = KeyCode.None;

    public KeyCode UseKey
    {
        get => _useKey;
        set => _useKey = value;
    }
}

