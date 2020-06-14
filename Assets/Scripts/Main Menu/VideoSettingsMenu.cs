using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsMenu : MonoBehaviour
{
    public Dropdown fullscreenDropdown;
    public Dropdown resolutionDropdown;

    // Start is called before the first frame update
    void Start()
    {
        /*if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            fullscreenDropdown.value = 0;
        else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            fullscreenDropdown.value = 1;
        else if (Screen.fullScreenMode == FullScreenMode.MaximizedWindow)
            fullscreenDropdown.value = 2;
        else if (Screen.fullScreenMode == FullScreenMode.Windowed)
            fullscreenDropdown.value = 3;

        if (Screen.currentResolution.height == 720)
            resolutionDropdown.value = 0;
        else if (Screen.currentResolution.height == 1080)
            resolutionDropdown.value = 1;
        else if (Screen.currentResolution.height == 1440)
            resolutionDropdown.value = 2;
        else if (Screen.currentResolution.height == 2160)
            resolutionDropdown.value = 3;*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleFullScreen()
    {
        if(fullscreenDropdown.value == 0)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.ExclusiveFullScreen);
        }
        else if (fullscreenDropdown.value == 1)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
        }
        else if (fullscreenDropdown.value == 2)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.MaximizedWindow);
        }
        else if (fullscreenDropdown.value == 3)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.Windowed);
        }
    }

    public void ChangeResolution()
    {
        if(resolutionDropdown.value == 0)
        {
            Screen.SetResolution(1280, 720, Screen.fullScreenMode);
        }
        else if (resolutionDropdown.value == 1)
        {
            Screen.SetResolution(1920, 1080, Screen.fullScreenMode);
        }
        else if (resolutionDropdown.value == 2)
        {
            Screen.SetResolution(2560, 1440, Screen.fullScreenMode);
        }
        else if (resolutionDropdown.value == 3)
        {
            Screen.SetResolution(3840, 2160, Screen.fullScreenMode);
        }
    }

    public void LoadSettings()
    {
        VideoSettings.LoadSettings();

        fullscreenDropdown.value = VideoSettings.fullscreenMode;
        resolutionDropdown.value = VideoSettings.resolution;

        ToggleFullScreen();
        ChangeResolution();
    }

    public void SaveSettings()
    {
        VideoSettings.fullscreenMode = fullscreenDropdown.value;
        VideoSettings.resolution = resolutionDropdown.value;

        VideoSettings.SaveSettings();
    }
}

public class VideoSettings
{
    public static bool loaded = false;

    public static int sensitivity = 90;
    public static int viewDistance = 1000;
    public static int quality = 0;

    public static int fullscreenMode;
    public static int resolution;

    public static void SaveSettings()
    {
        PlayerPrefs.SetInt("Sensitivity", sensitivity);
        PlayerPrefs.SetInt("View Distance", viewDistance);
        PlayerPrefs.SetInt("Quality", quality);
        PlayerPrefs.SetInt("Fullscreen Mode", fullscreenMode);
        PlayerPrefs.SetInt("Resolution", resolution);
    }

    public static void LoadSettings()
    {
        if (loaded)
            return;

        loaded = true;

        sensitivity = PlayerPrefs.GetInt("Sensitivity", 90);
        viewDistance = PlayerPrefs.GetInt("View Distance", 1000);
        quality = PlayerPrefs.GetInt("Quality", 0);
        fullscreenMode = PlayerPrefs.GetInt("Fullscreen Mode", 3);
        resolution = PlayerPrefs.GetInt("Resolution", 0);
    }
}