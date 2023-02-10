using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager
{
    public enum AreaReservationType { Open, ReservedByRoad, ReservedByBuilding, LackingRequiredFoundation, ReservedForExtraPerimeter }

    private City city;

    //Area/zoning system (how city is subdivided and organized)
    [HideInInspector] public int areaSize = 5;
    [HideInInspector] public AreaReservationType[,] areaTaken;
    [HideInInspector] public List<CityBlock> availableCityBlocks;
    [HideInInspector] public List<int> horizontalRoads, verticalRoads; //Even index entries denote start point of road, odd index entries indicate end point of previous index's road


    public AreaManager (City city)
    {
        this.city = city;
    }

    public void InitializeAreaReservationSystem()
    {
        //Initialize 2D array and set all values to OPEN
        areaTaken = new AreaReservationType[city.radius * 2 / areaSize, city.radius * 2 / areaSize];
        ReserveAllAreasRegardlessOfType(AreaReservationType.Open);
    }

    public void GenerateRoads()
    {
        //Needed set up regardless of what kind of roads we want to generate
        horizontalRoads = new List<int>();
        verticalRoads = new List<int>();
        availableCityBlocks = new List<CityBlock>();
        Vector2Int roadWidthRange = new Vector2Int(5, 20);
        Vector2Int roadSpacingRange = new Vector2Int(40, city.buildingManager.GetMaxLengthBetweenRoadsInLocalUnits());

        //Choose and execute specific kind of road generation
        if (city.circularCity)
            GenerateConcentricCircleRoads(roadWidthRange, roadSpacingRange);
        else
            GenerateCrosshatchRoads(roadWidthRange, roadSpacingRange);
    }

    private void GenerateConcentricCircleRoads(Vector2Int roadWidthRange, Vector2Int roadSpacingRange)
    {
        //Make some precomputations
        int cityRadiusInAreas = areaTaken.GetLength(0) / 2;

        roadWidthRange /= areaSize;
        if (roadWidthRange.x < 1)
            roadWidthRange.x = 1;
        if (roadWidthRange.y < 1)
            roadWidthRange.y = 1;

        roadSpacingRange /= areaSize;
        if (roadSpacingRange.x < 1)
            roadSpacingRange.x = 1;
        if (roadSpacingRange.y < 1)
            roadSpacingRange.y = 1;

        //Make a single, centered road for both the horizontal and vertical directions that cuts through the diameter of the city
        Vector2Int cardinalRoadWidths = new Vector2Int(roadWidthRange.y, roadWidthRange.y) * areaSize;
        GenerateStraightCardinalRoad(true, cityRadiusInAreas, cardinalRoadWidths, true);
        GenerateStraightCardinalRoad(false, cityRadiusInAreas, cardinalRoadWidths, true);

        //Make a series of concentric circle roads separate by concentric circle blocks. If the city is small enough, don't make any such roads (all will be buildings)
        if (roadWidthRange.y + roadSpacingRange.x >= cityRadiusInAreas)
            return;

        for (int areasOut = Random.Range(roadSpacingRange.x, roadSpacingRange.y); areasOut < cityRadiusInAreas;)
        {
            //Generate a layer of road
            int newRoadWidth = Random.Range(roadWidthRange.x, roadWidthRange.y);
            GenerateConcentricCircleRoad(cityRadiusInAreas, areasOut, areasOut + newRoadWidth);
            areasOut += newRoadWidth;

            //Generate a layer of buildings -- a block
            areasOut += Random.Range(roadSpacingRange.x, roadSpacingRange.y);
        }

    }

    //Generates a single road that is a concentric circle around the center of the city.
    //The road is defined by the min (inclusive) and max (exclusive) radii passed in as parameters.
    private void GenerateConcentricCircleRoad(int cityRadiusInAreas, int minRadiusInAreas, int maxRadiusInAreas)
    {
        for (int x = 0, xDistanceFromCenter = 0; x < areaTaken.GetLength(0); x++)
        {
            xDistanceFromCenter = Mathf.Abs(cityRadiusInAreas - x);
            for (int z = 0, zDistanceFromCenter = 0; z < areaTaken.GetLength(1); z++)
            {
                zDistanceFromCenter = Mathf.Abs(cityRadiusInAreas - z);
                float pointRadius = Mathf.Sqrt(xDistanceFromCenter * xDistanceFromCenter + zDistanceFromCenter * zDistanceFromCenter);

                if (pointRadius >= minRadiusInAreas && pointRadius < maxRadiusInAreas)
                    areaTaken[x, z] = AreaReservationType.ReservedByRoad;
            }
        }
    }

    private void GenerateCrosshatchRoads(Vector2Int roadWidthRange, Vector2Int roadSpacingRange)
    {
        List<CityBlock> horizontalStrips = new List<CityBlock>();

        //Horizontal roads
        for (int z = 0; z < areaTaken.GetLength(1);)
        {
            int roadWidth = GenerateStraightCardinalRoad(true, z, roadWidthRange, false);

            CityBlock newHorizontalStrip = new CityBlock();
            newHorizontalStrip.coords.y = z + roadWidth;
            newHorizontalStrip.dimensions.y = Mathf.Max(Random.Range(roadSpacingRange.x, roadSpacingRange.y) / areaSize, 1);
            horizontalStrips.Add(newHorizontalStrip);

            z += newHorizontalStrip.dimensions.y;
        }

        //Vertical roads
        for (int x = 0; x < areaTaken.GetLength(0);)
        {
            int roadWidth = GenerateStraightCardinalRoad(false, x, roadWidthRange, false);
            int nextGap = Mathf.Max(Random.Range(roadSpacingRange.x, roadSpacingRange.y) / areaSize, 1);

            foreach (CityBlock horizontalStrip in horizontalStrips)
            {
                CityBlock newBlock = new CityBlock(horizontalStrip);
                newBlock.coords.x = x + roadWidth;
                //Debug.Log(newBlock.coords.x);
                newBlock.dimensions.x = nextGap;
                availableCityBlocks.Add(newBlock);
            }

            x += nextGap;
        }
    }

    //The start coord will represent the left/bottom edge of the road unless the parameter centerOnStartCoord is true.
    //These roads are straight lines that going in the cardinal directions. Either up/down (north/south) or east/west (left/right).
    private int GenerateStraightCardinalRoad(bool horizontal, int startCoord, Vector2Int roadWidthRange, bool centerOnStartCoord)
    {
        int areasWide = Mathf.Max(1, Random.Range(roadWidthRange.x, roadWidthRange.y) / areaSize);
        if (centerOnStartCoord)
            startCoord -= areasWide / 2;

        if (horizontal) //Horizontal road
        {
            horizontalRoads.Add(startCoord);
            horizontalRoads.Add(startCoord + areasWide);

            for (int x = 0; x < areaTaken.GetLength(0); x++)
            {
                for (int z = startCoord; z < startCoord + areasWide; z++)
                {
                    if (z < areaTaken.GetLength(1))
                        areaTaken[x, z] = AreaReservationType.ReservedByRoad;
                }
            }
        }
        else //Vertical road
        {
            verticalRoads.Add(startCoord);
            verticalRoads.Add(startCoord + areasWide);

            for (int z = 0; z < areaTaken.GetLength(1); z++)
            {
                for (int x = startCoord; x < startCoord + areasWide; x++)
                {
                    if (x < areaTaken.GetLength(0))
                        areaTaken[x, z] = AreaReservationType.ReservedByRoad;
                }
            }
        }

        return areasWide;
    }

    public void GetWidestCardinalRoad(bool searchVerticalRoads, bool skipFirstRoad, out float widestRoadWidth, out float widestRoadCenteredAt)
    {
        float cityWidth = areaSize * areaTaken.GetLength(0);
        widestRoadWidth = 0.0f;
        widestRoadCenteredAt = 0.0f;

        //Find the largest road/gap
        if (searchVerticalRoads)
        {
            float startX = -cityWidth / 2.0f + 5;
            for (int x = skipFirstRoad ? 2 : 0; x < verticalRoads.Count; x += 2)
            {
                int gapSize = (verticalRoads[x + 1] - verticalRoads[x]) * areaSize;

                if (gapSize > widestRoadWidth)
                {
                    widestRoadWidth = gapSize;
                    widestRoadCenteredAt = (startX + verticalRoads[x] * areaSize) + gapSize / 2.0f;
                }
            }
        }
        else
        {
            float startZ = -cityWidth / 2.0f + 5;
            for (int z = skipFirstRoad ? 2 : 0; z < horizontalRoads.Count; z += 2)
            {
                int gapSize = (horizontalRoads[z + 1] - horizontalRoads[z]) * areaSize;

                if (gapSize > widestRoadWidth)
                {
                    widestRoadWidth = gapSize;
                    widestRoadCenteredAt = (startZ + horizontalRoads[z] * areaSize) + gapSize / 2.0f;
                }
            }
        }
    }

    public int GetAverageLengthOfCityBlockInLocalUnits()
    {
        if (availableCityBlocks.Count == 0)
            return 0;

        int avgLength = 0;
        for (int x = 0; x < availableCityBlocks.Count; x++)
        {
            /* //For debugging where the city blocks are and what their dimensions are. Just add "GameObject doodad;" at the top of the file and assign it
            Vector3 areaCoord = Vector3.zero;
            areaCoord.x = availableCityBlocks[x].coords.x;
            areaCoord.z = availableCityBlocks[x].coords.y;
            GameObject dud = Instantiate(doodad, transform);
            dud.transform.localRotation = Quaternion.Euler(0, 0, 0);
            dud.transform.localPosition = AreaCoordToLocalCoord(areaCoord);
            dud.name = "Block " + (x + 1);
            Debug.Log(dud.name + " - " + availableCityBlocks[x].dimensions * areaSize);
            */

            avgLength += availableCityBlocks[x].GetSmallestDimension();
        }

        avgLength /= availableCityBlocks.Count;
        return avgLength * areaSize;
    }

    public bool SafeToGenerate(int startX, int startZ, int areasLong, AreaReservationType safeType, bool overrideRoads)
    {
        for (int x = startX; x < startX + areasLong; x++)
        {
            for (int z = startZ; z < startZ + areasLong; z++)
            {
                //Lower boundaries
                if (x < 0 || z < 0)
                    return false;

                //Upper boundaries
                if (x >= areaTaken.GetLength(0) || z >= areaTaken.GetLength(1))
                    return false;

                //Debug.Log("Length: " + areaTaken.Length);
                //Debug.Log("X: " + x + ", Z: " + z + ", Width: " + areaTaken.GetLength(0) + ", Depth: " + areaTaken.GetLength(1));

                //Occupation check
                if (areaTaken[x, z] != safeType && (!overrideRoads || areaTaken[x, z] != AreaReservationType.ReservedByRoad))
                    return false;

                //Circular boundary
                if (city.circularCity && !IsWithinCircularCityPerimeter(x, z))
                    return false;
            }
        }

        return true;
    }

    public void ReserveAreasWithType(int startX, int startZ, int areasLong, AreaReservationType newType, AreaReservationType oldType)
    {
        ReserveTheseAreas(startX, startZ, areasLong, newType, false, oldType);
    }

    public void ReserveAreasRegardlessOfType(int startX, int startZ, int areasLong, AreaReservationType newType)
    {
        ReserveTheseAreas(startX, startZ, areasLong, newType, true, AreaReservationType.Open);
    }

    public void ReserveAllAreasWithType(AreaReservationType newType, AreaReservationType oldType)
    {
        ReserveTheseAreas(0, 0, areaTaken.GetLength(0), newType, false, oldType);
    }

    public void ReserveAllAreasRegardlessOfType(AreaReservationType newType)
    {
        ReserveTheseAreas(0, 0, areaTaken.GetLength(0), newType, true, AreaReservationType.Open);
    }

    private void ReserveTheseAreas(int startX, int startZ, int areasLong, AreaReservationType newType, bool overrideRegardlessOfType, AreaReservationType oldType)
    {
        for (int x = startX; x < startX + areasLong; x++)
        {
            for (int z = startZ; z < startZ + areasLong; z++)
            {
                if(overrideRegardlessOfType || areaTaken[x, z] == oldType)
                    areaTaken[x, z] = newType;
            }
        }
    }

    public void ReserveAreasWithinThisCircle(int centerX, int centerZ, int radiusInAreas, AreaReservationType newType, bool overrideRegardlessOfType, AreaReservationType oldType)
    {
        int startX = centerX - radiusInAreas;
        int startZ = centerZ - radiusInAreas;
        int areasLong = radiusInAreas * 2;

        for (int x = startX; x < startX + areasLong; x++)
        {
            //X boundaries
            if (x < 0 || x >= areaTaken.GetLength(0))
                continue;

            for (int z = startZ; z < startZ + areasLong; z++)
            {
                //Z boundaries
                if (z < 0 || z >= areaTaken.GetLength(1))
                    continue;

                //Reserve the area if conditions match
                if (overrideRegardlessOfType || areaTaken[x, z] == oldType)
                {
                    if (Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(z - centerZ, 2)) < radiusInAreas)
                        areaTaken[x, z] = newType;
                }
            }
        }
    }

    //This may have a bug where it doesn't check the whole sector before reserving it, just the corner.
    //Please fix before using.
    private Vector3 ReserveSector()
    {
        //Default case where there are not multiple roads to form sectors with (return center of city, no reservations)
        if (horizontalRoads.Count < 4 || verticalRoads.Count < 4)
            return Vector3.zero;

        //Variable initialization
        int horizontalStartRoad = 0, verticalStartRoad = 0;
        int horizontalStartArea = 0, verticalStartArea = 0;
        int horizontalEndArea = 0, verticalEndArea = 0;

        //Select random sector that hasn't been reserved yet...
        int attempt = 0;
        do
        {
            attempt++;

            //Determine horizontal road sector starts at
            horizontalStartRoad = Random.Range(0, horizontalRoads.Count - 2);
            if (horizontalStartRoad % 2 == 0)
                horizontalStartRoad++;

            //Determine vertical road sector starts at
            verticalStartRoad = Random.Range(0, verticalRoads.Count - 2);
            if (verticalStartRoad % 2 == 0)
                verticalStartRoad++;

            //Compute boundaries in terms of city areas...
            horizontalStartArea = verticalRoads[verticalStartRoad] + 1;
            verticalStartArea = horizontalRoads[horizontalStartRoad] + 1;
            horizontalEndArea = verticalRoads[verticalStartRoad + 1];
            verticalEndArea = horizontalRoads[horizontalStartRoad + 1];
        }
        while (attempt <= 50 && areaTaken[horizontalStartArea, verticalStartArea] != AreaReservationType.Open);

        //Give up
        if (areaTaken[horizontalStartArea, verticalStartArea] != AreaReservationType.Open)
            return Vector3.zero;

        //Reserve areas within sector
        for (int x = horizontalStartArea; x < horizontalEndArea; x++)
        {
            for (int z = verticalStartArea; z < verticalEndArea; z++)
                areaTaken[x, z] = AreaReservationType.ReservedByBuilding;
        }

        //Return center of sector
        Vector3 sectorCenter = Vector3.zero;
        sectorCenter.x = horizontalStartArea + (horizontalEndArea - horizontalStartArea) * 0.5f;
        sectorCenter.z = verticalStartArea + (verticalEndArea - verticalStartArea) * 0.5f;
        return AreaCoordToLocalCoord(sectorCenter);
    }

    public Vector3 AreaCoordToLocalCoord(Vector3 areaCoord)
    {
        //Used to be / 2 (int math)
        areaCoord.x = (areaCoord.x - areaTaken.GetLength(0) * 0.5f) * areaSize;
        areaCoord.z = (areaCoord.z - areaTaken.GetLength(1) * 0.5f) * areaSize;

        return areaCoord;
    }

    public Vector3 LocalCoordToAreaCoord(Vector3 localCoord)
    {
        localCoord.x = (localCoord.x / areaSize) + (areaTaken.GetLength(0) * 0.5f);
        localCoord.z = (localCoord.z / areaSize) + (areaTaken.GetLength(1) * 0.5f);

        return localCoord;
    }

    //Usually cities are a rectangle where building placement is restricted by a city width and height.
    //This adds another requirement that buildings be within a certain radius from the city center.
    public bool IsWithinCircularCityPerimeter(int x, int z)
    {
        return AreaCoordToLocalCoord(new Vector3(x, 0, z)).magnitude < city.radius;
    }

    public static float CalculateAreaFromDimensions(bool circular, float halfLength) { return circular ? Mathf.PI * Mathf.Pow(halfLength, 2) : Mathf.Pow(halfLength * 2, 2); }
}
