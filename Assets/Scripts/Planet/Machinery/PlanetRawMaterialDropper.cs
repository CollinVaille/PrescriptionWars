using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRawMaterialDropper : PlanetFactoryMachine
{
    public GameObject payloadToInstantiate;
    public Vector3 localStartingPosition, localEndingPosition;

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Intake && !intakeSlot)
        {
            intakeSlot = Instantiate(payloadToInstantiate).transform;
            intakeSlot.position = transform.TransformPoint(localStartingPosition);
            intakeSlot.rotation = transform.rotation;

            if (!intakeSlot.GetComponent<Rigidbody>())
                intakeSlot.gameObject.AddComponent<Rigidbody>();
        }
        else if (step == MachineStep.Process)
        {
            if (intakeSlot.GetComponent<Rigidbody>())
                Destroy(intakeSlot.gameObject.GetComponent<Rigidbody>());

            processingSlot.position = transform.TransformPoint(localEndingPosition);
        }
    }

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage)
    {
        //hakuna matata
    }

    protected override bool IsStartingMachine() { return true; }
}
