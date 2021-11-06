using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetButton : Interactable
{
    private bool beingPressed = false;
    private float unpressedHeight;

    public AudioClip pressSound;
    public float pressedHeight, pressedTime;
    public List<Interactable> effectors;

    private void Start()
    {
        unpressedHeight = transform.Find("Button").localPosition.y;
    }

    public override void Interact(Pill interacting)
    {
        if (beingPressed)
            return;

        beingPressed = true;

        base.Interact(interacting);

        StartCoroutine(PhysicallyPressButton(interacting));

        foreach (Interactable effector in effectors)
        {
            if (effector)
                effector.Interact(interacting);
        }
    }

    private IEnumerator PhysicallyPressButton(Pill interacting)
    {
        //Sound of pressing switch
        interacting.GetAudioSource().PlayOneShot(pressSound);

        //Update position to pressed
        Vector3 buttonPosition = transform.Find("Button").localPosition;
        buttonPosition.y = pressedHeight;
        transform.Find("Button").localPosition = buttonPosition;

        //Wait
        yield return new WaitForSeconds(pressedTime);

        //Update position to unpressed
        buttonPosition.y = unpressedHeight;
        transform.Find("Button").localPosition = buttonPosition;

        beingPressed = false;
    }

    public override string GetInteractionDescription()
    {
        if (effectors != null && effectors[0] != null && effectors[0].OverrideTriggerDescription())
            return effectors[0].GetInteractionDescription();
        else
            return base.GetInteractionDescription();
    }

    protected override string GetInteractionVerb() { return "Press"; }
}
