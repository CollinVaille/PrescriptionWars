using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGalaxyPlanet : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The game object that has the planet renderer applied to it.")] private GameObject planet = null;
    [SerializeField, Tooltip("The game object that has the atmosphere renderer applied to it.")] private GameObject atmosphere = null;
    [SerializeField, Tooltip("The game object that has the rings renderer applied to it.")] private GameObject rings = null;
    [SerializeField, Tooltip("The transform that marks the location at which the planet label should be placed.")] private Transform planetLabelLocation = null;
    [SerializeField, Tooltip("The transform that marks the location at which the capital symbol should be placed if the planet is the capital of its solar system.")] private Transform capitalSymbolLocation = null;

    //Non-inspector variables.

    /// <summary>
    /// Private holder variable for the name of the planet.
    /// </summary>
    private string planetNameVar;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the planet.
    /// </summary>
    public string planetName { get => planetNameVar; set { planetNameVar = value; gameObject.name = value; } }
    /// <summary>
    /// Private holder variable for the text object that display's the planet's name to the player.
    /// </summary>
    private Text planetNameLabel = null;

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
            planet.GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + value);
            atmosphere.GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + value + " Atmosphere");
            materialNameVar = value;
        }
    }

    /// <summary>
    /// Private variable that holds a boolean value that indictates whether or not the planet has visible rings.
    /// </summary>
    private bool hasRingsVar = false;
    /// <summary>
    /// Public property that should be used both to access and mutate whether the planet has visible rings or not.
    /// </summary>
    public bool hasRings
    {
        get => hasRingsVar;
        set { hasRingsVar = value; rings.SetActive(value); }
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
        get => transform.localScale.x;
        set => transform.localScale = Vector3.one * value;
    }

    /// <summary>
    /// Private variable that holds the amount of degrees that the planet rotates on its own axis every second.
    /// </summary>
    private float rotationSpeedVar = 0;
    /// <summary>
    /// Public property that should be used both to access and mutate the amount of degrees that the planet rotates on its own axis every second.
    /// </summary>
    public float rotationSpeed { get => rotationSpeedVar; set => rotationSpeedVar = value; }

    /// <summary>
    /// Public property that should be used both to access and mutate the speed of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public float cloudSpeed
    {
        get => planet.GetComponent<Renderer>().material.GetFloat("_CloudSpeed");
        set => planet.GetComponent<Renderer>().material.SetFloat("_CloudSpeed", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the main color, not the shadow color, of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public Color cloudColor
    {
        get => planet.GetComponent<Renderer>().material.GetColor("_ColorA");
        set => planet.GetComponent<Renderer>().material.SetColor("_ColorA", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the shadow color, not the main color, of the clouds that are moving in the planet's atmosphere.
    /// </summary>
    public Color cloudShadowColor
    {
        get => planet.GetComponent<Renderer>().material.GetColor("_ShadowColorA");
        set => planet.GetComponent<Renderer>().material.SetColor("_ShadowColorA", value);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the color of the cities that are located on the planet's surface.
    /// </summary>
    public Color cityColor
    {
        get => planet.GetComponent<Renderer>().material.GetColor("_Citiescolor");
        set => planet.GetComponent<Renderer>().material.SetColor("_Citiescolor", value);
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
        get => int.Parse(transform.parent.gameObject.name.Substring(16));
    }

    /// <summary>
    /// Publicly accessible property that returns a boolean that indicates whether or not the planet and its respective orbit are the farthest away from the star in its solar system.
    /// </summary>
    public bool farthestPlanetFromTheStar
    {
        get
        {
            if (solarSystem == null || solarSystem.planets == null)
                return false;
            foreach(NewGalaxyPlanet planet in solarSystem.planets)
                if (planet.planetaryOrbitProximityToStar > planetaryOrbitProximityToStar)
                    return false;
            return true;
        }
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the rotation of the planet's orbit.
    /// </summary>
    public float planetaryOrbitRotation
    {
        get => transform.parent.localRotation.eulerAngles.y;
        set => transform.parent.localRotation = Quaternion.Euler(transform.parent.localRotation.eulerAngles.x, value, transform.parent.localRotation.eulerAngles.z);
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the local position of the top most game object belonging directly to the planet.
    /// </summary>
    public Vector3 localPosition
    {
        get => transform.localPosition;
        set => transform.localPosition = value;
    }

    /// <summary>
    /// Private variable that holds the id of the empire that controls the planet (-1 if the planet is unowned).
    /// </summary>
    private int ownerIDVar = -1;
    /// <summary>
    /// Public property that should be used both to access and mutate which empire controls the planet.
    /// </summary>
    public NewEmpire owner
    {
        get => ownerIDVar == -1 ? null : NewGalaxyManager.empires[ownerIDVar];
        set
        {
            ownerID = value == null ? -1 : value.ID;
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate the empire that controls the planet via ID, though it might be more intuitive usually to use the owner property instead.
    /// </summary>
    public int ownerID
    {
        get => ownerIDVar;
        set
        {
            //Stores the previous owner ID of the planet.
            int previousOwnerID = ownerIDVar;
            //Updates the owner ID to the specified value.
            ownerIDVar = value < 0 || value >= NewGalaxyManager.empires.Count ? -1 : value;
            //Removes the planet from the list of planets controlled by the previous owning empire if needed.
            if (previousOwnerID != -1 && NewGalaxyManager.empires[previousOwnerID].planetIDs.Contains(ID))
                NewGalaxyManager.empires[previousOwnerID].planetIDs.Remove(ID);
            //Adds the planet to the list of planets controlled by the newly specified empire if needed.
            if (ownerIDVar != -1 && !NewGalaxyManager.empires[ownerIDVar].planetIDs.Contains(ID))
                NewGalaxyManager.empires[ownerIDVar].AddPlanet(ownerIDVar);
            //Updates the color of the planet's name label if it has been created already.
            if(planetNameLabel != null)
                planetNameLabel.color = owner == null ? Color.white : owner.labelColor;
            //Executes the needed logic for when the planet's solar system changes owner if the planet is indeed the capital planet of its solar system.
            if (solarSystem.capitalPlanet == this)
                solarSystem.OnOwnerChange(previousOwnerID);
        }
    }

    /// <summary>
    /// Private variable that holds the ID of the planet (which is the planet's index in the list of planets within the galaxy).
    /// </summary>
    private int IDVar = -1;
    /// <summary>
    /// Public property that should be used to access the ID of the planet (which is the planet's index in the list of planets within the galaxy).
    /// </summary>
    public int ID { get => IDVar; }

    /// <summary>
    /// Private variable that holds a reference to the solar system that the planet belongs to.
    /// </summary>
    private GalaxySolarSystem solarSystemVar = null;
    /// <summary>
    /// This public property should be used in order to access the solar system that the planet belongs to.
    /// </summary>
    public GalaxySolarSystem solarSystem { get => solarSystemVar; }

    /// <summary>
    /// Private variable that holds the game object that serves as the parent of the 3d objects that outline the orbit of the planet around the star of the solar system.
    /// </summary>
    private GameObject planetaryOrbitOutline = null;

    /// <summary>
    /// Public property that should be accessed in order to determine whether the planet is the capital planet of its solar system.
    /// </summary>
    public bool isSystemCapital { get => solarSystem.capitalPlanet == this; }

    /// <summary>
    /// Public property that should be accessed in order to determine whether the planet is the capital planet in the owning empire's capital system.
    /// </summary>
    public bool isEmpireCapital { get => owner.capitalSystemID == solarSystem.ID && isSystemCapital; }

    /// <summary>
    /// Private holder variable for the image that marks the planet as either the system's capital or the empire's capital.
    /// </summary>
    private Image capitalSymbolImage = null;

    // Start is called before the first frame update
    void Start()
    {
        //Adds the OnZoomChangeFunction of the planet to the list of functions to be executed by the galaxy camera whenever the camera's zoom percentage meaningfully changes.
        NewGalaxyCamera.AddZoomFunction(OnZoomPercentageChange);
    }

    // Update is called once per frame
    void Update()
    {
        //Updates the planet's rotation.
        planet.transform.localRotation = Quaternion.Euler(planet.transform.localRotation.eulerAngles.x, planet.transform.localRotation.eulerAngles.y + (rotationSpeed * Time.deltaTime), planet.transform.localRotation.eulerAngles.z);

        //Updates the planet name label's position.
        planetNameLabel.transform.position = Camera.main.WorldToScreenPoint(planetLabelLocation.transform.position);
        planetNameLabel.transform.localPosition = (Vector2)planetNameLabel.transform.localPosition;

        //Updates the capital symbol image's position.
        if(capitalSymbolImage != null)
        {
            capitalSymbolImage.transform.position = Camera.main.WorldToScreenPoint(capitalSymbolLocation.transform.position);
            capitalSymbolImage.transform.localPosition = (Vector2)capitalSymbolImage.transform.localPosition;
        }
    }

    /// <summary>
    /// Private method that should be universally (aka regardless of whether starting a new game or loading an old game) called in order to initialize all needed data members of the planet.
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
    private void Initialize(GalaxySolarSystem solarSystem, int ID, string planetName, Planet.Biome biomeType, string materialName, bool hasRings, float ringSize, float planetarySize, float planetaryRotationSpeed, float cloudSpeed, DualColorSet cloudColorCombo, Color cityColor, DualColorSet ringColorCombo, Light starLight)
    {
        //Initializes the reference of the solar system that the planet belongs to.
        solarSystemVar = solarSystem;
        solarSystemVar.AddOnBecameVisibleFunction(OnSolarSystemBecameVisible);
        solarSystemVar.AddOnBecameInvisibleFunction(OnSolarSystemBecameInvisible);
        //Initializes the ID of the planet.
        IDVar = ID;
        //Initializes the name of the planet.
        this.planetName = planetName;
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
        //Initializes the rotation speed of the planet.
        rotationSpeed = planetaryRotationSpeed;
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
        gameObject.GetComponent<LightSource>().Sun = starLight.gameObject;

        //Adds the OnGalaxyGenerationCompletion function to the list of functions to be executed once the galaxy has completely finished generating.
        NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 0);
    }

    /// <summary>
    /// This method is called by the GenerateSolarSystems method in the galaxy generator when loading save game data to generate the galaxy and initializes all needed variables for the galaxy planet.
    /// </summary>
    /// <param name="planetData"></param>
    /// <param name="starLight"></param>
    public void InitializeFromSaveData(GalaxyPlanetData planetData, GalaxySolarSystem solarSystem, int ID, Light starLight)
    {
        Initialize(solarSystem, ID, planetData.planetName, planetData.biomeType, planetData.materialName, planetData.hasRings, planetData.ringSize, planetData.planetarySize, planetData.planetaryRotationSpeed, planetData.cloudSpeed, new DualColorSet(new Color(planetData.cloudColorCombo[0], planetData.cloudColorCombo[1], planetData.cloudColorCombo[2], planetData.cloudColorCombo[3]), new Color(planetData.cloudColorCombo[4], planetData.cloudColorCombo[5], planetData.cloudColorCombo[6], planetData.cloudColorCombo[7])), new Color(planetData.cityColor[0], planetData.cityColor[1], planetData.cityColor[2], planetData.cityColor[3]), new DualColorSet(new Color(planetData.ringColorCombo[0], planetData.ringColorCombo[1], planetData.ringColorCombo[2], planetData.ringColorCombo[3]), new Color(planetData.ringColorCombo[4], planetData.ringColorCombo[5], planetData.ringColorCombo[6], planetData.ringColorCombo[7])), starLight);
        ownerIDVar = planetData.ownerID;
    }

    /// <summary>
    /// This method is called by the GenerateSolarSystems method in the galaxy generator when there is no load game data and a new galaxy is being generated and initializes all needed variables for the galaxy planet.
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="starLight"></param>
    public void InitializeFromGalaxyGenerator(GalaxySolarSystem solarSystem, int ID, string planetName, NewGalaxyBiome biome, Light starLight)
    {
        Initialize(solarSystem, ID, planetName, biome.biome, biome.randomMaterialName, UnityEngine.Random.Range(0f, 1f) <= biome.planetaryRingChance, biome.randomRingSize, biome.randomPlanetarySize, biome.randomPlanetaryRotationSpeed, biome.randomCloudSpeed, biome.randomCloudColorCombo, biome.randomCityColor, biome.randomRingColorCombo, starLight);
    }

    /// <summary>
    /// This function should be added to the list of functions to be executed once the galaxy has fully finished generating.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        //Sets the owner of the planet.
        if (ownerIDVar == -1)
        {
            for (int empireID = 0; empireID < NewGalaxyManager.empires.Count; empireID++)
            {
                if (NewGalaxyManager.empires[empireID].solarSystemIDs.Contains(solarSystem.ID))
                {
                    ownerID = empireID;
                    break;
                }
            }
        }
        else
        {
            ownerID = ownerIDVar;
        }

        //Instantiates the planet's name label.
        planetNameLabel = Instantiate(Resources.Load<GameObject>("Galaxy/Prefabs/Planet Label")).GetComponent<Text>();
        planetNameLabel.transform.SetParent(NewGalaxyManager.planetLabelsParent);
        planetNameLabel.transform.localScale = Vector3.one;
        planetNameLabel.text = planetName;
        planetNameLabel.color = owner == null ? Color.white : owner.labelColor;
        planetNameLabel.gameObject.name = planetName + " Label";
        planetNameLabel.transform.position = Camera.main.WorldToScreenPoint(planetLabelLocation.transform.position);
        planetNameLabel.transform.localPosition = (Vector2)planetNameLabel.transform.localPosition;

        //Instantiates the planet's capital system image.
        if (isSystemCapital)
        {
            capitalSymbolImage = new GameObject().AddComponent<Image>();
            capitalSymbolImage.transform.SetParent(NewGalaxyManager.capitalSymbolsParent);
            capitalSymbolImage.transform.localScale = Vector3.one;
            capitalSymbolImage.rectTransform.sizeDelta = new Vector2(30, 30);
            capitalSymbolImage.rectTransform.pivot = new Vector2(0.5f, 0);
            capitalSymbolImage.raycastTarget = false;
            capitalSymbolImage.sprite = isEmpireCapital ? Resources.Load<Sprite>("Galaxy/Icons/Empire Capital Icon") : Resources.Load<Sprite>("Galaxy/Icons/System Capital Icon");
            capitalSymbolImage.gameObject.name = planetName + " Capital Symbol";
            capitalSymbolImage.transform.position = Camera.main.WorldToScreenPoint(capitalSymbolLocation.transform.position);
            capitalSymbolImage.transform.localPosition = (Vector2)capitalSymbolImage.transform.localPosition;
        }

        //Executes the OnZoomPercentageChange function at the start in order to ensure the planet is adapted to the galaxy camera's initial zoom percentage.
        OnZoomPercentageChange();
    }

    /// <summary>
    /// Private function that should be added to the list of functions to be called by the galaxy camera whenever the zoom percentage of the camera changes and updates the planet's visibility to optimize performance among other things.
    /// </summary>
    private void OnZoomPercentageChange()
    {
        //Updates the activation state of the renderer game objects of the planet in order to optimize the game's performance if the camera is too zoomed out and wouldn't even see the planet anyway.
        bool planetsVisible = NewGalaxyCamera.planetsVisible;
        if (planet.activeSelf != (solarSystem.visible && planetsVisible))
        {
            planet.SetActive(solarSystem.visible && planetsVisible);
            atmosphere.SetActive(solarSystem.visible && planetsVisible);
            rings.SetActive(solarSystem.visible && hasRings && planetsVisible);
            if(planetNameLabel != null)
                planetNameLabel.gameObject.SetActive(solarSystem.visible && planetsVisible);
            if (capitalSymbolImage != null)
                capitalSymbolImage.gameObject.SetActive(solarSystem.visible && planetsVisible);
        }

        //Instantiates a planetary orbit outline if the player is now zoomed far enough into the galaxy to see it.
        if(planetaryOrbitOutline == null && NewGalaxyCamera.planetaryOrbitOutlinesVisible && solarSystem.visible)
        {
            CreatePlanetaryOrbitOutline();
        }
        //Destroys the previously created planetary orbit outine if the player is now too far zoomed out to see it.
        else if (planetaryOrbitOutline != null && (!NewGalaxyCamera.planetaryOrbitOutlinesVisible || !solarSystem.visible))
        {
            Destroy(planetaryOrbitOutline);
            planetaryOrbitOutline = null;
            if (farthestPlanetFromTheStar)
                foreach (HyperspaceLane hyperspaceLane in solarSystem.hyperspaceLanes)
                    hyperspaceLane.SetSolarSystemPosition(solarSystem, solarSystem.transform.position);
        }
    }

    /// <summary>
    /// Private method that should be called in order to create a visible outline of the planet's orbit for the player to see.
    /// </summary>
    private void CreatePlanetaryOrbitOutline()
    {
        planetaryOrbitOutline = Instantiate(Resources.Load<GameObject>("Galaxy/Prefabs/Planetary Orbit Outline"));
        planetaryOrbitOutline.transform.SetParent(transform.parent);
        planetaryOrbitOutline.transform.localPosition = new Vector3(0, (-0.003f * planetaryOrbitProximityToStar), 0);
        planetaryOrbitOutline.transform.localScale = Vector3.one;
        planetaryOrbitOutline.transform.GetChild(1).localScale = new Vector3((localPosition.x * (1.005f + (0.0008f * planetaryOrbitProximityToStar)) * 2) - (planetaryOrbitOutline.transform.GetChild(0).localScale.x - planetaryOrbitOutline.transform.GetChild(1).localScale.x), (localPosition.x * (1.005f + (0.0008f * planetaryOrbitProximityToStar)) * 2) - (planetaryOrbitOutline.transform.GetChild(0).localScale.y - planetaryOrbitOutline.transform.GetChild(1).localScale.y), planetaryOrbitOutline.transform.GetChild(1).localScale.z);
        planetaryOrbitOutline.transform.GetChild(0).localScale = new Vector3(localPosition.x * (1.005f + (0.0008f * planetaryOrbitProximityToStar)) * 2, localPosition.x * (1.005f + (0.0008f * planetaryOrbitProximityToStar)) * 2, planetaryOrbitOutline.transform.GetChild(0).localScale.z);
        if (farthestPlanetFromTheStar)
        {
            Physics.SyncTransforms();
            foreach(HyperspaceLane hyperspaceLane in solarSystem.hyperspaceLanes)
            {
                GalaxySolarSystem connectingSolarSystem = hyperspaceLane.solarSystems[0] == solarSystem ? hyperspaceLane.solarSystems[1] : hyperspaceLane.solarSystems[0];
                hyperspaceLane.SetSolarSystemPosition(solarSystem, planetaryOrbitOutline.transform.GetChild(0).GetComponent<SphereCollider>().ClosestPoint(connectingSolarSystem.transform.position));
            }
        }
    }

    /// <summary>
    /// Public method that should be called by the planet's solar system whenever the planet becomes the capital planet of the empire.
    /// </summary>
    public void OnBecameEmpireCapitalPlanet()
    {

    }

    /// <summary>
    /// Public method that should be called by the planet's solar system whenever the planet is no longer the capital planet of the empire.
    /// </summary>
    public void OnBecameEmpireNoncapitalPlanet()
    {

    }

    /// <summary>
    /// This method is added to the list of functions to be called by the solar system once it becomes visible to the main galaxy camera.
    /// </summary>
    private void OnSolarSystemBecameVisible()
    {
        OnZoomPercentageChange();
    }

    /// <summary>
    /// This method is added to the list of functions to be called by the solar system once it becomes invisible to the main galaxy camera.
    /// </summary>
    private void OnSolarSystemBecameInvisible()
    {
        OnZoomPercentageChange();
    }

    /// <summary>
    /// This function is called whenever the planet is destroyed and removes the planet's OnZoomPercentageChange function from the list of functions to be executed once the galaxy camera's zoom percentage meaningfully changes.
    /// </summary>
    private void OnDestroy()
    {
        //Removes the planet's OnZoomPercentageChange function from the list of functions to be executed by the galaxy camera whenever the camera's zoom percentage meaningfully changes.
        NewGalaxyCamera.RemoveZoomFunction(OnZoomPercentageChange);
    }
}

[System.Serializable]
public class GalaxyPlanetData
{
    public string planetName;
    public Planet.Biome biomeType = Planet.Biome.Unknown;
    public string materialName = null;

    public bool hasRings = false;
    public float ringSize = 0;
    public float planetarySize = 0;
    public float planetaryRotationSpeed = 0;
    public float cloudSpeed = 0;
    public float[] cloudColorCombo;
    public float[] cityColor;
    public float[] ringColorCombo;

    public int planetaryOrbitProximityToStar = 0;
    public float planetaryOrbitRotation = 0;
    public float[] localPosition = new float[3];

    public int ownerID = -1;

    public GalaxyPlanetData(NewGalaxyPlanet planet)
    {
        planetName = planet.planetName;
        biomeType = planet.biomeType;
        materialName = planet.materialName;

        hasRings = planet.hasRings;
        ringSize = planet.ringSize;
        planetarySize = planet.planetarySize;
        planetaryRotationSpeed = planet.rotationSpeed;
        cloudSpeed = planet.cloudSpeed;
        cloudColorCombo = new float[8] { planet.cloudColor.r, planet.cloudColor.g, planet.cloudColor.b, planet.cloudColor.a, planet.cloudShadowColor.r, planet.cloudShadowColor.g, planet.cloudShadowColor.b, planet.cloudShadowColor.a };
        cityColor = new float[4] { planet.cityColor.r, planet.cityColor.g, planet.cityColor.b, planet.cityColor.a };
        ringColorCombo = new float[8] { planet.primaryRingColor.r, planet.primaryRingColor.g, planet.primaryRingColor.b, planet.primaryRingColor.a, planet.secondaryRingColor.r, planet.secondaryRingColor.g, planet.secondaryRingColor.b, planet.secondaryRingColor.a };

        planetaryOrbitProximityToStar = planet.planetaryOrbitProximityToStar;
        planetaryOrbitRotation = planet.planetaryOrbitRotation;
        localPosition[0] = planet.localPosition.x;
        localPosition[1] = planet.localPosition.y;
        localPosition[2] = planet.localPosition.z;

        ownerID = planet.ownerID;
    }
}