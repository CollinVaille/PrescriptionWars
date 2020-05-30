using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{
    //References
    private Transform clip;

    //Customization
    public AudioClip fire, dryFire, reload;
    public int loadedBullets, clipSize, spareClips;
    public int range, bulletDamage, bulletKnockback;
    public float cooldown = 0;

    //Status variables
    [HideInInspector]
    public bool aiming = false;
    private float lastFired = 0;

    private void Start ()
    {
        clip = transform.Find("Clip");
    }

    //Shoot
    public override void PrimaryAction ()
    {
        Shoot();
    }

    //Aim
    public override void SecondaryAction ()
    {
        if (!holder)
            return;

        holder.AimDownSights();
    }

    //Reload
    public override void TertiaryAction ()
    {
        StartCoroutine(Reload());
    }

    public void Shoot ()
    {
        if (!holder || Time.timeSinceLevelLoad - lastFired <= cooldown)
            return;

        lastFired = Time.timeSinceLevelLoad;

        if (loadedBullets <= 0) //Dry fire
            holder.GetAudioSource().PlayOneShot(dryFire);
        else //Fire!
        {
            //Lose bullet
            loadedBullets--;

            //Update bullet count display
            if (holderIsPlayer)
            {
                holder.GetComponent<Player>().FlashItemInfo();

                StartCoroutine(Recoil());
            }

            //Play sound effect
            holder.GetAudioSource().PlayOneShot(fire);

            //Cast ray in place of bullet to see if we hit something
            RaycastHit hit;
            if(holder.RaycastShoot(transform, range, out hit))
            {
                Pill hitPill = hit.collider.GetComponent<Pill>();

                if (hitPill && hitPill.team != holder.team)
                {
                    hitPill.ApplyHit(bulletDamage, bulletKnockback, transform.position);

                    hitPill.AlertOfAttacker(holder, true);
                    if (hitPill.squad != null)
                        hitPill.squad.AlertSquadOfAttacker(holder, hitPill, Random.Range(3, 6));
                    if (holder.squad != null)
                        holder.squad.AlertSquadOfAttacker(hitPill, holder, Random.Range(3, 6));
                }

                //Hit marker sounds for player
                if(holderIsPlayer)
                {
                    if (hitPill)
                        holder.GetAudioSource().PlayOneShot(holder.GetComponent<Player>().hitMarker);
                    else if(hit.collider.CompareTag("Gear"))
                        holder.GetAudioSource().PlayOneShot(holder.GetComponent<Player>().hitArmorMarker);
                }

                //Play ricochet sound effect if bullet hit nearby player
                Player.player.BulletRicochet(hit);
            }
        }
    }

    public IEnumerator Reload ()
    {
        //Can't reload without reloader
        if (!holder)
            yield break;

        //Can't reload if no more clips
        if (spareClips <= 0)
        {
            //Notify player they are screwed
            if (holderIsPlayer)
                holder.GetComponent<Player>().FlashItemInfo();

            yield break;
        }

        //Can't reload if busy doing something else
        if (holder.performingAction)
            yield break;

        holder.performingAction = true;

        //Play sound effect
        holder.GetAudioSource().PlayOneShot(reload);

        //Rotate weapon around so we can load it
        transform.localRotation = Quaternion.Euler(0, 0, -45);

        //Take out current clip
        loadedBullets = 0;
        clip.Translate(Vector3.down * 0.1f, Space.Self);

        //Update bullet count display for mid-reload to be 0
        if (holderIsPlayer)
            holder.GetComponent<Player>().FlashItemInfo();

        //Wait for reload to take place
        yield return new WaitForSeconds(reload.length * 0.8f);

        //Visually put clip back in (happens even if no holder)
        clip.Translate(Vector3.up * 0.1f, Space.Self);

        if (holder)
        {
            //Put new clip in
            spareClips--;
            loadedBullets = clipSize;

            //Update bullet count display after reload
            if (holderIsPlayer)
                holder.GetComponent<Player>().FlashItemInfo();

            //Reset rotation of weapon
            transform.localRotation = Quaternion.Euler(0, 0, 0);

            holder.performingAction = false;
        }
    }

    public override string GetItemInfo ()
    {
        if (loadedBullets <= 0)
        {
            if(spareClips == 1)
                return name + ": EMPTY   -   1 spare clip";
            else
                return name + ": EMPTY   -   " + spareClips + " spare clips";

        }
        else
        {
            if(spareClips == 1)
                return name + ": ".PadRight(loadedBullets + 2, 'I') + "   -   1 spare clip";
            else
                return name + ": ".PadRight(loadedBullets + 2, 'I') + "   -   " + spareClips + " spare clips";
        }
    }

    public IEnumerator Recoil ()
    {
        if (!holder || (holder.performingAction && !aiming))
            yield break;

        //Recoil backward
        float duration = 0.1f;
        for(float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            if (!holder || (holder.performingAction && !aiming))
                yield break;

            //Move back
            transform.Translate(Vector3.back * 0.1f * Time.deltaTime / duration, Space.Self);

            //Wait a frame
            yield return null;
        }

        //Bounce back (forwards)
        duration = 0.2f;
        for (float t = 0.0f; t < duration; t += Time.deltaTime)
        {
            if (!holder || (holder.performingAction && !aiming))
                yield break;

            //Move back
            transform.Translate(Vector3.forward * 0.1f * Time.deltaTime / duration, Space.Self);

            //Wait a frame
            yield return null;
        }
    }

    public override void PutInHand (Pill newHolder)
    {
        base.PutInHand(newHolder);

        if (holderIsPlayer)
            holder.GetComponent<Player>().SetContinuousPrimaryAction(cooldown > 0);
    }

    public override void RetireFromHand ()
    {
        aiming = false;

        base.RetireFromHand();
    }
}
