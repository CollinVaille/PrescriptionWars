using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetFactoryMachine : MonoBehaviour
{
    protected enum MachineStep { Intake, Process, Outtake }

    [HideInInspector] public Transform intakeSlot, processingSlot;
    public float processStepThreshold = 0.33f, outtakeStepThreshold = 0.66f;
    private float lastCycleUpdateAtPercentage = 0.0f;
    public PlanetFactoryMachine outputsTo;

    public void PerformMachineCyleUpdate(float cycleCompletionPercentage)
    {
        if (cycleCompletionPercentage < processStepThreshold) //Step 1: Intake
        {
            if (intakeSlot && !processingSlot)
                PerformMachineStepUpdate(MachineStep.Intake, cycleCompletionPercentage / processStepThreshold);
            else if (ReadyToProgressToNextStep(MachineStep.Intake))
                MoveToNextStep(MachineStep.Intake);
        }
        else if (cycleCompletionPercentage < outtakeStepThreshold) //Step 2: Process
        {
            if(processingSlot)
                PerformMachineStepUpdate(MachineStep.Process, (cycleCompletionPercentage - processStepThreshold) / (outtakeStepThreshold - processStepThreshold));
            else if (ReadyToProgressToNextStep(MachineStep.Process))
                MoveToNextStep(MachineStep.Process);
        }
        else //Step 3: Outtake
        {
            if(processingSlot && outputsTo && !outputsTo.intakeSlot)
                PerformMachineStepUpdate(MachineStep.Outtake, (cycleCompletionPercentage - outtakeStepThreshold) / (1.0f - outtakeStepThreshold));
            else if (ReadyToProgressToNextStep(MachineStep.Outtake))
                MoveToNextStep(MachineStep.Outtake);
        }

        lastCycleUpdateAtPercentage = cycleCompletionPercentage;

        if(outputsTo)
            outputsTo.PerformMachineCyleUpdate(cycleCompletionPercentage);
    }

    protected virtual void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage) { }

    protected virtual void OnStartOfStep(MachineStep step) { }

    protected virtual bool IsStartingMachine() { return false; }

    protected virtual void BeforePushingToNextMachine() { }

    private void MoveToNextStep(MachineStep nextStep)
    {
        if (nextStep == MachineStep.Process)
            PushInputToProcessing();
        else if (nextStep == MachineStep.Intake)
        {
            if (processingSlot && outputsTo && !outputsTo.intakeSlot)
            {
                BeforePushingToNextMachine();
                PushProcessingToNextMachine();
            }
        }

        OnStartOfStep(nextStep);
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
    }

    private bool ReadyToProgressToNextStep(MachineStep nextStep)
    {
        if (nextStep == MachineStep.Intake)
            return processingSlot && outputsTo && !outputsTo.intakeSlot && ACloserToCThanB(lastCycleUpdateAtPercentage, outtakeStepThreshold, 1.0f);
        else if (nextStep == MachineStep.Process)
            return intakeSlot && !processingSlot && ACloserToCThanB(lastCycleUpdateAtPercentage, 0.0f, processStepThreshold);
        else if (nextStep == MachineStep.Outtake)
        {
            if (IsStartingMachine() && !processingSlot && outputsTo && !outputsTo.intakeSlot)
                return true;
            else
                return processingSlot && outputsTo && !outputsTo.intakeSlot && ACloserToCThanB(lastCycleUpdateAtPercentage, processStepThreshold, outtakeStepThreshold);
        }
        else //Shouldn't ever get here. This is really just to prevent compiler errors
            return false;
    }

    private bool ACloserToCThanB(float a, float b, float c) //Where its assumed b <= c
    {
        return a > (b + c) * 0.5f;
    }
}
