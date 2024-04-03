using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelect : MonoBehaviour
{
    // Start is called before the first frame update
    public void Ready()
    {
        PlayerReady.Instance.SetPlayerReady();
    }
}
