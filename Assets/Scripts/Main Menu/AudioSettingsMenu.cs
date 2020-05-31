using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsMenu : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    Text selectedText;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetText(Text text)
    {
        selectedText = text;
    }

    public void ChangeSliderValue(string type)
    {
        if (type.Equals("Master Volume"))
        {
            AudioListener.volume = masterVolumeSlider.value / 100.0f;
            selectedText.text = masterVolumeSlider.value + "%";
        }
        if(type.Equals("Music Volume"))
        {
            musicSource.volume = musicVolumeSlider.value / 100.0f;
            selectedText.text = musicVolumeSlider.value + "%";
        }
        if(type.Equals("SFX Volume"))
        {
            sfxSource.volume = sfxVolumeSlider.value / 100.0f;
            selectedText.text = sfxVolumeSlider.value + "%";
        }
    }

    public void LoadSettings()
    {
        AudioSettings.LoadSettings();

        AudioListener.volume = AudioSettings.masterVolume;
        musicSource.volume = AudioSettings.musicVolume;
        sfxSource.volume = AudioSettings.sfxVolume;

        masterVolumeSlider.value = Mathf.RoundToInt(AudioSettings.masterVolume * 100.0f);
        musicVolumeSlider.value = Mathf.RoundToInt(AudioSettings.musicVolume * 100.0f);
        sfxVolumeSlider.value = Mathf.RoundToInt(AudioSettings.sfxVolume * 100.0f);
    }

    public void SaveSettings()
    {
        AudioSettings.masterVolume = masterVolumeSlider.value / 100.0f;
        AudioSettings.musicVolume = musicVolumeSlider.value / 100.0f;
        AudioSettings.sfxVolume = sfxVolumeSlider.value / 100.0f;

        AudioSettings.SaveSettings();
    }
}

public class AudioSettings
{
    public static bool loaded = false;

    public static float masterVolume;
    public static float musicVolume;
    public static float sfxVolume;

    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("Master Volume", masterVolume);
        PlayerPrefs.SetFloat("Music Volume", musicVolume);
        PlayerPrefs.SetFloat("SFX Volume", sfxVolume);
        //PlayerPrefs.Save();
    }

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