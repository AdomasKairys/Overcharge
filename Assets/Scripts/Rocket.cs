using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Rocket : NetworkBehaviour
{
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private Transform rocket;
    [SerializeField] private ParticleController particleController;
    private NetworkVariable<bool> isCollided = new NetworkVariable<bool>(false);
    private Rigidbody ridgidBody;
    public void Setup(Vector3 shootDir)
    {
        rocket.up = shootDir;

        ridgidBody = GetComponent<Rigidbody>();
        float moveSpeed = 95f;
        ridgidBody.AddForce(shootDir * moveSpeed, ForceMode.Impulse);
        Destroy(gameObject, 5f);
    }
    public void FixedUpdate()
    {
        if(!isCollided.Value)
            rocket.Rotate(new Vector3(0f, 100f, 0f), Space.Self);
        else 
            rocket.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (collisionLayerMask == (collisionLayerMask | (1 << other.gameObject.layer)))
        {
            KnockBackPlayer();
            particleController.PlayParticles();
            ridgidBody.velocity = Vector3.zero;
            if (IsServer)
            {
                isCollided.Value = true;
                Destroy(gameObject, 1f);
            }
        }
    }
    private void KnockBackPlayer()
    {
        Collider[] players = Physics.OverlapSphere(transform.position, 10f, playerLayerMask); //right now is using tag collider
        foreach(Collider player in players) 
        {
            PlayerMovement pm = player.gameObject.GetComponentInParent<PlayerMovement>();
            pm.RocketKnockback(transform.position, player.transform.parent.parent.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
