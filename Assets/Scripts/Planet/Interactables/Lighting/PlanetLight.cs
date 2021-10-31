using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetLight : Interactable
{
    protected bool on = false;

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        if (on)
            TurnOff(interacting.GetAudioSource());
        else
            TurnOn(interacting.GetAudioSource());
    }

    public override void Interact(Pill interacting, bool turnOn)
    {
        base.Interact(interacting, turnOn);

        if(turnOn && !on)
            TurnOn(interacting.GetAudioSource());
        else if(!turnOn && on)
            TurnOff(interacting.GetAudioSource());
    }

    protected virtual void TurnOn(AudioSource interactingAudioSource)
    {
        on = true;
    }

    protected virtual void TurnOff(AudioSource interactingAudioSource)
    {
        on = false;
    }

    protected override string GetInteractionVerb() { return on ? "Turn Off" : "Turn On"; }
}
