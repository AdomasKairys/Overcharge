using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    public PlayerInputs playerInputs;
    private void Awake()
    {
        Instance = this;
        playerInputs=new PlayerInputs();
        playerInputs.Enable();
        DontDestroyOnLoad(gameObject);
    }
}
