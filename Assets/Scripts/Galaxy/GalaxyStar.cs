using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyStar : MonoBehaviour
{
    [Header("Particle System Options")]

    [SerializeField, Tooltip("Indicates the lowest percentage of the initial max particles that each particle system of the star can go to in order to optimize for zooming. Value range: (0-1).")] private float minimumParticlePercentage = 0.2f;
    [SerializeField, Tooltip("Indicates the zoom percentage where the stars emit the maximum amount of particles.")] private float maxParticlesZoomPercentage = 0.8f;

    [Header("Particle System Components")]

    [SerializeField, Tooltip("The particle system responsible for emitting the particles that make up the surface of the star.")] private ParticleSystem surfacePS = null;
    [SerializeField, Tooltip("The particle system responsible for emitting the particles that make up the corona of the star.")] private ParticleSystem coronaPS = null;
    [SerializeField, Tooltip("The particle system responsible for emitting the particles that make up the loops of the star.")] private ParticleSystem loopsPS = null;

    [Header("Light Components")]

    [SerializeField, LabelOverride("Star Light"), Tooltip("The light component that emits light out of the star.")] private Light starLightVar = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Private variable that holds the initial maximum amount of particles that can be rendered by the surface particle system of the star.
    /// </summary>
    private int surfacePSInitialMaxParticles = 0;
    /// <summary>
    /// Private variable that holds the initial maximum amount of particles that can be rendered by the corona particle system of the star.
    /// </summary>
    private int coronaPSInitialMaxParticles = 0;
    /// <summary>
    /// Private variable that holds the initial maximum amount of particles that can be rendered by the loops particle system of the star.
    /// </summary>
    private int loopsPSInitialMaxParticles = 0;

    public enum StarType
    {
        RedDwarf,
        RedGiant,
        BlueGiant,
        YellowDwarf
    }

    /// <summary>
    /// Private variable that holds the enum value that indicates what type of star this star is.
    /// </summary>
    private StarType typeVar = 0;
    /// <summary>
    /// Public property that should be used in order to access the type of star this star is.
    /// </summary>
    public StarType type { get => typeVar; }

    /// <summary>
    /// Public property that should be used to access the light component that emits light out of the star.
    /// </summary>
    public Light starLight { get => starLightVar; }

    /// <summary>
    /// Public property that should be used in order to access the local scale of the star.
    /// </summary>
    public Vector3 localScale { get => transform.localScale; }

    private void Awake()
    {
        //Stores the initial max particles of the surface particle system of the star.
        surfacePSInitialMaxParticles = surfacePS.main.maxParticles;
        //Stores the initial max particles of the corona particle system of the star.
        coronaPSInitialMaxParticles = coronaPS.main.maxParticles;
        //Stores the initial max particles of the loops particle system of the star.
        loopsPSInitialMaxParticles = loopsPS.main.maxParticles;

        //Adds the on zoom change function of the star to the list of functions that should be called by the galaxy camera whenever the zoom percentage meaningfully changes.
        NewGalaxyCamera.AddZoomFunction(OnZoomChange);
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
    /// This method should be called in the GenerateSolarSystems method in the galaxy generator and initializes all needed variables in the star.
    /// </summary>
    public void InitializeFromGalaxyGenerator(StarType starType)
    {
        typeVar = starType;
    }

    /// <summary>
    /// This method should be called by the galaxy generator in order to load in all of the data from the star in the galaxy save data into this star.
    /// </summary>
    /// <param name="starData"></param>
    public void InitializeFromSaveData(GalaxyStarData starData)
    {
        //Loads in the local scale from the star save data.
        transform.localScale = new Vector3(starData.localScale[0], starData.localScale[1], starData.localScale[2]);
        //Loads in the star type from the star save data.
        typeVar = starData.starType;
    }

    /// <summary>
    /// This public method should be called whenever the zoom of the galaxy camera is changed and optimizes the star and its particle systems accordingly.
    /// </summary>
    public void OnZoomChange()
    {
        //Updates the max particles of the surface particle system of the star.
        ParticleSystem.MainModule surfacePSMain = surfacePS.main;
        int surfacePSMaxParticles = (int)(surfacePSInitialMaxParticles * (NewGalaxyCamera.zoomPercentage * (1 / maxParticlesZoomPercentage)));
        if (surfacePSMaxParticles < minimumParticlePercentage * surfacePSInitialMaxParticles)
            surfacePSMaxParticles = (int)(minimumParticlePercentage * surfacePSInitialMaxParticles);
        else if (surfacePSMaxParticles > surfacePSInitialMaxParticles)
            surfacePSMaxParticles = surfacePSInitialMaxParticles;
        surfacePSMain.maxParticles = surfacePSMaxParticles;
        //Updates the max particles of the corona particle system of the star.
        ParticleSystem.MainModule coronaPSMain = coronaPS.main;
        int coronaPSMaxParticles = (int)(coronaPSInitialMaxParticles * (NewGalaxyCamera.zoomPercentage * (1 / maxParticlesZoomPercentage)));
        if (coronaPSMaxParticles < minimumParticlePercentage * coronaPSInitialMaxParticles)
            coronaPSMaxParticles = (int)(minimumParticlePercentage * coronaPSInitialMaxParticles);
        else if (coronaPSMaxParticles > coronaPSInitialMaxParticles)
            coronaPSMaxParticles = coronaPSInitialMaxParticles;
        coronaPSMain.maxParticles = coronaPSMaxParticles;
        //Updates the max particles of the loops particle system of the star.
        ParticleSystem.MainModule loopsPSMain = loopsPS.main;
        int loopsPSMaxParticles = (int)(loopsPSInitialMaxParticles * (NewGalaxyCamera.zoomPercentage * (1 / maxParticlesZoomPercentage)));
        if (loopsPSMaxParticles < minimumParticlePercentage * loopsPSInitialMaxParticles)
            loopsPSMaxParticles = (int)(minimumParticlePercentage * loopsPSInitialMaxParticles);
        else if (loopsPSMaxParticles > loopsPSInitialMaxParticles)
            loopsPSMaxParticles = loopsPSInitialMaxParticles;
        loopsPSMain.maxParticles = loopsPSMaxParticles;
    }

    /// <summary>
    /// This function is called upon the star object being destroyed and removes the OnZoomChange function from the list of functions in the galaxy camera that need to executed when the camera's zoom percentage meaningfully changes.
    /// </summary>
    private void OnDestroy()
    {
        NewGalaxyCamera.RemoveZoomFunction(OnZoomChange);
    }
}

[System.Serializable]
public class GalaxyStarData
{
    public float[] localScale = new float[3];
    public GalaxyStar.StarType starType = 0;

    public GalaxyStarData(GalaxyStar star)
    {
        localScale[0] = star.localScale.x;
        localScale[1] = star.localScale.y;
        localScale[2] = star.localScale.z;

        starType = star.type;
    }
}