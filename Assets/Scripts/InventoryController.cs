using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickUp
{
    public abstract void Use(PlayerMovement movementController);
}

public class SpeedBoost : PickUp
{
    public override void Use(PlayerMovement movementController)
    {
        Debug.Log("Speed boost used");
        movementController.StartCoroutine(movementController.UseSpeedUp());
    }
}

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement movementController;
    
    [SerializeField]
    private UIController uiController;

    // This variable stores the current pick up (if one exists)
    private PickUp pickUpSlot;

    private float disableTime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Use the pick up
            if(pickUpSlot != null)
            {
                UseCurrentPickUp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherGameObject = other.gameObject;
        if (IsSpeedBoost(otherGameObject))
        {
            Debug.Log("Speed boost picked up");
            StartCoroutine(DisablePickUp(otherGameObject));
            pickUpSlot = new SpeedBoost();
            uiController.UpdatePickUp(typeof(SpeedBoost));
            return;
        }
    }
    
    private void UseCurrentPickUp()
    {
        pickUpSlot.Use(movementController);
        pickUpSlot = null;
        uiController.UpdatePickUp(null);
    }

    private IEnumerator DisablePickUp(GameObject obj)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(disableTime);
        obj.SetActive(true);
    }

    private bool IsSpeedBoost(GameObject obj)
    {
        return obj.CompareTag("SpeedBoost");
    }
}
