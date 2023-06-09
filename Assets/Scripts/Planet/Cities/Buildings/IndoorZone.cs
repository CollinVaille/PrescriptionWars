using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorZone : MonoBehaviour
{
    //Customization
    public AudioReverbPreset indoorReverb = AudioReverbPreset.Room;

    //References
    private IndoorZoneGrouping indoorZoneGrouping;

    private void Start()
    {
        indoorZoneGrouping = GetComponentInParent<IndoorZoneGrouping>();
    }

    private void OnTriggerEnter ()
    {
        PlanetPlayerPill.player.IncrementIndoorZoneCount(indoorReverb, indoorZoneGrouping);
    }

    private void OnTriggerExit ()
    {
        PlanetPlayerPill.player.DecrementIndoorZoneCount();
    }
}
