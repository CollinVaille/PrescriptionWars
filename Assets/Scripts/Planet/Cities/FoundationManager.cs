using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab, PerBuilding }

    public City city;
    public FoundationType foundationType = FoundationType.NoFoundations;
    public int foundationHeight = 0;
    public FoundationSelections foundationSelections;

    public Material largeSlabMaterial, largeGroundMaterial;
    public Material slabMaterial, groundMaterial;
    public List<FoundationJSON> foundations;
    public List<Collider> foundationColliders;
    public List<VerticalScaler> verticalScalers;

    public FoundationManager(City city)
    {
        this.city = city;
        largeGroundMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Ground Material");
        largeSlabMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Slab Material");
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
    }

    private void GenerateNewSingleSlabFoundation()
    {
        Vector3 foundationScale = Vector3.one * city.radius * 2.15f;
        foundationScale.y = foundationHeight;
        GenerateNewFoundation(Vector3.zero, foundationScale, city.circularCity, false);

        GenerateEntrancesForCardinalDirections();
    }

    private void GenerateNewPerBuildingFoundations()
    {
        //Still need entrances
        GenerateEntrancesForCardinalDirections();

        //Ensure no walls are generated
        city.cityWallManager.wallSectionPrefab = null;

        //Tell building construction to give extra radius around the buildings so we can walk around them
        city.buildingSpecifications.extraBuildingRadius = 15;
        if(Random.Range(0, 2) == 0)
            city.buildingSpecifications.extraRadiusForSpacing = Random.Range(0, 10);

        //Tell building construction to create foundations underneath each building at this height range
        city.buildingSpecifications.foundationHeightRange = new Vector2(Random.Range(0.9f, 1.0f), Random.Range(1.0f, 1.1f)) * foundationHeight;
    }

    private void GenerateEntrancesForCardinalDirections()
    {
        bool circularEntrances = Random.Range(0, 2) == 0;
        bool entrancesNeedConnecting = EntranceFoundationNeedsConnecting();
        float distanceFromCityCenter = city.radius * 1.075f;

        //Prepare for X gates
        city.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
        Vector3 foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;
        
        //-X gate
        Vector3 foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, -distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 180);

        //+X gate
        foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 0);

        //Prepare for Z gates
        city.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
        foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //-Z gate
        foundationPosition = new Vector3(-distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, -90);

        //+Z gate
        foundationPosition = new Vector3(distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideEntrance(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 90);
    }

    private void GenerateVerticalScalerBesideEntrance(Vector3 entrancePosition, Vector3 entranceScale, bool generateOnNegativeSide, int yAxisRotation)
    {
        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler("Planet/City/Miscellaneous/Elevator", city.transform, this);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        verticalScaler.SetYAxisRotation(yAxisRotation);

        //Position it
        verticalScalerTransform.localPosition = entrancePosition;
        Vector3 translation = verticalScalerTransform.right * ((entranceScale.x / 2.0f) + (verticalScaler.width / 2.0f));
        if (generateOnNegativeSide)
            translation *= -1;
        verticalScalerTransform.Translate(translation, Space.World);
        verticalScalerTransform.Translate(verticalScalerTransform.forward * (verticalScaler.width / 2.0f), Space.World);

        //Scale it and connect it to the entrance foundation
        verticalScaler.ScaleToHeightAndConnect(foundationHeight / 2.0f, !generateOnNegativeSide);
    }

    //This function gives the foundation manager the chance to generate a foundation underneath the building before it is created.
    public FoundationJSON RightBeforeBuildingGenerated(int radiusOfBuilding, bool hasCardinalRotation, Vector3 buildingPosition)
    {
        //Determine whether building should have a foundation generated underneath it
        float buildingFoundationHeight = Random.Range(city.buildingSpecifications.foundationHeightRange.x, city.buildingSpecifications.foundationHeightRange.y);

        if (buildingFoundationHeight > 5) //Include foundation
        {
            //Place foundation
            Vector3 foundationScale = Vector3.one * radiusOfBuilding;
            foundationScale.y = buildingFoundationHeight;
            bool circularFoundation = !hasCardinalRotation || Random.Range(0, 2) == 0;
            FoundationJSON foundationJSON = GenerateNewFoundation(buildingPosition, foundationScale, circularFoundation, BuildingFoundationNeedsConnecting());

            return foundationJSON;
        }
        else //Skip foundation
            return null;
    }

    private FoundationJSON GenerateNewFoundation(Vector3 localPosition, Vector3 localScale, bool circular, bool needsConnecting)
    {
        //Take notes so we can save and restore the foundation later
        FoundationJSON foundationJSON = new FoundationJSON();

        //Load in the customization
        foundationJSON.prefab = foundationSelections.GetFoundationPrefab(circular, localScale);
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
            if (circular)
                bridgeDestination = new BridgeDestination(bridgeDestinationPosition, localScale.x / 2.0f);
            else
            {
                Collider[] slabColliders = foundationJSON.transform.Find("Slab").Find("Ground Collider").GetComponents<Collider>();
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

    private bool EntranceFoundationNeedsConnecting()
    {
        return foundationType == FoundationType.PerBuilding;
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
