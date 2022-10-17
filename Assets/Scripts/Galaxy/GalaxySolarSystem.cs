using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, LabelOverride("Star"), Tooltip("The script component of the star that is at the center of the solar system. Specified through the inspector.")] private GalaxyStar starVar = null;
    [SerializeField, Tooltip("The particle sytstem that is responsible for creating space dust around the star of the galaxy.")] private ParticleSystem spaceDustPS = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

    /// <summary>
    /// Private variable that holds the maximum amount of particles for the space dust particle system at full zoom. Set in the start method.
    /// </summary>
    private int spaceDustInitialMaxParticles;

    // Start is called before the first frame update
    void Start()
    {
        //Stores the initial max particles of the space dust particle system.
        spaceDustInitialMaxParticles = spaceDustPS.main.maxParticles;
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateSpaceDustMaxParticles();
    }

    /// <summary>
    /// This private method should be called in order to update the max particles of the space dust particle system according to the galaxy camera's zoom percentage.
    /// </summary>
    private void UpdateSpaceDustMaxParticles()
    {
        ParticleSystem.MainModule main = spaceDustPS.main;
        main.maxParticles = (int)(spaceDustInitialMaxParticles * NewGalaxyCamera.zoomPercentage);
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