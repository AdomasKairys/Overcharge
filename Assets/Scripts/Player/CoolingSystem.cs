using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class CoolingSystem : NetworkBehaviour
{
    public float coolRate = 0.5f;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("TagTrigger"))// reduntant check for future proofing
        {
            PlayerStateController otherStateController = other.gameObject.GetComponentInParent<PlayerStateController>();
            PlayerState otherState = otherStateController.GetState();
            if (IsServer && otherState == PlayerState.Runner)
            {
                if(otherStateController.currCharge.Value > 0)
                    otherStateController.currCharge.Value -= coolRate*Time.deltaTime;
            }
        }
    }

    //private void OnCollisionEnter(Collision other)
    //{
    //    ProcessCollision(other.gameObject);
    //}

    //void ProcessCollision(GameObject collider)
    //{
    //    if (collider.CompareTag(coolingStationTag) && !isCoolingStationCooldown)
    //    {
    //        //Heal();
    //        if (playerStateController.currCharge.Value - coolValue <= 0)
    //        {
    //            playerStateController.currCharge.Value = 0;
    //        }
    //        else
    //        {
    //            playerStateController.currCharge.Value -= coolValue;
    //        }
    //        isCoolingStationCooldown = true;
    //        currentCoolingStationCooldown = coolingStationCooldown;  
    //        //light.enabled = !light.enabled;   //Norisi padaryti kad sviestu
    //    }
    //}

    //void Heal()
    //{
    //    Debug.Log("Heal");
    //}

    //private void CoolDown(ref float currentCooldown, float maxCooldown, ref bool isCooldown)
    //{
    //    if (isCooldown)
    //    {
    //        currentCooldown -= Time.deltaTime;
    //        if (currentCooldown <= 0f)
    //        {
    //            isCooldown = false;
    //            currentCooldown = 0f;
    //        }
    //    }
    //}

    //void UpdateLight()
    //{

    //}

}
