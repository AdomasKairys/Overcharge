using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Swinging : EquipmentController
{
    [Header("References")]
    public NetworkObject pc;
    public LineRenderer lr;
    public MeshRenderer gun;
    public Transform gunHolder, gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public LayerMask playerLayer;
    public PlayerMovement pm;

    [Header("Swinging")]
    public float swingDuration;
    private float swingTimer;
    private float maxSwingDistance = 50f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    //public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    //[Header("Input")]
    //public KeyCode swingKey = KeyCode.Mouse0;

    private bool isPlayerGrappled = false;

    private void Start()
    {
        swingTimer = swingDuration;
        gun.enabled = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(UseKey) && !IsSwingOver()) StartSwing();
        if (Input.GetKeyUp(UseKey) || IsSwingOver()) StopSwing();

        CheckForSwingPoints();
    }
    private void FixedUpdate()
    {
        if (joint != null) SwingMovement();
        //check because when player dies joint is set to null but spring joint isn't destroyed
        //could also remove spring joint when player dies
        if (joint == null && GetComponent<SpringJoint>() != null)
            Destroy(GetComponent<SpringJoint>());
    }

    private void LateUpdate()
    {
        if (joint) DrawRopeRPC(pc);
    }
    [Rpc(SendTo.Everyone)]
    private void SetLineRendererRPC(NetworkObjectReference pc, int count, bool enabled)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");
        var gunHolder = player.Find("GrapplingGunHolder");
        var gun = gunHolder.Find("GrapplingGun");

        gun.GetComponent<LineRenderer>().positionCount = count;
        player.GetComponent<Swinging>().currentGrapplePosition = gunTip.position;
        gun.GetComponent<MeshRenderer>().enabled = enabled;

    }
    [Rpc(SendTo.Everyone)]
    private void DrawRopeRPC(NetworkObjectReference pc)
    {
        if (!pc.TryGet(out NetworkObject networkObject))
            return;
        var player = networkObject.transform.Find("Player");

        DrawRope(
            player, 
            player.GetComponent<Swinging>().lr, 
            player.GetComponent<Swinging>().gunHolder,
            ref player.GetComponent<Swinging>().currentGrapplePosition,
            player.GetComponent<Swinging>().predictionPoint.position, 
            player.GetComponent<Swinging>().gunTip.position
            );
    }

    private ulong hitPlayerClientId;
    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
                            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
                            out raycastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHitPlayer;
        Physics.Raycast(cam.position, cam.forward,
                            out raycastHitPlayer, maxSwingDistance, playerLayer);
        Vector3 realHitPoint;

        if (raycastHitPlayer.point == Vector3.zero)
            isPlayerGrappled = false;

        // Option 0 - Hit player
        if (raycastHitPlayer.point != Vector3.zero)
        {
            Debug.Log("Player hit");
            isPlayerGrappled = true;
            realHitPoint = raycastHitPlayer.point;
            hitPlayerClientId = raycastHitPlayer.collider.GetComponentInParent<NetworkObject>().OwnerClientId;
        }
        // Option 1 - Direct Hit
        else if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        // Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;

        // Option 3 - Miss
        else
            realHitPoint = Vector3.zero;

        // realHitPoint found
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        // realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    private void StartSwing()
    {
        // return if predictionHit not found
        if (predictionHit.point == Vector3.zero) return;

        if (isPlayerGrappled)
        {
            Debug.Log("Player hit1");
            GrapplePlayerRPC((transform.position - predictionHit.point).normalized, RpcTarget.Single(hitPlayerClientId, RpcTargetUse.Temp));
            swingTimer = 0.5f;
        }

        // deactivate active grapple
        //if (GetComponent<Grappling>() != null)
        //    GetComponent<Grappling>().StopGrapple();
        //pm.ResetRestrictions();

        pm.isSwinging = true;

        swingPoint = predictionHit.point;

        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        // the distance grapple will try to keep from grapple point. 
        //joint.maxDistance = distanceFromPoint * 0.8f;
        //joint.minDistance = distanceFromPoint * 0.25f;

        // customize values as you like
        //joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        SetLineRendererRPC(pc, 2, true);
        currentGrapplePosition = gunTip.position;
    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void GrapplePlayerRPC(Vector3 direction, RpcParams rpcParams)
    {
        rb.AddForce(direction * 200f, ForceMode.Impulse);
    }
    public void StopSwing()
    {
        swingTimer = swingDuration;

        pm.isSwinging = false;
        SetLineRendererRPC(pc, 0, false);

        Destroy(joint);
    }

    private void SwingMovement()
    {
        swingTimer -= Time.deltaTime;
        Vector3 forceHorizontal = orientation.forward * Mathf.Pow(swingTimer/swingDuration, 2) + (swingPoint - player.position).normalized;
        // right
        if (Input.GetKey(KeyCode.D)) forceHorizontal += orientation.right * Mathf.Pow(swingTimer / swingDuration, 2);
        // left
        if (Input.GetKey(KeyCode.A)) forceHorizontal -= orientation.right * Mathf.Pow(swingTimer / swingDuration, 2);

        rb.AddForce(forceHorizontal.normalized * horizontalThrustForce, ForceMode.Force);

    }
    private bool IsSwingOver()
    {
        return swingTimer <= 0f || Vector3.Angle(cam.forward, (predictionHit.point - cam.position).normalized) > 90f;
    }
    public Vector3 currentGrapplePosition;

    private void DrawRope(Transform player, LineRenderer lineRenderer, Transform gunHolder, ref Vector3 currentGrapplePosition, Vector3 swingPoint, Vector3 gunTipPos)
    {
        // if not grappling, don't draw rope
        gunHolder.forward = (swingPoint - player.position).normalized;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lineRenderer.SetPosition(0, gunTipPos);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }
}
