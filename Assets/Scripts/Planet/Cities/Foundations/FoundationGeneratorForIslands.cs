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
            Vector2Int center = new Vector2Int(Random.Range(0, areaManager.areaTaken.GetLength(0)), Random.Range(0, areaManager.areaTaken.GetLength(1)));
            int areasLong = Random.Range(level2WidthRange.x, level2WidthRange.y) / areaManager.areaSize;
            Vector2Int outerAreaStart = new Vector2Int(center.x - areasLong / 2, center.y - areasLong / 2);

            //See if it fits
            if (!areaManager.SafeToGenerate(outerAreaStart.x, outerAreaStart.y, areasLong, AreaManager.AreaReservationType.LackingRequiredFoundation, true))
                continue;

            //If it does, then go ahead with adding the foundation...

            //Create some needed variables upfront
            bool circularFoundation = allFoundationsAreSameShape ? city.circularCity : (Random.Range(0, 2) == 0);
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
            foundationLocalPosition.y += level1Height / 2.0f;
            Vector3 foundationScale = Vector3.one * squareMetersLong;
            foundationScale.y = Mathf.Max(20.0f, Random.Range(level2HeightRange.x, level2HeightRange.y));

            //Place the foundation and hook it up with bridges
            foundationManager.GenerateNewFoundation(foundationLocalPosition, foundationScale, circularFoundation, true);

            //Create walls that line the edges of the foundation
            if (generateWalls)
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
            foundationManager.GenerateNewFoundation(Vector3.zero, level1FoundationScale, city.circularCity, false);
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
        if (foundationManager.foundationColliders == null || foundationManager.foundationColliders.Count == 0)
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
        Vector3 placeInGlobal = FindAPlaceInGlobalToPutA2ndLevelVerticalScaler(out Collider nearestCollider, previousScalerPositions);

        //Place it
        float cityElevation = city.transform.position.y;
        float level1GlobalElevation = level1LocalElevation + cityElevation;
        float level2GlobalElevation = placeInGlobal.y;
        foundationManager.GenerateVerticalScalerBesideFoundationCollider(nearestCollider.transform.position, placeInGlobal, level1GlobalElevation, level2GlobalElevation);

        //Reserve area around the vertical scaler so that no buildings can spawn there
        Vector3 placeInAreas = city.areaManager.LocalCoordToAreaCoord(city.transform.InverseTransformPoint(placeInGlobal));
        int radiusInAreas = 20 / city.areaManager.areaSize;
        city.areaManager.ReserveAreasWithinThisCircle((int)placeInAreas.x, (int)placeInAreas.z, radiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);

        return placeInGlobal;
    }

    private Vector3 FindAPlaceInGlobalToPutA2ndLevelVerticalScaler(out Collider nearestCollider, List<Vector3> previousScalerPositions)
    {
        //These are the variables we need to set before we leave this function (these defaults will never be used but are needed to avoid compilation errors)
        Vector3 closestPointInGlobal = Vector3.zero;
        nearestCollider = null;

        //Keep trying until we are successful or get the fuck-its
        for (int attempt = 1; attempt <= 400; attempt++)
        {
            //Get a random point outside the city
            Vector3 randomOutsidePointInGlobal = GetRandomGlobalPointOutsideOfCity(city);

            //Find the nearest foundation collider and point on that collider
            nearestCollider = GetClosestFoundationColliderAndPoint(foundationManager.foundationColliders, randomOutsidePointInGlobal, out closestPointInGlobal);

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

    private static Vector3 GetRandomGlobalPointOutsideOfCity(City city)
    {
        //Generate a point in local space 1 unit out from the center of the city that is 0-180 degrees spherically interpolated between the front and back
        Vector3 randomDirection = Vector3.Slerp(Vector3.forward, Vector3.back, Random.Range(0.0f, 1.0f));

        //Make the 0-180 range now 0-360
        if (Random.Range(0, 2) == 0)
            randomDirection = -randomDirection;

        //We have our direction, now change it from 1 unit out to cityRadius+ units out and change from local to global space
        Vector3 randomPointInGlobal = city.transform.TransformPoint(randomDirection * city.radius * 100.0f);

        //Make the point high in the air so that closest points on a collider will be the highest point
        return randomPointInGlobal + Vector3.up * 1000.0f;
    }

    private static Collider GetClosestFoundationColliderAndPoint(List<Collider> collidersToSearch, Vector3 referencePointInGlobal, out Vector3 closestPointInGlobal)
    {
        //This function assumes the parameter, "collidersToSearch", is not empty...

        //Start with the first collider being the closest
        Collider closestCollider = collidersToSearch[0];
        closestPointInGlobal = collidersToSearch[0].ClosestPoint(referencePointInGlobal);
        float smallestDistance = Vector3.Distance(referencePointInGlobal, closestPointInGlobal);

        //Go through the rest of the colliders and see if any of them are closer
        for(int x = 0; x < collidersToSearch.Count; x++)
        {
            Collider currentCollider = collidersToSearch[x];
            Vector3 currentClosestPoint = currentCollider.ClosestPoint(referencePointInGlobal);
            float currentDistance = Vector3.Distance(referencePointInGlobal, currentClosestPoint);

            if (currentDistance < smallestDistance)
            {
                closestCollider = currentCollider;
                closestPointInGlobal = currentClosestPoint;
                smallestDistance = currentDistance;
            }
        }

        //Return what we found
        return closestCollider;
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
