using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is meant to serve as an all-in-one for any data passing that needs to be done with regard to reserving space on the terrain for things like a city.
 * It does all of the following:
 * 1. Contains the parameters passed to PlanetTerrain.planetTerrain.ReserveTerrainPosition(...).
 * 2. Facilitates data passing between the helper functions called by PlanetTerrain.planetTerrain.ReserveTerrainPosition(...).
 * 3. Stores/persists all the data about terrain reservation for a city and is passed back into PlanetTerrain.planetTerrain.ReserveTerrainPosition(...) on restoration to get the same result as before.
 * 
 * NOTE: To accomplish #2 and #3, PlanetTerrain.planetTerrain.ReserveTerrainPosition(...) modifies the values of the instance of this class it receives.
*/
[System.Serializable]
public class TerrainReservationOptions
{
    public enum TerrainResModType { NoChange, Flatten, FlattenEdgesCeilMiddle } //When reserving a spot on the terrain, should the city flatten the area underneath?

    public bool newGeneration;

    //General options (new or restored)
    public bool circular = false;
    public int radius;
    public float relativeMaxHeight; //This does not impact what location is chosen. It just controls the maximum height the reserved terrain will be after modification
    public TerrainResModType terrainModification = TerrainResModType.NoChange;
    public bool foundPlaceOnTerrain = false; //This is used as recordkeeping for the reservation system. Do not touch it.

    //Options for new generation
    public int minHeightToFlattenTo; //This does not impact what location is chosen. It just controls the minimum height to flatten the terrain to afterwards
    public float preferredSteepness;
    public Transform targetToGenerateCloseTo;
    public float minimumDistanceFromTarget;
    public bool centerOnTerrainAsFallbackIfPossible = false;

    //Options for restorations
    public Vector3 globalPosition;
    public Vector2Int terrainAreaCoordinates;

    public TerrainReservationOptions(bool newGeneration, bool circular, int radius, float absoluteMaxHeight)
    {
        this.newGeneration = newGeneration;
        this.circular = circular;
        this.radius = radius;
        relativeMaxHeight = absoluteMaxHeight;
    }
}
