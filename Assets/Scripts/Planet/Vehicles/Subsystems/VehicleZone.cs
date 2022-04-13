using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VehicleZone : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Pill pill = other.GetComponentInParent<Pill>();

        //if (pill && pill.GetComponent<Player>())
        //    Debug.Log("Entering");

        if (pill)
            pill.ChangeVehicleZone(this, true);
        else
        {
            Item item = other.GetComponentInParent<Item>();

            if (item && !item.BeingHeld())
                item.transform.parent = transform;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Pill pill = other.GetComponentInParent<Pill>();

        //if (pill && pill.GetComponent<Player>())
        //    Debug.Log("Exiting");

        if (pill)
            pill.ChangeVehicleZone(this, false);
        else
        {
            Item item = other.GetComponentInParent<Item>();

            if (item && !item.BeingHeld())
                item.transform.parent = null;
        }
    }
}
