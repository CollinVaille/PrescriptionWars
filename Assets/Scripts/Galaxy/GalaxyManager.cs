using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GalaxyManager : GalaxyViewBehaviour
{
    [Header("Galaxy Manager Components")]

    [SerializeField, LabelOverride("Cheat Console")] private CheatConsole cheatConsoleVar = null;
    [SerializeField, LabelOverride("Research View")] private GameObject researchViewVar = null;

    [Header("Galaxy Manager SFX Options")]

    [SerializeField] private AudioClip switchToResearchViewSFX = null;
    [SerializeField] private AudioClip techFinishedSFX = null;
    [SerializeField] private AudioClip endTurnSFX = null;

    [Header("Galaxy Manager Settings")]

    [SerializeField] private bool ironpillModeEnabledVar = false;

    //Non-inspector variables.

    public CheatConsole cheatConsole { get => cheatConsoleVar; }

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

    //Indicates the empire ID of the player's empire.
    private static int playerIDVar = 0;
    public static int playerID
    {
        get => playerIDVar;
        set
        {
            //Sets the variable that indicates the empire ID of the player's empire to the specified value.
            playerIDVar = value;
            //Updates the resource bar to accurately reflect the new empire that the player has switched to.
            ResourceBar.UpdateAllEmpireDependantComponents();
            //Logs with the planet ships that the playerID value has been changed and that the visibility of the planet ships might need to be updated.
            PlanetShip.OnPlayerIDChange();
        }
    }
    //Indicates the turn that the game is on.
    public static int turnNumber
    {
        get => turnNumberVar;
        private set
        {
            //Sets the variable that indicates what turn the game is on to the specified value.
            turnNumberVar = value;
            //Updates the resource bar to accurately reflect what turn the game is on.
            ResourceBar.UpdateTurnText();
        }
    }
    private static int turnNumberVar = 0;

    public static List<GalaxyPlanet> planets = null;

    public static bool observationModeEnabled = false;
    public static bool activeInHierarchy
    {
        get
        {
            if (galaxyManager == null)
                return false;
            return galaxyManager.gameObject.activeInHierarchy;
        }
    }

    public static GalaxyManager galaxyManager = null;

    public static List<Material> empireMaterials = new List<Material>() { null, null, null, null, null, null };
    public static Dictionary<Empire.Culture, Material[]> pillMaterials = new Dictionary<Empire.Culture, Material[]>();

    public static Camera galaxyCamera { get => galaxyCameraVar; }
    private static Camera galaxyCameraVar = null;

    public static Canvas galaxyCanvas { get => galaxyCanvasVar; }
    private static Canvas galaxyCanvasVar = null;

    public static Transform galaxyConfirmationPopupParent { get => galaxyConfirmationPopupParentVar; }
    private static Transform galaxyConfirmationPopupParentVar = null;

    public static Transform popupsParent { get => popupsParentVar; }
    private static Transform popupsParentVar = null;

    public static void Initialize(List<GalaxyPlanet> planetList, Camera galaxyCam, Canvas canvasOfGalaxy, Transform parentOfGalaxyConfirmationPopup, Transform parentOfPopups)
    {
        planets = planetList;
        galaxyCameraVar = galaxyCam;
        galaxyCanvasVar = canvasOfGalaxy;
        galaxyConfirmationPopupParentVar = parentOfGalaxyConfirmationPopup;
        popupsParentVar = parentOfPopups;

        if (NewGameMenu.initialized)
        {
            //Sets whether or not the game has ironman mode enabled.
            ironpillModeEnabled = NewGameMenu.IronmanModeEnabled;
        }

        //Loads in all of the materials that will be applied to pills in pill views.
        if(pillMaterials == null || pillMaterials.Keys.Count == 0)
            galaxyManager.LoadInPillMaterials();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        if (!GalaxyGameSettings.loaded)
            GalaxyGameSettings.LoadSettings();
    }

    public override void Awake()
    {
        base.Awake();

        //Assigns the static reference of galaxy manager.
        galaxyManager = this;
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
    public override void Update()
    {
        base.Update();

        //Resets the boolean that indicates if a popup was closed on the frame.
        ResetPopupClosedOnFrame();

        //Resets the boolean that indicates if the settings menu was closed on the current frame.
        GalaxySettingsMenu.ResetClosedOnFrameBool();

        //Toggles the cheat console if the player presses tilde.
        if (Input.GetKeyDown(KeyCode.BackQuote) && !GalaxyConfirmationPopupBehaviour.IsAGalaxyConfirmationPopupOpen() && !GalaxyPauseMenu.isOpen)
        {
            if(!ironpillModeEnabled)
                cheatConsoleVar.ToggleConsole();
        }

        //Opens the pause menu if the player presses the escape key and no other popup is open.
        if(Input.GetKeyDown(KeyCode.Escape) && !GalaxyConfirmationPopupBehaviour.IsAGalaxyConfirmationPopupOpen() && !GalaxyPopupBehaviour.IsAPopupActiveInHierarchy && !GalaxyPauseMenu.isOpen)
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
        playerIDVar = 0;
        turnNumberVar = 0;
        planets = null;
        observationModeEnabled = false;
        galaxyManager = null;
        empireMaterials = new List<Material>() { null, null, null, null, null, null };
        pillMaterials = new Dictionary<Empire.Culture, Material[]>();
        galaxyCameraVar = null;
        galaxyCanvasVar = null;
        galaxyConfirmationPopupParentVar = null;
        popupsParentVar = null;
    }
}