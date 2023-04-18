using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour, INavZoneUpdater
{
    //General
    public int radius = 100;
    public GameObject mapMarkerPrefab;
    [HideInInspector] public CityType cityType; //This is just to cache the value. The value is really determined per-planet in the class PlanetCityCustomization.
    [HideInInspector] public bool circularCity = false;
    [HideInInspector] public TerrainReservationOptions.TerrainResModType terrainModifications = TerrainReservationOptions.TerrainResModType.NoChange;

    //Relegate other aspects of city management to specialized managers (to declutter code)
    [HideInInspector] public AreaManager areaManager;
    [HideInInspector] public BuildingManager buildingManager;
    [HideInInspector] public FoundationManager foundationManager;
    [HideInInspector] public VerticalScalerManager verticalScalerManager;
    [HideInInspector] public CityWallManager cityWallManager;
    [HideInInspector] public BridgeManager bridgeManager;
    [HideInInspector] public CityLightManager cityLightManager;
    [HideInInspector] public NewCitySpecifications newCitySpecifications;

    //Called after city has been generated or regenerated
    public void BeforeCityGeneratedOrRestored()
    {
        cityType = Planet.planet.planetWideCityCustomization.cityType;

        areaManager = new AreaManager(this);
        buildingManager = new BuildingManager(this);
        foundationManager = new FoundationManager(this);
        verticalScalerManager = new VerticalScalerManager(this);
        cityWallManager = new CityWallManager(this);
        bridgeManager = new BridgeManager(this);
        cityLightManager = new CityLightManager(this);
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
        TerrainReservationOptions options = new TerrainReservationOptions(newCity, circularCity, (int)(radius * 1.2f), foundationManager.foundationHeight * 0.5f - 0.25f);

        //Reserve location for city
        if (newCity)
        {
            options.minHeightToFlattenTo = (int)PlanetTerrain.planetTerrain.GetSeabedHeight() + Random.Range(0, 4);
            options.preferredSteepness = Random.Range(0, 10);
            terrainModifications = foundationManager.GetTerrainModificationTypeForCity();

            if(newCitySpecifications.daddyCity)
            {
                options.targetToGenerateCloseTo = newCitySpecifications.daddyCity.transform;
                options.minimumDistanceFromTarget = radius + 50.0f;
            }

            options.centerOnTerrainAsFallbackIfPossible = !newCitySpecifications.smallCompound;
        }
        else
            options.position = cityLocation;

        options.terrainModification = terrainModifications;
        cityLocation = PlanetTerrain.planetTerrain.ReserveTerrainPosition(options);

        transform.position = cityLocation;
    }

    public void GenerateNewCity()
    {
        BeforeCityGeneratedOrRestored();

        //Estimate city radius. Not finalized yet and could be very off at this point.
        //radius = Random.Range(40, 100);
        //radius = Random.Range(40, 60);
        //radius = Random.Range(70, 110); //Small city
        //radius = Random.Range(80, 130);
        radius = 140;
        //radius = Random.Range(250, 300); //Huge city
        //radius = Random.Range(200, 300);
        //radius = 500; //Approximately the size of the whole terrain

        //Determine city type, which determines whether we have walls, fence posts...
        //what buildings and building materials are used etc...
        CityGenerator.generator.CustomizeCity(this);

        //Early on, let's get references to our building prototypes so we can reference them anywere below (needed for adjusting the city radius and GenerateRoads())
        buildingManager.LoadBuildingPrototypes();

        //Just determine what our plans are for the foundations. The actual building of foundations is later on.
        //We need to determine our plans now so that we can finalize the city radius. A lot of stuff is based on the city radius.
        foundationManager.DetermineFoundationPlans();
        
        //Finalize city radius
        if (newCitySpecifications.smallCompound) //If its a small compound made to house a special building, make sure it can fit that special building (and not much more)
        {
            radius = (buildingManager.GetLongestBuildingLength() / 2) + areaManager.areaSize;

            if (circularCity) //Convert from side length to hypotenuse (c^2 = a^2 + b^2)
                radius = (int)Mathf.Sqrt(radius * radius * 2);
        }
        else //Otherwise, we will use the previous radius computations and adjust them based on the foundation plans
            foundationManager.AdjustCityRadiusToCompensateForFoundationPlans();

        //At this point, the city radius is final. We will now create our area management system and reserve our place in the terrain...
        //...based on the city radius.
        areaManager.InitializeAreaReservationSystem();
        ReserveTerrainLocation(true, Vector3.zero);

        //Generate roads (and city blocks)
        areaManager.GenerateRoadsAndCityBlocks();
        areaManager.availableCityBlocks.Sort(); //Sort the city blocks, smallest to largest
        int avgBlockLength = areaManager.GetAverageLengthOfCityBlockInLocalUnits();

        //Generate foundations
        foundationManager.GenerateNewFoundations();

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        //The buildings need to raycast snap to the top of the foundations (thus why this is needed here).
        Physics.SyncTransforms();

        //Generate buildings
        buildingManager.GenerateNewBuildings(avgBlockLength);

        //Updates the physics colliders based on changes to transforms.
        //Needed for raycasts to work correctly for the remainder of the city generation (since its all done in one frame).
        //The walls need to raycast snap to the top of the foundations (and buildings?). Not sure if this one is needed actually...
        Physics.SyncTransforms();

        //Generate walls
        cityWallManager.GenerateNewWalls();

        //Generate bridges
        bridgeManager.GenerateNewBridges();

        //Generate the public lighting that illuminates the city at night
        cityLightManager.GenerateNewCityLights();

        //Debug.Log("City radius: " + radius + ", buildings: " + buildings.Count);

        //After the city has been generated, build the nav mesh to pathfind through it
        GenerateNavMesh();

        //Set the name after all possible influencing factors on the name have been set
        if (!newCitySpecifications.smallCompound)
            gameObject.name = CityGenerator.GenerateCityName(Planet.planet.biome, radius);
        else if(!string.IsNullOrEmpty(newCitySpecifications.compoundMainBuilding))
            gameObject.name = newCitySpecifications.compoundMainBuilding;
        else
            gameObject.name = "Auxillary Compound";

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

    public Vector3 GetRandomGlobalPointOutsideOfCity()
    {
        //Generate a point in local space 1 unit out from the center of the city that is 0-180 degrees spherically interpolated between the front and back
        Vector3 randomDirection = Vector3.Slerp(Vector3.forward, Vector3.back, Random.Range(0.0f, 1.0f));

        //Make the 0-180 range now 0-360
        if (Random.Range(0, 2) == 0)
            randomDirection = -randomDirection;

        //We have our direction, now change it from 1 unit out to cityRadius+ units out and change from local to global space
        Vector3 randomPointInGlobal = transform.TransformPoint(randomDirection * radius * 100.0f);

        //Make the point high in the air so that closest points on a collider will be the highest point
        return randomPointInGlobal + Vector3.up * 1000.0f;
    }
}

[System.Serializable]
public class CityJSON
{
    //General
    public string name;
    public int radius;
    public bool circularCity;
    public TerrainReservationOptions.TerrainResModType terrainModifications;
    public Vector3 cityLocation;

    //Sub-managers
    public BuildingManagerJSON buildingManagerJSON;
    public FoundationManagerJSON foundationManagerJSON;
    public VerticalScalerManagerJSON verticalScalerManagerJSON;
    public CityWallManagerJSON cityWallManagerJSON;
    public BridgeManagerJSON bridgeManagerJSON;
    public CityLightManagerJSON cityLightManagerJSON;

    public CityJSON(City city)
    {
        name = city.name;
        radius = city.radius;

        circularCity = city.circularCity;
        terrainModifications = city.terrainModifications;
        cityLocation = city.transform.position;

        buildingManagerJSON = new BuildingManagerJSON(city.buildingManager);
        foundationManagerJSON = new FoundationManagerJSON(city.foundationManager);
        verticalScalerManagerJSON = new VerticalScalerManagerJSON(city.verticalScalerManager);
        cityWallManagerJSON = new CityWallManagerJSON(city.cityWallManager);
        bridgeManagerJSON = new BridgeManagerJSON(city.bridgeManager);
        cityLightManagerJSON = new CityLightManagerJSON(city.cityLightManager);
    }

    public void RestoreCity(City city)
    {
        city.BeforeCityGeneratedOrRestored();

        city.gameObject.name = name;
        city.radius = radius;

        string cityTypePathSuffix = city.cityType.name + "/";

        city.circularCity = circularCity;
        city.terrainModifications = terrainModifications;
        city.ReserveTerrainLocation(false, cityLocation);

        buildingManagerJSON.RestoreBuildingManager(city.buildingManager, cityTypePathSuffix);
        foundationManagerJSON.RestoreFoundationManager(city.foundationManager);
        verticalScalerManagerJSON.RestoreVerticalScalerManager(city.verticalScalerManager);
        cityWallManagerJSON.RestoreCityWallManager(city.cityWallManager, cityTypePathSuffix);
        bridgeManagerJSON.RestoreBridgeManager(city.bridgeManager);
        cityLightManagerJSON.RestoreCityLightManager(city.cityLightManager, cityTypePathSuffix);

        //After the city has been regenerated, build the nav mesh to pathfind through it
        city.GenerateNavMesh();

        city.AfterCityGeneratedOrRestored();
    }
}