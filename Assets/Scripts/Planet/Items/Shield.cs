using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Item
{
    private bool shielding = false;

    public AudioClip plant;

    //Raise shield
    public override void SecondaryAction ()
    {
        if (!holder || shielding)
            return;

        holder.RaiseShield();
    }

    //Plant shield in ground
    public override void TertiaryAction ()
    {
        if (!holder || holder.performingAction || !holder.IsGrounded(0.1f))
            return;

        //Use holder to calculate where to stick shield
        float newHeight = holder.transform.position.y;

        //Drop shield
        holder.GetAudioSource().PlayOneShot(plant);
        holder.Equip(null);

        //Then, stick it in ground...

        //Won't fall over or be pushed easily
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GetComponent<Rigidbody>().mass = 3;

        //Rotation
        Vector3 newRotation = transform.eulerAngles;
        newRotation.x = 0;
        transform.eulerAngles = newRotation;

        //Positioning
        Vector3 newPosition = transform.position;
        newPosition.y = newHeight;
        transform.position = newPosition;
    }

    public override void RetireFromHand ()
    {
        base.RetireFromHand();

        shielding = false;
    }

    public bool IsShielding () { return shielding; }

    public void SetShielding (bool shielding) { this.shielding = shielding; }

    public void ImpactRecoil () { StartCoroutine(ImpactRecoilImplement()); }

    private IEnumerator ImpactRecoilImplement ()
    {
        if (!shielding || !holder || holder.performingAction)
            yield break;

        float duration = 0.2f;
        float recoil = 0.05f / duration;

        //Move backward
        for(float t = 0; t < duration && shielding && holder && !holder.performingAction; t += Time.deltaTime)
        {
            transform.Translate(Vector3.back * recoil * Time.deltaTime, Space.Self);

            yield return null;
        }

        //Move forward
        for (float t = 0; t < duration && shielding && holder && !holder.performingAction; t += Time.deltaTime)
        {
            transform.Translate(Vector3.forward * recoil * Time.deltaTime, Space.Self);

            yield return null;
        }
    }

    public override Vector3 GetPlaceOnBack () { return new Vector3(0.0f, 0.1f, -1.0f); }
    public override Vector3 GetRotationOnBack () { return Vector3.zero; }

    public override Vector3 GetPlaceInItemRack () { return new Vector3(0.0f, 0.0f, 0.5f); }
    public override Vector3 GetRotationInItemRack () { return new Vector3(180, 0, 0); }
}
