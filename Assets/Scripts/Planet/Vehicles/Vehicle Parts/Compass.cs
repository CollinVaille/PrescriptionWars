using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : VehiclePart
{
    //References
    public Transform pointsTo;

    //References
    private Transform rotatingPiece;

    //Status variables
    private bool completedOffProcedure = false;
    private float timeSinceOn = 0.0f;
    private Quaternion startRotation = Quaternion.identity;

    protected override void Start()
    {
        base.Start();

        rotatingPiece = transform.Find("Rotating Piece");
    }

    private void FixedUpdate()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (working && belongsTo && belongsTo.PoweredOn() && pointsTo)
            PointToTarget();
        else
            FallToDefaultPosition();
    }

    private void PointToTarget()
    {
        //Point towards target
        if (pointsTo)
            rotatingPiece.LookAt(pointsTo);
        else
            rotatingPiece.LookAt(Vector3.zero);

        //Correct local x and z axis to be unchanged (only want local y axis to change)
        Vector3 armRotation = rotatingPiece.localEulerAngles;
        armRotation.x = 0.0f;
        armRotation.z = 0.0f;
        rotatingPiece.localEulerAngles = armRotation;

        if (completedOffProcedure)
        {
            completedOffProcedure = false;
            timeSinceOn = 0.0f;
        }
    }

    private void FallToDefaultPosition()
    {
        if (completedOffProcedure)
            return;

        if (timeSinceOn <= 0.0f)
            startRotation = rotatingPiece.localRotation;
        
        timeSinceOn += Time.fixedDeltaTime;

        if (timeSinceOn >= 1.0f)
        {
            completedOffProcedure = true;
            rotatingPiece.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }
        else
            rotatingPiece.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0.0f, 180.0f, 0.0f), timeSinceOn);
    }
}
