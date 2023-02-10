using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A single instance of this class is created for a city right before the city is newly generated.
//The city and its managers use this class to pass data back and forth while generating a new city. The data has no other use and is not saved anywhere.
//No instance of this class is created (or reloaded) for restored cities because it is not needed/used then.
public class NewCitySpecifications
{
    //Buildings
    public int extraUsedBuildingRadius = 0, extraBuildingRadiusForSpacing = 0;

    //Foundations
    public Vector2 buildingFoundationHeightRange = Vector2.zero;
    //There are some foundation types that have multiple levels of foundations/buildings. When this bool is true, it means even the lowest...
    //...levels of buildings need foundations below them so that they are not underwater. This happens when the city is near sea level and an ocean is present.
    public bool lowerBuildingsMustHaveFoundations = false;

    //City walls
    public bool shouldGenerateCityPerimeterWalls = true;

    //---

    public float GetRandomBuildingFoundationHeight() { return Random.Range(buildingFoundationHeightRange.x, buildingFoundationHeightRange.y); }


}
