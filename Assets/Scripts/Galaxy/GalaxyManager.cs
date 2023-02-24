using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GalaxyManager : GalaxyViewBehaviour
{
    [Header("Galaxy Manager Components")]

    [SerializeField, LabelOverride("Research View")] private GameObject researchViewVar = null;

    [Header("Galaxy Manager SFX Options")]

    [SerializeField] private AudioClip switchToResearchViewSFX = null;
    [SerializeField] private AudioClip techFinishedSFX = null;
    [SerializeField] private AudioClip endTurnSFX = null;

    [Header("Galaxy Manager Settings")]

    [SerializeField, LabelOverride("Ironpill Mode Enabled")] private bool ironpillModeEnabledVar = false;
    [SerializeField, LabelOverride("Observation Mode Enabled")] private bool observationModeEnabledVar = false;

    [Header("Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField, LabelOverride("Turn Number")] private int turnNumberVar = 0;
    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField, LabelOverride("Player ID")] private int playerIDVar = 0;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used to access the cheat console.
    /// </summary>
    public static CheatConsole cheatConsole { get => galaxyManager.cheatConsoleVar; }
    private CheatConsole cheatConsoleVar = null;

    public GameObject researchView { get => researchViewVar; }

    /// <summary>
    /// Public property that should be used both to access and mutate whether ironpill mode (cheats disabled and achievements enabled) is enabled.
    /// </summary>
    public static bool ironpillModeEnabled
    {
        get
        {
            if (galaxyManager == null)
                return false;
            return galaxyManager.ironpillModeEnabledVar;
        }
        set
        {
            if (galaxyManager != null)
                galaxyManager.ironpillModeEnabledVar = value;
        }
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the player's empire ID.
    /// </summary>
    public static int playerID
    {
        get => galaxyManager.playerIDVar;
        set
        {
            //Sets the variable that indicates the empire ID of the player's empire to the specified value.
            galaxyManager.playerIDVar = value;
            //Updates the resource bar to accurately reflect the new empire that the player has switched to.
            ResourceBar.UpdateAllEmpireDependantComponents();
            //Logs with the planet ships that the playerID value has been changed and that the visibility of the planet ships might need to be updated.
            PlanetShip.OnPlayerIDChange();
        }
    }

    /// <summary>
    /// Public property that should be accessed in order to determine what turn the game is on (value set privately upon the player ending their turn).
    /// </summary>
    public static int turnNumber
    {
        get => galaxyManager.turnNumberVar;
        private set
        {
            //Sets the variable that indicates what turn the game is on to the specified value.
            galaxyManager.turnNumberVar = value;
            //Updates the resource bar to accurately reflect what turn the game is on.
            ResourceBar.UpdateTurnText();
        }
    }

    /// <summary>
    /// Public property that should be used to access the list of planets in the galaxy.
    /// </summary>
    public static List<GalaxyPlanet> planets { get => galaxyManager.planetsVar; }
    private List<GalaxyPlanet> planetsVar = null;

    /// <summary>
    /// Public property that should be used both to access and mutate whether or not observation mode is enabled where the player loses control and the bot does everything for them upon the player hitting the end turn button.
    /// </summary>
    public static bool observationModeEnabled { get => galaxyManager.observationModeEnabledVar; set => galaxyManager.observationModeEnabledVar = value; }

    /// <summary>
    /// Public property that should be accessed in order to determine if the galaxy view (excluding research view) is active in the hierarchy of the engine.
    /// </summary>
    public static bool activeInHierarchy
    {
        get
        {
            if (galaxyManager == null)
                return false;
            return galaxyManager.gameObject.activeInHierarchy;
        }
    }

    /// <summary>
    /// Public property that should be used to access the static reference of the galaxy manager instance.
    /// </summary>
    public static GalaxyManager galaxyManager { get => galaxyManagerVar; }
    private static GalaxyManager galaxyManagerVar = null;

    /// <summary>
    /// Public property that should be used to access the materials of each empire according to their culture index.
    /// </summary>
    public static List<Material> empireMaterials { get => galaxyManager.empireMaterialsVar; }
    private List<Material> empireMaterialsVar = new List<Material>() { null, null, null, null, null, null };

    private Dictionary<Empire.Culture, Material[]> pillMaterials = new Dictionary<Empire.Culture, Material[]>();

    /// <summary>
    /// Public property that should be used to access the camera for the galaxy view.
    /// </summary>
    public static Camera galaxyCamera { get => galaxyManager.galaxyCameraVar; }
    private Camera galaxyCameraVar = null;

    /// <summary>
    /// Public property that should be used to access the galaxy view's canvas.
    /// </summary>
    public static Canvas galaxyCanvas { get => galaxyManager.galaxyCanvasVar; }
    private Canvas galaxyCanvasVar = null;

    /// <summary>
    /// Public property that should be used to access the parent of all confirmation popups.
    /// </summary>
    public static Transform confirmationPopupParent { get => galaxyManager.confirmationPopupParentVar; }
    private Transform confirmationPopupParentVar = null;

    /// <summary>
    /// Public property that should be used to access the parent of all regular popups.
    /// </summary>
    public static Transform popupsParent { get => galaxyManager.popupsParentVar; }
    private Transform popupsParentVar = null;

    /// <summary>
    /// Public property that should be used both to access and mutate the material that is used as the galaxy's skybox.
    /// </summary>
    public static Material skyboxMaterial
    {
        get => galaxyManager.skyboxMaterialVar;
        set
        {
            galaxyManager.skyboxMaterialVar = value;
            if (activeInHierarchy)
                RenderSettings.skybox = skyboxMaterial;
        }
    }
    private Material skyboxMaterialVar = null;

    public static void Initialize(List<GalaxyPlanet> planetList, Camera galaxyCam, Canvas canvasOfGalaxy, Transform parentOfGalaxyConfirmationPopup, Transform parentOfPopups, Material skyboxMaterial, List<Material> empireMaterials, CheatConsole cheatConsole)
    {
        galaxyManager.planetsVar = planetList;
        galaxyManager.galaxyCameraVar = galaxyCam;
        galaxyManager.galaxyCanvasVar = canvasOfGalaxy;
        galaxyManager.confirmationPopupParentVar = parentOfGalaxyConfirmationPopup;
        galaxyManager.popupsParentVar = parentOfPopups;
        GalaxyManager.skyboxMaterial = skyboxMaterial;
        galaxyManager.empireMaterialsVar = empireMaterials;
        galaxyManager.cheatConsoleVar = cheatConsole;

        if (NewGameMenu.initialized)
        {
            //Sets whether or not the game has ironman mode enabled.
            ironpillModeEnabled = NewGameMenu.IronmanModeEnabled;
        }

        //Loads in all of the materials that will be applied to pills in pill views.
        if(galaxyManager.pillMaterials == null || galaxyManager.pillMaterials.Keys.Count == 0)
            galaxyManager.LoadInPillMaterials();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (!GalaxyGameSettings.loaded)
            GalaxyGameSettings.LoadSettings();
    }

    protected override void Awake()
    {
        base.Awake();

        //Assigns the static reference of galaxy manager.
        galaxyManagerVar = this;
    }

    //This method loads in all of the materials that will be applied to pills in pill views.
    private void LoadInPillMaterials()
    {
        for (int cultureIndex = 0; cultureIndex < Enum.GetNames(typeof(Empire.Culture)).Length; cultureIndex++)
        {
            pillMaterials.Add((Empire.Culture)cultureIndex, Resources.LoadAll<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(((Empire.Culture)cultureIndex).ToString())));
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //Resets the boolean that indicates if a popup was closed on the frame.
        ResetPopupClosedOnFrame();

        //Resets the boolean that indicates if the settings menu was closed on the current frame.
        GalaxySettingsMenu.ResetClosedOnFrameBool();

        //Toggles the cheat console if the player presses tilde.
        if (Input.GetKeyDown(KeyCode.BackQuote) && !GalaxyConfirmationPopupBehaviour.isAConfirmationPopupOpen && !GalaxyPauseMenu.isOpen)
        {
            if(!ironpillModeEnabled)
                cheatConsoleVar.ToggleConsole();
        }

        //Opens the pause menu if the player presses the escape key and no other popup is open.
        if(Input.GetKeyDown(KeyCode.Escape) && !GalaxyConfirmationPopupBehaviour.isAConfirmationPopupOpen && !GalaxyPopupBehaviour.IsAPopupActiveInHierarchy && !GalaxyPauseMenu.isOpen)
        {
            GalaxyPauseMenu.Open();
        }
    }

    public void WarningRightSideNotificationsUpdate()
    {
        //No research selected warning.
        if(Empire.empires[playerID].techManager.techTotemSelected < 0)
        {
            if (!RightSideNotificationManager.NotificationExistsOfTopic("No Research Selected") && !RightSideNotificationManager.NotificationExistsOfTopic("Research Completed"))
                RightSideNotificationManager.CreateNewWarningRightSideNotification("Science Icon", "No Research Selected", WarningRightSideNotificationClickEffect.OpenResearchView);
        }
        else
        {
            RightSideNotificationManager.DismissNotificationsOfTopic("No Research Selected");
        }
    }

    public void SwitchToResearchView()
    {
        //Closes the planet management menu if it is open.
        if (PlanetManagementMenu.planetManagementMenu.gameObject.activeInHierarchy)
            PlanetManagementMenu.planetManagementMenu.Close();

        //Turns off the galaxy view.
        transform.gameObject.SetActive(false);
        //Turns on the research view.
        researchViewVar.SetActive(true);
        //Switches the skybox material to the one assigned to the research view.
        RenderSettings.skybox = researchViewVar.GetComponent<ResearchViewManager>().skyboxMaterial;

        //Plays the switch to research view sound effect.
        AudioManager.PlaySFX(switchToResearchViewSFX);
    }

    public void EndTurn()
    {
        if(!RightSideNotificationManager.ContainsNotificationWithAnswerRequired() && !GalaxyPopupManager.ContainsNonDismissablePopup())
        {
            //Dismisses all right side notifications that still exist and do not require an answer.
            RightSideNotificationManager.DismissAllNotifications(false);
            //Closes all popups that still exist and do not require an answer.
            GalaxyPopupManager.CloseAllPopups();

            //Plays the end turn sound effect.
            AudioManager.PlaySFX(endTurnSFX);

            //Closes the planet management menu if it is currently open.
            if (PlanetManagementMenu.planetManagementMenu.gameObject.activeInHierarchy)
                PlanetManagementMenu.planetManagementMenu.Close();

            //Closes all army management menus.
            ArmyManagementMenu.CloseAll();

            //Everyone makes their moves for the turn.
            for (int x = 0; x < Empire.empires.Count; x++)
            {
                if (x != playerID || observationModeEnabled)
                    Empire.empires[x].PlayAI();
            }

            //Stuff is calculated and added after everyone's turn.
            foreach (Empire empire in Empire.empires)
            {
                empire.EndTurn();
            }

            WarningRightSideNotificationsUpdate();

            //Logs that a turn has been completed.
            turnNumber++;
        }
    }

    public void PlayTechFinishedSFX()
    {
        AudioManager.PlaySFX(techFinishedSFX);
    }

    /// <summary>
    /// This method is called upon the galaxy manager being destroyed which effectively means that the scene has changed and resets all needed static variables.
    /// </summary>
    private void OnDestroy()
    {
        galaxyManagerVar = null;
    }
}