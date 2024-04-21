using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public enum Scene
    {
        CharacterSelectScene,
        GameScene,
        LobbyScene,
        MainMenu,
        map1
    }
    public static void LoadScene(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }
}
