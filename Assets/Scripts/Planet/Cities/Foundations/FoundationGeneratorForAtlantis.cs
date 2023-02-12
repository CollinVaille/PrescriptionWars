using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForAtlantis
{
    //References
    private City city;
    private FoundationManager foundationManager;

    //Cached computations
    private Vector2Int cityCenterInAreaCoords;
    private float radiusMultiplierForEachIteration = 0.765f; //Each ring's radius is computed by multiplying this times the previous ring's radius...
    //...Outer radius of ring model: 0.5, Inner radius of ring model: 0.3825, 0.3825/0.5 = 0.765

    //Main access functions---

    public FoundationGeneratorForAtlantis(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;

        //Cache this computation since it will be used multiple times later
        Vector3 center = city.areaManager.LocalCoordToAreaCoord(Vector3.zero);
        cityCenterInAreaCoords = new Vector2Int((int)center.x, (int)center.z);
    }

    public void GenerateNewAtlantisFoundations()
    {
        //We want to be able to control where the walls are ourselves here
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = false;

        //Will use standard cardinal entrances and maintain standard cardinal roads running throughout the city
        foundationManager.GenerateEntrancesForCardinalDirections(false);

        //Determine elevation data for the rings
        float currentRingHeight = foundationManager.foundationHeight / 2.0f;
        float elevationAngleInDegrees = DetermineElevationAngleBetweenRings();

        //Create other variables needed for the upcoming for-loop
        float placementRadius = city.radius * 1.075f;
        float minimumAnnulusRadius = GetMinimumAnnulusRadiusForLargestRing(), currentAnnulusRadius = 0.0f;
        bool shouldMakeGapForThisIteration = false, previousIterationWasGap = false;
        float outerRadiusOfCurrentGap = 0.0f, outerHeightOfCurrentGap = 0.0f;
        bool alreadyCompletedOneSizableRing = false, onlyCareAboutOneGoodRing = (Random.Range(0, 2) == 0);
        bool wouldConsiderLargerRings = (Random.Range(0, 2) == 0);

        //Place concentric rings, one ring per iteration, from outside inwards
        for (int iterationsCompleted = 0; placementRadius > 40; iterationsCompleted++)
        {
            float radiusForRingAfterThisOne = placementRadius * radiusMultiplierForEachIteration;

            //Make gap
            if (shouldMakeGapForThisIteration)
            {
                GenerateGapRing(placementRadius);

                //Decision making for next iteration
                shouldMakeGapForThisIteration = (Random.Range(0, 2) == 0);
                previousIterationWasGap = true;
            }
            
            //Make foundation ring
            else
            {
                //If this is the first ring after a gap, make bridges to cover the gap and connect us to the last ring
                if (previousIterationWasGap)
                {
                    //First, determine the height for the new ring (which will be needed for the next part)
                    currentRingHeight = CalculateNewRingHeight(outerRadiusOfCurrentGap - placementRadius, outerHeightOfCurrentGap, elevationAngleInDegrees);

                    //The generate the bridge
                    GenerateBridgesForGap(outerRadiusOfCurrentGap, placementRadius, outerHeightOfCurrentGap, currentRingHeight);
                }

                //Tally how much real estate we're about to add to the housing market
                currentAnnulusRadius += (placementRadius - radiusForRingAfterThisOne);

                //If we've assisted homebuyers enough, add some golf courses next time (giant gaping hellpits where all golfers belong)
                if(RingShouldEndHere(currentAnnulusRadius, minimumAnnulusRadius, alreadyCompletedOneSizableRing, onlyCareAboutOneGoodRing, wouldConsiderLargerRings))
                {
                    if (!alreadyCompletedOneSizableRing)
                        alreadyCompletedOneSizableRing = true;

                    currentAnnulusRadius = 0.0f;
                    shouldMakeGapForThisIteration = true;
                    outerRadiusOfCurrentGap = radiusForRingAfterThisOne;
                    outerHeightOfCurrentGap = currentRingHeight;
                }

                bool placeOuterGate = placementRadius > 100 && (iterationsCompleted == 0 || previousIterationWasGap);
                bool placeInnerGate = placementRadius > 175 && shouldMakeGapForThisIteration;

                //Finally, after much squabling, place the ring
                GenerateFoundationRing(placementRadius, currentRingHeight, iterationsCompleted % 2 == 1, placeOuterGate, placeInnerGate);

                //Take notes for next iteration
                previousIterationWasGap = false;
            }

            //Iteration complete, ring placed and/or skipped
            placementRadius = radiusForRingAfterThisOne;
        }

        //Either place some foundations at the city center or make it a gap where nothing can spawn...

        //First, determine the height for any foundations placed at the center (which will be needed for the next part)
        currentRingHeight = CalculateNewRingHeight(outerRadiusOfCurrentGap - placementRadius, outerHeightOfCurrentGap, elevationAngleInDegrees);

        //Then commence determining what foundations to place or hole to make in the center
        bool foundationAtCityCenter = DecideWhatToDoWithCityCenter(placementRadius, currentRingHeight);
        if(foundationAtCityCenter && previousIterationWasGap) //Generate bridges between city center and rest of city
            GenerateBridgesForGap(outerRadiusOfCurrentGap, placementRadius, outerHeightOfCurrentGap, currentRingHeight);
    }

    private void GenerateFoundationRing(float ringOuterRadius, float ringHeight, bool slightlyLowerHeight, bool generateOuterWalls, bool generateInnerWalls)
    {
        //Foundation position
        Vector3 foundationPosition = Vector3.zero;
        if (slightlyLowerHeight) //Since ring foundations have a chance to be placed consecutively, make every other foundation slightly lower to avoid clipping
            foundationPosition.y -= 0.01f;

        //Foundation scale
        Vector3 foundationScale = Vector3.one * (ringOuterRadius * 2.0f);
        foundationScale.y = ringHeight * 2.0f;

        //Place foundation
        foundationManager.GenerateNewFoundation(foundationPosition, foundationScale, FoundationShape.Torus, false);

        //Generate outer walls
        float wallOffset = GetBufferSizeForWalls(city.radius, ringOuterRadius);
        if (generateOuterWalls)
            GenerateWallRing(ringOuterRadius - wallOffset, ringOuterRadius);

        //Mark the area inside the walls as open for things like buildings to spawn on them
        int buildingRadiusInAreas = Mathf.FloorToInt((ringOuterRadius - wallOffset) / city.areaManager.areaSize);
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, buildingRadiusInAreas, AreaManager.AreaReservationType.Open, false, AreaManager.AreaReservationType.ReservedForExtraPerimeter);

        //Generate inner walls
        if(generateInnerWalls)
        {
            float ringInnerRadius = ringOuterRadius * radiusMultiplierForEachIteration;
            GenerateWallRing(ringInnerRadius + wallOffset, ringInnerRadius + wallOffset);
        }
    }

    private void GenerateWallRing(float wallRadius, float reservedAreaRadius)
    {
        //Request wall manager to place walls here later
        NewCityWallRequest newCityWallRequest = new NewCityWallRequest();
        newCityWallRequest.circular = true;
        newCityWallRequest.localCenter = Vector3.zero;
        newCityWallRequest.halfLength = wallRadius;
        city.cityWallManager.newCityWallRequests.Add(newCityWallRequest);

        //Reserve the area around these walls so that buildings cannot spawn on top of them (causing clipping)
        int wallsRadiusInAreas = Mathf.CeilToInt(reservedAreaRadius / city.areaManager.areaSize);
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, wallsRadiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, false, AreaManager.AreaReservationType.Open);
    }

    private void GenerateGapRing(float ringOuterRadius)
    {
        //Mark the area within this radius as no man's land
        int gapRadiusInAreas = Mathf.CeilToInt(ringOuterRadius / city.areaManager.areaSize);
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, gapRadiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, false, AreaManager.AreaReservationType.Open);
    }

    private void GenerateBridgesForGap(float gapOuterRadius, float gapInnerRadius, float gapOuterHeight, float gapInnerHeight)
    {
        //Each iteration of the for-loop, four in total, creates one bridge.
        //The four bridges go in the four cardinal directions.
        for(int bridgeNumber = 1; bridgeNumber <= 4; bridgeNumber++)
        {
            //Determine which cardinal direction bridge we're doing
            Vector3 direction;
            switch(bridgeNumber)
            {
                case 1: direction = city.transform.forward; break; //Z+ bridge
                case 2: direction = -city.transform.forward; break; //Z- bridge
                case 3: direction = city.transform.right; break; //X+ bridge
                default: direction = -city.transform.right; break; //X- bridge
            }
            
            //Give the bridge manager the instructions to make the bridge later on when its his turn
            BridgeDestinationPairing bridgeDestinationPairing = GenerateBridgePairingForGap(gapOuterRadius, gapInnerRadius, gapOuterHeight, gapInnerHeight, direction);
            city.bridgeManager.AddNewDestinationPairing(bridgeDestinationPairing);
        }
    }

    private BridgeDestinationPairing GenerateBridgePairingForGap(float gapOuterRadius, float gapInnerRadius, float gapOuterHeight, float gapInnerHeight, Vector3 direction)
    {
        BridgeDestination outerDestination = GenerateBridgeDestination(gapOuterRadius, gapOuterHeight, direction);
        BridgeDestination innerDestination = GenerateBridgeDestination(gapInnerRadius, gapInnerHeight, direction);

        return new BridgeDestinationPairing(outerDestination, innerDestination);
    }

    private BridgeDestination GenerateBridgeDestination(float destinationRadius, float destinationHeight, Vector3 directionInGlobal)
    {
        Vector3 destinationPoint = directionInGlobal * destinationRadius;
        destinationPoint.y = destinationHeight;
        destinationPoint = city.transform.TransformPoint(destinationPoint);

        return new BridgeDestination(destinationPoint, 0.1f);
    }

    private static float GetBufferSizeForWalls(int cityRadius, float ringOuterRadius)
    {
        float baseBufferSize = GetBufferSizeForWallsJustBasedOnCity(cityRadius);

        if (cityRadius < 175 || ringOuterRadius > 200.0f)
            return baseBufferSize;
        else
            return Mathf.Max(7.5f, baseBufferSize - (250.0f / ringOuterRadius));
    }

    private static float GetBufferSizeForWallsJustBasedOnCity(int cityRadius)
    {
        if (cityRadius < 175)
            return 7.5f;
        else
            return Mathf.Min(12.5f, 7.5f + (cityRadius - 175) / 25.0f);
    }

    //Returns whether a foundation was spawned at the city center that could need connecting with a bridge. (That would be the caller's concern.)
    private bool DecideWhatToDoWithCityCenter(float centerRadius, float centerHeight)
    {
        //So small there is no point in doing anything
        if (centerRadius < 1.0f)
            return false;

        if (Random.Range(0, 2) == 0) //Mark as gap
        {
            GenerateGapRing(centerRadius);
            return false;
        }
        else //Plug it up with circular foundation and allow maybe a special building to spawn in the center
        {
            GenerateCircularFoundationAtCenter(centerRadius, centerHeight, Random.Range(0, 2) == 0);
            return true;
        }
    }

    private void GenerateCircularFoundationAtCenter(float radius, float height, bool allowThingsToSpawnAtCenter)
    {
        //Convey to area manager whether buildings should be allowed to spawn at the city center (decided by parameter allowThingsToSpawnAtCenter)
        float radiusInAreasFloat = radius / city.areaManager.areaSize;
        int radiusInAreas = allowThingsToSpawnAtCenter ? Mathf.FloorToInt(radiusInAreasFloat) : Mathf.CeilToInt(radiusInAreasFloat);
        AreaManager.AreaReservationType centerAreaStatus = allowThingsToSpawnAtCenter ? AreaManager.AreaReservationType.Open : AreaManager.AreaReservationType.ReservedForExtraPerimeter;
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, radiusInAreas, centerAreaStatus, true, AreaManager.AreaReservationType.ReservedForExtraPerimeter);

        //Let the building generator know this is a good place to put a building if we decided to allow buildings here
        if(allowThingsToSpawnAtCenter)
        {
            CityBlock centerBlock = new CityBlock(cityCenterInAreaCoords, Vector2Int.one * ((int)(radius / city.areaManager.areaSize)));
            city.areaManager.availableCityBlocks.Add(centerBlock);
        }

        //Place the actual foundation
        Vector3 localFoundationScale = Vector3.one * (radius * 2.0f);
        localFoundationScale.y = height * 2.0f;
        FoundationShape foundationShape = Random.Range(0, 2) == 0 ? FoundationShape.Circular : FoundationShape.Rectangular;
        foundationManager.GenerateNewFoundation(Vector3.zero, localFoundationScale, foundationShape, false);
    }

    //Returned number is in degrees. 0 degrees means the city is flat with no elevation changes.
    //A positive number means the inner rings are progressively higher in elevation. A negative number means they are progressively lower in elevation.
    //The number itself is the angle the bridge is at that connects each ring to the next.
    private static float DetermineElevationAngleBetweenRings()
    {
        if(Random.Range(0, 2) == 0) //Chance for non-zero angle
            return Random.Range(-14.0f, 14.0f); //Any higher or lower and connecting bridges would use ladders which we don't want
        else //Zero angle, meaning flat city
            return 0.0f;
    }

    private static float CalculateNewRingHeight(float radiusDifference, float previousRingHeight, float ringElevationDelta)
    {
        //Opposite = Adjacent * Tan(Angle)
        float heightChange = radiusDifference * Mathf.Tan(ringElevationDelta * Mathf.Deg2Rad);

        return Mathf.Max(10.0f, previousRingHeight + heightChange);
    }

    //Returns the minimum "annulus radius" (outer radius - inner radius - some possible extra buffer) the largest ring should have in the city.
    //In other words, there should be at least one ring with an annulus radius greater than or equal to this in the city.
    //This is to ensure there is enough room in the city to fit all the buildings we need to place.
    private float GetMinimumAnnulusRadiusForLargestRing()
    {
        return city.buildingManager.GetLongestBuildingLength() + (GetBufferSizeForWallsJustBasedOnCity(city.radius) * 2.0f);
    }

    private bool RingShouldEndHere(float currentAnnulusRadius, float minimumAnnulusRadius, bool alreadyCompletedOneSizableRing, bool onlyCareAboutOneGoodRing, bool wouldConsiderLargerRings)
    {
        if (alreadyCompletedOneSizableRing && onlyCareAboutOneGoodRing)
            return (Random.Range(0, 2) == 0);
        else if (currentAnnulusRadius >= minimumAnnulusRadius)
            return !wouldConsiderLargerRings || Random.Range(0, 2) == 0;
        else
            return false;
    }
}
