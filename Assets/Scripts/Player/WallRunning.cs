using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallJumpUpForce;
    public float wallJumSideForce;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Limitations")]
    public bool doJumpOnEndOfTimer = false;
    public bool resetDoubleJumpsOnNewWall = true;
    public bool resetDoubleJumpsOnEveryWall = false;
    public int allowedWallJumps = 1;

    [Header("Gravity")]
    private bool useGravity = true; // always use gravity
    public float gravityCounterForce;

    [Header("Exiting")]
    private bool isExitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public bool isWallLeft;
    public bool isWallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    public float defaultFov = 50f;
    public float highFov = 60f;

    private bool wallRemembered;
    private Transform lastWall;
    private Vector3 lastWallNormal;

    private int wallJumpsDone;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        GameSettings.Instance.playerInputs.MoveAction.Enable();

        GameSettings.Instance.playerInputs.JumpAction.performed += OnJump;
        GameSettings.Instance.playerInputs.JumpAction.Enable();

        GameSettings.Instance.playerInputs.UpwardsWallRun.Enable();

        GameSettings.Instance.playerInputs.DownwardWallRun.Enable();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if ((isWallLeft || isWallRight) && verticalInput > 0 && IsAboveGround() && !isExitingWall)
            WallJump();
    }

    // Update is called once per frame
    private void Update()
    {
        CheckForWall();
        StateMachine();

        if (pm.isGrounded && lastWall != null)
            lastWall = null;
    }
    private void FixedUpdate()
    {
        if (pm.isWallrunning && !isExitingWall)
            WallRunningMovement();
    }
    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
        
        if ((isWallLeft || isWallRight) && NewWallHit())
        {
            wallJumpsDone = 0;
            wallRunTimer = maxWallRunTime;
        }
    }

    private bool IsAboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }
    private void StateMachine()
    {
        horizontalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().x;
        verticalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().y;

        upwardsRunning = GameSettings.Instance.playerInputs.UpwardsWallRun.inProgress;
        downwardsRunning = GameSettings.Instance.playerInputs.UpwardsWallRun.inProgress;

        if ((isWallLeft || isWallRight) && verticalInput > 0 && IsAboveGround() && !isExitingWall)
        {
            if (!pm.isWallrunning)
                StartWallRun();

            wallRunTimer -= Time.deltaTime;
            

            if (wallRunTimer < 0 && pm.isWallrunning)
            {
                if (doJumpOnEndOfTimer)
                    WallJump();

                isExitingWall = true;
            } 
        }
        else if (isExitingWall)
        {
            
            if (pm.isWallrunning)
                StopWallRun();

            if (NewWallHit())
                isExitingWall = false;
        }
        else
        {
            if (pm.isWallrunning)
                StopWallRun();
        }
    }
    private void StartWallRun()
    {
        pm.isWallrunning = true;

        //rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        wallRemembered = false;
    }
    private void RememberLastWall()
    {
        if (isWallLeft)
        {
            lastWall = leftWallHit.transform;
            lastWallNormal = leftWallHit.normal;
            print(leftWallHit.transform);
        }

        if (isWallRight)
        {
            lastWall = rightWallHit.transform;
            lastWallNormal = rightWallHit.normal;
        }
    }

    private bool NewWallHit()
    {
        if (lastWall == null)
            return true;

        if (isWallLeft && (leftWallHit.transform != lastWall
            || Mathf.Abs(Vector3.Angle(lastWallNormal, leftWallHit.normal)) > 5))
            return true;

        else if (isWallRight && (rightWallHit.transform != lastWall 
            || Mathf.Abs(Vector3.Angle(lastWallNormal, rightWallHit.normal)) > 5))
            return true;

        return false;
    }
    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upwardsRunning)
        {
            if (lastWall.CompareTag("Magnet"))
            {
                rb.velocity = new Vector3(rb.velocity.x, pm.VerticalMagnetRunSpeed, rb.velocity.z);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
            }
        }
        if (downwardsRunning)
		{
            if (lastWall.CompareTag("Magnet"))
            {
                rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
            }
        }

        if (!isExitingWall && !(isWallLeft && horizontalInput > 0) && !(isWallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if(useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);

        //seems unnecessary just call in StartWallRun() (?)
        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
    }
    private void StopWallRun()
    {
        pm.isWallrunning = false;
    }
    private void WallJump()
    {
        isExitingWall = true;
        exitWallTimer = exitWallTime;
        Vector3 wallNormal = isWallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumSideForce;
        
        bool firstJump = wallJumpsDone < allowedWallJumps;
        wallJumpsDone++;

        if (!firstJump)
            forceToApply = new Vector3(forceToApply.x, 0f, forceToApply.z);

        rb.AddForce(forceToApply, ForceMode.Impulse);

        RememberLastWall();
    }
}
