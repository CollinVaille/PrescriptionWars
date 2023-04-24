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

    public void MakeRequestForCityPerimeterWalls(City city, float wallHeightInLocalSpace)
    {
        circular = city.circularCity;
        localCenter = new Vector3(0.0f, wallHeightInLocalSpace, 0.0f);
        halfLength = circular ? city.radius : city.areaManager.areaSize * city.areaManager.areaTaken.GetLength(0) * 0.5f;
    }
}
