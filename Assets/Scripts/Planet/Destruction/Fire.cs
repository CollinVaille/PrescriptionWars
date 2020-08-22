using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    //STATIC MANAGEMENT----------------------------------------------------------------------------------

    //If already on fire, updates and returns pre-existing fire, else returns newly created fire
    public static Fire SetOnFire(Transform target, Vector3 localPositionOfFire, float fireIntensity)
    {
        //First boundary check (had an infinitely large fire once, whoops)...
        fireIntensity = Mathf.Min(fireIntensity, 100.0f);

        //Determine if subject is already on fire
        Fire subjectFire = GetSubjectFire(target.transform);

        if (subjectFire) //Subject already on fire... should we amplify it???
        {
            //Of course!
            if (subjectFire.intensity < fireIntensity)
                subjectFire.intensity = fireIntensity;

            //Also update position
            subjectFire.transform.localPosition = localPositionOfFire;

            return subjectFire;
        }
        else //Spread fire to new subject
        {
            Fire newFire = Instantiate(Planet.planet.firePrefab).GetComponent<Fire>();

            newFire.Ignite(target, localPositionOfFire);
            newFire.intensity = fireIntensity * 0.8f;

            return newFire;
        }
    }

    public static Fire GetSubjectFire(Transform subject)
    {
        Fire subjectFire = null;

        foreach (Transform subjectPart in subject)
        {
            subjectFire = subjectPart.GetComponent<Fire>();

            if (subjectFire)
                break;
        }

        return subjectFire;
    }

    public static bool IsFlammable (Transform target)
    {
        return target.CompareTag("Wood") || target.GetComponent<Damageable>() != null;
    }

    //INSTANCE STUFF-------------------------------------------------------------------------------------

    public float intensity = 1.0f;
    public AudioClip extinguish;

    private void Ignite(Transform toIgnite, Vector3 localFirePosition, float intensity = -1.0f)
    {
        if (transform.parent)
            return;

        //If subject/target is pill, remember on fire for purposes of extinguishing on respawn
        Pill pill = toIgnite.GetComponent<Pill>();
        if (pill)
            pill.onFire = true;

        //Place fire on object to ignite
        transform.parent = toIgnite;
        transform.localPosition = localFirePosition;
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        //Start fire with specified intensity, or default intensity of prefab if unspecified
        StartCoroutine(FlameLifetime(intensity > 0 ? intensity : this.intensity));
    }

    private IEnumerator FlameLifetime(float newIntensity)
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

        //Apply optional intensity parameter after scaling of visuals based on original intensity
        intensity = newIntensity;

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

            //If pill, no longer on fire
            Pill pill = transform.parent.GetComponent<Pill>();
            if (pill)
                pill.onFire = false;

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

        //Not all materials have a color property and those that don't throw an exception (annoying)
        if(toBlacken.material.HasProperty("_Color"))
            toBlacken.material.color *= blackenFactor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsFlammable(other.transform))
            SetOnFire(other.transform, Vector3.zero, intensity);
    }
}
