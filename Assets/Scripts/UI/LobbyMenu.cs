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
        SceneLoader.LoadScene(SceneLoader.Scene.CharacterSelectScene);
    }
    public void Host()
    {
        GameMultiplayer.Instance.StartHost();
        SceneLoader.LoadScene(SceneLoader.Scene.CharacterSelectScene);
    }
}
