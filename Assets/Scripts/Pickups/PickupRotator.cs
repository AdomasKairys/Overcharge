using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script that handles the rotation of the pickup blocks
/// </summary>
public class PickupRotator : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(40, 50, 30) * Time.deltaTime, Space.Self);
        transform.Rotate(new Vector3(0, 70, 0) * Time.deltaTime, Space.World);
    }
}
