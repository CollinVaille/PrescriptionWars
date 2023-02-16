using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCityWallRequest
{
    public bool circular = false; //True = circular walls, False = Square walls
    public Vector3 localCenter;
    public float halfLength;

    public NewCityWallRequest(bool circular, Vector3 localCenter, float halfLength)
    {
        this.circular = circular;
        this.localCenter = localCenter;
        this.halfLength = halfLength;
    }

    public NewCityWallRequest() { }
}
