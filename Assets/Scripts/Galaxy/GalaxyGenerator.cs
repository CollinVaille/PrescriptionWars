using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyGenerator : MonoBehaviour
{
    string[] planetNames;

    public int numberOfPlanets;
    public int numberOfEmpires;
    public int distanceBetweenPlanets;
    public int hyperspaceLaneCheckingRadius;

    public float leftBoundary;
    public float rightBoundary;
    public float topBoundary;
    public float bottomBoundary;

    public string playerEmpireName;

    public GameObject planetPrefab;
    public Transform planetDaddy;
    public Transform hyperspaceLanesDaddy;

    List<GameObject> planets;
    public GameObject hyperspaceLanesManager;
    public GameObject planetManagementMenu;

    public List<Sprite> flagSymbols;

    public List<Material> frozenMaterials;
    public List<Material> spiritMaterials;
    public List<Material> temperateMaterials;
    public List<Material> desertMaterials;
    public List<Material> swampMaterials;
    public List<Material> hellMaterials;
    public List<Material> forestMaterials;

    // Start is called before the first frame update
    void Start()
    {
        if (NewGameMenu.initialized)
        {
            LoadNewGameSettings();
        }
        GeneratePlanets();
        GenerateHyperspaceLanes();
        GenerateEmpires();
        GeneratePlanetStats();
        GalaxyManager.Initialize(planets, flagSymbols, planetManagementMenu);
        //Physics.CheckSphere()
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


    //Generates each planet's stats.
    private void GeneratePlanetStats()
    {
        foreach(Empire empire in Empire.empires)
        {
            for(int x = 0; x < empire.planetsOwned.Count; x++)
            {
                if(x == 0)
                {
                    planets[empire.planetsOwned[x]].GetComponent<PlanetIcon>().GenerateCities(true);
                }
                else
                {
                    planets[empire.planetsOwned[x]].GetComponent<PlanetIcon>().GenerateCities(false);
                }
            }
        }
    }

    private void GenerateEmpires()
    {
        Empire.empires = new List<Empire>();

        for (int x = 0; x < numberOfEmpires; x++)
        {
            Empire.empires.Add(new Empire());

            //----------------------------------------------------------------------------------------------------
            //Determines if the empire is being controlled by the player.

            if (x == GalaxyManager.playerID)
                Empire.empires[x].playerEmpire = true;
            else
                Empire.empires[x].playerEmpire = false;

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's culture.

            Empire.empires[x].empireCulture = new Empire.Culture();
            Empire.Culture empireCulture = new Empire.Culture();
            if (x == GalaxyManager.playerID && FlagCreationMenu.initialized)
                empireCulture = NewGameMenu.empireCulture;
            else
            {
                while (true)
                {
                    int random = Random.Range(0, 3);
                    if (random == 0)
                        empireCulture = Empire.Culture.Red;
                    else if (random == 1)
                        empireCulture = Empire.Culture.Green;
                    else if (random == 2)
                        empireCulture = Empire.Culture.Blue;

                    if (x == 0)
                        break;
                    bool goodCulture = true;
                    for (int y = 0; y < x; y++)
                    {
                        if (Empire.empires[y].empireCulture == empireCulture)
                            goodCulture = false;
                    }
                    if (goodCulture)
                        break;
                }
            }
            Empire.empires[x].empireCulture = empireCulture;

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's flag.

            Empire.empires[x].empireFlag = new Flag();
            if (x == GalaxyManager.playerID && FlagCreationMenu.initialized)
            {
                Empire.empires[x].empireFlag.symbolSelected = FlagCreationMenu.symbolSelected;
                Empire.empires[x].empireFlag.backgroundColor = FlagCreationMenu.backgroundColor;
                Empire.empires[x].empireFlag.symbolColor = FlagCreationMenu.symbolColor;
            }
            else
            {
                //Generates the symbol color of each empire's flag based on the empire's culture.
                if (Empire.empires[x].empireCulture == Empire.Culture.Red)
                {
                    Empire.empires[x].empireFlag.symbolColor = new Vector3(Random.Range(0.25f, 1.0f), 0, 0);
                }
                else if (Empire.empires[x].empireCulture == Empire.Culture.Green)
                {
                    Empire.empires[x].empireFlag.symbolColor = new Vector3(0, Random.Range(0.25f, 1.0f), 0);
                }
                else if (Empire.empires[x].empireCulture == Empire.Culture.Blue)
                {
                    Empire.empires[x].empireFlag.symbolColor = new Vector3(0, 0, Random.Range(0.25f, 1.0f));
                }

                //Generates the background color of each empire's flag.
                if (Empire.empires[x].empireFlag.symbolColor.x + Empire.empires[x].empireFlag.symbolColor.y + Empire.empires[x].empireFlag.symbolColor.z < 0.6f)
                    Empire.empires[x].empireFlag.backgroundColor = new Vector3(1.0f, 1.0f, 1.0f);
                else
                    Empire.empires[x].empireFlag.backgroundColor = new Vector3(0, 0, 0);

                //Generates the symbol on each empire's flag (ensures there will be no duplicates).
                int random = 0;
                while (true)
                {
                    random = Random.Range(0, flagSymbols.Count);

                    if (x == 0)
                        break;

                    bool goodSymbol = true;
                    for(int y = 0; y < x; y++)
                    {
                        if (Empire.empires[y].empireFlag.symbolSelected == random)
                            goodSymbol = false;
                    }
                    if (goodSymbol)
                        break;
                }
                Empire.empires[x].empireFlag.symbolSelected = random;
            }

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's color.

            if(x == GalaxyManager.playerID)
            {
                if(Empire.empires[x].empireCulture == Empire.Culture.Red)
                {
                    Empire.empires[x].empireColor = new Color(Random.Range(0.25f, 1.0f), 0, 0, 1.0f);
                }
                else if(Empire.empires[x].empireCulture == Empire.Culture.Green)
                {
                    Empire.empires[x].empireColor = new Color(0, Random.Range(0.25f, 1.0f), 0, 1.0f);
                }
                else if (Empire.empires[x].empireCulture == Empire.Culture.Blue)
                {
                    Empire.empires[x].empireColor = new Color(0, 0, Random.Range(0.25f, 1.0f), 1.0f);
                }
            }
            else
            {
                Empire.empires[x].empireColor =  new Color(Empire.empires[x].empireFlag.symbolColor.x, Empire.empires[x].empireFlag.symbolColor.y, Empire.empires[x].empireFlag.symbolColor.z, 1.0f);
            }

            //----------------------------------------------------------------------------------------------------
            //Generates the empire's planets.

            Empire.empires[x].planetsOwned = new List<int>();

            GameObject sourcePlanet = planets[0];
            for (int y = 0; y < planets.Count; y++)
            {
                sourcePlanet = planets[y];
                if (sourcePlanet.GetComponent<PlanetIcon>().ownerID == -1)
                {
                    Empire.empires[x].planetsOwned.Add(y);
                    planets[y].GetComponent<PlanetIcon>().SetPlanetOwner(x);
                    planets[y].GetComponent<PlanetIcon>().culture = Empire.empires[x].empireCulture;
                    planets[y].GetComponent<PlanetIcon>().isCapital = true;
                    sourcePlanet = planets[y];
                    break;
                }
            }

            while(Empire.empires[x].planetsOwned.Count < planets.Count / numberOfEmpires)
            {
                int indexToAdd = -1;

                for(int y = 0; y < planets.Count; y++)
                {
                    if(planets[y].GetComponent<PlanetIcon>().ownerID == -1)
                    {
                        if(indexToAdd == -1)
                        {
                            indexToAdd = y;
                        }
                        else
                        {
                            if (Vector3.Distance(planets[y].transform.localPosition, sourcePlanet.transform.localPosition) < Vector3.Distance(planets[indexToAdd].transform.localPosition, sourcePlanet.transform.localPosition))
                                indexToAdd = y;
                        }
                    }
                }

                Empire.empires[x].planetsOwned.Add(indexToAdd);
                planets[indexToAdd].GetComponent<PlanetIcon>().SetPlanetOwner(x);
                planets[indexToAdd].GetComponent<PlanetIcon>().culture = Empire.empires[x].empireCulture;
                planets[indexToAdd].GetComponent<PlanetIcon>().isCapital = false;
            }

            //----------------------------------------------------------------------------------------------------
            //Generate the empire's name.

            if (x == GalaxyManager.playerID && FlagCreationMenu.initialized && !playerEmpireName.Equals(""))
                Empire.empires[x].empireName = playerEmpireName;
            else
            {
                string empireName = "";
                while (true)
                {
                    empireName = EmpireNameGenerator.GenerateEmpireName(planets[Empire.empires[x].planetsOwned[0]].GetComponent<PlanetIcon>().nameLabel.text);

                    if (x == 0)
                        break;

                    bool goodName = true;
                    for (int y = 0; y < x; y++)
                    {
                        if (Empire.empires[y].empireName.Equals(empireName))
                            goodName = false;
                    }

                    if (goodName)
                        break;
                }
                Empire.empires[x].empireName = empireName;
            }
        }

        //----------------------------------------------------------------------------------------------------
        //Deals with the leftover planets.

        int iteration = 0;
        while (PlanetsAttachedToEmpires() < planets.Count)
        {
            for (int y = 0; y < planets.Count; y++)
            {
                if (planets[y].GetComponent<PlanetIcon>().ownerID == -1)
                {
                    Empire.empires[iteration].planetsOwned.Add(y);
                    planets[y].GetComponent<PlanetIcon>().SetPlanetOwner(iteration);
                    planets[y].GetComponent<PlanetIcon>().culture = Empire.empires[iteration].empireCulture;
                    planets[y].GetComponent<PlanetIcon>().isCapital = false;
                    break;
                }
            }

            iteration++;
        }
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
                planets[possibleLane.planet1Index], planets[possibleLane.planet2Index], hyperspaceLanesDaddy);
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
                    laneManager.AddHyperspaceLane(planets[x], planets[y], hyperspaceLanesDaddy);
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
            newPlanet.transform.parent = planetDaddy;

            //Assigns a name to each planet.
            string newPlanetName = "";
            for(int attempt = 1; attempt <= 1000; attempt++)
            {
                newPlanetName = GeneratePlanetName();
                if (PlanetNameRedundant(newPlanetName) == false)
                    break;
            }
            newPlanet.GetComponent<PlanetIcon>().InitializePlanet(newPlanetName);

            //Assigns a biome to each planet;
            newPlanet.GetComponent<PlanetIcon>().biome = GenerateBiome();
            newPlanet.GetComponent<MeshRenderer>().material = GetPlanetMaterial(newPlanet.GetComponent<PlanetIcon>().biome);

            //Assigns a radius to each planet
            newPlanet.transform.localScale = Vector3.one * GeneratePlanetScale();
            newPlanet.transform.localPosition = GeneratePlanetLocation(newPlanet.transform.localScale.x);

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
        if (biome == Planet.Biome.Frozen)
        {
            return frozenMaterials[Random.Range(0, frozenMaterials.Count)];
        }
        else if (biome == Planet.Biome.Temperate)
        {
            return temperateMaterials[Random.Range(0, temperateMaterials.Count)];
        }
        else if (biome == Planet.Biome.Desert)
        {
            return desertMaterials[Random.Range(0, desertMaterials.Count)];
        }
        else if (biome == Planet.Biome.Swamp)
        {
            return swampMaterials[Random.Range(0, swampMaterials.Count)];
        }
        else if (biome == Planet.Biome.Hell)
        {
            return hellMaterials[Random.Range(0, hellMaterials.Count)];
        }
        else if (biome == Planet.Biome.Forest)
        {
            return forestMaterials[Random.Range(0, forestMaterials.Count)];
        }
        else if (biome == Planet.Biome.Spirit)
        {
            return spiritMaterials[Random.Range(0, spiritMaterials.Count)];
        }

        return frozenMaterials[0];
    }

    //Method the randomly generates a biome for a planet.
    private Planet.Biome GenerateBiome()
    {
        int random = Random.Range(0, 7);

        if (random == 0)
            return Planet.Biome.Frozen;
        else if (random == 1)
            return Planet.Biome.Temperate;
        else if (random == 2)
            return Planet.Biome.Desert;
        else if (random == 3)
            return Planet.Biome.Swamp;
        else if (random == 4)
            return Planet.Biome.Hell;
        else if (random == 5)
            return Planet.Biome.Forest;
        else if (random == 6)
            return Planet.Biome.Spirit;

        return Planet.Biome.Unknown;
    }

    //Method that detects if the planet name is already being used by another planet.
    private bool PlanetNameRedundant(string planetName)
    {
        for(int x = 0; x < planets.Count; x++)
        {
            if (planetName.Equals(planets[x].GetComponent<PlanetIcon>().nameLabel.text))
                return true;
        }

        return false;
    }

    //Reads in the text file that stores planet names.
    private void ReadInPlanetNames()
    {
        //Get list of planet names
        TextAsset planetNamesFile = Resources.Load<TextAsset>("Text/Location Names/Planet Names");
        planetNames = planetNamesFile.text.Split('\n');
    }


    //Returns a random planet name;
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