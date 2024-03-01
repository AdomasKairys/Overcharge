using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallRotation : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //transform.rotation = Quaternion.LookRotation(rb.velocity, transform.up);
        transform.Rotate(new Vector3 (rb.velocity.z, rb.velocity.y, -rb.velocity.x), Space.World);
    }
}
