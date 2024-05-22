using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedEffect : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem ps;
    // Start is called before the first frame update
    void Awake()
    {
        ps.Clear();
        ps.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.magnitude >= 20)
        {
            transform.parent.forward = rb.velocity.normalized;
            ps.Play();
        }
        else ps.Stop();
    }
}
