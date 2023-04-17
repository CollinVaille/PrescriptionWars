using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationManager
{
    public enum FoundationType { NoFoundations, SingularSlab, PerBuilding, Islands, Atlantis, Hammocks, Pyramid }

    //All torus foundations have their inner radius = this multiplier * their outer radius.
    //...Inner radius of ring model = 0.3825, outer radius of ring model = 0.5... 0.3825/0.5 = 0.765
    public const float torusAnnulusMultiplier = 0.765f; //Do not touch unless you know what you're doing

    public City city;
    public FoundationType foundationType = FoundationType.NoFoundations;
    public int foundationHeight = 0;
    public bool nothingBelowFoundationHeight = true;
    public FoundationSelections foundationSelections;

    public Material largeSlabMaterial, largeGroundMaterial;
    public List<Foundation> foundations;
    public List<Collider> foundationGroundColliders; //List of all colliders for the city that belong to a foundation and are used as the foundation's "top ground".

    //Main entry points---

    public FoundationManager(City city)
    {
        this.city = city;

        largeGroundMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Ground Material");
        largeSlabMaterial = Resources.Load<Material>("Planet/City/Miscellaneous/Large Slab Material");

        foundations = new List<Foundation>();
        foundationGroundColliders = new List<Collider>();
    }

    public void DetermineFoundationPlans()
    {
        //The foundation height has been set by CityGenerator. Now, we need to see if the height should be tweaked and what foundation type we should go for...

        //If the city is underwater, raise it above water with foundations
        if (Planet.planet.hasAnyKindOfOcean)
        {
            float heightDifference = (Planet.planet.oceanTransform.position.y + 2.0f) - city.transform.position.y;
            if (heightDifference > 0.0f)
            {
                city.newCitySpecifications.lowerBuildingsMustHaveFoundations = true;

                float heightDifferenceWithFoundations = (heightDifference * 2.0f) - foundationHeight;
                if (heightDifferenceWithFoundations > 0.0f)
                    foundationHeight += Mathf.CeilToInt(heightDifferenceWithFoundations);
            }
        }

        //Either finalize decision to skip foundations...
        if (foundationHeight <= 0)
        {
            foundationHeight = 0;
            foundationType = FoundationType.NoFoundations;
        }

        //Or, commence with choosing a foundation...
        else
        {
            //Enforce minimum non-zero foundation height (super short foundations don't play well with our elevator and bridge systems)
            if (foundationHeight < 10)
                foundationHeight = 10;

            //Choose a random foundation type
            if (foundationType == FoundationType.NoFoundations || city.newCitySpecifications.smallCompound)
            {
                if (city.newCitySpecifications.smallCompound)
                    foundationType = FoundationType.SingularSlab;
                else
                    foundationType = GetRandomNonNullFoundationType();
            }
        }

        //Enforce required conditions for certain foundation types
        if (foundationType == FoundationType.Islands)
            nothingBelowFoundationHeight = city.newCitySpecifications.lowerBuildingsMustHaveFoundations;
        else if (foundationType == FoundationType.Atlantis)
        {
            city.circularCity = true;
            nothingBelowFoundationHeight = Random.Range(0, 2) == 0;
        }
        else if (foundationType == FoundationType.Hammocks)
        {
            city.circularCity = false;
            nothingBelowFoundationHeight = true;
            city.foundationManager.foundationHeight += Random.Range(60, 80); //Hammocks need to be high up in the air
        }
    }

    //The idea here is that certain foundation types create wasted space in the city that cannot be used for buildings.
    //Since we're trying be able to control the "size" of a city regardless of foundation type, it becomes necessary to expand the size...
    //...of the city (by adjusting city.radius) to compensate for wasted space. These calculations are made below based on square feet so that it scales well.
    public void AdjustCityRadiusToCompensateForFoundationPlans()
    {
        float totalSquareMetersForCity = AreaManager.CalculateAreaFromDimensions(city.circularCity, city.radius);
        float usablePercentage = GetEstimatedUsableSquareMeterPercentageForCity();
        float newSquareMetersForCity = totalSquareMetersForCity / usablePercentage;

        float minimumSquareMeters = GetMinimumSquareMetersForFoundationType(foundationType);
        if (newSquareMetersForCity < minimumSquareMeters)
            newSquareMetersForCity = minimumSquareMeters;

        float newRadiusForCity = AreaManager.CalculateHalfLengthFromArea(city.circularCity, newSquareMetersForCity);
        //Debug.Log(newSquareMetersForCity);

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
        God.CopyMaterialValues(Planet.planet.planetWideCityCustomization.slabMaterial, largeSlabMaterial, scaling, foundationHeight / 20.0f, true);
        God.CopyMaterialValues(Planet.planet.planetWideCityCustomization.groundMaterial, largeGroundMaterial, scaling, scaling, true);
    }

    private void ProceedWithCreatingFoundationBasedOnType()
    {
        switch(foundationType)
        {
            case FoundationType.PerBuilding: GenerateNewPerBuildingFoundations(); return;
            case FoundationType.Islands: (new FoundationGeneratorForIslands(this)).GenerateNewIslandFoundations(); return;
            case FoundationType.Atlantis: (new FoundationGeneratorForAtlantis(this)).GenerateNewAtlantisFoundations(); return;
            case FoundationType.Hammocks: (new FoundationGeneratorForHammocks(this)).GenerateNewHammockFoundations(); return;
            case FoundationType.Pyramid: (new FoundationGeneratorForPyramid(this)).GenerateNewPyramidFoundations(); return;
            default: GenerateNewSingleSlabFoundation(); return;
        }
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
        city.newCitySpecifications.buildingFoundationHeightRange = new Vector2(Random.Range(1.0f, 1.1f), Random.Range(1.1f, 1.2f)) * foundationHeight;
    }

    //Helper methods---

    public void GenerateEntrancesForCardinalDirections(bool entrancesNeedConnecting, float extraDistanceFromCityCenter = 0.0f, float overrideDistanceFromCityCenter = 0.0f, float foundationLocalY = 0.0f, bool reserveArea = false, bool forceEntranceToCenter = false)
    {
        FoundationShape foundationShape = (Random.Range(0, 2) == 0) ? FoundationShape.Circular : FoundationShape.Rectangular;
        float distanceFromCityCenter = overrideDistanceFromCityCenter > 1.0f ? overrideDistanceFromCityCenter : (city.radius * 1.075f) + extraDistanceFromCityCenter;

        bool needToFlattenTerrainByEntrances = city.terrainModifications != TerrainReservationOptions.TerrainResModType.Flatten && foundationLocalY < 1.0f;

        //Prepare for X gates
        city.areaManager.GetWidestCardinalRoad(true, !city.circularCity, out float widestRoadWidth, out float widestRoadCenteredAt);
        if (forceEntranceToCenter)
            widestRoadCenteredAt = 0.0f;
        Vector3 foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //Generate X gates
        GenerateEntranceForCardinalDirections(widestRoadCenteredAt, distanceFromCityCenter, foundationScale, foundationShape, entrancesNeedConnecting, true, true, needToFlattenTerrainByEntrances, foundationLocalY, reserveArea);
        GenerateEntranceForCardinalDirections(widestRoadCenteredAt, distanceFromCityCenter, foundationScale, foundationShape, entrancesNeedConnecting, true, false, needToFlattenTerrainByEntrances, foundationLocalY, reserveArea);        

        //Prepare for Z gates
        city.areaManager.GetWidestCardinalRoad(false, !city.circularCity, out widestRoadWidth, out widestRoadCenteredAt);
        if (forceEntranceToCenter)
            widestRoadCenteredAt = 0.0f;
        foundationScale = Vector3.one * Mathf.Clamp(widestRoadWidth * 2.0f, 20.0f, 30.0f);
        foundationScale.y = foundationHeight - 0.05f;

        //Generate Z gates
        GenerateEntranceForCardinalDirections(widestRoadCenteredAt, distanceFromCityCenter, foundationScale, foundationShape, entrancesNeedConnecting, false, true, needToFlattenTerrainByEntrances, foundationLocalY, reserveArea);
        GenerateEntranceForCardinalDirections(widestRoadCenteredAt, distanceFromCityCenter, foundationScale, foundationShape, entrancesNeedConnecting, false, false, needToFlattenTerrainByEntrances, foundationLocalY, reserveArea);
        
    }

    private void GenerateEntranceForCardinalDirections(float widestRoadCenteredAt, float distanceFromCityCenter, Vector3 foundationScale, FoundationShape foundationShape, bool entrancesNeedConnecting, bool xAxis, bool negative, bool flatten, float foundationLocalY, bool reserveArea)
    {
        Vector3 foundationPosition;
        if(xAxis)
        {
            if(negative)
                foundationPosition = new Vector3(widestRoadCenteredAt, foundationLocalY, -distanceFromCityCenter);
            else
                foundationPosition = new Vector3(widestRoadCenteredAt, foundationLocalY, distanceFromCityCenter);
        }
        else //Z-axis
        {
            if (negative)
                foundationPosition = new Vector3(-distanceFromCityCenter, foundationLocalY, widestRoadCenteredAt);
            else
                foundationPosition = new Vector3(distanceFromCityCenter, foundationLocalY, widestRoadCenteredAt);
        }

        if(flatten)
        {
            float previousLocalYPosition = foundationPosition.y;

            foundationPosition = city.transform.TransformPoint(foundationPosition);
            foundationPosition = PlanetTerrain.planetTerrain.SnapToTerrainAndFlattenAreaAroundPoint(foundationPosition.x, foundationPosition.z);
            foundationPosition = city.transform.InverseTransformPoint(foundationPosition);

            float yChange = foundationPosition.y - previousLocalYPosition;
            foundationScale.y -= yChange * 2.0f; //need to multiply it by 2 because half of the foundation is underground
        }

        if (foundationScale.y < 0.5f)
            return;

        GenerateNewFoundation(foundationPosition, foundationScale, foundationShape, entrancesNeedConnecting);

        if (xAxis)
        {
            if (negative)
                city.verticalScalerManager.GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 180);
            else
                city.verticalScalerManager.GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 0);
        }
        else //Z-axis
        {
            if (negative)
                city.verticalScalerManager.GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, -90);
            else
                city.verticalScalerManager.GenerateVerticalScalerBesideFoundation(foundationPosition, foundationScale, widestRoadCenteredAt > city.radius, 90);
        }

        //Optionally, reserve the area around the entrance as being off-limits for buildings (optional b/c if its just used like normal the entrance is outside the city limits anyway)
        if(reserveArea)
        {
            Vector3 placeInAreas = city.areaManager.LocalCoordToAreaCoord(foundationPosition);
            int radiusInAreas = 35 / city.areaManager.areaSize;
            city.areaManager.ReserveAreasWithinThisCircle((int)placeInAreas.x, (int)placeInAreas.z, radiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);
        }   
    }

    //This function gives the foundation manager the chance to generate a foundation underneath the building before it is created.
    public Foundation RightBeforeBuildingGenerated(int radiusOfBuilding, bool hasCardinalRotation, Vector3 buildingPosition)
    {
        //Determine whether building should have a foundation generated underneath it
        float buildingFoundationHeight = city.newCitySpecifications.GetRandomBuildingFoundationHeight();

        //Proceed with creating foundation if applicable
        if (buildingFoundationHeight > 5) //Include foundation
        {
            //Place foundation
            Vector3 foundationScale = Vector3.one * radiusOfBuilding;
            foundationScale.y = buildingFoundationHeight;
            FoundationShape foundationShape = (!hasCardinalRotation || Random.Range(0, 2) == 0) ? FoundationShape.Circular : FoundationShape.Rectangular;
            Foundation foundation = GenerateNewFoundation(buildingPosition, foundationScale, foundationShape, BuildingFoundationNeedsConnecting());

            return foundation;
        }
        else //Skip foundation
            return null;
    }

    public Foundation GenerateNewFoundation(Vector3 localPosition, Vector3 localScale, FoundationShape shape, bool needsConnecting)
    {
        //Create the new foundation
        string foundationPrefab = foundationSelections.GetFoundationPrefab(shape, localScale);
        Foundation newFoundation = GenerateNewOrRestoreFoundation(foundationPrefab, localPosition, localScale);

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
                Collider[] slabColliders = newFoundation.transform.Find("Slab").Find("Ground Colliders").GetComponentsInChildren<Collider>();
                bridgeDestination = new BridgeDestination(bridgeDestinationPosition, slabColliders);
            }
            city.bridgeManager.AddNewDestination(bridgeDestination);
        }

        return newFoundation;
    }

    public Foundation GenerateNewOrRestoreFoundation(string prefab, Vector3 localPosition, Vector3 localScale)
    {
        //Instantiate and parent the foundation
        Foundation foundation = GameObject.Instantiate(Resources.Load<GameObject>("Planet/City/Foundations/" + prefab)).GetComponent<Foundation>();
        foundation.transform.parent = city.transform;

        //let the instantianted object perform the rest of its generation
        foundation.GenerateNewOrRestoreFoundation(this, prefab, localPosition, localScale);

        return foundation;
    }

    private bool BuildingFoundationNeedsConnecting()
    {
        return foundationType == FoundationType.PerBuilding;
    }

    private static FoundationType GetRandomNonNullFoundationType()
    {
        int selection = Random.Range(0, 6);
        switch(selection)
        {
            case 0: return FoundationType.PerBuilding;
            case 1: return FoundationType.Islands;
            case 2: return FoundationType.Atlantis;
            case 3: return FoundationType.Hammocks;
            case 4: return FoundationType.Pyramid;
            default: return FoundationType.SingularSlab;
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
            case FoundationType.Hammocks: return 0.35f;
            case FoundationType.Pyramid: return 0.75f;
            default: return 1.0f;
        }
    }

    public float GetEstimatedUsableSquareMetersForCity()
    {
        float totalSquareMetersForCity = AreaManager.CalculateAreaFromDimensions(city.circularCity, city.radius);
        float usablePercentage = GetEstimatedUsableSquareMeterPercentageForCity();
        return totalSquareMetersForCity * usablePercentage;
    }

    private static float GetMinimumSquareMetersForFoundationType(FoundationType foundationType)
    {
        switch(foundationType)
        {
            case FoundationType.Hammocks:
                return 400000.0f;
            case FoundationType.Pyramid:
                return 100000.0f;
            default:
                return 50000.0f;
        }
    }

    //Can be called on new planet or restored planet
    public TerrainReservationOptions.TerrainResModType GetTerrainModificationTypeForCity()
    {
        switch(foundationType)
        {
            case FoundationType.Hammocks:
            case FoundationType.PerBuilding:
            case FoundationType.Atlantis:
                if(nothingBelowFoundationHeight)
                    return TerrainReservationOptions.TerrainResModType.FlattenEdgesCeilMiddle;
                else
                    return TerrainReservationOptions.TerrainResModType.Flatten;
            default:
                return TerrainReservationOptions.TerrainResModType.Flatten;
        }
    }

    public Foundation GetClosestFoundationAndPoint(Vector3 referencePointInGlobal, out Vector3 closestPointInGlobal, bool ignoreYWhenSearching)
    {
        //Start with the first foundation being the closest
        Foundation closestFoundation = foundations[0];
        closestPointInGlobal = foundations[0].GetClosestTopBoundaryPoint(referencePointInGlobal);
        float shortestDistance = God.GetDistanceBetween(closestPointInGlobal, referencePointInGlobal, ignoreYWhenSearching);

        //See if any of the other foundations can give a closer point
        for(int x = 1; x < foundations.Count; x++)
        {
            //Get data for the next foundation
            Foundation foundation = foundations[x];
            Vector3 foundationClosestPoint = foundation.GetClosestTopBoundaryPoint(referencePointInGlobal);
            float foundationShortestDistance = God.GetDistanceBetween(foundationClosestPoint, referencePointInGlobal, ignoreYWhenSearching);

            //If the distance is the best yet, write that shit down
            if(foundationShortestDistance < shortestDistance)
            {
                shortestDistance = foundationShortestDistance;
                closestPointInGlobal = foundationClosestPoint;
                closestFoundation = foundation;
            }
        }

        //Return what we found
        return closestFoundation;
    }
}

[System.Serializable]
public class FoundationManagerJSON
{
    //General
    FoundationManager.FoundationType foundationType;

    //Materials
    public Vector2 slabMaterialTiling, groundMaterialTiling;

    //Foundations
    public List<FoundationJSON> foundationJSONs;

    public FoundationManagerJSON(FoundationManager foundationManager)
    {
        foundationType = foundationManager.foundationType;

        slabMaterialTiling = foundationManager.largeSlabMaterial.mainTextureScale;
        groundMaterialTiling = foundationManager.largeGroundMaterial.mainTextureScale;

        foundationJSONs = new List<FoundationJSON>(foundationManager.foundations.Count);
        foreach (Foundation foundation in foundationManager.foundations)
            foundationJSONs.Add(new FoundationJSON(foundation));
    }

    public void RestoreFoundationManager(FoundationManager foundationManager)
    {
        foundationManager.foundationType = foundationType;

        God.CopyMaterialValues(Planet.planet.planetWideCityCustomization.slabMaterial, foundationManager.largeSlabMaterial, slabMaterialTiling.x, slabMaterialTiling.y, false);

        God.CopyMaterialValues(Planet.planet.planetWideCityCustomization.groundMaterial, foundationManager.largeGroundMaterial, groundMaterialTiling.x, groundMaterialTiling.y, false);

        foreach (FoundationJSON foundationJSON in foundationJSONs)
            foundationJSON.RestoreFoundation(foundationManager);

        Physics.SyncTransforms();
    }
}
