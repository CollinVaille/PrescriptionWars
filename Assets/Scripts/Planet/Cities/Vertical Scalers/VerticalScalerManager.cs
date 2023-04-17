using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalScalerManager
{
    public City city;
    public List<VerticalScaler> verticalScalers;

    public VerticalScalerManager(City city)
    {
        this.city = city;
        verticalScalers = new List<VerticalScaler>();
    }

    public VerticalScaler InstantiateVerticalScaler(string resourcePath, Transform parent)
    {
        //Instantiate it
        GameObject verticalScalerPrefab = Resources.Load<GameObject>(resourcePath);
        VerticalScaler verticalScaler = GameObject.Instantiate(verticalScalerPrefab, parent).GetComponent<VerticalScaler>();

        //Configure it (not all configuration is done here. only configuration that needs to be done the same way in every situation)
        verticalScaler.name = verticalScaler.name.Substring(0, verticalScaler.name.Length - 7);
        verticalScaler.resourcePath = resourcePath;

        //Add it to the save system
        if (verticalScalers == null)
            verticalScalers = new List<VerticalScaler>();
        verticalScalers.Add(verticalScaler);

        //Return it
        return verticalScaler;
    }

    public void GenerateVerticalScalerBesideFoundation(Vector3 foundationPosition, Vector3 foundationScale, bool generateOnNegativeSide, int yAxisRotation)
    {
        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = InstantiateVerticalScaler(city.cityType.GetVerticalScaler(false), city.transform);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        verticalScaler.SetYAxisRotation(yAxisRotation);

        //Position it
        verticalScalerTransform.localPosition = foundationPosition;
        Vector3 translation = verticalScalerTransform.right * ((foundationScale.x / 2.0f) + (verticalScaler.width / 2.0f));
        if (generateOnNegativeSide)
            translation *= -1;
        verticalScalerTransform.Translate(translation, Space.World);
        verticalScalerTransform.Translate(verticalScalerTransform.forward * (verticalScaler.width / 2.0f), Space.World);

        //Scale it and connect it to the entrance foundation
        verticalScaler.ScaleToHeightAndConnect(foundationScale.y * 0.5f, !generateOnNegativeSide);
    }

    public void GenerateVerticalScalerWithFocalPoint(Vector3 focalPoint, Vector3 edgePoint, float globalBottomLevel, float globalTopLevel, bool minor)
    {
        if (Mathf.Abs(globalTopLevel - globalBottomLevel) < 0.2f)
            return;

        //Adjust parameters
        globalBottomLevel += 0.05f;
        edgePoint.y = globalBottomLevel;
        focalPoint.y = globalBottomLevel;

        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = InstantiateVerticalScaler(city.cityType.GetVerticalScaler(minor), city.transform);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        if (minor)
            verticalScaler.SetYAxisRotation((int)(Quaternion.LookRotation(edgePoint - focalPoint).eulerAngles.y), inLocalSpace: false);
        else
            verticalScaler.SetYAxisRotation((int)(Quaternion.LookRotation(focalPoint - edgePoint).eulerAngles.y + 90.0f), inLocalSpace: false);

        //Position
        Vector3 scalerPosition;
        if (minor)
            scalerPosition = edgePoint;
        else
            scalerPosition = Vector3.MoveTowards(edgePoint, focalPoint, -(verticalScaler.width / 2.0f) - 1.0f);
        verticalScaler.transform.position = scalerPosition;

        //Scale it and connect it to the entrance foundation
        bool platformToLeft = verticalScaler.transform.InverseTransformPoint(focalPoint).x < 0.0f;
        verticalScaler.ScaleToHeightAndConnect(globalTopLevel - globalBottomLevel, platformToLeft);
    }

    public void GenerateVerticalScalersOnRandomEdgeOfFoundation(int howMany, Foundation foundation, float globalBottomLevel, float globalTopLevel)
    {
        List<Vector3> previousPlacesInGlobal = new List<Vector3>();
        for (int x = 0; x < howMany; x++)
            GenerateVerticalScalerOnRandomEdgeOfFoundation(foundation, globalBottomLevel, globalTopLevel, previousPlacesInGlobal);
    }

    public void GenerateVerticalScalerOnRandomEdgeOfFoundation(Foundation foundation, float globalBottomLevel, float globalTopLevel, List<Vector3> avoidThesePlacesInGlobal = null)
    {
        //Get a random point outside the city. Make an effort (but no guarantee) for the point to not be close to certain locations if specified.
        Vector3 randomOutsidePointInGlobal = city.GetRandomGlobalPointOutsideOfCity();
        if(avoidThesePlacesInGlobal != null)
        {
            float distanceThreshold = 200.0f;
            for (int attempt = 1; TooCloseToPreviousXZPositions(randomOutsidePointInGlobal, avoidThesePlacesInGlobal, distanceThreshold - attempt) && attempt <= 200; attempt++)
                randomOutsidePointInGlobal = city.GetRandomGlobalPointOutsideOfCity();

            //Now that we have committed to placing a vertical scaler at this point, let's agree not to try this location again later
            avoidThesePlacesInGlobal.Add(randomOutsidePointInGlobal);
        }

        //Find the closest edge point the foundation has to it
        Vector3 topEdgePoint = foundation.GetClosestTopBoundaryPoint(randomOutsidePointInGlobal);

        //Place the vertical scaler at that edge point
        bool useMinorVerticalScaler = (globalTopLevel - globalBottomLevel) < 15.0f;
        GenerateVerticalScalerWithFocalPoint(foundation.transform.position, topEdgePoint, globalBottomLevel, globalTopLevel, useMinorVerticalScaler);
    }

    public static bool TooCloseToPreviousXZPositions(Vector3 potentialPosition, List<Vector3> previousScalerPositions, float distanceThreshold)
    {
        //See if any of the positions in the list are too close to the potential position
        foreach (Vector3 previousPosition in previousScalerPositions)
        {
            //Make y-values the same because we don't want to compare y-values
            Vector3 previousXZPosition = previousPosition;
            previousXZPosition.y = potentialPosition.y;

            //Too close?
            if (Vector3.Distance(potentialPosition, previousXZPosition) < distanceThreshold)
                return true;
        }

        return false;
    }
}

[System.Serializable]
public class VerticalScalerManagerJSON
{
    //Vertical scalers
    public List<VerticalScalerJSON> verticalScalers;

    public VerticalScalerManagerJSON(VerticalScalerManager verticalScalerManager)
    {
        verticalScalers = new List<VerticalScalerJSON>();
        foreach (VerticalScaler verticalScaler in verticalScalerManager.verticalScalers)
            verticalScalers.Add(new VerticalScalerJSON(verticalScaler));
    }

    public void RestoreVerticalScalerManager(VerticalScalerManager verticalScalerManager)
    {
        foreach (VerticalScalerJSON verticalScaler in verticalScalers)
            verticalScaler.RestoreVerticalScaler(verticalScalerManager);
    }
}
