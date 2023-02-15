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
        float cityCenterRadiusLimit = Random.Range(15.0f, 45.0f);
        bool allowOuterWalls = (Random.Range(0, 3) != 0);
        bool allowInnerWalls = allowOuterWalls && (Random.Range(0, 2) == 0);
        float minimumAnnulusRadius = GetMinimumAnnulusRadiusForLargestRing(), currentAnnulusRadius = 0.0f;
        bool shouldMakeGapForThisIteration = false, previousIterationWasGap = false;
        float outerRadiusOfCurrentGap = 0.0f, outerHeightOfCurrentGap = 0.0f;
        bool strictlyAlternateWhenPossible = (Random.Range(0, 3) == 0);
        bool alreadyCompletedOneSizableRing = false, onlyCareAboutOneGoodRing = strictlyAlternateWhenPossible || (Random.Range(0, 2) == 0);
        bool wouldConsiderLargerRings = (Random.Range(0, 2) == 0);

        //Place concentric rings, one ring per iteration, from outside inwards until we get to the city center.
        //Once at the city center, we are done with this loop and will had what is placed at the center with a separate function, DecideWhatToDoWithCityCenter.
        int iterationsCompleted = 0;
        for (; placementRadius > cityCenterRadiusLimit; iterationsCompleted++)
        {
            float radiusForRingAfterThisOne = placementRadius * FoundationManager.torusAnnulusMultiplier;

            //Make gap
            if (shouldMakeGapForThisIteration)
            {
                GenerateGapRing(placementRadius);

                //Decision making for next iteration
                shouldMakeGapForThisIteration = (Random.Range(0, 2) == 0) && !strictlyAlternateWhenPossible;
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
                    GenerateBridgesAndLaddersForGap(outerRadiusOfCurrentGap, placementRadius, outerHeightOfCurrentGap, currentRingHeight);
                }

                //Tally how much real estate we're about to add to the housing market
                currentAnnulusRadius += (placementRadius - radiusForRingAfterThisOne);

                //If we've assisted homebuyers enough, add some golf courses next time (giant gaping hellpits where all golfers belong)
                if(RingShouldEndHere(currentAnnulusRadius, minimumAnnulusRadius, alreadyCompletedOneSizableRing, onlyCareAboutOneGoodRing, wouldConsiderLargerRings, strictlyAlternateWhenPossible))
                {
                    if (!alreadyCompletedOneSizableRing)
                        alreadyCompletedOneSizableRing = true;

                    currentAnnulusRadius = 0.0f;
                    shouldMakeGapForThisIteration = true;
                    outerRadiusOfCurrentGap = radiusForRingAfterThisOne;
                    outerHeightOfCurrentGap = currentRingHeight;
                }

                bool placeOuterWalls = allowOuterWalls && placementRadius > 100 && (iterationsCompleted == 0 || previousIterationWasGap);
                bool placeInnerWalls = allowInnerWalls && placementRadius > 175 && shouldMakeGapForThisIteration;

                //Finally, after much squabling, place the ring
                GenerateFoundationRing(placementRadius, currentRingHeight, iterationsCompleted % 2 == 1, placeOuterWalls, placeInnerWalls, true);

                //Take notes for next iteration
                previousIterationWasGap = false;
            }

            //Iteration complete, ring placed and/or skipped
            placementRadius = radiusForRingAfterThisOne;
        }

        //Either place some foundations at the city center or make it a gap where nothing can spawn...

        //First, determine the height for any foundations placed at the center (which will be needed for the next part)
        if(previousIterationWasGap)
            currentRingHeight = CalculateNewRingHeight(outerRadiusOfCurrentGap - placementRadius, outerHeightOfCurrentGap, elevationAngleInDegrees);

        //Then commence determining what foundations to place or hole to make in the center
        float outerRadiusOfPotentialGap = previousIterationWasGap ? outerRadiusOfCurrentGap : placementRadius;
        float outerHeightOfPotentialGap = previousIterationWasGap ? outerHeightOfCurrentGap : currentRingHeight;
        bool foundationAtCityCenter = DecideWhatToDoWithCityCenter(placementRadius, currentRingHeight, outerRadiusOfPotentialGap, outerHeightOfPotentialGap, iterationsCompleted % 2 == 1);
        if(foundationAtCityCenter && previousIterationWasGap) //Generate bridges between city center and rest of city
            GenerateBridgesAndLaddersForGap(outerRadiusOfCurrentGap, placementRadius, outerHeightOfCurrentGap, currentRingHeight);

        //Finally, determine what to do with the bottom level of the city. Create a floor for the pits? Level as rock, sand, etc.?
        DetermineWhatToDoWithBottomLevel();
    }

    private void GenerateFoundationRing(float ringOuterRadius, float ringHeight, bool slightlyLowerHeight, bool generateOuterWalls, bool generateInnerWalls, bool markAsOpen)
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
        if(markAsOpen)
        {
            int buildingRadiusInAreas = Mathf.FloorToInt((ringOuterRadius - wallOffset) / city.areaManager.areaSize);
            city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, buildingRadiusInAreas, AreaManager.AreaReservationType.Open, false, AreaManager.AreaReservationType.ReservedForExtraPerimeter);
        }

        //Generate inner walls
        if (generateInnerWalls)
        {
            float ringInnerRadius = ringOuterRadius * FoundationManager.torusAnnulusMultiplier;
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

    private void GenerateBridgesAndLaddersForGap(float gapOuterRadius, float gapInnerRadius, float gapOuterHeight, float gapInnerHeight)
    {
        GenerateBridgesForGap(gapOuterRadius, gapInnerRadius, gapOuterHeight, gapInnerHeight);
        GenerateEscapeLadderOnOutsideOfGap(gapOuterRadius, gapOuterHeight);
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
    private bool DecideWhatToDoWithCityCenter(float centerRadius, float centerHeight, float outerRadiusOfPotentialGap, float outerHeightOfPotentialGap, bool slightlyLowerHeight)
    {
        //So small there is no point in doing anything
        if (centerRadius < 1.0f)
            return false;

        if (centerRadius < 30.0f && Random.Range(0, 2) == 0) //Create some hypnotic trippy ass fountain-like shit at the center with the rings
        {
            GenerateAscendingRingsAtCenter(centerRadius, centerHeight);
            return false;
        }
        else if (Random.Range(0, 2) == 0) //Mark as gap
        {
            GenerateGapRing(centerRadius);
            GenerateEscapeLadderOnOutsideOfGap(outerRadiusOfPotentialGap, outerHeightOfPotentialGap);
            return false;
        }
        else //Plug it up with a circular foundation and maybe allow some buildings to spawn at the center
        {
            GenerateCircularFoundationAtCenter(centerRadius, centerHeight, Random.Range(0, 2) == 0);
            return true;
        }
    }

    private void GenerateCircularFoundationAtCenter(float centerRadius, float centerHeight, bool allowThingsToSpawnAtCenter)
    {
        //Convey to area manager whether buildings should be allowed to spawn at the city center (decided by parameter allowThingsToSpawnAtCenter)
        float radiusInAreasFloat = centerRadius / city.areaManager.areaSize;
        int radiusInAreas = allowThingsToSpawnAtCenter ? Mathf.FloorToInt(radiusInAreasFloat) : Mathf.CeilToInt(radiusInAreasFloat);
        AreaManager.AreaReservationType centerAreaStatus = allowThingsToSpawnAtCenter ? AreaManager.AreaReservationType.Open : AreaManager.AreaReservationType.ReservedForExtraPerimeter;
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, radiusInAreas, centerAreaStatus, true, AreaManager.AreaReservationType.ReservedForExtraPerimeter);

        //Let the building generator know this is a good place to put a building if we decided to allow buildings here
        if(allowThingsToSpawnAtCenter)
        {
            CityBlock centerBlock = new CityBlock(cityCenterInAreaCoords, Vector2Int.one * ((int)(centerRadius / city.areaManager.areaSize)));
            city.areaManager.availableCityBlocks.Add(centerBlock);
        }

        //Place the actual foundation
        Vector3 localFoundationScale = Vector3.one * (centerRadius * 2.1f);
        localFoundationScale.y = centerHeight * 2.0f;
        foundationManager.GenerateNewFoundation(Vector3.zero, localFoundationScale, FoundationShape.Circular, false);
    }

    private void GenerateAscendingRingsAtCenter (float centerRadius, float centerHeight)
    {
        //Mark the area within this radius as no man's land
        int gapRadiusInAreas = Mathf.CeilToInt(centerRadius / city.areaManager.areaSize);
        city.areaManager.ReserveAreasWithinThisCircle(cityCenterInAreaCoords.x, cityCenterInAreaCoords.y, gapRadiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, false, AreaManager.AreaReservationType.Open);

        //Create some needed variables before we start placing rings
        float ringRadius = centerRadius, ringHeight = centerHeight;
        float targetEndRadius = Random.Range(5.0f, 2.5f);
        bool constantIncrement = (Random.Range(0, 2) == 0);
        float ringHeightIncrement = Random.Range(150.0f, 300.0f);

        if (constantIncrement)
            ringHeightIncrement *= Random.Range(0.2f, 0.35f);

        //Generate concentric rings at the city center from outside, in. One ring per iteration
        for (; ringRadius > targetEndRadius;)
        {
            //Determine the ring's height
            if (constantIncrement)
                ringHeight += ringHeightIncrement;
            else
                ringHeight += ringHeightIncrement / ringRadius;

            //Generate the decorative ring
            GenerateFoundationRing(ringRadius, ringHeight, false, false, false, false);

            //Move to next ring
            ringRadius *= FoundationManager.torusAnnulusMultiplier;
        }

        //Plug up the remaining center with a tiny circular foundation
        GenerateCircularFoundationAtCenter(ringRadius, ringHeight, false);
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

    private static bool RingShouldEndHere(float currentAnnulusRadius, float minimumAnnulusRadius, bool alreadyCompletedOneSizableRing, bool onlyCareAboutOneGoodRing, bool wouldConsiderLargerRings, bool strictlyAlternateWhenPossible)
    {
        if (alreadyCompletedOneSizableRing && onlyCareAboutOneGoodRing)
            return strictlyAlternateWhenPossible || (Random.Range(0, 2) == 0);
        else if (currentAnnulusRadius >= minimumAnnulusRadius)
            return strictlyAlternateWhenPossible || !wouldConsiderLargerRings || Random.Range(0, 3) != 0;
        else
            return false;
    }

    private void GenerateEscapeLadderOnOutsideOfGap(float gapOuterRadius, float localTopHeight)
    {
        float globalTop = localTopHeight + city.transform.position.y;
        float globalBottom = globalTop - localTopHeight;

        Vector3 pointOutsideCityWithGlobalTop = city.transform.position + Vector3.one * city.radius;
        pointOutsideCityWithGlobalTop.y = globalTop;

        Vector3 cityCenterWithGlobalTop = city.transform.position;
        cityCenterWithGlobalTop.y = globalTop;

        Vector3 ringEdgePoint = Vector3.MoveTowards(cityCenterWithGlobalTop, pointOutsideCityWithGlobalTop, gapOuterRadius - 2.0f);
        ringEdgePoint.y = globalTop;

        Vector3 ringFocalPoint = Vector3.MoveTowards(ringEdgePoint, cityCenterWithGlobalTop, -1.0f);

        bool useMinorVerticalScaler = gapOuterRadius < 150.0f || localTopHeight < 25.0f;
        city.verticalScalerManager.GenerateVerticalScalerWithFocalPoint(ringFocalPoint, ringEdgePoint, globalBottom, globalTop, useMinorVerticalScaler);
    }

    private void DetermineWhatToDoWithBottomLevel()
    {
        //Determine if there's an ocean and if so, then if its sea level is close to the height level of our city.
        bool oceanNearCityElevation;
        if (Planet.planet.hasOcean)
            oceanNearCityElevation = (Planet.planet.oceanTransform.position.y + 2.0f - city.transform.position.y) > 0.0f;
        else
            oceanNearCityElevation = false;

        //Chance to generate a very short and fat foundation at the bottom center of the city. Makes pits have floors.
        if (oceanNearCityElevation || Random.Range(0, 2) == 0)
        {
            Vector3 floorFoundationScale = Vector3.one * city.radius * 1.95f;
            floorFoundationScale.y = 0.15f;

            foundationManager.GenerateNewFoundation(Vector3.zero, floorFoundationScale, FoundationShape.Circular, false);
        }
    }
}
