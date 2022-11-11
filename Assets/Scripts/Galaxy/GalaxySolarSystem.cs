using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The particle system that creates the space dust clouds that encompass the solar system.")] private ParticleSystem spaceDustPS = null;
    [SerializeField, LabelOverride("Planetary Orbits Parent"), Tooltip("The transform of the game object that serves as the parent of all planet orbits in the solar system.")] private Transform planetaryOrbitsParentVar = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Private variable that holds a reference to the star that is at the center of this solar system. Should be accessed from the publicly accessible property.
    /// </summary>
    private GalaxyStar starVar = null;
    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

    /// <summary>
    /// Public property that should be used to acces the parent of orbits that planets in the solar system can be assigned to.
    /// </summary>
    public Transform planetaryOrbitsParent { get => planetaryOrbitsParentVar; }

    /// <summary>
    /// Private list that holds the planets that are contained within this solar system in the galaxy.
    /// </summary>
    private List<NewGalaxyPlanet> planetsVar = null;
    /// <summary>
    /// Public property that should be used in order to access the list of planets that are contained within this solar system in the galaxy.
    /// </summary>
    public List<NewGalaxyPlanet> planets { get => planetsVar; }

    /// <summary>
    /// Private variable that holds the index of the planet in the system's list of planets that is the capital planet of the system.
    /// </summary>
    private int capitalPlanetIndexVar = 0;
    /// <summary>
    /// Public property that should be used to access the index of the solar system's capital planet in the list of planets in the solar system.
    /// </summary>
    public int capitalPlanetIndex { get => capitalPlanetIndexVar; }
    /// <summary>
    /// Public property that should be used to access the planet that serves as the capital planet of the solar system.
    /// </summary>
    public NewGalaxyPlanet capitalPlanet { get => planets[capitalPlanetIndexVar]; }

    /// <summary>
    /// Public property that should be used to access the empire that controls the solar system, which is actually just the empire that controls the capital planet of the solar system.
    /// </summary>
    public NewEmpire owner { get => capitalPlanet.owner; }
    /// <summary>
    /// Public property that should be used to access the ID of the empire that controls the solar system, which is actually just the empire that controls the capital planet of the solar system, though it might be more intuitive usually to use the owner property directly.
    /// </summary>
    public int ownerID { get => capitalPlanet.ownerID; }

    /// <summary>
    /// Public property that should be used in order to access the color of the empire that owns the solar system. Returns white if no empire controls the solar system.
    /// </summary>
    public Color ownerColor { get => owner == null ? Color.white : owner.color; }

    /// <summary>
    /// Private variable that holds the ID of the solar system (index in the list of solar systems in the galaxy).
    /// </summary>
    private int IDVar = -1;
    /// <summary>
    /// Public property that should be used to access the ID of the solar system (index in the list of solar systems in the galaxy).
    /// </summary>
    public int ID { get => IDVar; }

    private void Awake()
    {
        
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
    /// This public method should be called in order to initialize the solar system from the GenerateSolarSystems method in the galaxy generator.
    /// </summary>
    /// <param name="star"></param>
    public void InitializeFromGalaxyGenerator(GalaxyStar star, List<NewGalaxyPlanet> planets, int ID)
    {
        //Initializes the star variable.
        starVar = star;
        //Initializes the planets list variable.
        planetsVar = planets;
        //Initializes the int that indicates which planet in the solar system serves as the capital planet of the system.
        capitalPlanetIndexVar = 0;
        //Initializes the ID of the solar system.
        IDVar = ID;
    }

    /// <summary>
    /// This public method should be called by the GenerateSolarSystems method in the galaxy generator in order to initialize the solar system from save data that has been statically passed over from the load game menu.
    /// </summary>
    /// <param name="solarSystemData"></param>
    public void InitializeFromSaveData(GalaxySolarSystemData solarSystemData, GalaxyStar star, int ID)
    {
        //Loads in the local position from the solar system save data.
        transform.localPosition = new Vector3(solarSystemData.localPosition[0], solarSystemData.localPosition[1], solarSystemData.localPosition[2]);
        //Sets the star variable to the star that has already been loaded in from the solar system save data.
        starVar = star;
        //Sets the variable that indicates the index of the system's capital planet in the list of planets in the solar system.
        capitalPlanetIndexVar = solarSystemData.capitalPlanetIndex;
        //Sets the variable that indicates the ID of the solar system.
        IDVar = ID;
    }

    /// <summary>
    /// Public function that should be called by the script of the empire's capital planet whenever its owner changes.
    /// </summary>
    public void OnOwnerChange(int previousOwnerID)
    {
        //Removes the solar system from the list of solar systems controlled by the previous owning empire if needed.
        if(previousOwnerID >= 0 && previousOwnerID < NewGalaxyManager.empires.Count)
            NewGalaxyManager.empires[previousOwnerID].solarSystemIDs.Remove(ID);
        //Adds the solar system to the list of solar systems controlled by the new owner empire if needed.
        if (owner != null)
            owner.solarSystemIDs.Add(ID);

        //Updates the color of the solar system's space dust particle system.
        ParticleSystem.MainModule spaceDustPSMain = spaceDustPS.main;
        spaceDustPSMain.startColor = Color.Lerp(spaceDustPSMain.startColor.color, new Color(ownerColor.r, ownerColor.g, ownerColor.b, spaceDustPSMain.startColor.color.a), 0.09f);
    }
}

[System.Serializable]
public class GalaxySolarSystemData
{
    public float[] localPosition = new float[3];
    public GalaxyStarData star = null;
    public List<GalaxyPlanetData> planets = null;
    public int capitalPlanetIndex = 0;

    public GalaxySolarSystemData(GalaxySolarSystem solarSystem)
    {
        localPosition[0] = solarSystem.transform.localPosition.x;
        localPosition[1] = solarSystem.transform.localPosition.y;
        localPosition[2] = solarSystem.transform.localPosition.z;

        star = new GalaxyStarData(solarSystem.star);

        planets = new List<GalaxyPlanetData>();
        for(int planetIndex = 0; planetIndex < solarSystem.planets.Count; planetIndex++)
            planets.Add(new GalaxyPlanetData(solarSystem.planets[planetIndex]));

        capitalPlanetIndex = solarSystem.capitalPlanetIndex;
    }
}