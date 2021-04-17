using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Audio source that sound effects will play through. Example: sfxSource.PlayOneShot(soundEffectAudioClip).
    private static AudioSource sfxSource = null;
    //Audio source that music will play through. Example: musicSource.Play(musicAudioClip).
    private static AudioSource musicSource = null;

    //Indicates the previously used music source.
    private static AudioSource previousMusicSource = null;
    //Indicates the last song that was played.
    private static AudioClip lastSongPlayed = null;
    //Indicates what time the last song that was played got to.
    private static float lastSongPlayedTime;

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
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Assigns the sfxSource and the musicSource to the specified audio sources.
    public static void SetAudioSources(AudioSource sfxSource, AudioSource musicSource)
    {
        //Asigns the sfx source and updates its volume.
        AudioManager.sfxSource = sfxSource;
        if(AudioManager.sfxSource != null)
            AudioManager.sfxSource.volume = AudioSettings.SFXVolume;

        //Assigns the music source and updates its volume and makes sure that the song that was already playing continues to play.
        AudioManager.musicSource = musicSource;
        if(AudioManager.musicSource != null)
        {
            AudioManager.musicSource.volume = AudioSettings.MusicVolume;

            if(previousMusicSource != musicSource && lastSongPlayed != null)
            {
                AudioManager.musicSource.clip = lastSongPlayed;
                AudioManager.musicSource.Play();
                AudioManager.musicSource.time = lastSongPlayedTime;
            }
        }
    }

    //Updates the volume of the audio listener to equal the master volume value indicated in the AudioSettings.
    public static void UpdateMasterVolume()
    {
        AudioListener.volume = AudioSettings.MasterVolume;
    }

    //Updates the volume of the sfxSource to match the sfx volume indicated in the AudioSettings.
    public static void UpdateSFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = AudioSettings.SFXVolume;
    }

    //Updates the volume of the musicSource to match the music volume indicated in the AudioSettings.
    public static void UpdateMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = AudioSettings.MusicVolume;
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

    //Stores the clip of the music source and the time of the music source into two respective static variables.
    public static void SaveMusicSourceDetails()
    {
        if(musicSource != null)
        {
            previousMusicSource = musicSource;
            lastSongPlayed = musicSource.clip;
            lastSongPlayedTime = musicSource.time;
        }
        else
        {
            previousMusicSource = null;
            lastSongPlayed = null;
            lastSongPlayedTime = 0;
        }
    }
}
