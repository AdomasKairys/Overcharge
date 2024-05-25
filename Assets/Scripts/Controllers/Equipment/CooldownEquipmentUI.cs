using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CooldownEquipmentUI : NetworkBehaviour
{
    [SerializeField] private Image eq1ImageCooldown;
    [SerializeField] private Image eq2ImageCooldown;
    [SerializeField] private Swinging swinging;
    [SerializeField] private ProjectileController projectileController;
    // Start is called before the first frame update
    void Start()
    {
        eq1ImageCooldown.fillAmount = 0f;
        eq2ImageCooldown.fillAmount = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        float eq1Cooldown = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).primaryEquipment == EquipmentType.GrapplingHook ? swinging.GetCooldownTimer() : projectileController.GetCooldownTimer();
        float eq2Cooldown = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).secondaryEquipment == EquipmentType.RocketLauncher ? projectileController.GetCooldownTimer() : swinging.GetCooldownTimer();

        float eq1MaxCooldown = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).primaryEquipment == EquipmentType.GrapplingHook ? swinging.swingCooldown : projectileController.maxCooldown;
        float eq2MaxCooldown = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId).secondaryEquipment == EquipmentType.RocketLauncher ? projectileController.maxCooldown : swinging.swingCooldown;

        if (eq1Cooldown <= 0.1f) 
        {
            eq1ImageCooldown.fillAmount = 0f;
        }
        else
        {
            eq1ImageCooldown.fillAmount = eq1Cooldown/ eq1MaxCooldown;
        }

        if (eq2Cooldown <= 0.1f)
        {
            eq2ImageCooldown.fillAmount = 0f;
        }
        else
        {
            eq2ImageCooldown.fillAmount = eq2Cooldown / eq2MaxCooldown;
        }
    }
}
