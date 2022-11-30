using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyManager :  GalaxyViewBehaviour
{
    /// <summary>
    /// Private holder variable of the material that should be applied to the galaxy skybox.
    /// </summary>
    private Material skyboxMaterialVar = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the skybox material of the galaxy scene.
    /// </summary>
    public static Material skyboxMaterial
    {
        get => galaxyManager.skyboxMaterialVar;
        set
        {
            if (RenderSettings.skybox == galaxyManager.skyboxMaterialVar)
                RenderSettings.skybox = value;
            galaxyManager.skyboxMaterialVar = value;
        }
    }

    /// <summary>
    /// Private holder variable that contains the name of the current save.
    /// </summary>
    private string saveNameVar;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the current save.
    /// </summary>
    public static string saveName { get => galaxyManager.saveNameVar; set => galaxyManager.saveNameVar = value; }

    /// <summary>
    /// Private holder variable for the list of solar systems that are in the galaxy.
    /// </summary>
    private List<GalaxySolarSystem> solarSystemsVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains the solar systems existing within the galaxy.
    /// </summary>
    public static List<GalaxySolarSystem> solarSystems { get => galaxyManager.solarSystemsVar; }

    /// <summary>
    /// Private holder variable for the list of planets that are in the galaxy.
    /// </summary>
    private List<NewGalaxyPlanet> planetsVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains the planets existing within the galaxy.
    /// </summary>
    public static List<NewGalaxyPlanet> planets { get => galaxyManager.planetsVar; }

    /// <summary>
    /// Private holder variable for the list of empires that exist within the galaxy.
    /// </summary>
    private List<NewEmpire> empiresVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains the empires existing within the galaxy.
    /// </summary>
    public static List<NewEmpire> empires { get => galaxyManager.empiresVar; }

    /// <summary>
    /// Private holder variable of a galaxy manager instance.
    /// </summary>
    private static NewGalaxyManager galaxyManagerVar = null;
    /// <summary>
    /// Publicly accessible property that returns an instance of a galaxy manager.
    /// </summary>
    public static NewGalaxyManager galaxyManager { get => galaxyManagerVar; }
    /// <summary>
    /// Publicly accessible property that returns a boolean that indicates whether or not the player is inside of the galaxy scene and the galaxy manager static instance has been initialized.
    /// </summary>
    public static bool sceneActive { get => galaxyManager != null; }

    /// <summary>
    /// Publicly accessible property that indicates whether the game object that the galaxy manager script is attached to is active in the hierarchy. In other words, it indicates whether the visible galaxy itself is visible in the hierarchy.
    /// </summary>
    public static bool activeInHierarchy { get => galaxyManager != null && galaxyManager.gameObject.activeInHierarchy; }

    /// <summary>
    /// Private holder variable that indicates the name of the shape that the galaxy was made from.
    /// </summary>
    private string galaxyShapeVar = null;
    /// <summary>
    /// Public static property that should be used to access the name of the shape that the galaxy was generated to fit.
    /// </summary>
    public static string galaxyShape { get => galaxyManager == null ? null : galaxyManager.galaxyShapeVar; }

    /// <summary>
    /// Private holder variable for the player's empire ID (index in the list of empires within the galaxy).
    /// </summary>
    private int playerIDVar = -1;
    /// <summary>
    /// Public static property that should be used to access the player's empire ID (index in the list of empires within the galaxy).
    /// </summary>
    public static int playerID { get => galaxyManager == null ? -1 : galaxyManager.playerIDVar; }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all planet labels within the galaxy.
    /// </summary>
    private Transform planetLabelsParentVar = null;
    /// <summary>
    /// Public static property that should be used to access the transform of the game object that serves as the parent object for all planet labels within the galaxy.
    /// </summary>
    public static Transform planetLabelsParent { get => galaxyManager == null ? null : galaxyManager.planetLabelsParentVar; }

    /// <summary>
    /// Public static method that should be called by the galaxy generator at the end of the start method in order to initialize all of the needed variables within the galaxy manager.
    /// </summary>
    public static void InitializeFromGalaxyGenerator(NewGalaxyManager galaxyManager, Material skyboxMaterial, List<GalaxySolarSystem> solarSystems, List<NewGalaxyPlanet> planets, List<NewEmpire> empires, string galaxyShape, int playerID, List<Transform> parents)
    {
        //Sets the static instance of the galaxy manager.
        galaxyManagerVar = galaxyManager;

        //Sets the value of the variable that holds the skybox material of the galaxy.
        galaxyManager.skyboxMaterialVar = skyboxMaterial;

        //Sets the value of the variable that contains all of the solar systems existing within the galaxy.
        galaxyManager.solarSystemsVar = solarSystems;

        //Sets the value of the variable that contains all of the planets existing within the galaxy.
        galaxyManager.planetsVar = planets;

        //Sets the value of the variable that contains all of the empires existing within the galaxy.
        galaxyManager.empiresVar = empires;

        //Sets the value of the variable that contains the name of the shape that the galaxy was generated to fit.
        galaxyManager.galaxyShapeVar = galaxyShape;

        //Sets the value of the variable that contains the player's empire ID (index in the list of empires within the galaxy).
        galaxyManager.playerIDVar = playerID;

        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all planet labels within the galaxy.
        galaxyManager.planetLabelsParentVar = parents[0];
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// This method should be called whenever the galaxy game object that the galaxy manager script is attached to is destroyed, which effectively means that the scene has been changed.
    /// </summary>
    private void OnDestroy()
    {
        //Resets the static galaxy manager instance variable to null.
        galaxyManagerVar = null;
    }
}
