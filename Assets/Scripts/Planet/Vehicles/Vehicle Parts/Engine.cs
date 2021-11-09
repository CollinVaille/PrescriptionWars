using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : VehiclePart
{
    public ParticleSystem exhaustCloud, exhaustStream;
    public ExhaustManager exhaustManager;

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

    public void UpdateExhaustStream(bool backwardThrusting, float currentSpeed, float absoluteMaxSpeed)
    {
        if (!working || exhaustManager == null)
            return;

        exhaustManager.UpdateExhaustStream(exhaustStream, backwardThrusting, currentSpeed, absoluteMaxSpeed);
    }

    private void UpdateEngineFire()
    {
        //Start fire if applicable
        if(health < initialHealth / 2.0f && !engineFire)
            engineFire = Fire.SetOnFire(transform, center, Mathf.Min(initialHealth / health, 2.5f));

        //Update intensity of fire if pre-existing
        else if (engineFire && belongsTo.PoweredOn())
            engineFire.intensity = Mathf.Min(initialHealth / health, 2.5f);
    }
}
