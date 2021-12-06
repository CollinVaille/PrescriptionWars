using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactEffect : MonoBehaviour, ManagedVolatileObject, PlanetPooledObject
{
    public static PlanetObjectPool impactEffectPool;

    [Tooltip("Time in seconds from frame game object is activated to frame game object is cleaned up.")]
    public float duration = 1.0f;

    private float timeElapsed = 0.0f;

    public void CreateEffect(Vector3 pointOfEffect)
    {
        //Reset status
        timeElapsed = 0.0f;

        //Set position
        transform.position = pointOfEffect;
        
        //Turn on
        God.god.ManageVolatileObject(this);
        gameObject.SetActive(true);

        //Play particle system
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles)
            particles.Play();

        //Play audio
        AudioSource sfxSource = GetComponent<AudioSource>();
        if (sfxSource)
            sfxSource.Play();
    }

    public void OneTimeSetUp()
    {
        //Remove (Clone) from end of name (necessary for pooling to work)
        name = name.Substring(0, name.Length - 7);
    }

    public void UpdateActiveStatus(float stepTime)
    {
        timeElapsed += stepTime;

        if (timeElapsed >= duration)
            impactEffectPool.PoolGameObject(gameObject);
    }

    public void CleanUp()
    {
        God.god.UnmanageVolatileObject(this);

        gameObject.SetActive(false);
    }
}
