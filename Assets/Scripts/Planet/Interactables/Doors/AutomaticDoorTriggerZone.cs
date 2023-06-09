using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoorTriggerZone : MonoBehaviour
{
    private int occupantCounter = 0;

    public bool HasOccupants() { return occupantCounter > 0; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponentInParent<PlanetPill>())
            occupantCounter++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.GetComponentInParent<PlanetPill>())
            occupantCounter--;
    }
}
