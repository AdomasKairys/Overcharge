using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PredictionPointUI : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform predictionPoint;
    [SerializeField] private Swinging swinging;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!predictionPoint.gameObject.activeSelf || !swinging.enabled)
        {
            img.enabled = false;
        }
        else
        {
            img.enabled = true;
            Vector3 predictionPointPos = cam.WorldToScreenPoint(predictionPoint.position);
            transform.position = predictionPointPos;
        }
        }
    }
