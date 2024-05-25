using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Climbing : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public PlayerMovement pm;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool isClimbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool isExitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private void OnEnable()
    {
        _playerInputActions = new PlayerInputActions();

        _moveAction = _playerInputActions.Player.Move;
        _moveAction.Enable();

        _jumpAction = _playerInputActions.Player.Jump;
        _jumpAction.performed += OnJump;
        _jumpAction.Enable();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (wallFront && climbJumpsLeft > 0)
        {
            Debug.Log("Climb jumping");
            ClimbJump();
        }
    }

    private void OnDisable()
    {
        _moveAction.Disable();

        _jumpAction.performed -= OnJump;
        _jumpAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
        StateMachine();

    }

    private void FixedUpdate()
    {
        if (isClimbing && !isExitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        
        // State 1 - Climbing
        if (wallFront && _moveAction.ReadValue<Vector2>().y > 0 && wallLookAngle < maxWallLookAngle && !isExitingWall)
        {
            if (!isClimbing && climbTimer > 0) StartClimbing();

            // timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }

        // State 2 - Exiting
        else if (isExitingWall)
        {
            if (isClimbing) StopClimbing();
            
            if (climbTimer > 0) isExitingWall = false;
        }

        // State 3 - None
        else
        {
            if (isClimbing) StopClimbing();
        }
    }
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);
        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.isGrounded || (lastWall!= null && lastWall.CompareTag("Magnet")))
        {

            if (wallFront && frontWallHit.collider.CompareTag("Magnet"))
			{
                climbTimer = float.MaxValue;
            }
            else
			{
                climbTimer = maxClimbTime;
            }
            climbJumpsLeft = climbJumps;
        }
    }
    private void StartClimbing()
    {
        isClimbing = true;
        pm.isClimbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
		if (lastWall.CompareTag("Magnet"))
		{
            rb.velocity = new Vector3(rb.velocity.x, pm.VerticalMagnetClimbSpeed, rb.velocity.z);
        }
		else
		{
            rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        pm.isClimbing = false;

    }
    private void ClimbJump()
    {
        if (pm.isGrounded) return;

        isExitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
