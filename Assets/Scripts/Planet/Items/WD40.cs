using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WD40 : RepairTool
{
    public AudioClip flickNozzleUp, flickNozzleDown;

    public override void PutInHand(Pill newHolder)
    {
        base.PutInHand(newHolder);

        //Flip nozzle up
        transform.Find("Nozzle").localEulerAngles = new Vector3(-90, 0, 0);
        transform.Find("Nozzle").Find("Tip").gameObject.layer = 9; //Make nozzle blunt weapon
        newHolder.GetAudioSource().PlayOneShot(flickNozzleUp);

    }

    public override void RetireFromHand()
    {
        //Flick nozzle down
        transform.Find("Nozzle").localEulerAngles = Vector3.zero;
        transform.Find("Nozzle").Find("Tip").gameObject.layer = 0; //Make nozzle harmless
        holder.GetAudioSource().PlayOneShot(flickNozzleDown);

        base.RetireFromHand();
    }

    public override Vector3 GetPlaceInPlayerHand() { return new Vector3(0.5f, -0.25f, 0.4f); }

    public override Vector3 GetRotationInItemRack() { return new Vector3(0, Random.Range(-180, 180), 0); }
}
