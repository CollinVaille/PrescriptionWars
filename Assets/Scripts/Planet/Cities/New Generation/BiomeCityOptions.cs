using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeCityOptions
{
    [Tooltip("Just for readability in the inspector. Not used anywhere else.")]
    public string name;

    public Planet.Biome biome;
    
    public string[] wallMaterials;
    public string[] floorMaterials;
    public string[] slabMaterials;
    public string[] groundMaterials;

    [Tooltip("0.0-1.0 = 0-100% chance for the city to have foundations.")]
    public float foundationChance = 0.5f;

    [Tooltip("If the city has foundations, the foundation height will be between x-y 2/3's of the time, and z-w the remainder.")]
    public Vector4 foundationHeightRange;
}
