using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : Interactable
{
    private Pill occupant;
    public Vehicle controls;

    public AudioClip sit, getUp;

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        Sit(interacting);
    }

    private void Sit(Pill pill)
    {
        if (occupant || !pill || !pill.CanOverride())
            return;

        occupant = pill;

        occupant.transform.parent = transform;
        occupant.transform.localPosition = Vector3.up;
        occupant.transform.localEulerAngles = Vector3.zero;

        occupant.GetAudioSource().PlayOneShot(sit);
        occupant.OverrideControl(this);

        if (controls)
            controls.SetPower(true);
    }

    public override void ReleaseControl(bool voluntary)
    {
        base.ReleaseControl(voluntary);

        GetUp(voluntary);
    }

    private void GetUp(bool voluntary)
    {
        if (!occupant)
            return;

        if (controls)
            controls.SetPower(false);

        if (voluntary)
            occupant.GetAudioSource().PlayOneShot(getUp);

        occupant.transform.parent = null;
        occupant.transform.eulerAngles = Vector3.up * transform.eulerAngles.y;

        occupant.ReleaseOverride();
        occupant = null;
    }
}
