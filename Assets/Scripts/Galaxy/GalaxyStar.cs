using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Transform Components")]

    [SerializeField, Tooltip("The transform that marks the location at which the star label should be placed.")] private Transform starLabelLocation = null;

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

    /// <summary>
    /// Public property that should be used both to access and mutate the name of the star.
    /// </summary>
    public string starName { get => starNameVar; set { starNameVar = value; name = value + " Star"; if(solarSystem != null) solarSystem.name = value + " Solar System"; if (starNameLabel != null) { starNameLabel.text = value; starNameLabel.gameObject.name = value + " Label"; } } }
    /// <summary>
    /// Private holder variable for the name of the star.
    /// </summary>
    private string starNameVar = null;

    /// <summary>
    /// Private holder variable for the text component that acts as the name label for the star so that the name of the star is visible to the player in game.
    /// </summary>
    private Text starNameLabel = null;

    /// <summary>
    /// Private holder variable for the solar system that the star belongs to in the galaxy.
    /// </summary>
    private GalaxySolarSystem solarSystemVar = null;
    /// <summary>
    /// Public property that should be used to access the solar system that the star belongs to.
    /// </summary>
    public GalaxySolarSystem solarSystem { get => solarSystemVar; }

    private void Awake()
    {
        //Stores the initial max particles of the surface particle system of the star.
        surfacePSInitialMaxParticles = surfacePS.main.maxParticles;
        //Stores the initial max particles of the corona particle system of the star.
        coronaPSInitialMaxParticles = coronaPS.main.maxParticles;
        //Stores the initial max particles of the loops particle system of the star.
        loopsPSInitialMaxParticles = loopsPS.main.maxParticles;

        //Adds the on zoom change function of the star to the list of functions that should be called by the galaxy camera whenever the zoom percentage meaningfully changes.
        NewGalaxyCamera.AddZoomFunction(OnZoomPercentageChange);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Updates the planet name label's position.
        starNameLabel.transform.position = Camera.main.WorldToScreenPoint(starLabelLocation.transform.position);
        starNameLabel.transform.localPosition = (Vector2)starNameLabel.transform.localPosition;
    }

    /// <summary>
    /// Private method that should be universally (aka regardless of whether starting a new game or loading an old game) called in order to initialize all needed data members of the star.
    /// </summary>
    private void Initialize(GalaxySolarSystem solarSystem, StarType starType, string starName)
    {
        //Initializes all needed data members.
        solarSystemVar = solarSystem;
        solarSystemVar.AddOnBecameVisibleFunction(OnSolarSystemBecameVisible);
        solarSystemVar.AddOnBecameInvisibleFunction(OnSolarSystemBecameInvisible);
        typeVar = starType;
        this.starName = starName;

        //Adds the OnGalaxyGenerationCompletion function to the list of functions to be executed once the galaxy has completely finished generating.
        NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 1);
    }

    /// <summary>
    /// This method should be called in the GenerateSolarSystems method in the galaxy generator and initializes all needed variables in the star.
    /// </summary>
    public void InitializeFromGalaxyGenerator(GalaxySolarSystem solarSystem, StarType starType, string starName)
    {
        //Calls the universal initialize method and passes in the needed data.
        Initialize(solarSystem, starType, starName);
    }

    /// <summary>
    /// This method should be called by the galaxy generator in order to load in all of the data from the star in the galaxy save data into this star.
    /// </summary>
    /// <param name="starData"></param>
    public void InitializeFromSaveData(GalaxySolarSystem solarSystem, GalaxyStarData starData)
    {
        //Loads in the local scale from the star save data.
        transform.localScale = new Vector3(starData.localScale[0], starData.localScale[1], starData.localScale[2]);

        //Calls the universal initialize method and passes in the needed data.
        Initialize(solarSystem, starData.starType, starData.starName);

        //Ensures the star name is positioned directly underneath the star.
        starLabelLocation.parent.localRotation = Quaternion.Euler(starLabelLocation.parent.localRotation.eulerAngles.x, -transform.rotation.eulerAngles.y, starLabelLocation.parent.localRotation.eulerAngles.z);
    }

    /// <summary>
    /// This function should be added to the list of functions to be executed once the galaxy has fully finished generating.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        //Instantiates the star's name label.
        starNameLabel = Instantiate(Resources.Load<GameObject>("Galaxy/Prefabs/Star Label")).GetComponent<Text>();
        starNameLabel.transform.SetParent(NewGalaxyManager.starLabelsParent);
        starNameLabel.transform.localScale = Vector3.one;
        starNameLabel.text = starName;
        starNameLabel.color = solarSystemVar.owner == null ? Color.white : solarSystemVar.owner.labelColor;
        starNameLabel.gameObject.name = starName + " Label";
        starNameLabel.transform.position = Camera.main.WorldToScreenPoint(starLabelLocation.transform.position);
        starNameLabel.transform.localPosition = (Vector2)starNameLabel.transform.localPosition;

        //Executes the OnZoomPercentageChange function at the start in order to ensure the planet is adapted to the galaxy camera's initial zoom percentage.
        OnZoomPercentageChange();
    }

    /// <summary>
    /// This public method should be called whenever the zoom of the galaxy camera is changed and optimizes the star and its particle systems accordingly.
    /// </summary>
    private void OnZoomPercentageChange()
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

        //Updates the activation state of the star's name label.
        if (starNameLabel != null && starNameLabel.gameObject.activeSelf != (solarSystemVar.visible && NewGalaxyCamera.starNameLabelsVisible))
        {
            starNameLabel.gameObject.SetActive(solarSystemVar.visible && NewGalaxyCamera.starNameLabelsVisible);
        }
    }

    /// <summary>
    /// Public method that should be called by the solar system in its EndTurnUpdate method and ensures the star name is positioned directly underneath the star at all times during the end turn process.
    /// </summary>
    public void EndTurnUpdate()
    {
        //Ensures the star name is positioned directly underneath the star.
        starLabelLocation.parent.localRotation = Quaternion.Euler(starLabelLocation.parent.localRotation.eulerAngles.x, -transform.rotation.eulerAngles.y, starLabelLocation.parent.localRotation.eulerAngles.z);
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
    /// This function is called upon the star object being destroyed and removes the OnZoomChange function from the list of functions in the galaxy camera that need to executed when the camera's zoom percentage meaningfully changes.
    /// </summary>
    private void OnDestroy()
    {
        NewGalaxyCamera.RemoveZoomFunction(OnZoomPercentageChange);
    }
}

[System.Serializable]
public class GalaxyStarData
{
    public float[] localScale = new float[3];
    public GalaxyStar.StarType starType = 0;
    public string starName = null;

    public GalaxyStarData(GalaxyStar star)
    {
        localScale[0] = star.localScale.x;
        localScale[1] = star.localScale.y;
        localScale[2] = star.localScale.z;

        starType = star.type;

        starName = star.starName;
    }
}