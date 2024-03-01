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
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(rb.velocity, transform.up);
        transform.Rotate(rb.velocity.normalized, Space.World);
    }
}
