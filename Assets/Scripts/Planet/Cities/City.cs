using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour, INavZoneUpdater
{
    private enum AreaReservationType { Open, ReservedByRoad, ReservedByBuilding }

    //General
    [HideInInspector] public int radius = 100;
    public GameObject mapMarkerPrefab;

    //Building construction
    public List<GameObject> buildingPrefabs;
    private Building[] buildingPrototypes; //First instance of each building (parallel array to one above)
    private int totalSpawnChance = 0;

    //Building maintenance
    private int nextAvailableBuilding = 0;
    private int nextAvailableBed = -1;
    [HideInInspector] public List<Building> buildings;

    //HOA rules
    public Material defaultWallMaterial, defaultFloorMaterial;
    public Material[] wallMaterials, floorMaterials;

    //Walls
    public GameObject wallSectionPrefab, horGatePrefab, verGatePrefab, fencePostPrefab;
    [HideInInspector] public Transform walls;
    [HideInInspector] public int wallMaterialIndex;
    public Material cityWallMaterial;

    //Area/zoning system (how city is subdivided and organized)
    private int areaSize = 5;
    private AreaReservationType[,] areaTaken;
    private List<CityBlock> availableCityBlocks;
    private List<int> horizontalRoads, verticalRoads; //Even index entries denote start point of road, odd index entries indicate end point of previous index's road
    [HideInInspector] public bool circularCity = false;
    [HideInInspector] public bool elevatedCity = false;

    //Called after city has been generated or regenerated
    public void OnCityStart()
    {
        //Create marker for city on the planet map
        MapMarker mapMarker = Instantiate(mapMarkerPrefab).GetComponent<MapMarker>();
        mapMarker.InitializeMarker(transform);
    }

    private void InitializeAreaReservationSystem()
    {
        //Initialize 2D array and set all values to OPEN
        areaTaken = new AreaReservationType[radius * 2 / areaSize, radius * 2 / areaSize];
        for(int x = 0; x < areaTaken.GetLength(0); x++)
        {
            for (int y = 0; y < areaTaken.GetLength(1); y++)
                areaTaken[x, y] = AreaReservationType.Open;
        }
    }

    public void ReserveTerrainLocation()
    {
        //Reserve location for city
        Vector3 cityLocation = PlanetTerrain.planetTerrain.ReserveTerrainPosition(Random.Range(0, 10),
            (int)PlanetTerrain.planetTerrain.GetSeabedHeight() + Random.Range(0, 4), 500, (int)(radius * 1.2f), true);

        //Place city at slight offset to location
        cityLocation.x -= 10;
        cityLocation.z -= 10;

        transform.position = cityLocation;
    }

    public void GenerateCity()
    {
        //Reserve terrain location
        //radius = Random.Range(40, 100);
        //radius = Random.Range(40, 60);
        //radius = Random.Range(70, 110); //Small city
        radius = Random.Range(80, 130);
        //radius = Random.Range(150, 300); //Huge city
        InitializeAreaReservationSystem();
        ReserveTerrainLocation();

        //Determine city type, which determines whether we have walls, fence posts...
        //what buildings and building materials are used etc...
        CityGenerator.generator.CustomizeCity(this);

        //Early on, let's get references to our building prototypes so we can reference them anywere below (needed for GenerateRoads())
        LoadBuildingPrototypes();

        //Generate roads (and city blocks)
        GenerateRoads();
        availableCityBlocks.Sort(); //Sort the city blocks, smallest to largest
        int avgBlockLength = GetAverageLengthOfCityBlockInLocalUnits();

        //for (int x = 0; x < Random.Range(0, radius / 40); x++)
        //    ReserveSector();

        //Generate foundations
        if(elevatedCity)
            GenerateCityFoundations();

        //Generate buildings
        buildings = new List<Building>();
        GenerateSpecialBuildings();
        GenerateGenericBuildings(avgBlockLength);

        //Generate walls
        if (wallSectionPrefab)
            GenerateWalls();

        //Debug.Log("City radius: " + radius + ", buildings: " + buildings.Count);

        //After the city has been generated, build the nav mesh to pathfind through it
        GenerateNavMesh();

        //Set the name after all possible influencing factors on the name have been set
        gameObject.name = CityGenerator.GenerateCityName(Planet.planet.biome, radius);

        OnCityStart();
    }

    private void LoadBuildingPrototypes()
    {
        buildingPrototypes = new Building[buildingPrefabs.Count];

        for (int x = 0; x < buildingPrefabs.Count; x++)
        {
            buildingPrototypes[x] = Instantiate(buildingPrefabs[x]).GetComponent<Building>();
            buildingPrototypes[x].gameObject.SetActive(false);

            if(!buildingPrototypes[x].CompareTag("Special Building"))
                totalSpawnChance += buildingPrototypes[x].spawnChance;
        }
    }

    private Transform InstantiateNewBuilding(int buildingIndex)
    {
        if (!buildingPrototypes[buildingIndex].gameObject.activeSelf) //Is model home available?
        {
            buildingPrototypes[buildingIndex].gameObject.SetActive(true);
            return buildingPrototypes[buildingIndex].transform;
        }
        else //Fine, we'll create a new one
            return Instantiate(buildingPrefabs[buildingIndex]).transform;
    }

    private void GenerateRoads()
    {
        //Needed set up regardless of what kind of roads we want to generate
        horizontalRoads = new List<int>();
        verticalRoads = new List<int>();
        availableCityBlocks = new List<CityBlock>();
        Vector2Int roadWidthRange = new Vector2Int(5, 20);
        Vector2Int roadSpacingRange = new Vector2Int(40, GetMaxLengthBetweenRoadsInLocalUnits());

        //Choose and execute specific kind of road generation
        if (circularCity)
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

        for(int areasOut = Random.Range(roadSpacingRange.x, roadSpacingRange.y); areasOut < cityRadiusInAreas;)
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
        for(int x = 0, xDistanceFromCenter = 0; x < areaTaken.GetLength(0); x++)
        {
            xDistanceFromCenter = Mathf.Abs(cityRadiusInAreas - x);
            for(int z = 0, zDistanceFromCenter = 0; z < areaTaken.GetLength(1); z++)
            {
                zDistanceFromCenter = Mathf.Abs(cityRadiusInAreas - z);
                float pointRadius = Mathf.Sqrt(xDistanceFromCenter * xDistanceFromCenter + zDistanceFromCenter * zDistanceFromCenter);

                if(pointRadius >= minRadiusInAreas && pointRadius < maxRadiusInAreas)
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

            foreach(CityBlock horizontalStrip in horizontalStrips)
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

    private int GetMaxLengthBetweenRoadsInLocalUnits()
    {
        int longestBuildingLength = 0;
        foreach(Building building in buildingPrototypes)
        {
            if (longestBuildingLength < building.length)
                longestBuildingLength = building.length;
        }

        return Mathf.Max(70, (int)(longestBuildingLength * 1.5f));
    }
    
    private int GetAverageLengthOfCityBlockInLocalUnits()
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

    private void GenerateCityFoundations()
    {
        GameObject foundationPrefab;
        if (circularCity)
            foundationPrefab = Resources.Load<GameObject>("Planet/City/Miscellaneous/Circular Foundation");
        else
            foundationPrefab = Resources.Load<GameObject>("Planet/City/Miscellaneous/Rectangular Foundation");

        Transform foundation = Instantiate(foundationPrefab, transform).transform;
        foundation.localRotation = Quaternion.Euler(0, 0, 0);
        foundation.localPosition = Vector3.zero;
        foundation.localScale = Vector3.one * radius * 2.1f;
    }

    private void GenerateSpecialBuildings()
    {
        for(int x = 0; x < buildingPrototypes.Length; x++)
        {
            //For each special building that is supposed to be included in the city
            if(buildingPrototypes[x].CompareTag("Special Building"))
            {
                //Keep trying to generate it until we succeed or hit 50 attempts
                for(int attempt = 1; attempt <= 50; attempt++)
                {
                    if (GenerateBuilding(x, true, true))
                        break;
                }
            }
        }
    }

    private void GenerateGenericBuildings(int averageBlockLength)
    {
        //Go through all the large generic buildings and give each once chance to spawn early (so they doesn't get crowded out by a bunch of small buildings)
        for(int x = 0; x < buildingPrototypes.Length; x++)
        {
            if (buildingPrototypes[x].CompareTag("Special Building"))
                continue;

            if (buildingPrototypes[x].length > averageBlockLength)
                GenerateBuilding(x, true, false);
        }

        int buildingIndex = -1;
        for (int x = 0; x < 400; x++)
        {
            //Randomly pick a model to build
            buildingIndex = SelectGenericBuildingPrototype();

            //Attempt to place it somewhere
            GenerateBuilding(buildingIndex, buildingPrototypes[buildingIndex].length > averageBlockLength, false);
        }
    }

    //Used to generate a NEW building. Pass in index of model if particular one is desired, else a random model will be selected.
    //Specify aggressive placement to ignore roads--if necessary--during placement. The algorithm will still try to take roads into account if it can.
    //Returns whether building was successfully generated.
    private bool GenerateBuilding(int buildingIndex, bool placeInLargestAvailableBlock, bool overrideRoadsIfNeeded)
    {

        //Find place that can fit model...

        int newX = 0, newZ = 0;
        int areaLength = Mathf.CeilToInt(buildingPrototypes[buildingIndex].length * 1.0f / areaSize);
        bool foundPlace = false;

        //Placement strategy #1: Center on the largest available city block
        if (placeInLargestAvailableBlock)
        {
            while(availableCityBlocks.Count > 0)
            {
                int indexToPop = availableCityBlocks.Count - 1;
                CityBlock possibleLocation = availableCityBlocks[indexToPop];
                availableCityBlocks.RemoveAt(indexToPop);

                //The largest city block left is not big enough, so resort to random placement
                //Keep the >= because same size generates will fail SafeToGenerate (tested)
                if (areaLength >= possibleLocation.GetSmallestDimension())
                    break;

                //Block is large enough for the building, but is there something already in it?
                newX = possibleLocation.coords.x;
                newZ = possibleLocation.coords.y;
                if (SafeToGenerate(newX, newZ, areaLength, false))
                {
                    foundPlace = true;
                    break;
                }
            }
        }

        //Placement strategy #2: Random placement
        if (!foundPlace)
        {
            int maxAttempts = overrideRoadsIfNeeded ? 200 : 50;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                newX = Random.Range(0, areaTaken.GetLength(0));
                newZ = Random.Range(0, areaTaken.GetLength(1));

                if (SafeToGenerate(newX, newZ, areaLength, maxAttempts > 150))
                {
                    foundPlace = true;
                    break;
                }
            }
        }

        //If found place, create model, position it, and call set up on it
        if (foundPlace)
        {
            //Reserve area for building
            ReserveAreas(newX, newZ, areaLength, AreaReservationType.ReservedByBuilding);

            //Create it
            Transform newBuilding = InstantiateNewBuilding(buildingIndex);
            newBuilding.parent = transform;
            newBuilding.localRotation = Quaternion.Euler(0, 0, 0);

            //Position it...
            Vector3 buildingPosition = Vector3.zero;

            //Center building within allocated area and convert from area space to local coordinates
            buildingPosition.x = (newX + (areaLength / 2.0f) - areaTaken.GetLength(0) / 2.0f) * areaSize;
            buildingPosition.z = (newZ + (areaLength / 2.0f) - areaTaken.GetLength(1) / 2.0f) * areaSize;

            newBuilding.localPosition = buildingPosition;

            God.SnapToGround(newBuilding);

            //Rotate it
            SetBuildingRotation(newBuilding, newX + (areaLength / 2), newZ + (areaLength / 2));

            //Remember building and finally, call set up on it
            buildings.Add(newBuilding.GetComponent<Building>());
            newBuilding.GetComponent<Building>().SetUpBuilding(
                this,
                buildingIndex,
                wallMaterials[Random.Range(0, wallMaterials.Length)],
                floorMaterials[Random.Range(0, floorMaterials.Length)]);
        }
        //Otherwise, we fucking give up

        return foundPlace;
    }

    private int SelectGenericBuildingPrototype()
    {
        for (int attempt = 1; attempt <= 50; attempt++)
        {
            for (int x = 0; x < buildingPrototypes.Length; x++)
            {
                //The end of the prototypes array is where the special buildings are. Since this function is to pick generic models, we stop once we hit the special section
                if (buildingPrototypes[x].CompareTag("Special Building"))
                    break;

                if (Random.Range(0, totalSpawnChance) < buildingPrototypes[x].spawnChance)
                    return x;
            }
        }

        return 0;
    }

    private bool SafeToGenerate(int startX, int startZ, int areasLong, bool overrideRoads)
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
                if (areaTaken[x, z] == AreaReservationType.ReservedByBuilding || (!overrideRoads && areaTaken[x, z] == AreaReservationType.ReservedByRoad))
                    return false;

                //Circular boundary
                if (circularCity && !IsWithinCircularCityWalls(x, z))
                    return false;
            }
        }

        return true;
    }

    //Usually cities are a rectangle where building placement is restricted by a city width and height.
    //This adds another requirement that buildings be within a certain radius from the city center.
    private bool IsWithinCircularCityWalls(int x, int z)
    {
        return AreaCoordToLocalCoord(new Vector3(x, 0, z)).magnitude < radius;
    }

    private void ReserveAreas(int startX, int startZ, int areasLong, AreaReservationType reservationType)
    {
        for (int x = startX; x < startX + areasLong; x++)
        {
            for (int z = startZ; z < startZ + areasLong; z++)
                areaTaken[x, z] = reservationType;
        }
    }

    private void SetBuildingRotation(Transform building, int xCoord, int zCoord)
    {
        Vector3 newRotation = Vector3.zero;

        int newMargin = 0;

        //Find closest horizontal road
        int closestZMargin = 9999;
        bool faceDown = false;
        for (int z = 0; z < horizontalRoads.Count; z++)
        {
            newMargin = Mathf.Abs(zCoord - horizontalRoads[z]);
            if (newMargin < closestZMargin)
            {
                closestZMargin = newMargin;
                faceDown = zCoord > horizontalRoads[z];
            }
        }

        //Find closest vertical road
        int closestXMargin = 9999;
        bool faceLeft = false;
        for (int x = 0; x < verticalRoads.Count; x++)
        {
            newMargin = Mathf.Abs(xCoord - verticalRoads[x]);
            if (newMargin < closestXMargin)
            {
                closestXMargin = newMargin;
                faceLeft = xCoord > verticalRoads[x];
            }
        }

        //Review results and determine rotation
        if (closestXMargin == closestZMargin && Random.Range(0, 2) == 0) //Rotate according to both horizontal and vertical
        {
            if (faceLeft)
            {
                if (faceDown)
                    newRotation.y = -135;
                else
                    newRotation.y = -45;
            }
            else
            {
                if (faceDown)
                    newRotation.y = 135;
                else
                    newRotation.y = 45;
            }
        }
        else if (closestXMargin < closestZMargin) //Rotate according to closest vertical road
        {
            if (faceLeft)
                newRotation.y = -90;
            else
                newRotation.y = 90;
        }
        else  //Rotate according to closest horizontal road
        {
            if (faceDown)
                newRotation.y = 180;
            else
                newRotation.y = 0;
        }

        //Apply rotation
        building.localEulerAngles = newRotation;
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

    private Vector3 AreaCoordToLocalCoord(Vector3 areaCoord)
    {
        //Used to be / 2 (int math)
        areaCoord.x = (areaCoord.x - areaTaken.GetLength(0) / 2.0f) * areaSize;
        areaCoord.z = (areaCoord.z - areaTaken.GetLength(1) / 2.0f) * areaSize;

        return areaCoord;
    }

    public Vector3 GetNewSpawnPoint()
    {
        nextAvailableBed++;

        Building building = null;
        bool foundBed = false;

        for (int attempt = 1; !foundBed && attempt <= 50; attempt++)
        {
            if (nextAvailableBuilding >= buildings.Count)
                nextAvailableBuilding = 0;

            building = buildings[nextAvailableBuilding];

            if (building.HasNoBeds())
            {
                nextAvailableBuilding++;
                continue;
            }

            if (!building.HasBedAtIndex(nextAvailableBed))
            {
                nextAvailableBed = 0;
                nextAvailableBuilding++;
            }
            else
                foundBed = true;
        }

        if (foundBed)
            return building.GetPositionOfBedAtIndex(nextAvailableBed) + Vector3.up * 3;
        else
            return transform.position; //Couldn't find bed so shove into center of city as fallback
    }

    public void GenerateNavMesh()
    {
        transform.Find("City Limits").GetComponent<NavigationZone>()
            .BakeNavigation(GetComponent<UnityEngine.AI.NavMeshSurface>(), (int)(radius * 1.1f), true);
    }

    public AsyncOperation UpdateNavMesh()
    {
        return transform.Find("City Limits").GetComponent<NavigationZone>()
            .BakeNavigation(GetComponent<UnityEngine.AI.NavMeshSurface>(), (int)(radius * 1.1f), false);
    }

    public void PrepareWalls(int wallMaterialIndex)
    {
        this.wallMaterialIndex = wallMaterialIndex;

        //Prepare for wall creation
        walls = new GameObject("City Walls").transform;
        walls.parent = transform;
        walls.localPosition = Vector3.zero;
        walls.localRotation = Quaternion.Euler(0, 0, 0);

        //Set texture of walls using reference material
        Material referenceMaterial = wallMaterials[wallMaterialIndex];
        cityWallMaterial.mainTexture = referenceMaterial.mainTexture;
        cityWallMaterial.SetTexture("_BumpMap", referenceMaterial.GetTexture("_BumpMap"));

        //Set metallic and smoothness properties of walls using reference material
        SetMetallicAndSmoothnessOfMaterial(cityWallMaterial, referenceMaterial);

        //Scale wall texture
        Vector2 wallTextureScale = referenceMaterial.mainTextureScale;
        wallTextureScale.x *= 3.0f;
        wallTextureScale.y *= 1.5f;
        cityWallMaterial.mainTextureScale = wallTextureScale;
    }

    private void GenerateWalls()
    {
        PrepareWalls(Random.Range(0, wallMaterials.Length));

        float wallLength = wallSectionPrefab.transform.localScale.x;
        float placementHeight = 0.0f;
        //float placementHeight = wallSectionPrefab.transform.localScale.y / 3.0f;

        if (circularCity)
            GenerateCircularWalls(wallLength, placementHeight);
        else
            GenerateSquareWalls(wallLength, placementHeight);
    }

    private void GenerateCircularWalls(float wallLength, float placementHeight)
    {
        Transform temporaryRotatingBase = new GameObject("Temp - Delete After City Generation").transform;
        temporaryRotatingBase.parent = walls.parent;
        temporaryRotatingBase.localPosition = walls.localPosition + Vector3.up * placementHeight;
        temporaryRotatingBase.localRotation = walls.localRotation;

        //Determine how many wall sections to place, the spacing, the angular math...
        Vector3 fenceOffsetFromBase = Vector3.forward * radius;
        float circumference = 2 * Mathf.PI * radius;
        int targetWallCount = (int)(circumference / wallLength);
        float currentEulerAngle = 0.0f;
        float eulerAngleStep = 360.0f / targetWallCount;

        //Precalculations for when to make the wall section a gate
        //The algorithm is to make a section a gate when its rotation is within 7.5 degrees of a cardinal direction (up, down, left, right)
        float eulerAngleGateThreshold = 7.5f;
        float[] gateAngles = new float[5];
        for (int x = 0; x < gateAngles.Length; x++)
            gateAngles[x] = x * (360.0f / 4.0f);

        //Place the wall sections, one section per iteration
        bool lastWasGate = false;
        for (int wallsPlaced = 0; wallsPlaced < targetWallCount; wallsPlaced++)
        {
            //Compute the new fence location
            Vector3 newFenceLocation = temporaryRotatingBase.TransformPoint(fenceOffsetFromBase);
            newFenceLocation = walls.InverseTransformPoint(newFenceLocation);

            //Determine its angle
            int fencePostRotation = (int)(currentEulerAngle + eulerAngleStep / 2.0f);

            //Determine whether it should be a gate
            bool isGate = false;
            for(int x = 0; x < gateAngles.Length; x++)
            {
                if(Mathf.Abs(gateAngles[x] + currentEulerAngle) < eulerAngleGateThreshold)
                {
                    isGate = true;
                    break;
                }
            }

            //Place it
            PlaceWallSection(isGate, lastWasGate,
                newFenceLocation.x, newFenceLocation.y, newFenceLocation.z,
                (int)temporaryRotatingBase.localEulerAngles.y,
                fencePostRotation);

            //Rotate the base for the next iteration
            currentEulerAngle -= eulerAngleStep;
            temporaryRotatingBase.localEulerAngles = Vector3.up * currentEulerAngle;

            //Other preparation for next iteration
            lastWasGate = isGate;
        }

        Destroy(temporaryRotatingBase.gameObject);
    }

    private void GenerateSquareWalls(float wallLength, float placementHeight)
    {
        float cityWidth = areaSize * areaTaken.GetLength(0);

        float startX = -cityWidth / 2.0f + 5;
        int horizontalSections = Mathf.CeilToInt(cityWidth / wallLength);

        float startZ = -cityWidth / 2.0f + 5;
        int verticalSections = horizontalSections;

        //Debug.Log("Radius: " + radius + ", Start X: " + startX + ", Start Z: " + startZ);

        //HORIZONTAL WALLS-------------------------------------------------------------------------------------

        bool[] skipWallSection = new bool[horizontalSections];

        //Find the largest road/gap
        float largestGapSize = 0;
        float largestGapCenteredAt = 0;
        for (int x = 2; x < verticalRoads.Count; x += 2)
        {
            int gapSize = (verticalRoads[x + 1] - verticalRoads[x]) * areaSize;

            if (gapSize > largestGapSize)
            {
                largestGapSize = gapSize;
                largestGapCenteredAt = (startX + verticalRoads[x] * areaSize) + gapSize / 2.0f;
            }
        }

        //Find the wall section closest to the gap and remove it
        int closestWallSectionIndex = 0;
        float closestWallSectionDist = Mathf.Infinity;
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            float wallSectionCenteredAt = startX + x * wallLength;
            float dist = Mathf.Abs(wallSectionCenteredAt - largestGapCenteredAt);
            if (dist < closestWallSectionDist)
            {
                closestWallSectionIndex = x;
                closestWallSectionDist = dist;
            }
        }
        skipWallSection[closestWallSectionIndex] = true;

        //Now that the wall section locations have been determined, place them
        float minZ = startZ - wallLength / 2.0f;
        float maxZ = startZ + verticalSections * wallLength - wallLength / 2.0f;
        bool previousWasGate = false;
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            bool nextIsGate = false;
            if (x < horizontalSections - 1)
                nextIsGate = skipWallSection[x + 1];

            //Front walls
            PlaceWallSection(skipWallSection[x], previousWasGate,
                startX + x * wallLength, placementHeight, minZ, 180);

            //Back walls
            PlaceWallSection(skipWallSection[x], nextIsGate,
                startX + x * wallLength, placementHeight, maxZ, 0);

            previousWasGate = skipWallSection[x];
        }

        bool firstHorSectionIsGate = skipWallSection[0];
        bool lastHorSectionIsGate = skipWallSection[horizontalSections - 1];

        //VERTICAL WALLS-------------------------------------------------------------------------------------

        skipWallSection = new bool[verticalSections];

        //Find the largest road/gap
        largestGapSize = 0;
        largestGapCenteredAt = 0;
        for (int z = 2; z < horizontalRoads.Count; z += 2)
        {
            int gapSize = (horizontalRoads[z + 1] - horizontalRoads[z]) * areaSize;

            if (gapSize > largestGapSize)
            {
                largestGapSize = gapSize;
                largestGapCenteredAt = (startZ + horizontalRoads[z] * areaSize) + gapSize / 2.0f;
            }
        }

        //Find the wall section closest to the gap and remove it
        closestWallSectionIndex = 0;
        closestWallSectionDist = Mathf.Infinity;
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            float wallSectionCenteredAt = startZ + z * wallLength;
            float dist = Mathf.Abs(wallSectionCenteredAt - largestGapCenteredAt);
            if (dist < closestWallSectionDist)
            {
                closestWallSectionIndex = z;
                closestWallSectionDist = dist;
            }
        }
        skipWallSection[closestWallSectionIndex] = true;

        //Now that the wall section locations have been determined, place them
        float minX = startX - wallLength / 2.0f;
        float maxX = startX + horizontalSections * wallLength - wallLength / 2.0f;
        previousWasGate = false;
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            bool nextIsGate = false;
            if (z < verticalSections - 1)
                nextIsGate = skipWallSection[z + 1];

            PlaceWallSection(skipWallSection[z], nextIsGate,
                minX, placementHeight, startZ + z * wallLength, -90);

            PlaceWallSection(skipWallSection[z], previousWasGate,
                maxX, placementHeight, startZ + z * wallLength, 90);

            previousWasGate = skipWallSection[z];
        }

        //Place fence post at near corner if no fence gates there
        /*
        if (fencePostPrefab && !firstHorSectionIsGate && !skipWallSection[0])
            PlaceFencePost(new Vector3(
                startX - wallLength / 2.0f, placementHeight, startZ - wallLength / 2.0f), 90);

        //Place fence post at far corner if no fence gates there
        if (fencePostPrefab && !lastHorSectionIsGate && !skipWallSection[verticalSections - 1])
            PlaceFencePost(new Vector3(startX + (verticalSections - 1) * wallLength + wallLength / 2.0f,
                placementHeight, startZ + (verticalSections - 1) * wallLength + wallLength / 2.0f), 90);    */
    }

    public void PlaceWallSection(bool gate, bool skipFencePost, float x, float y, float z, int rotation, int fencePostRotation = 9000)
    {
        int absRotation = Mathf.Abs(rotation);
        bool horizontalSection = (absRotation == 0 || absRotation == 180);
        
        //Place wall section
        Transform newWallSection;
        if (gate)
            newWallSection = Instantiate(horizontalSection ? horGatePrefab : verGatePrefab, walls).transform;
        else
            newWallSection = Instantiate(wallSectionPrefab, walls).transform;

        newWallSection.localRotation = Quaternion.Euler(0, rotation, 0);

        Vector3 wallPosition = new Vector3(x, y, z);
        newWallSection.localPosition = wallPosition;

        //Place fence post correlating to wall section
        if (fencePostPrefab && !gate && !skipFencePost)
        {
            Vector3 fencePostPosition = newWallSection.TransformPoint(new Vector3(0.5f, 0.0f, 0.0f));
            fencePostPosition = walls.InverseTransformPoint(fencePostPosition);

            /*
            if (horizontalSection)
                fencePostPosition.x += newWallSection.localScale.x / 2.0f;
            else
                fencePostPosition.z += newWallSection.localScale.x / 2.0f;  */

            PlaceFencePost(fencePostPosition, fencePostRotation < 9000 ? fencePostRotation : newWallSection.localEulerAngles.y);
        }
    }

    public void PlaceFencePost(Vector3 position, float rotation)
    {
        Transform fencePost = Instantiate(fencePostPrefab, walls).transform;
        fencePost.localEulerAngles = new Vector3(0, rotation, 0);
        fencePost.localPosition = position;
    }

    //Set material to updates' metallic and smoothness properties to equal that of reference material
    private void SetMetallicAndSmoothnessOfMaterial(Material materialToUpdate, Material referenceMaterial)
    {
        float metallic = referenceMaterial.GetFloat("_Metallic");
        float smoothness = referenceMaterial.GetFloat("_Glossiness");

        //if (metallic > 0 || smoothness > 0)
        //    materialToUpdate.EnableKeyword("_METALLICGLOSSMAP");

        materialToUpdate.SetFloat("_Metallic", metallic);
        materialToUpdate.SetFloat("_Glossiness", smoothness);

       // if (metallic == 0 && smoothness == 0)
       //     materialToUpdate.DisableKeyword("_METALLICGLOSSMAP");

        //materialToUpdate.
        //DynamicGI.UpdateEnvironment();
    }
}

//Class for encapsulating the coordinates and dimensions of each city block. The dimensions could be wrong for the blocks on the edge of the city.
//Thus, you would need to check the area coordinate system with the SafeToGenerate method before taking anything for granted.
public class CityBlock : System.IComparable<CityBlock>
{
    public Vector2Int coords = Vector2Int.zero;
    public Vector2Int dimensions = Vector2Int.zero;

    public CityBlock() { }

    public CityBlock(CityBlock other)
    {
        coords = other.coords;
        dimensions = other.dimensions;
    }

    //Order the city blocks smallest to largest
    public int CompareTo(CityBlock other)
    {
        int mySmallest = GetSmallestDimension();
        int otherSmallest = other.GetSmallestDimension();

        if (mySmallest < otherSmallest)
            return -1;
        else if (mySmallest > otherSmallest)
            return 1;
        else
        {
            int myLargest = GetLargestDimension();
            int otherLargest = other.GetLargestDimension();

            if (myLargest < otherLargest)
                return -1;
            else if (myLargest > otherLargest)
                return 1;
            else
                return 0;
        }

    }

    public int GetSmallestDimension() { return dimensions.x < dimensions.y ? dimensions.x : dimensions.y; }
    private int GetLargestDimension() { return dimensions.x < dimensions.y ? dimensions.y : dimensions.x; }
}

[System.Serializable]
public class CityJSON
{
    public CityJSON(City city)
    {
        name = city.name;

        radius = city.radius;

        buildingPrefabs = new List<string>(city.buildingPrefabs.Count);
        foreach (GameObject buildingPrefab in city.buildingPrefabs)
            buildingPrefabs.Add(buildingPrefab.name);

        wallMaterials = new string[city.wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            wallMaterials[x] = city.wallMaterials[x].name;

        floorMaterials = new string[city.floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            floorMaterials[x] = city.floorMaterials[x].name;

        buildings = new List<BuildingJSON>(city.buildings.Count);
        for (int x = 0; x < city.buildings.Count; x++)
            buildings.Add(new BuildingJSON(city.buildings[x]));

        //City walls
        Transform cityWalls = city.walls;
        walls = cityWalls;
        if (walls)
        {
            wallMaterialIndex = city.wallMaterialIndex;

            wallSection = city.wallSectionPrefab.name;
            horGate = city.horGatePrefab.name;
            verGate = city.verGatePrefab.name;
            if (city.fencePostPrefab)
                fencePost = city.fencePostPrefab.name;
            else
                fencePost = "";

            wallSectionLocations = new List<Vector3>();
            wallSectionRotations = new List<int>();
            wallSectionTypes = new List<string>();

            foreach (Transform wallSection in cityWalls)
            {
                wallSectionLocations.Add(wallSection.localPosition);
                wallSectionRotations.Add((int)wallSection.localEulerAngles.y);
                wallSectionTypes.Add(wallSection.tag);
            }
        }
        else
        {
            wallMaterialIndex = -1;

            wallSection = "";
            horGate = "";
            verGate = "";
            fencePost = "";

            wallSectionLocations = null;
            wallSectionRotations = null;
            wallSectionTypes = null;
        }
    }

    public void RestoreCity(City city)
    {
        city.gameObject.name = name;

        city.radius = radius;

        city.ReserveTerrainLocation();

        city.buildingPrefabs = new List<GameObject>();
        for (int x = 0; x < buildingPrefabs.Count; x++)
            city.buildingPrefabs.Add(Resources.Load<GameObject>("Planet/City/Buildings/" + buildingPrefabs[x]));

        city.wallMaterials = new Material[wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            city.wallMaterials[x] = Resources.Load<Material>("Planet/City/Building Materials/" + wallMaterials[x]);

        city.floorMaterials = new Material[floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            city.floorMaterials[x] = Resources.Load<Material>("Planet/City/Building Materials/" + floorMaterials[x]);

        city.buildings = new List<Building>(buildings.Count);
        for (int x = 0; x < buildings.Count; x++)
        {
            Building newBuilding = GameObject.Instantiate(city.buildingPrefabs[
                buildings[x].buildingIndex]).GetComponent<Building>();
            buildings[x].RestoreBuilding(newBuilding, city);
            city.buildings.Add(newBuilding);
        }

        if (walls)
        {
            city.wallSectionPrefab = Resources.Load<GameObject>("Planet/City/Wall Sections/" + wallSection);
            city.horGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + horGate);
            city.verGatePrefab = Resources.Load<GameObject>("Planet/City/Gates/" + verGate);
            if (!fencePost.Equals(""))
                city.fencePostPrefab = Resources.Load<GameObject>("Planet/City/Fence Posts/" + fencePost);

            city.PrepareWalls(wallMaterialIndex);

            for (int x = 0; x < wallSectionLocations.Count; x++)
            {
                Vector3 location = wallSectionLocations[x];

                if (wallSectionTypes[x].Equals("Fence Post"))
                    city.PlaceFencePost(location, wallSectionRotations[x]);
                else
                    //We want to place the fence posts manually, so just always say next section is gate
                    //so it won't place them for us
                    city.PlaceWallSection(wallSectionTypes[x].Equals("Gate"), true,
                        location.x, location.y, location.z, wallSectionRotations[x]);
            }
        }

        //After the city has been regenerated, build the nav mesh to pathfind through it
        city.GenerateNavMesh();

        city.OnCityStart();
    }

    //General
    public string name;
    public int radius;

    //Buildings
    public List<string> buildingPrefabs;
    public string[] wallMaterials, floorMaterials;
    public List<BuildingJSON> buildings;

    //City walls
    public bool walls;
    public int wallMaterialIndex;
    public string wallSection, horGate, verGate, fencePost;
    public List<Vector3> wallSectionLocations;
    public List<int> wallSectionRotations;  //Parallel array to wallSectionLocations
    public List<string> wallSectionTypes;  //Parallel array to wallSectionLocations
}