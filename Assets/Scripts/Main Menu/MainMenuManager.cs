using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private static bool popupClosedOnFrame = false;
    public static bool PopupClosedOnFrame
    {
        get
        {
            return popupClosedOnFrame;
        }
        set
        {
            popupClosedOnFrame = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PopupClosedOnFrame = false;
    }
}
