using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Interactable, IVerticalScalerImplement
{
    public enum ElevatorStatus { IdleOnBottom, IdleOnTop, GoingUp, GoingDown }

    public ElevatorStatus elevatorStatus = ElevatorStatus.IdleOnBottom;
    public float cabHeight = 4.5f;
    public float upSpeed = 7.5f, downSpeed = 12.5f;

    private Transform cab, cables, shaft;

    private void Start()
    {
        if (!shaft)
            SetReferences();

        //Make sure elevator is correctly configured for its initial state
        SetCabHeight(elevatorStatus == ElevatorStatus.IdleOnBottom ? 0.0f : GetHeight());
    }

    private void SetReferences()
    {
        cab = transform.Find("Elevator Cab");
        cables = transform.Find("Elevator Cables");
        shaft = transform.Find("Elevator Shaft");
    }

    public override void Interact(Pill interacting)
    {
        base.Interact(interacting);

        if (!interacting)
            return;

        if (WantsToGoUp(interacting))
        {
            if (elevatorStatus == ElevatorStatus.IdleOnBottom)
                StartCoroutine(MoveElevatorCab(true));
        }
        else if (elevatorStatus == ElevatorStatus.IdleOnTop)
            StartCoroutine(MoveElevatorCab(false));
    }

    //Returns true if the pill that called the elevator wants to go up, false if he wants to go down.
    private bool WantsToGoUp(Pill interacting)
    {
        float pillsLocalY = transform.InverseTransformPoint(interacting.transform.position).y;
        return pillsLocalY < shaft.localScale.y / 2.0f;
    }

    private IEnumerator MoveElevatorCab(bool goingUp)
    {
        //Prepare to start moving
        float targetElevation;
        if (goingUp)
        {
            elevatorStatus = ElevatorStatus.GoingUp;
            targetElevation = GetHeight();
        }
        else
        {
            elevatorStatus = ElevatorStatus.GoingDown;
            targetElevation = 0.0f;
        }

        //Loop for moving the cab each frame
        while(true)
        {
            //Check if we're done moving the cab
            float newLocalY = cab.localPosition.y;
            if (goingUp)
            {
                if (newLocalY >= targetElevation)
                    break;
            }
            else if (newLocalY <= targetElevation)
                break;

            //Wait a frame
            yield return null;

            //Move the cab
            if (goingUp)
                SetCabHeight(cab.localPosition.y + upSpeed * Time.deltaTime);
            else
                SetCabHeight(cab.localPosition.y - upSpeed * Time.deltaTime);
        }

        //Finalize transition
        SetCabHeight(targetElevation);
        if (goingUp)
            elevatorStatus = ElevatorStatus.IdleOnTop;
        else
            elevatorStatus = ElevatorStatus.IdleOnBottom;
    }

    private void SetCabHeight(float newLocalCabHeight)
    {
        //Move the cab
        Vector3 localCabPosition = cab.localPosition;
        localCabPosition.y = newLocalCabHeight;
        cab.localPosition = localCabPosition;

        //Adjust the cables to the new cab height...
        
        //Compute the new y-axis scale for the cables
        Vector3 localCableScale = Vector3.one;
        float cableCeiling = GetHeight();
        localCableScale.y = cableCeiling - localCabPosition.y;

        //If the scale is approaching 0, just disable the game object
        if (Mathf.Abs(localCableScale.y) < 0.02f)
            cables.gameObject.SetActive(false);
        else //Else, apply the scale and then set the y position too
        {
            if (!cables.gameObject.activeSelf)
                cables.gameObject.SetActive(true);

            //Apply the scale
            cables.localScale = localCableScale;

            //Set the cables height
            Vector3 localCablePosition = Vector3.zero;
            localCablePosition.y = 4.5f + (cableCeiling + localCabPosition.y) / 2.0f;
            cables.localPosition = localCablePosition;
        }
    }

    public override bool OverrideTriggerDescription() { return true; }

    protected override string GetInteractionVerb() { return "Call"; }

    public void ScaleToHeight(float heightToScaleTo)
    {
        if (!shaft)
            SetReferences();

        Vector3 scalersScale = Vector3.one;
        scalersScale.y = 1.0f + (heightToScaleTo / cabHeight);
        shaft.localScale = scalersScale;
    }

    public float GetHeight()
    {
        if (!shaft)
            SetReferences();

        return cabHeight * (shaft.localScale.y - 1.0f);
    }
}
