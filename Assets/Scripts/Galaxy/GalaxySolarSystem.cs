using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    //Non-inspector variables and properties.

    /// <summary>
    /// Private variable that holds a reference to the star that is at the center of this solar system. Should be accessed from the publicly accessible property.
    /// </summary>
    private GalaxyStar starVar = null;
    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

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