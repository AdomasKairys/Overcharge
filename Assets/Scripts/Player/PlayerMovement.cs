using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float moveSpeedMultiplier;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float grappleSpeed;
    public float swingSpeed;
    public float climbSpeed;
    public float dashSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    public float wallRunSpeed;

    public float maxYSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool isReadyToJump;

    [Header("Keybinds")]
    //public KeyCode jumpKey = KeyCode.Space;
    //public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public Transform orientation;
    public bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool isExitingSlope;
    public float slideForce;

    [Header("References")]
    public Climbing climbingSc;
    public ThirdPersonCam cam;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDir;

    Rigidbody rb;

    private MovementState state;
    public enum MovementState
    {
        walking,
        crouching,
        wallrunning,
        climbing,
        sliding,
        swinging,
        grappling,
        dashing,
        knockback,
        air
    }


    public bool activeGrapple;
    public bool isSwinging;
    public bool isSliding;
    public bool isWallrunning;
    public bool isClimbing;
    public bool isDashing;
    public bool isKnockedBack;


    [Header("Magnet wall speed")]
    public float VerticalMagnetRunSpeed;
    public float VerticalMagnetClimbSpeed;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameSettings.Instance.playerInputs.MoveAction.Enable();

            GameSettings.Instance.playerInputs.JumpAction.performed += OnJump;
            GameSettings.Instance.playerInputs.JumpAction.Enable();

            moveSpeed = walkSpeed;
            moveSpeedMultiplier = 1f;
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            isReadyToJump = true;
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            GameSettings.Instance.playerInputs.MoveAction.Disable();
            GameSettings.Instance.playerInputs.JumpAction.performed -= OnJump;
            GameSettings.Instance.playerInputs.JumpAction.Disable();
        }

        base.OnNetworkDespawn();
    }

    //private void Start()
    //{
    //    moveSpeed = walkSpeed;
    //    moveSpeedMultiplier = 1f;
    //    rb = GetComponent<Rigidbody>();
    //    rb.freezeRotation = true;

    //    isReadyToJump = true;
    //}

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isReadyToJump && isGrounded)
        {

            isReadyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // TODO: figure out why updates are being called for other player's intances even though this should be disabled on other players

    private void Update()
    {
        if (!IsOwner) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        MyInput();
        StateHandler();
        //Debug.Log(isKnockedBack);
        rb.drag = isGrounded && state != MovementState.dashing && state != MovementState.knockback && !isSwinging ? groundDrag : 0;
        
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;

        SpeedControl();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        MovePlayer(); 
    }

    private void MyInput()
    {
        horizontalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().x;
        verticalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().y;

    }
    private MovementState lastState;
    private void StateHandler()
    {
        if (isKnockedBack)
        {
            state = MovementState.knockback;
            desiredMoveSpeed = dashSpeed;
        }
        else if (isSwinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }
        else if (isDashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }
        else if (isClimbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (isWallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        else if (isGrounded)
        {
            state = MovementState.walking;
            if (IsOnSlope() && rb.velocity.y < -0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
        if (state != MovementState.dashing && state != MovementState.knockback && state != MovementState.swinging && Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else 
        {
            moveSpeed = desiredMoveSpeed;
        }
        

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //bug here fixed (?)
            if (flatVel.magnitude < walkSpeed && desiredMoveSpeed <= startValue || flatVel.magnitude < walkSpeed && desiredMoveSpeed == walkSpeed)
            {
                break;
            }

            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (IsOnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
    }
    private float detectionLength = 1.25f;
    private float sphereCastRadius = 0.25f;
    private void MovePlayer()
    {
        if (climbingSc.isExitingWall || state == MovementState.dashing || state == MovementState.knockback
            || activeGrapple || isSwinging) return;
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        bool isWallInDir = Physics.SphereCast(transform.position, sphereCastRadius, moveDir, out RaycastHit wallFrontHit, detectionLength, whatIsGround);
        if (isWallInDir && !isClimbing)
        {

            float wallLookAngle = Vector3.Angle(moveDir, -wallFrontHit.normal);
            if(wallLookAngle < 90f)
            {
                moveDir = wallLookAngle > 15f ? Vector3.ProjectOnPlane(moveDir, wallFrontHit.normal).normalized : Vector3.zero;
            }
        }

        if (IsOnSlope() && !isExitingSlope)
        {
            if (rb.velocity.y > -0.1f)
                rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDir), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);

            if (rb.velocity.y < -0.1f)
                rb.AddForce(GetSlopeMoveDirection(moveDir) * slideForce, ForceMode.Force);
        }
        else if (isGrounded)
        {
            rb.AddForce(10f * moveSpeed * moveDir.normalized, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDir.normalized, ForceMode.Force);
        }

        if(!isWallrunning) rb.useGravity = !IsOnSlope();
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed * moveSpeedMultiplier)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed * moveSpeedMultiplier;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
        //if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
        //    rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
    }

    private void Jump()
    {
        isExitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        isReadyToJump = true;

        isExitingSlope = false;
    }

    public bool IsOnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    public void UniversalKnockback(Vector3 otherPosition, float strength, ulong targetId, bool resetVel = false)
    {
        UniversalKnockbackServerRPC(otherPosition, strength, targetId, resetVel);
    }
    [ServerRpc(RequireOwnership=false)]
    private void UniversalKnockbackServerRPC(Vector3 otherPosition, float strength, ulong targetId, bool resetVel)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { targetId }
            }
        };
        UniversalKnockbackClientRPC(otherPosition, strength, resetVel, clientRpcParams);
    }
    [ClientRpc]
    private void UniversalKnockbackClientRPC(Vector3 otherPosition, float strength, bool resetVel, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("hello");
        isKnockedBack = true;
        var thisRb = GetComponent<Rigidbody>();
        if (resetVel)
        {
            thisRb.velocity = Vector3.zero;
        }
        Vector3 pushDirection = (thisRb.transform.position - otherPosition).normalized;
        thisRb.AddForce(pushDirection * strength, ForceMode.Impulse);
        Invoke(nameof(stopKnockback), 0.25f);
    }
    private void stopKnockback()
    {
        isKnockedBack = false;
    }
    public void PushFrom(Vector3 otherPosition, float pushForce)
    {
        Debug.Log("Getting pushed from " + otherPosition + " with force of " + pushForce);
        Vector3 pushDirection = (rb.transform.position - otherPosition).normalized;
        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    public void PushTo(Vector3 otherPosition, float pushForce)
    {
        Debug.Log("Getting pushed towards " + otherPosition + " with force of " +  pushForce);
        Vector3 pushDirection = (otherPosition - rb.transform.position).normalized;
        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
    }

    public IEnumerator UseSpeedBoost(float boostSpeedMultiplier, float boostDuration)
    {
        moveSpeedMultiplier = boostSpeedMultiplier;
        yield return new WaitForSeconds(boostDuration);
        moveSpeedMultiplier = 1f;
    }
}
