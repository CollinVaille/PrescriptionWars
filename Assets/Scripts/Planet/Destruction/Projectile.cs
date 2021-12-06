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

    private float distanceCovered = 0.0f, actualLaunchSpeed = 0.0f;

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
        float transitiveVelocity = transform.InverseTransformVector(launcher.GetRootGlobalVelocity()).z;
        actualLaunchSpeed = transitiveVelocity <= 0 ? launchSpeed : launchSpeed + transitiveVelocity;
        if (rBody)
            rBody.AddRelativeForce(Vector3.forward * actualLaunchSpeed, ForceMode.VelocityChange);
        else
            God.god.ManageVolatileObject(this);
    }

    //Used to update the collision detection, movement, etc...
    public void UpdateActiveStatus(float stepTime)
    {
        float stepDistance = actualLaunchSpeed * stepTime;

        //Check for collision
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stepDistance, ~0, QueryTriggerInteraction.Ignore))
            Impact(hit.transform);

        //Spin for cool effect
        transform.Rotate(Vector3.forward * stepTime * 720, Space.Self);

        //Move forward
        if(!rBody)
            transform.Translate(Vector3.forward * stepDistance, Space.Self);

        //Time to put to pasture?
        distanceCovered += stepDistance;
        if (distanceCovered > range)
            Decommission();
    }

    //Called when the projectile hits something
    private void Impact(Transform hit)
    {
        Damageable victim = God.GetDamageable(hit);
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
