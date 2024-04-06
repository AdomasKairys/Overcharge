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

    /// <summary>
    /// Stores the current pickup, null if no pickup is picked up
    /// </summary>
    public Pickup currentPickup { get; private set; }

    /// <summary>
    /// Determines if the player is currently picking up an item TODO: use this to play the animation
    /// </summary>
    public bool pickingUp = false;

    /// <summary>
    /// Determines the time it takes to choose a pickup (delay between collision with pickup and input of a new pickup into inventory)
    /// </summary>
    private float pickingUpDelay = 3f;


    /// <summary>
    /// The amount of time a pickup is disabled for after being picked up
    /// </summary>
    private float pickupDisableTime = 8f;

 
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
            // Initiates the pickup process
            StartCoroutine(Pickup(other.gameObject));
            // TODO: implement UI
            //uiController.UpdatePickUp(typeof(SpeedBoost));
        }
    }

    /// <summary>
    /// Method that handles picking up a new pickup
    /// </summary>
    private IEnumerator Pickup(GameObject pickupObj)
    {
        if (!pickingUp) // prevents repeated pickups
        {
            pickingUp = true;

            // Clears the current pickup
            currentPickup = null;

            // Initiates the disabling of the pickup
            StartCoroutine(DisablePickUp(pickupObj));

            // Waits the delay
            yield return new WaitForSeconds(pickingUpDelay);

            // Gets a new pickup
            currentPickup = GetNewPickup();

            pickingUp = false;

            Debug.Log("A new pickup picked up: " + currentPickup.Name);
        }
    }
    
    /// <summary>
    /// Uses the current pickup and removes it from the inventory if it has no more uses
    /// </summary>
    private void UseCurrentPickUp()
    {
        currentPickup.Use();

        if(currentPickup.Uses <= 0)
        {
            // Remove the pickup from inventory if fully used up
            currentPickup = null;
        }

        // TODO: update the UI
    }

    /// <summary>
    /// Checks if the object is a pickup
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool IsPickup(GameObject obj)
    {
        return obj.CompareTag("Pickup");
    }

    /// <summary>
    /// Gets a new random pickup TODO: implement logic for randomly choosing from an array of pickups
    /// </summary>
    /// <returns>A new pickup</returns>
    private Pickup GetNewPickup()
    {
        return ScriptableObject.CreateInstance<SpeedBoost>();
    }

    /// <summary>
    /// Disables the pickup
    /// NOTE: this is synced up, but idk why or how :) 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private IEnumerator DisablePickUp(GameObject obj)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(pickupDisableTime);
        obj.SetActive(true);
    }
}
