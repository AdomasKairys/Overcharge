using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.rotation = Quaternion.LookRotation(rb.velocity, transform.up);
        transform.Rotate(new Vector3 (rb.velocity.z, rb.velocity.y, -rb.velocity.x), Space.World);
    }
}
