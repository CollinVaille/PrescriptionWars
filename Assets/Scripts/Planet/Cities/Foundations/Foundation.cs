using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foundation : MonoBehaviour
{
    public FoundationManager foundationManager;
    public string prefab;
    public Vector3 localScale;

    public void GenerateNewOrRestoreFoundation(FoundationManager foundationManager, string prefab, Vector3 localPosition, Vector3 localScale)
    {
        //Save passed in data
        this.foundationManager = foundationManager;
        this.prefab = prefab;
        this.localScale = localScale;

        //Then perform the rest of the initialization/generation...

        //Get needed variables
        Transform slab = transform.Find("Slab");
        Transform ground = transform.Find("Ground");

        //Apply customization to foundation
        transform.localPosition = localPosition;
        SetScale(slab, ground);
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        SetRenderMaterialsIfNeeded(slab, ground, foundationManager);
        SetPlanetMaterialForGround(slab, foundationManager);

        //Add foundation colliders to the foundation collider system (used for things like snapping buildings to the nearest surface)
        if (foundationManager.foundationGroundColliders == null)
            foundationManager.foundationGroundColliders = new List<Collider>();
        foundationManager.foundationGroundColliders.AddRange(slab.Find("Ground Colliders").GetComponentsInChildren<Collider>());

        //Add foundation to saving system
        if (foundationManager.foundations == null)
            foundationManager.foundations = new List<Foundation>();
        foundationManager.foundations.Add(this);
    }

    private void SetScale(Transform slab, Transform ground)
    {
        //Scale the x, y, z of the slab
        slab.localScale = localScale - Vector3.up * 0.025f;

        //Scale the x, z of the ground
        Vector3 groundScale = ground.localScale.x * localScale;
        groundScale.y = ground.localScale.y;
        ground.localScale = groundScale;

        //Adjust the y position of the ground
        Vector3 groundPosition = ground.localPosition;
        groundPosition.y = groundPosition.y * localScale.y - 0.005f;
        ground.localPosition = groundPosition;
    }

    private void SetRenderMaterialsIfNeeded(Transform slab, Transform ground, FoundationManager foundationManager)
    {
        //Large scale sections of the foundation use a large scale version of the ground and slab material.
        //That large scale version is already placed on them and programatically scaled elsewhere in the program.
        //However, for smaller scale sections of foundation we directly set the renderer's shared material to that of the reference material.
        //This allows the smaller scale sections of the foundation to not have extra tiling while the large scale sections do.

        if (slab.localScale.x < 100.0f)
        {
            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer)
                groundRenderer.sharedMaterial = foundationManager.groundMaterial;
        }

    }

    private void SetPlanetMaterialForGround(Transform slab, FoundationManager foundationManager)
    {
        PlanetMaterial.SetMaterialTypeBasedOnNameRecursive(foundationManager.groundMaterial.name, slab.Find("Ground Colliders"));
    }
}


[System.Serializable]
public class FoundationJSON
{
    public string prefab;
    public Vector3 localPosition;
    public Vector3 localScale;

    public FoundationJSON(Foundation foundation)
    {
        prefab = foundation.prefab;
        localPosition = foundation.transform.localPosition;
        localScale = foundation.localScale;
    }

    public void RestoreFoundation(FoundationManager foundationManager)
    {
        foundationManager.GenerateNewOrRestoreFoundation(prefab, localPosition, localScale);
    }
}