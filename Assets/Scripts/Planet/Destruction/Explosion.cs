using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour, PlanetPooledObject
{
    public static PlanetObjectPool explosionPool;

    //Explosion parameters
    private int team = 0;

    //Customization
    public float range = 20;
    public float damage = 50;

    //References
    private AudioSource sfxSource;
    private ParticleSystem visuals;

    public void OneTimeSetUp()
    {
        //Get references
        sfxSource = GetComponent<AudioSource>();
        visuals = GetComponent<ParticleSystem>();

        //Remove (Clone) from end of name (necessary for pooling to work)
        name = name.Substring(0, name.Length - 7);
    }

    public void Explode(int team)
    {
        //Remember explosion parameters
        this.team = team;

        //Start explosion
        gameObject.SetActive(true);
        God.god.ManageAudioSource(sfxSource);
        sfxSource.Play();
        visuals.Play(true);

        //Rest of explosion lifetime
        StartCoroutine(FixedExplosionLifetime());
    }

    private IEnumerator FixedExplosionLifetime()
    {
        ExplosionEffects(range);

        //Wait until smoke clears
        float waitTime = Mathf.Max(visuals.main.duration, sfxSource.clip.length) + 0.25f;
        yield return new WaitForSeconds(waitTime);

        //Remove explosion
        Decommission();
    }

    //Apply damage to damageables, force to rigidbodies, and destruction to structures like walls and windows
    private void ExplosionEffects(float currentRange)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRange);

        INavZoneUpdater alteredNavMesh = null;

        foreach (Collider hit in hits)
        {
            //Explosion cannot go through barriers/walls
            if (!DirectLineToTarget(hit))
                continue;

            //Compute damage to apply to hit object based on base damage and distance away
            float actualDamage = damage / Vector3.Distance(hit.transform.position, transform.position);

            //First, damage any damageable
            Damageable damageable = hit.GetComponent<Damageable>();

            if (damageable != null)
                damageable.Damage(actualDamage, actualDamage * 10, transform.position, DamageType.Explosive, team);
            else
            {
                //Then, push any rigidbodies
                Rigidbody rBody = hit.GetComponent<Rigidbody>();

                if (rBody)
                    rBody.AddExplosionForce(actualDamage * 10, transform.position, currentRange);
                else
                {
                    INavZoneUpdater damagedNavMesh = DamageStructure(hit, actualDamage);
                    if (damagedNavMesh != null)
                        alteredNavMesh = damagedNavMesh;
                }
            }
        }

        if (alteredNavMesh != null)
            God.god.PaintNavMeshDirty(alteredNavMesh);
    }

    private bool DirectLineToTarget(Collider target)
    {
        if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized,
            out RaycastHit directHit, 9000, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            return directHit.collider == target;
        else
            return false; //In this case, didn't hit anything... so I guess that's a no?
    }

    private bool CanDamageStructure(Transform t)
    {
        return God.GetDamageable(t) == null && !t.CompareTag("Essential") && !t.GetComponent<Terrain>()
            && !t.name.Equals("Floor");
    }

    //Returns nav mesh that needs to be updated as result of structure change, or null if N/A
    private INavZoneUpdater DamageStructure(Collider victim, float actualDamage)
    {
        if (!CanDamageStructure(victim.transform))
            return null;

        //float victimSize = victim.bounds.size.magnitude;
        //Debug.Log("Beating up " + victim.name + ": " + actualDamage + " damage, " + victimSize + " size");

        float blastEffect = actualDamage * 0.25f / Mathf.Pow(victim.bounds.size.magnitude, 2);

        Transform t = victim.transform;

        //Blast pushes object away based on blastStrength
        t.Translate((t.position - transform.position).normalized * blastEffect);

        //Blast randomly rotates object based on blastStrength
        Vector3 rotation = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        rotation *= blastEffect * 10;
        t.Rotate(rotation);

        //Vain attempt to get rotation based on angle of impact, taking blast strength and different forward axes into account
        /*
        Vector3 originalPos = t.position;
        Vector3 originalRot = t.eulerAngles;
        t.Translate((t.position - transform.position).normalized * blastEffect);
        t.LookAt(originalPos);
        t.Rotate(-originalRot); */

        return t.GetComponentInParent<INavZoneUpdater>();
    }

    //Called to deactive the explosion and either... destroy it OR put it back in reserve pool
    private void Decommission()
    {
        

        //Either pool or destroy
        explosionPool.PoolGameObject(gameObject);
    }

    public void CleanUp()
    {
        //Silence
        if (sfxSource)
        {
            sfxSource.Stop();
            God.god.UnmanageAudioSource(sfxSource);
        }

        //Hide
        visuals.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameObject.SetActive(false);
    }
}
