using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WD40 : RepairTool
{
    //Customization
    public AudioClip flickNozzleUp, flickNozzleDown, flameStopSound, flameThrower, fireExtinguisher;

    //Action code is responsible for coordinating which coroutine is active
    //If a running coroutine has a key that is out-of-date, it means another coroutine has replaced it
    private int actionCode = 0;

    private bool shootingFlames = false;

    //References
    private AudioSource shootingSFXSource;

    protected override void Start()
    {
        base.Start();

        shootingSFXSource = GetComponents<AudioSource>()[1];

        //Pause audio on pause menu
        God.god.ManageAudioSource(shootingSFXSource);
    }

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

    public override Vector3 GetRotationInHolster() { return new Vector3(90, 0, 0); }

    public override void SecondaryAction() { StartCoroutine(ShootSubstance(false)); }

    public override void TertiaryAction() { StartCoroutine(ShootSubstance(true)); }

    private IEnumerator ShootSubstance(bool substanceIsFlames)
    {
        shootingFlames = substanceIsFlames;

        int actionKey = ++actionCode;
        float startTime = Time.timeSinceLevelLoad;
        float shootRange;

        //Start effects
        if(substanceIsFlames)
        {
            shootRange = 15.0f;
            transform.Find("Flame Stream").GetComponent<ParticleSystem>().Play(true);
            shootingSFXSource.clip = flameThrower;
        }
        else
        {
            shootRange = 10.0f;
            transform.Find("Retardant Stream").GetComponent<ParticleSystem>().Play(true);
            shootingSFXSource.clip = fireExtinguisher;
        }
        shootingSFXSource.Play();

        //Lifetime of single spurt
        IDamageable us = transform.GetComponentInParent<IDamageable>();
        while (actionKey == actionCode && holder)
        {
            //Player/bot specific loop guards/conditions
            if (holderIsPlayer) //Player stops when release button
            {
                if (substanceIsFlames && !Input.GetButton("Tertiary Action"))
                    break;
                else if (!substanceIsFlames && !Input.GetButton("Secondary Action"))
                    break;
            }
            else if (Time.timeSinceLevelLoad - startTime > Random.Range(1.5f, 5.0f)) //Bot stops after timer
                break;

            //Get list of hit targets
            RaycastHit[] hits = Physics.SphereCastAll(transform.TransformPoint(0.0f, 0.12f, 3.6f),
                3.0f, Vector3.forward, shootRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            //Apply effects to targets that should be affected
            if (hits != null)
            {
                foreach(RaycastHit hit in hits)
                {
                    Transform hitTransform = hit.collider.transform;

                    //Can't touch ourselves and can't go through walls
                    if (hitTransform.GetComponentInParent<IDamageable>() == us || !DirectLineToTarget(hit.collider))
                        continue;

                    if (substanceIsFlames)
                        SetOnFire(hit, hitTransform);
                    else
                        PutOutFire(hit, hitTransform);
                }
            }

            //Cooldown until next update
            yield return new WaitForSeconds(0.25f);
        }

        //Stop effects
        if (actionKey == actionCode || shootingFlames != substanceIsFlames)
        {
            transform.Find(substanceIsFlames ? "Flame Stream" : "Retardant Stream")
                .GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (actionKey == actionCode)
            shootingSFXSource.Stop();

        if (substanceIsFlames)
            shootingSFXSource.PlayOneShot(flameStopSound);
    }

    private void SetOnFire(RaycastHit hit, Transform hitTransform)
    {
        if (Fire.IsFlammable(hitTransform))
            Fire.SetOnFire(hitTransform, Vector3.zero, Mathf.Min(10.0f / hit.distance, 5.0f));
    }

    private void PutOutFire(RaycastHit hit, Transform hitTransform)
    {
        Fire subjectFire = Fire.GetSubjectFire(hitTransform);

        if (subjectFire)
        {
            if (subjectFire.intensity > 1.0f)
                subjectFire.intensity *= 0.9f;
            else if (subjectFire.intensity > 0.5f)
                subjectFire.intensity *= 0.75f;
            else
                subjectFire.intensity *= 0.1f;

            shootingSFXSource.PlayOneShot(flameStopSound);
        }
    }

    private bool DirectLineToTarget (Collider target)
    {
        if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized,
            out RaycastHit directHit, 9000, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            return directHit.collider == target;
        else
            return false; //In this case, didn't hit anything... so I guess that's a no?
    }
}
