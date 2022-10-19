using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The particle system that creates the space dust clouds that encompass the solar system.")] private ParticleSystem spaceDustPS = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Private variable that holds a reference to the star that is at the center of this solar system. Should be accessed from the publicly accessible property.
    /// </summary>
    private GalaxyStar starVar = null;
    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

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
    public void InitializeFromGalaxyGenerator(GalaxyStar star)
    {
        //Initializes the star variable.
        starVar = star;
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

    public GalaxySolarSystemData(GalaxySolarSystem solarSystem)
    {
        localPosition[0] = solarSystem.transform.localPosition.x;
        localPosition[1] = solarSystem.transform.localPosition.y;
        localPosition[2] = solarSystem.transform.localPosition.z;

        star = new GalaxyStarData(solarSystem.star);
    }
}