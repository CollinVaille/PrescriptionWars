using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VideoSettingsMenu : GalaxyMenuBehaviour
{
    [Header("Video Settings Menu")]

    public Dropdown fullscreenDropdown;
    public Dropdown resolutionDropdown;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        LoadSettings();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void ToggleFullScreen()
    {
        VideoSettings.fullScreenMode = (FullScreenMode)fullscreenDropdown.value;
    }

    public void ChangeResolution()
    {
        if (resolutionDropdown.value == 0)
            VideoSettings.resolution = new Vector2Int(1280, 720);
        else if (resolutionDropdown.value == 1)
            VideoSettings.resolution = new Vector2Int(1920, 1080);
        else if (resolutionDropdown.value == 2)
            VideoSettings.resolution = new Vector2Int(2560, 1440);
        else if (resolutionDropdown.value == 3)
            VideoSettings.resolution = new Vector2Int(3840, 2160);
    }

    private void LoadSettings()
    {
        fullscreenDropdown.value = (int)VideoSettings.fullScreenMode;
        if (VideoSettings.resolution.x == 1280 && VideoSettings.resolution.y == 720)
            resolutionDropdown.value = 0;
        else if (VideoSettings.resolution.x == 1920 && VideoSettings.resolution.y == 1080)
            resolutionDropdown.value = 1;
        else if (VideoSettings.resolution.x == 2560 && VideoSettings.resolution.y == 1440)
            resolutionDropdown.value = 2;
        else if (VideoSettings.resolution.x == 3840 && VideoSettings.resolution.y == 2160)
            resolutionDropdown.value = 3;
    }

    public void SaveSettings()
    {
        VideoSettings.SaveSettings();
    }
}





public static class VideoSettings
{
    public static bool loaded { get => loadedVar; private set => loadedVar = value; }
    private static bool loadedVar = false;

    public static int sensitivity = 90;
    public static int viewDistance = 1000;
    public static int quality = 0;

    //Fullscreen mode.
    /// <summary>
    /// Enum property that should be used to both access and modify the fullscreen mode of the application.
    /// </summary>
    public static FullScreenMode fullScreenMode { get => fullScreenModeVar; set { fullScreenModeVar = value; Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, value); resolution = resolution; } }
    private static FullScreenMode fullScreenModeVar;

    //Resolution
    /// <summary>
    /// Property that should be used to both access and modify the resolution of the application.
    /// </summary>
    public static Vector2Int resolution
    {
        get => possibleResolutions[resolutionIndex];
        set
        {
            int correspondingResolutionIndex = -1;
            for(int tempResolutionIndex = 0; tempResolutionIndex < possibleResolutions.Length; tempResolutionIndex++)
            {
                if(possibleResolutions[tempResolutionIndex].x == value.x && possibleResolutions[tempResolutionIndex].y == value.y)
                {
                    correspondingResolutionIndex = tempResolutionIndex;
                    break;
                }
            }
            if (correspondingResolutionIndex < 0 || correspondingResolutionIndex >= possibleResolutions.Length)
            {
                Debug.LogWarning("Resolution: " + value.x + "x" + value.y + " was not found in the array of possible resolutions, resolution remains unchanged.");
                return;
            }
            resolutionIndex = correspondingResolutionIndex;
            Screen.SetResolution(possibleResolutions[resolutionIndex].x, possibleResolutions[resolutionIndex].y, Screen.fullScreenMode);
        }
    }
    /// <summary>
    /// Readonly array that contains all possible resolutions of the application.
    /// </summary>
    public static readonly Vector2Int[] possibleResolutions = new Vector2Int[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160) };
    /// <summary>
    /// Indicates which index in the array of possible resolutions in the current resolution of the application.
    /// </summary>
    public static int resolutionIndex { get; private set; }

    //VSync.
    /// <summary>
    /// Property that should be used to both access and modify if the application has vsync enabled.
    /// </summary>
    public static bool vSyncEnabled { get => vSyncCount > 0; set { vSyncCount = value ? 1 : 0; QualitySettings.vSyncCount = vSyncCount; Application.targetFrameRate = vSyncEnabled ? -1 : targetFrameRateVar; } }
    private static int vSyncCount = 0;

    //Target Frame Rate.
    /// <summary>
    /// Readonly array that contains all target frame rates that have been chosen to be supported (mostly due to being common refresh rates).
    /// </summary>
    public static readonly int[] supportedTargetFrameRates = new int[] { -1, 24, 30, 60, 75, 120, 144, 165, 240, 360 };
    /// <summary>
    /// Array that contains all possible target frame rates depending on the current monitor's refresh rate.
    /// </summary>
    public static int[] possibleTargetFrameRates
    {
        get
        {
            List<int> possibleTargetFrameRatesList = new List<int>();
            for(int supportedTargetFrameRateIndex = 0; supportedTargetFrameRateIndex < supportedTargetFrameRates.Length; supportedTargetFrameRateIndex++)
            {
                if (supportedTargetFrameRates[supportedTargetFrameRateIndex] > Screen.currentResolution.refreshRate)
                    break;
                possibleTargetFrameRatesList.Add(supportedTargetFrameRates[supportedTargetFrameRateIndex]);
            }
            return possibleTargetFrameRatesList.ToArray();
        }
    }
    /// <summary>
    /// Possible target frame rates int array converted to a string array with -1 being changed to unlimited.
    /// </summary>
    public static string[] possibleTargetFrameRateDropdownOptions
    {
        get
        {
            int[] possibleTargetFrameRatesArr = possibleTargetFrameRates;
            List<string> dropdownOptionsList = new List<string>();
            for(int possibleTargetFrameRateIndex = 0; possibleTargetFrameRateIndex < possibleTargetFrameRatesArr.Length; possibleTargetFrameRateIndex++)
            {
                dropdownOptionsList.Add(possibleTargetFrameRatesArr[possibleTargetFrameRateIndex] >= 0 ? possibleTargetFrameRatesArr[possibleTargetFrameRateIndex].ToString() : "Unlimited");
            }
            return dropdownOptionsList.ToArray();
        }
    }
    /// <summary>
    /// Property that should be used to both access and mutate the target frame rate of the application.
    /// The accessor returns -1 if vsync is enabled and the actual target frame rate if vsync is disabled.
    /// The mutator checks if the specified target frame rate is possible first, then it sets the private variable equal to the specified value, then if vsync is enabled it logs a warning saying that changes will not be applied until vsync is disabled or if vsync is disabled then it applies the specified target frame rate immediately.
    /// </summary>
    public static int targetFrameRate
    {
        get => vSyncEnabled ? -1 : targetFrameRateVar;
        set
        {
            if(!new List<int>(possibleTargetFrameRates).Contains(value))
            {
                Debug.LogWarning(value.ToString() + " is not a valid target frame rate. Target frame rate value remains unchanged.");
                return;
            }
            targetFrameRateVar = value;
            if (vSyncEnabled)
                Debug.LogWarning("Target frame rate set to " + value + ", however actual change will not take effect while vsync is enabled.");
            else
                Application.targetFrameRate = value;
        }
    }
    /// <summary>
    /// Access only property that indicates at exactly which index in the array of possible target frame rates that the current target frame rate located at (returns -1 if current target frame rate could not be found within the possible target frame rates array).
    /// </summary>
    public static int targetFrameRateIndex
    {
        get
        {
            int targetFrameRateIndexVar = -1;
            for(int index = 0; index < possibleTargetFrameRates.Length; index++)
            {
                if(targetFrameRate == possibleTargetFrameRates[index])
                {
                    targetFrameRateIndexVar = index;
                    break;
                }
            }
            return targetFrameRateIndexVar;
        }
    }
    /// <summary>
    /// Private static variable that holds the user's specified target frame rate (thus will not match the property's value when vsync is enabled).
    /// </summary>
    private static int targetFrameRateVar = -1;

    /// <summary>
    /// Readonly array that contains all anti-aliasing (msaa) values that have been chosen to be supported (mostly due to Unity and hardware limitations).
    /// </summary>
    public static readonly int[] antiAliasingOptions = new int[] { 0, 2, 4, 8 };
    /// <summary>
    /// Property that should be used both to access and mutate the anti-aliasing (msaa) value of the application. Does not set the variable to the specified value if the specified anti-aliasing value is not a valid option from the readonly array of options.
    /// </summary>
    public static int antiAliasing
    {
        get => antiAliasingVar;
        set
        {
            if (!new List<int>(antiAliasingOptions).Contains(value))
            {
                Debug.LogWarning(value.ToString() + " is not a valid anti-aliasing (msaa) option. Anti-aliasing (msaa) value remains unchanged.");
                return;
            }
            antiAliasingVar = value;
            QualitySettings.antiAliasing = value;
        }
    }
    /// <summary>
    /// Access only property that indicates which index in the readonly array of anti-aliasing options currently corresponds to the anti-aliasing value of the application. Returns -1 if no such index is found.
    /// </summary>
    public static int antiAliasingIndex
    {
        get
        {
            int antiAliasingIndexVar = -1;
            for (int index = 0; index < antiAliasingOptions.Length; index++)
            {
                if (antiAliasing == antiAliasingOptions[index])
                {
                    antiAliasingIndexVar = index;
                    break;
                }
            }
            return antiAliasingIndexVar;
        }
    }
    /// <summary>
    /// Private static variable that holds the user's specified anti-aliasing (msaa) value.
    /// </summary>
    private static int antiAliasingVar = 0;

    /// <summary>
    /// Property that should be used both to access and mutate the value that indicates whether or not the application's fps counter is enabled.
    /// </summary>
    public static bool fpsCounterEnabled
    {
        get => fpsCounterEnabledVar;
        set
        {
            if(!fpsCounterEnabledVar && value)
            {
                Text fpsCounterText = GameObject.Instantiate(Resources.Load<GameObject>("General/Video Settings Prefabs/FPS Counter Text").GetComponent<Text>());
                fpsCounterText.transform.SetParent(canvas.transform);
                fpsCounterText.transform.localPosition = new Vector3(Mathf.Abs(fpsCounterText.transform.localPosition.x), Mathf.Abs(fpsCounterText.transform.localPosition.y), Mathf.Abs(fpsCounterText.transform.localPosition.z));
                fpsCounterText.gameObject.name = "FPS Counter Text";
            }
            else if (fpsCounterEnabledVar && !value)
            {
                GameObject.Destroy(canvas.transform.Find("FPS Counter Text").gameObject);
            }
            fpsCounterEnabledVar = value;
        }
    } 
    /// <summary>
    /// Private static variable that holds the user's preference for whether or not the application's fps counter is enabled.
    /// </summary>
    private static bool fpsCounterEnabledVar = false;

    /// <summary>
    /// Private static string the holds the name of the previously active scene (string.empty if there was no previously active scene).
    /// </summary>
    private static string previousSceneName = string.Empty;
    /// <summary>
    /// Private static variable that holds the canvas that displays all needed video information to the user.
    /// </summary>
    private static Canvas canvas = null;

    /// <summary>
    /// Public static method that should be called in order to save the current video settings to Unity's player prefs.
    /// </summary>
    public static void SaveSettings()
    {
        PlayerPrefs.SetInt("Sensitivity", sensitivity);
        PlayerPrefs.SetInt("View Distance", viewDistance);
        PlayerPrefs.SetInt("Quality", quality);
        PlayerPrefs.SetInt("Fullscreen Mode", (int)fullScreenMode);
        PlayerPrefs.SetInt("Resolution Index", resolutionIndex);
        PlayerPrefs.SetInt("VSync", vSyncCount);
        PlayerPrefs.SetInt("Target Frame Rate", targetFrameRateVar);
        PlayerPrefs.SetInt("Anti-aliasing", antiAliasingVar);
        PlayerPrefs.SetInt("FPS Counter", fpsCounterEnabledVar ? 1 : 0);
    }

    /// <summary>
    /// Public static method that should be called at the start of every scene in order to load the player's preferred video settings from Unity's player prefs.
    /// </summary>
    public static void LoadSettings()
    {
        if (!previousSceneName.Equals(SceneManager.GetActiveScene().name))
        {
            canvas = GameObject.Instantiate(Resources.Load<GameObject>("General/Video Settings Prefabs/Video Settings Canvas")).GetComponent<Canvas>();
            canvas.transform.SetParent(Camera.main.transform.parent);
            canvas.gameObject.name = "Video Settings Canvas";
            canvas.transform.SetAsLastSibling();
            canvas.gameObject.SetActive(false);
            canvas.gameObject.SetActive(true);

            if (fpsCounterEnabled)
            {
                Text fpsCounterText = GameObject.Instantiate(Resources.Load<GameObject>("General/Video Settings Prefabs/FPS Counter Text").GetComponent<Text>());
                fpsCounterText.transform.SetParent(canvas.transform);
                fpsCounterText.transform.localPosition = new Vector3(Mathf.Abs(fpsCounterText.transform.localPosition.x), Mathf.Abs(fpsCounterText.transform.localPosition.y), Mathf.Abs(fpsCounterText.transform.localPosition.z));
                fpsCounterText.gameObject.name = "FPS Counter Text";
            }
        }
        previousSceneName = SceneManager.GetActiveScene().name;

        if (loaded)
            return;

        loaded = true;

        sensitivity = PlayerPrefs.GetInt("Sensitivity", 90);
        viewDistance = PlayerPrefs.GetInt("View Distance", 1000);
        quality = PlayerPrefs.GetInt("Quality", 0);
        fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("Fullscreen Mode", 3);
        resolution = possibleResolutions[PlayerPrefs.GetInt("Resolution Index", 0)];
        vSyncEnabled = PlayerPrefs.GetInt("VSync", 0) > 0;
        targetFrameRate = PlayerPrefs.GetInt("Target Frame Rate", -1);
        antiAliasing = PlayerPrefs.GetInt("Anti-aliasing", 0);
        fpsCounterEnabled = PlayerPrefs.GetInt("FPS Counter", 0) == 1;
    }
}