using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRay : MonoBehaviour, ManagedVolatileObject
{
    //STATIC POOLING OF DEATH RAYS----------------------------------------------------------------

    private static List<DeathRay> pooledDeathRays;

    public static void SetUpPooling()
    {
        pooledDeathRays = new List<DeathRay>();
    }

    //Retrieve death ray from pool if one exists, else return newly created one
    public static DeathRay GetDeathRay()
    {
        DeathRay deathRay;

        //Happy day: Back up in cache
        if (pooledDeathRays.Count > 0)
        {
            deathRay = pooledDeathRays[0];
            pooledDeathRays.Remove(deathRay);
            return deathRay;
        }

        //Sad day: nothing in cache
        //So return newly created death ray
        deathRay = Instantiate(Resources.Load<GameObject>("Planet/Projectiles/Death Ray")).GetComponent<DeathRay>();
        return deathRay;
    }

    private static void PoolDeathRay(DeathRay deathRay)
    {
        if (pooledDeathRays.Count <= 30)
            pooledDeathRays.Add(deathRay);
        else
            deathRay.DestroyDeathRay();
    }

    //DEATH RAY INSTANCE-------------------------------------------------------------------------- 

    private float lifetime = 1.0f; //Duration in seconds from emission to destruction/clean up
    private float rollingTime = 0.0f; //Time in seconds since emission

    public void Emit(Vector3 from, Vector3 rotation, float damage, float range, Pill launcher, float lifetime, float diameter, string impactEffect)
    {
        //Update variables
        rollingTime = 0.0f;
        this.lifetime = lifetime;

        //Set visuals
        Reskin(launcher);
        gameObject.SetActive(true);

        //Update reach, diameter, rotation and collisions of ray
        UpdateTransformAndCollisions(from, rotation, range, diameter, launcher, damage, impactEffect);

        //God script becomes responsible for regularly updating this object
        God.god.ManageVolatileObject(this);
    }

    private void UpdateTransformAndCollisions(Vector3 from, Vector3 rotation, float range, float diameter, Pill launcher, float damage, string impactEffect)
    {
        //The below steps need to be in order because each one is dependent on the one before...

        //1. Set rotation
        transform.eulerAngles = rotation;

        //2. Update collisions and get distance to collision point
        float distanceToCollision = CheckForCollision(from, range, diameter, launcher, damage, impactEffect);

        //3. Set scale
        transform.localScale = new Vector3(diameter, diameter, distanceToCollision < 0 ? range : distanceToCollision);

        //4. Set position
        transform.position = from;
        transform.Translate(Vector3.forward * transform.localScale.z / 2.0f, Space.Self);
    }

    //If there's a collision, returns distance between from and collision point. Else, returns -1
    private float CheckForCollision(Vector3 from, float range, float diameter, Pill launcher, float damage, string impactEffect)
    {
        //Perform collision check
        if (Physics.SphereCast(from, diameter / 2.0f, transform.forward, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            //Create special effects (noise, visuals, etc.) at impact point
            if (impactEffect != null && !impactEffect.Equals(""))
                ImpactEffect.impactEffectPool.GetGameObject(impactEffect).GetComponent<ImpactEffect>().CreateEffect(hit.point);

            //Apply damage to hit object
            Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
            if (damageable != null)
                damageable.Damage(damage, 0.0f, hit.point, DamageType.Projectile, launcher.team);

            //Return distance from point of emission to point of collision
            return hit.distance;
        }

        //Returning -1 to indicate there was no collision
        return -1.0f;
    }

    public void UpdateActiveStatus(float stepTime)
    {
        rollingTime += stepTime;

        if (rollingTime >= lifetime)
            Decomission();
    }

    private void Decomission()
    {
        //Stop updating
        God.god.UnmanageVolatileObject(this);

        //Hide
        gameObject.SetActive(false);

        //Either pool or destroy
        PoolDeathRay(this);
    }

    private void Reskin(Pill launcher)
    {
        Army army = Army.GetArmy(launcher.team);
        if (army)
        {
            transform.Find("Horizontal Faces").GetComponent<Renderer>().sharedMaterial = army.plasma1;
            transform.Find("Vertical Faces").GetComponent<Renderer>().sharedMaterial = army.plasma1;
        }
    }

    private void DestroyDeathRay()
    {
        //Perform clean up here

        //Destroy game object
        Destroy(gameObject);
    }
}
