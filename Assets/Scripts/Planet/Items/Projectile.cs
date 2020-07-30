using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //Launch parameters
    private float damage = 34;
    private float knockback = 200;
    private float range = 300;
    private int team = 0;

    public float launchSpeed = 40;

    private Rigidbody rBody;
    private AudioSource sfxSource;

    private float distanceCovered = 0.0f;

    public void SetUp()
    {
        rBody = GetComponent<Rigidbody>();
        sfxSource = GetComponent<AudioSource>();
    }

    public void Launch(float damage, float knockback, float range, int team)
    {
        //First, remember our mission briefing
        this.damage = damage;
        this.knockback = knockback;
        this.range = range;
        this.team = team;

        //Then, proceed with launch
        distanceCovered = 0.0f;

        //Its a go houston
        gameObject.SetActive(true);

        //Bwwaaaaahhh
        if (sfxSource)
            sfxSource.Play();

        //Raahhhhhhhh
        if (rBody)
            rBody.AddRelativeForce(Vector3.forward * launchSpeed, ForceMode.VelocityChange);
        else
            God.god.ManageProjectile(this);
    }

    //Used to update the collision detection, movement, and enforce the range limit of the projectile
    public void UpdateLaunchedProjectile(float stepTime)
    {
        float stepDistance = launchSpeed * stepTime;

        //Check for collision
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stepDistance, ~0, QueryTriggerInteraction.Ignore))
            Impact(hit.transform);

        //Move forward
        transform.Translate(Vector3.forward * stepDistance, Space.Self);

        //Time to put to pasture?
        distanceCovered += stepDistance;
        if (distanceCovered > range)
            Decommission();
    }

    //Called when the projectile hits something
    private void Impact(Transform hit)
    {
        Debug.Log("Hit " + hit.name);

        Damageable victim = hit.GetComponent<Damageable>();
        if (victim != null)
            victim.Damage(damage, knockback, transform.position, DamageType.Projectile, team);

        Decommission();
    }

    //Called to deactive the projectile and destroy it OR put it back in reserve pool
    private void Decommission()
    {
        //Silence
        if (sfxSource)
            sfxSource.Stop();

        //Stop updating
        if (rBody)
            rBody.velocity = Vector3.zero;
        else
            God.god.UnmanageProjectile(this);

        //Hide
        gameObject.SetActive(false);
    }
}
