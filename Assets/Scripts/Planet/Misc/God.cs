using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class God : MonoBehaviour
{
    public enum MenuScreen { PauseMenu = 0, LoadingScreen = 1, SquadMenu = 2, SettingsMenu = 3, MapMenu = 4 }

    public static God god;
    private bool paused = false;

    //Menu screens
    private MenuScreen currentScreen;
    public Transform[] pauseMenus;
    public Transform HUD;

    //Audio
    private AudioSource oneShotAudioSource;
    public AudioClip mouseOver, buttonClick, pauseSound;
    public AudioClip deflection, jab, softItemImpact, hardItemImpact, genericImpact;

    //Audio Managment
    //Parallel lists for pause/resuming audio sources on pause menu
    private List<AudioSource> managedAudioSources;
    private List<bool> wasPlaying;

    //Projectile Management
    private List<Projectile> managedProjectiles;

    //Loading screen
    public int loadingScreens = 2;
    private Sprite loadingScreenArtwork;
    private string loadingScreenTrivia = "";
    private static string[] triviaLines;

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

    //Initialization
    private void Awake ()
    {
        god = this;

        //Initialize settings
        AudioSettings.LoadSettings();
        VideoSettings.LoadSettings();

        //Call for restart of all static initialization
        Vehicle.setUp = false;
        Voice.InitialSetUp();

        //Variable initialization
        currentScreen = MenuScreen.PauseMenu;
        oneShotAudioSource = GetComponents<AudioSource>()[0];
        managedAudioSources = new List<AudioSource>();
        wasPlaying = new List<bool>();
        managedProjectiles = new List<Projectile>();
        mapCamera = GetComponent<Camera>();

        //Refresh pause state to start out as resumed
        paused = true;
        Pause(false);

        //Begin loading...
        LoadingScreen(true, false);
    }

    //Delayed Initialization
    private void Start ()
    {
        StartCoroutine(ManageProjectiles());
    }

    private void Update ()
    {
        //Can pause/resume with pause button on keyboard
        if (Input.GetButtonDown("Pause"))
        {
            if (paused)
                BackOneScreen();
            else
                Pause(true);
        }

        //Update map
        if(mapOpen)
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
                transform.position = mapPosition;
            }

            //Update info for next frame
            previousMousePosition = Input.mousePosition;
            lastFrameTime = Time.realtimeSinceStartup;
        }
    }

    public void Pause (bool pause)
    {
        if(pause)
        {
            if(!paused) //Pause game
            {
                paused = true;

                //Pause time
                Time.timeScale = 0;

                //Enable cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                //Show pause menu
                SetMenuScreen(MenuScreen.PauseMenu);

                //Play pause sound effect
                oneShotAudioSource.PlayOneShot(pauseSound);

                //Pause audio sources and keep track of which ones we paused so we can resume them later
                for (int x = 0; x < managedAudioSources.Count; x++)
                {
                    if (managedAudioSources[x].isPlaying)
                    {
                        managedAudioSources[x].Pause();
                        wasPlaying[x] = true;
                    }
                    else
                        wasPlaying[x] = false;
                }
            }
        }
        else
        {
            if(paused) //Resume game
            {
                paused = false;

                //Resume time
                Time.timeScale = 1;

                //Disable cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                //Hide pause menu
                pauseMenus[(int)currentScreen].gameObject.SetActive(false);

                //Resume audio sources that were playing before being paused
                for (int x = 0; x < managedAudioSources.Count; x++)
                {
                    if (wasPlaying[x])
                        managedAudioSources[x].Play();
                }
            }
        }
    }

    public void SetMenuScreen (MenuScreen newScreen)
    {
        //Hide previous menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(false);

        //Initialize new menu screen contents
        OnMenuScreenLoad(newScreen, currentScreen);

        //Set new menu screen
        currentScreen = newScreen;

        //Show new menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(true);
    }

    private void OnMenuScreenLoad (MenuScreen newScreen, MenuScreen oldScreen)
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
    }

    public void LoadingScreen (bool show, bool generateNew)
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

    private string GetRandomTrivia ()
    {
        //Get list of trivia lines (only need to do this on first loading screen in session)
        if(triviaLines == null)
        {
            TextAsset triviaLinesFile = Resources.Load<TextAsset>("Text/Trivia Lines");
            triviaLines = triviaLinesFile.text.Split('\n');
        }

        //Pick a random name
        return triviaLines[Random.Range(0, triviaLines.Length)];
    }

    public void OnButtonClick (Transform buttonTransform)
    {
        oneShotAudioSource.PlayOneShot(buttonClick);

        string buttonName = buttonTransform.name;

        //Special management of back button
        if (buttonName.Equals("Back Button"))
            BackOneScreen();

        //Management of all other buttons
        if (currentScreen == MenuScreen.PauseMenu)
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

            SetMenuScreen(MenuScreen.PauseMenu);
        }
        else if (currentScreen == MenuScreen.SquadMenu)
        {
            if (buttonName.Equals("Map Button"))
                SetMenuScreen(MenuScreen.MapMenu);
        }
    }

    public void OnButtonMouseOver (Transform buttonTransform)
    {
        oneShotAudioSource.PlayOneShot(mouseOver);
    }

    private void BackOneScreen ()
    {
        if (currentScreen == MenuScreen.SettingsMenu)
            SetMenuScreen(MenuScreen.PauseMenu);
        else if (currentScreen == MenuScreen.MapMenu)
            SwitchToFromMap(false);
        else
            Pause(false);
    }

    private void SwitchToFromMap (bool toMap)
    {
        Transform mapScreen = pauseMenus[(int)MenuScreen.MapMenu];

        //Toggle views
        mapOpen = toMap;
        HUD.gameObject.SetActive(!toMap);
        mapCamera.enabled = toMap;
        if (Player.player)
            Player.player.SetCameraState(!toMap);
        Pause(toMap);

        if (mapOpen) //Load map
        {
            //Update status
            mapZoom = mapCamera.orthographicSize;
            lastFrameTime = Time.realtimeSinceStartup;
            mapPosition = transform.position;

            //Update map view
            mapScreen.Find("Planet Name").GetComponent<Text>().text = Planet.planet.planetName;

            //Update water
            if(Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
            {
                realWater = Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial;
                mapWater.color = HUD.Find("Underwater").GetComponent<Image>().color;
                Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = mapWater;
            }

            //Makes back button rendered above map markers
            mapScreen.Find("Back Button").SetAsLastSibling();
        }
        else //Unload map
        {
            //Update water
            if (Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
                Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = realWater;
        }
    }

    private void ReadInSettingsFromDisplay ()
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

    public void ManageAudioSource (AudioSource toManage)
    {
        managedAudioSources.Add(toManage);
        wasPlaying.Add(false);
    }

    public void UnmanageAudioSource (AudioSource toUnmanage)
    {
        int parallelIndex = managedAudioSources.IndexOf(toUnmanage);

        managedAudioSources.RemoveAt(parallelIndex);
        wasPlaying.RemoveAt(parallelIndex);
    }

    public static void InitializeAudioList (List<AudioClip> audioList, string resourcesPath)
    {
        for (int x = 1; x <= 10000; x++)
        {
            AudioClip newClip = Resources.Load<AudioClip>(resourcesPath + x);

            if (newClip)
                audioList.Add(newClip);
            else
                break;
        }
    }

    public static AudioClip RandomClip (List<AudioClip> audioList)
    {
        return audioList[Random.Range(0, audioList.Count)];
    }

    public static Damageable GetDamageable (Transform t)
    {
        if (t.GetComponent<Damageable>() != null)
            return t.GetComponent<Damageable>();
        else if (t.parent)
            return GetDamageable(t.parent);
        else
            return null;
    }

    public static string SpaceOutString (string toSpaceOut)
    {
        char[] original = toSpaceOut.ToCharArray();

        int newSize = original.Length;

        //Compute size of new array by incrementing it everytime we find a place to add a space character
        for(int x = 0; x < original.Length; x++)
        {
            if (x != 0 && original[x] >= 65 && original[x] <= 90)
                newSize++;
        }

        char[] modified = new char[newSize];

        //Create new string as char array
        int newIndex = 0;
        for (int oldIndex = 0; oldIndex < original.Length; oldIndex++, newIndex++)
        {
            //Add space
            if (oldIndex != 0 && original[oldIndex] >= 65 && original[oldIndex] <= 90)
            {
                modified[newIndex] = ' ';
                newIndex++;
            }

            //Copy character over
            modified[newIndex] = original[oldIndex];
        }

        return new string(modified);
    }

    private IEnumerator ManageProjectiles ()
    {
        float stepTime = 0.033f;

        float lastTime = Time.timeSinceLevelLoad;
        float actualStepTime = stepTime;

        while(true)
        {
            yield return new WaitForSeconds(stepTime);

            actualStepTime = Time.timeSinceLevelLoad - lastTime;

            for(int x = 0; x < managedProjectiles.Count; x++)
            {
                //Update projectile
                Projectile original = managedProjectiles[x];
                original.UpdateLaunchedProjectile(actualStepTime);

                //If projectile was removed from list during update, adjust accordingly
                if (x == managedProjectiles.Count || original != managedProjectiles[x])
                    x--;
            }

            lastTime = Time.timeSinceLevelLoad;
        }
    }

    public void ManageProjectile (Projectile projectile) { managedProjectiles.Add(projectile); }

    public void UnmanageProjectile (Projectile projectile) { managedProjectiles.Remove(projectile); }
}
