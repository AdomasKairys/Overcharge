using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileController : EquipmentController
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pfRocket;

    private float cooldownTimer = 0f;
    public event EventHandler<OnShootEventArgs> OnShoot;
    public float maxCooldown = 4f;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 spawnPos;
        public Vector3 shootDir;
    }

    public override void Initialize(InputAction useAction)
    {
        OnShoot += ProjectileController_OnShoot;

        base.Initialize(useAction);
    }

    private void ProjectileController_OnShoot(object sender, OnShootEventArgs e)
    {
        OnShootServerRPC(e.spawnPos, e.shootDir);
    }

    [ServerRpc]
    public void OnShootServerRPC(Vector3 spawnPos, Vector3 shootDir)
    {
        Transform rocket = Instantiate(pfRocket, spawnPos, Quaternion.identity);
        rocket.GetComponent<NetworkObject>().Spawn();
        rocket.GetComponent<Rocket>().Setup(shootDir);
    }

    private void Update()
    {
        if( !_initialized ) { return; }

        if(_useAction.ReadValue<float>() > 0 && cooldownTimer <= 0f)
        {
            
            cooldownTimer = maxCooldown;
            Vector3 mousePosWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 50f));
            Vector3 shootDir = (mousePosWorld - transform.position).normalized;
            OnShoot?.Invoke(this, new OnShootEventArgs { 
                spawnPos = transform.position,
                shootDir = shootDir
            });
        }
        else if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
    public float GetCooldownTimer() => cooldownTimer;
}
