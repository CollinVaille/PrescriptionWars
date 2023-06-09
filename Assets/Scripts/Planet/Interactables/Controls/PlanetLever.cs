using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lever that has up/down state.
public class PlanetLever : Interactable
{
    public bool up = true, upIsOn = true;

    public AudioClip pullUp, pullDown;
    public float upRotation, downRotation;
    public List<Interactable> effectors;

    public override void Interact(PlanetPill interacting)
    {
        base.Interact(interacting);

        FlipLever(interacting);

        foreach (Interactable effector in effectors)
        {
            if(effector)
                effector.Interact(interacting, up ? upIsOn : !upIsOn);
        }
    }

    private void FlipLever(PlanetPill interacting)
    {
        //Play flip sound
        if (up)
            interacting.GetAudioSource().PlayOneShot(pullDown);
        else
            interacting.GetAudioSource().PlayOneShot(pullUp);

        //Flip state
        up = !up;

        //Update rotation
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.x = up ? upRotation : downRotation;
        transform.localEulerAngles = localEulerAngles;
    }

    public override string GetInteractionDescription()
    {
        if (effectors != null && effectors[0] != null && effectors[0].OverrideTriggerDescription())
            return effectors[0].GetInteractionDescription();
        else
            return base.GetInteractionDescription();
    }

    protected override string GetInteractionVerb() { return up ? "Pull Down" : "Pull Up"; }
}
