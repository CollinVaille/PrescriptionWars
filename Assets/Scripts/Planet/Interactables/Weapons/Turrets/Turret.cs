using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Turret : Interactable
{
    //Customization
    public int rounds = 200;
    public AudioClip dryFire;
    [Tooltip("Part of the turret that swivels when you aim it.")] public Transform swivelingBody;
    [Tooltip("Local position of occupant within swiveling body.")] public Vector3 localOccupantPosition;
    public Transform[] turretTriggers;
    [Tooltip("Local z position of trigger when pressed.")] public float triggerPressedPos;
    [Tooltip("Local z position of trigger when released.")] public float triggerReleasedPos;
    public AudioSource sfxSource;
    [Tooltip("Occupant does not rotate with turret. Makes aiming more difficult and limits vertical reach.")] public bool detachedRotation = false;
    [Tooltip("How far above/below 0.0f can the turret rotate on its x-axis in degrees.")] public float rotationLimit = 60.0f;

    //Status and reference variables
    protected float maxRounds = 0;
    protected Pill occupant = null;
    private CollisionDetectionMode occupantsPriorMode;
    private int occupantCode = 0;
    protected bool triggerPressed = false;

    protected virtual void Start()
    {
        if (sfxSource)
            God.god.ManageAudioSource(sfxSource);

        maxRounds = rounds;
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        if (interacting == occupant)
            GetoffTurret();
        else
            GetInTurret(interacting);
    }

    protected virtual void GetInTurret(Pill interacting)
    {
        if (occupant || !interacting || !interacting.CanOverride())
            return;

        //Update status
        occupant = interacting;

        //Update physics layer (make it so it can't be interacted with)
        gameObject.layer = 0;

        //Update rigidbody
        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantsPriorMode = occupantRBody.collisionDetectionMode;
        occupantRBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        occupantRBody.isKinematic = true;

        //Update transform
        occupant.transform.parent = swivelingBody;
        occupant.transform.localPosition = localOccupantPosition;

        //Update rotation
        occupant.transform.localEulerAngles = Vector3.zero;
        if (occupant.IsPlayer)
            occupant.GetComponent<Player>().ResetHeadRotation();

        //Visual clean up
        occupant.Holster(true);
        UpdateRoundsOnUI();

        //Begin control override
        occupant.OverrideControl(this);
    }

    protected virtual void GetoffTurret()
    {
        if (!occupant)
            return;

        //Update transform
        occupant.transform.parent = null;

        //Make sure player doesn't leave with broken back rotation
        Vector3 occupantRot = occupant.transform.eulerAngles;
        occupantRot.x = 0.0f;
        occupantRot.z = 0.0f;
        occupant.transform.eulerAngles = occupantRot;

        //Update rigidbody
        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantRBody.isKinematic = false;
        occupantRBody.collisionDetectionMode = occupantsPriorMode;

        //Visual clean up
        DurabilityTextManager.ClearDurabilityText();
        occupant.Holster(false);

        //Update status and release control
        occupant.ReleaseOverride();
        occupantCode++;
        occupant = null;

        //Update physics layer (make it so it can be interacted with again)
        gameObject.layer = 10;
    }

    public override void ReleaseControl(bool voluntary)
    {
        base.ReleaseControl(voluntary);

        GetoffTurret();
    }

    public void SetTriggerPressed(bool pressed)
    {
        if (triggerPressed == pressed)
            return;

        //Change state
        triggerPressed = pressed;

        //Update all trigger visuals
        foreach(Transform turretTrigger in turretTriggers)
        {
            Vector3 triggerPosition = turretTrigger.localPosition;
            triggerPosition.z = pressed ? triggerPressedPos : triggerReleasedPos;
            turretTrigger.localPosition = triggerPosition;
        }

        //Trigger behaviour
        if (pressed)
            OnTriggerPressed();
        else
            OnTriggerReleased();
    }

    public virtual void OnTriggerPressed()
    {

    }

    public virtual void OnTriggerReleased()
    {

    }

    public void RotateTurret(Vector3 axis, float angle)
    {
        swivelingBody.Rotate(axis, angle, Space.Self);

        ApplyXAxisRotationLimits();
        ApplyZAxisRotationLimits();
        UpdateOccupantTransform();
    }

    private void ApplyXAxisRotationLimits()
    {
        //Put limits on x rotation
        if (swivelingBody.localEulerAngles.x > 180)
        {
            if (swivelingBody.localEulerAngles.x < 360 - rotationLimit)
                swivelingBody.localEulerAngles = new Vector3(360 - rotationLimit, swivelingBody.localEulerAngles.y, 0);
        }
        else if (swivelingBody.localEulerAngles.x > rotationLimit)
            swivelingBody.localEulerAngles = new Vector3(rotationLimit, swivelingBody.localEulerAngles.y, 0);
    }

    private void ApplyZAxisRotationLimits()
    {
        //Global z-axis must always have no rotation, otherwise you get really funky behaviour
        Vector3 globalRot = swivelingBody.localEulerAngles;
        globalRot.z = 0.0f;
        swivelingBody.localEulerAngles = globalRot;
    }

    private void UpdateOccupantTransform()
    {
        if (detachedRotation)
        {
            //Reset LOCAL x pos of occupant
            Vector3 occupantPosition = occupant.transform.localPosition;
            occupantPosition.x = localOccupantPosition.x;
            occupant.transform.localPosition = occupantPosition;

            //Adjust GLOBAL y pos of occupant to be slightly above turret
            occupantPosition = occupant.transform.position;
            occupantPosition.y = transform.position.y + 1;
            occupant.transform.position = occupantPosition;

            //Then, snap occupant to ground
            occupant.SnapToGround();

            //Make occupant look down sight of turret
            occupant.transform.LookAt(transform);

            //Reset GLOBAL x and z axis rotation (only y axis is allowed to change)
            Vector3 occupantRotation = occupant.transform.eulerAngles;
            occupantRotation.x = 0.0f;
            occupantRotation.z = 0.0f;
            occupant.transform.eulerAngles = occupantRotation;
        }
        else
            occupant.transform.localEulerAngles = Vector3.zero;
    }

    protected virtual void UpdateRoundsOnUI() { DurabilityTextManager.SetDurabilityText(rounds); }

    protected override string GetInteractionVerb() { return occupant && occupant.IsPlayer ? "Get Off" : "Man"; }
}
