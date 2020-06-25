using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : Interactable
{
    private Pill occupant;

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        GoToBed(interacting);
    }

    public void GoToBed (Pill pill)
    {
        if (occupant || !pill || !pill.CanOverride())
            return;

        occupant = pill;

        occupant.transform.parent = transform.parent;
        occupant.transform.localPosition = Vector3.up;
        occupant.transform.localEulerAngles = new Vector3(-90, 180, 0);

        occupant.OverrideControl(this);
    }

    public override void ReleaseControl(bool voluntary)
    {
        base.ReleaseControl(voluntary);

        WakeUp();
    }

    private void WakeUp ()
    {
        occupant.transform.localEulerAngles = new Vector3(0, 0, 0);
        occupant.transform.parent = null;

        occupant.ReleaseOverride();

        occupant = null;
    }

    private void OnCollisionEnter (Collision collision)
    {
        Pill pill = collision.gameObject.GetComponent<Pill>();

        if (pill && !collision.gameObject.GetComponent<Player>())
            GoToBed(pill);
    }
}
