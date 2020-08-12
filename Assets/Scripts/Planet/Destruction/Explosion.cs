using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    //STATIC POOLING OF EXPLOSIONS----------------------------------------------------------------

    private static Dictionary<string, List<Explosion>> pooledExplosions;

    public static void SetUpPooling()
    {
        pooledExplosions = new Dictionary<string, List<Explosion>>();
    }

    //Retrieve explosion from pool if one exists, else return newly created one
    public static Explosion GetExplosion(string explosionName)
    {
        pooledExplosions.TryGetValue(explosionName, out List<Explosion> explosions);

        Explosion explosion;

        //Happy day: list exists and is not empty
        //So return pooled explosion
        if (explosions != null && explosions.Count != 0)
        {
            explosion = explosions[Random.Range(0, explosions.Count)];
            explosions.Remove(explosion);
            return explosion;
        }

        //Sad day: either list doesn't exist or is empty
        //So return newly created explosion
        explosion = Instantiate(Resources.Load<GameObject>("Explosions/" + explosionName)).GetComponent<Explosion>();
        explosion.InitialSetUp(); //One-time initialization on creation
        return explosion;
    }

    //Put explosion in pool unless pool is full, in which case destroy explosion
    private static void PoolExplosion(Explosion explosion)
    {
        pooledExplosions.TryGetValue(explosion.name, out List<Explosion> explosions);

        if (explosions == null) //No such pool, so create pool and add explosion to it
        {
            explosions = new List<Explosion>(); //Create pool
            explosions.Add(explosion); //Add explosion to pool
            pooledExplosions.Add(explosion.name, explosions); //Add pool to list of pools
        }
        else //Found explosion pool, so see if explosion fits...
        {
            if (explosions.Count > 30) //Too many pooled so just destroy explosion
                explosion.DestroyExplosion();
            else //There's still room in pool, so put it in there
                explosions.Add(explosion);
        }
    }

    //EXPLOSION INSTANCE--------------------------------------------------------------------------

    //Explosion parameters
    private int team = 0;

    //Customization
    public float range = 20;
    public float damage = 50;

    //References
    private AudioSource sfxSource;
    private ParticleSystem visuals;

    private void InitialSetUp()
    {
        //Get references
        sfxSource = GetComponent<AudioSource>();
        visuals = GetComponent<ParticleSystem>();

        //Makes pause menu pause/resume audio appropriately
        God.god.ManageAudioSource(sfxSource);

        //Remove (Clone) from end of name (necessary for pooling to work)
        name = name.Substring(0, name.Length - 7);
    }

    public void Explode(int team)
    {
        //Remember explosion parameters
        this.team = team;

        //Start explosion
        gameObject.SetActive(true);
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

    //Apply damage to damageables, force to rigidbodies, and destruction to destructable objects like walls and windows
    private void ExplosionEffects(float currentRange)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRange);

        foreach(Collider hit in hits)
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
                else if (CanBeatUpObject(hit.transform))
                    Debug.Log("Can beat up " + hit.transform.name);
            }
        }
    }

    private bool DirectLineToTarget(Collider target)
    {
        if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized,
            out RaycastHit directHit))
            return directHit.collider == target;
        else
            return false; //In this case, didn't hit anything... so I guess that's a no?
    }

    private bool CanBeatUpObject (Transform t)
    {
        return God.GetDamageable(t) == null && !t.CompareTag("Essential") && !t.GetComponent<Terrain>();
    }

    //Called to deactive the explosion and either... destroy it OR put it back in reserve pool
    private void Decommission()
    {
        //Silence
        if (sfxSource)
            sfxSource.Stop();

        //Hide
        visuals.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameObject.SetActive(false);

        //Either pool or destroy
        PoolExplosion(this);
    }

    //Call this instead of Object.Destroy to ensure all needed clean up is performed
    private void DestroyExplosion()
    {
        God.god.UnmanageAudioSource(sfxSource);
        Destroy(gameObject);
    }
}
