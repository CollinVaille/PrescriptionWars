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
    private float radiusMultiplierForEachIteration = 0.765f; //Each ring's radius is computed by multiplying this times the previous ring's radius

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

        //Create variables needed for the upcoming for-loop
        float placementRadius = city.radius * 1.075f;
        float minimumFoundationLength = 80.0f, currentFoundationLength = 0.0f;
        bool shouldMakeGapForThisIteration = false, previousIterationWasGap = false;
        float outerRadiusOfCurrentGap = 0.0f;

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
                //Tally how much real estate we're about to add to the housing market
                currentFoundationLength += (placementRadius - radiusForRingAfterThisOne);

                //If we've assisted homebuyers enough, add some golf courses next time (giant gaping hellpits where all golfers belong)
                if(currentFoundationLength >= minimumFoundationLength)
                {
                    currentFoundationLength = 0.0f;
                    shouldMakeGapForThisIteration = true;
                    outerRadiusOfCurrentGap = radiusForRingAfterThisOne;
                }

                bool placeOuterGate = (iterationsCompleted == 0 || previousIterationWasGap);
                bool placeInnerGate = shouldMakeGapForThisIteration;

                //Finally, after much squabling, place the ring
                GenerateFoundationRing(placementRadius, foundationManager.foundationHeight, iterationsCompleted % 2 == 1, placeOuterGate, placeInnerGate);

                //If this is the first ring after a gap, make bridges to cover the gap and connect us to the last ring
                if(previousIterationWasGap)
                    GenerateBridgesForGap(outerRadiusOfCurrentGap, placementRadius, foundationManager.foundationHeight / 2.0f, foundationManager.foundationHeight / 2.0f);

                //Take notes for next iteration
                previousIterationWasGap = false;
            }

            //Iteration complete, ring placed and/or skipped
            placementRadius = radiusForRingAfterThisOne; //Outer radius of ring: 0.5, Inner radius of ring: 0.3825, 0.5/0.3825 = 0.765
        }
    }

    private void GenerateFoundationRing(float ringOuterRadius, float ringHeight, bool slightlyLowerHeight, bool generateOuterWalls, bool generateInnerWalls)
    {
        //Foundation position
        Vector3 foundationPosition = Vector3.zero;
        if (slightlyLowerHeight) //Since ring foundations have a chance to be placed consecutively, make every other foundation slightly lower to avoid clipping
            foundationPosition.y -= 0.01f;

        //Foundation scale
        Vector3 foundationScale = Vector3.one * (ringOuterRadius * 2.0f);
        foundationScale.y = ringHeight;

        //Place foundation
        foundationManager.GenerateNewFoundation(foundationPosition, foundationScale, FoundationShape.Torus, false);

        //Generate outer walls
        float wallOffset = GetBufferSizeForWalls(city.radius);
        if (generateOuterWalls)
            GenerateWallRing(ringOuterRadius - wallOffset, ringOuterRadius);

        //Mark the area inside the walls as open for things like buildings to spawn on them
        int buildingRadiusInAreas = Mathf.CeilToInt((ringOuterRadius - wallOffset) / city.areaManager.areaSize);
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

    private static float GetBufferSizeForWalls(int cityRadius)
    {
        if (cityRadius < 175)
            return 7.5f;
        else
            return Mathf.Min(12.5f, 7.5f + (cityRadius - 175) / 25.0f);
    }
}
