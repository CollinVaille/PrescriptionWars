using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CityType
{
    public string name = "New City Type";
    public float spawnChance = 1.0f; //Higher the number, the more likely to spawn, no limit

    //Biomes
    public string[] biomes; //Leave unitialized to indicate all

    //Buildings
    public string[] buildings;

    //Walls
    public string[] wallSections;

    //Gates
    public string[] gates;

    //Fence Posts
    public float fencePostChance = 0.5f; //0 = 0% spawn chance, 1 = 100% spawn chance
    public string[] fencePosts;

    //Bridges
    public string[] bridges;

    //Materials
    public string defaultWallMaterial, defaultFloorMaterial;
    public string[] wallMaterials;
    public string[] floorMaterials;

    //Vertical scalers
    public string minorVerticalScaler, majorVerticalScaler;

    //City lights
    public string[] lights;

    public string GetVerticalScaler(bool minor)
    {
        return "Planet/City/Vertical Scalers/" + (minor ? minorVerticalScaler : majorVerticalScaler);
    }
}
