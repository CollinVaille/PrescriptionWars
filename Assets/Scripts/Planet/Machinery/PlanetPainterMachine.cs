using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetPainterMachine : PlanetFactoryMachine
{
    //Customization
    public Vector3 subjectPinPosition;
    public Material fallbackPaintMaterial;
    public AudioClip lowerChamber, raiseChamber, fillSound, drainSound;
    public PlanetFactoryMachinePart chamber, chamberFluidPosition, chamberFluidScale;

    //Memory
    private bool startedPhase2 = false;
    private Material paintMaterial;

    protected override void PerformMachineStepUpdate(MachineStep step, float stepCompletionPercentage, float stepDuration)
    {
        if(step == MachineStep.Intake)
            LerpMachinePartLocalPosition(chamber, stepCompletionPercentage, GeneralHelperMethods.WhichVector.Y);
        else if(step == MachineStep.Process)
        {
            if(stepCompletionPercentage < 0.5f)
            {
                LerpMachinePartLocalPosition(chamberFluidPosition, stepCompletionPercentage * 2.0f, GeneralHelperMethods.WhichVector.Y);
                LerpMachinePartLocalScale(chamberFluidScale, stepCompletionPercentage * 2.0f, GeneralHelperMethods.WhichVector.Y);
            }
            else
            {
                if(!startedPhase2)
                {
                    startedPhase2 = true;
                    SetSoundOnSubject(processingSlot, drainSound, loop: true);
                    PaintThisShit(processingSlot);
                }

                LerpMachinePartLocalPosition(chamberFluidPosition, 2.0f * (0.5f - (stepCompletionPercentage - 0.5f)), GeneralHelperMethods.WhichVector.Y);
                LerpMachinePartLocalScale(chamberFluidScale, 2.0f * (0.5f - (stepCompletionPercentage - 0.5f)), GeneralHelperMethods.WhichVector.Y);
            }
        }
        else if (step == MachineStep.Outtake)
            LerpMachinePartLocalPosition(chamber, 1.0f - stepCompletionPercentage, GeneralHelperMethods.WhichVector.Y);
    }

    protected override void OnStartOfStep(MachineStep step)
    {
        startedPhase2 = false;

        if (step == MachineStep.Intake)
        {
            intakeSlot.position = transform.TransformPoint(subjectPinPosition);

            LerpMachinePartLocalPosition(chamber, 0.0f, GeneralHelperMethods.WhichVector.Y);
            chamberFluidPosition.machinePart.gameObject.SetActive(false);

            SetSoundOnSubject(intakeSlot, lowerChamber, loop: false);
        }
        else if(step == MachineStep.Process)
        {
            LerpMachinePartLocalPosition(chamber, 1.0f, GeneralHelperMethods.WhichVector.Y);
            PaintThisShit(chamberFluidPosition.machinePart); //Set the material of the paint fluid
            chamberFluidPosition.machinePart.gameObject.SetActive(true);

            SetSoundOnSubject(processingSlot, fillSound, loop: true);
        }
        else if(step == MachineStep.Outtake)
        {
            LerpMachinePartLocalPosition(chamber, 1.0f, GeneralHelperMethods.WhichVector.Y);

            SetSoundOnSubject(processingSlot, raiseChamber, loop: false);
        }

        LerpMachinePartLocalPosition(chamberFluidPosition, 0.0f, GeneralHelperMethods.WhichVector.Y);
        LerpMachinePartLocalScale(chamberFluidScale, 0.0f, GeneralHelperMethods.WhichVector.Y);
    }

    protected override void OnEndOfStep(MachineStep step)
    {
        if(step == MachineStep.Intake)
            SetSoundOnSubject(intakeSlot, null);
        else if (step == MachineStep.Process)
        {
            PaintThisShit(processingSlot);
            SetSoundOnSubject(processingSlot, null);
            chamberFluidPosition.machinePart.gameObject.SetActive(false);
        }
        else if(step == MachineStep.Outtake)
            SetSoundOnSubject(processingSlot, null);
    }

    private void PaintThisShit(Transform shitToPaint)
    {
        MeshRenderer subjectsMeshRenderer = shitToPaint.GetComponent<MeshRenderer>();
        Material toPaintWith = GetPaintMaterial();

        if (toPaintWith && subjectsMeshRenderer.sharedMaterial != toPaintWith)
            subjectsMeshRenderer.sharedMaterial = toPaintWith;
    }

    private Material GetPaintMaterial()
    {
        if (paintMaterial)
            return paintMaterial;
        else
        {
            paintMaterial = GetPaintMaterialNotCached();

            if (!paintMaterial && fallbackPaintMaterial)
                paintMaterial = fallbackPaintMaterial;

            return paintMaterial;
        }
    }

    private Material GetPaintMaterialNotCached()
    {
        Army army = Army.GetArmy(0);
        if (!army || army.squads == null || army.squads.Count == 0)
            return null;

        Squad squad = army.squads[0];
        if (squad.members == null || squad.members.Count == 0)
            return null;

        PlanetPill pill = squad.members[Random.Range(0, squad.members.Count)];
        if (!pill)
            return null;

        MeshRenderer pillRenderer = pill.GetComponent<MeshRenderer>();
        if (!pillRenderer)
            return null;

        return pillRenderer.sharedMaterial;
    }
}
