using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float intensity = 1.0f;
    public AudioClip extinguish;

    public void Ignite(Transform toIgnite, Vector3 localFirePosition)
    {
        if (transform.parent)
            return;

        //Place fire on object to ignite
        transform.parent = toIgnite;
        transform.localPosition = localFirePosition;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        //Start fire
        StartCoroutine(FlameLifetime());
    }

    private IEnumerator FlameLifetime()
    {
        //Audio effects
        AudioSource sfx = GetComponent<AudioSource>();
        God.god.ManageAudioSource(sfx);
        sfx.Play();

        //Visual effects
        GetComponent<ParticleSystem>().Play();
        ParticleSystem.MainModule flamesMain = GetComponent<ParticleSystem>().main;

        //Visual effects: dynamic flame size
        float minMult = flamesMain.startSize.constantMin / intensity;
        float maxMult = flamesMain.startSize.constantMax / intensity;
        ParticleSystem.MinMaxCurve flameSize = flamesMain.startSize;

        //Visual effects: embers
        ParticleSystem embers = null;
        ParticleSystem.EmissionModule embersEmission;
        ParticleSystem.MinMaxCurve embersOverTime = flameSize; //Initialization only to avoid "use of assigned" error
        float minEmbersMult = 0.0f;
        float maxEmbersMult = 0.0f;
        if (transform.Find("Embers"))
        {
            embers = transform.Find("Embers").GetComponent<ParticleSystem>();
            embersEmission = embers.emission;
            embersOverTime = embersEmission.rateOverTime;
            minEmbersMult = embers.emission.rateOverTime.constantMin / intensity;
            maxEmbersMult = embers.emission.rateOverTime.constantMax / intensity;
        }

        //Visual effects: light emitted by fire
        Light fireLight = GetComponent<Light>();
        fireLight.enabled = true;

        //Damage
        Damageable damageable = transform.parent.GetComponent<Damageable>();

        //Make fire able to spread
        GetComponent<Collider>().enabled = true;

        //Fire loop
        bool extinguishImmediately = false;
        while(intensity > 0.25f && !SubmersedInWater())
        {
            //Determine if flames should be immediately and silently put out (i.e. when pill respawns)
            if (!gameObject.activeInHierarchy)
            {
                extinguishImmediately = true;
                break;
            }

            //Slowly dissipate flames
            intensity -= 0.01f;

            //Update sound based on intensity
            sfx.volume = intensity / 2.5f;

            //Update visuals based on intensity
            flameSize.constantMin = minMult * intensity;
            flameSize.constantMax = maxMult * intensity;
            flamesMain.startSize = flameSize;

            //Update embers based on intensity
            if(embers)
            {
                embersOverTime.constantMin = minEmbersMult * intensity;
                embersOverTime.constantMax = maxEmbersMult * intensity;
                embersEmission.rateOverTime = embersOverTime;
            }

            //Update light based on intensity
            fireLight.intensity = intensity + Random.Range(-0.3f, 0.3f);

            //Apply damage
            if (damageable != null)
                damageable.Damage(intensity, 0, transform.position, DamageType.Fire, -69);

            //Blacken burning objects
            BlackenBurningMaterials(transform.parent);

            //Delay
            yield return new WaitForSeconds(0.1f);
        }

        //Extinguish fire
        if (extinguishImmediately)
        {
            God.god.UnmanageAudioSource(sfx);
            Destroy(gameObject);
        }
        else
        {
            //Make fire unable to spread
            GetComponent<Collider>().enabled = false;

            //Remove fire visual
            GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireLight.enabled = false;
            if(embers)
                embers.Stop(false, ParticleSystemStopBehavior.StopEmitting);

            //Stop fire sound
            sfx.Stop();
            sfx.clip = null;

            //Play extinguish sound
            sfx.volume = 1;
            sfx.PlayOneShot(extinguish);

            //Wait for extinguish sound to be over
            yield return new WaitForSeconds(extinguish.length);

            //Destroy fire
            God.god.UnmanageAudioSource(sfx);
            Destroy(gameObject);
        }
    }

    private bool SubmersedInWater()
    {
        return Planet.planet.hasOcean && Planet.planet.oceanTransform.position.y >= transform.position.y;
    }

    private void BlackenBurningMaterials(Transform burning)
    {
        //Determine rate of blackening based on fire intensity
        float blackenFactor = 1.0f - intensity / 300.0f;

        //Fire is not intense enough to blacken subjects
        if (blackenFactor >= 1)
            return;

        //Apply blackening to each subject individually
        Renderer[] possibleSubjects = burning.GetComponentsInChildren<Renderer>();
        foreach (Renderer possibleSubject in possibleSubjects)
            BlackenBurningMaterial(possibleSubject, blackenFactor);    
    }

    private void BlackenBurningMaterial(Renderer toBlacken, float blackenFactor)
    {
        //Can only darken existing renderers that are not themselves fire!
        if (!toBlacken || toBlacken.gameObject == gameObject)
            return;

        toBlacken.material.color *= blackenFactor;
    }

    private void OnTriggerEnter(Collider other)
    {
        Damageable newSubject = other.GetComponent<Damageable>();

        //Something flammable came into contact with fire
        if (newSubject != null)
        {
            //Determine if subject is already on fire
            Fire subjectFire = GetSubjectFire(other.transform);

            if (subjectFire) //Subject already on fire... should we amplify it???
            {
                //Of course!
                if (subjectFire.intensity < intensity)
                    subjectFire.intensity = intensity;
            }
            else //Spread fire to new subject
            {
                Fire newFire = Instantiate(Planet.planet.firePrefab).GetComponent<Fire>();
                newFire.Ignite(other.transform, Vector3.zero);
                newFire.intensity = intensity * 0.8f;
            }
        }
    }

    private Fire GetSubjectFire(Transform subject)
    {
        Fire subjectFire = null;

        foreach(Transform subjectPart in subject)
        {
            subjectFire = subjectPart.GetComponent<Fire>();

            if (subjectFire)
                break;
        }

        return subjectFire;
    }
}
