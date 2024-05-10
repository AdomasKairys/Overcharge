using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using System;
using Unity.Netcode;
using System.Linq;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public WallRunning wr;
    public PlayerMovement pr;
    public NetworkObject netObj;

    public float rotationSpeed;

    public GameObject thirdPersonCam;

    private bool isFovChanged;
    private bool isTiltChanged;

    public float defaultFov = 50f;
    public float maxFovChange = 20f;

    // Start is called before the first frame update
    void Start()
    {
        UnfreezeCamera();
        isTiltChanged = false;
        isFovChanged = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void LateUpdate()
    {
        CameraEffects();
    }
    private int camIndex = 0;

    void Update()
    {
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(netObj.OwnerClientId).playerState == PlayerState.Dead)
        {
            var thirdPersonCams = GameObject.FindGameObjectsWithTag("ThirdPersonCam");
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                thirdPersonCams[camIndex].GetComponent<CinemachineFreeLook>().Priority = 0;
                camIndex = (camIndex + 1) % thirdPersonCams.Count();
                thirdPersonCams[camIndex].GetComponent<CinemachineFreeLook>().Priority = 10;
            }
        }
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticallInput = Input.GetAxis("Vertical");

        Vector3 inputDir = orientation.forward * verticallInput + orientation.right * horizontalInput;
        
        //if (inputDir != Vector3.zero)
        //    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
         

    }
    public void FreezeCamera()
    {
        CinemachineCore.GetInputAxis = delegate (string axisName) { return 0; };
    }
    public void UnfreezeCamera()
    {
        CinemachineCore.GetInputAxis = delegate (string axisName) { return Input.GetAxis(axisName); };

    }
    private void CameraEffects()
    {
        if (rb.velocity.magnitude > 15 && !isFovChanged)
        {
            isFovChanged = true;
            DoFov(defaultFov+2*((rb.velocity.magnitude - 20f > maxFovChange)?maxFovChange:(rb.velocity.magnitude - 20f)));
        }
        else if (isFovChanged)
        {
            isFovChanged = false;
            DoFov(defaultFov);
        }
        if(pr.isWallrunning && !isTiltChanged)
        {
            isTiltChanged = true;
            if (wr.isWallLeft) DoTilt(-10f);
            if (wr.isWallRight) DoTilt(10f);
        }
        else if(isTiltChanged)
        {
            isTiltChanged = false;
            DoTilt(0f);
        }
    }

    public void DoFov(float endValue)
    {
        IEnumerator fovCoroutine = ChangeFOV((result) => thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView = result,
            thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView, endValue, 0.25f);
        StopCoroutine(fovCoroutine);
        StartCoroutine(fovCoroutine);
    }
    private IEnumerator ChangeFOV(Action<float> fvalue, float startValue,  float endValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            fvalue(Mathf.Lerp(startValue, endValue, time / duration));
            yield return null;
            time += Time.deltaTime;
        }
        fvalue(endValue);
    }
    public void DoTilt(float zTilt)
    {
        IEnumerator tiltCoroutine = ChangeFOV((result) => thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.Dutch = result,
            thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.Dutch, zTilt, 0.25f);
        StopCoroutine(tiltCoroutine);
        StartCoroutine(tiltCoroutine);
    }
}
