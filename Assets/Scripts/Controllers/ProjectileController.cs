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

    public event EventHandler<OnShootEventArgs> OnShoot;
    public float maxCooldown = 4f;
    public class OnShootEventArgs : EventArgs
    {
        public Vector3 spawnPos;
        public Vector3 shootDir;
    }

    public override void Initialize(EquipmentSlot slot, PlayerInputs playerInputs)
    {
        OnShoot += ProjectileController_OnShoot;

        base.Initialize(slot, playerInputs);

        _useAction.performed += OnPress;

        _initialized = true;
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        if (_useCooldown <= 0f)
        {
            _useCooldown = maxCooldown;
            Vector3 mousePosWorld = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 50f));
            Vector3 shootDir = (mousePosWorld - transform.position).normalized;
            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                spawnPos = transform.position,
                shootDir = shootDir
            });
        }
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
        if( _initialized && _useCooldown > 0f)
            _useCooldown -= Time.deltaTime;
    }
}
