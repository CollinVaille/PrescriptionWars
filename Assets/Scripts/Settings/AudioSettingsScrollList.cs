using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsScrollList : MonoBehaviour
{
    [Header("Slider Components")]

    [SerializeField] private Slider masterVolumeSlider = null;
    [SerializeField] private Slider sfxVolumeSlider = null;
    [SerializeField] private Slider musicVolumeSlider = null;

    [Header("Text Components")]

    [SerializeField] private Text masterVolumeSliderText = null;
    [SerializeField] private Text sfxVolumeSliderText = null;
    [SerializeField] private Text musicVolumeSliderText = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip hoverButtonSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads all audio settings from the static audio settings class to the components in the scroll list.
    /// </summary>
    private void LoadSettings()
    {
        masterVolumeSlider.SetValueWithoutNotify(AudioSettings.masterVolume);
        masterVolumeSliderText.text = (int)(AudioSettings.masterVolume * 100) + "%";
        sfxVolumeSlider.SetValueWithoutNotify(AudioSettings.sfxVolume);
        sfxVolumeSliderText.text = (int)(AudioSettings.sfxVolume * 100) + "%";
        musicVolumeSlider.SetValueWithoutNotify(AudioSettings.musicVolume);
        musicVolumeSliderText.text = (int)(AudioSettings.musicVolume * 100) + "%";
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a button and plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterButton()
    {
        AudioManager.PlaySFX(hoverButtonSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks the save settings button and saves the audio settings and plays the appropriate sound effect for clicking the button.
    /// </summary>
    public void OnClickSaveSettingsButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Saves all audio settings.
        SaveSettings();
    }

    /// <summary>
    /// Saves all audio settings by calling the save settings static method of the audio settings class.
    /// </summary>
    private void SaveSettings()
    {
        AudioSettings.SaveSettings();
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the master volume slider changes and adjusts the text to the right of the slider as well as the application's master volume level appropriately.
    /// </summary>
    public void OnMasterVolumeSliderValueChange()
    {
        //Updates the text to the right of the slider.
        masterVolumeSliderText.text = (int)(masterVolumeSlider.value * 100) + "%";

        //Adjusts the application's master volume.
        AudioSettings.masterVolume = masterVolumeSlider.value;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the sfx volume slider changes and adjusts the text to the right of the slider as well as the application's sfx volume level appropriately.
    /// </summary>
    public void OnSFXVolumeSliderValueChange()
    {
        //Updates the text to the right of the slider.
        sfxVolumeSliderText.text = (int)(sfxVolumeSlider.value * 100) + "%";

        //Adjusts the application's master volume.
        AudioSettings.sfxVolume = sfxVolumeSlider.value;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the music volume slider changes and adjusts the text to the right of the slider as well as the application's music volume level appropriately.
    /// </summary>
    public void OnMusicVolumeSliderValueChange()
    {
        //Updates the text to the right of the slider.
        musicVolumeSliderText.text = (int)(musicVolumeSlider.value * 100) + "%";

        //Adjusts the application's master volume.
        AudioSettings.musicVolume = musicVolumeSlider.value;
    }
}
