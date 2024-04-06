using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryController : NetworkBehaviour
{
    [SerializeField]
    private PlayerMovement movementController;
    
    [SerializeField]
    private UIController uiController;

    // This variable stores the current pick up (if one exists)
    private Pickup currentPickup;

    private float disableTime = 5f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Use the pick up
            if(currentPickup != null && currentPickup.Uses > 0)
            {
                UseCurrentPickUp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var otherGameObject = other.gameObject;
        if (IsPickup(otherGameObject))
        {
            Debug.Log("Pickup picked up");
            StartCoroutine(DisablePickUp(otherGameObject));
            currentPickup = GetPickup();
            Debug.Log("Current pickup: " + currentPickup);
            // TODO: implement UI
            //uiController.UpdatePickUp(typeof(SpeedBoost));
        }
    }
    
    private void UseCurrentPickUp()
    {
        currentPickup.Use();

        // Check if current pickup is fully used up
        if(currentPickup.Uses <= 0)
        {
            // Remove the pickup from inventory if so
            currentPickup = null;
        }

        // TODO: update the UI
    }

    private bool IsPickup(GameObject obj)
    {
        return obj.CompareTag("Pickup");
    }

    // Gets new random pickup
    private Pickup GetPickup()
    {
        // TODO: implement logic for randomly choosing from an array of pickups
        return ScriptableObject.CreateInstance<SpeedBoost>();
    }

    // Disables the pickup
    // TODO: make sure this is synced up somehow
    private IEnumerator DisablePickUp(GameObject obj)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(disableTime);
        obj.SetActive(true);
    }
}
