using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : Item
{
    //Customization
    public AudioClip flameStopSound;

    //References
    private AudioSource sfxSource;
    private ParticleSystem retardantStream;
    private Transform nozzle, trigger;

    //Status
    private int actionCode = 0;

    private void Start()
    {
        //Get references
        sfxSource = GetComponent<AudioSource>();
        retardantStream = transform.Find("Retardant Stream").GetComponent<ParticleSystem>();
        nozzle = transform.Find("Nozzle");
        trigger = transform.Find("Trigger");

        //Pause audio on pause menu
        God.god.ManageAudioSource(sfxSource);
    }

    public override void PrimaryAction() { StartCoroutine(ShootRetardant(true)); }

    public override void SecondaryAction() { StartCoroutine(ShootRetardant(false)); }

    private IEnumerator ShootRetardant(bool shootingOutwards)
    {
        int actionKey = ++actionCode;
        float startTime = Time.timeSinceLevelLoad;
        float shootRange = 10.0f;

        //Pull trigger
        SetTriggerVisual(true);

        //Position and rotate fire extinguisher
        PositionAndRotate(shootingOutwards);

        //Start effects
        if (shootingOutwards)
            retardantStream.Play(true);
        else
            retardantStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        sfxSource.Play();

        //Lifetime of single spurt
        IDamageable us = transform.GetComponentInParent<IDamageable>();
        while (actionKey == actionCode && holder)
        {
            //Player/bot specific loop guards/conditions
            if (holderIsPlayer) //Player stops when release button
            {
                if (shootingOutwards && !Input.GetButton("Primary Action"))
                    break;
                else if (!shootingOutwards && !Input.GetButton("Secondary Action"))
                    break;
            }
            else if (Time.timeSinceLevelLoad - startTime > Random.Range(1.5f, 5.0f)) //Bot stops after timer
                break;

            //Update visuals
            if (!shootingOutwards)
                transform.localEulerAngles = new Vector3(0, Random.Range(135, 225), 0);

            if (shootingOutwards)
            {
                //Get list of hit targets
                RaycastHit[] hits = Physics.SphereCastAll(transform.TransformPoint(0.0f, 0.12f, 3.6f),
                    3.0f, Vector3.forward, shootRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

                //Apply effects to targets that should be affected
                if (hits != null)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        Transform hitTransform = hit.collider.transform;

                        //Can't touch ourselves and can't go through walls
                        if (hitTransform.GetComponentInParent<IDamageable>() == us || !DirectLineToTarget(hit.collider))
                            continue;

                        PutOutFire(hitTransform);
                    }
                }
            }
            else
                PutOutFire(holder.transform);

            //Cooldown until next update
            yield return new WaitForSeconds(0.25f);
        }

        //Stop effects
        if(actionKey == actionCode)
        {
            retardantStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            sfxSource.Stop();

            //Release trigger
            SetTriggerVisual(false);

            //Return to normal way we hold fire extinguisher
            if (!shootingOutwards && holder)
                PositionAndRotate(true);
        }
    }

    private void SetTriggerVisual(bool pulled)
    {
        if(pulled) //Set to pulled position
        {
            trigger.localPosition = new Vector3(0, 0.425f, -0.1f);
            trigger.localEulerAngles = Vector3.zero;
        }
        else //Set to released position
        {
            trigger.localPosition = new Vector3(0, 0.4f, -0.1f);
            trigger.localEulerAngles = new Vector3(-20, 0, 0);
        }
    }

    private void PositionAndRotate(bool forwards)
    {
        float yPos = transform.localPosition.y;

        if (forwards)
        {
            transform.localPosition = GetPlaceInPlayerHand();
            transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            transform.position = holder.transform.TransformPoint(0.0f, -0.25f, 1.25f);
            transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        //Player/bot specific positioning
        if (holderIsPlayer) //Remember item bob height
            SetYPosition(yPos);
        else //Bot held items have fixed offset to player held items
            transform.localPosition += Vector3.up * 0.5f;
    }

    private void SetYPosition(float newLocalY)
    {
        Vector3 localPos = transform.localPosition;
        localPos.y = newLocalY;
        transform.localPosition = localPos;
    }

    private bool DirectLineToTarget(Collider target)
    {
        if (Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized,
            out RaycastHit directHit, 9000, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            return directHit.collider == target;
        else
            return false; //In this case, didn't hit anything... so I guess that's a no?
    }

    private void PutOutFire(Transform hitTransform)
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

            sfxSource.PlayOneShot(flameStopSound);
        }
    }

    public override void PutInHand(PlanetPill newHolder)
    {
        base.PutInHand(newHolder);

        //Flip nozzle up
        nozzle.localEulerAngles = new Vector3(-90, 0, 0);
        nozzle.Find("Tip").gameObject.layer = 9; //Make nozzle blunt weapon
    }

    public override void RetireFromHand()
    {
        //Flick nozzle down
        nozzle.localEulerAngles = Vector3.zero;
        nozzle.Find("Tip").gameObject.layer = 0; //Make nozzle harmless

        base.RetireFromHand();
    }

    public override Vector3 GetPlaceInPlayerHand() { return new Vector3(0.5f, -0.5f, 0.5f); }

    public override Vector3 GetPlaceOnBack() { return new Vector3(0.0f, 0.25f, -0.5f); }
    public override Vector3 GetRotationOnBack() { return new Vector3(-90, 0, -90); }

    public override Vector3 GetPlaceInItemRack() { return new Vector3(0.0f, 0.25f, 0.0f); }
    public override Vector3 GetRotationInItemRack() { return new Vector3(0, Random.Range(-180, 180), 0); }

    public override Vector3 GetPlaceInHolster() { return new Vector3(0.5f, -0.1f, 0); }
    public override Vector3 GetRotationInHolster() { return new Vector3(115, 0, 0); }
}
