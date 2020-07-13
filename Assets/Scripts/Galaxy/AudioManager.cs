using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Makes sure that the audio settings have been loaded in.
        if (!AudioSettings.loaded)
            AudioSettings.LoadSettings();

        //Updates the master, music, and sfx volume.
        AudioListener.volume = AudioSettings.masterVolume;
        musicSource.volume = AudioSettings.musicVolume;
        sfxSource.volume = AudioSettings.sfxVolume;
    }
}
