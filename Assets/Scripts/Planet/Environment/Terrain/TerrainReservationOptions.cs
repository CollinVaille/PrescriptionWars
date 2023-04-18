using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainReservationOptions
{
    public enum TerrainResModType { NoChange, Flatten, FlattenEdgesCeilMiddle } //When reserving a spot on the terrain, should the city flatten the area underneath?

    public bool newGeneration;

    //General options (new or restored)
    public bool circular = false;
    public int radius;
    public float relativeMaxHeight; //This does not impact what location is chosen. It just controls the maximum height the reserved terrain will be after modification
    public TerrainResModType terrainModification = TerrainResModType.NoChange;

    //Options for new generation
    public int minHeightToFlattenTo; //This does not impact what location is chosen. It just controls the minimum height to flatten the terrain to afterwards
    public float preferredSteepness;
    public Transform targetToGenerateCloseTo;
    public float minimumDistanceFromTarget;
    public bool centerOnTerrainAsFallbackIfPossible = false;

    //Options for restorations
    public Vector3 position;

    public TerrainReservationOptions(bool newGeneration, bool circular, int radius, float absoluteMaxHeight)
    {
        this.newGeneration = newGeneration;
        this.circular = circular;
        this.radius = radius;
        relativeMaxHeight = absoluteMaxHeight;
    }
}
