using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Transform pfRocket;
    [SerializeField] private GameObject predictionPoint;

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
        rocket.GetComponent<NetworkObject>().Spawn();
        rocket.GetComponent<Rocket>().Setup(shootDir);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            float maxCooldownTimer = 0.5f;
            cooldownTimer = maxCooldownTimer;
            OnShoot?.Invoke(this, new OnShootEventArgs { 
                spawnPos = transform.position,
                shootDir = predictionPoint.activeSelf ? (predictionPoint.transform.position - transform.position).normalized : cam.forward
            });
        }
        else if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
}
