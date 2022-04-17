using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hovercraft : Vehicle
{
    public Transform[] fans;
    public float power = 1.0f, airCushion = 0.5f;

    public ParticleSystem hoverCloud;

    //Physics layers that the hovercraft should consider ground to propel on top off
    private int surfaceLayerMask = ~0; //Not no layers = all layers

    private float distanceToGround = 0;

    private AudioSource engineAudio;

    protected override void Start()
    {
        base.Start();

        engineAudio = GetComponents<AudioSource>()[1];
        God.god.ManageAudioSource(engineAudio);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        //Call this even when off so that we can properly deactivate engine effects
        UpdateEngineEffects();

        if (!on)
            return;


        UpdateMovement();

        UpdateRotation();

        UpdateFans();

        //Update engine pitch
        engineAudio.pitch = Mathf.Max(1, currentSpeed / 25.0f);
    }

    public override void SetPower(bool turnOn)
    {
        base.SetPower(turnOn);

        if(turnOn)
        {
            hoverCloud.Play();
            engineAudio.Play();
        }
        else
        {
            hoverCloud.Stop();
            engineAudio.pitch = 1.0f;
            engineAudio.Pause();
        }

        foreach (Engine engine in engines)
            engine.SetPower(turnOn);
    }

    private void UpdateMovement()
    {
        //Update movement
        if (currentSpeed > currentMaxSpeed) //Slow down if going over speed limit
            rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime, ForceMode.VelocityChange);
        else //Under speed limit; normal control
        {
            if (gasPedal > 0)
                rBody.AddForce(transform.forward * Time.fixedDeltaTime * GetThrustPower());
            else if (gasPedal < 0)
                rBody.AddForce(-transform.forward * Time.fixedDeltaTime * brakePower);

            //Stabilize vehicle to zero mph if operator doesn't resist
            if (currentSpeed < 3)
                rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime / 5.0f, ForceMode.VelocityChange);
        }
    }

    private void UpdateRotation()
    {
        //Steering wheel rotates vehicle
        if (steeringWheel != 0)
            transform.Rotate(0.0f, steeringWheel * Time.fixedDeltaTime * turnStrength, 0.0f);

        //Get back fan ground point
        Vector3 backPoint = fans[fans.Length - 1].position;
        if (Hovercast(backPoint, out RaycastHit hit))
            backPoint = hit.point;

        //Get front fan ground point
        Vector3 frontPoint = fans[0].position;
        if (Hovercast(frontPoint, out hit))
            frontPoint = hit.point;

        //Determine rotation of speeder based on back/front ground points
        //But only if hovercraft is close to ground and the angle isn't too harsh
        if(distanceToGround < 7 && Mathf.Abs(frontPoint.y - backPoint.y) < 5)
            transform.rotation = Quaternion.LookRotation(frontPoint - backPoint);

        //Adjust rotation so that vehicle rotates sideways when driver turns wheel
        //Rotation is sharper the faster you are going
        transform.Rotate(0.0f, 0.0f, steeringWheel * currentSpeed * -0.25f, Space.Self);

        //Apply limits on rotation so vehicle doesn't go vertical on sharp inclines
        //Vector3 localEulerAngles = transform.localEulerAngles;
        //localEulerAngles.x = ClampAngle(localEulerAngles.x, -45, 45);
        //transform.localEulerAngles = localEulerAngles;
    }

    private void UpdateFans()
    {
        //Rotate fans
        foreach (Transform fan in fans)
            fan.Rotate(0.0f, 0.0f, 1000 * Time.fixedDeltaTime * power, Space.Self);

        //Determine distance to ground
        distanceToGround = airCushion;
        //string hitName = " (None)";
        Vector3 forcePosition = transform.position;
        forcePosition.y += floorPosition;
        if (Hovercast(forcePosition, out RaycastHit hit))
        {
            distanceToGround = Mathf.Max(Mathf.Sqrt(hit.distance), airCushion);
            //hitName = " " + hit.collider.name;
        }

        //gearIndicator.text = distanceToGround.ToString("F2") + hitName;

        //Apply hover force (but only if needed)
        if(rBody.velocity.y < 10)
            rBody.AddForce(Time.fixedDeltaTime * power * Vector3.up / distanceToGround, ForceMode.Force);
    }

    //Casts ray downwards, detecting anything that should be "hovered over" (including water!)
    //Returns true if anything was hit
    private bool Hovercast(Vector3 origin, out RaycastHit hit)
    {
        if (Physics.Raycast(origin, Vector3.down, out hit, 99999, surfaceLayerMask, QueryTriggerInteraction.Ignore))
        {
            //Any hit below sea level in planets with sea gets replaced with equivalent sea level point
            //(How floating on water is achieved)
            if (Planet.planet.hasOcean)
            {
                //The exception is that hovercraft submerged can't use fans to push off water
                float seaLevel = Planet.planet.oceanTransform.position.y;
                if (hit.point.y < seaLevel && transform.position.y >= seaLevel)
                {
                    //Replace hit point
                    Vector3 newHitPoint = origin;
                    newHitPoint.y = seaLevel;
                    hit.point = newHitPoint;

                    //Replace hit distance
                    hit.distance = origin.y - seaLevel;

                    //Nothing else is used so no need to replace it...
                }
            }

            return true;
        }
        else
            return false;
    }

    //Provided from unity answers: https://answers.unity.com/questions/141775/limit-local-rotation.html
    /* private float ClampAngle(float angle, float min, float max)
    {
        //If angle in the critic region...
        if (angle < 90 || angle > 270)
        {
            //Convert all angles to -180..+180

            if (angle > 180)
                angle -= 360;

            if (max > 180)
                max -= 360;

            if (min > 180)
                min -= 360;
        }

        angle = Mathf.Clamp(angle, min, max);

        //If angle negative, convert to 0..360
        if (angle < 0)
            angle += 360;

        return angle;
     }  */
}
