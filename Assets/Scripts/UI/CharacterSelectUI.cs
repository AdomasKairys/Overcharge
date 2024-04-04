using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public void Ready()
    {
        PlayerReady.Instance.SetPlayerReady();
    }
    public void MainMenu()
    {
        GameMultiplayer.Instance.Shutdown(NetworkManager.Singleton.LocalClientId);
        SceneManager.LoadScene(SceneLoader.Scene.MainMenu.ToString());
    }
}
