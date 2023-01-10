using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSection : MonoBehaviour
{
    public enum WallType : int { Wall = 0, FencePost = 1, Gate = 2 }

    public WallType wallType = WallType.Wall;
}

[System.Serializable]
public class WallSectionJSON
{
    public int wallTypeIndex = 0;
    public Vector3 location;
    public int yAxisRotation;

    public WallSectionJSON(WallSection wallSection)
    {
        wallTypeIndex = (int)wallSection.wallType;
        location = wallSection.transform.localPosition;
        yAxisRotation = (int)wallSection.transform.localEulerAngles.y;
    }

    public void RestoreWallSection(CityWallManager cityWallManager)
    {
        WallSection.WallType wallType = (WallSection.WallType)wallTypeIndex;

        if (wallType == WallSection.WallType.FencePost)
            cityWallManager.PlaceFencePost(location, yAxisRotation);
        else
        {
            //We want to place the fence posts manually, so always make the skipFencePost flag true here so they aren't placed automatically
            cityWallManager.PlaceWallSection(wallType == WallSection.WallType.Gate, true, location.x, location.y, location.z, yAxisRotation, false);
        }
    }
}
