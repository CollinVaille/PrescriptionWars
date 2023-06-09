using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    //Customization
    public float meleeDamage = 50, meleeKnockback = 100;
    public AudioClip swoosh, stab, scrape, equip;

    //Status variables
    protected PlanetPill holder = null;
    protected bool stabbing = false, holderIsPlayer = false, executing = false;

    public IEnumerator CheapStab ()
    {
        if (stabbing || !holder || holder.performingAction)
            yield break;

        holder.GetAudioSource().PlayOneShot(swoosh);

        stabbing = true;
        holder.performingAction = true;

        //Protract
        transform.localPosition += Vector3.forward * 0.35f;

        //Wait
        yield return new WaitForSeconds(0.5f);

        if(holder)
        {
            //Retract
            transform.localPosition -= Vector3.forward * 0.35f;

            stabbing = false;
            holder.performingAction = false;
        }
    }

    public IEnumerator ExpensiveStab (float reach, Vector3 rotation, AudioClip swingSound)
    {
        if (stabbing || !holder || holder.performingAction)
            yield break;

        holder.GetAudioSource().PlayOneShot(swingSound);

        stabbing = true;
        holder.performingAction = true;

        Vector3 itemPosition = transform.localPosition;
        float retractedZ = itemPosition.z;
        float protractedZ = retractedZ + reach;

        Vector3 itemRotation = transform.localEulerAngles;

        //Protract
        float duration = 0.1f;
        for (float t = 0; t < duration && holder && !executing; t += Time.deltaTime)
        {
            //Slide item forward
            itemPosition = transform.localPosition;
            itemPosition.z = Mathf.Lerp(retractedZ, protractedZ, t / duration);
            transform.localPosition = itemPosition;

            //Rotate item
            transform.localEulerAngles = Vector3.Lerp(Vector3.zero, rotation, t / duration);

            //Wait a frame
            yield return null;
        }

        if (holder)
        {
            //Finalize protraction
            itemPosition = transform.localPosition;
            itemPosition.z = protractedZ;
            transform.localPosition = itemPosition;

            //Finalize rotation
            transform.localEulerAngles = rotation;

            //Keep item protracted while executing
            if (executing)
                yield return new WaitWhile(() => executing && holder);
        }

        //Retract
        duration = 0.4f;
        for (float t = 0; t < duration && holder; t += Time.deltaTime)
        {
            //Slide item backward
            itemPosition = transform.localPosition;
            itemPosition.z = Mathf.Lerp(protractedZ, retractedZ, t / duration);
            transform.localPosition = itemPosition;

            //Rotate item
            transform.localEulerAngles = Vector3.Lerp(rotation, Vector3.zero, t / duration);

            //Wait a frame
            yield return null;

            //Keep item stable while executing
            if (executing)
                yield return new WaitWhile(() => executing && holder);
        }

        //Keep item stable while executing
        if (executing)
            yield return new WaitWhile(() => executing && holder);

        if (holder)
        {
            //Finalize retraction
            itemPosition = transform.localPosition;
            itemPosition.z = retractedZ;
            transform.localPosition = itemPosition;

            //Finalize rotation
            transform.localEulerAngles = Vector3.zero;

            stabbing = false;
            holder.performingAction = false;
        }
    }

    public bool IsStabbing () { return stabbing; }

    public virtual void PrimaryAction ()
    {
        //Action requires holder
        if (!holder)
            return;

        //Default primary action is stab
        StartCoroutine(ExpensiveStab(0.35f, Vector3.zero, swoosh));
    }

    public virtual void SecondaryAction () { /* There is no default secondary action */ }

    public virtual void TertiaryAction () { /* There is no default tertiary action */ }

    //Called when item is first put into hand (equipped from ground or grabbed sidearm)
    public virtual void PutInHand (PlanetPill newHolder)
    {
        holder = newHolder;

        if (holder)
            holderIsPlayer = holder.IsPlayer;

        if (holderIsPlayer)
            holder.GetComponent<PlanetPlayerPill>().SetContinuousPrimaryAction(false);
    }

    //Called when item is no longer being held (unequipped or moved to sidearm)
    public virtual void RetireFromHand ()
    {
        stabbing = false;
        holder.performingAction = false;

        //This tells the item info display to stop its fade out for the item we're retiring
        if (holderIsPlayer)
            holder.GetComponent<PlanetPlayerPill>().IncrementItemInfoFlashCode();

        holder = null;
    }

    public virtual string GetItemInfo ()
    {
        return name;
    }

    public virtual void OnMeleeKill(PlanetPill pill) { }

    public bool BeingHeld () { return holder; }

    public virtual Vector3 GetPlaceInPlayerHand () { return new Vector3(0.5f, -0.25f, 0.0f); }

    public virtual Vector3 GetPlaceOnBack () { return new Vector3(0.0f, 0.5f, -0.5f); }
    public virtual Vector3 GetRotationOnBack () { return new Vector3(90, 90, 0); }

    public virtual Vector3 GetPlaceInItemRack () { return Vector3.zero; }
    public virtual Vector3 GetRotationInItemRack () { return new Vector3(-90, 0, 0); }

    public virtual Vector3 GetPlaceInHolster() { return new Vector3(0.5f, 0, 0); }
    public virtual Vector3 GetRotationInHolster() { return new Vector3(150, 0, 0); }
}
