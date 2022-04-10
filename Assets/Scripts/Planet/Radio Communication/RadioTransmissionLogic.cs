using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransmissionType { ReportingIn, Pronouncing }

public class RadioTransmissionLogic
{
    const int reportingInClipCount = 7;

    public static List<RadioClip> DecodeTransmission(RadioTransmission rt)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        //Play series of clips based on the type of transmission
        switch (rt.transmissionType)
        {
            case TransmissionType.ReportingIn:
                if (Random.Range(0, 2) == 0)
                {
                    radioClips.Add(new RadioClip(RadioCommonAudio.thisIs));
                    rt.subtitle += "This is ";
                }

                radioClips.Add(new RadioClip(rt.squad.Pronounce()));
                rt.subtitle += rt.squad.name + " ";

                if (Random.Range(0, 2) == 0)
                {
                    radioClips.Add(new RadioClip(RadioCommonAudio.squad));
                    rt.subtitle += "squad ";
                }

                radioClips.Add(new RadioClip("Planet/Radio/Reporting In/Reporting In " + Random.Range(1, reportingInClipCount + 1), false));
                rt.subtitle += "reporting in.";

                break;

            case TransmissionType.Pronouncing:
                string input = "This is Warpig Squad. Requesting backup now.";
                radioClips.AddRange(RadioWordPronounciation.PronounceWords(input));
                rt.subtitle += input;
                //audioClips.AddRange(RadioNumericalPronounciation.PronounceNumber(3000));
                break;
        }

        return radioClips;
    }
}
