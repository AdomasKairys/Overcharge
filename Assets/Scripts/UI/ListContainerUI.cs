using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListContainerUI : MonoBehaviour
{
    public RectTransform rectTransform;

    void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
