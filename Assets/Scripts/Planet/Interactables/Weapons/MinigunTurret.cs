using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigunTurret : Turret
{
    //Customization
    public Transform barrel, magazine, emissionPoint;
    [Tooltip("Local x positions of the magazine. 1st # is start position, 2nd # is end position")] public Vector2 magazinePositions;
    [Tooltip("Time in seconds between shots")] public float shotDelay = 0.1f;
    [Tooltip("Time from emission to destruction of death ray")] public float shotLifetime = 0.5f;
    public float damage = 20.0f, range = 300.0f;
    [Tooltip("Degrees per second the barrel rotates when firing")] public float rotaryVelocity = 90.0f;
    public AudioClip spinUp, cooldown, firing, dryFire;
    public Light lightFlash;

    //Status variables
    private int firingCode = 0;

    public override void OnTriggerPressed()
    {
        base.OnTriggerPressed();

        StartCoroutine(FireMinigun());
    }

    private IEnumerator FireMinigun()
    {
        int firingKey = ++firingCode;

        //Set up
        if(lightFlash)
            lightFlash.enabled = false;

        //Spin up
        PlaySound(spinUp, false);
        for (float t = 0.0f, duration = spinUp.length; t < duration && firingKey == firingCode && triggerPressed; t += Time.deltaTime)
        {
            float currentRotarySpeed = rotaryVelocity * Time.deltaTime * t / duration;
            barrel.Rotate(Vector3.forward * currentRotarySpeed, Space.Self);
            yield return null;
        }

        if(firingKey == firingCode)
        {
            if(triggerPressed)
            {
                //Firing loop
                PlaySound(firing, true);
                float flashTime = 0.05f;
                for (float t = 0.0f; firingKey == firingCode && triggerPressed; t += Time.deltaTime)
                {
                    //Spin barrel
                    float currentRotarySpeed = rotaryVelocity * Time.deltaTime;
                    barrel.Rotate(Vector3.forward * currentRotarySpeed, Space.Self);

                    //Fire shot?
                    if (t >= shotDelay)
                    {
                        t = 0.0f;
                        FireRound();
                    }
                    else if (lightFlash && lightFlash.enabled && t >= flashTime) //Turn off flash if its been long enough since shot
                        lightFlash.enabled = false;

                    //Wait a frame
                    yield return null;
                }
            }

            //Cooldown
            if(firingKey == firingCode)
            {
                PlaySound(cooldown, false);
                for (float t = 0.0f, duration = spinUp.length; t < duration && firingKey == firingCode; t += Time.deltaTime)
                {
                    float currentRotarySpeed = rotaryVelocity * Time.deltaTime * (1.0f - (t / duration));
                    barrel.Rotate(Vector3.forward * currentRotarySpeed, Space.Self);
                    yield return null;
                }

                //Clean up
                if (firingKey == firingCode && lightFlash)
                    lightFlash.enabled = false;
            }
        }
    }

    private void FireRound()
    {
        if (rounds > 0)
        {
            SetRounds(rounds - 1);

            if (lightFlash)
                lightFlash.enabled = true;

            EmitDeathRay();
        }
        else
            PlaySound(dryFire, false);
    }

    private void EmitDeathRay()
    {
        DeathRay deathRay = DeathRay.GetDeathRay();
        deathRay.Emit(emissionPoint.position, swivelingBody.eulerAngles, damage, range, occupant, shotLifetime);
    }

    private void SetRounds(int newRounds)
    {
        //Update state
        rounds = newRounds;

        //Update magazine visual
        Vector3 magazinePosition = magazine.localPosition;
        magazinePosition.x = Mathf.Lerp(magazinePositions.y, magazinePositions.x, rounds / maxRounds);
        magazine.localPosition = magazinePosition;
    }

    private void PlaySound(AudioClip sound, bool loopSound)
    {
        sfxSource.Stop();
        sfxSource.clip = sound;
        sfxSource.loop = loopSound;
        sfxSource.Play();
    }
}
