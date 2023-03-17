using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainReservationOptions
{
    public bool newGeneration;

    //General options (new or restored)
    public bool flatten = false, circular = false;
    public int radius;

    //Options for new generation
    public Vector2Int heightRange;
    public float preferredSteepness;

    //Options for restorations
    public Vector3 position;

    public TerrainReservationOptions(bool newGeneration, int radius, bool circular)
    {
        this.newGeneration = newGeneration;
        this.radius = radius;
        this.circular = circular;
    }
}
