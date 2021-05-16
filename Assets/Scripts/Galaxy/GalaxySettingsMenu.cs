using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySettingsMenu : GalaxyPopupBehaviour
{
    private static bool closedOnFrame = false;
    /// <summary>
    /// Read only property that indicates whether the settings menu has been closed on the current frame.
    /// </summary>
    public static bool ClosedOnFrame
    {
        get
        {
            return closedOnFrame;
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Sets the variable that indicates whether the settings menu has been closed on the current frame to false (essentially resetting it).
    /// </summary>
    public static void ResetClosedOnFrameBool()
    {
        closedOnFrame = false;
    }

    public override void Close()
    {
        base.Close();

        //Logs that the settings menu has been closed on the current frame.
        closedOnFrame = true;
    }

    public override void Open()
    {
        base.Open();

        SetDraggable(!GalaxyPauseMenu.IsOpen);
    }
}
