using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void Interact (PlanetPill interacting) { }

    public virtual void Interact (PlanetPill interacting, bool turnOn) { }

    public virtual void ReleaseControl (bool voluntary) { }

    public virtual bool OverrideTriggerDescription() { return false; }

    public virtual string GetInteractionDescription()
    {
        return GetInteractionVerb() + " " + gameObject.name;
    }

    protected virtual string GetInteractionVerb() { return "Interact"; }
}
