using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using System;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public GameObject thirdPersonCam;
    public GameObject combatCam;

    public CameraStyle currentStyle;
    public enum CameraStyle
    {
        Basic,
        Combat
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && currentStyle != CameraStyle.Combat) SwitchCameraStyle(CameraStyle.Combat);
        if (Input.GetKeyUp(KeyCode.Mouse1) && currentStyle != CameraStyle.Basic) SwitchCameraStyle(CameraStyle.Basic);


        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir;

        if (currentStyle == CameraStyle.Basic)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticallInput = Input.GetAxis("Vertical");

            Vector3 inputDir = orientation.forward * verticallInput + orientation.right * horizontalInput;

            if (inputDir != Vector3.zero)
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
        else if(currentStyle == CameraStyle.Combat)
        {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
        }
    }
    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Combat) combatCam.SetActive(true);

        currentStyle = newStyle;
    }

    //TODO: do something with combat cam
    public void DoFov(float endValue)
    {
        StartCoroutine(
            ChangeFOV((result) => thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView = result,
            thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.FieldOfView, endValue, 0.25f)
            );
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
    }
    public void DoTilt(float zTilt)
    {
        StartCoroutine(
            ChangeFOV((result) => thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.Dutch = result,
            thirdPersonCam.GetComponent<CinemachineFreeLook>().m_Lens.Dutch, zTilt, 0.25f)
            );
    }
}
