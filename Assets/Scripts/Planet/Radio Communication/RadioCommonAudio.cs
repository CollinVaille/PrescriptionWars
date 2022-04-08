using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCommonAudio
{
    //Common audio
    private static bool commonAudioInitialized = false;
    public static AudioClip startSound, endSound, thisIs, squad;
    public static List<AudioClip> reportingIn;

    public static void InitializeCommonAudio()
    {
        if (commonAudioInitialized)
            return;

        commonAudioInitialized = true;

        startSound = Resources.Load<AudioClip>("Planet/Radio/Common/Start Sound");
        endSound = Resources.Load<AudioClip>("Planet/Radio/Common/End Sound");

        thisIs = Resources.Load<AudioClip>("Planet/Radio/Common/This Is");
        squad = Resources.Load<AudioClip>("Planet/Radio/Common/Squad");

        reportingIn = new List<AudioClip>();
        God.InitializeAudioList(reportingIn, "Planet/Radio/Common/Reporting In ");
    }
}
