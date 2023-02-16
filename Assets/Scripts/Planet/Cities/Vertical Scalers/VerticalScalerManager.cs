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
        verticalScaler.ScaleToHeightAndConnect(city.foundationManager.foundationHeight / 2.0f, !generateOnNegativeSide);
    }

    public void GenerateVerticalScalerWithFocalPoint(Vector3 focalPoint, Vector3 edgePoint, float globalBottomLevel, float globalTopLevel, bool minor)
    {
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

    public void GenerateVerticalScalerOnRandomEdgeOfFoundation(Foundation foundation, float globalBottomLevel, float globalTopLevel)
    {
        //Get a random point outside the city
        Vector3 randomOutsidePointInGlobal = city.GetRandomGlobalPointOutsideOfCity();

        //Find the closest edge point the foundation has to it
        Vector3 topEdgePoint = foundation.GetClosestTopBoundaryPoint(randomOutsidePointInGlobal);

        //Place the vertical scaler at that edge point
        bool useMinorVerticalScaler = (globalTopLevel - globalBottomLevel) < 15.0f;
        GenerateVerticalScalerWithFocalPoint(foundation.transform.position, topEdgePoint, globalBottomLevel, globalTopLevel, useMinorVerticalScaler);
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
