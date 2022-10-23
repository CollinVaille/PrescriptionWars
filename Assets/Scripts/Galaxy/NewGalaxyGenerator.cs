using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyGenerator : MonoBehaviour
{
    [Header("Material Options")]

    [SerializeField, Tooltip("The material that will be applied to the skybox while the galaxy scene is open and the galaxy is active in the project hierarchy.")] private Material skyboxMaterial = null;

    [Header("New Game Data")]

    [SerializeField, Tooltip("Holds either the new game data passed from the new game menu or the new game data selected through the inspector.")] private NewGameData newGameData = new NewGameData();
    [SerializeField, Tooltip("The minimum amount of space that must exist between all stars in the galaxy.")] private int minimumSpaceBetweenStars = 0;
    [SerializeField, Tooltip("Array that contains the probability for each star type to be assigned to a star. The enum index of the star type correlates to the same index in this array.")] private float[] starTypeSpawnProbabilities = new float[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];
    [SerializeField, Tooltip("Indicates the amount of space between the star and the first planetary orbit in the solar system that may or may not have a planet on it.")] private float spaceBetweenStarAndPlanetaryOrbits = 10;
    [SerializeField, Tooltip("Indicates the amount of space between each planetary orbit in the solar system that may or may not have a planet on it.")] private float spaceBetweenPlanetaryOrbits = 5;

    [Header("Biome Options")]

    [SerializeField, Tooltip("List that specifies the data for every biome.")] private List<NewGalaxyBiome> biomes = new List<NewGalaxyBiome>();

    [Header("Parents")]

    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the solar systems in the galaxy. Specified through the inspector.")] private Transform solarSystemsParent = null;

    [Header("Prefabs")]

    [SerializeField, Tooltip("The prefab that all solar systems in the galaxy will be instanitated from. Specified through the inspector.")] private GameObject solarSystemPrefab = null;
    [SerializeField, Tooltip("Array that contains the prefab for each star type where the enum index of the star type correlates to the same index in this array.")] private GameObject[] starTypePrefabs = new GameObject[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];
    [SerializeField, Tooltip("The prefab that all planets in every solar system in the galaxy will be instantiated from.")] private GameObject planetPrefab = null;

    //Non-inspector variables.

    /// <summary>
    /// Indicates how many failed attempts can be made to place a solar system within the galaxy until any attempts to place any more solar systems will be given up.
    /// </summary>
    private int maxFailedSolarSystemPlacementAttemps = 1000;

    /// <summary>
    /// Holds all of the solar systems that have already been fully generated and placed appropriately into the galaxy.
    /// </summary>
    private List<GalaxySolarSystem> solarSystems = null;

    /// <summary>
    /// Private variable that holds any save game data that might be passed over statically from the load game menu.
    /// </summary>
    private GalaxyData saveGameData = null;

    /// <summary>
    /// Indicates the default amount of solar systems in the galaxy if the galaxy shape does not specify an amount.
    /// </summary>
    public static int defaultSolarSystemCount { get => 60; }

    private void Awake()
    {
        //LoadGameMenu.saveGameData = GalaxySaveSystem.LoadGalaxy("Test Save");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Loads in any galaxy save game data that might be coming from the load game menu.
        LoadInSaveGameData();

        //Loads in any new game data that might be coming from the new game menu.
        LoadInNewGameData();

        //Generates the systems of the galaxy.
        GenerateSolarSystems();

        //Sets the material of the galaxy scene's skybox.
        RenderSettings.skybox = skyboxMaterial;

        //Initializes the galaxy manager.
        NewGalaxyManager.InitializeFromGalaxyGenerator(gameObject.GetComponent<NewGalaxyManager>(), skyboxMaterial, solarSystems);

        //NewGalaxyManager.saveName = "Test Save";
        //GalaxySaveSystem.SaveGalaxy();
        //Debug.Log("Saved");

        //Destroys the galaxy generator script after galaxy generation completion.
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads in any galaxy save game data that might be passes over statically from the load game menu.
    /// </summary>
    private void LoadInSaveGameData()
    {
        //Sets the local galaxy save game data to the galaxy save game data passed over statically from the load game menu.
        saveGameData = LoadGameMenu.saveGameData;
        //Resets the static galaxy save game data variable of the laod game menu back to null since the data has already been grabbed.
        LoadGameMenu.saveGameData = null;
    }

    /// <summary>
    /// Loads in any new game data that might be passed over statically from the new game menu.
    /// </summary>
    private void LoadInNewGameData()
    {
        //Sets the local new game data to the new game data passed over statically from the new game menu if it exists.
        newGameData = NewGameMenu.newGameData != null ? NewGameMenu.newGameData : newGameData;
        //Resets the static new game data variable back to null since the data has already been grabbed.
        NewGameMenu.newGameData = null;
    }

    /// <summary>
    /// Private method that should be called in the start method and generates the systems of the galaxy.
    /// </summary>
    private void GenerateSolarSystems()
    {
        //Initializes the list of solar systems.
        solarSystems = new List<GalaxySolarSystem>();
        //Generates the solar systems of the galaxy from the galaxy save game data that has been loaded in from the load game menu if it exists.
        if (saveGameData != null)
        {
            //Loops through each solar system that was stored in the galaxy save game data and creates it.
            for(int solarSystemIndex = 0; solarSystemIndex < saveGameData.solarSystems.Count; solarSystemIndex++)
            {
                //Creates a new solar system by instantiating from the solar system prefab.
                GalaxySolarSystem solarSystem = Instantiate(solarSystemPrefab).GetComponent<GalaxySolarSystem>();
                //Sets the solar system's parent to the parent transform of all solar systems.
                solarSystem.transform.SetParent(solarSystemsParent);

                //Instantiates a star using the star data of the current solar system in the galaxy save data loaded in.
                GalaxyStar star = Instantiate(starTypePrefabs[(int)saveGameData.solarSystems[solarSystemIndex].star.starType]).GetComponent<GalaxyStar>();
                star.transform.SetParent(solarSystem.transform);
                star.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex].star);
                star.transform.localPosition = Vector3.zero;

                //Loop that instantiates the planets of the solar system using the list of planet data of the current solar system in the galaxy save data loaded in.
                List<NewGalaxyPlanet> planets = new List<NewGalaxyPlanet>();
                for (int planetIndex = 0; planetIndex < saveGameData.solarSystems[solarSystemIndex].planets.Count; planetIndex++)
                {
                    //Instantiates a new empty gameobject for the planet to use an an orbit around the star.
                    GameObject planetaryOrbit = Instantiate(new GameObject());
                    //Names the planetary orbit based on how far it is from the star.
                    planetaryOrbit.name = "Planetary Orbit " + (saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].planetaryOrbitProximityToStar + 1);
                    //Sets the parent of the planetary orbit.
                    planetaryOrbit.transform.SetParent(solarSystem.planetaryOrbitsParent);
                    //Randomly sets the y rotation of the planetary orbit in order to give the planet a random rotation around the star.
                    planetaryOrbit.transform.localRotation = Quaternion.Euler(0, saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].planetaryOrbitRotation, 0);

                    //Instantiates a new planet from the planet prefab.
                    NewGalaxyPlanet planet = Instantiate(planetPrefab).transform.GetChild(0).gameObject.GetComponent<NewGalaxyPlanet>();
                    //Parents the planet under its previously created planetary orbit.
                    planet.transform.parent.SetParent(planetaryOrbit.transform);
                    //Sets the planet's distance from the star based on the biome's specified proximity to the star.
                    planet.transform.parent.localPosition = new Vector3(saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[0], saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[1], saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[2]);
                    //Initializes all needed variables of the planet.
                    planet.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex].planets[planetIndex], star.starLight);

                    //Adds the planet to the list of planets that will belong to the current solar system.
                    planets.Add(planet);
                }

                //Initializes the solar system using the saved data of the solar system and the star that was just instantiated from the same save data.
                solarSystem.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex], star);

                //Adds the solar system to the list of solar systems within the galaxy.
                solarSystems.Add(solarSystem);
            }
        }
        //Generates the solar systems of the galaxy from the new game data that has either been loaded in from the new game menu or specified through the inspector.
        else
        {
            //Fills a list with as many star type enum values as there will be stars in the galaxy using the probabilities specified through the inspector.
            List<GalaxyStar.StarType> starTypesRemaining = new List<GalaxyStar.StarType>();
            for (int starTypeIndex = 0; starTypeIndex < starTypeSpawnProbabilities.Length; starTypeIndex++)
            {
                int numberOfStarsOfType = (int)(newGameData.solarSystemCount * starTypeSpawnProbabilities[starTypeIndex]);
                for (int amountAdded = 0; amountAdded < numberOfStarsOfType; amountAdded++)
                {
                    starTypesRemaining.Add((GalaxyStar.StarType)starTypeIndex);
                }
            }
            while (starTypesRemaining.Count < newGameData.solarSystemCount)
            {
                starTypesRemaining.Add(0);
            }

            //Loads in the sprite from resources that contains the shape that specifies where stars can and cannot be placed.
            Sprite galaxyShapeSprite = Resources.Load<Sprite>("Galaxy/Galaxy Shapes/" + newGameData.galaxyShape);
            //Declares and initializes a variable that indicates how many planets are still left to be generated and placed within a solar system within the galaxy.
            int numberOfPlanetsRemaining = newGameData.planetCount;
            //Declares and initializes a list that contains the number of planets generated for each biome.
            List<int> planetsOfBiomeCount = new List<int>();
            for(int biomeIndex = 0; biomeIndex < biomes.Count; biomeIndex++)
            {
                planetsOfBiomeCount.Add(0);
            }
            //Loops that creates all of the solar systems in the galaxy.
            int remainderPlanets = newGameData.planetCount - ((newGameData.planetCount / newGameData.solarSystemCount) * newGameData.solarSystemCount);
            for (int solarSystemIndex = 0; solarSystemIndex < newGameData.solarSystemCount; solarSystemIndex++)
            {
                //Creates a new solar system by instianiating from the solar system prefab.
                GalaxySolarSystem solarSystem = Instantiate(solarSystemPrefab).GetComponent<GalaxySolarSystem>();
                //Sets the solar system's parent to the parent transform of all solar systems.
                solarSystem.transform.SetParent(solarSystemsParent);
                //Resets the position of the solar system.
                solarSystem.transform.localPosition = Vector3.zero;

                //Instantiates a star from a prefab based on a randomly picked star type from a list of star types that was filled up based on probablities specified in the inspector.
                int starTypeIndexInList = UnityEngine.Random.Range(0, starTypesRemaining.Count);
                GalaxyStar star = Instantiate(starTypePrefabs[(int)starTypesRemaining[starTypeIndexInList]]).GetComponent<GalaxyStar>();
                star.transform.SetParent(solarSystem.transform);
                star.InitializeFromGalaxyGenerator(starTypesRemaining[starTypeIndexInList]);
                star.transform.localPosition = Vector3.zero;
                starTypesRemaining.RemoveAt(starTypeIndexInList);

                //Loop that generates the planets of the solar system.
                List<NewGalaxyPlanet> planets = new List<NewGalaxyPlanet>();
                for(int planetIndex = 0; planetIndex < (newGameData.planetCount / newGameData.solarSystemCount) + 1; planetIndex++)
                {
                    //Breaks out of the loop if there are no more remainder planets left to place and the required ones in this solar system have already been placed.
                    if(planetIndex >= newGameData.planetCount / newGameData.solarSystemCount && remainderPlanets <= 0)
                        break;

                    //Generates a biome for the planet.
                    int biomeIndexWithLowestPlanetCount = -1;
                    List<int> biomeIndexs = new List<int>();
                    for(int biomeIndex = 0; biomeIndex < planetsOfBiomeCount.Count; biomeIndex++)
                    {
                        biomeIndexs.Add(biomeIndex);
                    }
                    while(biomeIndexs.Count > 0)
                    {
                        int biomeIndexIndex = UnityEngine.Random.Range(0, biomeIndexs.Count);
                        int biomeIndex = biomeIndexs[biomeIndexIndex];
                        bool planetaryOrbitAvailable = true;
                        for(int planetsAlreadyAddedIndex = 0; planetsAlreadyAddedIndex < planets.Count; planetsAlreadyAddedIndex++)
                        {
                            if(biomes[biomeIndex].planetaryOrbitProximityToStar == GetBiomeOfType(planets[planetsAlreadyAddedIndex].biomeType).planetaryOrbitProximityToStar)
                            {
                                planetaryOrbitAvailable = false;
                                break;
                            }
                        }
                        if (!planetaryOrbitAvailable)
                        {
                            biomeIndexs.RemoveAt(biomeIndexIndex);
                            continue;
                        }
                        if(biomeIndexWithLowestPlanetCount < 0)
                        {
                            biomeIndexWithLowestPlanetCount = biomeIndex;
                            biomeIndexs.RemoveAt(biomeIndexIndex);
                            continue;
                        }

                        if (planetsOfBiomeCount[biomeIndex] < planetsOfBiomeCount[biomeIndexWithLowestPlanetCount])
                            biomeIndexWithLowestPlanetCount = biomeIndex;
                        biomeIndexs.RemoveAt(biomeIndexIndex);
                    }
                    planetsOfBiomeCount[biomeIndexWithLowestPlanetCount] += 1;
                    NewGalaxyBiome biome = biomes[biomeIndexWithLowestPlanetCount];

                    //Instantiates a new empty gameobject for the planet to use an an orbit around the star.
                    GameObject planetaryOrbit = Instantiate(new GameObject());
                    //Names the planetary orbit based on how far it is from the star.
                    planetaryOrbit.name = "Planetary Orbit " + (biome.planetaryOrbitProximityToStar + 1);
                    //Sets the parent of the planetary orbit.
                    planetaryOrbit.transform.SetParent(solarSystem.planetaryOrbitsParent);
                    //Randomly sets the y rotation of the planetary orbit in order to give the planet a random rotation around the star.
                    planetaryOrbit.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

                    //Instantiates a new planet from the planet prefab.
                    NewGalaxyPlanet planet = Instantiate(planetPrefab).transform.GetChild(0).gameObject.GetComponent<NewGalaxyPlanet>();
                    //Parents the planet under its previously created planetary orbit.
                    planet.transform.parent.SetParent(planetaryOrbit.transform);
                    //Sets the planet's distance from the star based on the biome's specified proximity to the star.
                    planet.transform.parent.localPosition = new Vector3((star.localScale.x / 2) + spaceBetweenStarAndPlanetaryOrbits + (spaceBetweenPlanetaryOrbits * biome.planetaryOrbitProximityToStar), planet.transform.parent.localPosition.y, planet.transform.parent.localPosition.z);
                    //Initializes all needed variables of the planet.
                    planet.InitializeFromGalaxyGenerator(biome, star.starLight);

                    //Adds the planet to the list of planets that will belong to the current solar system.
                    planets.Add(planet);
                }

                //Initializes the variables in the solar system.
                solarSystem.InitializeFromGalaxyGenerator(star, planets);

                //Declares and initializes a variable that will be used in order to ensure that not too many failed attempts are made to place the solar system within the galaxy.
                int solarSystemPlacementAttempsMade = 0;
                //Loops until either the solar system has been successfully placed or the maximum amount of attempts is reached.
                while (true)
                {
                    //Checks if the maximum amount of attempts allowed to fail validly positioning the solar system within the galaxy has been reached and stops attempting to place the solar system if so.
                    if (solarSystemPlacementAttempsMade >= maxFailedSolarSystemPlacementAttemps)
                        break;
                    //Assigns the solar system a random position where it still appears on the screen.
                    solarSystem.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1920 + (solarSystem.star.localScale.x / 2), 1921 - (solarSystem.star.localScale.x / 2)), solarSystem.transform.localPosition.y, UnityEngine.Random.Range(-1080 + (solarSystem.star.localScale.z / 2), 1081 - (solarSystem.star.localScale.z / 2)));
                    //Array of coordinates that will need to be checked in order to ensure that they are all within the non-transparent parts of the galaxy shape image.
                    Vector2Int[] coordinatesToCheck = new Vector2Int[4];
                    //Left coordinate check.
                    coordinatesToCheck[0] = new Vector2Int((((int)solarSystem.transform.localPosition.x + 1920) - (int)(solarSystem.star.localScale.x / 2)), (int)solarSystem.transform.localPosition.z + 1080);
                    //Upper coordinate check.
                    coordinatesToCheck[1] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920), (((int)solarSystem.transform.localPosition.z) + 1080) + (int)(solarSystem.star.localScale.z / 2));
                    //Right coordinate check.
                    coordinatesToCheck[2] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920) + (int)(solarSystem.star.localScale.x / 2), (int)solarSystem.transform.localPosition.z + 1080);
                    //Lower coordinate check.
                    coordinatesToCheck[3] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920), (((int)solarSystem.transform.localPosition.z) + 1080) - (int)(solarSystem.star.localScale.z / 2));
                    //Loops through the left, upper, right, and lower coordinates to make sure that they are at coordinates that are on the screen and not transparent in the galaxy shape image.
                    bool validCoordinates = true;
                    for (int index = 0; index < coordinatesToCheck.Length; index++)
                    {
                        if (coordinatesToCheck[index].x < 0 || coordinatesToCheck[index].x >= 3840 || coordinatesToCheck[index].y < 0 || coordinatesToCheck[index].y >= 2160 || galaxyShapeSprite.texture.GetPixel(coordinatesToCheck[index].x, coordinatesToCheck[index].y).a == 0)
                        {
                            validCoordinates = false;
                            break;
                        }
                    }
                    //Loops through each solar system that has already been validly positioned and ensures that the new position of the new solar is not too close to it.
                    for (int alreadyPositionedSolarSystemIndex = 0; alreadyPositionedSolarSystemIndex < solarSystems.Count; alreadyPositionedSolarSystemIndex++)
                    {
                        if (solarSystem.transform.localPosition.x + (solarSystem.star.localScale.x / 2) + minimumSpaceBetweenStars >= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.x - (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.x / 2) && solarSystem.transform.localPosition.x - (solarSystem.star.localScale.x / 2) - minimumSpaceBetweenStars <= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.x + (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.x / 2) && solarSystem.transform.localPosition.z + (solarSystem.star.localScale.z / 2) + minimumSpaceBetweenStars >= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.z - (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.z / 2) && solarSystem.transform.localPosition.z - (solarSystem.star.localScale.z / 2) - minimumSpaceBetweenStars <= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.z + (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.z / 2))
                        {
                            validCoordinates = false;
                            break;
                        }
                    }
                    //If the coordinates are valid, the solar system is added to the list of solar system in the galaxy and we move on to positioning the next solar system.
                    if (validCoordinates)
                    {
                        solarSystems.Add(solarSystem);
                        break;
                    }
                    //Increments the variable that indicates how many attempts have been made to position the current solar system validly.
                    solarSystemPlacementAttempsMade++;
                }
                //Checks if the maximum amount of attempts allowed to fail validly positioning the solar system within the galaxy has been reached and stops attempting to place any more solar systems if so. Also, a debug warning is put into the console.
                if (solarSystemPlacementAttempsMade >= maxFailedSolarSystemPlacementAttemps)
                {
                    Debug.LogWarning("Maximum amount of allowed attempts to fail to validly position a solar system reached. Will stop adding any more solar systems to the galaxy.");
                    Destroy(solarSystem.gameObject);
                    break;
                }
                //Adds the solar system just worked on to the list of all solar systems within the galaxy.
                solarSystems.Add(solarSystem);
                //Decrements the number of remainder planets.
                remainderPlanets--;
            }
        }
    }

    /// <summary>
    /// Private function that returns the biome of the specified biome type.
    /// </summary>
    /// <param name="biomeType"></param>
    /// <returns></returns>
    private NewGalaxyBiome GetBiomeOfType(Planet.Biome biomeType)
    {
        foreach(NewGalaxyBiome biome in biomes)
        {
            if (biome.biome == biomeType)
                return biome;
        }
        return null;
    }

    /// <summary>
    /// Public static method that should be used in order to determine the max amount of planets able to be fit into the specified galaxy shape.
    /// </summary>
    /// <param name="galaxyShape"></param>
    /// <returns></returns>
    public static int GetMaxPlanetsCount(string galaxyShape)
    {
        if (galaxyShape.Equals("Spiral"))
            return 60;
        Debug.LogWarning("Specified galaxy type does not have an assigned max planets count. Will return default value of 60.");
        return 60;
    }
}

[System.Serializable]
public class NewGalaxyBiome
{
    [SerializeField, Tooltip("Specifies the type of biome.")] private Planet.Biome _biome = Planet.Biome.Unknown;
    [SerializeField, Tooltip("The chance that a planet of this biome will spawn with a ring (Range: 0-1).")] private float _planetaryRingChance = 0.2f;
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can have its rings be.")] private Vector2 planetaryRingSizeRange = new Vector2(0.25f, 0.69f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can be.")] private Vector2 planetarySizeRange = new Vector2(0.2f, 0.3f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) speeds that a planet of this biome can have clouds moving.")] private Vector2 cloudSpeedRange = new Vector2(15, 40);
    [SerializeField, Tooltip("Specifies what planetary orbit of the solar system the planet will be on (0-1 with 0 being the closest to the sun).")] private int _planetaryOrbitProximityToStar = 0;
    [SerializeField, Tooltip("The list of names of planet materials that can be used on planets that belong to this biome.")] private List<string> planetMaterialNames = new List<string>();
    [SerializeField, Tooltip("The first color in each set is the color of the clouds and the second color in each set is the color of their shadow.")] private List<DualColorSet> cloudColorCombos = new List<DualColorSet>();
    [SerializeField, Tooltip("List of colors that could possibly be applied to cities on planets of this biome.")] private List<Color> cityColors = new List<Color>();
    [SerializeField, Tooltip("List of dual color sets that could possibly be applied to rings of planets of this biome.")] private List<DualColorSet> ringColorCombos = new List<DualColorSet>();

    //Non-inspector variables.

    private List<string> planetMaterialNamesUsed = new List<string>();

    /// <summary>
    /// Public property that should be used to access the type of biome.
    /// </summary>
    public Planet.Biome biome { get => _biome; }

    /// <summary>
    /// Public property that should be used to access the percentage chance (0-1) for each planet of this biome to have a ring.
    /// </summary>
    public float planetaryRingChance { get => _planetaryRingChance; }

    /// <summary>
    /// Public property that should be used to access a random planetary material name of the biome.
    /// </summary>
    public string randomMaterialName
    {
        get
        {
            //Clones planet material names list.
            List<string> localPlanetMaterialNames = new List<string>(planetMaterialNames);
            if (localPlanetMaterialNames.Count <= planetMaterialNamesUsed.Count)
                planetMaterialNamesUsed.Clear();
            else if (planetMaterialNamesUsed.Count > 0)
            {
                //Removes each planet material name already used.
                foreach (string planetMaterialName in planetMaterialNamesUsed)
                    localPlanetMaterialNames.Remove(planetMaterialName);
            }
            //Gets a random planet material name not already used.
            string randomPlanetMaterialName = localPlanetMaterialNames[UnityEngine.Random.Range(0, localPlanetMaterialNames.Count)];
            //Marks the random planet material name as used.
            planetMaterialNamesUsed.Add(randomPlanetMaterialName);
            return randomPlanetMaterialName;
        }
    }

    /// <summary>
    /// Public property that should be used in order to access a new random ring size from the range of valid ring sizes for the biome.
    /// </summary>
    public float randomRingSize { get => UnityEngine.Random.Range(planetaryRingSizeRange.x, planetaryRingSizeRange.y); }

    /// <summary>
    /// Public property that should be used in order to access a new random planetary size from the range of valid planetary sizes for the biome.
    /// </summary>
    public float randomPlanetarySize { get => UnityEngine.Random.Range(planetarySizeRange.x, planetarySizeRange.y); }

    /// <summary>
    /// Public property that should be used in order to access a new random cloud speed from the range of valid cloud speeds for the biome.
    /// </summary>
    public float randomCloudSpeed { get => UnityEngine.Random.Range(cloudSpeedRange.x, cloudSpeedRange.y); }

    /// <summary>
    /// Public property that should be used in order to access a new random cloud color combo from the list of valid cloud color combos for the biome.
    /// </summary>
    public DualColorSet randomCloudColorCombo { get => cloudColorCombos[UnityEngine.Random.Range(0, cloudColorCombos.Count)]; }

    /// <summary>
    /// Public property that should be used in order to access a new random city color from the list of valid city colors for the biome.
    /// </summary>
    public Color randomCityColor { get => cityColors[UnityEngine.Random.Range(0, cityColors.Count)]; }

    /// <summary>
    /// Public property that should be used in order to access a new random ring color combo from the list of valid ring color combos for the biome.
    /// </summary>
    public DualColorSet randomRingColorCombo { get => ringColorCombos[UnityEngine.Random.Range(0, ringColorCombos.Count)]; }

    /// <summary>
    /// Public property that should be used in order to access how close the planetary orbit of planet's of this biome are to the star of the solar system.
    /// </summary>
    public int planetaryOrbitProximityToStar { get => _planetaryOrbitProximityToStar; }
}