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

    //Area system (how city is subdivided and organized)
    private int areaSize = 5;
    private AreaReservationType[,] areaTaken;

    //Even index entries denote start point of road, odd index entries indicate end point of previous index's road
    private List<int> horizontalRoads, verticalRoads;

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
        radius = Random.Range(70, 110);
        InitializeAreaReservationSystem();
        ReserveTerrainLocation();

        //Determine city type, which determines whether we have walls, fence posts...
        //what buildings and building materials are used etc...
        CityGenerator.generator.CustomizeCity(this);

        //Generate roads
        GenerateRoads();

        for (int x = 0; x < Random.Range(0, radius / 40); x++)
            ReserveSector();

        //Generate buildings
        buildings = new List<Building>();
        LoadBuildingPrototypes();
        GenerateSpecialBuildings();
        GenerateGenericBuildings();

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
        //Horizontal roads
        horizontalRoads = new List<int>();
        for (int z = 0; z < areaTaken.GetLength(1); z += Mathf.Max(Random.Range(40, 70) / areaSize, 1))
            GenerateRoad(true, z);

        //Vertical roads
        verticalRoads = new List<int>();
        for (int x = 0; x < areaTaken.GetLength(0); x += Mathf.Max(Random.Range(40, 70) / areaSize, 1))
            GenerateRoad(false, x);
    }

    private void GenerateRoad(bool horizontal, int startCoord)
    {
        int areasWide = Mathf.Max(1, Random.Range(5, 20) / areaSize);

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
                    if (GenerateBuilding(x, true))
                        break;
                }
            }
        }
    }

    private void GenerateGenericBuildings()
    {
        for (int x = 0; x < 400; x++)
            GenerateBuilding();
    }

    //Used to generate a NEW building. Pass in index of model if particular one is desired, else a random model will be selected. Specify aggressive placement to ignore roads during placement.
    //Returns whether building was successfully generated.
    private bool GenerateBuilding(int buildingIndex = -1, bool aggressiveBuildingPlacement = false)
    {
        //If the choice has not been already made, then randomly pick a model
        if(buildingIndex == -1)
            buildingIndex = SelectGenericBuildingPrototype();

        //Find place that can fit model...

        int newX = 0, newZ = 0;
        int areaLength = Mathf.CeilToInt(buildingPrototypes[buildingIndex].length * 1.0f / areaSize);

        bool foundPlace = false;

        for (int attempt = 1; attempt <= 50; attempt++)
        {
            newX = Random.Range(0, areaTaken.GetLength(0));
            newZ = Random.Range(0, areaTaken.GetLength(1));

            if (SafeToGenerate(newX, newZ, areaLength, aggressiveBuildingPlacement))
            {
                foundPlace = true;
                break;
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
            }
        }

        return true;
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
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            bool nextIsGate = false;
            if (x < horizontalSections - 1)
                nextIsGate = skipWallSection[x + 1];

            //Front walls
            PlaceWallSection(skipWallSection[x], nextIsGate,
                startX + x * wallLength, placementHeight, minZ, 0);

            //Back walls
            PlaceWallSection(skipWallSection[x], nextIsGate || x == horizontalSections - 1,
                startX + x * wallLength, placementHeight,
                maxZ, 0);
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
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            bool nextIsGate = false;
            if (z < verticalSections - 1)
                nextIsGate = skipWallSection[z + 1];

            PlaceWallSection(skipWallSection[z], nextIsGate,
                minX, placementHeight, startZ + z * wallLength, 90);

            PlaceWallSection(skipWallSection[z], nextIsGate || z == verticalSections - 1,
                maxX, placementHeight, startZ + z * wallLength, 90);
        }

        //Place fence post at near corner if no fence gates there
        if (fencePostPrefab && !firstHorSectionIsGate && !skipWallSection[0])
            PlaceFencePost(new Vector3(
                startX - wallLength / 2.0f, placementHeight, startZ - wallLength / 2.0f), 90);

        //Place fence post at far corner if no fence gates there
        if (fencePostPrefab && !lastHorSectionIsGate && !skipWallSection[verticalSections - 1])
            PlaceFencePost(new Vector3(startX + (verticalSections - 1) * wallLength + wallLength / 2.0f,
                placementHeight, startZ + (verticalSections - 1) * wallLength + wallLength / 2.0f), 90);
    }

    public void PlaceWallSection(bool gate, bool nextIsGate, float x, float y, float z, int rotation)
    {
        bool horizontalSection = Mathf.Abs(rotation) < 1;

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
        if (fencePostPrefab && !gate && !nextIsGate)
        {
            Vector3 fencePostPosition = newWallSection.localPosition;
            if (horizontalSection)
                fencePostPosition.x += newWallSection.localScale.x / 2.0f;
            else
                fencePostPosition.z += newWallSection.localScale.x / 2.0f;

            PlaceFencePost(fencePostPosition, newWallSection.localEulerAngles.y);
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