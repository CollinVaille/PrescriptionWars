using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathRay : MonoBehaviour, ManagedVolatileObject, PlanetPooledObject
{
    public static PlanetObjectPool deathRayPool;

    private float lifetime = 1.0f; //Duration in seconds from emission to destruction/clean up
    private float rollingTime = 0.0f; //Time in seconds since emission

    public void OneTimeSetUp()
    {
        //Required by PlanetPooledObject interface. So far, nothing to do here.
    }

    public void Emit(Vector3 from, Vector3 rotation, float damage, float range, Pill launcher, float lifetime, float diameter)
    {
        if(!launcher)
        {
            Debug.Log("Death ray emitted but no one shot it?");
            Decomission();
        }
        else
        {
            //Update variables
            rollingTime = 0.0f;
            this.lifetime = lifetime;

            //Set visuals
            Reskin(launcher);
            gameObject.SetActive(true);

            //Update reach, diameter, rotation and collisions of ray
            UpdateTransformAndCollisions(from, rotation, range, diameter, launcher, damage);

            //God script becomes responsible for regularly updating this object
            God.god.ManageVolatileObject(this);
        }
    }

    private void UpdateTransformAndCollisions(Vector3 from, Vector3 rotation, float range, float diameter, Pill launcher, float damage)
    {
        //The below steps need to be in order because each one is dependent on the one before...

        //1. Set rotation
        transform.eulerAngles = rotation;

        //2. Update collisions and get distance to collision point
        float distanceToCollision = CheckForCollision(from, range, diameter, launcher, damage);

        //3. Set scale
        transform.localScale = new Vector3(diameter, diameter, distanceToCollision < 0 ? range : distanceToCollision);

        //4. Set position
        transform.position = from;
        transform.Translate(Vector3.forward * transform.localScale.z / 2.0f, Space.Self);
    }

    //If there's a collision, returns distance between from and collision point. Else, returns -1
    private float CheckForCollision(Vector3 from, float range, float diameter, Pill launcher, float damage)
    {
        //Perform collision check
        if (Physics.SphereCast(from, diameter / 2.0f, transform.forward, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            //Create special effects (noise, visuals, etc.) at impact point
            ImpactEffect.impactEffectPool.GetGameObject("Plasma Impact").GetComponent<ImpactEffect>().CreateEffect(hit.point);

            //Apply damage to hit object
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
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
        deathRayPool.PoolGameObject(gameObject);
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

    public void CleanUp()
    {
        //Stop updating
        God.god.UnmanageVolatileObject(this);

        //Hide
        gameObject.SetActive(false);
    }
}
