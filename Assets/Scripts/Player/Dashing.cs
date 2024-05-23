using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;

    private InputAction _moveAction;
    private InputAction _dashAction;

    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;

    

    [Header("Settings")]
    public bool useCameraForward = false;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = false;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _playerInputActions = new PlayerInputActions();

        _moveAction = _playerInputActions.Player.Move;
        _moveAction.Enable();

        _dashAction = _playerInputActions.Player.Dash;
        _dashAction.performed += OnDash;
        _dashAction.Enable();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        Dash();
    }

    private void OnDisable()
    {
        _moveAction.Disable();

        _dashAction.performed -= OnDash;
        _dashAction.Disable();
    }

    private void Update()
    {
        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        pm.isDashing = true;
        pm.maxYSpeed = maxDashYSpeed;


        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam; /// where you're looking
        else
            forwardT = orientation; /// where you're facing (no up or down)

        Vector3 direction = GetDirection(forwardT);
        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;
        print(forceToApply);

        if (disableGravity)
            rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.isDashing = false;
        pm.maxYSpeed = 0;


        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = _moveAction.ReadValue<Vector2>().x; //Input.GetAxisRaw("Horizontal");
        float verticalInput = _moveAction.ReadValue<Vector2>().y; //Input.GetAxisRaw("Vertical");

        Vector3 direction;

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }
}
