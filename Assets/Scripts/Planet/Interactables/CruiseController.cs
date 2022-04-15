using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CruiseController : Interactable
{
    private Vehicle controls = null;

    public override void Interact(Pill interacting, bool turnOn)
    {
        base.Interact(interacting, turnOn);

        if (!controls)
            controls = transform.GetComponentInParent<Vehicle>();

        if (controls)
            controls.SetCruiseControl(turnOn);
    }
}
