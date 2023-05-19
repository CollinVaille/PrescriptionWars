using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetFactoryMachine : Interactable
{
    protected enum MachineStep { None, Intake, Process, Outtake }

    public Transform intakeSlot, processingSlot;
    public float processStepThreshold = 0.33f, outtakeStepThreshold = 0.66f;
    private float lastCycleUpdateAtPercentage = 0.0f;
    private bool performedOuttakeForCurrentSubject = false, on = true;
    private MachineStep currentStep = MachineStep.None;
    public PlanetFactoryMachine outputsTo;

    public void PerformMachineCyleUpdate(float cycleCompletionPercentage, float stepDuration)
    {
        //Update all machines that come after us first...

        //It it optimal from a "pipelining" perspective to update the machines end to beginning.
        //This is because you need the next machine in line to finish its job before you can insert a new job into it.
        if(outputsTo)
            outputsTo.PerformMachineCyleUpdate(cycleCompletionPercentage, stepDuration);

        //Then update our machine
        if (cycleCompletionPercentage < processStepThreshold) //Step 1: Intake
        {
            if (currentStep == MachineStep.Outtake)
                EndCurrentStep(MachineStep.Outtake);

            if(on)
            {
                if (currentStep == MachineStep.Intake && intakeSlot && !processingSlot)
                    PerformMachineStepUpdate(MachineStep.Intake, cycleCompletionPercentage / processStepThreshold, stepDuration);
                else if (ReadyToProgressToNextStep(MachineStep.Intake))
                    MoveToNextStep(MachineStep.Intake);
            }
        }
        else if (cycleCompletionPercentage < outtakeStepThreshold) //Step 2: Process
        {
            if (currentStep == MachineStep.Intake)
                EndCurrentStep(MachineStep.Intake);

            if (on)
            {
                if (currentStep == MachineStep.Process && processingSlot)
                    PerformMachineStepUpdate(MachineStep.Process, (cycleCompletionPercentage - processStepThreshold) / (outtakeStepThreshold - processStepThreshold), stepDuration);
                else if (ReadyToProgressToNextStep(MachineStep.Process))
                    MoveToNextStep(MachineStep.Process);
            }
        }
        else //Step 3: Outtake
        {
            if (currentStep == MachineStep.Process)
                EndCurrentStep(MachineStep.Process);

            if (on)
            {
                if (currentStep == MachineStep.Outtake && processingSlot && !performedOuttakeForCurrentSubject)
                    PerformMachineStepUpdate(MachineStep.Outtake, (cycleCompletionPercentage - outtakeStepThreshold) / (1.0f - outtakeStepThreshold), stepDuration);
                else if (ReadyToProgressToNextStep(MachineStep.Outtake))
                    MoveToNextStep(MachineStep.Outtake);
            }
        }

        if(on)
            lastCycleUpdateAtPercentage = cycleCompletionPercentage;
    }

    protected virtual void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage, float stepDuration) { }

    protected virtual void OnStartOfStep(MachineStep step) { }

    protected virtual void OnEndOfStep(MachineStep step) { }

    protected virtual bool IsStartingMachine() { return false; }

    protected void LerpMachinePartLocalPosition(PlanetFactoryMachinePart machinePart, float lerpPercentage, GeneralHelperMethods.WhichVector whichVector)
    {
        float newVectorValue = Mathf.Lerp(machinePart.retracted, machinePart.extended, lerpPercentage);
        machinePart.machinePart.localPosition = GeneralHelperMethods.GetModifiedVector3(machinePart.machinePart.localPosition, newVectorValue, whichVector);
    }

    protected void LerpMachinePartLocalScale(PlanetFactoryMachinePart machinePart, float lerpPercentage, GeneralHelperMethods.WhichVector whichVector)
    {
        float newVectorValue = Mathf.Lerp(machinePart.retracted, machinePart.extended, lerpPercentage);
        machinePart.machinePart.localScale = GeneralHelperMethods.GetModifiedVector3(machinePart.machinePart.localScale, newVectorValue, whichVector);
    }

    protected void SetSoundOnSubject(Transform subjectToPlayAudioFrom, AudioClip audioToPlay, bool loop = true)
    {
        AudioSource subjectsAudioSource = subjectToPlayAudioFrom.GetComponent<AudioSource>();

        if (audioToPlay)
        {
            subjectsAudioSource.Stop();
            subjectsAudioSource.clip = audioToPlay;
            subjectsAudioSource.loop = loop;
            subjectsAudioSource.Play();
        }
        else
        {
            subjectsAudioSource.Stop();
            subjectsAudioSource.clip = null;
        }
    }

    private void MoveToNextStep(MachineStep nextStep)
    {
        if (nextStep == MachineStep.Process)
            PushInputToProcessing();
        else if (nextStep == MachineStep.Intake)
        {
            if (processingSlot && NextMachineIsReadyToReceive())
                PushProcessingToNextMachine();
        }

        currentStep = nextStep;
        OnStartOfStep(nextStep);
    }

    private void EndCurrentStep(MachineStep step)
    {
        OnEndOfStep(step);

        if(step == MachineStep.Outtake)
        {
            performedOuttakeForCurrentSubject = true;

            if (processingSlot && NextMachineIsReadyToReceive())
                PushProcessingToNextMachine();
        }

        currentStep = MachineStep.None;
    }

    private void PushInputToProcessing()
    {
        processingSlot = intakeSlot;
        intakeSlot = null;
    }

    private void PushProcessingToNextMachine()
    {
        if (outputsTo)
            outputsTo.intakeSlot = processingSlot;
        processingSlot = null;

        performedOuttakeForCurrentSubject = false;
    }

    private bool NextMachineIsReadyToReceive()
    {
        if (!outputsTo)
            return false;
        else if (outputsTo.intakeSlot || outputsTo.processingSlot)
            return false;
        else
            return outputsTo.IsOn();
    }

    private bool ReadyToProgressToNextStep(MachineStep nextStep)
    {
        if (currentStep == nextStep)
            return false;
        else if (nextStep == MachineStep.Intake)
        {
            if (intakeSlot || IsStartingMachine())
            {
                if (processingSlot)
                    return NextMachineIsReadyToReceive();
                else
                    return true;
            }
            else
                return false;
        }
        else if (nextStep == MachineStep.Process)
            return intakeSlot && !processingSlot && ACloserToCThanB(lastCycleUpdateAtPercentage, 0.0f, processStepThreshold);
        else if (nextStep == MachineStep.Outtake)
        {
            if (IsStartingMachine())
                return !processingSlot && outputsTo && !outputsTo.intakeSlot;
            else
            {
                if (!processingSlot || !ACloserToCThanB(lastCycleUpdateAtPercentage, processStepThreshold, outtakeStepThreshold))
                    return false;
                else if (!performedOuttakeForCurrentSubject)
                    return true;
                else
                    return NextMachineIsReadyToReceive();
            }
        }
        else //Shouldn't ever get here. This is really just to prevent compiler errors
            return false;
    }

    private bool ACloserToCThanB(float a, float b, float c) //Where its assumed b <= c
    {
        return a > (b + c) * 0.5f;
    }

    public override void Interact(Pill interacting, bool turnOn)
    {
        base.Interact(interacting, turnOn);

        on = turnOn;
    }

    public override bool OverrideTriggerDescription() { return true; }

    protected override string GetInteractionVerb() { return on ? "Turn Off" : "Turn On"; }

    public bool IsOn() { return on; }
}


[System.Serializable]
public class PlanetFactoryMachinePart
{
    public Transform machinePart;
    public float retracted, extended;
}