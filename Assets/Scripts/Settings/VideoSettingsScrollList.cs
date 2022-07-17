using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsScrollList : MonoBehaviour
{
    [Header("Scroll List Option Components")]

    [SerializeField] private GameObject targetFrameRateOption = null;

    [Header("Dropdown Components")]

    [SerializeField] private Dropdown fullScreenModeDropdown = null;
    [SerializeField] private Dropdown resolutionDropdown = null;
    [SerializeField] private Dropdown targetFrameRateDropdown = null;
    [SerializeField] private Dropdown antiAliasingDropdown = null;

    [Header("Toggle Components")]

    [SerializeField] private Toggle vSyncToggle = null;
    [SerializeField] private Toggle fpsCounterToggle = null;

    [Header("Text Components")]

    [SerializeField] private Text vSyncToggleText = null;
    [SerializeField] private Text fpsCounterToggleText = null;

    [Header("Sprite Options")]

    [SerializeField] private Sprite unselectedDropdownOptionSprite = null;
    [SerializeField] private Sprite selectedDropdownOptionSprite = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip hoverButtonSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;
    [SerializeField] private AudioClip dropdownOptionHoverSFX = null;
    [SerializeField] private AudioClip dropdownOptionClickSFX = null;
    [SerializeField] private AudioClip toggleClickSFX = null;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the options of the fullscreen mode dropdown.
        List<string> fullScreenDropdownOptions = new List<string>();
        for(int enumIndex = 0; enumIndex < Enum.GetNames(typeof(FullScreenMode)).Length; enumIndex++)
        {
            fullScreenDropdownOptions.Add(GeneralHelperMethods.GetEnumText(((FullScreenMode)enumIndex).ToString()));
        }
        fullScreenModeDropdown.AddOptions(fullScreenDropdownOptions);

        //Updates the options of the resolution dropdown.
        List<string> resolutionDropdownOptions = new List<string>();
        for(int resolutionIndex = 0; resolutionIndex < VideoSettings.possibleResolutions.Length; resolutionIndex++)
        {
            resolutionDropdownOptions.Add(VideoSettings.possibleResolutions[resolutionIndex].x + "x" + VideoSettings.possibleResolutions[resolutionIndex].y);
        }
        resolutionDropdown.AddOptions(resolutionDropdownOptions);

        //Updates the options of the target frame rate dropdown.
        targetFrameRateDropdown.AddOptions(new List<string>(VideoSettings.possibleTargetFrameRateDropdownOptions));

        //Updates the options of the anti-aliasing (msaa) dropdown.
        List<string> antiAliasingDropdownOptions = new List<string>();
        for(int optionIndex = 0; optionIndex < VideoSettings.antiAliasingOptions.Length; optionIndex++)
        {
            antiAliasingDropdownOptions.Add(VideoSettings.antiAliasingOptions[optionIndex] == 0 ? "None" : VideoSettings.antiAliasingOptions[optionIndex].ToString() + "x");
        }
        antiAliasingDropdown.AddOptions(antiAliasingDropdownOptions);

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
        vSyncToggle.SetIsOnWithoutNotify(VideoSettings.vSyncEnabled);
        vSyncToggleText.text = vSyncToggle.isOn ? "Enabled" : "Disabled";
        targetFrameRateOption.SetActive(!vSyncToggle.isOn);
        targetFrameRateDropdown.SetValueWithoutNotify(VideoSettings.targetFrameRateIndex);
        antiAliasingDropdown.SetValueWithoutNotify(VideoSettings.antiAliasingIndex);
        fpsCounterToggle.SetIsOnWithoutNotify(VideoSettings.fpsCounterEnabled);
        fpsCounterToggleText.text = fpsCounterToggle.isOn ? "Enabled" : "Disabled";
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

    /// <summary>
    /// This method is called through an event trigger whenever the value of the target frame rate dropdown changes and updates the application's target frame rate.
    /// </summary>
    public void OnTargetFrameRateDropdownValueChange()
    {
        VideoSettings.targetFrameRate = VideoSettings.possibleTargetFrameRates[targetFrameRateDropdown.value];
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the anti-aliasing (msaa) dropdown changes and updates the application's anti-aliasing setting.
    /// </summary>
    public void OnAntiAliasingDropdownValueChange()
    {
        VideoSettings.antiAliasing = VideoSettings.antiAliasingOptions[antiAliasingDropdown.value];
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the vsync toggle changes and accomplishes the tasks of updating the application's vsync value, updating the toggle's text, and playing the appropriate sound effect.
    /// </summary>
    public void OnVSyncToggleValueChange()
    {
        //Updates the application's vsync value.
        VideoSettings.vSyncEnabled = vSyncToggle.isOn;

        //Updates the toggle's text.
        vSyncToggleText.text = vSyncToggle.isOn ? "Enabled" : "Disabled";
        //Updates whether or not the target frame rate dropdown option is active in the hierarchy.
        targetFrameRateOption.SetActive(!vSyncToggle.isOn);

        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(toggleClickSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the fps counter toggle changes and accomplishes the tasks of activating or deactivating the application's fps counter for the player to view and plays the appropriate sound effect.
    /// </summary>
    public void OnFPSCounterToggleValueChange()
    {
        //Updates the whether or not the application's fps counter is active and enabled.
        VideoSettings.fpsCounterEnabled = fpsCounterToggle.isOn;

        //Updates the toggle's text.
        fpsCounterToggleText.text = fpsCounterToggle.isOn ? "Enabled" : "Disabled";

        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(toggleClickSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a dropdown option and does the tasks of playing the appropriate sound effect and updating the background image's sprite.
    /// </summary>
    public void OnPointerEnterDropdownOption(Image backgroundImage)
    {
        //Updates the dropdown option's background image sprite.
        backgroundImage.sprite = selectedDropdownOptionSprite;

        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(dropdownOptionHoverSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer exits a dropdown option and updates the background image's sprite.
    /// </summary>
    public void OnPointerExitDropdownOption(Image backgroundImage)
    {
        backgroundImage.sprite = unselectedDropdownOptionSprite;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks on a dropdown option and plays the appropriate sound effect.
    /// </summary>
    public void OnClickDropdownOption()
    {
        AudioManager.PlaySFX(dropdownOptionClickSFX);
    }
}
