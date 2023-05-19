using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFactoryBox : PlanetFactoryMachine
{
    public Vector3 subjectPinPosition;
    public PlanetDropperMachine dropper;

    protected override void OnStartOfStep(MachineStep step)
    {
        if (step == MachineStep.Intake)
        {
            intakeSlot.position = transform.TransformPoint(subjectPinPosition);
        }
        else if (step == MachineStep.Process)
            SuckProcessingIntoBlackHole();
    }

    private void SuckProcessingIntoBlackHole()
    {
        //Stop any playing audio on this subject and tell the audio management system to stop managing this entity
        SetSoundOnSubject(processingSlot, null);
        God.god.UnmanageAudioSource(processingSlot.GetComponent<AudioSource>());

        //Hide it
        processingSlot.gameObject.SetActive(false);

        //Tell dropper this subject can be reused now
        dropper.AddSubjectToBuffer(processingSlot);

        //We're done with this subject. This step is needed so we can receive the next subject.
        processingSlot = null;
    }
}
