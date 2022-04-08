using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransmissionType { ReportingIn, Pronouncing }

public class RadioTransmissionLogic
{
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

                radioClips.Add(new RadioClip(RadioCommonAudio.reportingIn));
                rt.subtitle += "reporting in.";

                break;

            case TransmissionType.Pronouncing:
                radioClips.AddRange(RadioWordPronounciation.PronounceWords("Reporting In. Surrender Now Or Die."));
                rt.subtitle += "Reporting In. Surrender Now Or Die.";
                radioClips.AddRange(RadioNumericalPronounciation.PronounceNumber(1.69f));
                rt.subtitle += " " + 1.69f;
                //audioClips.AddRange(RadioNumericalPronounciation.PronounceNumber(3000));
                break;
        }

        return radioClips;
    }
}
