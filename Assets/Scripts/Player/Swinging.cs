using System.Linq;
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
        if (joint) DrawRope(pc);
    }
    
    private void SetLineRenderer(NetworkObjectReference pc, int count, bool enabled)
    {
        lr.positionCount = count;
        currentGrapplePosition = gunTip.position;
        gun.enabled = enabled;
        SetLineRendererServerRPC(pc, count, enabled);
    }
   [ServerRpc]
    private void SetLineRendererServerRPC(NetworkObjectReference pc, int count, bool enabled, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != serverRpcParams.Receive.SenderClientId).ToList()
            }
        };
        SetLineRendererClientRPC(pc, count, enabled, clientRpcParams);
    }
    [ClientRpc]
    private void SetLineRendererClientRPC(NetworkObjectReference pc, int count, bool enabled, ClientRpcParams clientRpcParams = default)
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
    private void DrawRope(NetworkObjectReference pc)
    {
        DrawRope(
            player,
            lr,
            gunHolder,
            ref currentGrapplePosition,
            predictionPoint.position,
            gunTip.position
        );
        DrawRopeServerRPC(pc);
    }
    [ServerRpc]
    private void DrawRopeServerRPC(NetworkObjectReference pc, ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != serverRpcParams.Receive.SenderClientId).ToList()
            }
        };
        DrawRopeClientRPC(pc, clientRpcParams);
    }
    [ClientRpc]
    private void DrawRopeClientRPC(NetworkObjectReference pc, ClientRpcParams clientRpcParams = default)
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

        // Option 3 - Miss

        Vector3 realHitPoint = Vector3.zero;

        if (raycastHitPlayer.point == Vector3.zero)
            isPlayerGrappled = false;

        // Option 0 - Hit player
        if (raycastHitPlayer.point != Vector3.zero)
        {
            hitPlayerClientId = raycastHitPlayer.collider.GetComponentInParent<NetworkObject>().OwnerClientId;
            if (hitPlayerClientId != OwnerClientId)
            {
                Debug.Log("Player hit");
                isPlayerGrappled = true;
                realHitPoint = raycastHitPlayer.point;
                Debug.Log(hitPlayerClientId + " " + OwnerClientId);
            }
        }
        // Option 1 - Direct Hit
        else if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;

        // Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;


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
            pm.UniversalKnockback(player.position, 50f, hitPlayerClientId);
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

        SetLineRenderer(pc, 2, true);
        currentGrapplePosition = gunTip.position;
    }
    public void StopSwing()
    {
        swingTimer = swingDuration;

        pm.isSwinging = false;
        SetLineRenderer(pc, 0, false);

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
