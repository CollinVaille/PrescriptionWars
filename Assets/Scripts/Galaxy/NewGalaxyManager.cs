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
    private static List<GalaxySolarSystem> solarSystemsVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains of the solar systems existing within the galaxy.
    /// </summary>
    public static List<GalaxySolarSystem> solarSystems { get => solarSystemsVar; }

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
    /// Public static method that should be called by the galaxy generator at the end of the start method in order to initialize all of the needed variables within the galaxy manager.
    /// </summary>
    public static void InitializeFromGalaxyGenerator(NewGalaxyManager galaxyManager, Material skyboxMaterial, List<GalaxySolarSystem> solarSystems)
    {
        //Sets the static instance of the galaxy manager.
        galaxyManagerVar = galaxyManager;

        //Sets the value of the variable that holds the skybox material of the galaxy.
        galaxyManager.skyboxMaterialVar = skyboxMaterial;

        //Sets the value of the variable that contains all of the solar systems existing within the galaxy.
        solarSystemsVar = solarSystems;
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
