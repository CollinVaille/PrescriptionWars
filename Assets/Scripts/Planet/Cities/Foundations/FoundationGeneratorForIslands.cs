using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForIslands
{
    private City city;
    private FoundationManager foundationManager;

    //Main access functions---

    public FoundationGeneratorForIslands(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;
    }

    //Generates a new set of foundations that each can house multiple buildings on them (or none at all). The foundations are connected by bridges.
    //These foundations are considered "islands" and I call them "level 2" foundations.
    //There is a possible singular lower elevation "level 1" foundation that these foundations can sit on that spans the entire area of the city.
    //Buildings can spawn on both level 1 and level 2, making for a multi-leveled city.
    public void GenerateNewIslandFoundations()
    {
        AreaManager areaManager = city.areaManager;

        //Determine some customization we will use later on
        bool allFoundationsAreSameShape = (Random.Range(0, 2) == 0);
        float torusChanceForCirculars = Random.Range(0.0f, 1.0f);
        float chanceForLevel3 = (Random.Range(0, 1) == 0) ? 1.0f : 0.0f;
        Vector2Int level2WidthRange = Get2ndLevelFoundationWidthRange(city.radius);

        //Determine level 1 and level 2 heights (level 1 is at the feet of the island foundations, level 2 is the top of the island foundations)
        bool level1IsElevated = (Random.Range(0, 2) == 0) || city.newCitySpecifications.lowerBuildingsMustHaveFoundations;
        float level1Height;
        Vector2 level2HeightRange = (new Vector2(Random.Range(0.8f, 1.0f), Random.Range(1.0f, 1.2f))) * foundationManager.foundationHeight;

        if (level1IsElevated) //Level 1 is elevated (making level 2 even higher)
            level1Height = foundationManager.foundationHeight;
        else //Level is flush with ground level (not elevated)
            level1Height = 0.0f;

        //Random chance to generate walls on city perimeter
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = level1IsElevated ? (Random.Range(0, 3) != 0) : (Random.Range(0, 2) == 0);

        //Determine how much square feet we want to try to take up with foundations. This will determine how dense the city is
        float totalSquareMetersForCity = AreaManager.CalculateAreaFromDimensions(city.circularCity, city.radius);
        float squareMetersWeAreTryingToClaim = Mathf.Max(totalSquareMetersForCity * Random.Range(0.2f, 0.8f), Mathf.Min(20000, totalSquareMetersForCity));
        float squareMetersClaimedSoFar = 0.0f;

        //Start with buildings being allowed to generate nowhere
        areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.LackingRequiredFoundation, AreaManager.AreaReservationType.Open);

        //Then, add the level 2 foundations one by one where buildings can spawn until we hit our square footage goal or run out of attempts
        for (int attempt = 1; attempt <= 400 && squareMetersClaimedSoFar < squareMetersWeAreTryingToClaim; attempt++)
        {
            //Randomly choose a new location and scale for a foundation
            Vector2Int centerInAreas = new Vector2Int(Random.Range(0, areaManager.areaTaken.GetLength(0)), Random.Range(0, areaManager.areaTaken.GetLength(1)));
            int areasLong = Random.Range(level2WidthRange.x, level2WidthRange.y) / areaManager.areaSize;
            Vector2Int outerAreaStart = new Vector2Int(centerInAreas.x - areasLong / 2, centerInAreas.y - areasLong / 2);

            //See if it fits
            if (!areaManager.SafeToGenerate(outerAreaStart.x, outerAreaStart.y, areasLong, AreaManager.AreaReservationType.LackingRequiredFoundation, true))
                continue;

            //If it does, then go ahead with adding the foundation...

            //Create some needed variables upfront
            int squareMetersLong = areasLong * areaManager.areaSize;
            bool circleOrTorusFoundation = allFoundationsAreSameShape ? city.circularCity : Random.Range(0, 2) == 0; //False = rectangular, True = circle or torus
            bool torusFoundation = circleOrTorusFoundation && squareMetersLong > 100 && level2HeightRange.x > 30.0f && torusChanceForCirculars > Random.Range(0.0f, 1.0f); //False = circle, True = torus
            bool generateWalls = squareMetersLong > 120 && !torusFoundation;

            //Tell the area reservation system that we are claiming this chunk to be taken up by this foundation
            areaManager.ReserveAreasWithType(outerAreaStart.x, outerAreaStart.y, areasLong, AreaManager.AreaReservationType.ReservedForExtraPerimeter, AreaManager.AreaReservationType.LackingRequiredFoundation);

            //Remember how much area we just reserved
            squareMetersClaimedSoFar += AreaManager.CalculateAreaFromDimensions(circleOrTorusFoundation, squareMetersLong * 0.5f);

            //Calculate the parameters needed to place the foundation
            Vector3 foundationLocalPosition = areaManager.AreaCoordToLocalCoord(new Vector3(centerInAreas.x, 0.0f, centerInAreas.y));
            foundationLocalPosition.y += level1Height * 0.5f;
            Vector3 foundationScale = Vector3.one * squareMetersLong;
            foundationScale.y = Mathf.Max(20.0f, Random.Range(level2HeightRange.x, level2HeightRange.y));

            //Place the foundation and hook it up with bridges
            FoundationShape foundationShape = circleOrTorusFoundation ? (torusFoundation ? FoundationShape.Torus : FoundationShape.Circular) : FoundationShape.Rectangular;
            foundationManager.GenerateNewFoundation(foundationLocalPosition, foundationScale, foundationShape, true);

            //Next, create a smaller concentric subpocket where buildings can spawn inside the larger area we just reserved
            //We'll call the area in between the two radii where the walls spawn the buffer zone
            int bufferInAreas = 3;
            if (generateWalls)
                bufferInAreas *= 2;

            if (circleOrTorusFoundation)
            {
                int innerCircleRadiusInAreas = (areasLong / 2);
                if (torusFoundation) //The inner circle from the torus is a greater deduction than the buffer ever could be so don't need to worry about buffer for toruses
                    innerCircleRadiusInAreas = Mathf.FloorToInt(innerCircleRadiusInAreas * FoundationManager.torusAnnulusMultiplier);
                else
                    innerCircleRadiusInAreas -= bufferInAreas;

                areaManager.ReserveAreasWithinThisCircle(centerInAreas.x, centerInAreas.y, innerCircleRadiusInAreas, AreaManager.AreaReservationType.Open, false, AreaManager.AreaReservationType.ReservedForExtraPerimeter);

                if(torusFoundation)
                    GenerateInsideOfTorusFoundationRecursive(foundationLocalPosition, foundationScale, 2, foundationScale.y, Random.Range(1, 4), centerInAreas);
            }
            else
            {
                Vector2Int innerAreaStart = new Vector2Int(outerAreaStart.x + bufferInAreas, outerAreaStart.y + bufferInAreas);
                areaManager.ReserveAreasWithType(innerAreaStart.x, innerAreaStart.y, areasLong - 2 * bufferInAreas, AreaManager.AreaReservationType.Open, AreaManager.AreaReservationType.ReservedForExtraPerimeter);
            }

            //After that, run a chance that we continue to a third level
            if (!torusFoundation && chanceForLevel3 > Random.Range(0.0f, 1.0f))
                GenerateAnotherLevelToFoundation(foundationLocalPosition, foundationScale, foundationShape);

            //Otherwise, create walls that line the edges of the foundation
            else if (generateWalls)
            {
                NewCityWallRequest newCityWallRequest = new NewCityWallRequest(circleOrTorusFoundation, foundationLocalPosition, (squareMetersLong - (bufferInAreas * areaManager.areaSize)) * 0.5f);
                city.cityWallManager.newCityWallRequests.Add(newCityWallRequest);
            }

            //Add the building subpocket to the city block system so that special and/or large buildings know to try to generate in the middle...
            //...of these locations first before trying random placement
            int innerRadiusInAreas = (squareMetersLong / areaManager.areaSize) - bufferInAreas;
            city.areaManager.availableCityBlocks.Add(new CityBlock(centerInAreas, Vector2Int.one * innerRadiusInAreas));
        }

        //If we are making level 1 elevated, then generate what is needed for that now
        if (level1IsElevated)
        {
            //FIRST, generate the vertical scalers to go from level 1 to level 2
            float level1Elevation = level1Height / 2.0f;
            float level2AvgHeight = (level2HeightRange.x + level2HeightRange.y) * 0.5f;
            GenerateLevel2VerticalScalers(level1Elevation, level1Elevation + (level2AvgHeight * 0.5f));

            //THEN, generate the level 1 foundation --- ordering is important!
            //This needs to be second so that its foundation colliders do not get added to the list of foundation colliders to search when we're looking...
            //...for a foundation collider to put our second level vertical scaler beside. That algorithm needs to just be looking at level 2 colliders.
            Vector3 level1FoundationScale = Vector3.one * city.radius * 2.15f;
            level1FoundationScale.y = level1Height;
            foundationManager.GenerateNewFoundation(Vector3.zero, level1FoundationScale, city.circularCity ? FoundationShape.Circular : FoundationShape.Rectangular, false);
        }

        //Still need entrances at city perimeters (could be level 1 or level 2 entrances depending on whether level 1 is elevated)
        //This should be done nearly last, after the GenerateLevel2VerticalScalers. We don't want the foundation colliders from this to show up in those computations.
        float extraDistanceFromCityToEntrance = level1IsElevated ? 0.0f : 10.0f;
        foundationManager.GenerateEntrancesForCardinalDirections(!level1IsElevated, extraDistanceFromCityCenter: extraDistanceFromCityToEntrance);

        //Make it so that buildings can spawn on level 1 as well
        areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.Open, AreaManager.AreaReservationType.LackingRequiredFoundation);
    }

    //Helper functions---

    private void GenerateLevel2VerticalScalers(float level1LocalElevation, float level2AvgLocalElevation)
    {
        //If there is no level 2, then don't bother making an elevator to it
        if (foundationManager.foundationGroundColliders == null || foundationManager.foundationGroundColliders.Count == 0)
            return;

        //Otherwise...

        //Updates the physics colliders based on changes to transforms.
        //This is needed in order for the foundation colliders to return the correct computations on the following lines.
        Physics.SyncTransforms();

        //Place the vertical scalers...
        List<Vector3> newScalerPositions = new List<Vector3>();
        int targetScalerCount = GetNumberOf2ndLevelScalersWeShouldGenerate(city.radius);
        for(int scalersPlaced = 0; scalersPlaced < targetScalerCount; scalersPlaced++)
        {
            Vector3 newScalerPosition = GenerateALevel2VerticalScaler(level1LocalElevation, newScalerPositions);
            newScalerPositions.Add(newScalerPosition);
        }
    }

    private Vector3 GenerateALevel2VerticalScaler(float level1LocalElevation, List<Vector3> previousScalerPositions)
    {
        //Figure out where to put it and what foundation it is up against
        Vector3 placeInGlobal = FindAPlaceInGlobalToPutA2ndLevelVerticalScaler(out Foundation nearestFoundation, previousScalerPositions);

        //Place it
        float cityElevation = city.transform.position.y;
        float level1GlobalElevation = level1LocalElevation + cityElevation;
        float level2GlobalElevation = placeInGlobal.y;
        city.verticalScalerManager.GenerateVerticalScalerWithFocalPoint(nearestFoundation.transform.position, placeInGlobal, level1GlobalElevation, level2GlobalElevation, false);

        //Reserve area around the vertical scaler so that no buildings can spawn there
        Vector3 placeInAreas = city.areaManager.LocalCoordToAreaCoord(city.transform.InverseTransformPoint(placeInGlobal));
        int radiusInAreas = 20 / city.areaManager.areaSize;
        city.areaManager.ReserveAreasWithinThisCircle((int)placeInAreas.x, (int)placeInAreas.z, radiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);

        return placeInGlobal;
    }

    private Vector3 FindAPlaceInGlobalToPutA2ndLevelVerticalScaler(out Foundation nearestFoundation, List<Vector3> previousScalerPositions)
    {
        //These are the variables we need to set before we leave this function (these defaults will never be used but are needed to avoid compilation errors)
        Vector3 closestPointInGlobal = Vector3.zero;
        nearestFoundation = null;

        //Keep trying until we are successful or get the fuck-its
        for (int attempt = 1; attempt <= 400; attempt++)
        {
            //Get a random point outside the city
            Vector3 randomOutsidePointInGlobal = city.GetRandomGlobalPointOutsideOfCity();

            //Find the nearest foundation collider and point on that collider
            nearestFoundation = foundationManager.GetClosestFoundationAndPoint(randomOutsidePointInGlobal, out closestPointInGlobal, true);

            //Determine if that nearest point will work...

            //Can't be too close to edge of city or might overlap with city gates/walls
            Vector3 closestPointInGlobalWithoutY = closestPointInGlobal;
            closestPointInGlobalWithoutY.y = city.transform.position.y;
            float okCityRadiusPercentage = GetOKPercentDistanceFromCityCenterForScaler(attempt);
            if (Vector3.Distance(closestPointInGlobalWithoutY, city.transform.position) > city.radius * okCityRadiusPercentage)
                continue;

            //Can't be too close to other vertical scalers that have already been placed
            //(for practical purposes and so they don't physically spawn over each other)
            if (!TooCloseToPreviousXZPositions(closestPointInGlobal, previousScalerPositions, 50.0f))
                break;
        }

        //Kowalski, status report!
        return closestPointInGlobal;
    }

    private void GenerateInsideOfTorusFoundationRecursive(Vector3 foundationLocalPosition, Vector3 encompassingFoundationScale, float iterationCount, float originalYScale, int verticalFillMode, Vector2Int centerInAreas)
    {
        //verticalFillMode code: 1 = maintain same 2nd story height, 2 = every iteration TILE another story on, 3 = every iteration STRETCH the y-scale higher

        //Compute the new foundation scale
        Vector3 newFoundationScale = encompassingFoundationScale * FoundationManager.torusAnnulusMultiplier;
        newFoundationScale.y = verticalFillMode == 3 ? originalYScale * iterationCount : originalYScale;

        //...and radius
        float newFoundationRadius = newFoundationScale.x * 0.5f;
        if (newFoundationRadius < 50.0f) //Torus has gotten too small to continue
        {
            //DetermineWhatToDoWithTorusCenter();
            return;
        }

        //Generate the new torus foundation
        Foundation newFoundation = null;
        if(verticalFillMode == 1 || verticalFillMode == 3)
            newFoundation = foundationManager.GenerateNewFoundation(foundationLocalPosition, newFoundationScale, FoundationShape.Torus, false);
        else if(verticalFillMode == 2)
        {
            Vector3 tiledFoundationPosition = foundationLocalPosition;
            Vector3 tiledFoundationScale = newFoundationScale;
            for (int x = 1; x <= iterationCount; x++)
            {
                newFoundation = foundationManager.GenerateNewFoundation(tiledFoundationPosition, tiledFoundationScale, FoundationShape.Torus, false);

                tiledFoundationPosition.y += originalYScale * 0.5f;
                tiledFoundationScale *= Random.Range(0.995f, 0.9999f); //Ever so slightly randomize x,z scale the prevent clipping
                tiledFoundationScale.y = newFoundationScale.y;
            }
        }

        //Generate any vertical scalers if needed
        if (newFoundation != null && (verticalFillMode == 2 || verticalFillMode == 3))
        {
            float globalBottomOfEntireStructure = city.transform.position.y + foundationLocalPosition.y;
            float globalBottomLevel = globalBottomOfEntireStructure + ((originalYScale * 0.5f) * (iterationCount - 1));
            float globalTopLevel = globalBottomLevel + originalYScale * 0.5f;
            city.verticalScalerManager.GenerateVerticalScalerOnRandomEdgeOfFoundation(newFoundation, globalBottomLevel, globalTopLevel);
        }

        //Tell the area reservation system that we are claiming this chunk to be taken up by this foundation
        int outerRadiusInAreas = Mathf.CeilToInt(newFoundationRadius / city.areaManager.areaSize);
        city.areaManager.ReserveAreasWithinThisCircle(centerInAreas.x, centerInAreas.y, outerRadiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.Open);

        //Recurse
        GenerateInsideOfTorusFoundationRecursive(foundationLocalPosition, newFoundationScale, iterationCount + 1, originalYScale, verticalFillMode, centerInAreas);
    }

    private void GenerateAnotherLevelToFoundation(Vector3 baseFoundationLocalPosition, Vector3 baseFoundationScale, FoundationShape foundationShape)
    {
        float bufferSize = Random.Range(20.0f, 25.0f);

        //Compute the new foundation scale
        Vector3 newFoundationScale = baseFoundationScale;
        newFoundationScale.x -= bufferSize;
        newFoundationScale.z -= bufferSize;

        //If the new scale is too small, then abort making another level
        if (newFoundationScale.x < 75.0f)
            return;

        //Compute the new foundation position
        Vector3 newFoundationLocalPosition = baseFoundationLocalPosition;
        newFoundationLocalPosition.y += newFoundationScale.y * 0.5f;

        //Place the new foundation
        Foundation newFoundation = foundationManager.GenerateNewFoundation(newFoundationLocalPosition, newFoundationScale, foundationShape, false);

        //Generate a vertical scaler to transit pills from the previous level to the new one
        float globalBottomLevel = city.transform.position.y + baseFoundationLocalPosition.y + baseFoundationScale.y * 0.5f;
        float globalTopLevel = globalBottomLevel + newFoundationScale.y * 0.5f;
        city.verticalScalerManager.GenerateVerticalScalerOnRandomEdgeOfFoundation(newFoundation, globalBottomLevel, globalTopLevel);

        //Reserve the area of the base foundation as being off-limits (so that buildings don't spawn on the edge)
        int areasLong = Mathf.CeilToInt(newFoundationScale.x / city.areaManager.areaSize);
        if(foundationShape == FoundationShape.Circular)
        {
            Vector3 centerInAreas = city.areaManager.LocalCoordToAreaCoord(newFoundationLocalPosition);
            city.areaManager.ReserveAreasWithinThisCircle((int)centerInAreas.x, (int)centerInAreas.z, areasLong / 2, AreaManager.AreaReservationType.ReservedForExtraPerimeter, false, AreaManager.AreaReservationType.Open);
        }
        else
        {
            Vector3 startCoordInAreas = city.areaManager.LocalCoordToAreaCoord(newFoundationLocalPosition);
            startCoordInAreas.x -= areasLong / 2;
            startCoordInAreas.z -= areasLong / 2;
            city.areaManager.ReserveAreasWithType((int)startCoordInAreas.x, (int)startCoordInAreas.z, areasLong, AreaManager.AreaReservationType.ReservedForExtraPerimeter, AreaManager.AreaReservationType.Open);
        }
    }

    private static bool TooCloseToPreviousXZPositions(Vector3 potentialPosition, List<Vector3> previousScalerPositions, float distanceThreshold)
    {
        //See if any of the positions in the list are too close to the potential position
        foreach(Vector3 previousPosition in previousScalerPositions)
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

    private static int GetNumberOf2ndLevelScalersWeShouldGenerate(float cityRadius)
    {
        if (cityRadius < 150)
            return Random.Range(2, 4);
        else if (cityRadius < 225)
            return Random.Range(3, 5);
        else
            return Random.Range(4, 6);
    }

    private static float GetOKPercentDistanceFromCityCenterForScaler(int attemptNumber)
    {
        if (attemptNumber < 100)
            return 0.85f;
        else if (attemptNumber < 200)
            return 0.9f;
        else if (attemptNumber < 300)
            return 0.95f;
        else
            return 2.0f; //At this point, anything under the sun is just great
    }

    private static Vector2Int Get2ndLevelFoundationWidthRange(float cityRadius)
    {
        if (cityRadius < 80) //This is really too small a city size. To compensate, force 2nd level foundations to be huge so everything will go on the 1st level.
            //The 1st level tends to fit things more efficiently and efficiency really matters here.
            return new Vector2Int(85, 100);
        else if (cityRadius < 140 || Random.Range(0, 5) == 0) //Small foundations
            return new Vector2Int(60, 130);
        else if (Random.Range(0, 2) == 0) //Standard variety -- decent sized foundations mixed with maybe a couple small ones
            return new Vector2Int(80, 230);
        else if (Random.Range(0, 2) == 0) //Go for larger foundations
            return new Vector2Int(120, 300);
        else //Super variety
            return new Vector2Int(50, 350);
    }
}
