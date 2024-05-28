using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
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

	SFXTrigger sfxTrigger;

	private void Awake()
	{
		sfxTrigger = GetComponent<SFXTrigger>();
	}

	private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        GameSettings.Instance.playerInputs.MoveAction.Enable();
        GameSettings.Instance.playerInputs.DashAction.performed += OnDash;
        GameSettings.Instance.playerInputs.DashAction.Enable();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        Dash();
    }

    private void OnDisable()
    {
        GameSettings.Instance.playerInputs.MoveAction.Disable();

        GameSettings.Instance.playerInputs.DashAction.performed -= OnDash;
        GameSettings.Instance.playerInputs.DashAction.Disable();
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
		sfxTrigger.PlaySFX_CanStop("dash", false);

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

		StartCoroutine(StopAudioAfterDash());
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
        {
            rb.useGravity = true;
		}         
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().x; //Input.GetAxisRaw("Horizontal");
        float verticalInput = GameSettings.Instance.playerInputs.MoveAction.ReadValue<Vector2>().y; //Input.GetAxisRaw("Vertical");

        Vector3 direction;

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }

	private IEnumerator StopAudioAfterDash()
	{
		yield return new WaitForSeconds(1.3f);
		sfxTrigger.StopSFX("dash");
	}
}
