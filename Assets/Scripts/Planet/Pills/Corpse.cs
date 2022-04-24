using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    private Pill soulOfCorpse;
    private Collider groundToStickTo;
    public bool beingExecuted = false;

    public void BootUpCorpse (Pill soulOfCorpse)
    {
        this.soulOfCorpse = soulOfCorpse;
        DavyJonesLocker.CheckIn(soulOfCorpse, this);

        StartCoroutine(FindNiceRestingPlace());
    }

    private IEnumerator FindNiceRestingPlace ()
    {
        //Initial roll time (stalled in the event of an execution)
        int gradePeriodTicks = Random.Range(10, 14);
        for(int x = 0; x < gradePeriodTicks; x++)
        {
            yield return new WaitForSeconds(0.5f);

            if(beingExecuted)
                yield return new WaitWhile(() => beingExecuted);
        }

        //Wait until we have a good resting place (executions can also stall this phase)
        while (!IsGrounded() || beingExecuted)
            yield return new WaitForSeconds(0.5f);

        //Now, stick pill in final resting place (no more rolling around)...

        //Get rid of moving parts
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());

        //Push down into ground
        PhysicallyPushIntoGround();

        //Finalize
        DavyJonesLocker.CheckOut(soulOfCorpse, this);
        AudioSource sfxSource = GetComponent<AudioSource>();
        if (sfxSource)
        {
            sfxSource.Stop();
            God.god.UnmanageAudioSource(sfxSource);
        }
        Destroy(this);
    }

    private void PhysicallyPushIntoGround()
    {
        transform.Translate(Vector3.down * 0.6f, Space.World);
        DetermineTransformParent();
    }

    private void DetermineTransformParent()
    {
        if (!groundToStickTo)
            return;

        Vehicle vehicleParent = groundToStickTo.GetComponentInParent<Vehicle>();

        if (vehicleParent)
            transform.parent = vehicleParent.transform;
        else
        {
            Rigidbody rBodyParent = groundToStickTo.GetComponentInParent<Rigidbody>();

            if (rBodyParent)
                transform.parent = rBodyParent.transform;
        }
    }

    private bool IsGrounded ()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 5))
        {
            groundToStickTo = hitInfo.collider;
            return true;
        }

        return false;
    }
}
