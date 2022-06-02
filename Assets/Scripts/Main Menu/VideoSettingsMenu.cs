using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

public class VideoSettings
{
    public static bool loaded { get => loadedVar; private set => loadedVar = value; }
    private static bool loadedVar = false;

    public static int sensitivity = 90;
    public static int viewDistance = 1000;
    public static int quality = 0;

    /// <summary>
    /// Enum property that should be used to both access and modify the fullscreen mode of the application.
    /// </summary>
    public static FullScreenMode fullScreenMode { get => fullScreenModeVar; set { fullScreenModeVar = value; Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, value); resolution = resolution; } }
    private static FullScreenMode fullScreenModeVar;

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

    public static void SaveSettings()
    {
        PlayerPrefs.SetInt("Sensitivity", sensitivity);
        PlayerPrefs.SetInt("View Distance", viewDistance);
        PlayerPrefs.SetInt("Quality", quality);
        PlayerPrefs.SetInt("Fullscreen Mode", (int)fullScreenMode);
        PlayerPrefs.SetInt("Resolution Index", resolutionIndex);
    }

    public static void LoadSettings()
    {
        if (loaded)
            return;

        loaded = true;

        sensitivity = PlayerPrefs.GetInt("Sensitivity", 90);
        viewDistance = PlayerPrefs.GetInt("View Distance", 1000);
        quality = PlayerPrefs.GetInt("Quality", 0);
        fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("Fullscreen Mode", 3);
        resolution = possibleResolutions[PlayerPrefs.GetInt("Resolution Index", 0)];
    }
}