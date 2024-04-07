using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Rocket : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private Transform rocket;
    public void Setup(Vector3 shootDir)
    {
        rocket.up = shootDir;

        Rigidbody ridgidBody = GetComponent<Rigidbody>();
        float moveSpeed = 150f;
        ridgidBody.AddForce(shootDir * moveSpeed, ForceMode.Impulse);
        Destroy(gameObject, 5f);
    }
    public void FixedUpdate()
    {
        rocket.Rotate(new Vector3(0f, 100f, 0f), Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {

        if (collisionLayerMask == (collisionLayerMask | (1 << other.gameObject.layer)))
        {

            KnockBackPlayer();

            Destroy(gameObject);


        }
    }
    private void KnockBackPlayer()
    {

        Collider[] players = Physics.OverlapSphere(transform.position, 10f, playerLayerMask); //right now is using tag collider
        foreach(Collider player in players) 
        {
        Debug.Log(transform.position);

            player.gameObject.GetComponentInParent<PlayerMovement>().PushAwayFromTagged(transform.position);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
