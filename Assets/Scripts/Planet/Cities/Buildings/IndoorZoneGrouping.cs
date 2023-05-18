using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndoorZoneGrouping : MonoBehaviour
{
    public static IndoorZoneGrouping playerInside = null;

    private int openings = 0;

    public void AddNewOpening()
    {
        //Lost air seal
        if (playerInside == this && openings == 0)
            Planet.planet.ambientVolume *= 4;

        openings++;
    }

    public void CloseExistingOpening()
    {
        //Gained air seal
        if (playerInside == this && openings == 1)
            Planet.planet.ambientVolume *= 0.25f;

        openings--;
    }

    public bool AirTight() { return openings <= 0; }
}
