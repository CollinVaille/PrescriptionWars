using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab, PerBuilding, Islands }

    public City city;
    public FoundationType foundationType = FoundationType.NoFoundations;
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
    }

    //Foundation type-specific generation logic---

    private void GenerateNewSingleSlabFoundation()
    {
        Vector3 foundationScale = Vector3.one * city.radius * 2.15f;
        foundationScale.y = foundationHeight;
        GenerateNewFoundation(Vector3.zero, foundationScale, city.circularCity, false);

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
        city.newCitySpecifications.foundationHeightRange = new Vector2(Random.Range(0.9f, 1.0f), Random.Range(1.0f, 1.1f)) * foundationHeight;
    }

    private void GenerateNewIslandFoundations()
    {
        FoundationGeneratorForIslands generator = new FoundationGeneratorForIslands(this);
        generator.GenerateNewIslandFoundations();
    }

    //Helper methods---

    public void GenerateEntrancesForCardinalDirections(bool entrancesNeedConnecting)
    {
        bool circularEntrances = Random.Range(0, 2) == 0;
        float distanceFromCityCenter = city.radius * 1.075f;

        //Prepare for X gates
        city.areaManager.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
        Vector3 foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;
        
        //-X gate
        Vector3 foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, -distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 180);

        //+X gate
        foundationPosition = new Vector3(widestRoadCenteredAt, 0.0f, distanceFromCityCenter);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 0);

        //Prepare for Z gates
        city.areaManager.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
        foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //-Z gate
        foundationPosition = new Vector3(-distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
        GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, -90);

        //+Z gate
        foundationPosition = new Vector3(distanceFromCityCenter, 0.0f, widestRoadCenteredAt);
        GenerateNewFoundation(foundationPosition, foundationScale, circularEntrances, entrancesNeedConnecting);
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

    public void GenerateVerticalScalerBesideFoundationCollider(Vector3 foundationCenter, Vector3 closestPointOnFoundation, float globalBottomLevel, float globalTopLevel)
    {
        //Adjust parameters
        globalBottomLevel += 0.05f;
        closestPointOnFoundation.y = globalBottomLevel;
        foundationCenter.y = globalBottomLevel;

        //Instantiate the vertical scaler
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler(city.cityType.GetVerticalScaler(false), city.transform, this);
        Transform verticalScalerTransform = verticalScaler.transform;

        //Rotate it
        verticalScaler.SetYAxisRotation((int)(Quaternion.LookRotation(foundationCenter - closestPointOnFoundation).eulerAngles.y + 90.0f), inLocalSpace: false);

        //Position
        Vector3 scalerPosition = Vector3.MoveTowards(closestPointOnFoundation, foundationCenter, -(verticalScaler.width / 2.0f) - 1.0f);
        verticalScaler.transform.position = scalerPosition;

        //Scale it and connect it to the entrance foundation
        bool platformToLeft = verticalScaler.transform.InverseTransformPoint(foundationCenter).x < 0.0f;
        verticalScaler.ScaleToHeightAndConnect(globalTopLevel - globalBottomLevel, platformToLeft);
    }

    //This function gives the foundation manager the chance to generate a foundation underneath the building before it is created.
    public FoundationJSON RightBeforeBuildingGenerated(int radiusOfBuilding, bool hasCardinalRotation, Vector3 buildingPosition)
    {
        //Determine whether building should have a foundation generated underneath it
        float buildingFoundationHeight = city.newCitySpecifications.GetRandomFoundationHeight();

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

    public FoundationJSON GenerateNewFoundation(Vector3 localPosition, Vector3 localScale, bool circular, bool needsConnecting)
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
