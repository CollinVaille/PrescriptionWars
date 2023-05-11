using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetConveyorBelt : PlanetFactoryMachine
{
    public Material conveyorBeltMaterial;
    public float beltLength = 5.0f, localCargoHeight = 1.0f;

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage)
    {
        if (step == MachineStep.Intake)
            LerpCargoPosition(intakeSlot, -beltLength, 0.0f, stepCompletionPercentage);
        else if(step == MachineStep.Outtake)
            LerpCargoPosition(processingSlot, 0.0f, beltLength, stepCompletionPercentage);
    }

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Intake) //Snap to intake position
            LerpCargoPosition(intakeSlot, -beltLength, 0.0f, 0.0f);
        else if(step == MachineStep.Process) //Snap to middle position
            LerpCargoPosition(intakeSlot, 0.0f, 0.0f, 0.0f);

    }

    private void LerpCargoPosition(Transform cargo, float zAtStart, float zAtEnd, float stepCompletionPercentage)
    {
        cargo.localPosition = new Vector3(0.0f, localCargoHeight, Mathf.Lerp(zAtStart, zAtEnd, stepCompletionPercentage));
    }
}
