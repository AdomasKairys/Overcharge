using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerMovement pm;
    public Climbing cl;
    public Swinging sw;
    public WallRunning wr;
    public CinemachineFreeLook fl;
    public Dashing ds;
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            pm.enabled = false;
            cl.enabled = false;
            sw.enabled = false;
            wr.enabled = false;
            ds.enabled = false;
            fl.Priority = 0;
        }
        else
        {
            fl.Priority = 10;
        }
    }
}
