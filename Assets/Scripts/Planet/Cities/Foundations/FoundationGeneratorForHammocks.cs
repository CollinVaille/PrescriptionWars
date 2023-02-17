using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForHammocks
{
    //References
    private City city;
    private FoundationManager foundationManager;

    //Status variables used across multiple functions
    private int minimumWidthForWidestHammock = 0;
    private bool metMinimumWidthRequirement = false;
    private float grounderToucherBonusHeight = 50.0f;

    //Main access functions---

    public FoundationGeneratorForHammocks(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;
    }

    public void GenerateNewHammockFoundations()
    {
        //We want to be able to control where the walls are ourselves here
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = false;

        //Hammocks need to be high up in the air
        foundationManager.foundationHeight += Random.Range(60, 80);

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

        //Loop for generating hammock PLANS. One hammock plan generated per iteration, moving from left (-x) to right (+x)
        for (float currentLocalXPosition = -cityHalfLength; currentLocalXPosition < cityHalfLength;)
        {
            //First, gather some information needed to decide on plans
            float remainingCityWidth = cityHalfLength - currentLocalXPosition;

            //If we realize we're running out of room, then abandon any further foundation plans
            if (remainingCityWidth < 50.0f)
                break;

            //Then, use that information to determine our plans here
            HammockPlan newHammockPlan;
            newHammockPlan = GenerateHammockPlan(currentLocalXPosition, remainingCityWidth, cityLength);

            //Add it to the list of plans
            hammockPlans.Add(newHammockPlan);

            //Move the placehead to the right per the plan
            currentLocalXPosition += newHammockPlan.hammockWidth + newHammockPlan.widthOfGapToRight;
        }

        return hammockPlans;
    }

    private HammockPlan GenerateHammockPlan(float currentLocalXPosition, float remainingCityWidth, float hammockLength)
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

            //If we realize we're running out of room, then abandon any further foundation plans
            if (remainingHammockLength < 50.0f)
                break;

            //Then, use that information to determine our plans here
            HammockFoundationPlan newHammockFoundationPlan;
            newHammockFoundationPlan = GenerateHammockFoundationPlan(hammockPlan, currentLocalZPosition, hammockPlan.foundationsPlans.Count == 0);

            //Add it to the list of plans
            hammockPlan.foundationsPlans.Add(newHammockFoundationPlan);

            //Move the placehead to the right per the plan
            currentLocalZPosition += newHammockFoundationPlan.scale.z;
        }

        ReflectFoundationPlans(hammockPlan);

        return hammockPlan;
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

        //Add the copies to the foundation plans
        if(copyPlans.Count > 0)
            hammockPlan.foundationsPlans.AddRange(copyPlans);
    }

    private HammockFoundationPlan GenerateHammockFoundationPlan(HammockPlan hammockPlan, float currentLocalZPosition, bool groundToucher)
    {
        HammockFoundationPlan hammockFoundationPlan = new HammockFoundationPlan();

        //Determine foundation scale
        float xScale = hammockPlan.hammockWidth;
        float yScale;
        if (groundToucher)
            yScale = (hammockPlan.localTopElevation * 2.0f) + grounderToucherBonusHeight;
        else
            yScale = Random.Range(31.0f, 45.0f);
        hammockFoundationPlan.scale = new Vector3(xScale, yScale, xScale);

        //Determine foundation's local z position
        hammockFoundationPlan.localZPosition = currentLocalZPosition + hammockFoundationPlan.scale.z / 2.0f;

        //Determine foundation shape
        hammockFoundationPlan.shape = (Random.Range(1, 2) == 0) ? FoundationShape.Circular : FoundationShape.Rectangular;

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
        for (int x = 0; x < hammockPlans.Count; x++)
            GenerateHammock(hammockPlans[x]);
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
    }

    private void GenerateHammockFoundation(HammockPlan hammockPlan, HammockFoundationPlan hammockFoundationPlan)
    {
        //Determine position
        float hammockFoundationYPosition = hammockFoundationPlan.groundToucher ? 0.0f : hammockPlan.localTopElevation - hammockFoundationPlan.scale.y * 0.5f;
        Vector3 hammockFoundationLocalPosition = new Vector3(hammockPlan.localXPosition, hammockFoundationYPosition, hammockFoundationPlan.localZPosition);

        //At this point we have what we need, so generate the foundation
        Foundation foundation = foundationManager.GenerateNewFoundation(hammockFoundationLocalPosition, hammockFoundationPlan.scale, hammockFoundationPlan.shape, false);

        //Tell the area system that buildings can spawn on the foundations
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
            city.areaManager.ReserveAreasWithinThisCircle(centerX, centerZ, areasLong / 2, AreaManager.AreaReservationType.Open, false, AreaManager.AreaReservationType.LackingRequiredFoundation);
        }
    }
}


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
}


//Holds the plans needed to generate a single foundation within a hammock. A hammock can (and almost certainly will) have multiple foundations.
public class HammockFoundationPlan
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
}