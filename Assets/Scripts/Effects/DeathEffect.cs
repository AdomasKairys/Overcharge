using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private PlayerStateController stateController;

    // Update is called once per frame
    private void Awake()
    {
        stateController.OnPlayerDeath += PlayerStateController_OnPlayerDeath;
    }

    private void PlayerStateController_OnPlayerDeath(object sender, System.EventArgs e)
    {
        var effect = Instantiate(deathEffect, stateController.transform.position, stateController.transform.rotation);
        Destroy(effect,3f);
    }
}
