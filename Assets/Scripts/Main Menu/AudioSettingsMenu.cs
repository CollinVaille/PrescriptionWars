using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsMenu : GalaxyMenuBehaviour
{
    [Header("Audio Settings Menu")]

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private Text selectedText;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        UpdateSliderValues();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void SetText(Text text)
    {
        selectedText = text;
    }

    public void ChangeSliderValue(string type)
    {
        if (type.Equals("Master Volume"))
        {
            AudioSettings.masterVolume = masterVolumeSlider.value / 100.0f;
            selectedText.text = masterVolumeSlider.value + "%";
        }
        if(type.Equals("Music Volume"))
        {
            AudioSettings.musicVolume = musicVolumeSlider.value / 100.0f;
            selectedText.text = musicVolumeSlider.value + "%";
        }
        if(type.Equals("SFX Volume"))
        {
            AudioSettings.sfxVolume = sfxVolumeSlider.value / 100.0f;
            selectedText.text = sfxVolumeSlider.value + "%";
        }
    }

    //Updates the sliders to accurately reflect the master volume, music volume, and sfx volume.
    public void UpdateSliderValues()
    {
        masterVolumeSlider.value = Mathf.RoundToInt(AudioSettings.masterVolume * 100.0f);
        musicVolumeSlider.value = Mathf.RoundToInt(AudioSettings.musicVolume * 100.0f);
        sfxVolumeSlider.value = Mathf.RoundToInt(AudioSettings.sfxVolume * 100.0f);
    }

    public void SaveSettings()
    {
        AudioSettings.SaveSettings();
    }
}

public class AudioSettings
{
    /// <summary>
    /// Publicly accessible and privately mutateable bool that indicates whether or not the audio settings have been loaded in from playerprefs at all yet.
    /// </summary>
    public static bool loaded { get; private set; }

    /// <summary>
    /// Publicly accessible and mutateable float that indicates the master volume level (from 0-1) of both the sfx and music audio sources.
    /// </summary>
    public static float masterVolume
    {
        get
        {
            return masterVolumeVar;
        }
        set
        {
            masterVolumeVar = value;
            AudioManager.UpdateMasterVolume();
        }
    }
    private static float masterVolumeVar;

    /// <summary>
    /// Publicly accessible and mutateable float that indicates the volume level (from 0-1) of the sfx audio source, this number will be multiplied by the master volume to get the final effective volume of the sfx audio source.
    /// </summary>
    public static float sfxVolume
    {
        get
        {
            return sfxVolumeVar;
        }
        set
        {
            sfxVolumeVar = value;
            AudioManager.UpdateSFXVolume();
        }
    }
    private static float sfxVolumeVar;

    /// <summary>
    /// Publicly accessible and mutateable float that indicates the volume level (from 0-1) of the music audio source, this number will be multiplied by the master volume to get the final effective volume of the music audio source.
    /// </summary>
    public static float musicVolume
    {
        get
        {
            return musicVolumeVar;
        }
        set
        {
            musicVolumeVar = value;
            AudioManager.UpdateMusicVolume();
        }
    }
    private static float musicVolumeVar;

    /// <summary>
    /// Public static method that should be called in order to save the current audio settings to Unity's player prefs.
    /// </summary>
    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("Master Volume", masterVolumeVar);
        PlayerPrefs.SetFloat("Music Volume", musicVolumeVar);
        PlayerPrefs.SetFloat("SFX Volume", sfxVolumeVar);
    }

    /// <summary>
    /// Public static method that should be called at the start of every scene in order to load the player's preferred audio settings from Unity's player prefs.
    /// </summary>
    public static void LoadSettings()
    {
        if (loaded)
            return;
        loaded = true;

        masterVolume = PlayerPrefs.GetFloat("Master Volume", 1.0f);
        musicVolume = PlayerPrefs.GetFloat("Music Volume", 1.0f);
        sfxVolume = PlayerPrefs.GetFloat("SFX Volume", 1.0f);
    }
}