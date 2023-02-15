using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab, PerBuilding, Islands, Atlantis }

    //All torus foundations have their inner radius = this multiplier * their outer radius.
    //...Inner radius of ring model: 0.3825, outer radius of ring model: 0.5... 0.3825/0.5 = 0.765
    public const float torusAnnulusMultiplier = 0.765f;

    public City city;
    public FoundationType foundationType = FoundationType.Islands;
    public int foundationHeight = 0;
    public FoundationSelections foundationSelections;

    public Material largeSlabMaterial, largeGroundMaterial;
    public Material slabMaterial, groundMaterial;
    public List<FoundationJSON> foundations;
    public List<Collider> foundationColliders;
    public List<VerticalScaler> verticalScalers;

    //Main entry points---

    public FoundationManager(City city)
    {
        this.city = city;
        largeGroundMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Ground Material");
        largeSlabMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Slab Material");
    }

    public void DetermineFoundationPlans()
    {
        //The foundation height has been set by CityGenerator. Now, we need to see if the height should be tweaked and what foundation type we should go for...

        //If the city is underwater, raise it above water with foundations
        if (Planet.planet.hasOcean)
        {
            float heightDifference = (Planet.planet.oceanTransform.position.y + 2.0f) - city.transform.position.y;
            if (heightDifference > 0.0f)
            {
                city.newCitySpecifications.lowerBuildingsMustHaveFoundations = true;

                float heightDifferenceWithFoundations = heightDifference - foundationHeight;
                if (heightDifferenceWithFoundations > 0.0f)
                    foundationHeight += Mathf.CeilToInt(heightDifferenceWithFoundations);
            }
        }

        //Either finalize decision to skip foundations...
        if (foundationHeight < 0)
            foundationType = FoundationType.NoFoundations;

        //Or, commence with choosing a foundation...
        else
        {
            //Enforce minimum non-zero foundation height (super short foundations don't play well with our elevator and bridge systems)
            if (foundationHeight < 10)
                foundationHeight = 10;

            //Choose a random foundation type
            if (foundationType == FoundationType.NoFoundations)
                foundationType = GetRandomNonNullFoundationType();
        }

        //Enforce required conditions for certain foundation types
        if (foundationType == FoundationType.Atlantis)
            city.circularCity = true;
    }

    //The idea here is that certain foundation types create wasted space in the city that cannot be used for buildings.
    //Since we're trying be able to control the "size" of a city regardless of foundation type, it becomes necessary to expand the size...
    //...of the city (by adjusting city.radius) to compensate for wasted space. These calculations are made below based on square feet so that it scales well.
    public void AdjustCityRadiusToCompensateForFoundationPlans()
    {
        float totalSquareMetersForCity = AreaManager.CalculateAreaFromDimensions(city.circularCity, city.radius);
        float usablePercentage = GetEstimatedUsableSquareMeterPercentageForCity();
        float newSquareMetersForCity = totalSquareMetersForCity / usablePercentage;
        float newRadiusForCity = AreaManager.CalculateHalfLengthFromArea(city.circularCity, newSquareMetersForCity);

        //Debug.Log("from " + ((int)city.radius) + " to " + ((int)newRadiusForCity) + " with usage percent " + usablePercentage);
        city.radius = (int)newRadiusForCity;
    }

    public void GenerateNewFoundations()
    {
        if (foundationType == FoundationType.NoFoundations)
            return;

        foundationSelections = new FoundationSelections(CityGenerator.generator.foundationOptions);

        //---
        ProceedWithCreatingFoundationBasedOnType();
        //---

        //Update the slab and ground materials
        float scaling = city.radius / 10.0f;
        God.CopyMaterialValues(slabMaterial, largeSlabMaterial, scaling, foundationHeight / 20.0f, true);
        God.CopyMaterialValues(groundMaterial, largeGroundMaterial, scaling, scaling, true);
    }

    private void ProceedWithCreatingFoundationBasedOnType()
    {
        if (foundationType == FoundationType.SingularSlab)
            GenerateNewSingleSlabFoundation();
        else if (foundationType == FoundationType.PerBuilding)
            GenerateNewPerBuildingFoundations();
        else if (foundationType == FoundationType.Islands)
            GenerateNewIslandFoundations();
        else if (foundationType == FoundationType.Atlantis)
            GenerateNewAtlantisFoundations();
    }

    //Foundation type-specific generation logic---

    private void GenerateNewSingleSlabFoundation()
    {
        Vector3 foundationScale = Vector3.one * city.radius * 2.15f;
        foundationScale.y = foundationHeight;
        GenerateNewFoundation(Vector3.zero, foundationScale, city.circularCity ? FoundationShape.Circular : FoundationShape.Rectangular, false);

        GenerateEntrancesForCardinalDirections(false);
    }

    private void GenerateNewPerBuildingFoundations()
    {
        //Still need entrances
        GenerateEntrancesForCardinalDirections(true);

        //Ensure no walls are generated
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = false;

        //Tell building construction to give extra radius around the buildings so we can walk around them
        city.newCitySpecifications.extraUsedBuildingRadius = 15;

        //We may also want extra spacing between the foundations for aesthetics, variation, and also more space for bridges
        if(Random.Range(0, 2) == 0)
            city.newCitySpecifications.extraBuildingRadiusForSpacing = Random.Range(0, 10);

        //Tell building construction to create foundations underneath each building at this height range
        //city.buildingSpecifications.foundationHeightRange = Vector2.one * foundationHeight;
        city.newCitySpecifications.buildingFoundationHeightRange = new Vector2(Random.Range(0.9f, 1.0f), Random.Range(1.0f, 1.1f)) * foundationHeight;
    }

    private void GenerateNewIslandFoundations()
    {
        FoundationGeneratorForIslands generator = new FoundationGeneratorForIslands(this);
        generator.GenerateNewIslandFoundations();
    }

    private void GenerateNewAtlantisFoundations()
    {
        FoundationGeneratorForAtlantis generator = new FoundationGeneratorForAtlantis(this);
        generator.GenerateNewAtlantisFoundations();
    }

    //Helper methods---

    public void GenerateEntrancesForCardinalDirections(bool entrancesNeedConnecting, float extraDistanceFromCityCenter = 0.0f)
    {
        FoundationShape foundationShape = (Random.Range(0, 2) == 0) ? FoundationShape.Circular : FoundationShape.Rectangular;
        float distanceFromCityCenter = (city.radius * 1.075f) + extraDistanceFromCityCenter;

        //Prepare for X gates
        city.areaManager.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
        Vector3 foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;
        
        //-X gate
        Vector3 foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, -distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, foundationShape, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 180);

        //+X gate
        foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, foundationShape, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 0);

        //Prepare for Z gates
        city.areaManager.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
        foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //-Z gate
        foundationPosition = new Vector3(-distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, foundationShape, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, -90);

        //+Z gate
        foundationPosition = new Vector3(distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, foundationShape, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 90);
    }

    public void GenerateVerticalScalerBesideFoundation(Vector3 foundationPosition, Vector3 foundationScale, bool generateOnNegativeSide, int yAxisRotation)
    {
        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler(city.cityType.GetVerticalScaler(false), city.transform, this);
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
        verticalScaler.ScaleToHeightAndConnect(foundationHeight / 2.0f, !generateOnNegativeSide);
    }

    public void GenerateVerticalScalerWithFocalPoint(Vector3 focalPoint, Vector3 edgePoint, float globalBottomLevel, float globalTopLevel, bool minor)
    {
        //Adjust parameters
        globalBottomLevel += 0.05f;
        edgePoint.y = globalBottomLevel;
        focalPoint.y = globalBottomLevel;

        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler(city.cityType.GetVerticalScaler(minor), city.transform, this);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        if(minor)
            verticalScaler.SetYAxisRotation((int)(Quaternion.LookRotation(edgePoint - focalPoint).eulerAngles.y), inLocalSpace: false);
        else
            verticalScaler.SetYAxisRotation((int)(Quaternion.LookRotation(focalPoint - edgePoint).eulerAngles.y + 90.0f), inLocalSpace: false);

        //Position
        Vector3 scalerPosition;
        if(minor)
            scalerPosition = edgePoint;
        else
            scalerPosition = Vector3.MoveTowards(edgePoint, focalPoint, -(verticalScaler.width / 2.0f) - 1.0f);
        verticalScaler.transform.position = scalerPosition;

        //Scale it and connect it to the entrance foundation
        bool platformToLeft = verticalScaler.transform.InverseTransformPoint(focalPoint).x < 0.0f;
        verticalScaler.ScaleToHeightAndConnect(globalTopLevel - globalBottomLevel, platformToLeft);
    }

    public void GenerateVerticalScalerByFoundation(Vector3 foundationPosition, Collider[] foundationColliders, float foundationRadius, float globalBottomLevel, float globalTopLevel)
    {

    }

    //This function gives the foundation manager the chance to generate a foundation underneath the building before it is created.
    public FoundationJSON RightBeforeBuildingGenerated(int radiusOfBuilding, bool hasCardinalRotation, Vector3 buildingPosition)
    {
        //Determine whether building should have a foundation generated underneath it
        float buildingFoundationHeight = city.newCitySpecifications.GetRandomBuildingFoundationHeight();

        if (buildingFoundationHeight > 5) //Include foundation
        {
            //Place foundation
            Vector3 foundationScale = Vector3.one * radiusOfBuilding;
            foundationScale.y = buildingFoundationHeight;
            FoundationShape foundationShape = (!hasCardinalRotation || Random.Range(0, 2) == 0) ? FoundationShape.Circular : FoundationShape.Rectangular;
            FoundationJSON foundationJSON = GenerateNewFoundation(buildingPosition, foundationScale, foundationShape, BuildingFoundationNeedsConnecting());

            return foundationJSON;
        }
        else //Skip foundation
            return null;
    }

    public FoundationJSON GenerateNewFoundation(Vector3 localPosition, Vector3 localScale, FoundationShape shape, bool needsConnecting)
    {
        //Take notes so we can save and restore the foundation later
        FoundationJSON foundationJSON = new FoundationJSON();

        //Load in the customization
        foundationJSON.prefab = foundationSelections.GetFoundationPrefab(shape, localScale);
        foundationJSON.localPosition = localPosition;
        foundationJSON.localScale = localScale;

        //Instantiate and place the foundation using the customization. This will also add it to the lists it needs to be in.
        foundationJSON.GenerateFoundation(city);

        //If requested, tell the bridge system this foundation needs connecting with the rest of the city
        if(needsConnecting)
        {
            BridgeDestination bridgeDestination;
            Vector3 bridgeDestinationPosition = city.transform.TransformPoint(localPosition);
            bridgeDestinationPosition.y += (localScale.y / 2.0f);
            if (shape == FoundationShape.Circular || shape == FoundationShape.Torus) //This treats toruses as convex shapes. To connect a torus to a destination inside it, you'll have to manually create the BridgeDestinationPairing
                bridgeDestination = new BridgeDestination(bridgeDestinationPosition, localScale.x / 2.0f);
            else
            {
                Collider[] slabColliders = foundationJSON.transform.Find("Slab").Find("Ground Collider").GetComponentsInChildren<Collider>();
                bridgeDestination = new BridgeDestination(bridgeDestinationPosition, slabColliders);
            }
            city.bridgeManager.AddNewDestination(bridgeDestination);
        }

        return foundationJSON;
    }

    private bool BuildingFoundationNeedsConnecting()
    {
        return foundationType == FoundationType.PerBuilding;
    }

    private static FoundationType GetRandomNonNullFoundationType()
    {
        int selection = Random.Range(0, 4);
        switch(selection)
        {
            case 0: return FoundationType.SingularSlab;
            case 1: return FoundationType.PerBuilding;
            case 2: return FoundationType.Islands;
            default: return FoundationType.Atlantis;
        }
    }

    private float GetEstimatedUsableSquareMeterPercentageForCity()
    {
        switch(foundationType)
        {
            case FoundationType.NoFoundations: return 1.0f;
            case FoundationType.SingularSlab: return 1.0f;
            case FoundationType.PerBuilding: return 0.65f;
            case FoundationType.Islands: return 0.8f;
            case FoundationType.Atlantis: return 0.55f;
            default: return 1.0f;
        }
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

    //Vertical scalers
    public List<VerticalScalerJSON> verticalScalers;

    public FoundationManagerJSON(FoundationManager foundationManager)
    {
        slabMaterial = foundationManager.slabMaterial.name;
        slabMaterialTiling = foundationManager.largeSlabMaterial.mainTextureScale;

        groundMaterial = foundationManager.groundMaterial.name;
        groundMaterialTiling = foundationManager.largeGroundMaterial.mainTextureScale;

        foundations = foundationManager.foundations;

        verticalScalers = new List<VerticalScalerJSON>();
        foreach (VerticalScaler verticalScaler in foundationManager.verticalScalers)
            verticalScalers.Add(new VerticalScalerJSON(verticalScaler));
    }

    public void RestoreFoundationManager(FoundationManager foundationManager)
    {
        foundationManager.slabMaterial = Resources.Load<Material>("Planet/City/Materials/" + slabMaterial);
        God.CopyMaterialValues(foundationManager.slabMaterial, foundationManager.largeSlabMaterial, slabMaterialTiling.x, slabMaterialTiling.y, false);

        foundationManager.groundMaterial = Resources.Load<Material>("Planet/City/Materials/" + groundMaterial);
        God.CopyMaterialValues(foundationManager.groundMaterial, foundationManager.largeGroundMaterial, groundMaterialTiling.x, groundMaterialTiling.y, false);

        foreach (FoundationJSON foundation in foundations)
            foundation.GenerateFoundation(foundationManager.city);

        foreach (VerticalScalerJSON verticalScaler in verticalScalers)
            verticalScaler.RestoreVerticalScaler(foundationManager);

        Physics.SyncTransforms();
    }
}
