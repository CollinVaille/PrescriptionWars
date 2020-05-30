using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    //Customization
    public float meleeDamage = 50, meleeKnockback = 100;
    public AudioClip swoosh, stab, scrape, equip;

    //Status variables
    protected Pill holder = null;
    protected bool stabbing = false, holderIsPlayer = false;

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

    public IEnumerator ExpensiveStab ()
    {
        if (stabbing || !holder || holder.performingAction)
            yield break;

        holder.GetAudioSource().PlayOneShot(swoosh);

        stabbing = true;
        holder.performingAction = true;

        Vector3 itemPosition = transform.localPosition;
        float retractedZ = itemPosition.z;
        float protractedZ = retractedZ + 0.35f;

        //Protract
        float duration = 0.1f;
        for (float t = 0; t < duration && holder; t += Time.deltaTime)
        {
            //Slide item forward
            itemPosition = transform.localPosition;
            itemPosition.z = Mathf.Lerp(retractedZ, protractedZ, t / duration);
            transform.localPosition = itemPosition;

            //Wait a frame
            yield return null;
        }

        if (holder)
        {
            //Finalize protraction
            itemPosition = transform.localPosition;
            itemPosition.z = protractedZ;
            transform.localPosition = itemPosition;
        }

        //Retract
        duration = 0.4f;
        for (float t = 0; t < duration && holder; t += Time.deltaTime)
        {
            //Slide item backward
            itemPosition = transform.localPosition;
            itemPosition.z = Mathf.Lerp(protractedZ, retractedZ, t / duration);
            transform.localPosition = itemPosition;

            //Wait a frame
            yield return null;
        }

        if(holder)
        {
            //Finalize retraction
            itemPosition = transform.localPosition;
            itemPosition.z = retractedZ;
            transform.localPosition = itemPosition;

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
        StartCoroutine(ExpensiveStab());
    }

    public virtual void SecondaryAction () { /* There is no default secondary action */ }

    public virtual void TertiaryAction () { /* There is no default tertiary action */ }

    //Called when item is first put into hand (equipped from ground or grabbed sidearm)
    public virtual void PutInHand (Pill newHolder)
    {
        holder = newHolder;

        if (holder)
            holderIsPlayer = holder.GetComponent<Player>();

        if (holderIsPlayer)
            holder.GetComponent<Player>().SetContinuousPrimaryAction(false);
    }

    //Called when item is no longer being held (unequipped or moved to sidearm)
    public virtual void RetireFromHand ()
    {
        stabbing = false;
        holder.performingAction = false;

        //This tells the item info display to stop its fade out for the item we're retiring
        if (holderIsPlayer)
            holder.GetComponent<Player>().IncrementItemInfoFlashCode();

        holder = null;
    }

    public virtual string GetItemInfo ()
    {
        return name;
    }

    public bool BeingHeld () { return holder; }
}
