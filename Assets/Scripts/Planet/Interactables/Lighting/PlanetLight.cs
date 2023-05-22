using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetLight : Interactable
{
    //Static-------------------------
    public bool automatic = false;
    private static List<PlanetLight> automaticLights;

    protected virtual void Start()
    {
        if(automatic)
            AddAutomaticLight(this); //Add it to the automatic lighting system
    }

    public static void ClearAllAutomaticLights()
    {
        if(automaticLights != null)
        {
            automaticLights.Clear();
            automaticLights = null;
        }
    }

    public static void AddAutomaticLight(PlanetLight lightToAdd)
    {
        if (automaticLights == null)
            automaticLights = new List<PlanetLight>();

        automaticLights.Add(lightToAdd);
    }

    public static void UpdateAutomaticLights(bool turnOn)
    {
        if (automaticLights == null)
            return;

        foreach(PlanetLight automaticLight in automaticLights)
        {
            if (turnOn)
                automaticLight.TurnOn(null);
            else
                automaticLight.TurnOff(null);
        }
    }

    //Instance-----------------------
    public Transform objectRoot;
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

    public Transform GetObjectRoot() { return objectRoot ? objectRoot : transform; }
}
