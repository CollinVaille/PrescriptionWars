using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlanetLODManager
{
    public static void RegisterRendererLODsForChildren(Transform parent, float LODScaleMultiplier)
    {
        LODGroup lodGroup = parent.gameObject.AddComponent<LODGroup>();

        Renderer[] interiorAndDetailRenderers = GetRenderersFromChildren(parent.Find("Interior And Details"));
        Renderer[] exteriorRenderers = GetRenderersFromChildren(parent.Find("Exterior"));
        Transform primitiveLODParent = parent.Find("Primitive LOD");
        Renderer[] primitiveLODRenderers = GetRenderersFromChildren(primitiveLODParent);

        float exteriorOnlyThreshold = 0.5f, primitiveOnlyThreshold = 0.2f, culledThreshold = 0.01f;

        //0 - Interior & Details + Exterior (everything), 1 - Exterior only, 2 - primitive LOD only
        bool hasPrimitives = !IsNullOrEmpty(primitiveLODRenderers);
        LOD[] lods = new LOD[hasPrimitives ? 3 : 2];

        Renderer[] lod0Renderers = MergeRendererArrays(interiorAndDetailRenderers, exteriorRenderers);
        if (IsNullOrEmpty(lod0Renderers))
            lod0Renderers = GetRenderersFromChildren(parent);

        lods[0] = new LOD(exteriorOnlyThreshold, lod0Renderers);
        lods[1] = new LOD(hasPrimitives ? primitiveOnlyThreshold : culledThreshold, exteriorRenderers);
        if (hasPrimitives)
        {
            lods[2] = new LOD(culledThreshold, primitiveLODRenderers);
            primitiveLODParent.gameObject.SetActive(true);
        }

        lodGroup.SetLODs(lods);
        lodGroup.size *= LODScaleMultiplier;
    }

    private static Renderer[] GetRenderersFromChildren(Transform parent)
    {
        if (parent)
            return parent.GetComponentsInChildren<Renderer>();
        else
            return null;
    }

    private static Renderer[] MergeRendererArrays(Renderer[] array1, Renderer[] array2)
    {
        if (IsNullOrEmpty(array1))
            return array2;
        else if (IsNullOrEmpty(array2))
            return array1;
        else
        {
            Renderer[] array1And2Combined = new Renderer[array1.Length + array2.Length];
            array1.CopyTo(array1And2Combined, 0);
            array2.CopyTo(array1And2Combined, array1.Length);
            return array1And2Combined;
        }
    }

    private static bool IsNullOrEmpty(Renderer[] arrayToCheck)
    {
        return arrayToCheck == null || arrayToCheck.Length == 0;
    }
}
