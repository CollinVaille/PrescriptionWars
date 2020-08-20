using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WD40 : RepairTool
{
    public AudioClip flickNozzleUp, flickNozzleDown;

    private bool shootingFlames = false;
    private int flamesCode = 0;

    public override void PutInHand(Pill newHolder)
    {
        base.PutInHand(newHolder);

        //Flip nozzle up
        transform.Find("Nozzle").localEulerAngles = new Vector3(-90, 0, 0);
        transform.Find("Nozzle").Find("Tip").gameObject.layer = 9; //Make nozzle blunt weapon
        newHolder.GetAudioSource().PlayOneShot(flickNozzleUp);

    }

    public override void RetireFromHand()
    {
        //Flick nozzle down
        transform.Find("Nozzle").localEulerAngles = Vector3.zero;
        transform.Find("Nozzle").Find("Tip").gameObject.layer = 0; //Make nozzle harmless
        holder.GetAudioSource().PlayOneShot(flickNozzleDown);

        base.RetireFromHand();
    }

    public override Vector3 GetPlaceInPlayerHand() { return new Vector3(0.5f, -0.25f, 0.4f); }

    public override Vector3 GetRotationInItemRack() { return new Vector3(0, Random.Range(-180, 180), 0); }

    public override void SecondaryAction()
    {
        if (!shootingFlames)
            StartCoroutine(ShootFlames());
    }

    private IEnumerator ShootFlames()
    {
        //Increment flamesCode to stop at any time
        int flamesKey = ++flamesCode;
        float startTime = Time.timeSinceLevelLoad;

        //Start flame effects
        transform.Find("Flame Stream").GetComponent<ParticleSystem>().Play(true);

        //Lifetime of single flame spurt
        Damageable us = God.GetDamageable(transform);
        while (flamesKey == flamesCode)
        {
            //Player/bot specific loop guards/conditions
            if (holderIsPlayer) //Player stops when release button
            {
                if (!Input.GetButton("Secondary Action"))
                    break;
            }
            else if (Time.timeSinceLevelLoad - startTime > Random.Range(1.5f, 5.0f)) //Bot stops after timer
                break;

            //Get list of hit targets
            RaycastHit[] hits = Physics.SphereCastAll(transform.TransformPoint(0.0f, 0.12f, 3.6f),
                3.0f, Vector3.forward, 12.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            //Set targets on fire if flammable
            if (hits != null)
            {
                foreach(RaycastHit hit in hits)
                {
                    //Don't light ourselves on fire
                    if (God.GetDamageable(hit.transform) == us)
                        continue;

                    //Light other, flammable things on fire
                    if (Fire.IsFlammable(hit.transform))
                        Fire.SetOnFire(hit.transform, Vector3.zero, Mathf.Min(10.0f / hit.distance, 5.0f));
                }
            }

            //Cooldown until next update
            yield return new WaitForSeconds(0.25f);
        }

        //Stop flame effects
        transform.Find("Flame Stream").GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
