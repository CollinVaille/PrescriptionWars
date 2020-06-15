using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : Interactable
{
    public AudioClip turnOn, turnOff;

    private bool on = false, destroyWhenOff = true;
    private Light lightSource = null;
    private int flickerCode = 0;

    private void Start ()
    {
        destroyWhenOff = !GetComponent<Light>();
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        ToggleLightState(interacting.GetAudioSource());
    }

    public void ToggleLightState (AudioSource audioSource)
    {
        if (on)
            TurnOff(audioSource);
        else
            TurnOn(audioSource);
    }

    private void TurnOn (AudioSource audioSource)
    {
        if (on)
            return;

        on = true;

        //Audio
        if (audioSource)
            audioSource.PlayOneShot(turnOn);
        if (GetComponent<AudioSource>())
            GetComponent<AudioSource>().Play();

        //Get reference to light
        lightSource = GetComponent<Light>();
        if(!lightSource)
        {
            lightSource = gameObject.AddComponent<Light>();
            lightSource.range = 20; //Covers surrounding 5-10ish meters well
            lightSource.color = new Color(1, 1, 0.7f); //Whitish-yellow
        }

        //Enable light
        lightSource.enabled = true;
        StartCoroutine(FlickerLight());
    }

    private void TurnOff (AudioSource audioSource)
    {
        if (!on)
            return;

        on = false;

        //Audio
        if (audioSource)
            audioSource.PlayOneShot(turnOff);
        if (GetComponent<AudioSource>())
            GetComponent<AudioSource>().Stop();

        //Tell light to stop flickering
        flickerCode++;

        //Turn off light
        if (destroyWhenOff)
        {
            Destroy(lightSource);
            lightSource = null;
        }
        else
            lightSource.enabled = false;
    }

    private IEnumerator FlickerLight ()
    {
        int flickerKey = ++flickerCode;

        float spazDuration = Random.Range(0.75f, 2.0f);

        //Initial strong flicker while transitioning to on state
        for(float t = 0; t < spazDuration; t += Time.deltaTime)
        {
            if (flickerKey != flickerCode)
                break;

            lightSource.intensity = Random.Range(0.8f, 1.2f);

            yield return null;
        }

        //Subtle flicker afterwards
        while(flickerKey == flickerCode)
        {
            lightSource.intensity = Random.Range(0.9f, 1.1f);

            yield return new WaitForSeconds(Random.Range(0.15f, 0.25f));
        }
    }
}
