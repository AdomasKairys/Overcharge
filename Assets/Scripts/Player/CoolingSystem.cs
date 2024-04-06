using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolingSystem : MonoBehaviour
{
    private string coolingStationTag = "CoolingStation";

    public float coolValue = 20f;

    private PlayerStateController playerStateController;
    private new Light light;

    public float coolingStationCooldown = 15f;
    private bool isCoolingStationCooldown = false;
    private float currentCoolingStationCooldown;

    void Awake()
    {
        playerStateController = GetComponent<PlayerStateController>();
        light = GetComponent<Light>();
    }

    void Update()
    {
        CoolDown(ref currentCoolingStationCooldown, coolingStationCooldown, ref isCoolingStationCooldown);
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        ProcessCollision(other.gameObject);
    }

    void ProcessCollision(GameObject collider)
    {
        if (collider.CompareTag(coolingStationTag) && !isCoolingStationCooldown)
        {
            //Heal();
            if (playerStateController.currCharge - coolValue <= 0)
            {
                playerStateController.currCharge = 0;
            }
            else
            {
                playerStateController.currCharge = playerStateController.currCharge - coolValue;
            }
            isCoolingStationCooldown = true;
            currentCoolingStationCooldown = coolingStationCooldown;  
            //light.enabled = !light.enabled;   //Norisi padaryti kad sviestu
        }
    }

    //void Heal()
    //{
    //    Debug.Log("Heal");
    //}

    private void CoolDown(ref float currentCooldown, float maxCooldown, ref bool isCooldown)
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;
            }
        }
    }

    //void UpdateLight()
    //{

    //}

}
