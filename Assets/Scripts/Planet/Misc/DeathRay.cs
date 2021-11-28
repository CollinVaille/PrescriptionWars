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

    public void Emit(Vector3 from, Vector3 rotation, float damage, float range, Pill launcher, float lifetime)
    {
        //Reset state
        rollingTime = 0.0f;

        //Save status
        this.lifetime = lifetime;

        //Prepare to turn on
        Reskin(launcher);

        //Set to new positioning and turn on
        UpdateTransform(from, rotation, range);
        gameObject.SetActive(true);

        CheckForCollision();

        //God script becomes responsible for regularly updating this object
        God.god.ManageVolatileObject(this);
    }

    private void UpdateTransform(Vector3 from, Vector3 rotation, float range)
    {
        //Set rotation
        transform.eulerAngles = rotation;

        //Set scale
        transform.localScale = new Vector3(0.1f, 0.1f, range);

        //Set position
        transform.position = from;
        transform.Translate(Vector3.forward * range / 2.0f, Space.Self);
    }

    private void CheckForCollision()
    {
        //Physics.Box
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
