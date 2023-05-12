using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, ManagedVolatileObject, PlanetPooledObject
{
    public enum ProjectileType { Default, Laser }

    public static PlanetObjectPool projectilePool;

    //Launch parameters
    private float damage = 34;
    private float knockback = 200;
    private float range = 300;
    private Pill launcher;

    //Customization
    public ProjectileType projectileType = ProjectileType.Default;
    public float launchSpeed = 40;
    public string explosionName;

    //References
    private Rigidbody rBody;
    private AudioSource sfxSource;

    //Status variables
    private float distanceCovered = 0.0f;
    private Vector3 actualLaunchVelocity; //In global space

    public void OneTimeSetUp()
    {
        //Get references
        rBody = GetComponent<Rigidbody>();
        sfxSource = GetComponent<AudioSource>();

        //Remove (Clone) from end of name (necessary for pooling to work)
        name = name.Substring(0, name.Length - 7);
    }

    public void Launch(float damage, float knockback, float range, Pill launcher)
    {
        if (!launcher)
        {
            Debug.Log("Projectile emitted but no one shot it?");
            Decommission();
        }
        else
        {
            //First, remember our mission briefing
            this.damage = damage;
            this.knockback = knockback;
            this.range = range;
            this.launcher = launcher;

            //Then, theme the projectile's appearance to the faction color
            Reskin();

            //Thereafter, randomize initial rotation to make it look cooler
            transform.Rotate(Vector3.forward * Random.Range(0, 90), Space.Self);

            //Finally, proceed with launch!
            distanceCovered = 0.0f;
            gameObject.SetActive(true);

            //Bwwaaaaahhh
            if (sfxSource)
            {
                God.god.ManageAudioSource(sfxSource);
                sfxSource.Play();
            }

            //Raahhhhhhhh
            actualLaunchVelocity = transform.TransformVector(Vector3.forward * launchSpeed); //Start with velocity that would be correct if the launcher was standing still

            Rigidbody referenceRBody = launcher.GetComponentInParent<Rigidbody>(); //Then add on the velocity that we transitively get from the launcher
            if (referenceRBody)
                actualLaunchVelocity += referenceRBody.velocity;

            if (rBody)
                rBody.AddForce(actualLaunchVelocity, ForceMode.VelocityChange);
            else
                God.god.ManageVolatileObject(this);
        }
    }

    //Used to update the collision detection, movement, etc...
    public void UpdateActiveStatus(float stepTime)
    {
        Vector3 stepDelta = actualLaunchVelocity * stepTime;

        //Check for collision
        if (Physics.Raycast(transform.position, stepDelta, out RaycastHit hit, stepDelta.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            IDamageable victim = hit.collider.GetComponentInParent<IDamageable>();

            if (victim == null || victim as Pill != launcher) //Make sure we don't collide with the very pill that launched us
                Impact(victim);
        }

        //Spin for cool effect
        transform.Rotate(Vector3.forward * stepTime * 720, Space.Self);

        //Move forward
        if(!rBody)
            transform.Translate(stepDelta, Space.World);

        //Time to put to pasture?
        distanceCovered += stepDelta.magnitude;
        if (distanceCovered > range)
            Decommission();
    }

    //Called when the projectile hits something
    private void Impact(IDamageable victim)
    {
        if (victim != null)
        {
            victim.Damage(damage, knockback, transform.position, DamageType.Projectile, launcher.team);

            if (Player.player.GetPill() == launcher)
                Player.player.GetAudioSource().PlayOneShot(Player.player.hitMarker);
        }

        if(!explosionName.Equals(""))
        {
            Explosion explosion = Explosion.explosionPool.GetGameObject(explosionName).GetComponent<Explosion>();
            explosion.transform.position = transform.position;
            explosion.transform.rotation = transform.rotation;
            explosion.Explode(launcher.team);
        }
        else if(projectileType == ProjectileType.Laser) //Create particle system at point of impact
            ImpactEffect.impactEffectPool.GetGameObject("Plasma Impact").GetComponent<ImpactEffect>().CreateEffect(transform.position);

        Decommission();
    }

    //Called to deactive the projectile and either... destroy it OR put it back in reserve pool
    private void Decommission()
    {
        //Either pool or destroy
        projectilePool.PoolGameObject(gameObject);
    }

    private void Reskin()
    {
        if(projectileType == ProjectileType.Laser)
        {
            Army army = Army.GetArmy(launcher.team);
            if(army)
            {
                transform.Find("Top Face").GetComponent<Renderer>().sharedMaterial = army.plasma1;
                transform.Find("Side Face").GetComponent<Renderer>().sharedMaterial = army.plasma2;
            }
        }
    }

    public void CleanUp()
    {
        //Silence
        if (sfxSource)
        {
            sfxSource.Stop();
            God.god.UnmanageAudioSource(sfxSource);
        }

        //Stop updating
        if (rBody)
            rBody.velocity = Vector3.zero;
        else
            God.god.UnmanageVolatileObject(this);

        //Hide
        gameObject.SetActive(false);
    }
}
