using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicBody : MonoBehaviour
{
    //Customization
    [HideInInspector] public float airResistance = 4.0f;

    //Status variables
    private Vector3 globalVelocity = Vector3.zero;
    private Rigidbody rBody;
    private float verticalMaxSpeed = 50.0f;

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        //Apply velocity to position
        rBody.MovePosition(rBody.position + globalVelocity * Time.fixedDeltaTime);

        ApplyAirResistance();
    }

    public void AddForce(Vector3 force, Space space)
    {
        if (space == Space.World)
            globalVelocity += force / rBody.mass;
        else
            globalVelocity += transform.TransformVector(force) / rBody.mass;

        //After changing velocity, make sure we are still within limits
        if(Mathf.Abs(globalVelocity.y) > verticalMaxSpeed)
        {
            if (globalVelocity.y < 0)
                globalVelocity.y = -verticalMaxSpeed;
            else
                globalVelocity.y = verticalMaxSpeed;
        }
    }

    public void SetVerticalMaxSpeed(float verticalMaxSpeed) { this.verticalMaxSpeed = verticalMaxSpeed; }

    public void AddRotation(Vector3 rotation, Space space)
    {
        if (space == Space.World)
            rBody.MoveRotation(Quaternion.Euler(rotation + transform.eulerAngles));
        else
            rBody.MoveRotation(Quaternion.Euler(transform.TransformDirection(rotation + transform.localEulerAngles)));
    }

    private void ApplyAirResistance()
    {
        Vector3 airResistanceVector = rBody.velocity.normalized * airResistance * Time.fixedDeltaTime;

        if (airResistanceVector.sqrMagnitude < globalVelocity.sqrMagnitude)
            globalVelocity -= airResistanceVector;
        else
            globalVelocity = Vector3.zero;
    }
}
