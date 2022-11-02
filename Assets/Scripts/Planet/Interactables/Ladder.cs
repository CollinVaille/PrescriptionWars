using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Interactable
{
    private Pill occupant = null;
    private Vector3 occupantLocalPosition = Vector3.zero;
    private int occupantCode = 0;
    private CollisionDetectionMode occupantsPriorMode;

    private bool lastRungWas1 = false;
    private bool stillInStartArea = true;

    public float bottomHeight = -3.5f, topHeight = 3.5f;
    public AudioClip rung1, rung2;

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        if (interacting == occupant)
            JumpOff();
        else
            GetOn(interacting);
    }

    public override void ReleaseControl(bool voluntary)
    {
        base.ReleaseControl(voluntary);

        GetOff();
    }

    private void JumpOff()
    {
        if (!occupant)
            return;

        Pill climber = occupant;

        //Detach from ladder
        GetOff();

        //Push off from ladder
        Vector3 pushOffPoint = transform.position;
        pushOffPoint.y = climber.transform.position.y;
        climber.GetRigidbody().AddExplosionForce(10.0f, pushOffPoint, 10.0f);
    }

    private void GetOn(Pill climber)
    {
        if (occupant || !climber || !climber.CanOverride())
            return;

        occupant = climber;

        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantsPriorMode = occupantRBody.collisionDetectionMode;
        occupantRBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        occupantRBody.isKinematic = true;

        occupant.transform.parent = transform.parent;
        occupantLocalPosition.x = 0.0f;
        occupantLocalPosition.y = occupant.transform.localPosition.y;
        occupantLocalPosition.z = 1.0f;
        occupant.transform.localPosition = occupantLocalPosition;

        occupant.OverrideControl(this);
        StartCoroutine(PlayerClimb());

        PlayRungSound(1.0f);
    }

    private void GetOff()
    {
        if (!occupant)
            return;

        PlayRungSound(1.0f);

        occupant.transform.parent = null;

        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantRBody.isKinematic = false;
        occupantRBody.collisionDetectionMode = occupantsPriorMode;

        Rigidbody referenceRBody = transform.GetComponentInParent<Rigidbody>();
        if (referenceRBody)
            occupantRBody.velocity = referenceRBody.velocity;

        occupant.ReleaseOverride();
        occupantCode++;
        occupant = null;
    }

    private IEnumerator PlayerClimb()
    {
        stillInStartArea = true;

        int occupantKey = ++occupantCode;

        while(occupantKey == occupantCode)
        {
            //Up/down movement
            if(Input.GetAxis("Vertical") != 0)
            {
                //Play sound of grabbing next rung on ladder
                PlayRungSound(Mathf.Abs(Input.GetAxis("Vertical")));

                //Move occupant upward/downward
                occupant.transform.Translate(Vector3.up * Input.GetAxis("Vertical") * 0.25f, Space.World);
            }

            //Update position tracker
            occupantLocalPosition = occupant.transform.localPosition;

            //Occupant knocked off ladder
            if (Mathf.Abs(occupantLocalPosition.x) > 2 || Mathf.Abs(occupantLocalPosition.z - 1) > 2)
                GetOff();

            //Occupant reached bottom or top so get off? unless that's where they started and haven't moved yet
            if (occupantLocalPosition.y <= bottomHeight || occupantLocalPosition.y >= topHeight)
            {
                if (!stillInStartArea)
                    GetOff();
            }
            else if (stillInStartArea)
                stillInStartArea = false;

            //Wait a short period of time
            yield return new WaitForSeconds(Input.GetButton("Sprint")  ? 0.15f : 0.25f);
        }
    }

    private void PlayRungSound(float volume)
    {
        //Play sound of climbing to next rung
        if (lastRungWas1)
            occupant.GetAudioSource().PlayOneShot(rung2, volume);
        else
            occupant.GetAudioSource().PlayOneShot(rung1, volume);

        //Rung sounds alternate
        lastRungWas1 = !lastRungWas1;
    }

    protected override string GetInteractionVerb() { return occupant == Player.player ? "Let Go" : "Climb"; }
}
