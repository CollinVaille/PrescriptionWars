using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicBody : MonoBehaviour
{
    //Customization
    [HideInInspector] public float airResistance = 4.0f;
    public LayerMask obstacleLayers;
    [Tooltip("The local y height of sea level when the ship is floating.")]
    public float localSeaLevel = 0.0f;

    //References
    private Rigidbody rBody;
    private Collider boundaryCollider;

    //Status variables
    private Vector3 globalVelocity = Vector3.zero;
    private float verticalMaxSpeed = 50.0f, absoluteMaxSpeed = 150.0f;
    private RaycastHit[] possibleHits;
    private RaycastHit hit;

    private void Start()
    {
        //Set references
        rBody = GetComponent<Rigidbody>();
        boundaryCollider = GetComponent<Collider>();

        //Initialize status
        possibleHits = new RaycastHit[100];
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
        ApplyLimitsToVelocity();
    }

    private void ApplyLimitsToVelocity()
    {
        //Vertical max speed
        if (Mathf.Abs(globalVelocity.y) > verticalMaxSpeed)
        {
            if (globalVelocity.y < 0)
                globalVelocity.y = -verticalMaxSpeed;
            else
                globalVelocity.y = verticalMaxSpeed;
        }

        //Absolute max speed
        if (globalVelocity.magnitude > absoluteMaxSpeed)
            globalVelocity = globalVelocity.normalized * absoluteMaxSpeed;

        //Don't push downward when at water level (this is how we are giving the illusion of buoyancy)
        if (rBody.position.y + localSeaLevel < Planet.seaLevel + 0.1f && globalVelocity.y < 0.0f)
            globalVelocity.y = 0.0f;
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

    //Try to translate the body per the velocity, but look for a collision and if there is one, abort translation and trigger collision logic and effects
    private void TryToTranslate()
    {
        //If it's not moving, then we don't need to check
        if (globalVelocity.sqrMagnitude < 0.001)
        {
            if (globalVelocity != Vector3.zero)
                globalVelocity = Vector3.zero;

            return;
        }

        Vector3 translationToAttempt = globalVelocity * Time.fixedDeltaTime;
        if (DidWeRunIntoSomething(translationToAttempt, rBody.rotation)) //This will set the member variable called "hit" as a side effect
        {
            //Something in the way, abort translation and handle collision
            HandleCollision();
        }
        else if(rBody.position.y + localSeaLevel < Planet.seaLevel)
        {
            //Ocean in the way
            Vector3 newPos = rBody.position;
            newPos.y = Planet.seaLevel - localSeaLevel + 0.05f;
            rBody.position = newPos;

            //Make sure we don't continue to try and go down
            if(globalVelocity.y < 0.0f)
                globalVelocity.y = 0.0f;
        }
        else
        {
            //Nothing in the way, complete the full translation
            rBody.MovePosition(rBody.position + translationToAttempt);
        }
    }

    //When trying to translate or rotate, this function can be used to determine if we'll run into anything in the process
    private bool DidWeRunIntoSomething(Vector3 translationToAttempt, Quaternion rotationToAttempt, float distanceToCheck = -1.0f)
    {
        if (distanceToCheck < 0)
            distanceToCheck = translationToAttempt.magnitude;

        Vector3 startPosition = rBody.position + translationToAttempt.normalized;

        int numberOfHits = Physics.BoxCastNonAlloc(startPosition, boundaryCollider.bounds.extents * 0.5f, translationToAttempt.normalized, possibleHits, rotationToAttempt,
            distanceToCheck, obstacleLayers.value, QueryTriggerInteraction.Ignore);

        if (numberOfHits == 0)
            return false; //hit nothing at all
        else
        {
            for(int hitIndex = 0; hitIndex < numberOfHits; hitIndex++)
            {
                if(!possibleHits[hitIndex].collider.transform.IsChildOf(transform))
                {
                    hit = possibleHits[hitIndex];
                    return true; //hit something that is not part of the ship
                }
            }

            return false; //just "hit" something that is part of the ship. so no collision
        }
    }

    private void HandleCollision()
    {
        ChangeVelocityAndPositionBasedOnCollision();

        //Debug.Log(hit.collider.transform.root.name);
        //Debug.Log(hit.collider.name);

        //Move up until the very point where we hit
        //rBody.MovePosition(rBody.position + deltaOfClosestPointAndHit * 0.2f);

        //Bounce with some energy loss
        //globalVelocity = Vector3.zero;
        //globalVelocity  = -globalVelocity * 0.5f;
        //globalVelocity -= (deltaOfClosestPointAndHit / Time.fixedDeltaTime) * 0.5f;
    }

    private void ChangeVelocityAndPositionBasedOnCollision()
    {
        //Get some info
        Vector3 closestPoint = boundaryCollider.ClosestPoint(hit.point);
        Vector3 deltaOfClosestPointAndHit = hit.point - closestPoint;

        //Calculate tentative new velocity
        globalVelocity = (globalVelocity.normalized - deltaOfClosestPointAndHit.normalized) * globalVelocity.magnitude;
        ApplyLimitsToVelocity();

        //If moving slow, gently decelerate to stop
        if (globalVelocity.magnitude < 20)
        {
            if (globalVelocity.magnitude < 1)
                globalVelocity = Vector3.zero;
            else
                globalVelocity = globalVelocity.normalized * 0.5f;
        }

        //If hit something below, zap any further downward movement to prevent the stupid sinking problem
        if (deltaOfClosestPointAndHit.y < 0.0f && globalVelocity.y < 0.0f)
            globalVelocity.y = 0.0f;

        //Update position per velocity
        rBody.MovePosition(rBody.position + (globalVelocity * Time.fixedDeltaTime));
    }
}
