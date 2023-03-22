using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FancyPlanetLight : PlanetLight
{
    [Tooltip("Percentage of original intensity light levels can fall to while flickering.")]
    public float maxDimness = 0.75f;

    public Light[] lights;
    public Renderer[] lightRenderers;
    public Material lightMaterial;

    private Material offMaterial;
    private float[] lightIntensities;
    private AudioSource audioSource;
    private int flickerCode = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (lightRenderers != null && lightRenderers[0] != null)
            offMaterial = lightRenderers[0].sharedMaterial;

        lightIntensities = new float[lights.Length];
        for (int x = 0; x < lights.Length; x++)
        {
            if (lights[x])
                lightIntensities[x] = lights[x].intensity;
        }
    }

    private void Start()
    {
        God.god.ManageAudioSource(audioSource);
    }

    protected override void TurnOn(AudioSource interactingAudioSource)
    {
        if (on)
            return;

        base.TurnOn(interactingAudioSource);

        //Turn on the lights
        StartCoroutine(FlickerLights());
    }

    protected override void TurnOff(AudioSource interactingAudioSource)
    {
        if (!on)
            return;

        base.TurnOff(interactingAudioSource);

        //Turn off the lights
        StartCoroutine(FlickerLights());
    }

    private IEnumerator FlickerLights()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();

        int flickerKey = ++flickerCode;
        float stepIncrement = 0.2f;

        //Flickering transition
        for (float t = 0.0f, duration = 3.0f; t < duration && flickerKey == flickerCode; t += stepIncrement)
        {
            if(on)
                SetCurrentLightLevel(Random.Range(0.25f, 1.0f) * t / duration);
            else
                SetCurrentLightLevel(Random.Range(0.25f, 1.0f) * (duration - t) / duration);

            //Wait
            yield return new WaitForSeconds(stepIncrement);
        }

        //Go into steady state, either on or off
        if (flickerKey == flickerCode)
        {
            if(on)
            {
                //Constant subtle flickering while lights are on
                while (flickerKey == flickerCode)
                {
                    SetCurrentLightLevel(Random.Range(maxDimness, 1.0f));

                    //Wait
                    yield return new WaitForSeconds(stepIncrement);
                }
            }
            else
            {
                SetCurrentLightLevel(0.0f);
                audioSource.Stop();
            }
        }
    }

    //Light level specified as range from 0-1
    private void SetCurrentLightLevel(float lightLevel)
    {
        //Set light material
        foreach(Renderer lightRenderer in lightRenderers)
        {
            if(lightRenderer)
            {
                if(lightLevel < 0.1f)
                    lightRenderer.sharedMaterial = offMaterial;
                else
                    lightRenderer.sharedMaterial = Random.Range(lightLevel, 2.5f) > 1.0f ? lightMaterial : offMaterial;
            }
                
        }

        //Set light emitted
        for(int x = 0; x < lights.Length; x++)
        {
            if (lights[x])
            {
                if (lightLevel < 0.1f)
                    lights[x].enabled = false;
                else
                {
                    lights[x].enabled = true;
                    lights[x].intensity = lightLevel * lightIntensities[x];
                }
            }
        }

        //Set volume
        audioSource.volume = lightLevel;
    }
}
