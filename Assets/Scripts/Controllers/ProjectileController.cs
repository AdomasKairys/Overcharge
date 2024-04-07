using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Transform pfRocket;
    private float cooldownTimer;
    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 spawnPos;
    }
    private void Awake()
    {
        OnShoot += ProjectileController_OnShoot;
    }

    private void ProjectileController_OnShoot(object sender, OnShootEventArgs e)
    {
        Transform rocket = Instantiate(pfRocket, e.spawnPos, Quaternion.identity);

        Vector3 shootDir = cam.forward;

        rocket.GetComponent<Rocket>().Setup(shootDir);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            float maxCooldownTimer = 0.5f;
            cooldownTimer = maxCooldownTimer;
            OnShoot?.Invoke(this, new OnShootEventArgs { 
                spawnPos = transform.position
            });
        }
        else if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }
}
