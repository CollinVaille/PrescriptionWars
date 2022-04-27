using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAltitudeController : MonoBehaviour
{
    public enum VerticalMovementType { Stable, GoingUp, GoingDown, Offline }

    //Customization
    public float maxFreeFallSpeed = 50.0f, maxControlledSpeed = 20.0f;

    //References
    private Aircraft aircraft;
    private CustomKinematicBody customPhysicsBody;

    //Status variables
    private VerticalMovementType currentState = VerticalMovementType.Offline;

    private void Awake()
    {
        aircraft = GetComponentInParent<Aircraft>();
        aircraft.SetAltitudeController(this);

        customPhysicsBody = aircraft.GetComponent<CustomKinematicBody>();
    }

    public void RegisterButtonPress(bool fromUpwardsButton)
    {
        if (currentState == VerticalMovementType.Offline)
            return;

        if (currentState == VerticalMovementType.GoingUp || currentState == VerticalMovementType.GoingDown)
            SetState(VerticalMovementType.Stable);
        else if (fromUpwardsButton)
            SetState(VerticalMovementType.GoingUp);
        else //Going down now
            SetState(VerticalMovementType.GoingDown);
    }

    private void SetState(VerticalMovementType newState)
    {
        if (currentState == newState)
            return;

        //Change max speed?
        if (currentState == VerticalMovementType.Offline) //Old state was offline so we're changing to tighter altitude control now
            SetMaxSpeed(false);
        else if(newState == VerticalMovementType.Offline) //New state is offline so we're changing to free fall / no altitude control
            SetMaxSpeed(true);

        currentState = newState;
        UpdateButtons();
    }

    private void SetMaxSpeed(bool freeFallSpeed) { customPhysicsBody.SetVerticalMaxSpeed(freeFallSpeed ? maxFreeFallSpeed : maxControlledSpeed); }

    private void UpdateButtons()
    {
        transform.Find("Go Up").GetComponent<PlanetDirectionalButton>().UpdateButtonFromController(currentState);
        transform.Find("Go Down").GetComponent<PlanetDirectionalButton>().UpdateButtonFromController(currentState);
    }

    public VerticalMovementType GetCurrentState() { return currentState; }

    public bool IsOffline() { return currentState == VerticalMovementType.Offline; }

    public void SetOffline(bool offline)
    {
        if (offline)
            SetState(VerticalMovementType.Offline);
        else
            SetState(VerticalMovementType.Stable);
    }

    public void UpdateVerticalTranslation()
    {
        if (currentState == VerticalMovementType.Offline) //Free fall from gravity
            customPhysicsBody.AddForce(Vector3.down * 200000.0f * Time.fixedDeltaTime, Space.World);
        else if (currentState == VerticalMovementType.GoingUp)
            customPhysicsBody.AddForce(Vector3.up * 200000.0f * Time.fixedDeltaTime, Space.World);
        else if(currentState == VerticalMovementType.GoingDown)
            customPhysicsBody.AddForce(Vector3.down * 250000.0f * Time.fixedDeltaTime, Space.World);
        else //Altitude stabilization mode
        {
            //if(customPhysicsBody.)
        }
    }
}
