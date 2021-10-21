using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voice
{
    //Static voice management---------------------------------------------------------------------------
    private static string[] voiceNames = new string[] { "Carmine", "Hero Jack" };

    private static List<Voice> voices;

    public static void InitialSetUp()
    {
        voices = new List<Voice>();
    }

    public static Voice GetVoice(string voiceName)
    {
        int voiceIndex = -1;

        for (int x = 0; x < voices.Count; x++)
        {
            if (voices[x].voiceName.Equals(voiceName))
            {
                voiceIndex = x;
                break;
            }
        }

        if (voiceIndex != -1)
            return voices[voiceIndex];
        else
        {
            Voice newVoice = new Voice(voiceName);
            voices.Add(newVoice);
            return newVoice;
        }
    }

    public static string[] GetVoiceNames() { return voiceNames; }

    public static implicit operator bool(Voice voice) { return voice != null; }

    //Voice instance------------------------------------------------------------------------------------
    private string voiceName, voicePath;
    private AudioClip[] oofs, copies, eagerBanter, idleBanter; //Common/generic dialogue lists
    public AudioClip holdPosition, follow, formSquare, formLine, roam; //Commands
    private AudioClip[] thankYous, dramaticDeaths; //Rare dialogue lists
    private AudioClip imGood; //Rare dialogue

    public Voice(string voiceName)
    {
        this.voiceName = voiceName;
        voicePath = "Planet/Voices/" + voiceName;

        //Common audio with alternate versions
        oofs = Resources.LoadAll<AudioClip>(voicePath + "/Oofs");
        copies = Resources.LoadAll<AudioClip>(voicePath + "/Copies");
        eagerBanter = Resources.LoadAll<AudioClip>(voicePath + "/Eager Banter");
        idleBanter = Resources.LoadAll<AudioClip>(voicePath + "/Idle Banter");

        //Commands
        holdPosition = Resources.Load<AudioClip>(voicePath + "/Commands/Hold Position");
        follow = Resources.Load<AudioClip>(voicePath + "/Commands/Follow");
        formSquare = Resources.Load<AudioClip>(voicePath + "/Commands/Form Square");
        formLine = Resources.Load<AudioClip>(voicePath + "/Commands/Form Line");
        roam = Resources.Load<AudioClip>(voicePath + "/Commands/Roam");
    }

    public string GetVoiceName() { return voiceName; }

    public AudioClip GetOof() { return oofs[Random.Range(0, oofs.Length)]; }

    public AudioClip GetCopy() { return copies[Random.Range(0, copies.Length)]; }

    public AudioClip GetEagerBanter() { return eagerBanter[Random.Range(0, eagerBanter.Length)]; }

    public AudioClip GetIdleBanter() { return idleBanter[Random.Range(0, idleBanter.Length)]; }

    public AudioClip GetThanks()
    {
        if(thankYous == null)
            thankYous = Resources.LoadAll<AudioClip>(voicePath + "/Thank Yous");

        return thankYous[Random.Range(0, thankYous.Length)];
    }

    public AudioClip GetDramaticDeath()
    {
        if (dramaticDeaths == null)
            dramaticDeaths = Resources.LoadAll<AudioClip>(voicePath + "/Dramatic Deaths");

        return dramaticDeaths[Random.Range(0, dramaticDeaths.Length)];
    }

    public AudioClip GetImGood()
    {
        if (imGood == null)
            imGood = Resources.Load<AudioClip>(voicePath + "/Misc/Im Good");

        return imGood;
    }
}