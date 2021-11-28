using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : Interactable
{
    private enum FiringState { Ready, Firing, Resetting }

    public int shells;
    public float fireDelay, recoilDistance, recoilDuration, recoveryDuration, projectileStartingZ;
    public float damage, knockback, range;
    public string projectileName;

    private FiringState currentState = FiringState.Ready;
    private AudioSource cannonSFX;

    private void Start()
    {
        cannonSFX = GetComponent<AudioSource>();
        God.god.ManageAudioSource(cannonSFX);
    }

    public override void Interact(Pill interacting)
    {
        if (currentState != FiringState.Ready || shells <= 0)
            return;

        base.Interact(interacting);

        currentState = FiringState.Firing;
        StartCoroutine(FireCannon(interacting));
    }

    private IEnumerator FireCannon(Pill interacting)
    {
        shells--;

        //Start the sound effect
        cannonSFX.Play();

        //Wait for part where it fires
        yield return new WaitForSeconds(fireDelay);

        //FIRE!
        FireProjectile(interacting);

        //Recoil (animate every frame for smooth effect)
        Transform movingBody = transform.Find("Moving Body");
        Vector3 originalPosition = movingBody.localPosition;
        Vector3 recoilPosition = originalPosition - Vector3.forward * recoilDistance;
        for(float t = 0.0f; t < recoilDuration; t += Time.deltaTime)
        {
            //Move part of cannon that recoils
            movingBody.localPosition = Vector3.Lerp(originalPosition, recoilPosition, t / recoilDuration);

            //Wait a frame
            yield return null;
        }

        //Begin resetting
        currentState = FiringState.Resetting;

        //Recover from recoil (animate per increment of time for cheaper effect since animation is longer and slower)
        for (float t = 0.0f, stepDuration = 0.033f; t < recoveryDuration; t += stepDuration)
        {
            //Move part of cannon that recoils
            movingBody.localPosition = Vector3.Lerp(recoilPosition, originalPosition, t / recoveryDuration);

            //Wait a frame
            yield return new WaitForSeconds(stepDuration);
        }

        //Complete reset
        currentState = FiringState.Ready;
    }

    private void FireProjectile(Pill interacting)
    {
        Projectile projectile = Projectile.GetProjectile(projectileName);

        //Put projectile in launch position
        projectile.transform.rotation = transform.rotation;
        projectile.transform.position = transform.position;
        projectile.transform.Translate(Vector3.forward * projectileStartingZ, Space.Self);

        //Ignore collisions between cannon and projectile so we don't blow ourselves up
        Collider cannonCollider = GetComponent<Collider>();
        Collider projectileCollider = projectile.GetComponent<Collider>();
        if(cannonCollider && projectileCollider)
            Physics.IgnoreCollision(cannonCollider, projectileCollider);

        //Launch time!
        projectile.Launch(damage, knockback, range, interacting);
    }

    public override bool OverrideTriggerDescription() { return true; }

    public override string GetInteractionDescription()
    {
        if(currentState == FiringState.Firing)
            return "Fire " + name + " (Firing)";
        else if (currentState == FiringState.Resetting)
            return "Fire " + name + " (Cooling Down)";
        else if (shells == 0)
            return "Fire " + name + " (Empty)";
        else if (shells == 1)
            return "Fire " + name + " (" + shells + " Shell)";
        else
            return "Fire " + name + " (" + shells + " Shells)";
    }
}
