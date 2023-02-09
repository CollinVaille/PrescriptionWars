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

        GenerateEntrancesForCardinalDirections();
    }

    private void GenerateNewPerBuildingFoundations()
    {
        //Still need entrances
        GenerateEntrancesForCardinalDirections();

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
        AreaManager areaManager = city.areaManager;

        //Still need entrances
        GenerateEntrancesForCardinalDirections();

        //Ensure no walls are generated
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = (Random.Range(0, 2) == 0);

        //Determine how much square feet we want to try to take up with foundations. This will determine how dense the city is
        float totalSquareMetersForCity = AreaManager.CalculateAreaFromDimensions(city.circularCity, city.radius);
        float squareMetersWeAreTryingToClaim = Mathf.Max(totalSquareMetersForCity * Random.Range(0.2f, 0.8f), Mathf.Min(20000, totalSquareMetersForCity));
        float squareMetersClaimedSoFar = 0.0f;

        //Start with buildings being allowed to generate nowhere
        areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.LackingRequiredFoundation, AreaManager.AreaReservationType.Open);

        //Then, add foundations one by one where buildings can spawn until we hit our square footage goal or run out of attempts
        for(int attempt = 1; attempt <= 400 && squareMetersClaimedSoFar < squareMetersWeAreTryingToClaim; attempt++)
        {
            //Randomly choose a new location and scale for a foundation
            Vector2Int center = new Vector2Int(Random.Range(0, areaManager.areaTaken.GetLength(0)), Random.Range(0, areaManager.areaTaken.GetLength(1)));
            int areasLong = Random.Range(80, 230) / areaManager.areaSize;
            Vector2Int outerAreaStart = new Vector2Int(center.x - areasLong / 2, center.y - areasLong / 2);

            //See if it fits
            if (!areaManager.SafeToGenerate(outerAreaStart.x, outerAreaStart.y, areasLong, AreaManager.AreaReservationType.LackingRequiredFoundation, true))
                continue;

            //If it does, then go ahead with adding the foundation...

            //Create some needed variables upfront
            bool circularFoundation = (Random.Range(0, 2) == 0);
            int squareMetersLong = areasLong * areaManager.areaSize;
            bool generateWalls = squareMetersLong > 120;

            //Tell the area reservation system that we are claiming this chunk to be taken up by this foundation
            areaManager.ReserveAreasWithType(outerAreaStart.x, outerAreaStart.y, areasLong, AreaManager.AreaReservationType.ReservedForExtraPerimeter, AreaManager.AreaReservationType.LackingRequiredFoundation);

            //Next, create a smaller concentric subpocket where buildings can spawn inside the larger area we just reserved
            //We'll call the area in between the two radii where the walls spawn the buffer zone
            int bufferInAreas = 3;
            if (generateWalls)
                bufferInAreas *= 2;

            if (circularFoundation)
            {
                int innerCircleRadius = (areasLong / 2) - bufferInAreas;
                areaManager.ReserveAreasWithinThisCircle(center.x, center.y, innerCircleRadius, AreaManager.AreaReservationType.Open, false, AreaManager.AreaReservationType.ReservedForExtraPerimeter);
            }
            else
            {
                Vector2Int innerAreaStart = new Vector2Int(outerAreaStart.x + bufferInAreas, outerAreaStart.y + bufferInAreas);
                areaManager.ReserveAreasWithType(innerAreaStart.x, innerAreaStart.y, areasLong - 2 * bufferInAreas, AreaManager.AreaReservationType.Open, AreaManager.AreaReservationType.ReservedForExtraPerimeter);
            }

            //Remember how much area we just reserved
            squareMetersClaimedSoFar += AreaManager.CalculateAreaFromDimensions(circularFoundation, squareMetersLong * 0.5f);

            //Calculate the parameters needed to place the foundation
            Vector3 foundationLocalPosition = areaManager.AreaCoordToLocalCoord(new Vector3(center.x, 0.0f, center.y));
            Vector3 foundationScale = Vector3.one * squareMetersLong;
            foundationScale.y = foundationHeight * Random.Range(0.9f, 1.1f);

            //Place the foundation and hook it up with bridges
            GenerateNewFoundation(foundationLocalPosition, foundationScale, circularFoundation, true);

            //Create walls that line the edges of the foundation
            if(generateWalls)
            {
                NewCityWallRequest newCityWallRequest = new NewCityWallRequest();
                newCityWallRequest.circular = circularFoundation;
                newCityWallRequest.localCenter = foundationLocalPosition;
                newCityWallRequest.halfLength = (squareMetersLong - (bufferInAreas * areaManager.areaSize)) * 0.5f;
                city.cityWallManager.newCityWallRequests.Add(newCityWallRequest);
            }

            //Add the building subpocket to the city block system so that special and/or large buildings know to try to generate in the middle...
            //...of these locations first before trying random placement
            int innerRadiusInMeters = squareMetersLong - bufferInAreas * areaManager.areaSize;
            city.areaManager.availableCityBlocks.Add(new CityBlock(center, Vector2Int.one * innerRadiusInMeters));
        }

        if(Random.Range(0, 1) == 0)
            areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.Open, AreaManager.AreaReservationType.LackingRequiredFoundation);
    }

    //Helper methods---

    private void GenerateEntrancesForCardinalDirections()
    {
        bool circularEntrances = Random.Range(0, 2) == 0;
        bool entrancesNeedConnecting = EntranceFoundationNeedsConnecting();
        float distanceFromCityCenter = city.radius * 1.075f;

        //Prepare for X gates
        city.areaManager.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
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
        city.areaManager.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
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
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler(city.cityType.GetVerticalScaler(false), city.transform, this);
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
        return foundationType == FoundationType.PerBuilding || foundationType == FoundationType.Islands;
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
