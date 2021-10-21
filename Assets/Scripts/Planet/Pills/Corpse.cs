using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    private Pill soulOfCorpse;
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

        //Push down into ground
        transform.Translate(Vector3.down * 0.6f, Space.World);

        //Finalize
        DavyJonesLocker.CheckOut(soulOfCorpse, this);
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
        AudioSource sfxSource = GetComponent<AudioSource>();
        if (sfxSource)
        {
            sfxSource.Stop();
            God.god.UnmanageAudioSource(sfxSource);
        }
        Destroy(this);
    }

    private bool IsGrounded ()
    {
        return Physics.Raycast(transform.position, Vector3.down, 5);
    }
}
