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

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

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
    public bool isCrouching;
    public bool isClimbing;
    public bool isDashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        isReadyToJump = true;

        startYScale = transform.localScale.y;
    }
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
        print(isGrounded);
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

        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            isCrouching = true;

        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isCrouching = false;

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
        else if(isSliding)
        {
            state = MovementState.sliding;
            if (IsOnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = walkSpeed;
        }
        else if (isCrouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (isGrounded)
        {
            state = MovementState.walking;
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

            if (flatVel.magnitude < walkSpeed)
                break;

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
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDir), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if(isGrounded)
            rb.AddForce(10f * moveSpeed * moveDir.normalized, ForceMode.Force);
        else if (!isGrounded && !wallTouch)
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDir.normalized, ForceMode.Force);

        if(!isWallrunning) rb.useGravity = !IsOnSlope();
    }
    private void SpeedControl()
    {
        if (IsOnSlope() && !isExitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            print(moveSpeed);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
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
    //public void GrappleToPoint(Vector3 targetPos)
    //{
    //    activeGrapple = true;
    //    Vector3 direction = targetPos + cam.transform.forward;
    //    rb.AddForce(direction.normalized * 50f, ForceMode.Force);
    //}
    //private bool enableMovementOnNextTouch;
    //public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    //{
    //    activeGrapple = true;

    //    velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
    //    Invoke(nameof(SetVelocity), 0.1f);

    //    Invoke(nameof(ResetRestrictions), 3f);
    //}
    //private Vector3 velocityToSet;
    //private void SetVelocity()
    //{
    //    enableMovementOnNextTouch = true;
    //    rb.velocity = velocityToSet;

    //    cam.DoFov(55f);
    //}
    ////TODO: change magic numbers for fov to variables
    //public void ResetRestrictions()
    //{
    //    activeGrapple = false;
    //    cam.DoFov(50f);
    //}
    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (enableMovementOnNextTouch)
    //    {
    //        enableMovementOnNextTouch = false;
    //        ResetRestrictions();

    //        GetComponent<Grappling>().StopGrapple();
    //    }
    //}
    //public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    //{
    //    float gravity = Physics.gravity.y;
    //    float displacementY = endPoint.y - startPoint.y;
    //    Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

    //    Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
    //    Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
    //        + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

    //    return velocityXZ + velocityY;
    //}
}
