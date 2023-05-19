using UnityEngine;

public class PlanetClamperMachine : PlanetFactoryMachine
{
    private const float deploymentStage2Threshold = 0.5f, lungeDuration = 0.1f;
    private bool clamping = true, atStage2 = false;
    private float elapsedTimeForThisLunge = 0.0f;

    public Vector3 subjectPinPosition;
    public Mesh capsuleMesh;
    public AudioClip holdArmsDeploy, platformRetract, clampSound, platformDeploy, holdArmsRetract;
    public PlanetFactoryMachinePart platform, holdArms, lowerClamper, upperClamper;

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage, float stepDuration)
    {
        if(step == MachineStep.Intake)
        {
            if (stepCompletionPercentage < deploymentStage2Threshold)
                LerpMachinePartLocalPosition(holdArms, stepCompletionPercentage / deploymentStage2Threshold, GeneralHelperMethods.WhichVector.Z);
            else
            {
                if(!atStage2)
                {
                    atStage2 = true;
                    intakeSlot.GetComponent<AudioSource>().PlayOneShot(platformRetract);
                }

                LerpMachinePartLocalPosition(platform, 1.0f - (stepCompletionPercentage - deploymentStage2Threshold) / (1.0f - deploymentStage2Threshold),
                    GeneralHelperMethods.WhichVector.Z);
            }
        }
        else if(step == MachineStep.Process)
        {
            elapsedTimeForThisLunge += stepDuration;
            if (elapsedTimeForThisLunge > lungeDuration)
            {
                if (clamping)
                {
                    processingSlot.GetComponent<AudioSource>().PlayOneShot(clampSound);
                    SetSubjectsMeshToCapsule(processingSlot);
                }

                elapsedTimeForThisLunge = 0.0f;
                clamping = !clamping;
            }

            float clampExtensionPercentage = elapsedTimeForThisLunge / lungeDuration;
            if (!clamping)
                clampExtensionPercentage = 1.0f - clampExtensionPercentage;

            LerpMachinePartLocalPosition(lowerClamper, clampExtensionPercentage, GeneralHelperMethods.WhichVector.Y);
            LerpMachinePartLocalPosition(upperClamper, clampExtensionPercentage, GeneralHelperMethods.WhichVector.Y);
        }
        else if(step == MachineStep.Outtake)
        {
            if (stepCompletionPercentage < deploymentStage2Threshold)
                LerpMachinePartLocalPosition(platform, stepCompletionPercentage / deploymentStage2Threshold, GeneralHelperMethods.WhichVector.Z);
            else
            {
                if (!atStage2)
                {
                    atStage2 = true;
                    processingSlot.GetComponent<AudioSource>().PlayOneShot(holdArmsRetract);
                }

                LerpMachinePartLocalPosition(holdArms, 1.0f - (stepCompletionPercentage - deploymentStage2Threshold) / (1.0f - deploymentStage2Threshold),
                    GeneralHelperMethods.WhichVector.Z);
            }
        }
    }

    protected override void OnStartOfStep(MachineStep step)
    {
        atStage2 = false;

        if(step == MachineStep.Intake)
        {
            intakeSlot.position = transform.TransformPoint(subjectPinPosition);

            LerpMachinePartLocalPosition(holdArms, 0.0f, GeneralHelperMethods.WhichVector.Z);
            LerpMachinePartLocalPosition(platform, 1.0f, GeneralHelperMethods.WhichVector.Z);

            intakeSlot.GetComponent<AudioSource>().PlayOneShot(holdArmsDeploy);
        }
        else if(step == MachineStep.Process)
        {
            LerpMachinePartLocalPosition(holdArms, 1.0f, GeneralHelperMethods.WhichVector.Z);
            LerpMachinePartLocalPosition(platform, 0.0f, GeneralHelperMethods.WhichVector.Z);
        }
        else if(step == MachineStep.Outtake)
        {
            LerpMachinePartLocalPosition(holdArms, 1.0f, GeneralHelperMethods.WhichVector.Z);
            LerpMachinePartLocalPosition(platform, 0.0f, GeneralHelperMethods.WhichVector.Z);

            processingSlot.GetComponent<AudioSource>().PlayOneShot(platformDeploy);
        }

        LerpMachinePartLocalPosition(lowerClamper, 0.0f, GeneralHelperMethods.WhichVector.Y);
        LerpMachinePartLocalPosition(upperClamper, 0.0f, GeneralHelperMethods.WhichVector.Y);
    }

    protected override void OnEndOfStep(MachineStep step)
    {
        if (step == MachineStep.Process)
            SetSubjectsMeshToCapsule(processingSlot);
    }

    private void SetSubjectsMeshToCapsule(Transform subject)
    {
        MeshFilter subjectsMeshFilter = subject.GetComponent<MeshFilter>();

        if(subjectsMeshFilter.mesh != capsuleMesh)
            subjectsMeshFilter.mesh = capsuleMesh;
    }
}
