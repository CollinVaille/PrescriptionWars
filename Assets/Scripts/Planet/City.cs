using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    //General
    [HideInInspector] public int radius = 100;
    public GameObject mapMarkerPrefab;

    //Building construction
    public GameObject[] buildingPrefabs;
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
    public GameObject wallSectionPrefab, gatePrefab;
    [HideInInspector] public Transform walls;
    [HideInInspector] public int wallMaterialIndex;

    //Area system (how city is subdivided and organized)
    private int areaSize = 5;
    private bool[,] areaTaken;

    //Even index entries denote start point of road, odd index entries indicate end point of previous index's road
    private List<int> horizontalRoads, verticalRoads;

    //Called after city has been generated or regenerated
    public void OnCityStart ()
    {
        //Create marker for city on the planet map
        MapMarker mapMarker = Instantiate(mapMarkerPrefab).GetComponent<MapMarker>();
        mapMarker.InitializeMarker(transform);
    }

    public void ReserveTerrainLocation ()
    {
        //Reserve location for city
        Vector3 cityLocation = PlanetTerrain.planetTerrain.ReserveTerrainPosition(Random.Range(0, 3),
            (int)PlanetTerrain.planetTerrain.GetSeabedHeight() + Random.Range(0, 4), 500, (int)(radius * 1.2f), true);

        //Place city at slight offset to location
        cityLocation.x -= 20;
        cityLocation.z -= 20;
        transform.position = cityLocation;
    }

    public void GenerateCity ()
    {
        //Reserve terrain location
        //radius = Random.Range(40, 100);
        radius = Random.Range(40, 60);
        areaTaken = new bool[radius * 2 / areaSize, radius * 2 / areaSize];
        ReserveTerrainLocation();

        GenerateBuildingMaterials();

        //Generate roads, buildings, etc...
        GenerateRoads();

        for (int x = 0; x < Random.Range(0, radius / 40); x++)
            ReserveSector();

        buildings = new List<Building>();

        LoadBuildingPrototypes();

        for (int x = 0; x < 400; x++)
            GenerateBuilding();

        if(Random.Range(0, 2) == 0)
            GenerateWalls();

        //Debug.Log("City radius: " + radius + ", buildings: " + buildings.Count);

        //After the city has been generated, build the nav mesh to pathfind through it
        GenerateNavMesh();

        //Set the name after all possible influencing factors on the name have been set
        gameObject.name = GenerateCityName();

        OnCityStart();
    }

    private void LoadBuildingPrototypes ()
    {
        buildingPrototypes = new Building[buildingPrefabs.Length];

        for(int x = 0; x < buildingPrefabs.Length; x++)
        {
            buildingPrototypes[x] = Instantiate(buildingPrefabs[x]).GetComponent<Building>();
            buildingPrototypes[x].gameObject.SetActive(false);

            totalSpawnChance += buildingPrototypes[x].spawnChance;
        }
    }

    private Transform InstantiateNewBuilding (int buildingIndex)
    {
        if (!buildingPrototypes[buildingIndex].gameObject.activeSelf) //Is model home available?
        {
            buildingPrototypes[buildingIndex].gameObject.SetActive(true);
            return buildingPrototypes[buildingIndex].transform;
        }
        else //Fine, we'll create a new one
            return Instantiate(buildingPrefabs[buildingIndex]).transform;
    }

    private void GenerateRoads ()
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

    private void GenerateRoad (bool horizontal, int startCoord)
    {
        int areasWide = Mathf.Max(1, Random.Range(5, 20) / areaSize);

        if(horizontal) //Horizontal road
        {
            horizontalRoads.Add(startCoord);
            horizontalRoads.Add(startCoord + areasWide);

            for (int x = 0; x < areaTaken.GetLength(0); x++)
            {
                for (int z = startCoord; z < startCoord + areasWide; z++)
                {
                    if(z < areaTaken.GetLength(1))
                        areaTaken[x, z] = true;
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
                        areaTaken[x, z] = true;
                }
            }
        }
    }

    private void GenerateBuilding ()
    {
        //Select which model to make
        int buildingIndex = SelectBuildingPrototype();

        //Find place that can fit model...

        int newX = 0, newZ = 0;
        int areaLength = Mathf.CeilToInt(buildingPrototypes[buildingIndex].length * 1.0f / areaSize);

        bool foundPlace = false;
        
        for(int attempt = 1; attempt <= 50; attempt++)
        {
            newX = Random.Range(0, areaTaken.GetLength(0));
            newZ = Random.Range(0, areaTaken.GetLength(1));

            if (SafeToGenerate(newX, newZ, areaLength))
            {
                foundPlace = true;
                break;
            }
        }

        //If found place, create model, position it, and call set up on it
        if (foundPlace)
        {
            //Reserve area for building
            ReserveAreas(newX, newZ, areaLength);

            //Create it
            Transform newBuilding = InstantiateNewBuilding(buildingIndex);
            newBuilding.parent = transform;
            newBuilding.localRotation = Quaternion.Euler(0, 0, 0);

            //Position it...
            Vector3 buildingPosition = Vector3.zero;

            //Center building within allocated area and convert from area space to local coordinates
            buildingPosition.x = (newX + (areaLength / 2) - areaTaken.GetLength(0) / 2) * areaSize;
            buildingPosition.z = (newZ + (areaLength / 2) - areaTaken.GetLength(1) / 2) * areaSize;

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
    }

    private int SelectBuildingPrototype ()
    {
        for (int attempt = 1; attempt <= 50; attempt++)
        {
            for (int x = 0; x < buildingPrototypes.Length; x++)
            {
                if (Random.Range(0, totalSpawnChance) < buildingPrototypes[x].spawnChance)
                    return x;
            }
        }

        return buildingPrototypes.Length - 1;
    }

    private bool SafeToGenerate (int startX, int startZ, int areasLong)
    {
        for(int x = startX; x < startX + areasLong; x++)
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
                if (areaTaken[x, z])
                    return false;
            }
        }

        return true;
    }

    private void ReserveAreas (int startX, int startZ, int areasLong)
    {
        for (int x = startX; x < startX + areasLong; x++)
        {
            for (int z = startZ; z < startZ + areasLong; z++)
                areaTaken[x, z] = true;
        }
    }

    private void SetBuildingRotation (Transform building, int xCoord, int zCoord)
    {
        Vector3 newRotation = Vector3.zero;

        int newMargin = 0;

        //Find closest horizontal road
        int closestZMargin = 9999;
        bool faceDown = false;
        for(int z = 0; z < horizontalRoads.Count; z++)
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
                if(faceDown)
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
        else if(closestXMargin < closestZMargin) //Rotate according to closest vertical road
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

    private Vector3 ReserveSector ()
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
        while (attempt <= 50 && areaTaken[horizontalStartArea, verticalStartArea]);

        //Give up
        if (areaTaken[horizontalStartArea, verticalStartArea])
            return Vector3.zero;

        //Reserve areas within sector
        for (int x = horizontalStartArea; x < horizontalEndArea; x++)
        {
            for (int z = verticalStartArea; z < verticalEndArea; z++)
                areaTaken[x, z] = true;
        }

        //Return center of sector
        Vector3 sectorCenter = Vector3.zero;
        sectorCenter.x = horizontalStartArea + (horizontalEndArea - horizontalStartArea) * 0.5f;
        sectorCenter.z = verticalStartArea + (verticalEndArea - verticalStartArea) * 0.5f;
        return AreaCoordToLocalCoord(sectorCenter);
    }

    private Vector3 AreaCoordToLocalCoord (Vector3 areaCoord)
    {
        areaCoord.x = (areaCoord.x - areaTaken.GetLength(0) / 2) * areaSize;
        areaCoord.z = (areaCoord.z - areaTaken.GetLength(1) / 2) * areaSize;

        return areaCoord;
    }

    public Vector3 GetNewSpawnPoint ()
    {
        nextAvailableBed++;

        Building building = null;
        bool foundBed = false;

        for(int attempt = 1; !foundBed && attempt <= 50; attempt++)
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

    public void GenerateNavMesh ()
    {
        transform.Find("City Limits").GetComponent<NavigationZone>()
            .BakeNavigation(GetComponent<UnityEngine.AI.NavMeshSurface>(), (int)(radius * 1.1f));
    }

    public void PrepareWalls (int wallMaterialIndex)
    {
        this.wallMaterialIndex = wallMaterialIndex;

        //Prepare for wall creation
        walls = new GameObject("City Walls").transform;
        walls.parent = transform;
        walls.localPosition = Vector3.zero;
        walls.localRotation = Quaternion.Euler(0, 0, 0);

        //Set texture of walls using reference material
        Material cityWallMaterial = wallSectionPrefab.GetComponent<Renderer>().sharedMaterial;
        Material referenceMaterial = wallMaterials[wallMaterialIndex];
        cityWallMaterial.mainTexture = referenceMaterial.mainTexture;
        cityWallMaterial.SetTexture("_BumpMap", referenceMaterial.GetTexture("_BumpMap"));

        //Scale wall texture
        Vector2 wallTextureScale = referenceMaterial.mainTextureScale;
        wallTextureScale.x *= 3.0f;
        wallTextureScale.y *= 1.5f;
        cityWallMaterial.mainTextureScale = wallTextureScale;
    }

    private void GenerateWalls ()
    {
        PrepareWalls(Random.Range(0, wallMaterials.Length));

        float wallLength = wallSectionPrefab.transform.localScale.x;
        float placementHeight = wallSectionPrefab.transform.localScale.y / 3.0f;

        float cityWidth = areaSize * areaTaken.GetLength(0);

        float startX = -cityWidth / 2.0f;
        int horizontalSections = Mathf.CeilToInt(cityWidth / wallLength);

        float startZ = -cityWidth / 2.0f;
        int verticalSections = horizontalSections;

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
        for(int x = 0; x < skipWallSection.Length; x++)
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
        for (int x = 0; x < skipWallSection.Length; x++)
        {
            PlaceWallSection(skipWallSection[x], startX + x * wallLength, placementHeight, startZ - wallLength / 2.0f, 0);
            PlaceWallSection(skipWallSection[x], startX + x * wallLength, placementHeight,
                startZ + verticalSections * wallLength - wallLength / 2.0f, 0);
        }

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
        for (int z = 0; z < skipWallSection.Length; z++)
        {
            PlaceWallSection(skipWallSection[z], startX - wallLength / 2.0f, placementHeight, startZ + z * wallLength, 90);
            PlaceWallSection(skipWallSection[z], startX + horizontalSections * wallLength - wallLength / 2.0f,
                placementHeight, startZ + z * wallLength, 90);
        }
    }

    public void PlaceWallSection (bool gate, float x, float y, float z, int rotation)
    {
        Transform newWallSection;
        if(gate)
            newWallSection = Instantiate(gatePrefab, walls).transform;
        else
            newWallSection = Instantiate(wallSectionPrefab, walls).transform;

        newWallSection.localRotation = Quaternion.Euler(0, rotation, 0);

        Vector3 wallPosition = new Vector3(x, y, z);
        newWallSection.localPosition = wallPosition;
    }

    private void GenerateBuildingMaterials ()
    {
        //Keep concrete theme going
        if (Random.Range(0, 10) == 0)
            return;

        switch(Planet.planet.biome)
        {
            case Planet.Biome.Temperate:
                wallMaterials = new Material[] {
                    Resources.Load<Material>("Building Materials/Plaster Wall"),
                    Resources.Load<Material>("Building Materials/Brick Wall")
                };
                floorMaterials = new Material[] {
                    Resources.Load<Material>("Building Materials/Wood Floor"),
                    Resources.Load<Material>("Building Materials/Slate Tile Floor")
                };
                break;
            case Planet.Biome.Desert:
                wallMaterials = new Material[] {
                    Resources.Load<Material>("Building Materials/Sand Wall"),
                    Resources.Load<Material>("Building Materials/Sand Wall 2"),
                    Resources.Load<Material>("Building Materials/Cobblestone Wall")
                };
                floorMaterials = new Material[] {
                    Resources.Load<Material>("Building Materials/Sand Wall"),
                    Resources.Load<Material>("Building Materials/Sand Wall 2"),
                    Resources.Load<Material>("Building Materials/Slate Tile Floor")
                };
                break;
        }
    }

    public string GenerateCityName ()
    {
        string cityName = "";

        if (radius < 60) //Small station name
        {
            //Get list of station names
            TextAsset stationNamesFile = Resources.Load<TextAsset>("Text/Station Names");
            string[] stationNames = stationNamesFile.text.Split('\n');

            string[] stationSuffixes = new string[] { " Station", " Outpost", " Camp", " Settlement",
                " Installation", " Base", " Post", " Retreat", " Village", " Point", " Favela" };

            //Pick a random name
            cityName = stationNames[Random.Range(0, stationNames.Length)]
                + stationSuffixes[Random.Range(0, stationSuffixes.Length)];
        }
        else //Major city name
        {
            if(Random.Range(0, 4) == 0) //Two part generic city name
            {
                string[] part1 = new string[] { "East", "West", "North", "South", "White", "Gray", "Pale",
                    "Black", "Mourn", "Hjaal", "Haa", "Frost", "Way", "Storm", "Baren", "Falk" };

                string[] part2 = new string[] { "march", "reach", "hold", "rest", "haven", "fold", "garden",
                    "fingar", "run", "'s Hand", " Seed", " Harbour", " Solace" };

                cityName = part1[Random.Range(0, part1.Length)] + part2[Random.Range(0, part2.Length)];
            }
            else if (Random.Range(0, 4) == 0) //Two part nordic/dwarven city name
            {
                string[] part1 = new string[] { "Staavan", "Volks", "Korvan", "Weyro", "Teyro", "Vail", "Rhen",
                    "Bhor", "Vel", "Galto", "Vogh", "Mons", "Forel" };

                string[] part2 = new string[] { "gar", "gaard", "var", "boro", "baro", " Koros", "kura",
                    "brunnr", "kyyge", "kuldhir", "touhm", "thume" };

                cityName = part1[Random.Range(0, part1.Length)] + part2[Random.Range(0, part2.Length)];
            }
            else //Normal city name
            {
                //Get list of city names
                TextAsset cityNamesFile = Resources.Load<TextAsset>("Text/City Names");
                string[] cityNames = cityNamesFile.text.Split('\n');

                //Pick a random name
                cityName = cityNames[Random.Range(0, cityNames.Length)];
            }
        }

        return cityName;
    }
}

[System.Serializable]
public class CityJSON
{
    public CityJSON (City city)
    {
        name = city.name;

        radius = city.radius;

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
        if(walls)
        {
            wallMaterialIndex = city.wallMaterialIndex;

            wallSectionLocations = new List<Vector3>();
            wallSectionRotations = new List<int>();
            wallSectionIsGate = new List<bool>();

            foreach(Transform wallSection in cityWalls)
            {
                wallSectionLocations.Add(wallSection.localPosition);
                wallSectionRotations.Add((int)wallSection.localEulerAngles.y);
                wallSectionIsGate.Add(wallSection.CompareTag("Gate"));
            }
        }
        else
        {
            wallMaterialIndex = -1;
            wallSectionLocations = null;
            wallSectionRotations = null;
            wallSectionIsGate = null;
        }
    }

    public void RestoreCity (City city)
    {
        city.gameObject.name = name;

        city.radius = radius;

        city.ReserveTerrainLocation();

        city.wallMaterials = new Material[wallMaterials.Length];
        for (int x = 0; x < wallMaterials.Length; x++)
            city.wallMaterials[x] = Resources.Load<Material>("Building Materials/" + wallMaterials[x]);

        city.floorMaterials = new Material[floorMaterials.Length];
        for (int x = 0; x < floorMaterials.Length; x++)
            city.floorMaterials[x] = Resources.Load<Material>("Building Materials/" + floorMaterials[x]);

        city.buildings = new List<Building>(buildings.Count);
        for (int x = 0; x < buildings.Count; x++)
        {
            Building newBuilding = GameObject.Instantiate(city.buildingPrefabs[
                buildings[x].buildingIndex]).GetComponent<Building>();
            buildings[x].RestoreBuilding(newBuilding, city);
            city.buildings.Add(newBuilding);
        }

        if(walls)
        {
            city.PrepareWalls(wallMaterialIndex);

            for(int x = 0; x < wallSectionLocations.Count; x++)
            {
                Vector3 location = wallSectionLocations[x];
                city.PlaceWallSection(wallSectionIsGate[x], location.x, location.y, location.z, wallSectionRotations[x]);
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
    public string[] wallMaterials, floorMaterials;
    public List<BuildingJSON> buildings;

    //City walls
    public bool walls;
    public int wallMaterialIndex;
    public List<Vector3> wallSectionLocations;
    public List<int> wallSectionRotations;  //Parallel array to wallSectionLocations
    public List<bool> wallSectionIsGate;
}