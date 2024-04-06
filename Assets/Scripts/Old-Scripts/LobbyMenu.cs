using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkButtons : MonoBehaviour
{
    public void Join()
    {
        GameMultiplayer.Instance.StartClient();
    }
    public void Host()
    {
        GameMultiplayer.Instance.StartHost();
        SceneLoader.LoadScene(SceneLoader.Scene.CharacterSelectScene);
    }
}
