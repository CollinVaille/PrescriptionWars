using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class God : MonoBehaviour
{
    public enum MenuScreen { PauseMenu = 0, LoadingScreen = 1, SquadMenu = 2 }

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

        //Show new menu screen
        pauseMenus[(int)currentScreen].gameObject.SetActive(true);
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
        }
        else
            Pause(false);
    }

    public void OnButtonMouseOver (Transform buttonTransform)
    {
        oneShotAudioSource.PlayOneShot(mouseOver);
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
