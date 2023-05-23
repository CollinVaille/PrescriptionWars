using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NewGalaxyGenerator : MonoBehaviour
{
    [Header("Material Options")]

    [SerializeField, Tooltip("The material that will be applied to the skybox while the galaxy scene is open and the galaxy is active in the project hierarchy.")] private Material skyboxMaterial = null;

    [Header("New Game Data")]

    [SerializeField, Tooltip("Holds either the new game data passed from the new game menu or the new game data selected through the inspector.")] private NewGameData newGameData = new NewGameData();
    [SerializeField, Tooltip("The minimum amount of space that must exist between all stars in the galaxy.")] private int minimumSpaceBetweenStars = 100;
    [SerializeField, Tooltip("Array that contains the probability for each star type to be assigned to a star. The enum index of the star type correlates to the same index in this array.")] private float[] starTypeSpawnProbabilities = new float[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];
    [SerializeField, Tooltip("Indicates the amount of space between the star and the first planetary orbit in the solar system that may or may not have a planet on it.")] private float spaceBetweenStarAndPlanetaryOrbits = 10;
    [SerializeField, Tooltip("Indicates the amount of space between each planetary orbit in the solar system that may or may not have a planet on it.")] private float spaceBetweenPlanetaryOrbits = 5;

    [Header("Biome Options")]

    [SerializeField, Tooltip("List that specifies the data for every biome.")] private List<NewGalaxyBiome> biomes = new List<NewGalaxyBiome>();

    [Header("Parents")]

    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the solar systems in the galaxy. Specified through the inspector.")] private Transform solarSystemsParent = null;
    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the planet labels in the galaxy. Specified through the inspector.")] private Transform planetLabelsParent = null;
    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the star labels in the galaxy. Specified through the inspector.")] private Transform starLabelsParent = null;
    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the hyperspace lanes within the galaxy. Specified through the inspector.")] private Transform hyperspaceLanesParent = null;
    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the capital symbols in the galaxy. Specified through the inspector.")] private Transform capitalSymbolsParent = null;
    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the confirmation popups within the galaxy scene. Specified through the inspector.")] private Transform confirmationPopupsParent = null;

    [Header("Prefabs")]

    [SerializeField, Tooltip("The prefab that all solar systems in the galaxy will be instanitated from. Specified through the inspector.")] private GameObject solarSystemPrefab = null;
    [SerializeField, Tooltip("Array that contains the prefab for each star type where the enum index of the star type correlates to the same index in this array.")] private GameObject[] starTypePrefabs = new GameObject[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];
    [SerializeField, Tooltip("The prefab that all planets in every solar system in the galaxy will be instantiated from.")] private GameObject planetPrefab = null;
    [SerializeField, Tooltip("The prefab that all hyperspace lanes connecting solar systems within the galaxy will be instantiated from.")] private GameObject hyperspaceLanePrefab = null;

    [Header("Menus")]

    [SerializeField, Tooltip("The pause menu that the player can interact with to perform a variety of actions such as either resuming or saving and exiting the game.")] private NewGalaxyPauseMenu pauseMenu = null;
    [SerializeField, Tooltip("The settings menu that the player can interact with to change a variety of settings including game, video, and audio settings.")] private NewGalaxySettingsMenu settingsMenu = null;
    [SerializeField, Tooltip("The cheat console that allows the player to cheat mid game on the galaxy view.")] private NewCheatConsole cheatConsole = null;

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
    /// Holds all of the planets that have already been fully generated and placed appropriately into the galaxy.
    /// </summary>
    private List<NewGalaxyPlanet> planets = null;

    /// <summary>
    /// Holds all of the hyperspace lanes that have already been fully generated and placed appropriately within the galaxy.
    /// </summary>
    private List<HyperspaceLane> hyperspaceLanes = null;

    /// <summary>
    /// Private variable that holds any save game data that might be passed over statically from the load game menu.
    /// </summary>
    private GalaxyData saveGameData = null;

    /// <summary>
    /// Indicates the default amount of solar systems in the galaxy if the galaxy shape does not specify an amount.
    /// </summary>
    public static int defaultSolarSystemCount { get => 60; }

    /// <summary>
    /// Private property that should be used to specifically access the prefab of the default yellow dwarf star.
    /// </summary>
    private GameObject yellowDwarfStarPrefab
    {
        get
        {
            foreach(GameObject starTypePrefab in starTypePrefabs)
            {
                if (starTypePrefab.name.Contains("Yellow Dwarf"))
                    return starTypePrefab;
            }
            return null;
        }
    }

    /// <summary>
    /// Private variable that holds all of the empires that have been fully generated and positioned within the galaxy.
    /// </summary>
    private List<NewEmpire> empires = null;

    /// <summary>
    /// Private variable that holds a dictionary of functions to be executed once the galaxy has finished generating completely with the int indicating each function's execution order number.
    /// </summary>
    private Dictionary<int, List<Action>> galaxyGenerationCompletionFunctions = new Dictionary<int, List<Action>>();

    /// <summary>
    /// Private static galaxy generator instance.
    /// </summary>
    private static NewGalaxyGenerator galaxyGenerator = null;

    private void Awake()
    {
        galaxyGenerator = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Loads in any galaxy save game data that might be coming from the load game menu.
        LoadInSaveGameData();

        //Loads in any new game data that might be coming from the new game menu.
        LoadInNewGameData();

        //Resets any static helper functions that need to be reset.
        ResetHelperFunctions();

        //Generates the celestial bodies of the galaxy.
        GenerateCelestialBodies();

        //Generates the empires of the galaxy.
        GenerateEmpires();

        //Generates the hyperspace lanes of the galaxy.
        GenerateHyperspaceLanes();

        //Sets the material of the galaxy scene's skybox.
        RenderSettings.skybox = skyboxMaterial;

        //Initializes the resource modifiers.
        Dictionary<int, GalaxyResourceModifier> resourceModifiers = new Dictionary<int, GalaxyResourceModifier>();
        if (saveGameData != null)
            foreach (int resourceModifierID in saveGameData.resourceModifiers.Keys)
                resourceModifiers.Add(resourceModifierID, new GalaxyResourceModifier(saveGameData.resourceModifiers[resourceModifierID]));

        //Initializes the galaxy manager.
        NewGalaxyManager.InitializeFromGalaxyGenerator(
            gameObject.GetComponent<NewGalaxyManager>(),
            saveGameData != null ? saveGameData.saveName : null,
            skyboxMaterial,
            solarSystems,
            planets,
            empires,
            hyperspaceLanes,
            saveGameData != null ? saveGameData.galaxyShape : newGameData.galaxyShape,
            saveGameData != null ? saveGameData.playerID : 0,
            saveGameData != null ? saveGameData.observationModeEnabled : false,
            new List<Transform>() { planetLabelsParent, starLabelsParent, capitalSymbolsParent, confirmationPopupsParent },
            pauseMenu,
            settingsMenu,
            cheatConsole,
            saveGameData != null ? saveGameData.turnNumber : 0,
            resourceModifiers,
            saveGameData != null ? saveGameData.resourceModifiersCount : 0);

        //Executes all of the functions that need to be executed once the galaxy has completely finished generating.
        OnGalaxyGenerationCompletion();

        /*NewGalaxyManager.saveName = "Test Save";
        GalaxySaveSystem.SaveGalaxy();
        Debug.Log("Saved");*/

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
    /// Private method that should be called in the start method after the celestial bodies have been generated and generates the empires of the galaxy.
    /// </summary>
    private void GenerateEmpires()
    {
        //Initializes the list of empires.
        empires = new List<NewEmpire>();
        //Generates the empires of the galaxy based on save data if any exists.
        if(saveGameData != null)
        {
            //Loops through each empire in the save game data and recreates it and adds it to the list of empires.
            foreach(EmpireData empireData in saveGameData.empires)
                empires.Add(new NewEmpire(empireData));
        }
        //Generates the empires based on the new game settings if there is no save game data.
        else
        {
            //Declares and initializes a list that will eventually filled with the IDs of the empires that will get a bonus leftover solar system.
            List<int> empiresGettingBonusSolarSystem = new List<int>();
            //Fills a list with every possible empire ID that could get a bonus leftover solar system.
            List<int> empireIDsRemainingForBonusSolarSystem = new List<int>();
            for(int empireID = 0; empireID < newGameData.empireCount; empireID++)
                empireIDsRemainingForBonusSolarSystem.Add(empireID);
            //Loops through each leftover solar system remaining and assigns it to an empire ID.
            for(int leftOverPlanetIndex = 0; leftOverPlanetIndex < solarSystems.Count % newGameData.empireCount; leftOverPlanetIndex++)
            {
                int empireIDRemainingIndexChosen = UnityEngine.Random.Range(0, empireIDsRemainingForBonusSolarSystem.Count);
                empiresGettingBonusSolarSystem.Add(empireIDsRemainingForBonusSolarSystem[empireIDRemainingIndexChosen]);
                empireIDsRemainingForBonusSolarSystem.RemoveAt(empireIDRemainingIndexChosen);
            }

            //Fills a list with valid cultures for an empire while attempting to keep as much uniqueness as possible.
            List<NewEmpire.Culture> cultures = new List<NewEmpire.Culture>();
            while (cultures.Count < newGameData.empireCount)
            {
                List<NewEmpire.Culture> tempCultures = new List<NewEmpire.Culture>();
                for(int cultureIndex = 0; cultureIndex < NewEmpire.cultureCount; cultureIndex++)
                    tempCultures.Add((NewEmpire.Culture)cultureIndex);
                while(tempCultures.Count > 0)
                {
                    if (cultures.Count >= newGameData.empireCount)
                        break;
                    int randomTempCultureIndex = UnityEngine.Random.Range(0, tempCultures.Count);
                    cultures.Add(tempCultures[randomTempCultureIndex]);
                    tempCultures.RemoveAt(randomTempCultureIndex);
                }
            }
            //Removes one instance of the player's chosen empire culture from the list of chooseable cultures still remaining to even out the odds.
            for(int cultureListIndex = 0; cultureListIndex < cultures.Count; cultureListIndex++)
            {
                if(cultures[cultureListIndex] == newGameData.playerEmpireCulture)
                {
                    cultures.RemoveAt(cultureListIndex);
                    break;
                }
            }
            //Generates the empires and adds them to the list of empires within the galaxy.
            for(int empireIndex = 0; empireIndex < newGameData.empireCount; empireIndex++)
            {
                //Generates the culture of the empire.
                NewEmpire.Culture empireCulture = newGameData.playerEmpireCulture;
                if (empireIndex != 0)
                {
                    int culturesListIndex = UnityEngine.Random.Range(0, cultures.Count);
                    empireCulture = cultures[culturesListIndex];
                    cultures.RemoveAt(culturesListIndex);
                }

                //Randomly picks a system that is still unowned up until now to be this empire's capital system.
                int capitalSystemID = -1;
                for(int solarSystemIndex = 0; solarSystemIndex < solarSystems.Count; solarSystemIndex++)
                {
                    bool solarSystemOwned = false;
                    for(int alreadyGeneratedEmpireIndex = 0; alreadyGeneratedEmpireIndex < empires.Count; alreadyGeneratedEmpireIndex++)
                    {
                        if (empires[alreadyGeneratedEmpireIndex].solarSystemIDs.Contains(solarSystemIndex))
                        {
                            solarSystemOwned = true;
                            break;
                        }
                    }
                    if (!solarSystemOwned)
                    {
                        capitalSystemID = solarSystemIndex;
                        break;
                    }
                }
                //Initializes the list of solar systems owned by the empire.
                List<int> empireSolarSystemIDs = new List<int>();
                //Adds the capital system to the list of systems owned by the empire.
                empireSolarSystemIDs.Add(capitalSystemID);

                //Declares and initializes a list that will contain the distance of each unowned solar system to the empire's capital system in ascending sorted order.
                List<KeyValuePair<int, float>> solarSystemDistanceFromCapital = new List<KeyValuePair<int, float>>();
                //Loops through each solar system to see if it is still unowned and if so then it adds the solar system and its distance from the empire's capital system to the list.
                for (int solarSystemIndex = 1; solarSystemIndex < solarSystems.Count; solarSystemIndex++)
                {
                    bool solarSystemOwned = false;
                    for (int alreadyGeneratedEmpireIndex = 0; alreadyGeneratedEmpireIndex < empires.Count; alreadyGeneratedEmpireIndex++)
                    {
                        if (empires[alreadyGeneratedEmpireIndex].solarSystemIDs.Contains(solarSystemIndex) || capitalSystemID == solarSystemIndex)
                        {
                            solarSystemOwned = true;
                            break;
                        }
                    }
                    if (!solarSystemOwned)
                    {
                        solarSystemDistanceFromCapital.Add(new KeyValuePair<int, float>(solarSystemIndex, Vector2.Distance(new Vector2(solarSystems[solarSystemIndex].transform.localPosition.x, solarSystems[solarSystemIndex].transform.localPosition.z), new Vector2(solarSystems[capitalSystemID].transform.localPosition.x, solarSystems[capitalSystemID].transform.localPosition.z))));
                    }
                }
                //Sorts the list into ascending distance from the empire's capital system order.
                solarSystemDistanceFromCapital.Sort((x, y) => x.Value.CompareTo(y.Value));

                //Adds the closest unowned solar system to the empire's capital solar system until enough solar systems have been added to the empire's control, possibly including a bonus leftover solar system.
                for(int solarSystemNumber = 1; solarSystemNumber < (empiresGettingBonusSolarSystem.Contains(empireIndex) ? (solarSystems.Count / newGameData.empireCount) + 1 : solarSystems.Count / newGameData.empireCount); solarSystemNumber++)
                {
                    empireSolarSystemIDs.Add(solarSystemDistanceFromCapital[solarSystemNumber - 1].Key);
                }

                //Initializes the list of planets owned by the empire.
                List<int> empirePlanetIDs = new List<int>();
                for(int empireSolarSystemIndex = 0; empireSolarSystemIndex < empireSolarSystemIDs.Count; empireSolarSystemIndex++)
                {
                    for(int planetIndex = 0; planetIndex < solarSystems[empireSolarSystemIDs[empireSolarSystemIndex]].planets.Count; planetIndex++)
                    {
                        empirePlanetIDs.Add(solarSystems[empireSolarSystemIDs[empireSolarSystemIndex]].planets[planetIndex].ID);
                    }
                }

                //Generates the name of the empire.
                string empireName = empireIndex == 0 ? newGameData.playerEmpireName : EmpireNameGenerator.GenerateEmpireName();

                //Generates the color of the empire.
                Color empireColor = GetRandomColorBasedOnCulture(empireCulture);

                //Generates the flag of the empire.
                NewFlag empireFlag = empireIndex == 0 && newGameData.playerEmpireFlag != null ? newGameData.playerEmpireFlag : new NewFlag(FlagDataLoader.flagSymbolNames[UnityEngine.Random.Range(0, FlagDataLoader.flagSymbolNames.Length)], empireColor.r + empireColor.g + empireColor.b < 0.6f ? Color.white : Color.black, empireColor);

                //Adds the new empire to the list of empires existing within the galaxy.
                empires.Add(new NewEmpire(empireName, empireCulture, empireColor, empireFlag, empireIndex, capitalSystemID, empireSolarSystemIDs, empirePlanetIDs, 0, 0));
            }
        }
    }

    /// <summary>
    /// Private method that should be called in the start method and generates the celestial bodies of the galaxy.
    /// </summary>
    private void GenerateCelestialBodies()
    {
        //Initializes the list of solar systems.
        solarSystems = new List<GalaxySolarSystem>();
        //Initializes the list of planets.
        planets = new List<NewGalaxyPlanet>();
        //Generates the solar systems of the galaxy from the galaxy save game data that has been loaded in from the load game menu if it exists.
        if (saveGameData != null)
        {
            //Sets the local y rotation of the galaxy from the save data.
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, saveGameData.galaxyLocalYRotation, transform.localRotation.eulerAngles.z);
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
                star.InitializeFromSaveData(solarSystem, saveGameData.solarSystems[solarSystemIndex].star);
                star.transform.localPosition = Vector3.zero;

                //Loop that instantiates the planets of the solar system using the list of planet data of the current solar system in the galaxy save data loaded in.
                List<NewGalaxyPlanet> solarSystemPlanets = new List<NewGalaxyPlanet>();
                for (int planetIndex = 0; planetIndex < saveGameData.solarSystems[solarSystemIndex].planets.Count; planetIndex++)
                {
                    //Instantiates a new empty gameobject for the planet to use an an orbit around the star.
                    GameObject planetaryOrbit = new GameObject();
                    //Names the planetary orbit based on how far it is from the star.
                    planetaryOrbit.name = "Planetary Orbit " + (saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].planetaryOrbitProximityToStar + 1);
                    //Sets the parent of the planetary orbit.
                    planetaryOrbit.transform.SetParent(solarSystem.planetaryOrbitsParent);
                    //Randomly sets the y rotation of the planetary orbit in order to give the planet a random rotation around the star.
                    planetaryOrbit.transform.localRotation = Quaternion.Euler(0, saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].planetaryOrbitRotation, 0);

                    //Instantiates a new planet from the planet prefab.
                    NewGalaxyPlanet planet = Instantiate(planetPrefab).GetComponent<NewGalaxyPlanet>();
                    //Parents the planet under its previously created planetary orbit.
                    planet.transform.SetParent(planetaryOrbit.transform);
                    //Sets the planet's distance from the star based on the biome's specified proximity to the star.
                    planet.transform.localPosition = new Vector3(saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[0], saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[1], saveGameData.solarSystems[solarSystemIndex].planets[planetIndex].localPosition[2]);
                    //Initializes all needed variables of the planet.
                    planet.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex].planets[planetIndex], solarSystem, planets.Count, star.starLight);

                    //Adds the planet to the list of planets that will belong to the current solar system.
                    solarSystemPlanets.Add(planet);
                    //Adds the planet to the list of planets within the galaxy.
                    planets.Add(planet);
                }

                //Initializes the solar system using the saved data of the solar system and the star that was just instantiated from the same save data.
                solarSystem.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex], star, solarSystemPlanets, solarSystems.Count);

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
                star.InitializeFromGalaxyGenerator(solarSystem, starTypesRemaining[starTypeIndexInList], StarNameGenerator.GenerateStarName());
                star.transform.localPosition = Vector3.zero;
                starTypesRemaining.RemoveAt(starTypeIndexInList);

                //Loop that generates the planets of the solar system.
                List<NewGalaxyPlanet> solarSystemPlanets = new List<NewGalaxyPlanet>();
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
                        for(int planetsAlreadyAddedIndex = 0; planetsAlreadyAddedIndex < solarSystemPlanets.Count; planetsAlreadyAddedIndex++)
                        {
                            if(biomes[biomeIndex].planetaryOrbitProximityToStar == GetBiomeOfType(solarSystemPlanets[planetsAlreadyAddedIndex].biomeType).planetaryOrbitProximityToStar)
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

                    //Generates a name for the planet.
                    string planetName = PlanetNameGenerator.GeneratePlanetName();

                    //Generates a city with buildings for the planet.
                    List<NewGalaxyBuilding> planetCityBuildings = new List<NewGalaxyBuilding>();
                    NewGalaxyBuilding.BuildingType[] buildingTypes = (NewGalaxyBuilding.BuildingType[])Enum.GetValues(typeof(NewGalaxyBuilding.BuildingType));
                    if(planetIndex == 0)    //System capital has one of every building type.
                        foreach (NewGalaxyBuilding.BuildingType buildingType in buildingTypes)
                            planetCityBuildings.Add(new NewGalaxyBuilding(buildingType, planets.Count));
                    else    //Non-capital planets within a solar system have a random amount of buildings (at least one) but cannot have more than one building of a single type, just like the capital planet.
                    {
                        int buildingsCount = UnityEngine.Random.Range(1, buildingTypes.Length + 1);
                        List<int> buildingTypesIndicesRemaining = new List<int>();
                        for (int buildingTypeIndex = 0; buildingTypeIndex < buildingTypes.Length; buildingTypeIndex++)
                            buildingTypesIndicesRemaining.Add(buildingTypeIndex);
                        for (int buildingIndex = 0; buildingIndex < buildingsCount; buildingIndex++)
                        {
                            int buildingTypesIndicesRemainingIndex = UnityEngine.Random.Range(0, buildingTypesIndicesRemaining.Count);
                            planetCityBuildings.Add(new NewGalaxyBuilding(buildingTypes[buildingTypesIndicesRemaining[buildingTypesIndicesRemainingIndex]], planets.Count));
                            buildingTypesIndicesRemaining.RemoveAt(buildingTypesIndicesRemainingIndex);
                        }
                    }
                    NewGalaxyCity planetCity = new NewGalaxyCity(planetName + " City", planetCityBuildings, planets.Count);

                    //Instantiates a new empty gameobject for the planet to use an an orbit around the star.
                    GameObject planetaryOrbit = new GameObject();
                    //Names the planetary orbit based on how far it is from the star.
                    planetaryOrbit.name = "Planetary Orbit " + (biome.planetaryOrbitProximityToStar + 1);
                    //Sets the parent of the planetary orbit.
                    planetaryOrbit.transform.SetParent(solarSystem.planetaryOrbitsParent);
                    //Randomly sets the y rotation of the planetary orbit in order to give the planet a random rotation around the star.
                    planetaryOrbit.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

                    //Instantiates a new planet from the planet prefab.
                    NewGalaxyPlanet planet = Instantiate(planetPrefab).GetComponent<NewGalaxyPlanet>();
                    //Parents the planet under its previously created planetary orbit.
                    planet.transform.SetParent(planetaryOrbit.transform);
                    //Sets the planet's distance from the star based on the biome's specified proximity to the star.
                    planet.transform.localPosition = new Vector3((star.localScale.x / 2) + (spaceBetweenStarAndPlanetaryOrbits * (star.localScale.x / yellowDwarfStarPrefab.transform.localScale.x)) + (spaceBetweenPlanetaryOrbits * biome.planetaryOrbitProximityToStar), planet.transform.localPosition.y, planet.transform.localPosition.z);
                    //Initializes all needed variables of the planet.
                    planet.InitializeFromGalaxyGenerator(solarSystem, planetCity, planets.Count, planetName, biome, star.starLight);

                    //Adds the planet to the list of planets that will belong to the current solar system.
                    solarSystemPlanets.Add(planet);
                    //Adds the planet to the list of planets that are in the galaxy.
                    planets.Add(planet);
                }

                //Initializes the variables in the solar system.
                solarSystem.InitializeFromGalaxyGenerator(star, solarSystemPlanets, solarSystems.Count);

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
                        //Adds the solar system just worked on to the list of all solar systems within the galaxy.
                        solarSystems.Add(solarSystem);
                        //Decrements the number of remainder planets.
                        remainderPlanets--;
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
                    foreach (NewGalaxyPlanet solarSystemPlanet in solarSystemPlanets)
                    {
                        solarSystemPlanet.RemoveOnGalaxyGenerationCompletionFunction();
                        Destroy(solarSystemPlanet);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Private method that should be called in the start method after the celestial bodies have been generated and generates the hyperspace lanes connecting solar systems within the galaxy.
    /// </summary>
    private void GenerateHyperspaceLanes()
    {
        //Initializes the list of hyperspace lanes.
        hyperspaceLanes = new List<HyperspaceLane>();
        //Generates the hyperspace lanes of the galaxy from the galaxy save game data that has been loaded in from the load game menu if it exists.
        if (saveGameData != null)
        {
            //Loops through each hyperspace lane that had its data stored in the save game file and creates a new replica hyperspace lane.
            foreach (HyperspaceLaneData hyperspaceLaneData in saveGameData.hyperspaceLanes)
                CreateHyperspaceLane(hyperspaceLaneData.solarSystemIDs[0], hyperspaceLaneData.solarSystemIDs[1]);
        }
        else
        {
            //----------------------------------------------------------------------------------------------------
            //FIRST, PERFORM SOME SET UP

            //Get solar system positions.
            Vector3[] solarSystemPositions = new Vector3[solarSystems.Count];
            for (int x = 0; x < solarSystemPositions.Length; x++)
                solarSystemPositions[x] = solarSystems[x].transform.localPosition;

            //----------------------------------------------------------------------------------------------------
            //THEN, PERFORM KRUSKAL'S MINIMUM SPANNING TREE TO ENSURE CONNECTEDNESS OF ENTIRE GALAXY!!!!!!

            //Get all possible lanes and their weights, i.e. distances
            NewPossibleHyperspaceLane[] possibleLanes = new NewPossibleHyperspaceLane[solarSystems.Count * solarSystems.Count];
            int nextWeightIndex = 0;
            for (int x = 0; x < solarSystems.Count; x++)
            {
                for (int y = 0; y < solarSystems.Count; y++)
                {
                    if (x == y) //Do not allow self-loops
                        possibleLanes[nextWeightIndex++] = new NewPossibleHyperspaceLane(Mathf.Infinity, x, y);
                    else
                        possibleLanes[nextWeightIndex++] = new NewPossibleHyperspaceLane(Vector3.Distance(solarSystemPositions[x], solarSystemPositions[y]), x, y);
                }
            }

            //Sort possible lanes by weight, i.e. distance
            //This ensures that we try to add the shortest hyperspace lanes first
            System.Array.Sort(possibleLanes);

            //Initially, each solar system is it's own tree
            List<HashSet<int>> indexTrees = new List<HashSet<int>>();
            for (int x = 0; x < solarSystems.Count; x++)
            {
                HashSet<int> newTree = new HashSet<int>();
                newTree.Add(x);
                indexTrees.Add(newTree);
            }

            //Add hyperspace lanes until all solar systems are in a single connected tree
            //But only add a lane if it serves to connect two trees
            int possibleLaneIndex = 0;
            while (indexTrees.Count > 1)
            {
                NewPossibleHyperspaceLane possibleLane = possibleLanes[possibleLaneIndex++];

                //Determine which tree each solar system belongs to
                int solarSystem1Tree = -1;
                int solarSystem2Tree = -1;
                for (int x = 0; x < indexTrees.Count; x++)
                {
                    if (indexTrees[x].Contains(possibleLane.solarSystem1Index))
                        solarSystem1Tree = x;

                    if (indexTrees[x].Contains(possibleLane.solarSystem2Index))
                        solarSystem2Tree = x;
                }

                //Disjoint trees, so include hyperspace line
                if (solarSystem1Tree != solarSystem2Tree)
                {
                    //Merge trees
                    indexTrees[solarSystem1Tree].UnionWith(indexTrees[solarSystem2Tree]); //1 becomes union of 1 and 2
                    indexTrees.RemoveAt(solarSystem2Tree); //2 gets discarded

                    //Include hyperspace lane
                    CreateHyperspaceLane(possibleLane.solarSystem1Index, possibleLane.solarSystem2Index);
                }
            }
            //----------------------------------------------------------------------------------------------------
            //FINALLY, ENSURE ALL LANES ARE ADDED WITHIN CERTAIN DISTANCE THRESHOLD (CIARAN'S ALGORITHM)

            //Add hyperspace lanes for planets within certain distance of each other, regardless of connectedness
            for (int x = 0; x < solarSystems.Count; x++)
            {
                for (int y = 0; y < solarSystems.Count; y++)
                {
                    if (x != y && Vector3.Distance(solarSystems[x].transform.localPosition, solarSystems[y].transform.localPosition) <= newGameData.hyperspaceLaneCheckingRadius)
                    {
                        CreateHyperspaceLane(x, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Private method that should be called in order to create a new hyperspace lane between two solar systems.
    /// </summary>
    /// <param name="solarSystem1Index"></param>
    /// <param name="solarSystem2Index"></param>
    private void CreateHyperspaceLane(int solarSystem1Index, int solarSystem2Index)
    {
        //Loops through each existing hyperspace lane to determine if the new one would be a duplicate and returns out of the function if so.
        foreach(HyperspaceLane existingHyperspaceLane in hyperspaceLanes)
            if (existingHyperspaceLane.solarSystems.Contains(solarSystems[solarSystem1Index]) && existingHyperspaceLane.solarSystems.Contains(solarSystems[solarSystem2Index]))
                return;

        //Instantiates a new hyperspace lane from the hyperspace lane prefab.
        HyperspaceLane hyperspaceLane = Instantiate(hyperspaceLanePrefab).GetComponent<HyperspaceLane>();
        //Sets the parent of the hyperspace lane.
        hyperspaceLane.transform.SetParent(hyperspaceLanesParent);
        hyperspaceLane.transform.localPosition = Vector3.zero;
        hyperspaceLane.transform.localScale = Vector3.one;
        //Initializes the hyperspace lane with the specified solar systems and appropriate starting and ending colors.
        hyperspaceLane.Initialize(new List<GalaxySolarSystem>() { solarSystems[solarSystem1Index], solarSystems[solarSystem2Index] }, Color.yellow, Color.yellow);
        //Logs in the two solar systems via ID (index) that this hyperspace lane is directly connecting that solar system with another solar system.
        solarSystems[solarSystem1Index].AddHyperspaceLaneIDLog(hyperspaceLanes.Count);
        solarSystems[solarSystem2Index].AddHyperspaceLaneIDLog(hyperspaceLanes.Count);
        //Adds the hyperspace lane to the list of hyperspace lanes.
        hyperspaceLanes.Add(hyperspaceLane);
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
    /// Public static function that returns a random empire color based on the specified empire culture.
    /// </summary>
    /// <param name="culture"></param>
    /// <returns></returns>
    private static Color GetRandomColorBasedOnCulture(NewEmpire.Culture culture)
    {
        switch (culture)
        {
            case NewEmpire.Culture.Red:
                return new Color(UnityEngine.Random.Range(0.25f, 1.0f), 0, 0, 1);
            case NewEmpire.Culture.Green:
                return new Color(0, UnityEngine.Random.Range(0.25f, 1.0f), 0, 1);
            case NewEmpire.Culture.Blue:
                return new Color(0, 0, UnityEngine.Random.Range(0.25f, 1.0f), 1);
            case NewEmpire.Culture.Purple:
                List<Color> purpleColors = new List<Color>();
                purpleColors.Add(new Color(186.0f / 255, 85.0f / 255, 211.0f / 255, 1));         //Medium Orchid
                purpleColors.Add(new Color(147.0f / 255, 112.0f / 255, 219.0f / 255, 1));        //Medium Purple
                purpleColors.Add(new Color(138.0f / 255, 43.0f / 255, 226.0f / 255, 1));         //Blue Violet
                purpleColors.Add(new Color(148.0f / 255, 0.0f / 255, 211.0f / 255, 1));          //Dark Violet
                purpleColors.Add(new Color(153.0f / 255, 50.0f / 255, 204.0f / 255, 1));         //Dark Orchid
                purpleColors.Add(new Color(139.0f / 255, 0.0f / 255, 139.0f / 255, 1));          //Dark Magenta
                purpleColors.Add(new Color(128.0f / 255, 0.0f / 255, 128.0f / 255, 1));          //Purple
                int random = UnityEngine.Random.Range(0, purpleColors.Count);
                return purpleColors[random];
            case NewEmpire.Culture.Gold:
                List<Color> goldColors = new List<Color>();
                goldColors.Add(new Color(238.0f / 255, 232.0f / 255, 170.0f / 255, 1));          //Pale Golden Rod
                goldColors.Add(new Color(240.0f / 255, 230.0f / 255, 140.0f / 255, 1));          //Khaki
                goldColors.Add(new Color(255.0f / 255, 215.0f / 255, 0.0f / 255, 1));            //Gold
                goldColors.Add(new Color(255.0f / 255, 223.0f / 255, 0.0f / 255, 1));            //Golden Yellow
                goldColors.Add(new Color(212.0f / 255, 175.0f / 255, 55.0f / 255, 1));           //Metallic Gold
                goldColors.Add(new Color(207.0f / 255, 181.0f / 255, 59.0f / 255, 1));           //Old Gold
                goldColors.Add(new Color(197.0f / 255, 179.0f / 255, 88.0f / 255, 1));           //Vegas Gold
                int randomIndex = UnityEngine.Random.Range(0, goldColors.Count);
                return goldColors[randomIndex];
            case NewEmpire.Culture.Silver:
                List<Color> silverColors = new List<Color>();
                silverColors.Add(new Color(211.0f / 255, 211.0f / 255, 211.0f / 255, 1));
                silverColors.Add(new Color(192.0f / 255, 192.0f / 255, 192.0f / 255, 1));
                silverColors.Add(new Color(169.0f / 255, 169.0f / 255, 169.0f / 255, 1));
                int silverRandomIndex = UnityEngine.Random.Range(0, silverColors.Count);
                return silverColors[silverRandomIndex];
            default:
                return new Color(1, 1, 1, 1);
        }
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

    /// <summary>
    /// Adds the specified function to the list of functions to be executed once the galaxy has completely finished generating and the execution order number indicates when the function will be executed with the execution numbers starting at 0 and going until the first value where there are no functions with the execution number.
    /// </summary>
    /// <param name="function"></param>
    public static void ExecuteFunctionOnGalaxyGenerationCompletion(Action function, int executionOrderNumber)
    {
        if(galaxyGenerator == null)
        {
            Debug.LogWarning("Cannot execute function on galaxy generation completion, probably because the galaxy has already finished generating.");
            return;
        }

        if (!galaxyGenerator.galaxyGenerationCompletionFunctions.ContainsKey(executionOrderNumber))
            galaxyGenerator.galaxyGenerationCompletionFunctions.Add(executionOrderNumber, new List<Action>());
        galaxyGenerator.galaxyGenerationCompletionFunctions[executionOrderNumber].Add(function);
    }

    /// <summary>
    /// Removes the specified function to the list of functions to be executed once the galaxy has completely finished generating.
    /// </summary>
    /// <param name="function"></param>
    public static void RemoveFunctionFromFunctionsToExecuteOnGalaxyGenerationCompletion(Action function)
    {
        if (galaxyGenerator == null)
            return;

        foreach(int executionOrderNumber in galaxyGenerator.galaxyGenerationCompletionFunctions.Keys)
        {
            if (galaxyGenerator.galaxyGenerationCompletionFunctions[executionOrderNumber].Contains(function))
            {
                galaxyGenerator.galaxyGenerationCompletionFunctions[executionOrderNumber].Remove(function);
                break;
            }
        }
    }

    /// <summary>
    /// Private function that is executed once the galaxy has completely finished generating and the galaxy manager has been initialized and executes all needed functions in the correct order starting with execution number 0 and going until there are no more functions with the current execution number.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        int executionOrderNumber = 0;
        while (true)
        {
            if (!galaxyGenerationCompletionFunctions.ContainsKey(executionOrderNumber))
                break;
            List<Action> galaxyGenerationCompletionFunctionsWithExecutionOrderNumber = galaxyGenerationCompletionFunctions[executionOrderNumber];
            foreach (Action galaxyGenerationCompletionFunctionWithExecutionOrderNumber in galaxyGenerationCompletionFunctionsWithExecutionOrderNumber)
            {
                galaxyGenerationCompletionFunctionWithExecutionOrderNumber();
            }
            executionOrderNumber++;
        }
    }

    /// <summary>
    /// Private void that should be called before the galaxy is generated, after the new game data and save game data has been loaded in, and resets all needed static helper functions.
    /// </summary>
    private void ResetHelperFunctions()
    {
        PlanetNameGenerator.ResetPlanetNamesGenerated(saveGameData);
        StarNameGenerator.ResetStarNamesGenerated(saveGameData);
    }

    /// <summary>
    /// Private method that is called whenever the galaxy generator object is destroyed after the galaxy has finished generating and resets the static galaxy generator instance to null.
    /// </summary>
    private void OnDestroy()
    {
        galaxyGenerator = null;
    }
}

[System.Serializable]
public class NewGalaxyBiome
{
    [SerializeField, Tooltip("Specifies the type of biome.")] private Planet.Biome _biome = Planet.Biome.Unknown;
    [SerializeField, Tooltip("The chance that a planet of this biome will spawn with a ring (Range: 0-1).")] private float _planetaryRingChance = 0.2f;
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can have its rings be.")] private Vector2 planetaryRingSizeRange = new Vector2(0.25f, 0.69f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can be.")] private Vector2 planetarySizeRange = new Vector2(0.2f, 0.3f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) amount of degrees that the planet can rotate on its axis every second.")] private Vector2 planetaryRotationSpeedRange = new Vector2(3, 5);
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
    /// Public property that should be used in order to access a new random amount of degrees for a planet of the biome to rotate on its own axis every second.
    /// </summary>
    public float randomPlanetaryRotationSpeed { get => UnityEngine.Random.Range(planetaryRotationSpeedRange.x, planetaryRotationSpeedRange.y); }

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

class NewPossibleHyperspaceLane : System.IComparable
{
    public float weight;
    public int solarSystem1Index, solarSystem2Index;

    public NewPossibleHyperspaceLane(float weight, int solarSystem1Index, int solarSystem2Index)
    {
        this.weight = weight;
        this.solarSystem1Index = solarSystem1Index;
        this.solarSystem2Index = solarSystem2Index;
    }

    public int CompareTo(object other)
    {
        NewPossibleHyperspaceLane otherLane = other as NewPossibleHyperspaceLane;

        if (weight < otherLane.weight)
            return -1;
        else
            return 1;
    }
}