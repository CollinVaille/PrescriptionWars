using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyPlanet : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The game object that has the atmosphere renderer applied to it.")] private GameObject atmosphere = null;
    [SerializeField, Tooltip("The game object that has the rings renderer applied to it.")] private GameObject rings = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that holds what type of biome the planet belongs to.
    /// </summary>
    private Planet.Biome biomeTypeVar = Planet.Biome.Unknown;
    /// <summary>
    /// Public property that should be used to acces the type of biome that the planet belongs to.
    /// </summary>
    public Planet.Biome biomeType { get => biomeTypeVar; }

    /// <summary>
    /// Private variable that holds the name of the material applied to the planet.
    /// </summary>
    private string materialNameVar = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the material that is applied to the planet.
    /// </summary>
    public string materialName
    {
        get => materialNameVar;
        set
        {
            GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + value);
            atmosphere.GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + value + " Atmosphere");
            materialNameVar = value;
        }
    }

    /// <summary>
    /// Public property that should be used both to access and mutate whether the planet has visible rings or not.
    /// </summary>
    public bool hasRings
    {
        get => rings.activeSelf;
        set => rings.SetActive(value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the size of the planet's ring.
    /// </summary>
    public float ringSize
    {
        get => rings.GetComponent<Renderer>().material.GetFloat("_Size");
        set => rings.GetComponent<Renderer>().material.SetFloat("_Size", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the size of the actual planet.
    /// </summary>
    public float planetarySize
    {
        get => transform.parent.localScale.x;
        set => transform.parent.localScale = Vector3.one * value;
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the speed of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public float cloudSpeed
    {
        get => GetComponent<Renderer>().material.GetFloat("_CloudSpeed");
        set => GetComponent<Renderer>().material.SetFloat("_CloudSpeed", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the main color, not the shadow color, of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public Color cloudColor
    {
        get => GetComponent<Renderer>().material.GetColor("_ColorA");
        set => GetComponent<Renderer>().material.SetColor("_ColorA", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the shadow color, not the main color, of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public Color cloudShadowColor
    {
        get => GetComponent<Renderer>().material.GetColor("_ShadowColorA");
        set => GetComponent<Renderer>().material.SetColor("_ShadowColorA", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the color of the cities that are located on the planet's surface.
    /// </summary>
    public Color cityColor
    {
        get => GetComponent<Renderer>().material.GetColor("_Citiescolor");
        set => GetComponent<Renderer>().material.SetColor("_Citiescolor", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the primary color of the planet's rings.
    /// </summary>
    public Color primaryRingColor
    {
        get => rings.GetComponent<Renderer>().material.GetColor("_BaseColor");
        set => rings.GetComponent<Renderer>().material.SetColor("_BaseColor", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the secondary color of the planet's rings.
    /// </summary>
    public Color secondaryRingColor
    {
        get => rings.GetComponent<Renderer>().material.GetColor("_Color1");
        set => rings.GetComponent<Renderer>().material.SetColor("_Color1", value);
    }

    /// <summary>
    /// Public property that should be used to access how close the planet's orbit is to the star compare to the other orbits (with 0 being the closest proximity to the star).
    /// </summary>
    public int planetaryOrbitProximityToStar
    {
        get => int.Parse(transform.parent.parent.gameObject.name.Substring(16, transform.parent.parent.gameObject.name.Length - 16)) - 1;
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the rotation of the planet's orbit.
    /// </summary>
    public float planetaryOrbitRotation
    {
        get => transform.parent.parent.localRotation.eulerAngles.y;
        set => transform.parent.parent.localRotation = Quaternion.Euler(transform.parent.parent.localRotation.eulerAngles.x, value, transform.parent.parent.localRotation.eulerAngles.z);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the local position of the top most game object belonging directly to the planet.
    /// </summary>
    public Vector3 localPosition
    {
        get => transform.parent.localPosition;
        set => transform.parent.localPosition = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Private method that should be universally (aka regardless of whether starting a new game or loading an old game) called in order to initialize all needed data members of thr planet.
    /// </summary>
    /// <param name="biomeType"></param>
    /// <param name="materialName"></param>
    /// <param name="hasRings"></param>
    /// <param name="ringSize"></param>
    /// <param name="planetarySize"></param>
    /// <param name="cloudSpeed"></param>
    /// <param name="cloudColorCombo"></param>
    /// <param name="cityColor"></param>
    /// <param name="ringColorCombo"></param>
    /// <param name="starLight"></param>
    private void Initialize(Planet.Biome biomeType, string materialName, bool hasRings, float ringSize, float planetarySize, float cloudSpeed, DualColorSet cloudColorCombo, Color cityColor, DualColorSet ringColorCombo, Light starLight)
    {
        //Initializes the biome type.
        biomeTypeVar = biomeType;
        //Initializes the material of the planet.
        this.materialName = materialName;
        //Initializes whether the planet has ctive rings or not.
        this.hasRings = hasRings;
        //Initializes the size of the planet's rings.
        this.ringSize = ringSize;
        //Initializes the size of the planet.
        this.planetarySize = planetarySize;
        //Initializes the speed of the clouds on the planet.
        this.cloudSpeed = cloudSpeed;
        //Initializes the color of the planet's clouds and cloud shadow.
        cloudColor = cloudColorCombo[0];
        cloudShadowColor = cloudColorCombo[1];
        //Initializes the color of the cities on the planet.
        this.cityColor = cityColor;
        //Initializes the color of the planet's rings.
        primaryRingColor = ringColorCombo[0];
        secondaryRingColor = ringColorCombo[1];
        //Initializes the light source of the planet to the light being emitted from the star in the solar system.
        transform.parent.gameObject.GetComponent<LightSource>().Sun = starLight.gameObject;
    }

    /// <summary>
    /// This method is called by the GenerateSolarSystems method in the galaxy generator when loading save game data to generate the galaxy and initializes all needed variables for the galaxy planet.
    /// </summary>
    /// <param name="planetData"></param>
    /// <param name="starLight"></param>
    public void InitializeFromSaveData(GalaxyPlanetData planetData, Light starLight)
    {
        Initialize(planetData.biomeType, planetData.materialName, planetData.hasRings, planetData.ringSize, planetData.planetarySize, planetData.cloudSpeed, planetData.cloudColorCombo, planetData.cityColor, planetData.ringColorCombo, starLight);
    }

    /// <summary>
    /// This method is called by the GenerateSolarSystems method in the galaxy generator when there is no load game data and a new galaxy is being generated and initializes all needed variables for the galaxy planet.
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="starLight"></param>
    public void InitializeFromGalaxyGenerator(NewGalaxyBiome biome, Light starLight)
    {
        Initialize(biome.biome, biome.randomMaterialName, UnityEngine.Random.Range(0f, 1f) <= biome.planetaryRingChance, biome.randomRingSize, biome.randomPlanetarySize, biome.randomCloudSpeed, biome.randomCloudColorCombo, biome.randomCityColor, biome.randomRingColorCombo, starLight);
    }
}

[System.Serializable]
public class GalaxyPlanetData
{
    public Planet.Biome biomeType = Planet.Biome.Unknown;
    public string materialName = null;

    public bool hasRings = false;
    public float ringSize = 0;
    public float planetarySize = 0;
    public float cloudSpeed = 0;
    public DualColorSet cloudColorCombo;
    public Color cityColor;
    public DualColorSet ringColorCombo;

    public int planetaryOrbitProximityToStar = 0;
    public float planetaryOrbitRotation = 0;
    public float[] localPosition = new float[3];

    public GalaxyPlanetData(NewGalaxyPlanet planet)
    {
        biomeType = planet.biomeType;
        materialName = planet.materialName;

        hasRings = planet.hasRings;
        ringSize = planet.ringSize;
        planetarySize = planet.planetarySize;
        cloudSpeed = planet.cloudSpeed;
        cloudColorCombo = new DualColorSet(planet.cloudColor, planet.cloudShadowColor);
        cityColor = planet.cityColor;
        ringColorCombo = new DualColorSet(planet.primaryRingColor, planet.secondaryRingColor);

        planetaryOrbitProximityToStar = planet.planetaryOrbitProximityToStar;
        planetaryOrbitRotation = planet.planetaryOrbitRotation;
        localPosition[0] = planet.localPosition.x;
        localPosition[1] = planet.localPosition.y;
        localPosition[2] = planet.localPosition.z;
    }
}