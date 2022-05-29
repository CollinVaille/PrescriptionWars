using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GalaxyManager : GalaxyViewBehaviour
{
    [Header("Cheat Console")]

    [SerializeField]
    private CheatConsole cheatConsole = null;
    public CheatConsole CheatConsole
    {
        get
        {
            return cheatConsole;
        }
    }
    
    [Header("Other Views")]

    [SerializeField]
    private GameObject researchView = null;
    public GameObject ResearchView
    {
        get
        {
            return researchView;
        }
    }

    [Header("Sound Effects")]

    [SerializeField]
    private AudioClip switchToResearchViewSFX = null;
    [SerializeField]
    private AudioClip techFinishedSFX = null;
    [SerializeField]
    private AudioClip endTurnSFX = null;

    [Header("Settings")]

    [SerializeField]
    private bool ironpillModeEnabled = false;
    public static bool IronpillModeEnabled
    {
        get
        {
            if (galaxyManager == null)
                return false;
            return galaxyManager.ironpillModeEnabled;
        }
        set
        {
            if(galaxyManager != null)
                galaxyManager.ironpillModeEnabled = value;
        }
    }

    //Non-inspector variables.

    //Indicates the empire ID of the player's empire.
    private static int playerID = 0;
    public static int PlayerID
    {
        get
        {
            return playerID;
        }
        set
        {
            //Sets the variable that indicates the empire ID of the player's empire to the specified value.
            playerID = value;
            //Updates the resource bar to accurately reflect the new empire that the player has switched to.
            ResourceBar.UpdateAllEmpireDependantComponents();
            //Logs with the planet ships that the playerID value has been changed and that the visibility of the planet ships might need to be updated.
            PlanetShip.OnPlayerIDChange();
        }
    }
    //Indicates the turn that the game is on.
    private static int turnNumber = 0;
    public static int TurnNumber
    {
        get
        {
            return turnNumber;
        }
        set
        {
            //Sets the variable that indicates what turn the game is on to the specified value.
            turnNumber = value;
            //Updates the resource bar to accurately reflect what turn the game is on.
            ResourceBar.UpdateTurnText();
        }
    }

    public static List<GalaxyPlanet> planets;

    public static bool observationModeEnabled = false;
    public static bool IsGalaxyViewActiveInHierarchy
    {
        get
        {
            if (galaxyManager == null)
                return false;
            return galaxyManager.gameObject.activeInHierarchy;
        }
    }

    public static GalaxyManager galaxyManager;

    public static List<Material> empireMaterials = new List<Material>() { null, null, null, null, null, null };
    public static Dictionary<Empire.Culture, Material[]> pillMaterials = new Dictionary<Empire.Culture, Material[]>();

    private static Camera galaxyCamera = null;
    public static Camera GalaxyCamera
    {
        get
        {
            return galaxyCamera;
        }
    }

    private static Canvas galaxyCanvas = null;
    public static Canvas GalaxyCanvas
    {
        get
        {
            return galaxyCanvas;
        }
    }

    private static Transform galaxyConfirmationPopupParent = null;
    public static Transform GalaxyConfirmationPopupParent
    {
        get
        {
            return galaxyConfirmationPopupParent;
        }
    }

    private static Transform popupsParent = null;
    public static Transform PopupsParent
    {
        get
        {
            return popupsParent;
        }
    }

    public static void Initialize(List<GalaxyPlanet> planetList, Camera galaxyCam, Canvas canvasOfGalaxy, Transform parentOfGalaxyConfirmationPopup, Transform parentOfPopups)
    {
        planets = planetList;
        galaxyCamera = galaxyCam;
        galaxyCanvas = canvasOfGalaxy;
        galaxyConfirmationPopupParent = parentOfGalaxyConfirmationPopup;
        popupsParent = parentOfPopups;

        if (NewGameMenu.initialized)
        {
            //Sets whether or not the game has ironman mode enabled.
            IronpillModeEnabled = NewGameMenu.IronmanModeEnabled;
        }

        //Loads in all of the materials that will be applied to pills in pill views.
        if(pillMaterials == null || pillMaterials.Keys.Count == 0)
            galaxyManager.LoadInPillMaterials();
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
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
        if (Input.GetKeyDown(KeyCode.BackQuote) && !GalaxyConfirmationPopup.IsAGalaxyConfirmationPopupOpen())
        {
            if(!IronpillModeEnabled)
                cheatConsole.ToggleConsole();
        }
    }

    public void WarningRightSideNotificationsUpdate()
    {
        //No research selected warning.
        if(Empire.empires[PlayerID].techManager.techTotemSelected < 0)
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
        researchView.SetActive(true);
        //Switches the skybox material to the one assigned to the research view.
        RenderSettings.skybox = researchView.GetComponent<ResearchViewManager>().skyboxMaterial;

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
                if (x != PlayerID || observationModeEnabled)
                    Empire.empires[x].PlayAI();
            }

            //Stuff is calculated and added after everyone's turn.
            foreach (Empire empire in Empire.empires)
            {
                empire.EndTurn();
            }

            WarningRightSideNotificationsUpdate();

            //Logs that a turn has been completed.
            TurnNumber++;
        }
    }

    public void PlayTechFinishedSFX()
    {
        AudioManager.PlaySFX(techFinishedSFX);
    }
}