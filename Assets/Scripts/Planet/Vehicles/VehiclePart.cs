using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehiclePart : MonoBehaviour, Damageable
{
    protected Vehicle belongsTo;
    private Collider partCollider;

    public float health = 500;
    protected bool working = true;

    public AudioClip partFailure;

    protected virtual void Start()
    {
        //Determine which vehicle part belongs to
        Transform t = transform.parent;
        while (t && !belongsTo)
        {
            belongsTo = t.GetComponent<Vehicle>();

            t = t.parent;
        }

        //Get other references
        partCollider = GetComponent<Collider>();
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

        if(belongsTo.PoweredOn())
            belongsTo.GetGeneralAudio().PlayOneShot(partFailure);
    }
}
