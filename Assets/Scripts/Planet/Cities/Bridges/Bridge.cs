using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public enum BridgeFillMode { StretchingOrTilingOK, AvoidStretching, AvoidTiling, SpecialConnector }

    public Material slabMaterial, groundMaterial;
    public BridgeFillMode fillMode = BridgeFillMode.StretchingOrTilingOK;
    [HideInInspector] public int prefabIndex;
    [HideInInspector] public bool usingSubstructure = false, usingRoof = false;

    public void SetUpBridge(int prefabIndex, Vector3 position, Vector3 rotation, float zScale, BridgeManager bridgeManager, bool useSubstructure, bool useRoof)
    {
        //Remember needed variables
        this.prefabIndex = prefabIndex;

        //Fit into scene
        SetUpTransform(position, rotation, zScale, bridgeManager.city.transform);
        RepaintRecursive(transform, bridgeManager.slabMaterial, bridgeManager.groundMaterial);
        SetUpSubstructure(useSubstructure);
        SetUpRoof(useRoof);

        //Add to save system
        if (bridgeManager.bridges == null)
            bridgeManager.bridges = new List<Bridge>();
        bridgeManager.bridges.Add(this);
    }

    private void SetUpTransform(Vector3 position, Vector3 rotation, float zScale, Transform parent)
    {
        Transform bridgeTransform = transform;

        //Parent it
        bridgeTransform.parent = parent;

        //Position
        bridgeTransform.position = position;

        //Scale
        Vector3 bridgeScale = bridgeTransform.localScale;
        bridgeScale.z = zScale;
        bridgeTransform.localScale = bridgeScale;

        //Rotate
        bridgeTransform.eulerAngles = rotation;
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
                usingSubstructure = true;

                //Gather needed variables
                float topY = transform.position.y;
                float bottomY = hit.point.y;
                float yDiff = topY - bottomY;

                //Will use this at the end, but need to get the information now
                Transform tileablePart = substructure.Find("Tile This Downwards");
                float previousSubstructureYScale = substructure.localScale.y;

                //Unparent from bridge to prevent its scale and rotation from being not messed up
                //New parent is the bridge's parent because that guy doesn't have those issues (need parent with vector3.one scaling and vector3.zero rotation)
                substructure.parent = transform.parent;

                //Scale
                Vector3 substructureScale = substructure.localScale;
                substructureScale.y = yDiff + 4.0f;
                substructure.localScale = substructureScale;

                //Position (setting y in global space)
                Vector3 substructurePosition = substructure.position;
                substructurePosition.y = (topY - (yDiff * 0.5f)) - 3.5f;
                substructure.position = substructurePosition;

                //Position continued (Fixing x,z in local space)
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

                //Tile substructure if needed
                if (tileablePart)
                    VerticallyTileSubstructure(tileablePart, substructure, previousSubstructureYScale, hit.point, topY);
            }
            else
                RemoveSubstructure(substructure);
        }
        else
            RemoveSubstructure(substructure);
    }

    private void RemoveSubstructure(Transform substructure) { Destroy(substructure.gameObject); }

    private void VerticallyTileSubstructure(Transform tileablePart, Transform substructure, float originalSubstructureYScale, Vector3 bottomPosition, float topY)
    {
        int tileCount = Mathf.Max(1, (int)((topY - bottomPosition.y) / originalSubstructureYScale));
        float yInterval = (topY - bottomPosition.y) / tileCount;

        //Temporarily make the tileable part parentless so that when we replicate it we don't get scaling and positioning issues
        tileablePart.parent = null;

        //Spawn the tiles bottom to top
        for(int x = 1; x <= tileCount; x++)
        {
            //Create the tile
            Transform newTile = (x == tileCount) ? tileablePart : Instantiate(tileablePart.gameObject).transform;

            //Parent the tile
            newTile.parent = substructure;

            //Position the tile
            Vector3 position = bottomPosition;
            position.y = bottomPosition.y + x * yInterval - 3.5f;
            newTile.position = position;

            //Rotate the tile
            newTile.localEulerAngles = Vector3.zero;

            //Scale the tile
            float yScale = originalSubstructureYScale / substructure.localScale.y;
            newTile.localScale = new Vector3(1.0f, yScale, 1.0f);
        }
    }

    private void SetUpRoof(bool useRoof)
    {
        usingRoof = useRoof;

        if (useRoof)
            return;

        Transform optionalRoof = transform.Find("Superstructure").Find("Optional Roof");

        if (optionalRoof)
            Destroy(optionalRoof.gameObject);
    }

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
            PlanetMaterial.SetMaterialTypeBasedOnNameNOTRecursive(newGroundMaterial.name, theRenderer.gameObject);
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
}

[System.Serializable]
public class BridgeJSON
{
    public int prefabIndex;
    public Vector3 position, rotation;
    public float zScale;
    public bool useSubstructure, useRoof;

    public BridgeJSON(Bridge bridge, BridgeManager bridgeManager)
    {
        prefabIndex = bridge.prefabIndex;

        position = bridge.transform.position;
        rotation = bridge.transform.eulerAngles;
        zScale = bridge.transform.localScale.z;

        useSubstructure = bridge.usingSubstructure;
        useRoof = bridge.usingRoof;
    }

    public void RestoreBridge(BridgeManager bridgeManager)
    {
        Bridge bridge = bridgeManager.InstantiateBridge(prefabIndex);
        bridge.SetUpBridge(prefabIndex, position, rotation, zScale, bridgeManager, useSubstructure, useRoof);
    }
}