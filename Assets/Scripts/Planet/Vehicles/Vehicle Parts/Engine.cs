using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : VehiclePart
{
    public enum EngineType { ForwardEngine, VerticalEngine }

    public EngineType engineType = EngineType.ForwardEngine;
    public ParticleSystem exhaustCloud, exhaustStream;
    public ExhaustManager exhaustManager;
    public bool supportReverseThrusting = true;

    public Vector3 center = Vector3.zero;

    private Fire engineFire = null;
    private AudioSource engineAudio;

    protected override void Start()
    {
        base.Start();

        engineAudio = GetComponent<AudioSource>();
        if (engineAudio)
            God.god.ManageAudioSource(engineAudio);

        AddEngineToVehicle(true);
    }

    protected override void PartFailure()
    {
        //No more power
        SetPower(false);

        base.PartFailure();

        //Make sure all effects are updated before we stop getting updates (vehicle is responsible for calling update but we are removing ourselves from their list)
        UpdateEngineEffects(false, 1, 1, 1);

        RemoveEngineFromVehicle();
    }

    protected override void PartRecovery()
    {
        base.PartRecovery();

        AddEngineToVehicle(false);
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

    public void UpdateEngineEffects(bool backwardThrusting, float currentSpeed, float absoluteMaxSpeed, float engineAudioCoefficient)
    {
        bool shouldBeOn = belongsTo.PoweredOn() && working && (!backwardThrusting || supportReverseThrusting);

        UpdateEngineAudio(shouldBeOn, engineAudioCoefficient);
        UpdateEngineExhaust(shouldBeOn, backwardThrusting, currentSpeed, absoluteMaxSpeed);
    }

    private void UpdateEngineAudio(bool shouldBeOn, float pitch)
    {
        if (!engineAudio)
            return;

        if (shouldBeOn)
        {
            if (!engineAudio.isPlaying)
                engineAudio.Play();

            engineAudio.pitch = pitch;
        }
        else
        {
            if (engineAudio.isPlaying)
                engineAudio.Stop();
        }
    }

    private void UpdateEngineExhaust(bool shouldBeOn, bool backwardThrusting, float currentSpeed, float absoluteMaxSpeed)
    {
        if (exhaustManager == null)
            return;

        if(shouldBeOn)
        {
            if (!exhaustCloud.isPlaying)
                exhaustCloud.Play();

            if (!exhaustStream.isPlaying)
                exhaustStream.Play();

            exhaustManager.UpdateExhaustStream(exhaustStream, backwardThrusting, currentSpeed, absoluteMaxSpeed);
        }
        else
        {
            if (exhaustCloud.isPlaying)
                exhaustCloud.Stop();

            if (exhaustStream.isPlaying)
                exhaustStream.Stop();
        }
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

    public AudioSource GetEngineAudio() { return engineAudio; }

    private void AddEngineToVehicle(bool onStartUp)
    {
        if (engineType == EngineType.ForwardEngine)
            belongsTo.AddForwardEngine(this, onStartUp);
        else //Vertical engine
            ((Aircraft)belongsTo).AddVerticalEngine(this, onStartUp);
    }

    private void RemoveEngineFromVehicle()
    {
        if (engineType == EngineType.ForwardEngine)
            belongsTo.RemoveForwardEngine(this);
        else //Vertical engine
            ((Aircraft)belongsTo).RemoveVerticalEngine(this);
    }
}
