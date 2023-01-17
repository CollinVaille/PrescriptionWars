using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorZone : MonoBehaviour
{
    public Transform cab;

    public void OnTriggerEnter(Collider other)
    {
        Pill pill = other.GetComponentInParent<Pill>();

        //if (pill && pill.GetComponent<Player>())
        //    Debug.Log("Entering");

        if (pill && !pill.transform.parent)
            pill.transform.parent = cab;
        else
        {
            Item item = other.GetComponentInParent<Item>();

            if (item && !item.BeingHeld() && !item.transform.parent)
                item.transform.parent = cab;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Pill pill = other.GetComponentInParent<Pill>();

        //if (pill && pill.GetComponent<Player>())
        //    Debug.Log("Exiting");

        if (pill && pill.transform.parent == cab)
            pill.transform.parent = null;
        else
        {
            Item item = other.GetComponentInParent<Item>();

            if (item && !item.BeingHeld() && item.transform.parent == cab)
                item.transform.parent = null;
        }
    }
}
