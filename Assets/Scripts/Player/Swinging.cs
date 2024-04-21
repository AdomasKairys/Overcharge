using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Swinging : EquipmentController
{
    [Header("References")]
    public NetworkObject pc;
    public LineRenderer lr;
    public MeshRenderer gun;
    public Transform gunHolder, gunTip, cam, player;
    public LayerMask whatIsGrappleable;
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
        if (joint) DrawRopeServerRPC(pc);
    }

    [ServerRpc]
    private void DrawRopeServerRPC(NetworkObjectReference pc)
    {
        DrawRopeClientRPC(pc);
    }
    [ServerRpc]
    private void SetLineRendererServerRPC(NetworkObjectReference pc, int count, bool enabled)
    {
        SetLineRendererClientRPC(pc, count ,enabled);
    }
    [ClientRpc]
    private void SetLineRendererClientRPC(NetworkObjectReference pc, int count, bool enabled)
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
    [ClientRpc]
    private void DrawRopeClientRPC(NetworkObjectReference pc)
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


    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
                            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
                            out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
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

        SetLineRendererServerRPC(pc, 2, true);
        currentGrapplePosition = gunTip.position;
    }

    public void StopSwing()
    {
        swingTimer = swingDuration;

        pm.isSwinging = false;
        SetLineRendererServerRPC(pc, 0, false);

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
        Debug.Log(gunTipPos);
        lineRenderer.SetPosition(1, currentGrapplePosition);
    }
}
