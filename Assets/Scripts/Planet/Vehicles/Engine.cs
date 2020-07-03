using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : VehiclePart
{
    public ParticleSystem exhaustCloud, exhaustStream;

    public Vector3 center = Vector3.zero;

    private Fire engineFire = null;

    protected override void Start()
    {
        base.Start();

        belongsTo.AddEngine(this, true);
    }

    protected override void PartFailure()
    {
        SetPower(false);

        base.PartFailure();

        belongsTo.RemoveEngine(this);
    }

    protected override void PartRecovery()
    {
        base.PartRecovery();

        belongsTo.AddEngine(this, false);
    }

    protected override void DamageHealth(float amount)
    {
        base.DamageHealth(amount);

        UpdateEngineFire();
    }

    //Shut on/off exhaust based on power
    public void SetPower(bool on)
    {
        UpdateEngineFire();

        if (!working)
            return;

        if(on)
        {
            exhaustCloud.gameObject.SetActive(true);
            exhaustCloud.Play();

            exhaustStream.gameObject.SetActive(true);
            exhaustStream.Play();
        }
        else
        {
            exhaustCloud.Stop();
            exhaustCloud.gameObject.SetActive(false);

            exhaustStream.Stop();
            exhaustStream.gameObject.SetActive(false);
        }
    }

    public void UpdateExhaustStream(bool backwardThrusting, int currentSpeed)
    {
        if (!working)
            return;

        //Change size of exhaust based on speed
        ParticleSystem.MainModule mainMod = exhaustStream.main;
        mainMod.startSizeY = 0.5f + currentSpeed / 60.0f;

        //Change speed of exhaust simulation (also based on speed of vehicle)
        mainMod.simulationSpeed = 1.0f + currentSpeed / 3.0f;

        //Update whether exhaust is pointing forward/backward
        Vector3 exhaustRotation = exhaustStream.transform.localEulerAngles;
        if (backwardThrusting)
            exhaustRotation.y = 0;
        else
            exhaustRotation.y = 180;
        exhaustStream.transform.localEulerAngles = exhaustRotation;
    }

    private void UpdateEngineFire()
    {
        //Start fire if applicable
        if(health < initialHealth / 2.0f && !engineFire)
        {
            engineFire = Instantiate(Planet.planet.firePrefab).GetComponent<Fire>();
            engineFire.Ignite(transform, center);
        }

        //Update intensity of fire
        if (engineFire && belongsTo.PoweredOn())
            engineFire.intensity = Mathf.Min(initialHealth / health, 2.5f);
    }
}
