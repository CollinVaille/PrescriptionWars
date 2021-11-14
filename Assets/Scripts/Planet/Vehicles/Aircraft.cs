using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : Vehicle
{
    //Customization
    public Transform physicalSteeringWheel;

    //References
    private CustomKinematicBody customBody;

    protected override void Start()
    {
        customBody = GetComponent<CustomKinematicBody>();

        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        //Call this even when off so that we can properly deactivate engine effects
        UpdateEngineEffects();

        if (!on)
            return;

        UpdateTranslation();
        UpdateRotation();
    }

    public override void SetPower(bool turnOn)
    {
        base.SetPower(turnOn);

        foreach (Engine engine in engines)
            engine.SetPower(turnOn);
    }

    private void UpdateTranslation()
    {
        //Move position based on speed
        if (gasPedal > 0.01f) //Forward (thrust)
        {
            if(MovingBackward() || rBody.velocity.magnitude < currentMaxSpeed)
                customBody.AddForce(Vector3.forward * thrustPower * Time.fixedDeltaTime, Space.Self);
        }
        else if (gasPedal < -0.01f) //Backward (brakes)
        {
            if (!MovingBackward() || rBody.velocity.magnitude < currentMaxSpeed)
                customBody.AddForce(Vector3.back * brakePower * Time.fixedDeltaTime, Space.Self);
        }
        else if(!MovingBackward() && rBody.velocity.magnitude < currentMaxSpeed) //Cruise control
            customBody.AddForce(Vector3.forward * rBody.mass * Time.fixedDeltaTime * customBody.airResistance, Space.Self);
    }

    private void UpdateRotation()
    {
        physicalSteeringWheel.localEulerAngles = Vector3.up * 90 * steeringWheel;
        customBody.AddRotation(Vector3.up * turnStrength * steeringWheel * Time.fixedDeltaTime, Space.Self);
    }

    protected override void RefreshGear(bool updateIndicator, bool onStart)
    {
        base.RefreshGear(updateIndicator, onStart);

        if (currentMaxSpeed == 0)
            customBody.airResistance = (brakePower / rBody.mass) * 2.0f;
        else
            customBody.airResistance = 4.0f;
    }
}
