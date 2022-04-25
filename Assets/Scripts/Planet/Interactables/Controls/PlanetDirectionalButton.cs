using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDirectionalButton : Interactable
{
    private enum DirectionalButtonState { Offline, Off, On }

    //Customization
    public AudioClip pressSound;
    public Material onMaterial, offMaterial, offlineMaterial;

    //Status variables
    private bool isUpwardsButton = false;
    private DirectionalButtonState currentState = DirectionalButtonState.Offline;
    private VehicleAltitudeController controller;

    private void Awake()
    {
        controller = transform.parent.GetComponent<VehicleAltitudeController>();
        isUpwardsButton = name.ToLower().Contains("up");
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        interacting.GetAudioSource().PlayOneShot(pressSound);

        controller.RegisterButtonPress(isUpwardsButton);
    }

    public void UpdateButtonFromController(VehicleAltitudeController.VerticalMovementType movementType)
    {
        if (movementType == VehicleAltitudeController.VerticalMovementType.Offline)
            SetState(DirectionalButtonState.Offline);
        else if ((isUpwardsButton && movementType == VehicleAltitudeController.VerticalMovementType.GoingUp) ||
                (!isUpwardsButton && movementType == VehicleAltitudeController.VerticalMovementType.GoingDown))
            SetState(DirectionalButtonState.On);
        else
            SetState(DirectionalButtonState.Off);
    }

    private void SetState(DirectionalButtonState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (currentState == DirectionalButtonState.Offline)
            UpdateButtonMaterial(offlineMaterial);
        else if (currentState == DirectionalButtonState.Off)
            UpdateButtonMaterial(offMaterial);
        else //On
            UpdateButtonMaterial(onMaterial);
    }

    private void UpdateButtonMaterial(Material newMaterial)
    {
        GetComponent<MeshRenderer>().sharedMaterial = newMaterial;
    }

    public override string GetInteractionDescription()
    {
        VehicleAltitudeController.VerticalMovementType currentMovement = controller.GetCurrentState();

        if (currentMovement == VehicleAltitudeController.VerticalMovementType.Offline)
            return "Vertical Engines Offline";
        else if(currentMovement == VehicleAltitudeController.VerticalMovementType.Stable)
            return name;
        else //Either currently going up or going down so the next button press will just cancel out the moment and stabilize the altitude
            return "Hold Altitude";
    }
}
