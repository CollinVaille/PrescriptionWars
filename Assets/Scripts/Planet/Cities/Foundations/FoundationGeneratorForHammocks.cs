using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForHammocks
{
    private enum CityGroundToucherPattern { Corners, Alternating, WideAlternating, Random }
    private enum HammockGroundToucherPattern { Corners, Alternating, WideAlternating, Wall }

    //References
    private City city;
    private FoundationManager foundationManager;

    //Status variables used across multiple functions
    private int minimumWidthForWidestHammock = 0;
    private bool metMinimumWidthRequirement = false;
    private float grounderToucherBonusElevation = Random.Range(20.0f, 50.0f);
    private float chanceToUseFoundationsForBridges = 0.0f;
    private bool circularFoundations = Random.Range(0, 2) == 0;

    //Main access functions---

    public FoundationGeneratorForHammocks(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;
    }

    public void GenerateNewHammockFoundations()
    {
        //Start with buildings being allowed to generate nowhere
        city.areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.LackingRequiredFoundation, AreaManager.AreaReservationType.Open);

        //Determine our plans for generating the hammocks
        List<HammockPlan> hammockPlans = GenerateHammockPlans();

        //Now that the plans have been finalized, use the plans to generate the hammocks
        GenerateHammocks(hammockPlans);
    }

    //Helper functions---

    private List<HammockPlan> GenerateHammockPlans()
    {
        List<HammockPlan> hammockPlans = new List<HammockPlan>();

        //Determine needed variables before entering the generation loop
        int cityLength = city.radius * 2, cityHalfLength = cityLength / 2;
        int bufferSizeForWalls = 10;
        minimumWidthForWidestHammock = city.buildingManager.GetLongestBuildingLength() + bufferSizeForWalls * 2;
        metMinimumWidthRequirement = false;
        CityGroundToucherPattern cityGroundToucherPattern = DetermineGroundToucherPatternForCity();
        chanceToUseFoundationsForBridges = DetermineChanceToUseFoundationsAsBridges();

        //Loop for generating hammock PLANS. One hammock plan generated per iteration, moving from left (-x) to right (+x)
        for (float currentLocalXPosition = -cityHalfLength; currentLocalXPosition < cityHalfLength;)
        {
            //First, gather some information needed to decide on plans
            float remainingCityWidth = cityHalfLength - currentLocalXPosition;
            HammockGroundToucherPattern hammockGroundToucherPattern = DetermineGroundToucherPatternForHammock(cityGroundToucherPattern, hammockPlans.Count);

            //If we realize we're running out of room, then abandon any further foundation plans
            if (remainingCityWidth < 50.0f)
                break;

            //Then, use that information to determine our plans here
            HammockPlan newHammockPlan = GenerateHammockPlan(currentLocalXPosition, remainingCityWidth, cityLength, hammockGroundToucherPattern, hammockPlans.Count);

            //Add it to the list of plans
            hammockPlans.Add(newHammockPlan);

            //Move the placehead to the right per the plan
            currentLocalXPosition += newHammockPlan.hammockWidth + newHammockPlan.widthOfGapToRight;
        }

        return hammockPlans;
    }

    private static CityGroundToucherPattern DetermineGroundToucherPatternForCity()
    {
        //Randoms look dope af so give them a good hardy chance
        if(Random.Range(0, 2) == 0)
            return CityGroundToucherPattern.Random;

        //Other crap here
        int rand = Random.Range(0, 3);
        if (rand == 0)
            return CityGroundToucherPattern.Corners;
        else if (rand == 1)
            return CityGroundToucherPattern.Alternating;
        else
            return CityGroundToucherPattern.WideAlternating;
    }

    private static float DetermineChanceToUseFoundationsAsBridges()
    {
        if (Random.Range(0, 2) == 0)
            return 0.0f;

        if (Random.Range(0, 2) == 0)
            return 1.0f;

        return Random.Range(0.0f, 1.0f);
    }

    private HammockPlan GenerateHammockPlan(float currentLocalXPosition, float remainingCityWidth, float hammockLength, HammockGroundToucherPattern hammockGroundToucherPattern, int hammockIndex)
    {
        HammockPlan hammockPlan = new HammockPlan();

        //Determine hammock width
        bool reachedEndOfCity = DetermineHammockWidth(hammockPlan, remainingCityWidth, false);

        //Determine hammock's local x position
        hammockPlan.localXPosition = currentLocalXPosition + hammockPlan.hammockWidth * 0.5f;

        //Determine hammock's top elevation
        hammockPlan.localTopElevation = foundationManager.foundationHeight;

        //Determine with of the gap to the right of the hammock
        reachedEndOfCity = DetermineHammockGapWidth(hammockPlan, remainingCityWidth, reachedEndOfCity);

        //Loop for generating hammock FOUNDATION PLANS. One hammock foundation plan generated per iteration, moving from bottom (-z) to top (+z)
        hammockPlan.foundationsPlans = new List<HammockFoundationPlan>();
        for (float currentLocalZPosition = -hammockLength / 2; currentLocalZPosition < 0;)
        {
            //First, gather some information needed to decide on plans
            float remainingHammockLength = -currentLocalZPosition;
            bool groundToucher = ShouldBeGroundToucher(hammockGroundToucherPattern, hammockPlan.foundationsPlans.Count);

            //If we realize we're running out of room, then abandon any further foundation plans
            if (remainingHammockLength < 50.0f)
                break;

            //Then, use that information to determine our plans here
            HammockFoundationPlan newHammockFoundationPlan = GenerateHammockFoundationPlan(hammockPlan, currentLocalZPosition, groundToucher, hammockIndex == 0);

            //Add it to the list of plans
            hammockPlan.foundationsPlans.Add(newHammockFoundationPlan);

            //Move the placehead to the right per the plan
            currentLocalZPosition += newHammockFoundationPlan.scale.z;
        }

        ReflectFoundationPlans(hammockPlan);

        return hammockPlan;
    }

    private HammockGroundToucherPattern DetermineGroundToucherPatternForHammock(CityGroundToucherPattern cityGroundToucherPattern, int hammockIndex)
    {
        if (cityGroundToucherPattern == CityGroundToucherPattern.Corners)
            return HammockGroundToucherPattern.Corners;
        else if (cityGroundToucherPattern == CityGroundToucherPattern.Alternating)
            return HammockGroundToucherPattern.Alternating;
        else if (cityGroundToucherPattern == CityGroundToucherPattern.WideAlternating)
            return HammockGroundToucherPattern.WideAlternating;
        else //CityGroundToucherPattern.Random
        {
            int rand = Random.Range(0, 4);
            if (rand == 0)
                return HammockGroundToucherPattern.Corners;
            if (rand == 1)
                return HammockGroundToucherPattern.Alternating;
            else if (rand == 2 && hammockIndex != 0)
                return HammockGroundToucherPattern.Wall;
            else
                return HammockGroundToucherPattern.WideAlternating;
        }
    }

    private static bool ShouldBeGroundToucher(HammockGroundToucherPattern groundToucherPattern, int groundToucherIndex)
    {
        if (groundToucherPattern == HammockGroundToucherPattern.Corners)
            return groundToucherIndex == 0;
        else if (groundToucherPattern == HammockGroundToucherPattern.Alternating)
            return groundToucherIndex % 2 == 0;
        else if (groundToucherPattern == HammockGroundToucherPattern.WideAlternating)
            return groundToucherIndex % 3 == 0;
        else //HammockGroundToucherPattern.Wall
            return true;
    }

    //The assumption going into this is that hammockPlan.foundationPlans has a list of foundation plans that spans half the length of the hammock.
    //This function will then reflect those plans to the other side of the hammock.
    private void ReflectFoundationPlans(HammockPlan hammockPlan)
    {
        //First of all, make the foundations "reflectable".
        //The foundation closest to the center should be sent to 0 exactly. The rest of the foundations should slide over accordingly.
        float offsetFromCenter = hammockPlan.foundationsPlans[hammockPlan.foundationsPlans.Count - 1].localZPosition;
        for (int x = 0; x < hammockPlan.foundationsPlans.Count; x++)
            hammockPlan.foundationsPlans[x].localZPosition -= offsetFromCenter;

        //After that is settled, go through all but the center foundation and create an identical copy of each with the local z position negated.
        float totalFoundationsToFlip = hammockPlan.foundationsPlans.Count - 1;
        List<HammockFoundationPlan> copyPlans = new List<HammockFoundationPlan>();
        for (int foundationIndex = 0; foundationIndex < totalFoundationsToFlip; foundationIndex++)
        {
            HammockFoundationPlan originalPlan = hammockPlan.foundationsPlans[foundationIndex];
            HammockFoundationPlan copyPlan = new HammockFoundationPlan(originalPlan);
            copyPlan.localZPosition = -originalPlan.localZPosition;

            copyPlans.Add(copyPlan);
        }

        //Add the copies to the foundation plans and then make sure the whole list is sorted according to localZPosition
        //The list needs to be sorted because there is logic later on that relies on the list being in sorted order (HammockPlan.GetNeighborFoundations)
        if (copyPlans.Count > 0)
        {
            hammockPlan.foundationsPlans.AddRange(copyPlans);
            hammockPlan.foundationsPlans.Sort();
        }
    }

    private HammockFoundationPlan GenerateHammockFoundationPlan(HammockPlan hammockPlan, float currentLocalZPosition, bool groundToucher, bool mustAllowForBuildings)
    {
        HammockFoundationPlan hammockFoundationPlan = new HammockFoundationPlan();

        //Determine foundation scale
        float xScale = hammockPlan.hammockWidth;
        float yScale;
        if (groundToucher)
            yScale = (hammockPlan.localTopElevation * 2.0f) + (grounderToucherBonusElevation * 2.0f);
        else
            yScale = Random.Range(31.0f, 45.0f);
        hammockFoundationPlan.scale = new Vector3(xScale, yScale, xScale);

        //Determine foundation's local z position
        hammockFoundationPlan.localZPosition = currentLocalZPosition + hammockFoundationPlan.scale.z / 2.0f;

        //Determine foundation shape
        if (circularFoundations)
            hammockFoundationPlan.shape = mustAllowForBuildings || Random.Range(0, 4) != 0 ? FoundationShape.Circular : FoundationShape.Torus;
        else
            hammockFoundationPlan.shape = FoundationShape.Rectangular;

        //Will this foundation be one of the supports that reaches all the way to the ground?
        hammockFoundationPlan.groundToucher = groundToucher;

        return hammockFoundationPlan;
    }

    private bool DetermineHammockWidth(HammockPlan hammockPlan, float remainingCityWidth, bool reachedEndOfCitySoFar)
    {
        bool reachedEndOfCity = reachedEndOfCitySoFar;

        //Randomize hammock width
        hammockPlan.hammockWidth = Random.Range(75, 175);

        //Perform minimum hammock width check
        if (!metMinimumWidthRequirement)
        {
            if (hammockPlan.hammockWidth < minimumWidthForWidestHammock)
                hammockPlan.hammockWidth = minimumWidthForWidestHammock;

            metMinimumWidthRequirement = true;
        }

        //Perform city limit check
        if (hammockPlan.hammockWidth >= remainingCityWidth)
        {
            reachedEndOfCity = true;
            hammockPlan.hammockWidth = remainingCityWidth;
        }

        return reachedEndOfCity;
    }

    private bool DetermineHammockGapWidth(HammockPlan hammockPlan, float remainingCityWidth, bool reachedEndOfCitySoFar)
    {
        bool reachedEndOfCity = reachedEndOfCitySoFar;

        if (reachedEndOfCity)
            hammockPlan.widthOfGapToRight = 0.0f;
        else
        {
            hammockPlan.widthOfGapToRight = Random.Range(30.0f, 140.0f);

            if(hammockPlan.widthOfGapToRight >= remainingCityWidth)
            {
                reachedEndOfCity = true;
                hammockPlan.widthOfGapToRight = 0.0f;
            }
        }

        return reachedEndOfCity;
    }

    private void GenerateHammocks(List<HammockPlan> hammockPlans)
    {
        for (int hammocksGenerated = 0; hammocksGenerated < hammockPlans.Count; hammocksGenerated++)
        {
            GenerateHammock(hammockPlans[hammocksGenerated]);

            if (hammocksGenerated > 0)
                ConnectHammocks(hammockPlans[hammocksGenerated - 1], hammockPlans[hammocksGenerated]);
        }
    }

    private void GenerateHammock(HammockPlan hammockPlan)
    {
        //Generate the foundations that comprise the hammock
        foreach (HammockFoundationPlan hammockFoundationPlan in hammockPlan.foundationsPlans)
            GenerateHammockFoundation(hammockPlan, hammockFoundationPlan);

        //Generate a single foundation that runs throughout the hammock at middle elevation and acts to hold the rest of the foundations together
        Vector3 hammockBarScale = new Vector3(hammockPlan.hammockWidth * 0.25f, 20.0f, hammockPlan.GetZScaleForHammock() * 0.95f);
        float hammockBarLocalYPosition = hammockPlan.localTopElevation - 10.0f - hammockBarScale.y * 0.5f;
        Vector3 hammockBarLocalPosition = new Vector3(hammockPlan.localXPosition, hammockBarLocalYPosition, 0.0f);
        foundationManager.GenerateNewFoundation(hammockBarLocalPosition, hammockBarScale, FoundationShape.Rectangular, false);

        //Generate the vertical scalers at the ends of the hammock that allow movement to/from ground
        GenerateVerticalScalerForHammock(hammockPlan, true);
        GenerateVerticalScalerForHammock(hammockPlan, false);
    }

    private void GenerateHammockFoundation(HammockPlan hammockPlan, HammockFoundationPlan hammockFoundationPlan)
    {
        //Get some variables we will use later on
        hammockPlan.GetNeighborFoundations(hammockFoundationPlan, out HammockFoundationPlan negZNeighbor, out HammockFoundationPlan posZNeighbor);

        //Determine position
        float hammockFoundationYPosition = hammockFoundationPlan.groundToucher ? 0.0f : hammockPlan.localTopElevation - hammockFoundationPlan.scale.y * 0.5f;
        Vector3 hammockFoundationLocalPosition = new Vector3(hammockPlan.localXPosition, hammockFoundationYPosition, hammockFoundationPlan.localZPosition);

        //At this point we have what we need, so generate the foundation
        Foundation foundation = foundationManager.GenerateNewFoundation(hammockFoundationLocalPosition, hammockFoundationPlan.scale, hammockFoundationPlan.shape, false);

        //If applicable, tell the area system that buildings can spawn on the foundations
        if (!hammockFoundationPlan.groundToucher || grounderToucherBonusElevation < 0.1f)
        {
            int bufferInAreas = 2;
            int areasLong = Mathf.FloorToInt(hammockFoundationPlan.scale.x / city.areaManager.areaSize) - bufferInAreas;
            Vector3 foundationCenterInAreas = city.areaManager.LocalCoordToAreaCoord(foundation.transform.localPosition);
            if (hammockFoundationPlan.shape == FoundationShape.Rectangular) //Rectangular area
            {
                int xStart = Mathf.CeilToInt(foundationCenterInAreas.x - areasLong / 2);
                int zStart = Mathf.CeilToInt(foundationCenterInAreas.z - areasLong / 2);
                city.areaManager.ReserveAreasWithType(xStart, zStart, areasLong, AreaManager.AreaReservationType.Open, AreaManager.AreaReservationType.LackingRequiredFoundation);
            }
            else //Circular area
            {
                int centerX = Mathf.CeilToInt(foundationCenterInAreas.x);
                int centerZ = Mathf.CeilToInt(foundationCenterInAreas.z);
                AreaManager.AreaReservationType newAreaType = hammockFoundationPlan.shape == FoundationShape.Torus ? AreaManager.AreaReservationType.ReservedForExtraPerimeter : AreaManager.AreaReservationType.Open;
                city.areaManager.ReserveAreasWithinThisCircle(centerX, centerZ, areasLong / 2, newAreaType, false, AreaManager.AreaReservationType.LackingRequiredFoundation);
            }
        }
        //Otherwise, this is a ground toucher that we need to turn into a spire (the spires cannot host buildings)
        else
        {
            GenerateAscendingSpire(hammockFoundationLocalPosition, hammockFoundationPlan.scale, hammockFoundationPlan.shape, true);

            //After generating the spire, connect it to the rest of the hammock with vertical scalers
            GenerateVerticalScalerForSpire(hammockFoundationPlan, negZNeighbor, hammockPlan);
            GenerateVerticalScalerForSpire(hammockFoundationPlan, posZNeighbor,  hammockPlan);
        }

        //Generate mid-section bridge for torus foundations
        if (hammockFoundationPlan.shape == FoundationShape.Torus)
        {
            float zOffset = hammockFoundationPlan.scale.z * 0.5f * FoundationManager.torusAnnulusMultiplier;
            Vector3 smallerZBridgePoint = new Vector3(hammockPlan.localXPosition, hammockPlan.localTopElevation, hammockFoundationPlan.localZPosition - zOffset);
            Vector3 biggerZBridgePoint = new Vector3(hammockPlan.localXPosition, hammockPlan.localTopElevation, hammockFoundationPlan.localZPosition + zOffset);

            ConnectPointsWithBridge(smallerZBridgePoint, biggerZBridgePoint);
        }

        //Take care of connecting the foundation to the next foundation if needed
        GenerateCircularConnectorIfApplicable(hammockFoundationPlan, posZNeighbor, hammockPlan);
    }

    private void GenerateCircularConnectorIfApplicable(HammockFoundationPlan negZFoundation, HammockFoundationPlan posZFoundation, HammockPlan hammockPlan)
    {
        //Make sure there are in fact two things to connect (on the edges of hammocks one of these may be null)
        if (negZFoundation == null || posZFoundation == null)
            return;

        //Make sure at least one of the foundations is circular
        bool negZIsCircular = (negZFoundation.shape == FoundationShape.Circular || negZFoundation.shape == FoundationShape.Torus);
        bool posZIsCircular = (posZFoundation.shape == FoundationShape.Circular || posZFoundation.shape == FoundationShape.Torus);
        if (!negZIsCircular && !posZIsCircular)
            return;

        //Make sure there is no height difference between them
        float negZTopElevation = GetLocalTopElevationForFoundation(hammockPlan, negZFoundation);
        float posZTopElevation = GetLocalTopElevationForFoundation(hammockPlan, posZFoundation);
        if (Mathf.Abs(posZTopElevation - negZTopElevation) > 0.1f)
            return;

        //Compute scaling and positioning data for the connector
        Vector3 circularConnectorScale = Vector3.one * Random.Range(25.0f, 35.0f);
        float circularConnectorZPosition = (negZFoundation.localZPosition + posZFoundation.localZPosition) * 0.5f;
        Vector3 circularConnectorPosition = new Vector3(hammockPlan.localXPosition, negZTopElevation - circularConnectorScale.y * 0.5f - 0.002f, circularConnectorZPosition);

        //Finally, place the connector
        foundationManager.GenerateNewFoundation(circularConnectorPosition, circularConnectorScale, FoundationShape.Circular, false);
    }

    private void GenerateAscendingSpire(Vector3 baseLocalPosition, Vector3 baseScale, FoundationShape baseShape, bool firstIteration)
    {
        //Determine new scale
        Vector3 newScale = baseScale * FoundationManager.torusAnnulusMultiplier;
        if (firstIteration)
            newScale.y = newScale.x * 2.0f;

        //If the spire has gotten too small, then stop generation
        if (newScale.x < 25.0f)
            return;

        //Determine new position
        Vector3 newLocalPosition = baseLocalPosition;
        newLocalPosition.y += baseScale.y * 0.5f;

        //At this point we have everything that we need, so generate spire foundation
        foundationManager.GenerateNewFoundation(newLocalPosition, newScale, baseShape, false);

        //Then, recurse
        GenerateAscendingSpire(newLocalPosition, newScale, baseShape, false);
    }

    private void GenerateVerticalScalerForSpire(HammockFoundationPlan spireFoundation, HammockFoundationPlan otherFoundation, HammockPlan hammockPlan)
    {
        //Check if there is indeed something to connect
        if (spireFoundation == null || otherFoundation == null)
            return;

        float localTopElevation = GetLocalTopElevationForFoundation(hammockPlan, spireFoundation);
        float localBottomElevation = GetLocalTopElevationForFoundation(hammockPlan, otherFoundation);

        //Check if there actually is a height difference between them. If there isn't, then we don't need to do anything
        if (localTopElevation - localBottomElevation < 1.0f)
            return;

        //Figure out where everything is in local space
        Vector3 localCenterOfSpire = new Vector3(hammockPlan.localXPosition, localTopElevation, spireFoundation.localZPosition);
        Vector3 localCenterOfOther = new Vector3(hammockPlan.localXPosition, localBottomElevation, otherFoundation.localZPosition);
        Vector3 localCenterOfVerticalScaler = (localCenterOfSpire + localCenterOfOther) * 0.5f; //Should be right in between
        localCenterOfVerticalScaler.y = localBottomElevation;

        //Convert everything from local to global
        float globalTopElevation = localTopElevation + city.transform.position.y;
        float globalBottomElevation = localBottomElevation + city.transform.position.y;
        Vector3 globalCenterOfSpire = city.transform.TransformPoint(localCenterOfSpire);
        Vector3 globalCenterOfVerticalScaler = city.transform.TransformPoint(localCenterOfVerticalScaler);

        //Finally, place the vertical scaler
        city.verticalScalerManager.GenerateVerticalScalerWithFocalPoint(globalCenterOfSpire, globalCenterOfVerticalScaler, globalBottomElevation, globalTopElevation, false);

        //After finally, reserve area around the vertical scaler so that no buildings can spawn there
        Vector3 placeInAreas = city.areaManager.LocalCoordToAreaCoord(city.transform.InverseTransformPoint(globalCenterOfVerticalScaler));
        int radiusInAreas = 20 / city.areaManager.areaSize;
        city.areaManager.ReserveAreasWithinThisCircle((int)placeInAreas.x, (int)placeInAreas.z, radiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);
    }

    private void ConnectHammocks(HammockPlan leftHammock, HammockPlan rightHammock)
    {
        //Gather information about the hammock lengths
        float leftHammockZHalfLength = leftHammock.GetZScaleForHammock() * 0.5f;
        float rightHammockZHalfLength = rightHammock.GetZScaleForHammock() * 0.5f;
        float shorterHalfLength = Mathf.Min(leftHammockZHalfLength, rightHammockZHalfLength);

        //Loop for creating bridges, one bridge per iteration. From back (-z) to front (+z)
        for (float localZPosition = (-shorterHalfLength) + Random.Range(5.0f, 15.0f); localZPosition < shorterHalfLength;)
        {
            //Get the closest foundations on the left and right
            HammockFoundationPlan leftFoundation = leftHammock.GetClosestFoundationToLocalZPosition(localZPosition, out float leftZDistance);
            HammockFoundationPlan rightFoundation = rightHammock.GetClosestFoundationToLocalZPosition(localZPosition, out float rightZDistance);

            //If we couldn't find any foundations, then something is wrong so abort making bridges
            if (leftFoundation == null || rightFoundation == null)
                break;

            //From the foundations, determine the leftside and rightside points to connect with a bridge
            Vector3 leftBridgePoint = GetLocalBridgePoint(leftHammock, leftFoundation, localZPosition, leftZDistance, false);
            Vector3 rightBridgePoint = GetLocalBridgePoint(rightHammock, rightFoundation, localZPosition, rightZDistance, true);

            //Connect the two points either with a normal bridge, or by making a bridge with more floating foundations!
            if(ShouldConnectPointsWithFoundations(leftBridgePoint, rightBridgePoint, leftHammock, rightHammock)) //Make bridge with floating foundations
            {
                float localTopElevationForBridge = GetLocalTopElevationForFoundation(leftHammock, leftFoundation);
                float bridgeFoundationHeight = Mathf.Min(leftFoundation.scale.y, rightFoundation.scale.y);
                ConnectPointsWithFloatingFoundations(leftBridgePoint, rightBridgePoint, localTopElevationForBridge, bridgeFoundationHeight);
            }
            else //Normal bridge
                ConnectPointsWithBridge(leftBridgePoint, rightBridgePoint);

            //We're done with this bridge, now move further forwards (+z) to the next possible bridge location...
            localZPosition = Mathf.Max(localZPosition, leftBridgePoint.z, rightBridgePoint.z); //Make sure we at least go to where the current bridge is
            localZPosition += Mathf.Max(leftFoundation.scale.z, rightFoundation.scale.z); //Then some beyond it
        }
    }

    private bool ShouldConnectPointsWithFoundations(Vector3 leftBridgePoint, Vector3 rightBridgePoint, HammockPlan leftHammock, HammockPlan rightHammock)
    {
        if (Vector3.Distance(leftBridgePoint, rightBridgePoint) < 10.0f)
            return false;

        if (Mathf.Abs(leftBridgePoint.y - rightBridgePoint.y) > 0.1f)
            return false;

        float lowerTopElevation = Mathf.Min(leftHammock.localTopElevation, rightHammock.localTopElevation);

        if (leftBridgePoint.y - lowerTopElevation > 0.1f || rightBridgePoint.y - lowerTopElevation > 0.1f)
            return false;

        return chanceToUseFoundationsForBridges > Random.Range(0.0f, 1.0f);
    }

    private void ConnectPointsWithFloatingFoundations(Vector3 leftLocalBridgePoint, Vector3 rightLocalBridgePoint, float localTopElevation, float foundationHeight)
    {
        //If the foundations are circular, we want to move the bridge points closer to their corresponding hammocks so that there's enough overlap (circles are a little smaller)
        if (circularFoundations)
        {
            leftLocalBridgePoint = Vector3.MoveTowards(leftLocalBridgePoint, rightLocalBridgePoint, -5.0f);
            rightLocalBridgePoint = Vector3.MoveTowards(rightLocalBridgePoint, leftLocalBridgePoint, -5.0f);
        }

        //Determine spacing
        float bridgeWidth = Random.Range(15.0f, 40.0f); //Target bridge width
        float gapLength = Vector3.Distance(leftLocalBridgePoint, rightLocalBridgePoint);
        int bridgesToPlace = Mathf.Max(1, Mathf.FloorToInt(gapLength / bridgeWidth));
        bridgeWidth = gapLength / bridgesToPlace; //Adjust bridge width so that it aligns nicely with gap

        //Figure out other shit before we begin placing things
        float bridgeWidthMultiplier = circularFoundations ? 1.2f : 1.0f;
        Vector3 bridgeFoundationScale = new Vector3(bridgeWidth * bridgeWidthMultiplier, foundationHeight, bridgeWidth * bridgeWidthMultiplier);
        FoundationShape shape = circularFoundations ? FoundationShape.Circular : FoundationShape.Rectangular;

        //Loop for placing foundations. Places one foundation per iteration. From left (-x) to right (+x)
        for (int bridgesPlaced = 0; bridgesPlaced < bridgesToPlace; bridgesPlaced++)
        {
            Vector3 newLocalFoundationPosition = Vector3.MoveTowards(leftLocalBridgePoint, rightLocalBridgePoint, bridgeWidth * (0.5f + bridgesPlaced));
            newLocalFoundationPosition.y -= bridgeFoundationScale.y * 0.5f - Random.Range(0.01f, 0.02f);

            foundationManager.GenerateNewFoundation(newLocalFoundationPosition, bridgeFoundationScale, shape, false);
        }

        //Generate one long crossbar across the gap, through the foundations we just generated to give more structural integrity
        //But, only generate the crossbar if the bridge isn't angled along x or z axes (otherwise crossbar will appear out of place)
        if(Mathf.Abs(leftLocalBridgePoint.z - rightLocalBridgePoint.z) < 5.0f)
        {
            Vector3 crossbarLocationPosition = Vector3.MoveTowards(leftLocalBridgePoint, rightLocalBridgePoint, gapLength * 0.5f);
            crossbarLocationPosition.y -= foundationHeight * 0.5f;
            Vector3 crossbarScale = new Vector3(gapLength * 1.2f, 7.5f, 7.5f);
            foundationManager.GenerateNewFoundation(crossbarLocationPosition, crossbarScale, FoundationShape.Rectangular, false);
        }
    }

    private void ConnectPointsWithBridge(Vector3 leftLocalBridgePoint, Vector3 rightLocalBridgePoint)
    {
        //Tell the bridge manager to connect these two points with a bridge
        BridgeDestination leftDestination = new BridgeDestination(city.transform.TransformPoint(leftLocalBridgePoint), 0.1f);
        BridgeDestination rightDestination = new BridgeDestination(city.transform.TransformPoint(rightLocalBridgePoint), 0.1f);
        city.bridgeManager.AddNewDestinationPairing(new BridgeDestinationPairing(leftDestination, rightDestination));
    }

    private Vector3 GetLocalBridgePoint(HammockPlan hammockPlan, HammockFoundationPlan hammockFoundationPlan, float suggestedZPoint, float zDistanceFromPointToFoundation, bool onLeft)
    {
        Vector3 bridgePointInLocal = Vector3.zero;

        //Set x
        float foundationHalfWidth = hammockFoundationPlan.scale.x * 0.5f;
        bridgePointInLocal.x = hammockPlan.localXPosition + (onLeft ? -foundationHalfWidth : foundationHalfWidth);

        //Set y
        bridgePointInLocal.y = GetLocalTopElevationForFoundation(hammockPlan, hammockFoundationPlan);

        //Set z
        if (zDistanceFromPointToFoundation < 0.1f && hammockFoundationPlan.shape == FoundationShape.Rectangular)
            bridgePointInLocal.z = suggestedZPoint;
        else
            bridgePointInLocal.z = hammockFoundationPlan.localZPosition;

        //We done
        return bridgePointInLocal;
    }

    private float GetLocalTopElevationForFoundation(HammockPlan hammockPlan, HammockFoundationPlan hammockFoundationPlan)
    {
        if (hammockFoundationPlan.groundToucher && grounderToucherBonusElevation > 0.1f)
            return hammockPlan.localTopElevation + grounderToucherBonusElevation;
        else
            return hammockPlan.localTopElevation;
    }

    private void GenerateVerticalScalerForHammock(HammockPlan hammockPlan, bool negZEnd)
    {
        HammockFoundationPlan endFoundation = hammockPlan.foundationsPlans[negZEnd ? 0 : hammockPlan.foundationsPlans.Count - 1];

        //Figure out where everything is
        float localTopElevation = GetLocalTopElevationForFoundation(hammockPlan, endFoundation);
        Vector3 localFoundationCenter = new Vector3(hammockPlan.localXPosition, localTopElevation, endFoundation.localZPosition);
        float foundationToEdgeZDelta = endFoundation.scale.z * 0.5f * (negZEnd ? -1.0f : 1.0f);
        Vector3 localVerticalScalerPoint = localFoundationCenter + Vector3.forward * foundationToEdgeZDelta;
        localVerticalScalerPoint.y = 0.0f;

        //Convert from local to global
        Vector3 globalFoundationCenter = city.transform.TransformPoint(localFoundationCenter);
        Vector3 globalVerticalScalerPoint = city.transform.TransformPoint(localVerticalScalerPoint);
        float globalTopElevation = city.transform.position.y + localTopElevation;
        //float globalBottomElevation = city.transform.position.y;
        float globalBottomElevation = PlanetTerrain.planetTerrain.SnapToTerrainAndFlattenAreaAroundPoint(globalVerticalScalerPoint.x, globalVerticalScalerPoint.z).y;

        //Finally, generate the damn thing
        city.verticalScalerManager.GenerateVerticalScalerWithFocalPoint(globalFoundationCenter, globalVerticalScalerPoint, globalBottomElevation, globalTopElevation, false);
    }


    //Helper classes---


    //Holds the plans needed to generate a single hammock.
    public class HammockPlan
    {
        public float localXPosition, localTopElevation, hammockWidth, widthOfGapToRight;
        public List<HammockFoundationPlan> foundationsPlans;

        public float GetZScaleForHammock()
        {
            if (foundationsPlans == null || foundationsPlans.Count == 0)
                return 0.0f;

            HammockFoundationPlan leftmostFoundationPlan = foundationsPlans[0];
            return -leftmostFoundationPlan.localZPosition * 2.0f;
        }

        public HammockFoundationPlan GetClosestFoundationToLocalZPosition(float localZPositionToSearch, out float zDistanceToClosestFoundation)
        {
            zDistanceToClosestFoundation = Mathf.Infinity;

            if (foundationsPlans != null && foundationsPlans.Count > 0)
            {
                HammockFoundationPlan closestFoundation = null;
                foreach (HammockFoundationPlan foundationUnderInspection in foundationsPlans)
                {
                    float zDistanceToFoundation = foundationUnderInspection.DistanceToContainingLocalZPosition(localZPositionToSearch);

                    if (zDistanceToFoundation < zDistanceToClosestFoundation)
                    {
                        zDistanceToClosestFoundation = zDistanceToFoundation;
                        closestFoundation = foundationUnderInspection;
                    }
                }

                return closestFoundation;
            }

            return null;
        }

        //This requires that foundationPlans be sorted by localZPosition
        public void GetNeighborFoundations(HammockFoundationPlan relativeTo, out HammockFoundationPlan negZNeighbor, out HammockFoundationPlan posZNeighbor)
        {
            negZNeighbor = null;
            posZNeighbor = null;

            int indexOf = foundationsPlans.IndexOf(relativeTo);
            if (indexOf < 0)
                return;

            if (indexOf > 0)
                negZNeighbor = foundationsPlans[indexOf - 1];

            if (indexOf + 1 < foundationsPlans.Count)
                posZNeighbor = foundationsPlans[indexOf + 1];
        }
    }


    //Holds the plans needed to generate a single foundation within a hammock. A hammock can (and almost certainly will) have multiple foundations.
    public class HammockFoundationPlan : System.IComparable
    {
        public Vector3 scale;
        public float localZPosition;
        public FoundationShape shape;
        public bool groundToucher;

        public HammockFoundationPlan() { }

        public HammockFoundationPlan(HammockFoundationPlan other)
        {
            scale = other.scale;
            localZPosition = other.localZPosition;
            shape = other.shape;
            groundToucher = other.groundToucher;
        }

        //If the provided z position is contained within the foundation, returns 0.
        //Else, it returns the shortest distance from the point to the foundation's boundaries.
        public float DistanceToContainingLocalZPosition(float localZPositionToSearch)
        {
            float zHalfLength = scale.z * 0.5f;
            float negativeZExtent = localZPosition - zHalfLength;
            float positiveZExtent = localZPosition + zHalfLength;

            if (negativeZExtent <= localZPositionToSearch && positiveZExtent >= localZPositionToSearch)
                return 0.0f;
            else if (localZPositionToSearch < negativeZExtent)
                return negativeZExtent - localZPositionToSearch;
            else
                return localZPositionToSearch - positiveZExtent;
        }

        public int CompareTo(object obj)
        {
            HammockFoundationPlan otherFoundationPlan = obj as HammockFoundationPlan;
            if (localZPosition < otherFoundationPlan.localZPosition)
                return -1;
            else
                return 1;
        }
    }
}
