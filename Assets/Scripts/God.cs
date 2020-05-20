using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class God : MonoBehaviour
{
    public enum MenuScreen { PauseMenu = 0, LoadingScreen = 1, SquadMenu = 2, SettingsMenu = 3 }

    public static God god;
    private bool paused = false;

    //Menu screens
    private MenuScreen currentScreen;
    public Transform[] pauseMenus;
    public Transform canvas;

    //Audio
    private AudioSource oneShotAudioSource;
    public AudioClip mouseOver, buttonClick, pauseSound;
    public AudioClip deflection, jab, softItemImpact, hardItemImpact, genericImpact;

    //Parallel lists for pause/resuming audio sources on pause menu
    private List<AudioSource> managedAudioSources;
    private List<bool> wasPlaying;

    //Loading screen
    public int loadingScreens = 2;
    private Sprite loadingScreenArtwork;
    private string loadingScreenTrivia = "";
    private static string[] triviaLines;

    //Initialization
    void Awake ()
    {
        god = this;
        Planet.planet = GetComponent<Planet>();

        //Initialize display settings
        if (!DisplaySettings.loaded)
            DisplaySettings.LoadSettings();

        //Variable initialization
        currentScreen = MenuScreen.PauseMenu;
        oneShotAudioSource = GetComponents<AudioSource>()[0];
        managedAudioSources = new List<AudioSource>();
        wasPlaying = new List<bool>();

        //Refresh pause state to start out as resumed
        paused = true;
        Pause(false);

        //Begin loading...
        LoadingScreen(true, false);
    }

    void Update ()
    {
        //Can pause/resume with pause button on keyboard
        if (Input.GetButtonDown("Pause"))
            Pause(!paused);
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

        //Set new menu screen
        currentScreen = newScreen;

        //Initialize new menu screen contents
        OnMenuScreenLoad(newScreen);

        //Show new menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(true);
    }

    private void OnMenuScreenLoad (MenuScreen newScreen)
    {
        //Perform all needed menu screen initialization here

        if(newScreen == MenuScreen.SettingsMenu)
        {
            Transform menu = pauseMenus[(int)newScreen];

            //Set input fields
            menu.Find("Sensitivity Input Field").GetComponent<InputField>().text = DisplaySettings.sensitivity.ToString();
            menu.Find("View Distance Input Field").GetComponent<InputField>().text = DisplaySettings.viewDistance.ToString();

            //Set quality dropdown options
            Dropdown qualityDropdown = menu.Find("Quality Dropdown").GetComponent<Dropdown>();
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            qualityDropdown.value = DisplaySettings.quality;
        }
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
            //Done with loading which means player has been loaded in so get rid of placeholder camera
            Destroy(GetComponent<AudioListener>());
            Destroy(GetComponent<Camera>());

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

        if(currentScreen == MenuScreen.PauseMenu)
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
        else if(currentScreen == MenuScreen.SettingsMenu)
        {
            if (buttonName.Equals("Save Button"))
                ReadInSettingsFromDisplay();

            SetMenuScreen(MenuScreen.PauseMenu);
        }
        else
            Pause(false);
    }

    public void OnButtonMouseOver (Transform buttonTransform)
    {
        oneShotAudioSource.PlayOneShot(mouseOver);
    }

    private void ReadInSettingsFromDisplay ()
    {
        Transform menu = pauseMenus[(int)currentScreen];

        //Sensitivity
        if (!int.TryParse(menu.Find("Sensitivity Input Field").GetComponent<InputField>().text, out int sensitivity))
            sensitivity = 90;
        DisplaySettings.sensitivity = Mathf.Clamp(sensitivity, 1, 10000);

        //View distance
        if (!int.TryParse(menu.Find("View Distance Input Field").GetComponent<InputField>().text, out int viewDistance))
            viewDistance = 1000;
        DisplaySettings.viewDistance = Mathf.Clamp(viewDistance, 1, 100000);

        //Quality
        DisplaySettings.quality = menu.Find("Quality Dropdown").GetComponent<Dropdown>().value;

        //Save and apply
        DisplaySettings.SaveSettings();
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
}

public class DisplaySettings
{
    public static bool loaded = false;

    public static int sensitivity = 90;
    public static int viewDistance = 1000;
    public static int quality = 0;

    public static void SaveSettings ()
    {
        PlayerPrefs.SetInt("Sensitivity", sensitivity);
        PlayerPrefs.SetInt("View Distance", viewDistance);
        PlayerPrefs.SetInt("Quality", quality);
    }

    public static void LoadSettings ()
    {
        loaded = true;

        sensitivity = PlayerPrefs.GetInt("Sensitivity", 90);
        viewDistance = PlayerPrefs.GetInt("View Distance", 1000);
        quality = PlayerPrefs.GetInt("Quality", 0);
    }
}
