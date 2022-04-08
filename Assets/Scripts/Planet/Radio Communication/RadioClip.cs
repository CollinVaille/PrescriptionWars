using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioClip
{
    public AudioClip audioClip;
    public float pauseDurationAfter = StandardPauseLength();

    public RadioClip(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    public RadioClip(AudioClip audioClip, float pauseAfterDuration)
    {
        this.audioClip = audioClip;
        this.pauseDurationAfter = pauseAfterDuration;
    }

    public RadioClip(List<AudioClip> audioClips)
    {
        audioClip = God.RandomClip(audioClips);
    }

    public RadioClip(List<AudioClip> audioClips, float pauseAfterDuration)
    {
        audioClip = God.RandomClip(audioClips);
        this.pauseDurationAfter = pauseAfterDuration;
    }

    public RadioClip(string audioClipResourcePath, bool multipleClipsAtPath)
    {
        SetAudioClip(audioClipResourcePath, multipleClipsAtPath);
    }

    public RadioClip(string audioClipResourcePath, bool multipleClipsAtPath, float pauseAfterDuration)
    {
        SetAudioClip(audioClipResourcePath, multipleClipsAtPath);
        this.pauseDurationAfter = pauseAfterDuration;
    }

    private void SetAudioClip(string audioClipResourcePath, bool multipleClipsAtPath)
    {
        if (multipleClipsAtPath)
        {
            List<AudioClip> audioList = new List<AudioClip>();
            God.InitializeAudioList(audioList, audioClipResourcePath);
            audioClip = God.RandomClip(audioList);
        }
        else
            audioClip = Resources.Load<AudioClip>(audioClipResourcePath);
    }

    public static List<RadioClip> Combine(List<RadioClip> firstList, List<RadioClip> secondList)
    {
        if (firstList == null)
            return secondList;
        else if (secondList == null)
            return firstList;
        else
        {
            firstList.AddRange(secondList);
            return firstList;
        }
    }

    public static float StandardPauseLength() { return Random.Range(0.1f, 0.3f); }
}
