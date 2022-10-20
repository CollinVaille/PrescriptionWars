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
    public void InitializeFromGalaxyGenerator(GalaxyStar star, List<NewGalaxyPlanet> planets)
    {
        //Initializes the star variable.
        starVar = star;
        //Initializes the planets list variable.
        planetsVar = planets;
    }

    /// <summary>
    /// This public method should be called by the GenerateSolarSystems method in the galaxy generator in order to initialize the solar system from save data that has been statically passed over from the load game menu.
    /// </summary>
    /// <param name="solarSystemData"></param>
    public void InitializeFromSaveData(GalaxySolarSystemData solarSystemData, GalaxyStar star)
    {
        //Loads in the local position from the solar system save data.
        transform.localPosition = new Vector3(solarSystemData.localPosition[0], solarSystemData.localPosition[1], solarSystemData.localPosition[2]);
        //Sets the star variable to the star that has already been loaded in from the solar system save data.
        starVar = star;
    }
}

[System.Serializable]
public class GalaxySolarSystemData
{
    public float[] localPosition = new float[3];
    public GalaxyStarData star = null;
    public List<GalaxyPlanetData> planets = null;

    public GalaxySolarSystemData(GalaxySolarSystem solarSystem)
    {
        localPosition[0] = solarSystem.transform.localPosition.x;
        localPosition[1] = solarSystem.transform.localPosition.y;
        localPosition[2] = solarSystem.transform.localPosition.z;

        star = new GalaxyStarData(solarSystem.star);

        planets = new List<GalaxyPlanetData>();
        for(int planetIndex = 0; planetIndex < solarSystem.planets.Count; planetIndex++)
            planets.Add(new GalaxyPlanetData(solarSystem.planets[planetIndex]));
    }
}