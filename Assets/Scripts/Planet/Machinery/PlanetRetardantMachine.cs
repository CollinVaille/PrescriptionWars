using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRetardantMachine : PlanetFactoryMachine
{
    private const float deploymentStage2Threshold = 0.33f, deploymentStage3Threshold = 0.66f, spinAtDegreesPerSecond = 360.0f;
    private int stage = 1;

    public Vector3 subjectPinPosition;
    public Material retardedMaterial;
    public ParticleSystem retardantStream;
    public AudioClip intakeSound, outtakeSound, mechanicalSound, paintSound, coolingSound;
    public PlanetFactoryMachinePart platform, lowerClamper, upperClamper;
    public Transform[] spinningParts;

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage, float stepDuration)
    {
        if (step == MachineStep.Intake)
        {
            if (stepCompletionPercentage < deploymentStage2Threshold)
                LerpMachinePartLocalPosition(upperClamper, stepCompletionPercentage / deploymentStage2Threshold, GeneralHelperMethods.WhichVector.Y);
            else if (stepCompletionPercentage < deploymentStage3Threshold)
            {
                PlayMechanicalSoundIfStageStart(2, intakeSlot);
                LerpMachinePartLocalPosition(platform, 1.0f - (stepCompletionPercentage - deploymentStage2Threshold) / (deploymentStage3Threshold - deploymentStage2Threshold),
                    GeneralHelperMethods.WhichVector.Z);
            }
            else
            {
                PlayMechanicalSoundIfStageStart(3, intakeSlot);
                LerpMachinePartLocalPosition(lowerClamper, (stepCompletionPercentage - deploymentStage3Threshold) / (1.0f - deploymentStage3Threshold),
                    GeneralHelperMethods.WhichVector.Y);
            }
        }
        else if (step == MachineStep.Process)
        {
            SetSpinRotation(spinAtDegreesPerSecond * stepDuration);
            ChanceForCoolingFlicker(stepDuration);
        }
        else if (step == MachineStep.Outtake)
        {
            if (stepCompletionPercentage < deploymentStage2Threshold)
                LerpMachinePartLocalPosition(lowerClamper, 1.0f - stepCompletionPercentage / deploymentStage2Threshold, GeneralHelperMethods.WhichVector.Y);
            else if (stepCompletionPercentage < deploymentStage3Threshold)
            {
                PlayMechanicalSoundIfStageStart(2, processingSlot);
                LerpMachinePartLocalPosition(platform, (stepCompletionPercentage - deploymentStage2Threshold) / (deploymentStage3Threshold - deploymentStage2Threshold),
                    GeneralHelperMethods.WhichVector.Z);
            }
            else
            {
                PlayMechanicalSoundIfStageStart(3, processingSlot);
                LerpMachinePartLocalPosition(upperClamper, 1.0f - (stepCompletionPercentage - deploymentStage3Threshold) / (1.0f - deploymentStage3Threshold),
                    GeneralHelperMethods.WhichVector.Y);
            }
        }
    }

    protected override void OnStartOfStep(MachineStep step)
    {
        stage = 1;

        if (step == MachineStep.Intake)
        {
            intakeSlot.position = transform.TransformPoint(subjectPinPosition);

            LerpMachinePartLocalPosition(lowerClamper, 0.0f, GeneralHelperMethods.WhichVector.Y);
            LerpMachinePartLocalPosition(upperClamper, 0.0f, GeneralHelperMethods.WhichVector.Y);
            LerpMachinePartLocalPosition(platform, 1.0f, GeneralHelperMethods.WhichVector.Z);

            SetSoundOnSubject(intakeSlot, intakeSound, loop: false);
        }
        else if (step == MachineStep.Process)
        {
            LerpMachinePartLocalPosition(lowerClamper, 1.0f, GeneralHelperMethods.WhichVector.Y);
            LerpMachinePartLocalPosition(upperClamper, 1.0f, GeneralHelperMethods.WhichVector.Y);
            LerpMachinePartLocalPosition(platform, 0.0f, GeneralHelperMethods.WhichVector.Z);

            retardantStream.Play(true);
            SetSoundOnSubject(processingSlot, paintSound);
        }
        else if (step == MachineStep.Outtake)
        {
            SetSoundOnSubject(processingSlot, outtakeSound, loop: false);
        }
    }

    protected override void OnEndOfStep(MachineStep step)
    {
        if(step == MachineStep.Process)
        {
            PaintSubject(processingSlot);
            retardantStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            SetSoundOnSubject(processingSlot, null);
        }
    }

    private void PlayMechanicalSoundIfStageStart(int stageNumber, Transform subject)
    {
        if (stage != stageNumber)
        {
            stage = stageNumber;
            SetSoundOnSubject(subject, mechanicalSound, loop: false);
        }
    }

    private void ChanceForCoolingFlicker(float stepDuration)
    {
        if(Random.Range(0.0f, 0.25f) < stepDuration)
        {
            processingSlot.GetComponent<AudioSource>().PlayOneShot(coolingSound);

            if (Random.Range(0, 3) == 0)
                PaintSubject(processingSlot);
        }
    }

    private void PaintSubject(Transform subject)
    {
        MeshRenderer subjectsMeshRenderer = subject.GetComponent<MeshRenderer>();

        if (subjectsMeshRenderer.sharedMaterial != retardedMaterial)
            subjectsMeshRenderer.sharedMaterial = retardedMaterial;

        //Cools from gel material ("swamp") to harder rocky material
        PlanetMaterial.SetMaterialType(PlanetMaterialType.Rock, subject.gameObject);
    }

    private void SetSpinRotation(float spinYAxisByThisManyDegrees)
    {
        //Spin subject
        processingSlot.Rotate(0.0f, spinYAxisByThisManyDegrees, 0.0f, Space.World);

        //Spin machine parts
        for (int x = 0; x < spinningParts.Length; x++)
            spinningParts[x].Rotate(0.0f, spinYAxisByThisManyDegrees, 0.0f, Space.World);
        
        //spinningParts[x].eulerAngles = GeneralHelperMethods.GetModifiedVector3(spinningParts[x].eulerAngles, spinYAxisByThisManyEulerAngles, GeneralHelperMethods.WhichVector.Y);
    }
}
