using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CityTypeJSON
{
    public string name = "New City Type";

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

    //Vertical scalers
    public string minorVerticalScaler, majorVerticalScaler;

    //City lights
    public string[] lights;

    //Other options that are not required for each city type (if a city type doesn't have them, it will fallback to some default styling)
    public CityStylingJSON styling;

    public string GetResourcePathPrefix(bool includeSlashAtEnd)
    {
        return GetResourcePathPrefix(includeSlashAtEnd, name);
    }

    public static string GetResourcePathPrefix(bool includeSlashAtEnd, string cityTypeName)
    {
        string resourcePathPrefix = "Planet/City/City Types/" + cityTypeName;

        if (includeSlashAtEnd)
            resourcePathPrefix += "/";

        return resourcePathPrefix;
    }

    public string GetVerticalScaler(bool minor)
    {
        return GetResourcePathPrefix(true) + "Vertical Scalers/" + (minor ? minorVerticalScaler : majorVerticalScaler);
    }
}
