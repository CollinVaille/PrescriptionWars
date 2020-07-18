using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetLever : Interactable
{
    public bool up = true;

    public AudioClip pullUp, pullDown;
    public List<Interactable> effectors;

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        FlipLever(interacting);

        foreach (Interactable effector in effectors)
            effector.Interact(interacting, up);
    }

    private void FlipLever(Pill interacting)
    {
        //Play flip sound
        if (up)
            interacting.GetAudioSource().PlayOneShot(pullDown);
        else
            interacting.GetAudioSource().PlayOneShot(pullUp);

        //Flip state
        up = !up;

        //Flip rotation
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.x = -localEulerAngles.x;
        transform.localEulerAngles = localEulerAngles;
    }
}
