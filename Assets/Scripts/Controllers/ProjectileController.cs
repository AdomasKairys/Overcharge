using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileController : EquipmentController
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pfRocket;

    private float cooldownTimer;
    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 spawnPos;
        public Vector3 shootDir;
    }
    private void Awake()
    {
        OnShoot += ProjectileController_OnShoot;
    }

    private void ProjectileController_OnShoot(object sender, OnShootEventArgs e)
    {
        OnShootServerRPC(e.spawnPos, e.shootDir);
    }
    [ServerRpc]
    public void OnShootServerRPC(Vector3 spawnPos, Vector3 shootDir)
    {
        Transform rocket = Instantiate(pfRocket, spawnPos, Quaternion.identity);
        rocket.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        rocket.GetComponent<Rocket>().Setup(shootDir);
    }

    private void Update()
    {
        if(Input.GetKeyDown(UseKey) && cooldownTimer <= 0f)
        {
            float maxCooldownTimer = 0.5f;
            cooldownTimer = maxCooldownTimer;
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
}
