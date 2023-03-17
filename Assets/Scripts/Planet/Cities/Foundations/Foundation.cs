using System.Collections.Generic;
using UnityEngine;

public class Foundation : MonoBehaviour
{
    public FoundationShape foundationShape;

    [HideInInspector] public FoundationManager foundationManager;
    [HideInInspector] public string prefab;
    [HideInInspector] public Vector3 localScale;

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
        slab.localScale = localScale;

        //Scale the x, z of the ground
        Vector3 groundScale = ground.localScale.x * localScale;
        groundScale.y = ground.localScale.y;
        ground.localScale = groundScale;

        //Adjust the y position of the ground
        Vector3 groundPosition = ground.localPosition;
        groundPosition.y = groundPosition.y * localScale.y;
        ground.localPosition = groundPosition;

        //Make sure there is no clipping (use world space instead of local space so we know the coordinates won't get skewed)
        ground.position += Vector3.up * 0.005f;
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

    //Returns the closest point in global space on the edge of the foundation with a y value equal of that of the top of the foundation.
    public Vector3 GetClosestTopBoundaryPoint(Vector3 referencePointInGlobal)
    {
        //Determine the fixed y value (will always be top of foundation in global space)
        float topHeight = transform.position.y + localScale.y * 0.5f;

        //Adjust reference point to have fixed y value
        Vector3 yAdjustedReferencePoint = referencePointInGlobal;
        yAdjustedReferencePoint.y = topHeight;

        //Get center with fixed y value
        Vector3 yAdjustedCenter = transform.position;
        yAdjustedCenter.y = topHeight;
        Vector3 closestBoundaryPointInGlobal;

        //Finally, determine the closest boundary point...

        //Determine boundary point based on radius and height
        if(foundationShape == FoundationShape.Circular || foundationShape == FoundationShape.Torus)
            closestBoundaryPointInGlobal = Vector3.MoveTowards(yAdjustedCenter, yAdjustedReferencePoint, localScale.x * 0.5f);

        //Determine boundary point based on ground colliders and height
        else
        {
            Collider[] groundColliders = transform.Find("Slab").Find("Ground Colliders").GetComponentsInChildren<Collider>();
            closestBoundaryPointInGlobal = GetClosestPointAmongstColliders(yAdjustedReferencePoint, groundColliders, true);
        }

        return closestBoundaryPointInGlobal;
    }

    public static Vector3 GetClosestPointAmongstColliders(Vector3 referencePointInGlobal, Collider[] collidersToCheck, bool forceColliderUpdate)
    {
        if(forceColliderUpdate)
            Physics.SyncTransforms();

        Vector3 closestPointInGlobal = collidersToCheck[0].ClosestPoint(referencePointInGlobal);
        float shortestDistance = Vector3.Distance(closestPointInGlobal, referencePointInGlobal);

        for (int x = 1; x < collidersToCheck.Length; x++)
        {
            Vector3 closestPointOnThisCollider = collidersToCheck[x].ClosestPoint(referencePointInGlobal);
            float shortestDistanceOnThisCollider = Vector3.Distance(closestPointOnThisCollider, referencePointInGlobal);
            if (shortestDistanceOnThisCollider < shortestDistance)
            {
                closestPointInGlobal = closestPointOnThisCollider;
                shortestDistance = shortestDistanceOnThisCollider;
            }
        }

        return closestPointInGlobal;
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