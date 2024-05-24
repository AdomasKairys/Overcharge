using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SettingKeybindManager : MonoBehaviour
{
    public void updateKeybindText(string buttonNam, string key)
	{
        string buttonName = buttonNam+".Button";
        GameObject buttonObject = GameObject.Find(buttonName);

        // Check if the GameObject was found
        if (buttonObject != null)
        {
            // Get the Button component from the GameObject
            Button buttonComponent = buttonObject.GetComponent<Button>();

            // Check if the Button component was found
            if (buttonComponent != null)
            {
                // Get the TMP_Text component in the children of the button
                TMP_Text buttonText = buttonComponent.GetComponentInChildren<TMP_Text>();

                // Check if the TMP_Text component was found
                if (buttonText != null)
                {
                    // Set the text of the button
                    buttonText.text = key;
                }
                else
                {
                    Debug.LogWarning("TMP_Text component not found in button's children.");
                }
            }
            else
            {
                Debug.LogWarning("Button component not found on the GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("GameObject with the specified name not found.");
        }
    }
	public void Update()
	{
        if (PlayerPrefs.HasKey("CK")&&PlayerPrefs.GetString("CK")!="Null")
        {
            string currentKey = PlayerPrefs.GetString("CK");
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    PlayerPrefs.SetString(currentKey, keyCode.ToString());
                    updateKeybindText(currentKey, keyCode.ToString());
                    PlayerPrefs.SetString("CK", "Null");
                    break;
                }
            }
        }
    }
    public void updateKeybindButton(string s)
	{
        PlayerPrefs.SetString("CK", s);
	}
    public void Test()
	{

	}
}
