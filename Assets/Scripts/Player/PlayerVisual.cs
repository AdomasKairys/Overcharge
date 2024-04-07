using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Material material;
    private void Awake()
    {
        material = new Material(GetComponent<MeshRenderer>().material);
        GetComponent<MeshRenderer>().material = material;
    }
    public void SetPlayerColor(Color color)
    {
        material.color = color;
    }
}
