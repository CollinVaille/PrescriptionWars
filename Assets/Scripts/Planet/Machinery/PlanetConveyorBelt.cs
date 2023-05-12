using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetConveyorBelt : PlanetFactoryMachine
{
    public Transform intakePoint, outtakePoint;

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage, float stepDuration)
    {
        if (step == MachineStep.Intake)
            intakeSlot.position = Vector3.Lerp(intakePoint.position, MiddlePoint, stepCompletionPercentage);
        else if (step == MachineStep.Outtake)
            processingSlot.position = Vector3.Lerp(MiddlePoint, outtakePoint.position, stepCompletionPercentage);
    }

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Intake) //Snap to intake position
            intakeSlot.position = intakePoint.position;
        else if (step == MachineStep.Process) //Snap to middle position
            processingSlot.position = MiddlePoint;
    }

    private Vector3 MiddlePoint
    {
        get
        {
            return (intakePoint.position + outtakePoint.position) * 0.5f;
        }
    }
}
