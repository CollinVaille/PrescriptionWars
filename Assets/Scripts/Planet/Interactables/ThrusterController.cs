using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterController : Interactable
{
    private enum ThrustingState { Ready, Thrusting, Resetting, EnginesOffline }

    //Customization
    public AudioClip thrustingSound;
    public float thrustingDuration = 12.0f, resettingDuration = 30.0f;

    //Status variables
    private ThrustingState currentState = ThrustingState.Ready;
    private bool activated = false;
    private float stateCooldown = 0.0f;
    private Vehicle vehicle = null;

    private void Start()
    {
        vehicle = transform.GetComponentInParent<Vehicle>();
        vehicle.CoupleThrusterToVehicle();
    }

    public override void Interact(PlanetPill interacting, bool turnOn)
    {
        base.Interact(interacting, turnOn);
        ActivateThruster(turnOn);
    }

    private void ActivateThruster(bool activate)
    {
        if (activate == activated)
            return;

        activated = activate;

        if (activate)
        {
            if(currentState == ThrustingState.Ready)
            {
                currentState = ThrustingState.Thrusting;
                StartCoroutine(ThrustingSequence());
            }
        }
        else
        {
            if (currentState == ThrustingState.EnginesOffline)
                currentState = ThrustingState.Ready;
        }
    }

    private IEnumerator ThrustingSequence()
    {
        List<Engine> engines = vehicle.GetForwardEngines();
        if(engines == null || engines.Count == 0 || !vehicle.PoweredOn())
        {
            currentState = ThrustingState.EnginesOffline;
            yield break;
        }

        //THRUSTING PHASE------------------------------------------------------------------------------------------------------------
        currentState = ThrustingState.Thrusting;
        stateCooldown = thrustingDuration;

        //Set audio to thrusting sound
        AudioClip originalEngineSound = engines[0].GetEngineAudio().clip;
        float originalMinDistance = engines[0].GetEngineAudio().minDistance;
        SetEngineAudio(engines, thrustingSound, originalMinDistance * 2.5f);

        //Apply the thrust
        int originalGear = vehicle.GetCurrentGear();
        vehicle.SetThrusting(true);

        //Wait for thrusting to be over
        float startTimeOfThrust = Time.timeSinceLevelLoad;
        for (; activated && vehicle.PoweredOn() && stateCooldown > 0; stateCooldown -= Time.deltaTime * vehicle.ForwardEngineAudioCoefficient())
            yield return null;

        //RESETTING PHASE------------------------------------------------------------------------------------------------------------
        currentState = ThrustingState.Resetting;
        stateCooldown = resettingDuration;

        //Take away the thrust
        vehicle.SetThrusting(false);

        if (activated)
        {
            //Wait for thrusting sound to be over (actually stop it 0.25f seconds early just to be safe and avoid accidental repeat)
            float startOfRemainingWait = Time.timeSinceLevelLoad;
            AudioSource sampleEngineAudio = engines[0].GetEngineAudio();
            yield return new WaitWhile(() => activated && sampleEngineAudio.time < thrustingSound.length - 0.25f);
            stateCooldown -= Time.timeSinceLevelLoad - startOfRemainingWait;
        }

        //Reset audio to normal engine sound
        SetEngineAudio(engines, originalEngineSound, originalMinDistance);

        //Wait for remaining duration of reset period
        for (; stateCooldown > 0; stateCooldown -= Time.deltaTime)
            yield return null;

        currentState = ThrustingState.Ready;
    }

    private void SetEngineAudio(List<Engine> engines, AudioClip clip, float minDistance)
    {
        foreach(Engine engine in engines)
        {
            AudioSource engineAudio = engine.GetEngineAudio();
            engineAudio.Stop();
            engineAudio.clip = clip;
            engineAudio.minDistance = minDistance;
            //If the engine is still being used, it will automatically resume playing via control from vehicle
        }
    }

    public override bool OverrideTriggerDescription() { return true; }

    public override string GetInteractionDescription()
    {
        if (activated)
        {
            if (currentState == ThrustingState.Thrusting)
                return "Deactivate Thruster " + GetCurrentStateString();
            else
                return "Reset Thruster " + GetCurrentStateString();
        }
        else
            return "Activate Thruster " + GetCurrentStateString();
    }

    private string GetCurrentStateString()
    {
        string toReturn;

        if (currentState == ThrustingState.Ready)
            toReturn = "Armed";
        else if (currentState == ThrustingState.Thrusting)
            toReturn = ((int)(stateCooldown * 100)) + "Gs Remaining";
        else if (currentState == ThrustingState.Resetting)
            toReturn = ((int)stateCooldown) + "s Until Armed";
        else //Engines offline
            toReturn = "Failure; Engines Offline";

        return "(" + toReturn + ")";
    }

    public bool IsThrusting() { return currentState == ThrustingState.Thrusting; }
}
