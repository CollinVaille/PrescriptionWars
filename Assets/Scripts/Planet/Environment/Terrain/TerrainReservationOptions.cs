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
    public float relativeMaxHeight;
    public TerrainResModType terrainModification = TerrainResModType.NoChange;

    //Options for new generation
    public Vector2Int heightRange;
    public float preferredSteepness;

    //Options for restorations
    public Vector3 position;

    public TerrainReservationOptions(bool newGeneration, bool circular, int radius, float absoluteMaxHeight)
    {
        this.newGeneration = newGeneration;
        this.circular = circular;
        this.radius = radius;
        this.relativeMaxHeight = absoluteMaxHeight;
    }
}
