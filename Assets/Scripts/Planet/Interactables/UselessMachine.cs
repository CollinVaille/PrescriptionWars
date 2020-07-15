using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UselessMachine : Interactable
{
    private bool latchSet = false, machineRunning = false;
    private float aggrevation = 0.0f;
    private AudioSource sfxSource;

    public Transform lid, latch, arm;

    public AudioClip setLatch, resetLatch, openLid, closeLid;

    public float openDuration = 2.5f, closeDuration = 2.0f;

    private void Start()
    {
        sfxSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(aggrevation > 0)
        {
            aggrevation -= Time.deltaTime;

            if (aggrevation < 0)
                aggrevation = 0;
        }
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        SetLatch(true);
    }

    private void SetLatch(bool set)
    {
        if (set == latchSet)
            return;

        latchSet = set;

        sfxSource.pitch = 1.0f;

        if (set)
        {
            aggrevation += Random.Range(10.0f, 15.0f + aggrevation);
            if (aggrevation > 250)
                aggrevation = 250;

            latch.localEulerAngles = new Vector3(-30, 0, 0);
            sfxSource.PlayOneShot(setLatch);

            if(!machineRunning)
                StartCoroutine(ActivateMachine());
        }
        else
        {
            latch.localEulerAngles = Vector3.zero;
            sfxSource.PlayOneShot(resetLatch);
        }
    }

    private IEnumerator ActivateMachine()
    {
        if (machineRunning)
            yield break;

        machineRunning = true;

        while(latchSet)
        {
            bool attackThisTime = DetermineResponse(
            out float openMult, out float resetDelay, out float closeDelay, out float closeMult);

            sfxSource.pitch = openMult;
            sfxSource.PlayOneShot(openLid);

            //Open lid, protrude arm
            float duration = openDuration * openMult; //In seconds
            for (float t = 0.0f; t < duration; t += Time.deltaTime)
            {
                //Rotate from 0 to -30 degrees
                lid.Rotate(Vector3.right, -30.0f * Time.deltaTime / duration, Space.Self);

                //Protrude arm from 0.15f to 0.3f
                arm.Translate(Vector3.up * 0.15f * Time.deltaTime / duration, Space.Self);

                //Wait a frame
                yield return null;
            }

            //Finalize transformation
            lid.localEulerAngles = new Vector3(-30, 0, 0);
            arm.localPosition = new Vector3(0.0f, 0.3f, -0.25f);

            if (resetDelay > 0)
                yield return new WaitForSeconds(resetDelay);

            //Do the deed
            if (attackThisTime)
                yield return StartCoroutine(PushLatchBack(0.5f * openMult, 0.5f * closeMult));

            if (closeDelay > 0)
                yield return new WaitForSeconds(closeDelay);

            //Latch set again before we finished...
            if (latchSet && aggrevation > Random.Range(10.0f, 15.0f))
            {
                //Rapid back and forth loop
                while(latchSet)
                {
                    //Fast push back
                    if (Random.Range(0, aggrevation) > 15)
                    {
                        yield return StartCoroutine(PushLatchBack(25.0f / aggrevation, 25.0f / aggrevation));

                        //Passive aggressive stare down after fast push back
                        if (Random.Range(0, 4) == 0)
                            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f) * aggrevation);
                    }
                    else
                    {
                        if (aggrevation > 10.0f) //Passive aggressive stare down
                            yield return new WaitForSeconds(Random.Range(0.5f, aggrevation / 10.0f));

                        break;
                    }
                }
            }

            sfxSource.pitch = closeMult;
            sfxSource.PlayOneShot(closeLid);

            //Close lid, retract arm
            duration = closeDuration * closeMult; //In seconds
            for (float t = 0.0f; t < duration; t += Time.deltaTime)
            {
                //Rotate from -30 to 0 degrees
                lid.Rotate(Vector3.right, 30.0f * Time.deltaTime / duration, Space.Self);

                //Retract arm from 0.3f back to 0.15f
                arm.Translate(Vector3.down * 0.15f * Time.deltaTime / duration, Space.Self);

                //Wait a frame
                yield return null;
            }

            //Finalize transformation
            lid.localEulerAngles = Vector3.zero;
            arm.localPosition = new Vector3(0.0f, 0.15f, -0.25f);
        }

        machineRunning = false;
    }

    private IEnumerator PushLatchBack(float pushDuration, float pullDuration)
    {
        //Send arm forward
        for (float t = 0.0f; t < pushDuration; t += Time.deltaTime)
        {
            //Protrude arm from -0.25f to -0.175f
            arm.Translate(Vector3.forward * 0.075f * Time.deltaTime / pushDuration, Space.Self);

            //Wait a frame
            yield return null;
        }

        //Finalize transformation
        arm.localPosition = new Vector3(0.0f, 0.3f, -0.175f);

        //The grand cultivation of this effort
        SetLatch(false);

        //Pull arm back
        for (float t = 0.0f; t < pullDuration; t += Time.deltaTime)
        {
            //Retract arm from -0.175f back to -0.25f
            arm.Translate(Vector3.back * 0.075f * Time.deltaTime / pullDuration, Space.Self);

            //Wait a frame
            yield return null;
        }

        //Finalize transformation
        arm.localPosition = new Vector3(0.0f, 0.3f, -0.25f);
    }

    //Returns whether to reset latch during response
    //Delays are in seconds, 1.0f is normal speed for multipliers, 2.0f is twice as fast, etc.
    private bool DetermineResponse(out float openMult, out float resetDelay, out float closeDelay, out float closeMult)
    {
        if(aggrevation < Random.Range(7.5f, 10.0f)) //Normal
        {
            openMult = 1.0f;
            resetDelay = 0.0f;
            closeDelay = 0.0f;
            closeMult = 1.0f;
            return true;
        }
        else if(aggrevation < Random.Range(15.0f, 20.0f)) //Impatient
        {
            openMult = aggrevation / 7.5f;
            resetDelay = 0.0f;
            closeDelay = 0.0f;
            closeMult = aggrevation / 7.5f;
            return true;
        }
        else //Passive aggressive
        {
            openMult = 10.0f / aggrevation;
            resetDelay = Random.Range(0.0f, 3.0f);
            closeDelay = Random.Range(0.0f, 3.0f);
            closeMult = 10.0f / aggrevation;
            return Random.Range(0, 2) == 0;
        }
    }
}
