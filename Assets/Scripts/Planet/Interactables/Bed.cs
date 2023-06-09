using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : Interactable
{
    private PlanetPill occupant;

    public override void Interact(PlanetPill interacting)
    {
        base.Interact(interacting);

        GoToBed(interacting);
    }

    public void GoToBed (PlanetPill pill)
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
        PlanetPill pill = collision.gameObject.GetComponent<PlanetPill>();

        if (pill && !pill.IsPlayer)
            GoToBed(pill);
    }

    protected override string GetInteractionVerb() { return "Sleep"; }
}
