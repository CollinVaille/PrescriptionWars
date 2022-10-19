using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyGenerator : MonoBehaviour
{
    [Header("Material Options")]

    [SerializeField, Tooltip("The material that will be applied to the skybox while the galaxy scene is open and the galaxy is active in the project hierarchy.")] private Material skyboxMaterial = null;

    [Header("New Game Data")]

    [SerializeField, Tooltip("Holds either the new game data passed from the new game menu or the new game data selected through the inspector.")] private NewGameData newGameData = new NewGameData();
    [SerializeField, Tooltip("The minimum amount of space that must exist between all stars in the galaxy.")] private int minimumSpaceBetweenStars = 0;
    [SerializeField, Tooltip("Array that contains the probability for each star type to be assigned to a star. The enum index of the star type correlates to the same index in this array.")] private float[] starTypeSpawnProbabilities = new float[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];

    [Header("Parents")]

    [SerializeField, Tooltip("The transform of the game object that acts as the parent of all of the solar systems in the galaxy. Specified through the inspector.")] private Transform solarSystemsParent = null;

    [Header("Prefabs")]

    [SerializeField, Tooltip("The prefab that all solar systems in the galaxy will be instanitated from. Specified through the inspector.")] private GameObject solarSystemPrefab = null;
    [SerializeField, Tooltip("Array that contains the prefab for each star type where the enum index of the star type correlates to the same index in this array.")] private GameObject[] starTypePrefabs = new GameObject[Enum.GetNames(typeof(GalaxyStar.StarType)).Length];

    //Non-inspector variables.

    /// <summary>
    /// Indicates how many failed attempts can be made to place a solar system within the galaxy until any attempts to place any more solar systems will be given up.
    /// </summary>
    private int maxFailedSolarSystemPlacementAttemps = 1000;

    /// <summary>
    /// Holds all of the solar systems that have already been fully generated and placed appropriately into the galaxy.
    /// </summary>
    private List<GalaxySolarSystem> solarSystems = null;

    /// <summary>
    /// Private variable that holds any save game data that might be passed over statically from the load game menu.
    /// </summary>
    private GalaxyData saveGameData = null;

    /// <summary>
    /// Indicates the default amount of solar systems in the galaxy if the galaxy shape does not specify an amount.
    /// </summary>
    public static int defaultSolarSystemCount { get => 60; }

    private void Awake()
    {
        //LoadGameMenu.saveGameData = GalaxySaveSystem.LoadGalaxy("Test Save");
    }

    // Start is called before the first frame update
    void Start()
    {
        //Loads in any galaxy save game data that might be coming from the load game menu.
        LoadInSaveGameData();

        //Loads in any new game data that might be coming from the new game menu.
        LoadInNewGameData();

        //Generates the systems of the galaxy.
        GenerateSolarSystems();

        //Sets the material of the galaxy scene's skybox.
        RenderSettings.skybox = skyboxMaterial;

        //Initializes the galaxy manager.
        NewGalaxyManager.InitializeFromGalaxyGenerator(gameObject.GetComponent<NewGalaxyManager>(), skyboxMaterial, solarSystems);

        //NewGalaxyManager.saveName = "Test Save";
        //GalaxySaveSystem.SaveGalaxy();
        //Debug.Log("Saved");

        //Destroys the galaxy generator script after galaxy generation completion.
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads in any galaxy save game data that might be passes over statically from the load game menu.
    /// </summary>
    private void LoadInSaveGameData()
    {
        //Sets the local galaxy save game data to the galaxy save game data passed over statically from the load game menu.
        saveGameData = LoadGameMenu.saveGameData;
        //Resets the static galaxy save game data variable of the laod game menu back to null since the data has already been grabbed.
        LoadGameMenu.saveGameData = null;
    }

    /// <summary>
    /// Loads in any new game data that might be passed over statically from the new game menu.
    /// </summary>
    private void LoadInNewGameData()
    {
        //Sets the local new game data to the new game data passed over statically from the new game menu if it exists.
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
        //Generates the solar systems of the galaxy from the galaxy save game data that has been loaded in from the load game menu if it exists.
        if (saveGameData != null)
        {
            //Loops through each solar system that was stored in the galaxy save game data and creates it.
            for(int solarSystemIndex = 0; solarSystemIndex < saveGameData.solarSystems.Count; solarSystemIndex++)
            {
                //Creates a new solar system by instantiating from the solar system prefab.
                GalaxySolarSystem solarSystem = Instantiate(solarSystemPrefab).GetComponent<GalaxySolarSystem>();
                //Sets the solar system's parent to the parent transform of all solar systems.
                solarSystem.transform.SetParent(solarSystemsParent);

                //Instantiates a star using the star data of the current solar system in the galaxy save data loaded in.
                GalaxyStar star = Instantiate(starTypePrefabs[(int)saveGameData.solarSystems[solarSystemIndex].star.starType]).GetComponent<GalaxyStar>();
                star.transform.SetParent(solarSystem.transform);
                star.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex].star);

                //Initializes the solar system using the saved data of the solar system and the star that was just instantiated from the same save data.
                solarSystem.InitializeFromSaveData(saveGameData.solarSystems[solarSystemIndex], star);

                //Adds the solar system to the list of solar systems within the galaxy.
                solarSystems.Add(solarSystem);
            }
        }
        //Generates the solar systems of the galaxy from the new game data that has either been loaded in from the new game menu or specified through the inspector.
        else
        {
            //Fills a list with as many star type enum values as there will be stars in the galaxy using the probabilities specified through the inspector.
            List<GalaxyStar.StarType> starTypesRemaining = new List<GalaxyStar.StarType>();
            for (int starTypeIndex = 0; starTypeIndex < starTypeSpawnProbabilities.Length; starTypeIndex++)
            {
                int numberOfStarsOfType = (int)(newGameData.solarSystemCount * starTypeSpawnProbabilities[starTypeIndex]);
                for (int amountAdded = 0; amountAdded < numberOfStarsOfType; amountAdded++)
                {
                    starTypesRemaining.Add((GalaxyStar.StarType)starTypeIndex);
                }
            }
            while (starTypesRemaining.Count < newGameData.solarSystemCount)
            {
                starTypesRemaining.Add(0);
            }

            //Loads in the sprite from resources that contains the shape that specifies where stars can and cannot be placed.
            Sprite galaxyShapeSprite = Resources.Load<Sprite>("Galaxy/Galaxy Shapes/" + newGameData.galaxyShape);
            //Loops that creates all of the solar systems in the galaxy.
            for (int solarSystemIndex = 0; solarSystemIndex < newGameData.solarSystemCount; solarSystemIndex++)
            {
                //Creates a new solar system by instianiating from the solar system prefab.
                GalaxySolarSystem solarSystem = Instantiate(solarSystemPrefab).GetComponent<GalaxySolarSystem>();
                //Sets the solar system's parent to the parent transform of all solar systems.
                solarSystem.transform.SetParent(solarSystemsParent);
                //Resets the position of the solar system.
                solarSystem.transform.localPosition = Vector3.zero;

                //Instantiates a star from a prefab based on a randomly picked star type from a list of star types that was filled up based on probablities specified in the inspector.
                int starTypeIndexInList = UnityEngine.Random.Range(0, starTypesRemaining.Count);
                GalaxyStar star = Instantiate(starTypePrefabs[(int)starTypesRemaining[starTypeIndexInList]]).GetComponent<GalaxyStar>();
                star.transform.SetParent(solarSystem.transform);
                star.InitializeFromGalaxyGenerator(starTypesRemaining[starTypeIndexInList]);
                star.transform.localPosition = Vector3.zero;
                starTypesRemaining.RemoveAt(starTypeIndexInList);

                //Initializes the variables in the solar system.
                solarSystem.InitializeFromGalaxyGenerator(star);

                //Declares and initializes a variable that will be used in order to ensure that not too many failed attempts are made to place the solar system within the galaxy.
                int solarSystemPlacementAttempsMade = 0;
                //Loops until either the solar system has been successfully placed or the maximum amount of attempts is reached.
                while (true)
                {
                    //Checks if the maximum amount of attempts allowed to fail validly positioning the solar system within the galaxy has been reached and stops attempting to place the solar system if so.
                    if (solarSystemPlacementAttempsMade >= maxFailedSolarSystemPlacementAttemps)
                        break;
                    //Assigns the solar system a random position where it still appears on the screen.
                    solarSystem.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1920 + (solarSystem.star.localScale.x / 2), 1921 - (solarSystem.star.localScale.x / 2)), solarSystem.transform.localPosition.y, UnityEngine.Random.Range(-1080 + (solarSystem.star.localScale.z / 2), 1081 - (solarSystem.star.localScale.z / 2)));
                    //Array of coordinates that will need to be checked in order to ensure that they are all within the non-transparent parts of the galaxy shape image.
                    Vector2Int[] coordinatesToCheck = new Vector2Int[4];
                    //Left coordinate check.
                    coordinatesToCheck[0] = new Vector2Int((((int)solarSystem.transform.localPosition.x + 1920) - (int)(solarSystem.star.localScale.x / 2)), (int)solarSystem.transform.localPosition.z + 1080);
                    //Upper coordinate check.
                    coordinatesToCheck[1] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920), (((int)solarSystem.transform.localPosition.z) + 1080) + (int)(solarSystem.star.localScale.z / 2));
                    //Right coordinate check.
                    coordinatesToCheck[2] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920) + (int)(solarSystem.star.localScale.x / 2), (int)solarSystem.transform.localPosition.z + 1080);
                    //Lower coordinate check.
                    coordinatesToCheck[3] = new Vector2Int(((int)solarSystem.transform.localPosition.x + 1920), (((int)solarSystem.transform.localPosition.z) + 1080) - (int)(solarSystem.star.localScale.z / 2));
                    //Loops through the left, upper, right, and lower coordinates to make sure that they are at coordinates that are on the screen and not transparent in the galaxy shape image.
                    bool validCoordinates = true;
                    for (int index = 0; index < coordinatesToCheck.Length; index++)
                    {
                        if (coordinatesToCheck[index].x < 0 || coordinatesToCheck[index].x >= 3840 || coordinatesToCheck[index].y < 0 || coordinatesToCheck[index].y >= 2160 || galaxyShapeSprite.texture.GetPixel(coordinatesToCheck[index].x, coordinatesToCheck[index].y).a == 0)
                        {
                            validCoordinates = false;
                            break;
                        }
                    }
                    //Loops through each solar system that has already been validly positioned and ensures that the new position of the new solar is not too close to it.
                    for (int alreadyPositionedSolarSystemIndex = 0; alreadyPositionedSolarSystemIndex < solarSystems.Count; alreadyPositionedSolarSystemIndex++)
                    {
                        if (solarSystem.transform.localPosition.x + (solarSystem.star.localScale.x / 2) + minimumSpaceBetweenStars >= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.x - (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.x / 2) && solarSystem.transform.localPosition.x - (solarSystem.star.localScale.x / 2) - minimumSpaceBetweenStars <= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.x + (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.x / 2) && solarSystem.transform.localPosition.z + (solarSystem.star.localScale.z / 2) + minimumSpaceBetweenStars >= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.z - (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.z / 2) && solarSystem.transform.localPosition.z - (solarSystem.star.localScale.z / 2) - minimumSpaceBetweenStars <= solarSystems[alreadyPositionedSolarSystemIndex].transform.localPosition.z + (solarSystems[alreadyPositionedSolarSystemIndex].star.localScale.z / 2))
                        {
                            validCoordinates = false;
                            break;
                        }
                    }
                    //If the coordinates are valid, the solar system is added to the list of solar system in the galaxy and we move on to positioning the next solar system.
                    if (validCoordinates)
                    {
                        solarSystems.Add(solarSystem);
                        break;
                    }
                    //Increments the variable that indicates how many attempts have been made to position the current solar system validly.
                    solarSystemPlacementAttempsMade++;
                }
                //Checks if the maximum amount of attempts allowed to fail validly positioning the solar system within the galaxy has been reached and stops attempting to place any more solar systems if so. Also, a debug warning is put into the console.
                if (solarSystemPlacementAttempsMade >= maxFailedSolarSystemPlacementAttemps)
                {
                    Debug.LogWarning("Maximum amount of allowed attempts to fail to validly position a solar system reached. Will stop adding any more solar systems to the galaxy.");
                    break;
                }
                //Adds the solar system just worked on to the list of all solar systems within the galaxy.
                solarSystems.Add(solarSystem);
            }
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