using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : Vehicle
{
    //Customization
    public Transform physicalSteeringWheel;
    [SerializeField] private float verticalThrustPower = 200.0f, LODScaleMultiplier = 1.0f;

    //References
    private CustomKinematicBody customBody;
    private VehicleAltitudeController altitudeController;
    private List<Engine> verticalEngines;

    //Status variables
    private float thrustPerVerticalEngine = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        verticalEngines = new List<Engine>();
    }

    protected override void Start()
    {
        //Things that need to be called before base's start
        customBody = GetComponent<CustomKinematicBody>();
        PlanetLODManager.RegisterRendererLODsForChildren(transform, LODScaleMultiplier);

        base.Start();

        //Everything else
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        //Call this even when off so that we can properly deactivate engine effects
        UpdateEngineEffects();

        //Vehicle is completely stationary on its own, so we call this even when its off to simulate gravity
        altitudeController.UpdateVerticalTranslation();

        if (!on)
            return;

        UpdateHorizontalTranslation();
        UpdateRotation();
    }

    public override void SetPower(bool turnOn)
    {
        base.SetPower(turnOn);

        altitudeController.SetOffline(!turnOn);

        foreach (Engine engine in verticalEngines)
            engine.SetPower(turnOn);
    }

    protected override void UpdateEngineEffects()
    {
        base.UpdateEngineEffects();

        foreach (Engine engine in verticalEngines)
            engine.UpdateEngineEffects(false, CurrentFlooredUpwardSpeed(), altitudeController.maxControlledSpeed, VerticalEngineAudioCoefficient()); //Vertical engines don't do backwards thrusting
    }

    private void UpdateHorizontalTranslation()
    {
        //Move position based on speed
        if (gasPedal > 0.01f) //Forward (thrust)
        {
            if(MovingBackward() || rBody.velocity.magnitude < currentMaxSpeed)
                customBody.AddForce(Vector3.forward * GetThrustPower() * Time.fixedDeltaTime, Space.Self);
        }
        else if (gasPedal < -0.01f) //Backward (brakes)
        {
            if (!MovingBackward() || rBody.velocity.magnitude < currentMaxSpeed)
                customBody.AddForce(Vector3.back * brakePower * Time.fixedDeltaTime, Space.Self);
        }
        else if(!MovingBackward() && rBody.velocity.magnitude < currentMaxSpeed) //Keep current forward momentum going
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

    public void AddVerticalEngine(Engine engine, bool onStartUp)
    {
        verticalEngines.Add(engine);

        if (onStartUp)
            thrustPerVerticalEngine = verticalThrustPower / verticalEngines.Count;
        else
        {
            verticalThrustPower += thrustPerVerticalEngine;

            if (verticalEngines.Count == 1)
                altitudeController.SetOffline(false);
        }
    }

    public void RemoveVerticalEngine(Engine engine)
    {
        verticalEngines.Remove(engine);

        verticalThrustPower -= thrustPerVerticalEngine;

        if (verticalEngines.Count == 0)
            altitudeController.SetOffline(true);
    }

    private float CurrentFlooredUpwardSpeed() { return Mathf.Max(rBody.velocity.y, 0); }

    private float VerticalEngineAudioCoefficient() { return 1.0f + CurrentFlooredUpwardSpeed() / altitudeController.maxControlledSpeed; }

    public void SetAltitudeController(VehicleAltitudeController altitudeController)
    {
        this.altitudeController = altitudeController;
    }

    public override void SetCruiseControl(bool turnCruiseControlOn)
    {
        base.SetCruiseControl(turnCruiseControlOn);

        if (turnCruiseControlOn && altitudeController.IsOffline())
            altitudeController.SetOffline(false);
        else if (!turnCruiseControlOn && !hasDriverCurrently)
            altitudeController.SetOffline(true);
    }
}
