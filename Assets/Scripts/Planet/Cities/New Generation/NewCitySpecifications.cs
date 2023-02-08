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
    public Vector2 foundationHeightRange = Vector2.zero;

    //City walls
    public bool shouldGenerateCityPerimeterWalls = true;

    //---

    public float GetRandomFoundationHeight() { return Random.Range(foundationHeightRange.x, foundationHeightRange.y); }


}
