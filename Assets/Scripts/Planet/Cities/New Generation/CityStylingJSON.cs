using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityStylingJSON
{   
    public string[] wallMaterials;
    public string[] floorMaterials;
    public string[] slabMaterials;
    public string[] groundMaterials;

    public FoundationOptions foundationOptions;

    public void FillEmptyFieldsWithTheseValues(CityStylingJSON fallbackOptions)
    {
        if (wallMaterials == null || wallMaterials.Length == 0)
            wallMaterials = fallbackOptions.wallMaterials;

        if (floorMaterials == null || floorMaterials.Length == 0)
            floorMaterials = fallbackOptions.floorMaterials;

        if (slabMaterials == null || slabMaterials.Length == 0)
            slabMaterials = fallbackOptions.slabMaterials;

        if (groundMaterials == null || groundMaterials.Length == 0)
            groundMaterials = fallbackOptions.groundMaterials;

        if (foundationOptions == null)
            foundationOptions = fallbackOptions.foundationOptions;
        else
            foundationOptions.FillEmptyFieldsWithTheseValues(fallbackOptions.foundationOptions);
    }
}
