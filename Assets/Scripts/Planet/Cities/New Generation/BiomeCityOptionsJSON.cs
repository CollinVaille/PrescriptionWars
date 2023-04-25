using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeCityOptionsJSON
{   
    public string[] wallMaterials;
    public string[] floorMaterials;
    public string[] slabMaterials;
    public string[] groundMaterials;

    [Tooltip("0.0-1.0 = 0-100% chance for the city to have foundations.")]
    public float foundationChance = 0.5f;
}
