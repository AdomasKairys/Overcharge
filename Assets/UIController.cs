using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject text;
    public GameObject player;


    TextMeshProUGUI textMesh_velocity;

    // Start is called before the first frame update
    void Start()
    {
        textMesh_velocity = text.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textMesh_velocity.text = player.GetComponent<Rigidbody>().velocity.magnitude.ToString();
    }
}
