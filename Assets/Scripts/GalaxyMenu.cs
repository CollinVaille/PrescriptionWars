using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyMenu : MonoBehaviour
{
    public static GalaxyMenu galaxyMenu;

    public Font planetNameFont;

    private void Awake ()
    {
        galaxyMenu = this;
    }
}
