using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsScrollList : MonoBehaviour
{
    [Header("Dropdown Components")]

    [SerializeField] private Dropdown fullScreenModeDropdown = null;
    [SerializeField] private Dropdown resolutionDropdown = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip hoverButtonSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the options of the fullscreen mode dropdown.
        List<string> fullScreenDropdownOptions = new List<string>();
        for(int enumIndex = 0; enumIndex < Enum.GetNames(typeof(FullScreenMode)).Length; enumIndex++)
        {
            fullScreenDropdownOptions.Add(((FullScreenMode)enumIndex).ToString());
        }
        fullScreenModeDropdown.AddOptions(fullScreenDropdownOptions);

        //Updates the options of the resolution dropdown.
        List<string> resolutionDropdownOptions = new List<string>();
        for(int resolutionIndex = 0; resolutionIndex < VideoSettings.possibleResolutions.Length; resolutionIndex++)
        {
            resolutionDropdownOptions.Add(VideoSettings.possibleResolutions[resolutionIndex].x + "x" + VideoSettings.possibleResolutions[resolutionIndex].y);
        }
        resolutionDropdown.AddOptions(resolutionDropdownOptions);

        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method is called through an event trigger whenever the user clicks on the save settings button and saves all video settings.
    /// </summary>
    public void OnClickSaveSettingsButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Saves all video settings.
        SaveSettings();
    }

    /// <summary>
    /// Saves all video settings by calling the save settings static method of the video settings class.
    /// </summary>
    private void SaveSettings()
    {
        VideoSettings.SaveSettings();
    }

    /// <summary>
    /// Loads all video settings from the static video settings class to the components in the scroll list.
    /// </summary>
    private void LoadSettings()
    {
        fullScreenModeDropdown.SetValueWithoutNotify((int)VideoSettings.fullScreenMode);
        resolutionDropdown.SetValueWithoutNotify(VideoSettings.resolutionIndex);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a button and plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterButton()
    {
        //Plays the sound effect for hovering over a button.
        AudioManager.PlaySFX(hoverButtonSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the fullscreen dropdown changes and updates the application's fullscreen mode.
    /// </summary>
    public void OnFullScreenDropdownValueChange()
    {
        VideoSettings.fullScreenMode = (FullScreenMode)fullScreenModeDropdown.value;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the resolution dropdown changes and updates the application's resolution.
    /// </summary>
    public void OnResolutionDropdownValueChange()
    {
        VideoSettings.resolution = VideoSettings.possibleResolutions[resolutionDropdown.value];
    }
}
