﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetPauseMenu : MonoBehaviour
{
    public enum MenuScreen { SituationMenu = 0, LoadingScreen = 1, SquadMenu = 2, SettingsMenu = 3, MapMenu = 4 }
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

    //Audio
    private AudioSource oneShotAudioSource;
    public AudioClip mouseOver, buttonClick, pauseSound;

    //Loading screen
    public int loadingScreens = 2;
    private Sprite loadingScreenArtwork;
    private string loadingScreenTrivia = "";
    private static string[] triviaLines;

    //Squad
    private string squadLeader = "";

    //Map
    private bool mapOpen = false;
    public Material mapWater;
    private Material realWater;
    [HideInInspector] public Camera mapCamera;
    private float realDeltaTime = 0.0f, lastFrameTime = 0.0f;
    private float mapZoom = 0.0f;
    private Vector3 movementVector = Vector3.zero;
    private Vector3 previousMousePosition = Vector3.zero;
    private Vector3 mapPosition = Vector3.zero;

    private void Awake()
    {
        pauseMenu = this;

        //Variable initialization
        currentScreen = MenuScreen.SituationMenu;
        oneShotAudioSource = GetComponent<AudioSource>();
        mapCamera = God.god.GetComponent<Camera>();

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
        else if(Input.GetButtonDown("Squad Menu"))
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

        //Update map
        if (mapOpen)
        {
            realDeltaTime = Time.realtimeSinceStartup - lastFrameTime;

            //Map zoom
            mapZoom -= Input.GetAxis("Mouse ScrollWheel") * realDeltaTime * 20000;
            mapZoom = Mathf.Clamp(mapZoom, 100, 1000);
            mapCamera.orthographicSize = mapZoom;

            //Map click and drag movement
            if (Input.GetMouseButton(0))
            {
                //Get movement
                movementVector.x = (previousMousePosition.x - Input.mousePosition.x) * mapZoom / 10.0f;
                movementVector.z = (previousMousePosition.y - Input.mousePosition.y) * mapZoom / 10.0f;

                //Apply movement with boundaries in mind
                mapPosition += movementVector * realDeltaTime;
                mapPosition.x = Mathf.Clamp(mapPosition.x, -480, 1520);
                mapPosition.z = Mathf.Clamp(mapPosition.z, -480, 1520);
                God.god.transform.position = mapPosition;
            }

            //Update info for next frame
            previousMousePosition = Input.mousePosition;
            lastFrameTime = Time.realtimeSinceStartup;
        }

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

                //Show navigation bar
                navigationBar.gameObject.SetActive(true);

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

                God.god.OnResume();
            }
        }
    }

    public void SetMenuScreen(MenuScreen newScreen)
    {
        //Hide previous menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(false);

        //Perform any needed clean up with that previous screen
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
            SwitchToFromMap(false);
    }

    private void OnMenuScreenLoad(MenuScreen newScreen)
    {
        //Perform all needed menu screen initialization here

        //Load new screen
        if (newScreen == MenuScreen.SettingsMenu)
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
            SwitchToFromMap(true);
        else if (newScreen == MenuScreen.SquadMenu)
            UpdateSquadMenu();
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
                newLoadingScreen = Resources.Load<Sprite>("Loading Screens/Loading Screen " + Random.Range(1, loadingScreens + 1));
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
            Destroy(GetComponent<AudioListener>());
            mapCamera.enabled = false;

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
        else if (currentScreen == MenuScreen.SettingsMenu)
        {
            if (buttonName.Equals("Save Button"))
                ReadInSettingsFromDisplay();

            SetMenuScreen(MenuScreen.SituationMenu);
        }
        else if (currentScreen == MenuScreen.SquadMenu)
        {
            //Clicked on squad member button, so display details about squad member
            if (buttonTransform.parent.name.Equals("Content"))
                UpdateSquadMemberDisplay(Player.player.squad.GetMemberByName(
                    buttonName.Substring(0, buttonName.Length - 7)));
        }
    }

    public void OnButtonMouseOver(Transform buttonTransform)
    {
        if(buttonTransform.GetComponent<Button>().interactable)
            oneShotAudioSource.PlayOneShot(mouseOver);
    }

    private void BackOneScreen()
    {
        if (currentScreen == MenuScreen.SettingsMenu)
            SetMenuScreen(MenuScreen.SituationMenu);
        else
            Pause(false);
    }

    private void SwitchToFromMap(bool toMap)
    {
        Transform mapScreen = pauseMenus[(int)MenuScreen.MapMenu];

        //Toggle views
        mapCamera.enabled = toMap;
        if (Player.player)
            Player.player.SetCameraState(!toMap);

        if (toMap)
        {
            //Load map
            if (!mapOpen)
            {
                //Update status
                mapZoom = mapCamera.orthographicSize;
                lastFrameTime = Time.realtimeSinceStartup;
                mapPosition = God.god.transform.position;

                //Update water
                if (Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
                {
                    realWater = Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial;
                    mapWater.color = HUD.Find("Underwater").GetComponent<Image>().color;
                    Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = mapWater;
                }
            }
        }
        else if(mapOpen) //Unload map
        {
            //Update water
            if (Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
                Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = realWater;
        }

        //Finish toggling views
        mapOpen = toMap;
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
        Squad playerSquad = Player.player.squad;

        //Update squad name
        pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Name").GetComponent<Text>().text = playerSquad.name + " Squad";

        //Update squad member list
        UpdateSquadMemberList(playerSquad);

        //Blank squad member display since no member has been clicked on yet
        UpdateSquadMemberDisplay(null);
    }

    private void UpdateSquadMemberList(Squad playerSquad)
    {
        //float buttonHeight = squadMemberButton.GetComponent<RectTransform>().sizeDelta.y;
        float buttonHeight = 40;
        Transform contentPane = pauseMenus[(int)MenuScreen.SquadMenu].Find("Squad Member Scroll View")
            .Find("Viewport").Find("Content");

        //Get list of names of pre-existing member buttons
        List<string> priorMemberButtonNames = new List<string>();
        foreach (Transform t in contentPane)
            priorMemberButtonNames.Add(t.name);

        //Foreach current squad member, create new button if none pre-existing
        float yPos = -25;
        for (int x = 0; x < playerSquad.members.Count; x++)
        {
            Pill member = playerSquad.members[x];

            //Does button to represent member already exist?
            int preExistingIndex = priorMemberButtonNames.IndexOf(member.name + " Button");
            if (preExistingIndex >= 0)
            {
                //If so, just leave it and move onto next member
                //(But remember that we are still using that button)
                priorMemberButtonNames.RemoveAt(preExistingIndex);
                continue;
            }

            //Otherwise... create new button to represent member

            //Create and parent button
            Transform memberButton = Instantiate(squadMemberButton).transform;
            memberButton.SetParent(contentPane, true);

            //Scale and position button
            memberButton.localScale = Vector3.one;
            memberButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
            yPos -= buttonHeight;

            //Member name
            memberButton.name = member.name + " Button";
            memberButton.Find("Text").GetComponent<Text>().text = (member == Player.player) ?
                member.name + " *" : member.name;
        }

        //Once all buttons have been created, remove buttons for pills no longer in squad
        while (priorMemberButtonNames.Count > 0)
        {
            //Keep popping head of list until empty
            Destroy(contentPane.Find(priorMemberButtonNames[0]).gameObject);
            priorMemberButtonNames.RemoveAt(0);
        }

        //Adjust height of content window in scroll view to include added buttons (so scrollbar works properly)
        Vector2 sizeDelta = contentPane.GetComponent<RectTransform>().sizeDelta;
        sizeDelta.y = buttonHeight * playerSquad.members.Count + buttonHeight / 4.0f;
        contentPane.GetComponent<RectTransform>().sizeDelta = sizeDelta;

        //Special indicator for leader needs update
        if (!squadLeader.Equals(playerSquad.leader.name))
        {
            //Unmark old leader's button, if it still exists
            Transform oldButton = contentPane.Find(squadLeader + " Button");
            if(oldButton)
                oldButton.Find("Text").GetComponent<Text>().color = Color.white;

            //Mark new leader's button
            squadLeader = playerSquad.leader.name;
            contentPane.Find(squadLeader + " Button").Find("Text").GetComponent<Text>().color =
                playerSquad.GetArmy().color;
        }
    }

    private void UpdateSquadMemberDisplay(Pill member)
    {
        //Check for member
        if(!member)
        {
            BlankSquadMemberDisplay();
            return;
        }

        //Check for squad
        Squad squad = member.squad;
        if(!squad)
        {
            BlankSquadMemberDisplay();
            return;
        }

        //At this point, we have a member that belongs to a real squad so display his details...
        Transform squadMenu = pauseMenus[(int)MenuScreen.SquadMenu];

        squadMenu.Find("Member Name").GetComponent<Text>().text = member.name;

        if(member == squad.leader)
        {
            squadMenu.Find("Member Role").GetComponent<Text>().text = "Squad Leader";
            squadMenu.Find("Member Role").GetComponent<Text>().color = squad.GetArmy().color;
        }
        else
        {
            squadMenu.Find("Member Role").GetComponent<Text>().text = "Squadling";
            squadMenu.Find("Member Role").GetComponent<Text>().color = Color.white;
        }
        
        squadMenu.Find("Member Details").GetComponent<Text>().text = member.InfoDump();
    }

    private void BlankSquadMemberDisplay()
    {
        Transform squadMenu = pauseMenus[(int)MenuScreen.SquadMenu];

        squadMenu.Find("Member Name").GetComponent<Text>().text = "";
        squadMenu.Find("Member Role").GetComponent<Text>().text = "";
        squadMenu.Find("Member Role").GetComponent<Text>().color = Color.white;
        squadMenu.Find("Member Details").GetComponent<Text>().text = "";
    }
}
