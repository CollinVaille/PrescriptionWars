using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machete : Item
{
    public AudioClip boomerang, doink;
    private bool inFlight = false;
    private Collider collidedWith = null;

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
        Damageable damageable = God.GetDamageable(collidedWith.transform);
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

    public override Vector3 GetPlaceInItemRack() { return new Vector3(0.025f, 0, 0); }
    public override Vector3 GetRotationInItemRack() { return new Vector3(-103, -90, 180); }
}
