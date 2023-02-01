using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public enum BridgeFillMode { StretchingOrTilingOK, AvoidStretching, AvoidTiling }

    public Material slabMaterial, groundMaterial;
    public BridgeFillMode fillMode = BridgeFillMode.StretchingOrTilingOK;

    public void RepaintRecursive(Transform t, Material newSlabMaterial, Material newGroundMaterial)
    {
        foreach (Transform child in t)
        {
            MeshRenderer theRenderer = child.GetComponent<MeshRenderer>();
            if (theRenderer && theRenderer.sharedMaterial == slabMaterial)
                theRenderer.sharedMaterial = newSlabMaterial;
            else if (theRenderer && theRenderer.sharedMaterial == groundMaterial)
            {
                theRenderer.sharedMaterial = newGroundMaterial;
                PlanetMaterial.SetMaterialTypeBasedOnName(newGroundMaterial.name, child.gameObject);
            }

            if (child.childCount > 0)
                RepaintRecursive(child, newSlabMaterial, newGroundMaterial);
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
}