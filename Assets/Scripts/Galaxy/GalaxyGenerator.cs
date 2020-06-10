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
        hyperspaceLanesManager.GetComponent<HyperspaceLanesManager>().hyperspaceLanes = new List<GameObject>();
        for(int x = 0; x < planets.Count; x++)
        {
            for(int y = 0; y < planets.Count; y++)
            {
                if (x != y && Vector3.Distance(planets[x].transform.localPosition, planets[y].transform.localPosition) <= hyperspaceLaneCheckingRadius)
                {
                    hyperspaceLanesManager.GetComponent<HyperspaceLanesManager>().AddHyperspaceLane(planets[x], planets[y], hyperspaceLanesDaddy);
                }
            }
            if(hyperspaceLanesManager.GetComponent<HyperspaceLanesManager>().CheckIfPlanetHasHyperspaceLanes(planets[x]) == false)
            {
                int indexWithSmallestDistance = -1;
                for(int y = 0; y < planets.Count; y++)
                {
                    if(x != y)
                    {
                        if(indexWithSmallestDistance >= 0)
                        {
                            if (Vector3.Distance(planets[x].transform.localPosition, planets[y].transform.localPosition) < Vector3.Distance(planets[x].transform.localPosition, planets[indexWithSmallestDistance].transform.localPosition))
                                indexWithSmallestDistance = y;
                        }
                        if(y == 0)
                        {
                            indexWithSmallestDistance = 0;
                        }
                    }
                }
                hyperspaceLanesManager.GetComponent<HyperspaceLanesManager>().AddHyperspaceLane(planets[x], planets[indexWithSmallestDistance], hyperspaceLanesDaddy);
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
            while (true)
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
        Vector3 randomPosition;
        bool goodPosition;

        while (true)
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
