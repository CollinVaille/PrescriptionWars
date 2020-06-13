using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyGenerator : MonoBehaviour
{
    string[] planetNames;

    public int numberOfPlanets;
    public int distanceBetweenPlanets;
    public int hyperspaceLaneCheckingRadius;

    public float leftBoundary;
    public float rightBoundary;
    public float topBoundary;
    public float bottomBoundary;

    public GameObject planetPrefab;
    public Transform planetDaddy;
    public Transform hyperspaceLanesDaddy;

    List<GameObject> planets;
    public GameObject hyperspaceLanesManager;

    public List<Material> frozenMaterials;
    public List<Material> spiritMaterials;
    public List<Material> temperateMaterials;
    public List<Material> desertMaterials;
    public List<Material> swampMaterials;
    public List<Material> hellMaterials;
    public List<Material> forestMaterials;

    //Planet.Biome lol = Planet.Biome.Unknown;

    // Start is called before the first frame update
    void Start()
    {
        GeneratePlanets();
        GenerateHyperspaceLanes();
        GalaxyManager.Initialize(planets);
        //Physics.CheckSphere()
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadNewGameSettings()
    {
        
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
        TextAsset planetNamesFile = Resources.Load<TextAsset>("Text/Planet Names");
        planetNames = planetNamesFile.text.Split('\n');
    }


    //Returns a random planet name;
    private string GeneratePlanetName()
    {
        string planetName = "";

        if (Random.Range(0, 5) != 0) //Normal random name
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
        else //Some prefix + major or minor name
        {
            string[] prefixes = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega", "Ursa", "Virgo", "Canis", "Pisces", "Saega", "Polis"};

            if (Random.Range(0, 2) == 0)
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Major";
            else
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Minor";
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