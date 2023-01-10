using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab }

    public City city;
    public FoundationType foundationType = FoundationType.NoFoundations;

    public List<FoundationJSON> foundations;
    public List<Collider> foundationColliders;
    public Material actualSlabMaterial, actualGroundMaterial;
    public Material slabMaterial, groundMaterial;

    public FoundationManager(City city)
    {
        this.city = city;
        actualGroundMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Ground Material");
        actualSlabMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Slab Material");
    }

    public void GenerateNewFoundations()
    {
        if(foundationType == FoundationType.SingularSlab)
            GenerateNewSingleSlabFoundation();
    }

    private void GenerateNewSingleSlabFoundation()
    {
        //Update the slab and ground materials
        float scaling = city.radius / 10.0f;
        God.CopyMaterialValues(slabMaterial, actualSlabMaterial, scaling, scaling, true);
        God.CopyMaterialValues(groundMaterial, actualGroundMaterial, scaling, scaling, true);

        //---

        //Take notes so we can save and restore the foundation later
        FoundationJSON foundation = new FoundationJSON();

        //Customize the foundation
        if (city.circularCity)
            foundation.prefab = "Planet/City/Miscellaneous/Circular Foundation";
        else
            foundation.prefab = "Planet/City/Miscellaneous/Rectangular Foundation";

        //Instantiate and place the foundation using the customization. This will also add it to the lists it needs to be in.
        foundation.localPosition = Vector3.zero;
        foundation.localScale = Vector3.one * city.radius * 2.1f;
        foundation.GenerateFoundation(city);

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        Physics.SyncTransforms();
    }
}

[System.Serializable]
public class FoundationManagerJSON
{
    //Materials
    public string slabMaterial, groundMaterial;
    public Vector2 slabMaterialTiling, groundMaterialTiling;

    //Foundations
    public List<FoundationJSON> foundations;

    public FoundationManagerJSON(FoundationManager foundationManager)
    {
        slabMaterial = foundationManager.slabMaterial.name;
        slabMaterialTiling = foundationManager.actualSlabMaterial.mainTextureScale;

        groundMaterial = foundationManager.groundMaterial.name;
        groundMaterialTiling = foundationManager.actualGroundMaterial.mainTextureScale;

        foundations = foundationManager.foundations;
    }

    public void RestoreFoundationManager(FoundationManager foundationManager)
    {
        foundationManager.slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMaterial);
        God.CopyMaterialValues(foundationManager.slabMaterial, foundationManager.actualSlabMaterial, slabMaterialTiling.x, slabMaterialTiling.y, false);

        foundationManager.groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMaterial);
        God.CopyMaterialValues(foundationManager.groundMaterial, foundationManager.actualGroundMaterial, groundMaterialTiling.x, groundMaterialTiling.y, false);

        foreach (FoundationJSON foundation in foundations)
            foundation.GenerateFoundation(foundationManager.city);
        Physics.SyncTransforms();
    }
}
