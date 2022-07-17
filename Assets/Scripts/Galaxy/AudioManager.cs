using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Options")]

    [SerializeField, LabelOverride("Music Source")] private AudioSource localMusicSource = null;

    //Audio source that sound effects will play through. Example: sfxSource.PlayOneShot(soundEffectAudioClip).
    private static AudioSource sfxSource = null;
    //Audio source that music will play through. Example: musicSource.Play(musicAudioClip).
    private static AudioSource musicSource = null;

    //Bool return value indicates whether music is playing in the game through the musicSource AudioSource.
    public static bool IsMusicPlaying
    {
        get
        {
            if (musicSource != null)
                return musicSource.isPlaying;
            return false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        if (musicSource == null)
            SetMusicSource(localMusicSource);
        else
            Destroy(localMusicSource.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Assigns the sfxSource to the specified audio source.
    public static void SetSFXSource(AudioSource sfxSource)
    {
        //Asigns the sfx source and updates its volume.
        AudioManager.sfxSource = sfxSource;
        if(AudioManager.sfxSource != null)
            AudioManager.sfxSource.volume = AudioSettings.sfxVolume;
    }

    //Assigns the musicSource to the specified audio source.
    private static void SetMusicSource(AudioSource musicSource)
    {
        //Assigns the music source and updates its volume.
        AudioManager.musicSource = musicSource;
        if (AudioManager.musicSource != null)
            AudioManager.musicSource.volume = AudioSettings.musicVolume;
    }

    //Updates the volume of the audio listener to equal the master volume value indicated in the AudioSettings.
    public static void UpdateMasterVolume()
    {
        AudioListener.volume = AudioSettings.masterVolume;
    }

    //Updates the volume of the sfxSource to match the sfx volume indicated in the AudioSettings.
    public static void UpdateSFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = AudioSettings.sfxVolume;
    }

    //Updates the volume of the musicSource to match the music volume indicated in the AudioSettings.
    public static void UpdateMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = AudioSettings.musicVolume;
    }

    //Plays the specified sound effect if the sfxSource is assigned and not null.
    public static void PlaySFX(AudioClip soundEffect)
    {
        if (sfxSource != null && soundEffect != null)
            sfxSource.PlayOneShot(soundEffect);
    }

    //Plays the specified song if the musicSource is assigned and not null.
    public static void PlaySong(AudioClip song)
    {
        if (musicSource != null && song != null)
        {
            musicSource.clip = song;
            musicSource.Play();
        }
    }
}
