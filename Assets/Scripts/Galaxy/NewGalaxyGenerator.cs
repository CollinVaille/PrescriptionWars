using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyGenerator : MonoBehaviour
{
    [Header("Material Options")]

    [SerializeField, Tooltip("The material that will be applied to the skybox while the galaxy scene is open and the galaxy is active in the project hierarchy.")] private Material skyboxMaterial = null;

    [Header("New Game Data")]

    [SerializeField, Tooltip("Holds either the new game data passed from the new game menu or the new game data selected through the inspector.")] private NewGameData newGameData = new NewGameData();
    [SerializeField] private int minimumSpaceBetweenStars = 0;

    [Header("Parents")]

    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the solar systems in the galaxy. Specified through the inspector.")] private Transform solarSystemsParent = null;

    [Header("Prefabs")]

    [SerializeField, Tooltip("The prefab that all solar systems in the galaxy will be instanitated from. Specified through the inspector.")] private GameObject solarSystemPrefab = null;

    //Non-inspector variables.

    /// <summary>
    /// Holds all of the solar systems that have already been fully generated and placed appropriately into the galaxy.
    /// </summary>
    private List<GalaxySolarSystem> solarSystems = null;

    /// <summary>
    /// Indicates the default amount of solar systems in the galaxy if the galaxy shape does not specify an amount.
    /// </summary>
    public static int defaultSolarSystemCount { get => 10; }

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //Loads in any new game data that might be coming from the new game menu.
        LoadInNewGameData();

        //Generates the systems of the galaxy.
        GenerateSolarSystems();

        //Sets the material of the galaxy scene's skybox.
        RenderSettings.skybox = skyboxMaterial;

        //Initializes the galaxy manager.
        NewGalaxyManager.InitializeFromGalaxyGenerator(gameObject.GetComponent<NewGalaxyManager>(), skyboxMaterial);

        //Destroys the galaxy generator script after galaxy generation completion.
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads in any new game data that might be passed over staticly from the new game menu.
    /// </summary>
    private void LoadInNewGameData()
    {
        //Sets the local new game data to the new game data passed over staticly from the new game menu if it exists.
        newGameData = NewGameMenu.newGameData != null ? NewGameMenu.newGameData : newGameData;
        //Resets the static new game data variable back to null since the data has already been grabbed.
        NewGameMenu.newGameData = null;
    }

    /// <summary>
    /// Private method that should be called in the start method and generates the systems of the galaxy.
    /// </summary>
    private void GenerateSolarSystems()
    {
        //Initializes the list of solar systems.
        solarSystems = new List<GalaxySolarSystem>();

        //Loads in the sprite from resources that contains the shape that specifies where stars can and cannot be placed.
        Sprite galaxyShapeSprite = Resources.Load<Sprite>("Galaxy/Galaxy Shapes/" + newGameData.galaxyShape);
        //Loops that creates all of the solar systems in the galaxy.
        for(int solarSystemIndex = 0; solarSystemIndex < newGameData.solarSystemCount; solarSystemIndex++)
        {
            //Creates a new solar system by instianiating from the solar system prefab.
            GameObject solarSystem = Instantiate(solarSystemPrefab);
            solarSystem.transform.SetParent(solarSystemsParent);
            solarSystem.transform.localPosition = Vector3.zero;
            solarSystem.transform.localScale = new Vector3(50, 50, 50);
            while (!solarSystem.GetComponent<TestSphere>().isFullyWithinImage)
            {
                solarSystem.transform.localPosition = new Vector3(Random.Range(-1920, 1921), solarSystem.transform.localPosition.y, Random.Range(-1080, 1081));
            }
            //solarSystems.Add(solarSystem);
        }
    }

    /// <summary>
    /// Public static method that should be used in order to determine the max amount of planets able to be fit into the specified galaxy shape.
    /// </summary>
    /// <param name="galaxyShape"></param>
    /// <returns></returns>
    public static int GetMaxPlanetsCount(string galaxyShape)
    {
        if (galaxyShape.Equals("Spiral"))
            return 60;
        Debug.LogWarning("Specified galaxy type does not have an assigned max planets count. Will return default value of 60.");
        return 60;
    }
}