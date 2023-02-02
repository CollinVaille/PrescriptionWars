using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public enum BridgeFillMode { StretchingOrTilingOK, AvoidStretching, AvoidTiling, SpecialConnector }

    public Material slabMaterial, groundMaterial;
    public BridgeFillMode fillMode = BridgeFillMode.StretchingOrTilingOK;

    public void SetUpBridge(bool useSubstructure, Material newSlabMaterial, Material newGroundMaterial)
    {
        RepaintRecursive(transform, newSlabMaterial, newGroundMaterial);
        SetUpSubstructure(useSubstructure);
    }

    private void SetUpSubstructure(bool useSubstructure)
    {
        Transform substructure = transform.Find("Substructure");

        if (!substructure)
            return;

        if (useSubstructure)
        {
            if (Physics.Raycast(substructure.position + Vector3.down * 1.0f, Vector3.down, out RaycastHit hit))
            {
                //Unparent from bridge so that its scale and rotation are not messed
                //New parent is the bridge's parent because that guy doesn't have those issues
                substructure.parent = transform.parent;

                //Scale
                Vector3 substructureScale = substructure.localScale;
                substructureScale.y = ((substructure.position.y - hit.point.y) * 0.5f) + 1.0f;
                substructure.localScale = substructureScale;

                //Position (setting y in global space)
                Vector3 substructurePosition = substructure.position;
                substructurePosition.y = hit.point.y + substructureScale.y * 0.5f;
                substructure.position = substructurePosition;

                //Position  continued (Fixing x,z in local space)
                substructurePosition = substructure.localPosition;
                //substructurePosition.x = 0.0f;
                //substructurePosition.z = 0.0f;
                substructure.localPosition = substructurePosition;

                //Rotation
                Vector3 substructureRotation = substructure.eulerAngles;
                substructureRotation.x = 0.0f;
                substructureRotation.z = 0.0f;
                substructure.eulerAngles = substructureRotation;

                //Unhide substructure
                substructure.gameObject.SetActive(true);
            }
            else
                RemoveSubstructure(substructure);
        }
        else
            RemoveSubstructure(substructure);
    }

    private void RemoveSubstructure(Transform substructure) { Destroy(substructure.gameObject); }

    private void RepaintRecursive(Transform t, Material newSlabMaterial, Material newGroundMaterial)
    {
        MeshRenderer theRenderer = t.GetComponent<MeshRenderer>();
        if (theRenderer)
            RepaintSingleRenderer(theRenderer, newSlabMaterial, newGroundMaterial);

        foreach (Transform child in t)
            RepaintRecursive(child, newSlabMaterial, newGroundMaterial);
    }

    private void RepaintSingleRenderer(MeshRenderer theRenderer, Material newSlabMaterial, Material newGroundMaterial)
    {
        if (theRenderer.sharedMaterial == slabMaterial)
            theRenderer.sharedMaterial = newSlabMaterial;
        else if (theRenderer.sharedMaterial == groundMaterial)
        {
            theRenderer.sharedMaterial = newGroundMaterial;
            PlanetMaterial.SetMaterialTypeBasedOnName(newGroundMaterial.name, theRenderer.gameObject);
        }
    }

    public bool ShouldStretch(float gapLength, float defaultBridgeLength)
    {
        if (fillMode == BridgeFillMode.AvoidTiling)
            return true;
        else if (fillMode == BridgeFillMode.AvoidStretching)
            return false;
        else
            return (gapLength < defaultBridgeLength * 2) || (Random.Range(0, 2) == 0);
    }

    public Vector3 GetCorner(bool negativeX, bool negativeZ)
    {
        Vector3 corner = new Vector3(0.5f, 0.0f, 0.5f);

        if (negativeX)
            corner.x = -corner.x;

        if (negativeZ)
            corner.z = -corner.z;

        return transform.TransformPoint(corner);
    }

    public static Vector3 GetClosestPointAmongstColliders(Vector3 inRelationTo, Collider[] boundaryColliders)
    {
        Vector3 closestPoint = boundaryColliders[0].ClosestPoint(inRelationTo);
        float shortestDistance = Vector3.Distance(closestPoint, inRelationTo);

        for (int x = 1; x < boundaryColliders.Length; x++)
        {
            Vector3 closestPointOnThisCollider = boundaryColliders[x].ClosestPoint(inRelationTo);
            float shortestDistanceOnThisCollider = Vector3.Distance(closestPointOnThisCollider, inRelationTo);
            if (shortestDistanceOnThisCollider < shortestDistance)
            {
                closestPoint = closestPointOnThisCollider;
                shortestDistance = shortestDistanceOnThisCollider;
            }
        }

        return closestPoint;
    }
}