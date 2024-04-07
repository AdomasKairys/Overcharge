using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapController : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }
}
