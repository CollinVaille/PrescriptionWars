using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Switch that has pushed top/pushed bottom state.
public class PlanetSwitch : Interactable
{
    public bool on = false;

    public AudioClip pressSound;
    public Renderer[] changeVisualsFor;
    public Material onMaterial, offMaterial;
    public List<Interactable> effectors;

    public override void Interact(PlanetPill interacting)
    {
        base.Interact(interacting);

        FlipSwitch(interacting);

        foreach (Interactable effector in effectors)
        {
            if (effector)
                effector.Interact(interacting, on);
        }
    }

    private void FlipSwitch(PlanetPill interacting)
    {
        //Sound of pressing switch
        interacting.GetAudioSource().PlayOneShot(pressSound);

        //Flip state
        on = !on;

        //Update rotation
        Transform rotatingFace = transform.Find("Rotating Face");
        Vector3 localEulerAngles = rotatingFace.localEulerAngles;
        localEulerAngles.x = -localEulerAngles.x;
        rotatingFace.localEulerAngles = localEulerAngles;

        //Update materials
        foreach (Renderer renderer in changeVisualsFor)
        {
            if(renderer)
                renderer.sharedMaterial = on ? onMaterial : offMaterial;
        }
    }

    public override string GetInteractionDescription()
    {
        if (effectors != null && effectors[0] != null && effectors[0].OverrideTriggerDescription())
            return effectors[0].GetInteractionDescription();
        else
            return base.GetInteractionDescription();
    }

    protected override string GetInteractionVerb() { return on ? "Turn Off" : "Turn On"; }
}
