using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : Interactable
{
    private Pill occupant;
    public Vehicle controls = null;
    private Vehicle belongsTo = null;

    public float radius;
    private bool ejecting = false;
    private CollisionDetectionMode occupantsPriorMode;

    public AudioClip sit, getUp;

    private void Start()
    {
        //Determine which vehicle seat belongs to (if any)
        Transform t = transform.parent;
        while(t && !belongsTo)
        {
            belongsTo = t.GetComponent<Vehicle>();

            t = t.parent;
        }
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        if (occupant && occupant == interacting)
            ejecting = true;
        else
            Sit(interacting);
    }

    private void Sit(Pill pill)
    {
        if (occupant || !pill || !pill.CanOverride())
            return;

        occupant = pill;

        if(belongsTo)
        {
            Rigidbody occupantRBody = occupant.GetRigidbody();
            occupantsPriorMode = occupantRBody.collisionDetectionMode;
            occupantRBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            occupantRBody.isKinematic = true;

            belongsTo.SetPassengerCollisionRecursive(occupant.transform, true);
        }

        occupant.transform.parent = transform;
        occupant.transform.localPosition = Vector3.up;
        occupant.transform.localEulerAngles = Vector3.zero;

        occupant.GetAudioSource().PlayOneShot(sit);
        occupant.OverrideControl(this);

        if (controls)
            controls.SetPower(true);
    }

    public override void ReleaseControl(bool voluntary)
    {
        base.ReleaseControl(voluntary);

        GetUp(voluntary);
    }

    private void GetUp(bool voluntary)
    {
        if (!occupant)
            return;

        if (controls)
            controls.SetPower(false);

        if (voluntary)
            occupant.GetAudioSource().PlayOneShot(getUp);

        occupant.transform.parent = null;
        occupant.transform.eulerAngles = Vector3.up * transform.eulerAngles.y;

        if (belongsTo)
        {
            Rigidbody occupantRBody = occupant.GetRigidbody();
            occupantRBody.isKinematic = false;
            occupantRBody.collisionDetectionMode = occupantsPriorMode;

            belongsTo.SetPassengerCollisionRecursive(occupant.transform, false);
        }

        occupant.ReleaseOverride();
        occupant = null;

        ejecting = false;
    }

    public void UpdateSeatBelt()
    {
        if(Input.GetKey(KeyCode.Y))
        occupant.GetRigidbody().AddForce(
            (transform.position + Vector3.up - occupant.transform.position) * Time.fixedDeltaTime * 5000);
    }

    public bool EjectingOccupant()
    {
        return ejecting || Vector3.Distance(occupant.transform.position, transform.position) > radius;
    }
}
