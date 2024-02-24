using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
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

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    private void Start()
    {
        swingTimer = swingDuration;
    }
    private void Update()
    {
        if (Input.GetKeyDown(swingKey) && !IsSwingOver()) StartSwing();
        if (Input.GetKeyUp(swingKey) || IsSwingOver()) StopSwing();

        CheckForSwingPoints();
    }
    private void FixedUpdate()
    {
        if (joint != null) SwingMovement();
    }

    private void LateUpdate()
    {
        DrawRope();
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

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    public void StopSwing()
    {
        swingTimer = swingDuration;

        pm.isSwinging = false;

        lr.positionCount = 0;


        Destroy(joint);
    }

    private void SwingMovement()
    {
        swingTimer -= Time.deltaTime;
        Vector3 forceHorizontal = orientation.forward * Mathf.Pow(swingTimer/swingDuration, 2) + (swingPoint - player.position).normalized;
        // right
        if (Input.GetKey(KeyCode.D)) forceHorizontal += orientation.right;
        // left
        if (Input.GetKey(KeyCode.A)) forceHorizontal -= orientation.right * Mathf.Pow(swingTimer / swingDuration, 2);

        rb.AddForce(forceHorizontal.normalized * horizontalThrustForce, ForceMode.Force);

        // forward
        //if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);

        //// shorten cable
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    Vector3 directionToPoint = swingPoint - transform.position;
        //    rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

        //    float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

        //    joint.maxDistance = distanceFromPoint * 0.8f;
        //    joint.minDistance = distanceFromPoint * 0.25f;
        //}
        //// extend cable
        //if (Input.GetKey(KeyCode.S))
        //{
        //    float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

        //    joint.maxDistance = extendedDistanceFromPoint * 0.8f;
        //    joint.minDistance = extendedDistanceFromPoint * 0.25f;
        //}
    }
    private bool IsSwingOver()
    {
        //print(Vector3.Angle(orientation.forward, (predictionHit.point - player.position).normalized) + " " + swingTimer);
        return swingTimer <= 0f || Vector3.Angle(cam.forward, (predictionHit.point - cam.position).normalized) > 90f;
    }
    private Vector3 currentGrapplePosition;

    private void DrawRope()
    {
        // if not grappling, don't draw rope
        if (!joint) return;

        lr.enabled = true;

        currentGrapplePosition =
            Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
}
