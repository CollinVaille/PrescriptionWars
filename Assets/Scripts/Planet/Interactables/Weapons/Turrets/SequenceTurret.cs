using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceTurret : Turret
{
    //Customization
    public AudioClip firingSoundEffect;
    public string projectileName;
    public float damage = 20.0f, range = 300.0f, recoilDistance = 0.0f;
    [Tooltip("In seconds.")] public float recoilDuration = 0.1f, recoveryDuration = 0.4f;
    public FireBoreAt[] firingSequence;

    //Status and reference variables
    private bool firing = false;
    private int triggerReleaseCount = 0;

    public override void OnTriggerPressed()
    {
        base.OnTriggerPressed();

        if(!firing)
        {
            if (rounds > 0)
                StartCoroutine(FireSequence());
            else if(occupant)
                occupant.GetAudioSource().PlayOneShot(dryFire);
        }
    }

    public override void OnTriggerReleased()
    {
        base.OnTriggerReleased();

        triggerReleaseCount++;
    }

    private IEnumerator FireSequence()
    {
        if (!occupant)
            yield break;

        firing = true;
        int triggerReleaseStamp = triggerReleaseCount;

        while(triggerReleaseStamp == triggerReleaseCount)
        {
            if (rounds > 0)
            {
                if(occupant)
                    occupant.GetAudioSource().PlayOneShot(firingSoundEffect);

                float previousFireTime = 0.0f;
                for (int x = 0; x < firingSequence.Length; x++)
                {
                    //Wait
                    yield return new WaitForSeconds(firingSequence[x].timeInSequence - previousFireTime);

                    //Fire!
                    StartCoroutine(FireBore(firingSequence[x].bore));

                    //Bit of timekeeping
                    previousFireTime = firingSequence[x].timeInSequence;
                }

                //Wait for last bore to fully recover before we finish this sequence and allow another to begin
                yield return new WaitForSeconds(recoilDuration + recoveryDuration);
            }
            else
            {
                if (occupant)
                    occupant.GetAudioSource().PlayOneShot(dryFire);

                yield return new WaitForSeconds(firingSequence[firingSequence.Length - 1].timeInSequence + recoilDuration + recoveryDuration);
            }   
        }

        firing = false;
    }

    private IEnumerator FireBore(Transform bore)
    {
        //Unleash the demon
        FireProjectile(bore.position);
        rounds--;

        //Recoil phase
        float restingZ = bore.localPosition.z;
        float recoilZ = restingZ - recoilDistance;
        Vector3 borePos;
        for (float t = 0.0f; t < recoilDuration; t += Time.deltaTime)
        {
            //Update position
            borePos = bore.localPosition;
            borePos.z = Mathf.Lerp(restingZ, recoilZ, t / recoilDuration);
            bore.localPosition = borePos;

            //Wait a frame
            yield return null;
        }

        //Recovery phase
        for (float t = 0.0f; t < recoveryDuration; t += Time.deltaTime)
        {
            //Update position
            borePos = bore.localPosition;
            borePos.z = Mathf.Lerp(recoilZ, restingZ, t / recoveryDuration);
            bore.localPosition = borePos;

            //Wait a frame
            yield return null;
        }

        //Put back in final position
        borePos = bore.localPosition;
        borePos.z = restingZ;
        bore.localPosition = borePos;
    }

    private void FireProjectile(Vector3 position)
    {
        Projectile projectile = Projectile.projectilePool.GetGameObject(projectileName).GetComponent<Projectile>();

        //Put projectile in launch position
        projectile.transform.rotation = swivelingBody.rotation;
        projectile.transform.position = position;
        projectile.transform.Translate(Vector3.forward * 1.0f, Space.Self); //Arbitrary amount forward in front of the barrel

        //Ignore collisions between turret and projectile so we don't blow ourselves up
        Collider turretCollider = GetComponent<Collider>();
        Collider projectileCollider = projectile.GetComponent<Collider>();
        if (turretCollider && projectileCollider)
            Physics.IgnoreCollision(turretCollider, projectileCollider);

        //Launch time!
        projectile.Launch(damage, 100.0f, range, occupant);
    }
}

[System.Serializable]
public class FireBoreAt
{
    [Tooltip("Bore that is fired.")] public Transform bore;
    [Tooltip("Bore is fired at this time in seconds in the sequence.")] public float timeInSequence;
}
