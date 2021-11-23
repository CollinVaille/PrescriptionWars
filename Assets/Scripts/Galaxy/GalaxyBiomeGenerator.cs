using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyBiomeGenerator : MonoBehaviour
{
    private static GalaxyBiomeGenerator galaxyBiomeGeneratorLocal = null;
    public static GalaxyBiomeGenerator galaxyBiomeGenerator { get => galaxyBiomeGeneratorLocal; }

    [SerializeField] private List<GalaxyBiome> galaxyBiomes = new List<GalaxyBiome>();

    private void Awake()
    {
        galaxyBiomeGeneratorLocal = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateGalaxyPlanetBiomes(List<GalaxyPlanet> galaxyPlanets)
    {
        foreach(GalaxyPlanet galaxyPlanet in galaxyPlanets)
        {
            GalaxyBiome galaxyBiome = GetGalaxyBiome(galaxyPlanet.Biome);
            if (galaxyBiome == null)
                continue;

            //Randomizes the planet's material.
            galaxyPlanet.ChangeMaterial(galaxyBiome.RandomMaterialName);
            //Determines if the planet should have a ring.
            galaxyPlanet.Rings.SetActive(Random.Range(0f, 1f) <= galaxyBiome.PlanetaryRingChance);
            //Randomizes the planet's ring size.
            galaxyPlanet.Rings.GetComponent<Renderer>().material.SetFloat("_Size", galaxyBiome.RandomRingSize);
            //Randomizes the planet's size.
            galaxyPlanet.transform.parent.localScale = Vector3.one * galaxyBiome.RandomPlanetarySize;
            //Randomizes the planet's spin/rotation speed.
            galaxyPlanet.rotationSpeed = galaxyBiome.RandomPlanetarySpinSpeed;
            //Randomizes the speed of the planet's clouds.
            galaxyPlanet.GetComponent<Renderer>().material.SetFloat("_CloudSpeed", galaxyBiome.RandomCloudSpeed);
            //Randomizes the color of the planet's clouds.
            galaxyPlanet.GetComponent<Renderer>().material.SetColor("_ColorA", galaxyBiome.RandomCloudColor);
            //Randomizes the color of the planet's cities.
            galaxyPlanet.GetComponent<Renderer>().material.SetColor("_Citiescolor", galaxyBiome.RandomCityColor);
            //Randomizes the color of the planet's rings.
            DualColorSet randomRingColorCombo = galaxyBiome.RandomRingColorCombo;
            galaxyPlanet.Rings.GetComponent<Renderer>().material.SetColor("_BaseColor", randomRingColorCombo[0]);
            galaxyPlanet.Rings.GetComponent<Renderer>().material.SetColor("_Color1", randomRingColorCombo[1]);
        }
    }

    private GalaxyBiome GetGalaxyBiome(Planet.Biome biome)
    {
        foreach(GalaxyBiome galaxyBiome in galaxyBiomes)
        {
            if (galaxyBiome.Biome == biome)
                return galaxyBiome;
        }

        return null;
    }
}

[System.Serializable] public class GalaxyBiome
{
    [SerializeField] private Planet.Biome biome = Planet.Biome.Unknown;
    [SerializeField, Tooltip("The chance that a planet of this biome will spawn with a ring (Range: 0-1).")] private float planetaryRingChance = 0.2f;
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can have its rings be.")] private Vector2 planetaryRingSizeRange = new Vector2(0.25f, 0.69f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) size that a planet of this biome can be.")] private Vector2 planetarySizeRange = new Vector2(0.2f, 0.3f);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) speeds that a planet of this biome can spin at.")] private Vector2 planetarySpinSpeedRange = new Vector2(5, 21);
    [SerializeField, Tooltip("The minimum (x) and maximum (y) speeds that a planet of this biome can have clouds moving.")] private Vector2 cloudSpeedRange = new Vector2(15, 40);
    [SerializeField] private List<string> planetMaterialNames = new List<string>();
    private List<string> planetMaterialNamesUsed = new List<string>();
    [SerializeField] private List<Color> cloudColors = new List<Color>();
    [SerializeField] private List<Color> cityColors = new List<Color>();
    [SerializeField] private List<DualColorSet> ringColorCombos = new List<DualColorSet>();

    public Planet.Biome Biome { get => biome; }
    public float PlanetaryRingChance { get => planetaryRingChance; }
    public string RandomMaterialName
    {
        get
        {
            //Clones planet material names list.
            List<string> localPlanetMaterialNames = new List<string>(planetMaterialNames);
            if(localPlanetMaterialNames.Count <= planetMaterialNamesUsed.Count)
                planetMaterialNamesUsed.Clear();
            else if (planetMaterialNamesUsed.Count > 0)
            {
                //Removes each planet material name already used.
                foreach (string planetMaterialName in planetMaterialNamesUsed)
                    localPlanetMaterialNames.Remove(planetMaterialName);
            }
            //Gets a random planet material name not already used.
            string randomPlanetMaterialName = localPlanetMaterialNames[Random.Range(0, localPlanetMaterialNames.Count)];
            //Marks the random planet material name as used.
            planetMaterialNamesUsed.Add(randomPlanetMaterialName);
            return randomPlanetMaterialName;
        }
    }
    public float RandomRingSize { get => Random.Range(planetaryRingSizeRange.x, planetaryRingSizeRange.y); }
    public float RandomPlanetarySize { get => Random.Range(planetarySizeRange.x, planetarySizeRange.y); }
    public float RandomPlanetarySpinSpeed { get => Random.Range(planetarySpinSpeedRange.x, planetarySpinSpeedRange.y); }
    public float RandomCloudSpeed { get => Random.Range(cloudSpeedRange.x, cloudSpeedRange.y); }
    public Color RandomCloudColor { get => cloudColors[Random.Range(0, cloudColors.Count)]; }
    public Color RandomCityColor { get => cityColors[Random.Range(0, cityColors.Count)]; }
    public DualColorSet RandomRingColorCombo { get => ringColorCombos[Random.Range(0, ringColorCombos.Count)]; }
}

[System.Serializable] public struct DualColorSet
{
    public Color colorOne;
    public Color colorTwo;

    public Color this[int index]
    {
        get => GetColor(index);
        set => SetColor(index, value);
    }

    private Color GetColor(int index)
    {
        if (index == 0)
            return colorOne;
        else if (index == 1)
            return colorTwo;
        else
        {
            Debug.Log("Invalid Dual Color Set Index (must be either 0 or 1).");
            return new Color();
        }
    }

    private void SetColor(int index, Color color)
    {
        if (index == 0)
            colorOne = color;
        else if (index == 1)
            colorTwo = color;
        else
            Debug.Log("Invalid Dual Color Set Index (must be either 0 or 1).");
    }
}