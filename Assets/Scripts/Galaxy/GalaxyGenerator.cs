using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyGenerator : MonoBehaviour
{
    [Header("Tech")]

    [SerializeField]
    private List<string> techTotems = null;
    [SerializeField]
    private List<Tech> techs = null;

    [Header("Initial Game Settings")]

    [SerializeField]
    private string playerEmpireName;

    [SerializeField]
    private int numberOfPlanets = 60;
    [SerializeField]
    private int numberOfEmpires = 5;
    [SerializeField]
    private int distanceBetweenPlanets = 30;
    [SerializeField]
    private int hyperspaceLaneCheckingRadius = 60;

    [SerializeField]
    private List<Sprite> flagSymbols = null;

    [Header("Camera Settings")]

    [SerializeField]
    private Camera galaxyCamera = null;

    [SerializeField]
    private Canvas galaxyCanvas = null;

    public Material skyboxMaterial;

    [SerializeField]
    private float leftBoundary = -50;
    [SerializeField]
    private float rightBoundary = 500;
    [SerializeField]
    private float topBoundary = 260;
    [SerializeField]
    private float bottomBoundary = 0;

    [Header("Planet Materials")]

    [SerializeField]
    private List<Material> frozenMaterials = null;
    [SerializeField]
    private List<Material> spiritMaterials = null;
    [SerializeField]
    private List<Material> temperateMaterials = null;
    [SerializeField]
    private List<Material> desertMaterials = null;
    [SerializeField]
    private List<Material> swampMaterials = null;
    [SerializeField]
    private List<Material> hellMaterials = null;

    [Space]

    [SerializeField]
    private List<Material> empireMaterials = null;

    [Header("Prefabs")]

    [SerializeField]
    private GameObject planetPrefab = null;
    [SerializeField]
    private GameObject shipPrefab = null;
    [SerializeField]
    private GameObject galaxyConfirmationPopupPrefab = null;
    [SerializeField]
    private GameObject galaxyInputFieldConfirmationPopupPrefab = null;
    [SerializeField]
    private GameObject galaxyDropdownConfirmationPopupPrefab = null;
    [SerializeField]
    private GameObject tooltipPrefab = null;
    [SerializeField]
    private GameObject backArrowPrefab = null;
    [SerializeField]
    private GameObject pillViewPrefab = null;

    [Header("Parents")]

    [SerializeField]
    private Transform hyperspacesLanesParent = null;
    [SerializeField]
    private Transform planetParent = null;
    [SerializeField]
    private Transform shipParent = null;
    [SerializeField]
    private Transform galaxyConfirmationPopupParent = null;

    [Header("Manager Objects")]

    [SerializeField]
    private HyperspaceLanesManager hyperspaceLanesManager = null;
    [SerializeField]
    private PlanetManagementMenu planetManagementMenu = null;
    [SerializeField]
    private ArmyManagementMenu armyManagementMenu = null;

    //Non-inspector variables.

    private string[] planetNames;

    private List<GameObject> planets;

    //Indicates whether the galaxy is finished generating or not.
    private static bool galaxyFinishedGenerating;
    public static bool GalaxyFinishedGenerating
    {
        get
        {
            return galaxyFinishedGenerating;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Loads in the settings from the main menu scene if they exist.
        if (NewGameMenu.initialized)
            LoadNewGameSettings();

        //Generates the planets of the galaxy.
        GeneratePlanets();

        //Generates the hyperspace lanes of the galaxy.
        GenerateHyperspaceLanes();

        //Generates the empires of the galaxy.
        GenerateEmpires();

        //Generates the stats of each planet in the galaxy.
        GeneratePlanetStats();
        List<GalaxyPlanet> planetScripts = new List<GalaxyPlanet>();
        foreach (GameObject planet in planets)
        {
            planetScripts.Add(planet.GetComponent<GalaxyPlanet>());
        }
        GalaxyManager.Initialize(planetScripts, flagSymbols, galaxyCamera, galaxyCanvas, galaxyConfirmationPopupParent);

        //Generates the tech of the game.
        GenerateTech();

        //Creates the necessary warning notifications at the start of the game.
        GalaxyManager.galaxyManager.WarningRightSideNotificationsUpdate();
        
        //Clean up section :)
        GeneralHelperMethods.ClearTextFileCache();

        //Indicates that the galaxy has finished generating.
        galaxyFinishedGenerating = true;

        //Updates the resource bar to accurately reflect the player's empire.
        ResourceBar.UpdateAllEmpireDependantComponents();

        //Informs each empire that the galaxy has finished generating.
        foreach(Empire empire in Empire.empires)
        {
            empire.OnGalaxyGenerationCompletion();
        }
    }

    private void Awake()
    {
        //Prefabs.
        GalaxyInputFieldConfirmationPopup.galaxyInputFieldConfirmationPopupPrefab = galaxyInputFieldConfirmationPopupPrefab;
        GalaxyDropdownConfirmationPopup.galaxyDropdownConfirmationPopupPrefab = galaxyDropdownConfirmationPopupPrefab;
        GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab = galaxyConfirmationPopupPrefab;
        GalaxyTooltip.tooltipPrefab = tooltipPrefab;
        GalaxyMenuBehaviour.backArrowPrefab = backArrowPrefab;
        PillViewsManager.pillViewPrefab = pillViewPrefab;

        //Manager objects.
        HyperspaceLanesManager.hyperspaceLanesManager = hyperspaceLanesManager;
        PlanetManagementMenu.planetManagementMenu = planetManagementMenu;
        ArmyManagementMenu.InitializeFromGalaxyGenerator(armyManagementMenu);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadNewGameSettings()
    {
        numberOfPlanets = NewGameMenu.numberOfPlanets;
        numberOfEmpires = NewGameMenu.numberOfEmpires;
        playerEmpireName = NewGameMenu.empireName;
    }


    //Generates each empire's available tech.
    private void GenerateTech()
    {
        foreach(Tech tech in techs)
        {
            Tech.entireTechList.Add(tech);
        }

        for(int x = 0; x < Empire.empires.Count; x++)
        {
            //Adds all of the totems to the empire.
            foreach(string techTotemName in techTotems)
            {
                TechTotem totem = new TechTotem();
                totem.name = techTotemName;

                Empire.empires[x].techManager.techTotems.Add(totem);
            }

            //Adds every tech to its appropriate totem.
            for(int y = 0; y < Tech.entireTechList.Count; y++)
            {
                foreach(TechTotem totem in Empire.empires[x].techManager.techTotems)
                {
                    if (Tech.entireTechList[y].totemName.Equals(totem.name))
                    {
                        totem.techsAvailable.Add(y);
                        break;
                    }
                }
            }

            //Randomizes what tech is displayed on each totem.
            foreach(TechTotem totem in Empire.empires[x].techManager.techTotems)
            {
                totem.RandomizeTechDisplayed();
            }
        }
    }

    //Generates each planet's stats.
    private void GeneratePlanetStats()
    {
        foreach(Empire empire in Empire.empires)
        {
            for(int x = 0; x < empire.planetsOwned.Count; x++)
            {
                if(x == 0)
                {
                    planets[empire.planetsOwned[x]].GetComponent<GalaxyPlanet>().GenerateCities(true);
                }
                else
                {
                    planets[empire.planetsOwned[x]].GetComponent<GalaxyPlanet>().GenerateCities(false);
                }
            }
        }
    }

    private void GenerateEmpires()
    {
        Empire.empires = new List<Empire>();

        for (int empireID = 0; empireID < numberOfEmpires; empireID++)
        {
            Empire.empires.Add(new Empire(empireID));

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's culture.

            Empire.empires[empireID].empireCulture = new Empire.Culture();
            Empire.Culture empireCulture = new Empire.Culture();
            if (empireID == GalaxyManager.PlayerID && FlagCreationMenu.initialized)
                empireCulture = NewGameMenu.empireCulture;
            else
            {
                while (true)
                {
                    int random = Random.Range(0, 5);
                    switch (random)
                    {
                        case 0:
                            empireCulture = Empire.Culture.Red;
                            break;
                        case 1:
                            empireCulture = Empire.Culture.Green;
                            break;
                        case 2:
                            empireCulture = Empire.Culture.Blue;
                            break;
                        case 3:
                            empireCulture = Empire.Culture.Purple;
                            break;
                        case 4:
                            empireCulture = Empire.Culture.Gold;
                            break;
                    }

                    if (empireID == 0)
                        break;
                    bool goodCulture = true;
                    for (int y = 0; y < empireID; y++)
                    {
                        if (Empire.empires[y].empireCulture == empireCulture)
                            goodCulture = false;
                    }
                    if (goodCulture)
                        break;
                }
            }
            Empire.empires[empireID].empireCulture = empireCulture;

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's flag.

            Empire.empires[empireID].EmpireFlag = new Flag();
            if (empireID == GalaxyManager.PlayerID && FlagCreationMenu.initialized)
            {
                Empire.empires[empireID].EmpireFlag.symbolSelected = FlagCreationMenu.symbolSelected;
                Empire.empires[empireID].EmpireFlag.backgroundColor = FlagCreationMenu.backgroundColor;
                Empire.empires[empireID].EmpireFlag.symbolColor = FlagCreationMenu.symbolColor;
            }
            else
            {
                //Generates the symbol color of each empire's flag based on the empire's culture.
                Empire.empires[empireID].EmpireFlag.symbolColor = GetRandomColorBasedOnCulture(Empire.empires[empireID].empireCulture);

                //Generates the background color of each empire's flag.
                if (Empire.empires[empireID].EmpireFlag.symbolColor.x + Empire.empires[empireID].EmpireFlag.symbolColor.y + Empire.empires[empireID].EmpireFlag.symbolColor.z < 0.6f)
                    Empire.empires[empireID].EmpireFlag.backgroundColor = new Vector3(1.0f, 1.0f, 1.0f);
                else
                    Empire.empires[empireID].EmpireFlag.backgroundColor = new Vector3(0, 0, 0);

                //Generates the symbol on each empire's flag (ensures there will be no duplicates).
                int random = 0;
                while (true)
                {
                    random = Random.Range(0, flagSymbols.Count);

                    if (empireID == 0)
                        break;

                    bool goodSymbol = true;
                    for(int y = 0; y < empireID; y++)
                    {
                        if (Empire.empires[y].EmpireFlag.symbolSelected == random)
                            goodSymbol = false;
                    }
                    if (goodSymbol)
                        break;
                }
                Empire.empires[empireID].EmpireFlag.symbolSelected = random;
            }

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's color.

            if(empireID == GalaxyManager.PlayerID)
            {
                Vector3 randomColor = GetRandomColorBasedOnCulture(Empire.empires[empireID].empireCulture);
                Empire.empires[empireID].EmpireColor = new Color(randomColor.x, randomColor.y, randomColor.z, 1.0f);
            }
            else
            {
                Empire.empires[empireID].EmpireColor =  new Color(Empire.empires[empireID].EmpireFlag.symbolColor.x, Empire.empires[empireID].EmpireFlag.symbolColor.y, Empire.empires[empireID].EmpireFlag.symbolColor.z, 1.0f);
            }

            //----------------------------------------------------------------------------------------------------
            //Sets the empire's material color.
            int materialIndex = (int)Empire.empires[empireID].empireCulture;
            empireMaterials[materialIndex].color = Empire.empires[empireID].EmpireColor;
            GalaxyManager.empireMaterials[materialIndex] = empireMaterials[materialIndex];

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's planets.

            Empire.empires[empireID].planetsOwned = new List<int>();

            GameObject sourcePlanet = planets[0];
            for (int y = 0; y < planets.Count; y++)
            {
                sourcePlanet = planets[y];
                if (sourcePlanet.GetComponent<GalaxyPlanet>().OwnerID == -1)
                {
                    Empire.empires[empireID].planetsOwned.Add(y);
                    GalaxyPlanet planet = planets[y].GetComponent<GalaxyPlanet>();
                    planet.OwnerID = empireID;
                    planet.Culture = Empire.empires[empireID].empireCulture;
                    planet.IsCapital = true;
                    planet.GenerateShip(shipParent, shipPrefab);
                    sourcePlanet = planets[y];
                    break;
                }
            }

            while(Empire.empires[empireID].planetsOwned.Count < planets.Count / numberOfEmpires)
            {
                int indexToAdd = -1;

                for(int y = 0; y < planets.Count; y++)
                {
                    if(planets[y].GetComponent<GalaxyPlanet>().OwnerID == -1)
                    {
                        if(indexToAdd == -1)
                        {
                            indexToAdd = y;
                        }
                        else if (Vector3.Distance(planets[y].transform.localPosition, sourcePlanet.transform.localPosition) < Vector3.Distance(planets[indexToAdd].transform.localPosition, sourcePlanet.transform.localPosition))
                        {
                            indexToAdd = y;
                        }
                    }
                }

                Empire.empires[empireID].planetsOwned.Add(indexToAdd);
                GalaxyPlanet planet = planets[indexToAdd].GetComponent<GalaxyPlanet>();
                planet.OwnerID = empireID;
                planet.Culture = Empire.empires[empireID].empireCulture;
                planet.IsCapital = false;
                planet.GenerateShip(shipParent, shipPrefab);
            }

            //----------------------------------------------------------------------------------------------------
            //Generate the empire's name.

            if (empireID == GalaxyManager.PlayerID && FlagCreationMenu.initialized && !playerEmpireName.Equals(""))
                Empire.empires[empireID].EmpireName = playerEmpireName;
            else
            {
                string empireName = "";
                while (true)
                {
                    empireName = EmpireNameGenerator.GenerateEmpireName(planets[Empire.empires[empireID].planetsOwned[0]].GetComponent<GalaxyPlanet>().Name);

                    if (empireID == 0)
                        break;

                    bool goodName = true;
                    for (int y = 0; y < empireID; y++)
                    {
                        if (Empire.empires[y].EmpireName.Equals(empireName))
                            goodName = false;
                    }

                    if (goodName)
                        break;
                }
                Empire.empires[empireID].EmpireName = empireName;
            }
        }

        //----------------------------------------------------------------------------------------------------
        //Deals with the leftover planets.

        int iteration = 0;
        while (PlanetsAttachedToEmpires() < planets.Count)
        {
            for (int y = 0; y < planets.Count; y++)
            {
                if (planets[y].GetComponent<GalaxyPlanet>().OwnerID == -1)
                {
                    Empire.empires[iteration].planetsOwned.Add(y);
                    GalaxyPlanet planet = planets[y].GetComponent<GalaxyPlanet>();
                    planet.OwnerID = iteration;
                    planet.Culture = Empire.empires[iteration].empireCulture;
                    planet.IsCapital = false;
                    planet.GenerateShip(shipParent, shipPrefab);
                    break;
                }
            }

            iteration++;
        }
    }

    public Vector3 GetRandomColorBasedOnCulture(Empire.Culture culture)
    {
        switch (culture)
        {
            case Empire.Culture.Red:
                return new Vector3(Random.Range(0.25f, 1.0f), 0, 0);
            case Empire.Culture.Green:
                return new Vector3(0, Random.Range(0.25f, 1.0f), 0);
            case Empire.Culture.Blue:
                return new Vector3(0, 0, Random.Range(0.25f, 1.0f));
            case Empire.Culture.Purple:
                List<Vector3> purpleColors = new List<Vector3>();
                purpleColors.Add(new Vector3(186.0f / 255, 85.0f / 255, 211.0f / 255));         //Medium Orchid
                purpleColors.Add(new Vector3(147.0f / 255, 112.0f / 255, 219.0f / 255));        //Medium Purple
                purpleColors.Add(new Vector3(138.0f / 255, 43.0f / 255, 226.0f / 255));         //Blue Violet
                purpleColors.Add(new Vector3(148.0f / 255, 0.0f / 255, 211.0f / 255));          //Dark Violet
                purpleColors.Add(new Vector3(153.0f / 255, 50.0f / 255, 204.0f / 255));         //Dark Orchid
                purpleColors.Add(new Vector3(139.0f / 255, 0.0f / 255, 139.0f / 255));          //Dark Magenta
                purpleColors.Add(new Vector3(128.0f / 255, 0.0f / 255, 128.0f / 255));          //Purple
                int random = Random.Range(0, purpleColors.Count);
                return purpleColors[random];
            case Empire.Culture.Gold:
                List<Vector3> goldColors = new List<Vector3>();
                goldColors.Add(new Vector3(238.0f / 255, 232.0f / 255, 170.0f / 255));          //Pale Golden Rod
                goldColors.Add(new Vector3(240.0f / 255, 230.0f / 255, 140.0f / 255));          //Khaki
                goldColors.Add(new Vector3(255.0f / 255, 215.0f / 255, 0.0f / 255));            //Gold
                goldColors.Add(new Vector3(255.0f / 255, 223.0f / 255, 0.0f / 255));            //Golden Yellow
                goldColors.Add(new Vector3(212.0f / 255, 175.0f / 255, 55.0f / 255));           //Metallic Gold
                goldColors.Add(new Vector3(207.0f / 255, 181.0f / 255, 59.0f / 255));           //Old Gold
                goldColors.Add(new Vector3(197.0f / 255, 179.0f / 255, 88.0f / 255));           //Vegas Gold
                int randomIndex = Random.Range(0, goldColors.Count);
                return goldColors[randomIndex];
        }

        return new Vector3(1.0f, 1.0f, 1.0f);
    }

    private int PlanetsAttachedToEmpires()
    {
        int num = 0;

        foreach(Empire empire in Empire.empires)
        {
            num += empire.planetsOwned.Count;
        }

        return num;
    }

    private void GenerateHyperspaceLanes()
    {
        //----------------------------------------------------------------------------------------------------
        //FIRST, PERFORM SOME SET UP

        //Set up lane manager
        HyperspaceLanesManager laneManager = hyperspaceLanesManager.GetComponent<HyperspaceLanesManager>();
        laneManager.hyperspaceLanes = new List<GameObject>();

        //Get planet positions
        Vector3[] planetPositions = new Vector3[planets.Count];
        for (int x = 0; x < planetPositions.Length; x++)
            planetPositions[x] = planets[x].transform.localPosition;

        //----------------------------------------------------------------------------------------------------
        //THEN, PERFORM KRUSKAL'S MINIMUM SPANNING TREE TO ENSURE CONNECTEDNESS OF ENTIRE GALAXY!!!!!!

        //Get all possible lanes and their weights, i.e. distances
        PossibleHyperspaceLane[] possibleLanes = new PossibleHyperspaceLane[planets.Count * planets.Count];
        int nextWeightIndex = 0;
        for(int x = 0; x < planets.Count; x++)
        {
            for (int y = 0; y < planets.Count; y++)
            {
                if(x == y) //Do not allow self-loops
                    possibleLanes[nextWeightIndex++] = new PossibleHyperspaceLane(
                    Mathf.Infinity, x, y);
                else
                    possibleLanes[nextWeightIndex++] = new PossibleHyperspaceLane(
                    Vector3.Distance(planetPositions[x], planetPositions[y]), x, y);
            }
        }

        //Sort possible lanes by weight, i.e. distance
        //This ensures that we try to add the shortest hyperspace lanes first
        System.Array.Sort(possibleLanes);

        //Initially, each planet is it's own tree
        List<HashSet<int>> indexTrees = new List<HashSet<int>>();
        for(int x = 0; x < planets.Count; x++)
        {
            HashSet<int> newTree = new HashSet<int>();
            newTree.Add(x);
            indexTrees.Add(newTree);
        }

        //Add hyperspace lanes until all planets are in a single connected tree
        //But only add a lane if it serves to connect two trees
        int possibleLaneIndex = 0;
        while(indexTrees.Count > 1)
        {
            PossibleHyperspaceLane possibleLane = possibleLanes[possibleLaneIndex++];

            //Determine which tree each planet belongs to
            int planet1Tree = -1;
            int planet2Tree = -1;
            for(int x = 0; x < indexTrees.Count; x++)
            {
                if (indexTrees[x].Contains(possibleLane.planet1Index))
                    planet1Tree = x;

                if (indexTrees[x].Contains(possibleLane.planet2Index))
                    planet2Tree = x;
            }

            //Disjoint trees, so include hyperspace line
            if(planet1Tree != planet2Tree)
            {
                //Merge trees
                indexTrees[planet1Tree].UnionWith(indexTrees[planet2Tree]); //1 becomes union of 1 and 2
                indexTrees.RemoveAt(planet2Tree); //2 gets discarded

                //Include hyperspace lane
                laneManager.AddHyperspaceLane(
                planets[possibleLane.planet1Index], planets[possibleLane.planet2Index], hyperspacesLanesParent);
            }
        }

        //----------------------------------------------------------------------------------------------------
        //FINALLY, ENSURE ALL LANES ARE ADDED WITHIN CERTAIN DISTANCE THRESHOLD (CIARAN'S ALGORITHM)

        //Add hyperspace lanes for planets within certain distance of each other, regardless of connectedness
        for (int x = 0; x < planets.Count; x++)
        {
            for(int y = 0; y < planets.Count; y++)
            {
                if (x != y && Vector3.Distance(planetPositions[x], planetPositions[y]) <= hyperspaceLaneCheckingRadius)
                {
                    laneManager.AddHyperspaceLane(planets[x], planets[y], hyperspacesLanesParent);
                }
            }
        }
    }

    private void GeneratePlanets()
    {
        ReadInPlanetNames();

        planets = new List<GameObject>();
        for(int x = 0; x < numberOfPlanets; x++)
        {
            GameObject newPlanet = Instantiate(planetPrefab);
            newPlanet.transform.parent = planetParent;

            //Assigns a name to each planet.
            string newPlanetName = "";
            for(int attempt = 1; attempt <= 1000; attempt++)
            {
                newPlanetName = GeneratePlanetName();
                if (PlanetNameRedundant(newPlanetName) == false)
                    break;
            }
            newPlanet.GetComponent<GalaxyPlanet>().InitializePlanet(newPlanetName);

            //Assigns a biome to each planet;
            newPlanet.GetComponent<GalaxyPlanet>().Biome = GenerateBiome();
            newPlanet.GetComponent<MeshRenderer>().sharedMaterial = GetPlanetMaterial(newPlanet.GetComponent<GalaxyPlanet>().Biome);

            //Assigns a radius to each planet
            newPlanet.transform.localScale = Vector3.one * GeneratePlanetScale();
            newPlanet.transform.localPosition = GeneratePlanetLocation(newPlanet.transform.localScale.x);

            //Assigns a planet id to each planet.
            newPlanet.GetComponent<GalaxyPlanet>().PlanetID = x;

            //Adds the planet to the list on planets.
            planets.Add(newPlanet);
        }
    }

    private Vector3 GeneratePlanetLocation(float radius)
    {
        Vector3 randomPosition = Vector3.zero;
        bool goodPosition;

        for(int attempt = 1; attempt <= 1000; attempt++)
        {
            goodPosition = true;
            randomPosition = new Vector3(Random.Range(leftBoundary, rightBoundary), 0, Random.Range(bottomBoundary, topBoundary));
            for(int x = 0; x < planets.Count; x++)
            {
                /*if(Mathf.Abs(randomPosition.x - planets[x].transform.localPosition.x) <= 30 || Mathf.Abs(randomPosition.z - planets[x].transform.localPosition.z) <= 30)
                {
                    continue;
                }*/
                if(Vector3.Distance(randomPosition, planets[x].transform.localPosition) <= distanceBetweenPlanets)
                {
                    goodPosition = false;
                    break;
                }
            }
            if (goodPosition == true)
                break;
        }

        return randomPosition;
    }

    private int GeneratePlanetScale()
    {
        return Random.Range(8, 16);
    }

    //Gets the material for the planet depending on the planet's randomly generated biome.
    private Material GetPlanetMaterial(Planet.Biome biome)
    {
        switch (biome)
        {
            case Planet.Biome.Frozen:
                return frozenMaterials[Random.Range(0, frozenMaterials.Count)];
            case Planet.Biome.Temperate:
                return temperateMaterials[Random.Range(0, temperateMaterials.Count)];
            case Planet.Biome.Desert:
                return desertMaterials[Random.Range(0, desertMaterials.Count)];
            case Planet.Biome.Swamp:
                return swampMaterials[Random.Range(0, swampMaterials.Count)];
            case Planet.Biome.Hell:
                return hellMaterials[Random.Range(0, hellMaterials.Count)];
            case Planet.Biome.Spirit:
                return spiritMaterials[Random.Range(0, spiritMaterials.Count)];

            default:
                return frozenMaterials[0];
        }
    }

    //Method the randomly generates a biome for a planet.
    private Planet.Biome GenerateBiome()
    {
        int random = Random.Range(0, 6);

        switch (random)
        {
            case 0:
                return Planet.Biome.Frozen;
            case 1:
                return Planet.Biome.Temperate;
            case 2:
                return Planet.Biome.Desert;
            case 3:
                return Planet.Biome.Swamp;
            case 4:
                return Planet.Biome.Hell;
            case 5:
                return Planet.Biome.Spirit;

            default:
                return Planet.Biome.Unknown;
        }
    }

    //Method that detects if the planet name is already being used by another planet.
    private bool PlanetNameRedundant(string planetName)
    {
        for(int x = 0; x < planets.Count; x++)
        {
            if (planetName.Equals(planets[x].GetComponent<GalaxyPlanet>().Name))
                return true;
        }

        return false;
    }

    //Reads in the text file that stores planet names.
    private void ReadInPlanetNames()
    {
        planetNames = GeneralHelperMethods.GetLinesFromFile("Location Names/Planet Names");
    }


    //Returns a random planet name
    private string GeneratePlanetName()
    {
        string planetName = "";

        if (Random.Range(0, 6) != 0) //Normal random name
        {
            //Pick a random name
            planetName = planetNames[Random.Range(0, planetNames.Length)];
        }
        else if (Random.Range(0, 2) == 0) //Greek letter + astrological/zodiac/birth sign name
        {
            string[] greekLetters = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega"};

            string[] zodiacSigns = new string[] { "Carinae", "Tauri", "Pegasi", "Centauri", "Scuti", "Orionis", "Scorpius",
            "Geminorum"};

            planetName = greekLetters[Random.Range(0, greekLetters.Length)] + " "
                + zodiacSigns[Random.Range(0, zodiacSigns.Length)];
        }
        else if (Random.Range(0, 2) == 0) //Some prefix + major or minor name
        {
            string[] prefixes = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega", "Ursa", "Virgo", "Canis", "Pisces", "Saega", "Polis"};

            if (Random.Range(0, 2) == 0)
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Major";
            else
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Minor";
        }
        else //Some guy's name that wanted to shove his dick into history + some edgy sounding fate
        {
            string[] dickShovers = new string[] { "Troy's ", "Turner's ", "Coronado's ", "Septim's ", "Pelagius' ",
            "Haile's ", "Myra's ", "Midas' ", "Calypso's "};

            string[] edgyFates = new string[] { "Fall", "Demise", "Oblivion", "End", "Moon", "Shame", "Hell",
            "Garden", "Domain", "Eyrie", "Madness", "Lost Plane", "Last"};

            planetName = dickShovers[Random.Range(0, dickShovers.Length)] + " "
                + edgyFates[Random.Range(0, edgyFates.Length)];
        }

        //Add roman numeral onto end for variation
        if (Random.Range(0, 5) == 0)
        {
            string[] numerals = new string[] { "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            planetName += " " + numerals[Random.Range(0, numerals.Length)];
        }

        return planetName;
    }
}

class PossibleHyperspaceLane : System.IComparable
{
    public float weight;
    public int planet1Index, planet2Index;

    public PossibleHyperspaceLane(float weight, int planet1Index, int planet2Index)
    {
        this.weight = weight;
        this.planet1Index = planet1Index;
        this.planet2Index = planet2Index;
    }

    public int CompareTo(object other)
    {
        PossibleHyperspaceLane otherLane = other as PossibleHyperspaceLane;

        if (weight < otherLane.weight)
            return -1;
        else
            return 1;
    }
}