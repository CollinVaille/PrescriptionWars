using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Turret : Interactable
{
    //Customization
    public int rounds = 200;
    [Tooltip("Part of the turret that swivels when you aim it")] public Transform swivelingBody;
    [Tooltip("Local position of occupant within swiveling body")] public Vector3 localOccupantPosition;
    public Transform[] turretTriggers;
    [Tooltip("Local z position of trigger when pressed")] public float triggerPressedPos;
    [Tooltip("Local z position of trigger when released")] public float triggerReleasedPos;
    public AudioSource sfxSource;

    //Status and reference variables
    protected float maxRounds = 0;
    protected Pill occupant = null;
    private CollisionDetectionMode occupantsPriorMode;
    private int occupantCode = 0;
    protected bool triggerPressed = false;

    public virtual void Start()
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

        //Update rigidbody
        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantsPriorMode = occupantRBody.collisionDetectionMode;
        occupantRBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        occupantRBody.isKinematic = true;

        //Update transform
        occupant.transform.parent = swivelingBody;
        occupant.transform.localPosition = localOccupantPosition;

        //Begin control override
        occupant.OverrideControl(this);
    }

    protected virtual void GetoffTurret()
    {
        if (!occupant)
            return;

        //Update transform
        occupant.transform.parent = null;

        //Update rigidbody
        Rigidbody occupantRBody = occupant.GetRigidbody();
        occupantRBody.isKinematic = false;
        occupantRBody.collisionDetectionMode = occupantsPriorMode;

        //Update status and release control
        occupant.ReleaseOverride();
        occupantCode++;
        occupant = null;
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

        ApplyRotationLimits();
        UpdateOccupantTransform();
    }

    private void ApplyRotationLimits()
    {
        //Put limits on rotation
        if (swivelingBody.localEulerAngles.x > 180)
        {
            if (swivelingBody.localEulerAngles.x < 300)
                swivelingBody.localEulerAngles = new Vector3(300, 0, 0);
        }
        else if (swivelingBody.localEulerAngles.x > 60)
            swivelingBody.localEulerAngles = new Vector3(60, 0, 0);
    }

    private void UpdateOccupantTransform()
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

    protected override string GetInteractionVerb() { return occupant == Player.player ? "Get Off" : "Man"; }
}
