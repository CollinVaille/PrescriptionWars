using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehiclePart : MonoBehaviour, IDamageable
{
    protected Vehicle belongsTo;
    private Collider partCollider;

    public float health = 500;
    protected bool working = true;
    protected float initialHealth;

    public AudioClip partFailure;

    protected virtual void Start()
    {
        //Determine which vehicle the part belongs to
        belongsTo = transform.GetComponentInParent<Vehicle>();

        //Get other references
        partCollider = GetComponent<Collider>();

        //Remember initial health
        initialHealth = health;
    }

    public void Damage(float damage, float knockback, Vector3 from, DamageType damageType, int team)
    {
        //Translate/rotate part(s)
        if (damageType == DamageType.Explosive)
        {
            belongsTo.DamageParts(partCollider.ClosestPointOnBounds(from), 1000, damage, true);
            //Damage is applied on subsequent recursive call
        }
        else
        {
            if (team != -53) //-53 is special code indicating DamagePart called this function
            {
                belongsTo.DamagePart(transform, partCollider.ClosestPointOnBounds(from), 1, damage, false);

                if(damageType != DamageType.Fire)
                    belongsTo.PlayDamageSound(damage);
            }

            DamageHealth(damage);
        }
    }

    protected virtual void DamageHealth(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            health = 0;

            //Can only fail once
            if (!working)
                return;

            PartFailure();
        }
    }

    protected virtual void PartFailure()
    {
        working = false;

        belongsTo.GetGeneralAudio().PlayOneShot(partFailure);
    }

    protected virtual void PartRecovery()
    {
        working = true;
    }

    public void Repair(float repairPoints)
    {
        //Add health points
        health += repairPoints;
        if (health > initialHealth)
            health = initialHealth;

        //Part has recovered if goes back above zero
        if (!working && health > 0)
            PartRecovery();
    }

    public float GetHealthPercentage() { return health / initialHealth; }
}
