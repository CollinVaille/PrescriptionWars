using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NewGalaxyManager :  GalaxyViewBehaviour
{
    [Header("Button Components")]

    [SerializeField] private Button endTurnButton = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip clickEndTurnButtonSFX = null;
    [SerializeField] private AudioClip endTurnFinishedSFX = null;

    [Header("Options")]

    [SerializeField] private float endTurnProcessLength = 3;
    [SerializeField] private float solarSystemOrbitSpeed = 2;
    [SerializeField] private float _basePlanetaryOrbitalSpeed = 10;

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
    private string _saveName = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the current save.
    /// </summary>
    public static string saveName 
    {
        get
        {
            if (galaxyManager._saveName != null && !galaxyManager._saveName.Equals(""))
                return galaxyManager._saveName;
            int saveNameCreationAttempts = 0;
            while (true)
            {
                string saveNameCreationAttempt = saveNameCreationAttempts == 0 ? playerEmpire.name : playerEmpire.name + " " + (saveNameCreationAttempts + 1);
                if (!GalaxySaveSystem.SaveExists(saveNameCreationAttempt))
                {
                    galaxyManager._saveName = saveNameCreationAttempt;
                    return galaxyManager._saveName;
                }
                saveNameCreationAttempts++;
            }
        }
        set => galaxyManager._saveName = value;
    }

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
    /// Private holder variable for the list of hyperspace lanes that exist to connect solar systems within the galaxy.
    /// </summary>
    private List<HyperspaceLane> hyperspaceLanesVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains the hyperspace lanes that exist to connect solar systems within the galaxy.
    /// </summary>
    public static List<HyperspaceLane> hyperspaceLanes { get => galaxyManager.hyperspaceLanesVar; }

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
    private int _playerID = -1;
    /// <summary>
    /// Public static property that should be used to access the player's empire ID (index in the list of empires within the galaxy).
    /// </summary>
    public static int playerID
    {
        get => galaxyManager == null ? -1 : galaxyManager._playerID;
        set
        {
            //Checks if the galaxy manager static reference is null and if so then a warning is logged to the console before returning.
            if(galaxyManager == null)
            {
                Debug.LogWarning("Cannot set the playerID in the galaxy manager if the galaxy scene is not currently open.");
                return;
            }
            //Sets the playerID int to its new specified value.
            galaxyManager._playerID = value;
            //Updates all empire dependent components on the resource bar.
            NewResourceBar.UpdateAllEmpireDependentComponents();
        }
    }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all planet labels within the galaxy.
    /// </summary>
    private Transform planetLabelsParentVar = null;
    /// <summary>
    /// Public static property that should be used to access the transform of the game object that serves as the parent object for all planet labels within the galaxy.
    /// </summary>
    public static Transform planetLabelsParent { get => galaxyManager == null ? null : galaxyManager.planetLabelsParentVar; }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all star labels within the galaxy.
    /// </summary>
    private Transform starLabelsParentVar = null;
    /// <summary>
    /// Public static property that should be used to access the transform of the game object that serves as the parent object for all star labels within the galaxy.
    /// </summary>
    public static Transform starLabelsParent { get => galaxyManager == null ? null : galaxyManager.starLabelsParentVar; }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all capital symbols within the galaxy.
    /// </summary>
    private Transform _capitalSymbolsParent = null;
    /// <summary>
    /// Public static property that should be used in order to access the transform of the game object that serves as the parent object for all capital symbols within the galaxy.
    /// </summary>
    public static Transform capitalSymbolsParent { get => galaxyManager == null ? null : galaxyManager._capitalSymbolsParent; }

    /// <summary>
    /// Private holder variable for the popup that serves as the galaxy view's pause menu.
    /// </summary>
    private NewGalaxyPauseMenu _pauseMenu = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain a reference to the galaxy view's popup pause menu.
    /// </summary>
    public static NewGalaxyPauseMenu pauseMenu { get => galaxyManager == null ? null : galaxyManager._pauseMenu; }

    /// <summary>
    /// Private holder variable for the popup that serves as the galaxy view's settings menu.
    /// </summary>
    private NewGalaxySettingsMenu _settingsMenu = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain a reference to the galaxy view's popup settings menu.
    /// </summary>
    public static NewGalaxySettingsMenu settingsMenu { get => galaxyManager == null ? null : galaxyManager._settingsMenu; }

    /// <summary>
    /// Private holder variable for the cheat console that the player can access to cheat in the galaxy view of the game.
    /// </summary>
    private NewCheatConsole _cheatConsole = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain a reference to the galaxy view's cheat console that allows the player to cheat mid game on the galaxy view.
    /// </summary>
    public static NewCheatConsole cheatConsole { get => galaxyManager == null ? null : galaxyManager._cheatConsole; }

    /// <summary>
    /// Private holder variable for the manager that manages all notifications within the galaxy scene.
    /// </summary>
    private GalaxyNotificationManager _notificationManager = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain a reference to the galaxy view's notification manager that manages all notifications within the galaxy scene.
    /// </summary>
    public static GalaxyNotificationManager notificationManager { get => galaxyManager == null ? null : galaxyManager._notificationManager; }

    /// <summary>
    /// Private holder variable for the manager that manages all popups within the galaxy scene.
    /// </summary>
    private NewGalaxyPopupManager _popupManager = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain a reference to the galaxy view's popup manager that manages all popups within the galaxy scene.
    /// </summary>
    public static NewGalaxyPopupManager popupManager { get => galaxyManager == null ? null : galaxyManager._popupManager; }

    /// <summary>
    /// Private holder variable for the manager that manages all pills within the galaxy scene.
    /// </summary>
    private GalaxyPillManager _pillManager = null;
    /// <summary>
    /// Public static property that should be used in order to access a reference to the galaxy view's pill manager that manages all pills within the galaxy scene.
    /// </summary>
    public static GalaxyPillManager pillManager { get => galaxyManager == null ? null : galaxyManager._pillManager; }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all confirmation popups within the galaxy scene.
    /// </summary>
    private Transform _confirmationPopupsParent = null;
    /// <summary>
    /// Public static property that should be used in order to access the transform of the game object that serves as the parent object for all confirmation popups within the galaxy scene.
    /// </summary>
    public static Transform confirmationPopupsParent { get => galaxyManager == null ? null : galaxyManager._confirmationPopupsParent; }

    /// <summary>
    /// Private holder variable for the transform of the game object that serves as the parent object for all popups within the galaxy scene.
    /// </summary>
    private Transform _popupsParent = null;
    /// <summary>
    /// Public static property that should be used in order to access the transform of the game object that serves as the parent object for all popups within the galaxy scene.
    /// </summary>
    public static Transform popupsParent { get => galaxyManager == null ? null : galaxyManager._popupsParent; }

    /// <summary>
    /// Publicly accessible static property that should be accessed in order to obtain a reference to the current game's player empire.
    /// </summary>
    public static NewEmpire playerEmpire { get => galaxyManager == null || empires == null || playerID < 0 || playerID >= empires.Count ? null : empires[playerID]; }

    /// <summary>
    /// Private holder variable that indicates how many turns have passed since the start of the game.
    /// </summary>
    private int _turnNumber = -1;
    /// <summary>
    /// Publicly accessible static property that should be accessed in order to determine how many turns have passed since the start of the game.
    /// </summary>
    public static int turnNumber
    {
        get => galaxyManager == null ? -1 : galaxyManager._turnNumber;
        private set
        {
            //Logs a warning to the console before returning if the galaxy manager static instance is null, which should effectively mean that the galaxy scene is not currently active in the game.
            if(galaxyManager == null)
            {
                Debug.LogWarning("Cannot update the turn number of the game if there is no valid game currently to set the turn number on. Galaxy manager static instance is null.");
                return;
            }
            //Sets the value of the variable that indicates the game's turn number to the specified value.
            galaxyManager._turnNumber = value;
            //Updates the text on the resource bar that indicates the number of turns that have passed since the current game started.
            NewResourceBar.UpdateTurnNumberText();
        }
    }
    /// <summary>
    /// Private holder variable that indicates whether the current turn is in the process of ending.
    /// </summary>
    private bool _turnEnding = false;
    /// <summary>
    /// Publicly accessible static property that indicates whether the current turn is in the process of ending.
    /// </summary>
    public static bool turnEnding
    {
        get => galaxyManager == null ? false : galaxyManager._turnEnding;
        private set
        {
            //Checks if the galaxy manager is null and returns if so.
            if(galaxyManager == null)
            {
                Debug.LogWarning("Cannot set whether or not the turn is ending if there is no valid galaxy manager.");
                return;
            }

            //Closes all popups and dismisses all notifications if the turn is ending.
            if (value)
            {
                NewGalaxyPopupBehaviour.CloseAllPopups();
                notificationManager.DismissAllNotifications();
            }
            //Resets the variable that indicates how much time has passed since the turn started ending.
            galaxyManager.turnEndingTimeElapsed = 0;
            //Sets the variable that indicates whether or not the current turn is ending to the specified value.
            galaxyManager._turnEnding = value;
            //Updates the interactability of the end turn button.
            UpdateEndTurnButtonInteractability();
        }
    }
    /// <summary>
    /// Private holder variable that indicates how much time has passed since the turn started ending.
    /// </summary>
    private float turnEndingTimeElapsed = 0;

    /// <summary>
    /// Private holder variable that indicates whether or not the game is in observation mode. Observation mode is where the player empire is still controlled by a bot, making it to where all empires in the game are controlled by a bot.
    /// </summary>
    private bool _observationModeEnabled = false;
    /// <summary>
    /// Public static property that should be used both to access and mutate whether or not the game is currently in observation mode. Observation mode is where the player empire is still controlled by a bot, making it to where all empires in the game are controlled by a bot.
    /// </summary>
    public static bool observationModeEnabled
    {
        get => galaxyManager == null ? false : galaxyManager._observationModeEnabled;
        set
        {
            //Returns if there is no valid galaxy to toggle observation mode on.
            if(galaxyManager == null)
            {
                Debug.LogWarning("Cannot toggle observation mode if there is no galaxy to toggle observation mode on.");
                return;
            }

            //Sets observation mode being enabled to the specified value.
            galaxyManager._observationModeEnabled = value;
        }
    }

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether or not the game is in ironpill mode. Ironpill mode is essentially ironman or hardcore mode where the player cannot do a save as and cannot access the cheat console.
    /// </summary>
    private bool _ironPillModeEnabled = false;
    /// <summary>
    /// Public static property that should be used in order to access the boolean value that indicates whether or not the game is in ironpill mode. Ironpill mode is essentially ironman or hardcore mode where the player cannot do a save as and cannot access the cheat console.
    /// </summary>
    public static bool ironPillModeEnabled { get => galaxyManager == null ? false : galaxyManager._ironPillModeEnabled; }

    /// <summary>
    /// Publicly accessible static float property that should be accessed in order to determine the base planetary orbital speed before proximity to star calculations come into play.
    /// </summary>
    public static float basePlanetaryOrbitalSpeed { get => galaxyManager == null ? 0 : galaxyManager._basePlanetaryOrbitalSpeed; }

    /// <summary>
    /// Private holder variable for a dictionary that contains all of the resource modifiers affecting empires in the current galaxy game paired with their respective resource modifier ID int.
    /// </summary>
    private Dictionary<int, GalaxyResourceModifier> _resourceModifiers = null;
    /// <summary>
    /// Public static property that should be accessed in order to obtain the dictionary that contains all of the resource modifiers affecting empires in the current galaxy game paired with their respective resource modifier ID int.
    /// </summary>
    public static Dictionary<int, GalaxyResourceModifier> resourceModifiers { get => galaxyManager == null ? null : galaxyManager._resourceModifiers; }
    /// <summary>
    /// Private holder variable for the integer value that indicates exactly how many resources modifiers have been created so far and what the ID of the next resource modifier should be.
    /// </summary>
    private int _resourceModifiersCount = -1;
    /// <summary>
    /// Public static property that should be used to both access and mutate the integer value that indicates exactly how many resources modifiers have been created so far and what the ID of the next resource modifier should be.
    /// </summary>
    public static int resourceModifiersCount
    {
        get => galaxyManager == null ? -1 : galaxyManager._resourceModifiersCount;
        set { if (galaxyManager != null) galaxyManager._resourceModifiersCount = value; else Debug.LogWarning("Cannot set the resource modifiers count because the galaxy manager static reference is null, meaning there is no valid active galaxy game."); }
    }

    /// <summary>
    /// Private holder variable for a dictionary that contains global actions assigned to an integer value ID.
    /// </summary>
    private Dictionary<int, Action<string[]>> _globalActions = null;
    /// <summary>
    /// Private holder variable for the integer value that represents how many global actions have been added to the dictionary of global actions. The current value of the variable will be used as the ID for the next global action added.
    /// </summary>
    private int _globalActionsCount = -1;
    /// <summary>
    /// Public property that should be used in order to access the integer value that represents how many global actions have been added to the dictionary of global actions. The current value of the variable will be used as the ID for the next global action added.
    /// </summary>
    public static int globalActionsCount { get => galaxyManager == null ? -1 : galaxyManager._globalActionsCount; }

    /// <summary>
    /// Public static method that should be called by the galaxy generator at the end of the start method in order to initialize all of the needed variables within the galaxy manager.
    /// </summary>
    public static void InitializeFromGalaxyGenerator(NewGalaxyManager galaxyManager, string saveName, Material skyboxMaterial, List<GalaxySolarSystem> solarSystems, List<NewGalaxyPlanet> planets, List<NewEmpire> empires, List<HyperspaceLane> hyperspaceLanes, string galaxyShape, int playerID, bool observationModeEnabled, bool ironPillModeEnabled, List<Transform> parents, NewGalaxyPauseMenu pauseMenu, NewGalaxySettingsMenu settingsMenu, NewCheatConsole cheatConsole, GalaxyNotificationManager notificationManager, NewGalaxyPopupManager popupManager, GalaxyPillManager pillManager, int turnNumber, Dictionary<int, GalaxyResourceModifier> resourceModifiers, int resourceModifiersCount, int globalActionsCount)
    {
        //Sets the static instance of the galaxy manager.
        galaxyManagerVar = galaxyManager;

        //Sets the value of the variable that holds the name of the game save.
        galaxyManager._saveName = saveName;

        //Sets the value of the variable that holds the skybox material of the galaxy.
        galaxyManager.skyboxMaterialVar = skyboxMaterial;

        //Sets the value of the variable that contains all of the solar systems existing within the galaxy.
        galaxyManager.solarSystemsVar = solarSystems;

        //Sets the value of the variable that contains all of the planets existing within the galaxy.
        galaxyManager.planetsVar = planets;

        //Sets the value of the variable that contains all of the empires existing within the galaxy.
        galaxyManager.empiresVar = empires;

        //Sets the value of the variable that contains all the hyperspace lanes that exist to connect solar systems within the galaxy.
        galaxyManager.hyperspaceLanesVar = hyperspaceLanes;

        //Sets the value of the variable that contains the name of the shape that the galaxy was generated to fit.
        galaxyManager.galaxyShapeVar = galaxyShape;

        //Sets the value of the variable that contains the player's empire ID (index in the list of empires within the galaxy).
        galaxyManager._playerID = playerID;

        //Sets the value of the variable that indicates whether or not the game is in observation mode.
        galaxyManager._observationModeEnabled = observationModeEnabled;

        //Sets the value of the variable that indicates whether or not the game is in ironpill mode. Ironpill mode is essentially ironman or hardcore mode where the player cannot do a save as and cannot access the cheat console.
        galaxyManager._ironPillModeEnabled = ironPillModeEnabled;

        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all planet labels within the galaxy.
        galaxyManager.planetLabelsParentVar = parents[0];
        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all star labels within the galaxy.
        galaxyManager.starLabelsParentVar = parents[1];
        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all capital symbols within the galaxy.
        galaxyManager._capitalSymbolsParent = parents[2];
        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all confirmation popups within the galaxy scene.
        galaxyManager._confirmationPopupsParent = parents[3];
        //Sets the value of the variable that contains the transform of the game object that serves as the parent object for all popups within the galaxy scene.
        galaxyManager._popupsParent = parents[4];

        //Sets the value of the variable that contains a reference to the popup that serves as the pause menu of the galaxy view.
        galaxyManager._pauseMenu = pauseMenu;

        //Sets the value of the variable that contains a reference to the popup that serves as the settings menu of the galaxy view.
        galaxyManager._settingsMenu = settingsMenu;

        //Sets the value of the variable that contains a reference to the cheat console that allows the player to cheat mid game on the galaxy view.
        galaxyManager._cheatConsole = cheatConsole;

        //Sets the value of the variable that contains a reference to the manager that manages all notifications within the galaxy scene.
        galaxyManager._notificationManager = notificationManager;

        //Sets the value of the variable that contains a reference to the manager that manages all popups within the galaxy scene.
        galaxyManager._popupManager = popupManager;

        //Sets the value of the variable that contains a reference to the manager that manages all pills within the galaxy scene.
        galaxyManager._pillManager = pillManager;

        //Sets the value of the variable that indicates how many turns have passed since the start of the game.
        galaxyManager._turnNumber = turnNumber;

        //Sets the value of the variable that is a dictionary that contains all of the resource modifiers affecting empires in the current galaxy game paired with their respective resource modifier ID int.
        galaxyManager._resourceModifiers = resourceModifiers;
        //Sets the value of the variable that is an integer value that indicates exactly how many resources modifiers have been created so far and what the ID of the next resource modifier should be.
        galaxyManager._resourceModifiersCount = resourceModifiersCount;

        //Sets the value of the variable that is an integer value that indicates how many global actions have been added to the dictionary of global actions. The current value of the variable will be used as the ID for the next global action added.
        galaxyManager._globalActionsCount = globalActionsCount;
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

        if (turnEnding)
            EndTurnUpdate();
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !NewGalaxyPopupBehaviour.popupClosedOnFrame && !NewGalaxyPopupBehaviour.isAPopupOpen)
                pauseMenu.Open();
            if (!ironPillModeEnabled && Input.GetKeyDown(KeyCode.BackQuote) && !pauseMenu.open)
                cheatConsole.Toggle();
        }
    }

    /// <summary>
    /// This method should be called whenever the galaxy game object that the galaxy manager script is attached to is destroyed, which effectively means that the scene has been changed.
    /// </summary>
    private void OnDestroy()
    {
        //Resets the static galaxy manager instance variable to null.
        galaxyManagerVar = null;
    }

    /// <summary>
    /// This public method should be called by an event trigger created in the inspector whenever the end turn button is pressed and it goes through all of the logic needed for a turn ending.
    /// </summary>
    public void OnClickEndTurnButton()
    {
        //Plays the appropriate sound effect for clicking the end turn button.
        AudioManager.PlaySFX(clickEndTurnButtonSFX);
        //Logs that the turn is currently ending.
        turnEnding = true;
    }

    /// <summary>
    /// This private method should be called by the update method whenever the current turn is in the process of ending.
    /// </summary>
    private void EndTurnUpdate()
    {
        //Adds the amount of time that has passed since the last frame to the variable that indicates how much time has passed since the turn started ending.
        turnEndingTimeElapsed += Time.deltaTime;
        //Rotates the galaxy.
        transform.Rotate(new Vector3(0, -1 * (solarSystemOrbitSpeed * Time.deltaTime), 0));
        //Syncs the transforms to update them before calling the EndTurnUpdate functions on other objects.
        Physics.SyncTransforms();
        //Calls the end turn update function on each solar system which should fix the name label positioning of the star and the planets in the systems.
        foreach (GalaxySolarSystem solarSystem in solarSystems)
            solarSystem.EndTurnUpdate();
        //Ends the end turn process if the appropriate amount of time has elapsed.
        if (turnEndingTimeElapsed >= endTurnProcessLength)
        {
            //Calls the OnEndTurnFinalUpdate method on each empire so that the empire can get ready for the new turn.
            foreach(NewEmpire empire in empires)
                empire.OnEndTurnFinalUpdate();
            //Increments the turn number.
            turnNumber++;
            //Indicates that a turn is no longer ending.
            turnEnding = false;
            //Plays the appropriate sound effect.
            AudioManager.PlaySFX(endTurnFinishedSFX);
        }
    }

    /// <summary>
    /// Public static method that should be called in order to add a specified action to the dictionary of global actions before returning its now assigned integer value ID in the global actions dictionary.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static int AddGlobalAction(Action<String[]> action)
    {
        //Checks if the galaxy manager is null and logs a warning and returns -1 if so.
        if(galaxyManager == null)
        {
            Debug.LogWarning("Cannot add action to the dictionary of global actions because the galaxy manager is null. Probably meaning that there is no valid galaxy to add the global action to.");
            return -1;
        }

        //Checks if the specified action is null and logs a warning and returns -1 if so.
        if(action == null)
        {
            Debug.LogWarning("Cannot add a null action to the dictionary of global actions.");
            return -1;
        }

        //Initializes the dictionary of global actions if it has not yet been initialized.
        if (galaxyManager._globalActions == null)
            galaxyManager._globalActions = new Dictionary<int, Action<string[]>>();

        //Adds the specified action to the dictionary of global actions and assigns it to the integer ID that equals the global actions count before then incrementing the global actions count since a new one was just added.
        int globalActionIndex = galaxyManager._globalActionsCount;
        galaxyManager._globalActions.Add(globalActionIndex, action);
        galaxyManager._globalActionsCount++;

        //Returns the specified actions integer ID value in the dictionary of global actions.
        return globalActionIndex;
    }

    /// <summary>
    /// Sets the global action assigned to the specified ID integer value to the specified action.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="action"></param>
    public static void SetGlobalAction(int ID, Action<String[]> action)
    {
        //Checks if the galaxy manager is null and logs a warning and returns if so.
        if (galaxyManager == null)
        {
            Debug.LogWarning("Cannot set global action because the galaxy manager is null. Probably meaning that there is no valid galaxy.");
            return;
        }

        //Checks if the specified ID is less than 0 and logs a warning and returns if so.
        if(ID < 0)
        {
            Debug.LogWarning("Cannot assign global action to a negative global action ID integer value.");
            return;
        }

        //Checks if the specified action is null and logs a warning and returns if so.
        if (action == null)
        {
            Debug.LogWarning("Cannot set global action to a null action.");
            return;
        }

        //Initializes the dictionary of global actions if it has not yet been initialized.
        if (galaxyManager._globalActions == null)
            galaxyManager._globalActions = new Dictionary<int, Action<string[]>>();

        //Checks if the ID already exists in the dictionary and assigns the specified action to it if so.
        if (galaxyManager._globalActions.ContainsKey(ID))
            galaxyManager._globalActions[ID] = action;
        //Adds the specified action to the dictionary of global actions and updates the global actions count variable if needed.
        else
        {
            galaxyManager._globalActions.Add(ID, action);
            if (ID >= galaxyManager._globalActionsCount)
                galaxyManager._globalActionsCount = ID;
        }
    }

    /// <summary>
    /// Public static method that should be used in order to access the global action assigned to the specified ID int in the dictionary of global actions.
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static Action<String[]> GetGlobalAction(int ID)
    {
        //Checks if the galaxy manager is null and logs a warning and returns null if so.
        if (galaxyManager == null)
        {
            Debug.LogWarning("Cannot get a global action assigned to a specified ID if the galaxy manager itself is null. Probably meaning that there is no valid galaxy.");
            return null;
        }

        //Checks if the dictionary of global actions is null and returns null if so.
        if (galaxyManager._globalActions == null)
            return null;

        //Checks if the dictionary doesn't contain a global action assigned to the specified ID and returns null if so.
        if (!galaxyManager._globalActions.ContainsKey(ID))
            return null;

        //Returns the global action assigned to the specified global action ID.
        return galaxyManager._globalActions[ID];
    }

    /// <summary>
    /// Public static method that should be used in order to remove the global action assigned to the specified ID int from the dictionary of global actions.
    /// </summary>
    /// <param name="ID"></param>
    public static void RemoveGlobalAction(int ID)
    {
        //Checks if the galaxy manager is null and logs a warning and returns if so.
        if (galaxyManager == null)
        {
            Debug.LogWarning("Cannot remove a global action assigned to a specified ID if the galaxy manager itself is null. Probably meaning that there is no valid galaxy.");
            return;
        }

        //Checks if the dictionary of global actions is null and returns if so.
        if (galaxyManager._globalActions == null)
            return;

        //Checks if the dictionary doesn't contain a global action assigned to the specified ID and returns if so.
        if (!galaxyManager._globalActions.ContainsKey(ID))
            return;

        //Removes the global action assigned to the specified global action ID from the dictionary of global actions.
        galaxyManager._globalActions.Remove(ID);
    }

    /// <summary>
    /// Public static method that should be called by the popup manager whenever the popup count changes. Updates whether the end turn button is interactable or not.
    /// </summary>
    public static void OnPopupCountChange()
    {
        //Checks if the galaxy manager is null and logs a warning and returns if so.
        if (galaxyManager == null)
        {
            Debug.LogWarning("Cannot execute OnPopupCountChange logic because the galaxy manager itself is null. Probably meaning that there is no valid galaxy.");
            return;
        }

        //Updates whether the end turn button is interactable or not.
        UpdateEndTurnButtonInteractability();
    }

    /// <summary>
    /// Public static method that should be called by the notification manager whenever the notification count changes. Updates whether the end turn button is interactable or not.
    /// </summary>
    public static void OnNotificationCountChange()
    {
        //Checks if the galaxy manager is null and logs a warning and returns if so.
        if (galaxyManager == null)
        {
            Debug.LogWarning("Cannot execute OnNotificationCountChange logic because the galaxy manager itself is null. Probably meaning that there is no valid galaxy.");
            return;
        }

        //Updates whether the end turn button is interactable or not.
        UpdateEndTurnButtonInteractability();
    }

    /// <summary>
    /// Private method that should be called in order to update the interactability of the turn button.
    /// </summary>
    private static void UpdateEndTurnButtonInteractability()
    {
        //Checks if the galaxy manager is null and logs a warning and returns if so.
        if(galaxyManager == null)
        {
            Debug.LogWarning("Cannot update the interactability of the end turn button if there is no valid galaxy manager to update the end turn interactability on.");
            return;
        }

        //Updates the interactability of the end turn button.
        galaxyManager.endTurnButton.interactable = !turnEnding && popupManager.popupCount == 0 && !notificationManager.isNonDismissableNotificationInQueue;
    }
}
