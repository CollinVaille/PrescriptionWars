using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetPauseMenu : MonoBehaviour
{
    public enum MenuScreen { NoScreen = -1, SituationMenu = 0, LoadingScreen = 1, SquadMenu = 2, DisplaySettingsMenu = 3,
        MapMenu = 4, CommandsMenu = 5, SettingsMenu = 6 }
    public enum NavigationPane { Situation = 0, Squad = 1, Map = 2 }

    public static PlanetPauseMenu pauseMenu;

    private bool paused = false;

    //Menu panes and screens
    private MenuScreen currentScreen;
    private NavigationPane currentPane = NavigationPane.Situation;
    public Transform[] pauseMenus;
    public Transform HUD, navigationBar;
    public Image[] factionColored;
    public GameObject squadMemberButton;
    private bool saveOnLeave = true;

    //Audio
    private AudioSource oneShotAudioSource;
    public AudioClip mouseOver, buttonClick, pauseSound, dropdownOpen, confirmSound, backSound;

    //Loading screen
    public int loadingScreens = 2;
    private Sprite loadingScreenArtwork;
    private string loadingScreenTrivia = "";
    private static string[] triviaLines;

    //Squad
    public GameObject memberCameraPrefab;
    private Transform squadMemberCamera;
    private List<Transform> squadMemberButtons;
    private bool commandsMenuInitialized = false, resumeAfterCommandsMenu = false;

    //Map
    private PlanetMapManager mapManager;

    private void Awake()
    {
        pauseMenu = this;

        //Variable initialization
        currentScreen = MenuScreen.SituationMenu;
        oneShotAudioSource = GetComponent<AudioSource>();
        mapManager = GetComponent<PlanetMapManager>();
        mapManager.PerformSetUp(HUD, pauseMenus[(int)PlanetPauseMenu.MenuScreen.MapMenu]);

        //Set up other UI managers
        DurabilityTextManager.SetUp(HUD);

        //Refresh pause state to start out as resumed
        paused = true;
        Pause(false);

        //Begin loading...
        LoadingScreen(true, false);
    }

    private void Update()
    {
        //Can pause/resume with pause button on keyboard
        if (Input.GetButtonDown("Pause"))
        {
            if (paused)
            {
                if (currentPane == NavigationPane.Situation)
                    BackOneScreen();
                else
                    SetNavigationPane(NavigationPane.Situation);
            }
            else
                Pause(true);
        }
        else if (Input.GetButtonDown("Squad Menu"))
        {
            if (paused)
            {
                if (currentPane == NavigationPane.Squad)
                    Pause(false);
                else
                    SetNavigationPane(NavigationPane.Squad);
            }
            else
                Pause(true, NavigationPane.Squad);
        }
        else if (Input.GetButtonDown("Map Menu"))
        {
            if (paused)
            {
                if (currentPane == NavigationPane.Map)
                    Pause(false);
                else
                    SetNavigationPane(NavigationPane.Map);
            }
            else
                Pause(true, NavigationPane.Map);
        }
        else if (Input.GetButtonDown("Squad Command Menu"))
        {
            if (!paused) //Pause and pull up quick command menu
            {
                Pause(true, NavigationPane.Situation);
                SetMenuScreen(MenuScreen.CommandsMenu);
                resumeAfterCommandsMenu = true;
            }
            else if (currentScreen == MenuScreen.CommandsMenu) //Already on commands menu so just unpause game
                Pause(false);
            else //Already on another menu screen, so just navigate to commands menu
            {
                SetNavigationPane(NavigationPane.Situation);
                SetMenuScreen(MenuScreen.CommandsMenu);
            }
        }

        //Update the information on the map (only updates if the map is open)
        mapManager.UpdateMap();

        /*
        //Allow scrolling up/down member list in squad menu
        else if(paused && currentScreen == MenuScreen.SquadMenu)
        {
            if(Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                ScrollRect scrollRect = pauseMenus[(int)MenuScreen.SquadMenu].Find("Scroll View").GetComponent<ScrollRect>();
                scrollRect.verticalNormalizedPosition += Input.GetAxis("Mouse ScrollWheel");
            }
        }   */
    }

    public void Pause(bool pause, NavigationPane withPane = NavigationPane.Situation)
    {
        if (pause)
        {
            if (!paused) //Pause game
            {
                paused = true;

                //Pause time
                Time.timeScale = 0;

                //Enable cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                //Hide HUD
                HUD.gameObject.SetActive(false);

                //Show pause menu
                SetNavigationPane(withPane);

                //Play pause sound effect
                oneShotAudioSource.PlayOneShot(pauseSound);

                God.god.OnPause();
            }
        }
        else
        {
            if (paused) //Resume game
            {
                paused = false;

                //Resume time
                Time.timeScale = 1;

                //Disable cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //Show HUD
                HUD.gameObject.SetActive(true);

                //Hide navigation bar
                navigationBar.gameObject.SetActive(false);

                //Hide pause menu
                OnMenuScreenUnload(currentScreen);
                pauseMenus[(int)currentScreen].gameObject.SetActive(false);
                currentScreen = MenuScreen.NoScreen;

                God.god.OnResume();
            }
        }
    }

    public void SetMenuScreen(MenuScreen newScreen)
    {
        //Hide previous menu screen
        if(currentScreen != MenuScreen.NoScreen)
            pauseMenus[(int)currentScreen].gameObject.SetActive(false);

        //Perform any needed clean up with that previous screen
        if(currentScreen != MenuScreen.NoScreen)
            OnMenuScreenUnload(currentScreen);

        //Initialize new menu screen contents
        OnMenuScreenLoad(newScreen);

        //Set new menu screen
        currentScreen = newScreen;

        //Show new menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(true);
    }

    private void OnMenuScreenUnload(MenuScreen oldScreen)
    {
        if (oldScreen == MenuScreen.MapMenu)
            mapManager.SwitchToFromMap(false);
        else if (oldScreen == MenuScreen.SquadMenu)
            UpdateSquadMemberCamera(null);
        else if(oldScreen == MenuScreen.CommandsMenu)
        {
            if(saveOnLeave)
            {
                Player.player.AssignOrderButtons(
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 1 Dropdown").GetComponent<Dropdown>(),
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 2 Dropdown").GetComponent<Dropdown>(),
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 3 Dropdown").GetComponent<Dropdown>());
                oneShotAudioSource.PlayOneShot(confirmSound);
            }
            else
            {
                Player.player.RestoreOrderButtons(
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 1 Dropdown").GetComponent<Dropdown>(),
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 2 Dropdown").GetComponent<Dropdown>(),
                    pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command 3 Dropdown").GetComponent<Dropdown>());
            }

            resumeAfterCommandsMenu = false;
        }
        else if(oldScreen == MenuScreen.DisplaySettingsMenu)
        {
            if(saveOnLeave)
            {
                ReadInSettingsFromDisplay();
                oneShotAudioSource.PlayOneShot(confirmSound);
            }
        }

        //Reset save flag for next time
        saveOnLeave = true;
    }

    private void OnMenuScreenLoad(MenuScreen newScreen)
    {
        //Perform all needed menu screen initialization here...

        //Determine whether to show navigation bar based on new screen
        navigationBar.gameObject.SetActive(
            newScreen == MenuScreen.SituationMenu || newScreen == MenuScreen.SquadMenu || newScreen == MenuScreen.MapMenu);

        //Load new screen
        if (newScreen == MenuScreen.DisplaySettingsMenu)
        {
            Transform menu = pauseMenus[(int)newScreen];

            //Set input fields
            menu.Find("Sensitivity Input Field").GetComponent<InputField>().text = VideoSettings.sensitivity.ToString();
            menu.Find("View Distance Input Field").GetComponent<InputField>().text = VideoSettings.viewDistance.ToString();

            //Set quality dropdown options
            Dropdown qualityDropdown = menu.Find("Quality Dropdown").GetComponent<Dropdown>();
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            qualityDropdown.value = VideoSettings.quality;
        }
        else if (newScreen == MenuScreen.MapMenu)
            mapManager.SwitchToFromMap(true);
        else if (newScreen == MenuScreen.SquadMenu)
            UpdateSquadMenu();
        else if (newScreen == MenuScreen.CommandsMenu)
            OnCommandsMenuOpen();
    }

    public void LoadingScreen(bool show, bool generateNew)
    {
        if (show)
        {
            //Set contents of loading screen...

            Sprite newLoadingScreen;
            string newTrivia;

            if (generateNew)
            {
                newLoadingScreen = Resources.Load<Sprite>("General/Loading Screens/Loading Screen " + Random.Range(1, loadingScreens + 1));
                loadingScreenArtwork = newLoadingScreen;

                newTrivia = GetRandomTrivia();
                loadingScreenTrivia = newTrivia;
            }
            else
            {
                newLoadingScreen = loadingScreenArtwork;

                newTrivia = loadingScreenTrivia;
            }

            //Set artwork
            pauseMenus[(int)MenuScreen.LoadingScreen].Find("Artwork").GetComponent<Image>().sprite = newLoadingScreen;

            //Set trivia
            pauseMenus[(int)MenuScreen.LoadingScreen].Find("Trivia").GetComponent<Text>().text = newTrivia;

            //Display loading screen
            SetMenuScreen(MenuScreen.LoadingScreen);
        }
        else
        {
            //Done with loading which means player has been loaded so disable map camera
            God.god.SetActiveCamera(Player.player.transform.Find("Head").Find("Camera").GetComponent<Camera>(), true);
            Destroy(God.god.GetComponent<AudioListener>());

            pauseMenus[(int)MenuScreen.LoadingScreen].gameObject.SetActive(false);
        }
    }

    private string GetRandomTrivia()
    {
        //Get list of trivia lines (only need to do this on first loading screen in session)
        if (triviaLines == null)
            triviaLines = GeneralHelperMethods.GetLinesFromFile("Trivia Lines", false);

        //Pick a random name
        return triviaLines[Random.Range(0, triviaLines.Length)];
    }

    public void OnButtonClick(Transform buttonTransform)
    {
        //Click sound
        oneShotAudioSource.PlayOneShot(buttonClick);

        //Get basic info about button (used for determining what to do in response)
        string buttonName = buttonTransform.name;

        //Special management of back button
        if (buttonName.Equals("Back Button"))
            BackOneScreen();

        //Special management of navigation pane buttons
        else if (buttonTransform.parent == navigationBar)
            SetNavigationPane((NavigationPane)System.Enum.Parse(typeof(NavigationPane),
                buttonName.Substring(0, buttonName.Length - 7)));

        //Management of all other buttons
        else if (currentScreen == MenuScreen.SituationMenu)
        {
            if (buttonName.Equals("Resume Button"))
                Pause(false);
            else if (buttonName.Equals("New Button"))
            {
                Planet.newPlanet = true;
                LoadingScreen(true, true);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (buttonName.Equals("Save Button"))
            {
                Planet.planet.SavePlanet();
            }
            else if (buttonName.Equals("Load Button"))
            {
                Planet.newPlanet = false;
                LoadingScreen(true, true);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (buttonName.Equals("Settings Button"))
                SetMenuScreen(MenuScreen.SettingsMenu);
        }
        else if (currentScreen == MenuScreen.SquadMenu)
        {
            //Clicked on squad member button, so display details about squad member
            if (buttonTransform.parent.name.Equals("Content"))
                UpdateSquadMemberDisplay(Player.player.squad.GetMemberByName(
                    buttonName.Substring(0, buttonName.Length - 7)));

            //Clicked on squad overview button, so display overview info like objective and orders
            else if (buttonName.Equals("Overview Button"))
                BlankSquadMemberDisplay(Player.player.squad);

            //Take what is written in the transmission text box and send it over the airwaves as a radio transmission
            else if(buttonName.Equals("Send Transmission Button"))
            {
                InputField customMessageInputField = pauseMenus[(int)MenuScreen.SquadMenu].Find("Send Transmission Input Field").GetComponent<InputField>();
                string customMessage = customMessageInputField.text;
                
                if(customMessage?.Length > 0)
                {
                    customMessageInputField.text = "";
                    Squad playersSquad = Player.player.squad;
                    playersSquad.GetArmy().Comms().Send(new CustomTransmission(playersSquad, customMessage));
                }
            }
        }
        else if(currentScreen == MenuScreen.CommandsMenu)
        {
            if (buttonName.Equals("Save Button"))
                BackOneScreen();
            else if (buttonName.Equals("Cancel Button"))
            {
                saveOnLeave = false;
                BackOneScreen();
            }
        }
        else if(currentScreen == MenuScreen.SettingsMenu)
        {
            if (buttonName.Equals("Commands Button"))
                SetMenuScreen(MenuScreen.CommandsMenu);
            else if (buttonName.Equals("Display Settings Button"))
                SetMenuScreen(MenuScreen.DisplaySettingsMenu);
        }
        else if (currentScreen == MenuScreen.DisplaySettingsMenu)
        {
            if (buttonName.Equals("Cancel Button"))
                saveOnLeave = false;

                BackOneScreen();
        }
    }

    public void OnButtonMouseOver(Transform buttonTransform)
    {
        if(buttonTransform.GetComponent<Button>().interactable)
            oneShotAudioSource.PlayOneShot(mouseOver);
    }

    public void OnToggleClicked(Transform toggleTransform)
    {
        oneShotAudioSource.PlayOneShot(buttonClick);
    }

    public void OnToggleMouseOver(Transform toggleTransform)
    {
        oneShotAudioSource.PlayOneShot(pauseSound);
    }

    public void OnDropdownOpen(Transform dropdownTransform)
    {
        oneShotAudioSource.PlayOneShot(dropdownOpen);
    }

    private void BackOneScreen()
    {
        //Play back sound when backing out without any save action
        if (!saveOnLeave || !IsSettingsScreen(currentScreen))
            oneShotAudioSource.PlayOneShot(backSound);

        //Determine where to go next
        if (currentScreen == MenuScreen.SettingsMenu)
            SetMenuScreen(MenuScreen.SituationMenu);
        else if (currentScreen == MenuScreen.DisplaySettingsMenu)
            SetMenuScreen(MenuScreen.SettingsMenu);
        else if (currentScreen == MenuScreen.CommandsMenu)
        {
            if (resumeAfterCommandsMenu)
                Pause(false);
            else
                SetMenuScreen(MenuScreen.SettingsMenu);
        }
        else
            Pause(false);
    }

    private bool IsSettingsScreen(MenuScreen menuScreen)
    {
        if (menuScreen == MenuScreen.CommandsMenu)
            return true;
        else if (menuScreen == MenuScreen.DisplaySettingsMenu)
            return true;
        else
            return false;
    }

    private void ReadInSettingsFromDisplay()
    {
        Transform menu = pauseMenus[(int)currentScreen];

        //Sensitivity
        if (!int.TryParse(menu.Find("Sensitivity Input Field").GetComponent<InputField>().text, out int sensitivity))
            sensitivity = 90;
        VideoSettings.sensitivity = Mathf.Clamp(sensitivity, 1, 10000);

        //View distance
        if (!int.TryParse(menu.Find("View Distance Input Field").GetComponent<InputField>().text, out int viewDistance))
            viewDistance = 1000;
        VideoSettings.viewDistance = Mathf.Clamp(viewDistance, 1, 100000);

        //Quality
        VideoSettings.quality = menu.Find("Quality Dropdown").GetComponent<Dropdown>().value;

        //Save and apply
        VideoSettings.SaveSettings();
        Player.player.ApplyDisplaySettings();
    }

    private void SetNavigationPane(NavigationPane newActivePane)
    {
        //First, set the navigation bar button
        int newButtonIndex = (int)newActivePane;
        for(int x = 0; x < 3; x++)
        {
            //Get current button as we iterate through nav bar
            Button navButton = navigationBar.Find(((NavigationPane)x).ToString() + " Button").GetComponent<Button>();

            //Make button pressable if it doesn't belong to the pane newly selected
            navButton.interactable = newButtonIndex != x;
        }

        //Finally, open the default screen for the pane
        SetMenuScreen((MenuScreen)System.Enum.Parse(typeof(MenuScreen), newActivePane.ToString() + "Menu"));

        //Finally, finally, remember our choice
        currentPane = newActivePane;
    }

    public void UpdatePlanetName(string planetName)
    {
        //Update planet name label as displayed on top right of map screen
        pauseMenus[(int)MenuScreen.MapMenu].Find("Planet Name").GetComponent<Text>().text = planetName;

        //Update map navigation button's text
        navigationBar.Find("Map Button").Find("Text").GetComponent<Text>().text = planetName + " Map";
    }

    public void UpdateSquadName(string squadName, Color squadColor)
    {
        //Update squad navigation button's text
        navigationBar.Find("Squad Button").Find("Text").GetComponent<Text>().text = squadName + " Squad";

        //Update squad name title in squad menu
        Text squadNameText = pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Name").GetComponent<Text>();
        squadNameText.text = squadName + " Squad";
        squadNameText.color = squadColor;
    }

    public void UpdateFactionColor()
    {
        //Get player's faction color
        Color newColor = Army.GetArmy(Player.player.team).color;

        //Recolor each image that is supposed to be colored faction-specific
        foreach(Image image in factionColored)
        {
            newColor.a = image.color.a; //Keep transparency value same for image color
            image.color = newColor; //Apply faction color
        }
    }

    public void UpdateSquadMenu()
    {
        //Determine squad
        Squad squad = Player.player.squad;

        if(!squad) //Screen for when player belongs to no squad
            BlankSquadMemberDisplay(null);
        else //Have squad, so display info about it
        {
            //Update squad member list
            UpdateSquadMemberList(squad);

            //Refresh squad member display (keep current member selected, but refresh info)
            Pill member = squad.GetMemberByName(
                pauseMenus[(int)MenuScreen.SquadMenu].Find("Member Name").GetComponent<Text>().text);
            UpdateSquadMemberDisplay(member);
        }
    }

    private void UpdateSquadMemberList(Squad squad)
    {
        //First, show and update label indicating number of members in list
        pauseMenus[(int)MenuScreen.SquadMenu].Find("Member Count").gameObject.SetActive(true);
        pauseMenus[(int)MenuScreen.SquadMenu].Find("Member Count").GetComponent<Text>().text =
            squad.members.Count + " Members";

        //Then, update scroll view...

        //Display scroll view
        pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Member Scroll View").gameObject.SetActive(true);

        //Create some needed variables
        if (squadMemberButtons == null)
            squadMemberButtons = new List<Transform>();
        float buttonHeight = 25;
        Transform contentPane = pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Member Scroll View")
            .Find("Viewport").Find("Content");

        //For each current member: Repurpose old button for member OR create new button if ran out of old ones
        float yPos = -20;
        for (int x = 0; x < squad.members.Count; x++)
        {
            Pill member = squad.members[x];

            //Get button to represent member
            Transform memberButton;
            if (x < squadMemberButtons.Count) //Repurpose old button to represent member
            {
                //Retrieve next old button in list
                memberButton = squadMemberButtons[x];
                memberButton.gameObject.SetActive(true);
            }
            else //Create new button to represent member
            {
                //Create and parent button
                memberButton = Instantiate(squadMemberButton).transform;
                memberButton.SetParent(contentPane, true);

                //Scale and position button
                memberButton.localScale = Vector3.one;
                memberButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);

                //Add button to list of buttons
                squadMemberButtons.Add(memberButton);
            }

            Text memberButtonText = memberButton.Find("Text").GetComponent<Text>();

            //Customize button
            memberButton.name = member.name + " Button";
            if(member.IsDead()) //Dead
            {
                memberButton.GetComponent<Button>().interactable = false;
                memberButtonText.text = (member == Player.player) ? member.name + " *  --  KIA" : member.name + "  --  KIA";
                memberButtonText.color = Color.gray;
                memberButtonText.fontStyle = (member == squad.leader) ? FontStyle.Bold : FontStyle.Normal;
            }
            else //Alive
            {
                memberButton.GetComponent<Button>().interactable = true;
                memberButtonText.text = (member == Player.player) ? member.name + " *" : member.name;
                memberButtonText.color = (member == squad.leader) ? Color.white : squad.GetArmy().color;
                memberButtonText.fontStyle = (member == squad.leader) ? FontStyle.Bold : FontStyle.Normal;
            }

            //Keep track of where next button will go vertically
            yPos -= buttonHeight;
        }

        //Adjust height of content window in scroll view to include added buttons (so scrollbar works properly)
        Vector2 sizeDelta = contentPane.GetComponent<RectTransform>().sizeDelta;
        sizeDelta.y = buttonHeight * squad.members.Count + buttonHeight / 3.0f;
        contentPane.GetComponent<RectTransform>().sizeDelta = sizeDelta;

        //Hide all extra buttons
        for(int x = squad.members.Count; x < squadMemberButtons.Count; x++)
        {
            Transform reserveButton = squadMemberButtons[x];

            //Indicate unused status
            reserveButton.name = "UNUSED BUTTON";
            reserveButton.Find("Text").GetComponent<Text>().text = "NOT USED";

            //Hide button
            reserveButton.gameObject.SetActive(false);
        }
    }

    private void UpdateSquadMemberDisplay(Pill member)
    {
        //Check for member
        if(!member)
        {
            BlankSquadMemberDisplay(Player.player.squad);
            return;
        }

        //Check for squad
        Squad squad = member.squad;
        if(!squad)
        {
            BlankSquadMemberDisplay(null);
            return;
        }

        //At this point, we have a member that belongs to a real squad so display his details...
        Transform squadMenu = pauseMenus[(int)MenuScreen.SquadMenu];

        //But first, hide this so it doesn't get in the way
        squadMenu.Find("Squad Details").GetComponent<Text>().text = "";

        //Set background and camera
        squadMenu.Find("Transparent Background").gameObject.SetActive(true);
        squadMenu.Find("Opaque Background").gameObject.SetActive(false);
        UpdateSquadMemberCamera(member.transform);

        //Set name
        squadMenu.Find("Member Name").GetComponent<Text>().text = member.name;

        //Set role
        if(member == squad.leader)
        {
            squadMenu.Find("Member Role").GetComponent<Text>().text = "Squad Leader";
            squadMenu.Find("Member Role").GetComponent<Text>().color = Color.white;
            squadMenu.Find("Member Role").GetComponent<Text>().fontStyle = FontStyle.Bold;

            squadMenu.Find("Send Transmission Input Field").gameObject.SetActive(true);
            squadMenu.Find("Send Transmission Button").gameObject.SetActive(true);
        }
        else
        {
            squadMenu.Find("Member Role").GetComponent<Text>().text = "Squadling";
            squadMenu.Find("Member Role").GetComponent<Text>().color = squad.GetArmy().color;
            squadMenu.Find("Member Role").GetComponent<Text>().fontStyle = FontStyle.Normal;

            squadMenu.Find("Send Transmission Input Field").gameObject.SetActive(false);
            squadMenu.Find("Send Transmission Button").gameObject.SetActive(false);
        }
        
        //Set details
        squadMenu.Find("Member Details").GetComponent<Text>().text = member.GetInfoDump();
        
        //Play banter
        if(member.voice && Random.Range(0, 2) == 0)
        {
            int picker = Random.Range(0, 3);
            switch(picker)
            {
                case 0: oneShotAudioSource.PlayOneShot(member.voice.GetEagerBanter(), 0.5f); break;
                case 1: oneShotAudioSource.PlayOneShot(member.voice.GetIdleBanter(), 0.5f); break;
                default: oneShotAudioSource.PlayOneShot(member.voice.GetCopy(), 0.5f); break;
            }
        }

        //Select button of displayed member
        Transform contentPane = squadMenu.Find("Squad Member Scroll View").Find("Viewport").Find("Content");
        Button memberButton = contentPane.Find(member.name + " Button").GetComponent<Button>();
        memberButton.Select(); //Select the button
        memberButton.OnSelect(null); //Highlight the button
    }

    //Pass in squad to show squad overview in place of member display, pass in null to get Davy Jones' locker display
    private void BlankSquadMemberDisplay(Squad squad)
    {
        Transform squadMenu = pauseMenus[(int)MenuScreen.SquadMenu];

        //Swith backgrounds
        squadMenu.Find("Opaque Background").gameObject.SetActive(true);
        squadMenu.Find("Transparent Background").gameObject.SetActive(false);

        //Switch cameras
        UpdateSquadMemberCamera(null);

        //Hide squad member text
        squadMenu.Find("Member Name").GetComponent<Text>().text = "";
        squadMenu.Find("Member Role").GetComponent<Text>().text = "";
        squadMenu.Find("Member Role").GetComponent<Text>().color = Color.white;
        squadMenu.Find("Member Details").GetComponent<Text>().text = "";
        squadMenu.Find("Send Transmission Input Field").gameObject.SetActive(false);
        squadMenu.Find("Send Transmission Button").gameObject.SetActive(false);

        //Display squad overview if we have a squad, otherwise indicate lack of a squad
        if (squad) //Show squad overview details
        {
            //Show squad overview
            squadMenu.Find("Squad Details").GetComponent<Text>().text =
            "Objective: " + "Defend Athens' Prescriptor"
            + "\n\nOrders: " + GeneralHelperMethods.GetEnumText(squad.GetOrders().ToString());

            //Show member count and squad member scroll view
            squadMenu.Find("Member Count").gameObject.SetActive(true);
            squadMenu.Find("Squad Member Scroll View").gameObject.SetActive(true);
        }
        else //Davy Jones' locker (no squad)
        {
            //No name
            pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Name").GetComponent<Text>().text = "Davy Jones' Locker";

            //Under the sea status
            squadMenu.Find("Squad Details").GetComponent<Text>().text = "You have been owned.";

            //Hide member count and squad member scroll view
            squadMenu.Find("Member Count").gameObject.SetActive(false);
            squadMenu.Find("Squad Member Scroll View").gameObject.SetActive(false);
        }
    }

    private void UpdateSquadMemberCamera(Transform member)
    {
        //If we don't have a camera for this (first time use), create one
        if (!squadMemberCamera)
            squadMemberCamera = Instantiate(memberCameraPrefab).transform;

        //Switch to squad member camera if valid squad member, else switch back
        God.god.SetActiveCamera(member ? squadMemberCamera.GetComponent<Camera>() : Player.player.GetCamera(), true);

        //We're done here if not a valid squad member
        if (!member)
            return;

        //Position and rotate camera...

        //Try preferred angles
        bool foundAngle = false;
        for(int attempt = 1; attempt <= 100; attempt++)
        {
            squadMemberCamera.position = member.TransformPoint(GetRandomLocalCameraAngle());
            squadMemberCamera.LookAt(member);

            if (SquadMemberCameraAngleGood(member))
            {
                foundAngle = true;
                break;
            }
        }
        if (foundAngle)
            return;

        //Resort to more desperate angles
        for (int attempt = 1; attempt <= 100; attempt++)
        {
            squadMemberCamera.position = member.TransformPoint(GetRandomLocalCameraAngleBackUp());
            squadMemberCamera.LookAt(member);

            if (SquadMemberCameraAngleGood(member))
            {
                foundAngle = true;
                break;
            }
        }
        if (foundAngle)
            return;

        //Sadness
        Debug.Log("Couldn't find valid camera angle! >:(");
    }

    //Used for squad member camera, returned position is local to squad member
    private Vector3 GetRandomLocalCameraAngle()
    {
        int picker = Random.Range(0, 5);

        switch (picker)
        {
            case 0: return new Vector3(0.0f, 0.5f, Random.Range(2.5f, 4.0f)); //Front
            case 1: return new Vector3(Random.Range(1.5f, 3.0f), 0.5f, Random.Range(1.5f, 3.0f)); //Right front
            case 2: return new Vector3(Random.Range(-1.5f, -3.0f), 0.5f, Random.Range(1.5f, 3.0f)); //Left front
            case 3: return new Vector3(Random.Range(-2.5f, -4.0f), 0.5f, 0.0f); //Left
            default: return new Vector3(Random.Range(2.5f, 4.0f), 0.5f, 0.0f); //Right
        }
    }

    //These angles are less ideal, but are used when preferred ones are obstructed
    private Vector3 GetRandomLocalCameraAngleBackUp()
    {
        int picker = Random.Range(0, 3);

        switch (picker)
        {
            case 0: return new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(2.0f, 4.0f),
                Random.Range(-0.25f, 0.25f)); //Top
            case 1: return new Vector3(Random.Range(-0.25f, 0.25f), 0.5f, Random.Range(-2.0f, -4.0f)); //Back
            default: return new Vector3(0.0f, Random.Range(0.0f, 0.5f), Random.Range(0.25f, 0.75f)); //Close front
        }
    }

    //Requires that position and rotation of squad member camera be set beforehand
    private bool SquadMemberCameraAngleGood(Transform member)
    {
        //Get list of everything in between camera and pill
        Vector3 startPosition = squadMemberCamera.TransformPoint(Vector3.back);
        float distance = Vector3.Distance(startPosition, member.position);

        RaycastHit[] hits = Physics.SphereCastAll(startPosition, 0.5f, squadMemberCamera.forward, distance,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        //If any object in between is not part of pill, then it is obstructing view so return that angle is bad
        foreach(RaycastHit hit in hits)
        {
            if (!hit.transform.IsChildOf(member))
                return false;
        }

        //Otherwise nothing is obstructing the view from the camera to the pill, so return that angle is good
        return true;
    }

    //Initializes commands menu if it has not already been initialized
    private void OnCommandsMenuOpen()
    {
        //If not already initialized, inititalize commands menu
        if (commandsMenuInitialized)
            return;
        commandsMenuInitialized = true;

        int commands = 3;

        //Locate all command dropdowns
        Dropdown[] dropdowns = new Dropdown[commands];
        for (int x = 1; x <= commands; x++)
            dropdowns[x - 1] = pauseMenus[(int)MenuScreen.CommandsMenu].Find("Command " + x + " Dropdown").GetComponent<Dropdown>();

        //Copy options from first dropdown into all other dropdowns
        for(int x = 1; x < commands; x++)
        {
            dropdowns[x].ClearOptions();
            dropdowns[x].AddOptions(dropdowns[0].options);
        }

        //Set currently selected option in each dropdown to reflect chosen command
        Player.player.RestoreOrderButtons(dropdowns[0], dropdowns[1], dropdowns[2]);
    }

    public bool IsPaused() { return paused; }
}
