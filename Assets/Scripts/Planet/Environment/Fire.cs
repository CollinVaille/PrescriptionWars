using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float intensity = 1.0f;
    public AudioClip extinguish;

    public void Ignite(Transform toIgnite, Vector3 firePosition)
    {
        if (transform.parent)
            return;

        //Place fire on object to ignite
        transform.parent = toIgnite;
        transform.localPosition = firePosition;
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

        //Damage
        Damageable damageable = transform.parent.GetComponent<Damageable>();

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

            //Apply damage
            if (damageable != null)
                damageable.Damage(intensity, 0, transform.position, DamageType.Fire, -69);

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
            //Remove fire visual
            GetComponent<ParticleSystem>().Stop();

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
}
