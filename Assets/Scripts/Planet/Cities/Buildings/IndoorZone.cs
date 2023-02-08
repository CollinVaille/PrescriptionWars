using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorZone : MonoBehaviour
{
    public AudioReverbPreset indoorReverb = AudioReverbPreset.Room;
    public Building building;

    private void OnTriggerEnter ()
    {
        Player.player.IncrementIndoorZoneCount(indoorReverb, building);
    }

    private void OnTriggerExit ()
    {
        Player.player.DecrementIndoorZoneCount();
    }
}
