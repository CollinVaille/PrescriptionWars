using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisPod : Interactable, Damageable
{
    public bool on = false;
    public Transform subject, fluid;
    public float emptyHeight, fullHeight, fullScale;
    public DynamicWater fillOnDestroy;

    private int stasisCode = 0;
    private AudioSource sfxSource;

    //Subject mutation variables
    private float initialSubjectHeight = 1;
    private static Material defaultSubjectMaterial;
    private Pill lastInteracter;
    private int shakeCode = 0;

    private void Start()
    {
        //Initialize subject mutator functionalities
        initialSubjectHeight = subject.localPosition.y;
        defaultSubjectMaterial = subject.GetComponent<Renderer>().sharedMaterial;

        //Initialize audio
        sfxSource = GetComponent<AudioSource>();
        God.god.ManageAudioSource(sfxSource);
        if (on)
            sfxSource.Play();
        else
            sfxSource.Stop();
    }

    public override void Interact(Pill interacting, bool turnOn)
    {
        base.Interact(interacting);

        lastInteracter = interacting;

        StartCoroutine(TransitionPowerState(turnOn));
    }

    private IEnumerator TransitionPowerState(bool turningOn)
    {
        int stasisKey = ++stasisCode;

        on = turningOn;

        Vector3 fluidPosition = fluid.localPosition;
        float initialHeight = fluidPosition.y;

        Vector3 fluidScale = fluid.localScale;
        float initialTallness = fluidScale.y;

        //Start injecting fluid
        if(turningOn)
        {
            fluid.gameObject.SetActive(true);
            sfxSource.Play();
        }

        //Lerp position and scale over time
        float duration = 2.5f, timeStep = 0.05f;
        for (float t = 0.0f; t < duration; t += 0.2f)
        {
            if (stasisKey != stasisCode)
                break;

            if(turningOn)
            {
                fluidPosition.y = Mathf.Lerp(initialHeight, fullHeight, t / duration);
                fluid.localPosition = fluidPosition;

                fluidScale.y = Mathf.Lerp(initialTallness, fullScale, t / duration);
                fluid.localScale = fluidScale;

                sfxSource.volume = t / duration;
            }
            else
            {
                fluidPosition.y = Mathf.Lerp(initialHeight, emptyHeight, t / duration);
                fluid.localPosition = fluidPosition;

                fluidScale.y = Mathf.Lerp(initialTallness, 0, t / duration);
                fluid.localScale = fluidScale;

                sfxSource.volume = 1.0f - t / duration;
            }

            yield return new WaitForSeconds(timeStep);
        }

        //Finalize transition
        if(stasisKey == stasisCode)
        {
            if (turningOn)
            {
                fluidPosition.y = fullHeight;
                fluid.localPosition = fluidPosition;

                fluidScale.y = fullScale;
                fluid.localScale = fluidScale;

                sfxSource.volume = 1;

                MutateSubject();
            }
            else
            {
                fluidPosition.y = emptyHeight;
                fluid.localPosition = fluidPosition;

                fluidScale.y = 0;
                fluid.localScale = fluidScale;

                sfxSource.volume = 0;
                sfxSource.Stop();

                fluid.gameObject.SetActive(false);
            }
        }
    }

    private void MutateSubject()
    {
        int mutation = Random.Range(1, 11);

        switch(mutation)
        {
            case 1: //Random scale
                SetSubjectScale(new Vector3(Random.Range(0.1f, 1.0f), Random.Range(0.1f, 1.0f), Random.Range(0.1f, 1.0f)));
                break;
            case 2: //Normal scale
                SetSubjectScale(Vector3.one);
                break;
            case 3: //Normal material
                subject.GetComponent<Renderer>().sharedMaterial = defaultSubjectMaterial;
                break;
            case 4: //Fluid material
                subject.GetComponent<Renderer>().sharedMaterial = fluid.GetComponent<Renderer>().sharedMaterial;
                break;
            case 5: //Copy interacter's material
                if(lastInteracter)
                    subject.GetComponent<Renderer>().sharedMaterial = lastInteracter.GetComponent<Renderer>().sharedMaterial;
                break;
            case 6: //Stop subject from shaking
                shakeCode++;
                break;
            case 7: //Shake subject
                StartCoroutine(ShakeSubject());
                break;
            default: //Randomly hide/show subject
                subject.GetComponent<Renderer>().enabled = Random.Range(0, 2) == 0;
                break;
        }
    }

    private void SetSubjectScale(Vector3 newScale)
    {
        subject.localScale = newScale;

        Vector3 subjectLocalPosition = subject.localPosition;
        subjectLocalPosition.y = Mathf.Lerp(emptyHeight, initialSubjectHeight, newScale.y);
        subject.localPosition = subjectLocalPosition;
    }

    private IEnumerator ShakeSubject()
    {
        int shakeKey = ++shakeCode;

        Vector3 originalPosition = new Vector3(0, subject.localPosition.y, 0);
        float energy = Random.Range(0.05f, 0.25f);

        while(shakeKey == shakeCode)
        {
            Vector3 currentPosition = subject.localPosition;

            currentPosition.x = Random.Range(-energy, energy);
            currentPosition.y = Random.Range(-energy, energy);
            currentPosition.z = Random.Range(-energy, energy);

            subject.localPosition = currentPosition;

            yield return new WaitForSeconds(0.05f);
        }

        //Reset position of subject
        subject.localPosition = originalPosition;

        //And make sure it's grounded
        SetSubjectScale(subject.localScale);
    }

    public void Damage(float damage, float knockback, Vector3 from, DamageType damageType, int team)
    {
        if (damageType == DamageType.HullCompromised)
            DestroyPod(100, from);
    }

    private void DestroyPod(float explosionForce, Vector3 from)
    {
        //Release subject
        subject.parent = null;
        Rigidbody subjectRigidbody = subject.gameObject.AddComponent<Rigidbody>();
        subjectRigidbody.AddExplosionForce(explosionForce, from, 1000);
        subject.gameObject.AddComponent<Corpse>();

        //Release fluid
        float fluidVolume = fluid.lossyScale.magnitude;
        fluid.parent = null;
        Destroy(fluid.gameObject);
        if (fillOnDestroy)
            fillOnDestroy.AddWaterGradually(fluidVolume * 25.0f);

        //Determine what to do with remaining parts
        while(transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.parent = null;

            //Leave essential parts untouched, save that they are now unassociated with the parent since the parent will be deleted
            if (child.CompareTag("Essential"))
                continue;

            //Destroy insignificant parts
            if (!child.GetComponent<Collider>() || !child.GetComponent<MeshRenderer>())
                Destroy(child.gameObject);

            //Blow major parts off as debris
            Rigidbody rBody = child.gameObject.AddComponent<Rigidbody>();
            rBody.AddExplosionForce(explosionForce, from, 1000);
        }

        //Destroy stasis pod parent object
        God.god.UnmanageAudioSource(sfxSource);
        Destroy(gameObject);
    }
}
