using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour, INavZoneUpdater
{
    //General
    public int radius = 100;
    public GameObject mapMarkerPrefab;
    [HideInInspector] public CityType cityType;
    [HideInInspector] public bool circularCity = false;

    //Relegate other aspects of city management to specialized managers (to declutter code)
    [HideInInspector] public AreaManager areaManager;
    [HideInInspector] public BuildingManager buildingManager;
    [HideInInspector] public FoundationManager foundationManager;
    [HideInInspector] public CityWallManager cityWallManager;
    [HideInInspector] public BridgeManager bridgeManager;
    [HideInInspector] public NewCitySpecifications newCitySpecifications;

    //Called after city has been generated or regenerated
    public void BeforeCityGeneratedOrRestored()
    {
        areaManager = new AreaManager(this);
        buildingManager = new BuildingManager(this);
        foundationManager = new FoundationManager(this);
        cityWallManager = new CityWallManager(this);
        bridgeManager = new BridgeManager(this);
    }

    public void AfterCityGeneratedOrRestored()
    {
        //Create marker for city on the planet map
        MapMarker mapMarker = Instantiate(mapMarkerPrefab).GetComponent<MapMarker>();
        mapMarker.InitializeMarker(transform);
    }

    //If its a new city, the cityLocation parameter is ignored and a new position is chosen. Else, its a restored city and the cityLocation is reused.
    public void ReserveTerrainLocation(bool newCity, Vector3 cityLocation)
    {
        TerrainReservationOptions options = new TerrainReservationOptions(newCity, (int)(radius * 1.2f));
        options.flatten = true;

        //Reserve location for city
        if (newCity)
        {
            options.heightRange = new Vector2Int((int)PlanetTerrain.planetTerrain.GetSeabedHeight() + Random.Range(0, 4), 500);
            options.preferredSteepness = Random.Range(0, 10);
        }
        else
            options.position = cityLocation;

        cityLocation = PlanetTerrain.planetTerrain.ReserveTerrainPosition(options);

        /* if(newCity)
        {
            //Place city at slight offset to location
            cityLocation.x -= 10;
            cityLocation.z -= 10;
        }   */

        transform.position = cityLocation;
    }

    public void GenerateNewCity()
    {
        BeforeCityGeneratedOrRestored();
        newCitySpecifications = new NewCitySpecifications();

        //Reserve terrain location
        //radius = Random.Range(40, 100);
        //radius = Random.Range(40, 60);
        //radius = Random.Range(70, 110); //Small city
        //radius = Random.Range(80, 130);
        //radius = Random.Range(250, 300); //Huge city
        radius = Random.Range(70, 300);
        //radius = 500; //Approximately the size of the whole terrain
        areaManager.InitializeAreaReservationSystem();
        ReserveTerrainLocation(true, Vector3.zero);

        //Determine city type, which determines whether we have walls, fence posts...
        //what buildings and building materials are used etc...
        CityGenerator.generator.CustomizeCity(this);

        //Early on, let's get references to our building prototypes so we can reference them anywere below (needed for GenerateRoads())
        buildingManager.LoadBuildingPrototypes();

        //Generate roads (and city blocks)
        areaManager.GenerateRoads();
        areaManager.availableCityBlocks.Sort(); //Sort the city blocks, smallest to largest
        int avgBlockLength = areaManager.GetAverageLengthOfCityBlockInLocalUnits();

        //for (int x = 0; x < Random.Range(0, radius / 40); x++)
        //    ReserveSector();

        //Generate foundations
        foundationManager.GenerateNewFoundations();

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        Physics.SyncTransforms();

        //Generate buildings
        buildingManager.GenerateNewBuildings(avgBlockLength);

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        Physics.SyncTransforms();

        //Generate walls
        cityWallManager.GenerateNewWalls();

        //Generate bridges
        bridgeManager.GenerateNewBridges();

        //Debug.Log("City radius: " + radius + ", buildings: " + buildings.Count);

        //After the city has been generated, build the nav mesh to pathfind through it
        GenerateNavMesh();

        //Set the name after all possible influencing factors on the name have been set
        gameObject.name = CityGenerator.GenerateCityName(Planet.planet.biome, radius);

        AfterCityGeneratedOrRestored();
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
}

[System.Serializable]
public class CityJSON
{
    //General
    public string name;
    public int radius;
    public int cityTypeIndex;
    public Vector3 cityLocation;

    //Sub-managers
    public BuildingManagerJSON buildingManagerJSON;
    public FoundationManagerJSON foundationManagerJSON;
    public CityWallManagerJSON cityWallManagerJSON;
    public BridgeManagerJSON bridgeManagerJSON;

    public CityJSON(City city)
    {
        name = city.name;
        radius = city.radius;

        for(int x = 0; x < CityGenerator.generator.cityTypes.Length; x++)
        {
            if (CityGenerator.generator.cityTypes[x] == city.cityType)
            {
                cityTypeIndex = x;
                break;
            }
        }

        cityLocation = city.transform.position;

        buildingManagerJSON = new BuildingManagerJSON(city.buildingManager);
        foundationManagerJSON = new FoundationManagerJSON(city.foundationManager);
        cityWallManagerJSON = new CityWallManagerJSON(city.cityWallManager);
        bridgeManagerJSON = new BridgeManagerJSON(city.bridgeManager);
    }

    public void RestoreCity(City city)
    {
        city.BeforeCityGeneratedOrRestored();

        city.gameObject.name = name;
        city.radius = radius;

        city.cityType = CityGenerator.generator.cityTypes[cityTypeIndex];
        string cityTypePathSuffix = city.cityType.name + "/";

        city.ReserveTerrainLocation(false, cityLocation);

        buildingManagerJSON.RestoreBuildingManager(city.buildingManager, cityTypePathSuffix);
        foundationManagerJSON.RestoreFoundationManager(city.foundationManager);
        cityWallManagerJSON.RestoreCityWallManager(city.cityWallManager, cityTypePathSuffix);
        bridgeManagerJSON.RestoreBridgeManager(city.bridgeManager);

        //After the city has been regenerated, build the nav mesh to pathfind through it
        city.GenerateNavMesh();

        city.AfterCityGeneratedOrRestored();
    }
}