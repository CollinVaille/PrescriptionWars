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
            AudioSettings.MasterVolume = masterVolumeSlider.value / 100.0f;
            selectedText.text = masterVolumeSlider.value + "%";
        }
        if(type.Equals("Music Volume"))
        {
            AudioSettings.MusicVolume = musicVolumeSlider.value / 100.0f;
            selectedText.text = musicVolumeSlider.value + "%";
        }
        if(type.Equals("SFX Volume"))
        {
            AudioSettings.SFXVolume = sfxVolumeSlider.value / 100.0f;
            selectedText.text = sfxVolumeSlider.value + "%";
        }
    }

    //Updates the sliders to accurately reflect the master volume, music volume, and sfx volume.
    public void UpdateSliderValues()
    {
        masterVolumeSlider.value = Mathf.RoundToInt(AudioSettings.MasterVolume * 100.0f);
        musicVolumeSlider.value = Mathf.RoundToInt(AudioSettings.MusicVolume * 100.0f);
        sfxVolumeSlider.value = Mathf.RoundToInt(AudioSettings.SFXVolume * 100.0f);
    }

    public void SaveSettings()
    {
        AudioSettings.SaveSettings();
    }
}

public class AudioSettings
{
    private static bool loaded = false;
    public static bool Loaded
    {
        get
        {
            return loaded;
        }
    }

    private static float masterVolume;
    public static float MasterVolume
    {
        get
        {
            return masterVolume;
        }
        set
        {
            masterVolume = value;
            AudioManager.UpdateMasterVolume();
        }
    }

    private static float sfxVolume;
    public static float SFXVolume
    {
        get
        {
            return sfxVolume;
        }
        set
        {
            sfxVolume = value;
            AudioManager.UpdateSFXVolume();
        }
    }

    private static float musicVolume;
    public static float MusicVolume
    {
        get
        {
            return musicVolume;
        }
        set
        {
            musicVolume = value;
            AudioManager.UpdateMusicVolume();
        }
    }

    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("Master Volume", MasterVolume);
        PlayerPrefs.SetFloat("Music Volume", MusicVolume);
        PlayerPrefs.SetFloat("SFX Volume", SFXVolume);
        //PlayerPrefs.Save();
    }

    public static void LoadSettings()
    {
        if (Loaded)
            return;
        loaded = true;

        MasterVolume = PlayerPrefs.GetFloat("Master Volume", 1.0f);
        MusicVolume = PlayerPrefs.GetFloat("Music Volume", 1.0f);
        SFXVolume = PlayerPrefs.GetFloat("SFX Volume", 1.0f);
    }
}