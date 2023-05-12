using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machete : Item
{
    public AudioClip boomerang, doink, heavySwoosh, finishingMove;
    private bool inFlight = false;
    private Collider collidedWith = null;

    public override void PrimaryAction()
    {
        if (!holder)
            return;

        StartCoroutine(ExpensiveStab(0.6f, new Vector3(0, 0, Random.Range(60, 90)), heavySwoosh));
    }

    //Throw bommerang
    public override void SecondaryAction()
    {
        if (!holder)
            return;

        ThrowBoomerang();
    }

    private void ThrowBoomerang()
    {
        if (inFlight || !holder)
            return;

        inFlight = true;
        Pill thrower = holder;

        StartCoroutine(BoomerangFlight(thrower));
    }

    private IEnumerator BoomerangFlight(Pill thrower)
    {
        //---------------------------------------------------------------------------------------------------------------------------------------------
        //SET UP

        Vector3 newPosition = transform.position + transform.forward * 1.0f;
        thrower.Equip(null, false);
        transform.position = newPosition;
        
        Rigidbody rBody = gameObject.AddComponent<Rigidbody>();
        rBody.useGravity = true;
        rBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        collidedWith = null;

        //Start boomerang sound
        AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
        God.god.ManageAudioSource(sfxSource);
        sfxSource.spatialBlend = 1.0f;
        sfxSource.volume = 1.0f;
        sfxSource.loop = true;
        sfxSource.clip = boomerang;
        sfxSource.Play();

        //---------------------------------------------------------------------------------------------------------------------------------------------
        //OUTBOUND FLIGHT

        float oneWayDuration = 2.0f, stepIncrement = 0.1f, translationSpeed = 30.0f, rotationSpeed = 1000.0f;
        Vector3 forward = transform.forward;

        //Flying towards
        for(float t = 0.0f; t < oneWayDuration; t += stepIncrement)
        {
            if(collidedWith != null)
            {
                ApplyBoomerangHitDamageAndSound(sfxSource, thrower, rBody);

                //Stop with forward motion since we hit something
                collidedWith = null;
                break;
            }

            //Spin
            transform.Rotate(Vector3.right * rotationSpeed * stepIncrement, Space.Self);

            //Translate forward
            rBody.MovePosition(rBody.position + forward * translationSpeed * stepIncrement);

            //Wait
            yield return new WaitForSeconds(stepIncrement);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------
        //INBOUND RETURN FLIGHT

        //Flying back
        Vector3 newRotation = transform.localEulerAngles;
        int hitsOnWayBack = 0;
        for (float t = 0.0f; (t < 3.0f || rBody.velocity.magnitude > 1) && t < 30.0f; t += stepIncrement)
        {
            if (collidedWith != null)
            {
                ApplyBoomerangHitDamageAndSound(sfxSource, thrower, rBody);

                //Impact slows down movement and rotation
                translationSpeed *= 0.75f;
                rotationSpeed *= 0.75f;

                //We are done processing collision...
                collidedWith = null;
                hitsOnWayBack++;

                //Determine what to do now (running "break" will end return flight, whereas not doing a "break" will continue the flight)
                if (hitsOnWayBack > 2 || CloseToThrower(thrower.transform))
                    break;
            }

            //Look back at thrower on local y and z axes. Keep x axis unaffected so it can keep spinning
            newRotation.x = transform.localEulerAngles.x;
            transform.LookAt(thrower.transform);
            newRotation.y = transform.localEulerAngles.y;
            newRotation.z = transform.localEulerAngles.z;
            transform.localEulerAngles = newRotation;

            //Spin
            transform.Rotate(Vector3.right * rotationSpeed * stepIncrement, Space.Self);

            //Translate forward
            Vector3 dir = (thrower.transform.position - transform.position).normalized;
            rBody.MovePosition(rBody.position + dir * translationSpeed * stepIncrement);
            
            //Wait
            yield return new WaitForSeconds(stepIncrement);
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------
        //CONCLUSION

        //Stop sounds and clean up sfx source
        God.god.UnmanageAudioSource(sfxSource);
        sfxSource.Stop();
        sfxSource.clip = null;
        Destroy(sfxSource);

        if (!thrower.GetItemInHand() && CloseToThrower(thrower.transform)) //Back in hand
            thrower.Equip(this, false);
        else //Falls to ground
        {
            AudioSource.PlayClipAtPoint(doink, transform.position);

            //Can now grab it off ground
            gameObject.layer = 10;

            //Need trigger collider again since we're putting it back on ground
            if (GetComponent<Collider>())
                GetComponent<Collider>().enabled = true;
        }

        inFlight = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Record the hit for later processing
        if (inFlight)
            collidedWith = collision.collider;
    }

    private void ApplyBoomerangHitDamageAndSound(AudioSource sfxSource, Pill thrower, Rigidbody rBody)
    {
        //Removing a cool feature
        if (thrower == collidedWith.GetComponent<Pill>())
            return;

        //Apply damage and sound effects
        IDamageable damageable = collidedWith.transform.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.Damage(meleeDamage, meleeKnockback, transform.position, DamageType.Projectile, thrower.team);
            sfxSource.PlayOneShot(stab);
            if (thrower == Player.player)
                Player.player.PlayHitMarkerSound(false);
        }
        else
        {
            sfxSource.PlayOneShot(doink);

            if (thrower == Player.player && collidedWith.CompareTag("Gear"))
                Player.player.PlayHitMarkerSound(true);
        }

        //Bounce boomerang out and up upon impact
        rBody.AddExplosionForce(10.0f, collidedWith.transform.position, 10000.0f);
    }

    private bool CloseToThrower(Transform thrower) { return Vector3.Distance(transform.position, thrower.position) < 3; }

    public override void OnMeleeKill(Pill pill)
    {
        if (!executing && stabbing && holder && holder.StabbingWithIntentToExecute(0))
            StartCoroutine(PerformStabbingExecution(pill, DavyJonesLocker.GetResident(pill)));
    }

    private IEnumerator PerformStabbingExecution(Pill victimPill, Corpse victimCorpse)
    {
        //Set status as executing
        executing = true;

        //Prepare corpse for execution
        victimCorpse.beingExecuted = true;
        Destroy(victimCorpse.GetComponent<Rigidbody>());
        victimCorpse.transform.parent = transform;

        //Modify local position of corpse
        Vector3 originalCorpsePosition = victimCorpse.transform.localPosition;
        Vector3 corpsePosition = originalCorpsePosition;
        corpsePosition.x = 0;
        corpsePosition.z = Mathf.Min(1.0f, corpsePosition.z);
        victimCorpse.transform.localPosition = corpsePosition;

        //Add a bit of drama
        AudioSource screamer = victimCorpse.gameObject.AddComponent<AudioSource>();
        screamer.spatialBlend = 1.0f;
        screamer.volume = 1.0f;
        screamer.clip = victimPill.voice.GetOof();
        screamer.Play();
        God.god.ManageAudioSource(screamer);

        //Execution loop
        float executionDuration = 0.0f;
        float timeSinceLastYelp = 0.0f;
        while(holder && holder.StabbingWithIntentToExecute(executionDuration) && victimCorpse)
        {
            if(!PlanetPauseMenu.pauseMenu.IsPaused())
            {
                //Make sure there's always noise
                if (timeSinceLastYelp > 1.0f)
                {
                    timeSinceLastYelp = 0;

                    screamer.Stop();
                    screamer.clip = victimPill.voice.GetDramaticDeath();
                    screamer.Play();
                }
                else
                    timeSinceLastYelp += Time.deltaTime;

                //Slide corpse down blade the higher it is hoisted in the air
                float slideVector = (holder.transform.position.y - victimCorpse.transform.position.y) * Time.deltaTime * 2;
                corpsePosition.z = Mathf.Clamp(corpsePosition.z + slideVector, 0.5f, 1.0f);

                //Vibrate corpse on blade for extra drama
                corpsePosition.y = Mathf.Clamp(originalCorpsePosition.y + Random.Range(-5.0f, 5.0f) * Time.deltaTime, originalCorpsePosition.y - 0.15f, originalCorpsePosition.y + 0.15f);
                corpsePosition.x = Mathf.Clamp(originalCorpsePosition.x + Random.Range(-5.0f, 5.0f) * Time.deltaTime, originalCorpsePosition.x - 0.15f, originalCorpsePosition.x + 0.15f);

                //Apply translations
                victimCorpse.transform.localPosition = corpsePosition;
            }

            //Wait a frame
            yield return null;
            executionDuration += Time.deltaTime;
        }

        //Restore corpse to what it was like before
        victimCorpse.transform.parent = null;
        Rigidbody rBody = victimCorpse.gameObject.AddComponent<Rigidbody>();
        victimCorpse.beingExecuted = false;

        //Fling corpse away
        if(executionDuration > 0.5f)
        {
            //Throw corpse
            rBody.AddExplosionForce(GetExecutionReleaseForce(executionDuration), transform.position, 10.0f);

            //Corpse wailing
            screamer.Stop();
            screamer.clip = victimPill.voice.GetDramaticDeath();
            screamer.Play();

            //Make it look like we twisted the machete to fling the corpse off
            if (holder)
            {
                //Twist sword side ways
                Vector3 itemRotation = transform.localEulerAngles;
                float beforeRotation = itemRotation.z;
                itemRotation.z = 120;
                transform.localEulerAngles = itemRotation;

                //Sound of final sword twist
                holder.GetAudioSource().PlayOneShot(finishingMove);

                //Wait a moment for visual to be noticable
                yield return new WaitForSeconds(0.5f);

                if (holder)
                {
                    //Return sword to original position
                    itemRotation.z = beforeRotation;
                    transform.localEulerAngles = itemRotation;
                }
            }
        }

        //Remove execution status
        executing = false;
    }

    private float GetExecutionReleaseForce(float executionDuration)
    {
        return Mathf.Max(1.0f, executionDuration) * Random.Range(0.75f, 1.0f) * meleeKnockback;
    }

    public override Vector3 GetPlaceInItemRack() { return new Vector3(0.025f, 0, 0); }
    public override Vector3 GetRotationInItemRack() { return new Vector3(-103, -90, 180); }
}
