using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
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
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

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
        air
    }


    public bool activeGrapple;
    public bool isSwinging;

    public bool isSliding;
    public bool isWallrunning;
    public bool isClimbing;
    public bool isDashing;

    private void Start()
    {
        moveSpeed = walkSpeed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        isReadyToJump = true;

    }
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        MyInput();
        StateHandler();

        rb.drag = isGrounded && state != MovementState.dashing && !isSwinging ? groundDrag : 0;
    }
    private void LateUpdate()
    {
        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && isReadyToJump && isGrounded)
        {
            isReadyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }
    private MovementState lastState;
    private void StateHandler()
    {
        if (isSwinging)
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
        if (state != MovementState.dashing && state != MovementState.swinging && Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f)
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
            if (flatVel.magnitude < walkSpeed && desiredMoveSpeed <= startValue)
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
    private void MovePlayer()
    {
        if (climbingSc.isExitingWall || state == MovementState.dashing
            || activeGrapple || isSwinging) return;

        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        bool wallTouch = Physics.OverlapSphere(transform.position,climbingSc.detectionLength, whatIsGround).Count() > 0;

        if (IsOnSlope() && !isExitingSlope)
        {
            if(rb.velocity.y > -0.1f)
                rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDir), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);

            if (rb.velocity.y < -0.1f)
                rb.AddForce(GetSlopeMoveDirection(moveDir) * slideForce, ForceMode.Force);
        }
        else if(isGrounded)
            rb.AddForce(10f * moveSpeed * moveDir.normalized, ForceMode.Force);
        else if (!isGrounded && !wallTouch)
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDir.normalized, ForceMode.Force);

        if(!isWallrunning) rb.useGravity = !IsOnSlope();
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
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
}
