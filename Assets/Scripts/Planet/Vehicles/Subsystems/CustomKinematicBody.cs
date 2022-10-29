using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicBody : MonoBehaviour
{
    //Customization
    [HideInInspector] public float airResistance = 4.0f;

    //References
    private Rigidbody rBody;
    private Collider boundaryCollider;
    private Transform spotter;

    //Status variables
    private Vector3 globalVelocity = Vector3.zero;
    private float verticalMaxSpeed = 50.0f;

    private void Start()
    {
        rBody = GetComponent<Rigidbody>();
        boundaryCollider = GetComponent<Collider>();
        spotter = transform.Find("Spotter");
    }

    private void FixedUpdate()
    {
        //Apply velocity to position
        TryToTranslate();

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

    //Try to translate the body per the velocity, but look for a collision and if there is one, stop at point of contact and trigger collision logic and effects
    private void TryToTranslate()
    {
        //If it's not moving, then we don't need to check
        if (globalVelocity.sqrMagnitude < 0.001)
        {
            if (globalVelocity != Vector3.zero)
                globalVelocity = Vector3.zero;

            return;
        }

        Vector3 attemptedTranslation = globalVelocity * Time.fixedDeltaTime;
        //Vector3 newTargetPosition = rBody.position + globalVelocity * Time.fixedDeltaTime;
        Vector3 closestPoint = boundaryCollider.ClosestPoint(rBody.position + (attemptedTranslation * 1000));
        spotter.position = closestPoint;

        if (Physics.Raycast(closestPoint, attemptedTranslation.normalized, out RaycastHit hit, attemptedTranslation.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            //Something in the way... oooo-oooo...
            //Time to deliver some justice!

            Vector3 deltaOfClosestPointAndHit = hit.point - closestPoint;

            //Move up until the very point where we hit
            //rBody.MovePosition(rBody.position + deltaOfClosestPointAndHit);

            //Bounce with some energy loss
            globalVelocity  = -globalVelocity * 0.5f;
        }
        else
        {
            //Nothing in the way, complete the full translation
            rBody.MovePosition(rBody.position + attemptedTranslation);
        }
    }
}
