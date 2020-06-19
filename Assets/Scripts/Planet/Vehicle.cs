﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vehicle : MonoBehaviour
{
    public static bool setUp = false;
    public static Text speedometer, gearIndicator;
    public static int speedometerReading = 0;

    private static void InitializeStaticVariables()
    {
        if (setUp)
            return;

        speedometer = God.god.HUD.Find("Speedometer").GetComponent<Text>();
        gearIndicator = God.god.HUD.Find("Gear Indicator").GetComponent<Text>();
        speedometerReading = 0;

        setUp = true;
    }

    protected bool on = false;
    public AudioClip powerOn, powerOff;

    protected AudioSource generalAudio, engineAudio;
    protected Rigidbody rBody;
    private List<Collider> vehicleColliders;

    [HideInInspector] public float gasPedal = 0.0f; //0.0f = not pressed, 1.0f = full forward, -1.0f = full backward
    [HideInInspector] public float steeringWheel = 0.0f; //0.0f = even/no rotation, 1.0f = full right, -1.0f = full left
    public float thrustPower = 2000, breakPower = 1500, turnStrength = 90;
    public float floorPosition = -0.1f;

    public int[] gears;
    public AudioClip gearShift, gearStuck, slowDown;
    private int gearNumber = 0;
    protected int maxSpeed = 0, currentSpeed = 0;

    private bool tractionControl = false;
    public int traction = 50;

    protected virtual void Start()
    {
        InitializeStaticVariables();

        generalAudio = GetComponent<AudioSource>();
        engineAudio = GetComponents<AudioSource>()[1];
        rBody = GetComponent<Rigidbody>();

        God.god.ManageAudioSource(engineAudio);

        maxSpeed = gears[gearNumber];

        //Initialize vehicle colliders list
        vehicleColliders = new List<Collider>();
        AddCollidersRecursive(transform);

        tractionControl = traction > 0;
    }

    protected virtual void FixedUpdate()
    {
        if (!on)
            return;
        
        //Update speed
        int newSpeed = (int)rBody.velocity.magnitude;
        if (newSpeed + 10 < currentSpeed)
            generalAudio.PlayOneShot(slowDown, (currentSpeed - newSpeed) / 50.0f);
        currentSpeed = newSpeed;

        //Update engine pitch
        engineAudio.pitch = Mathf.Max(1, currentSpeed / 25.0f);

        //Update movement
        if (currentSpeed > maxSpeed) //Slow down if going over speed limit
            rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime,  ForceMode.VelocityChange);
        else //Under speed limit; normal control
        {
            if (gasPedal > 0)
                rBody.AddForce(transform.forward * Time.fixedDeltaTime * thrustPower);
            else if (gasPedal < 0)
                rBody.AddForce(-transform.forward * Time.fixedDeltaTime * breakPower);

            //Stabilize vehicle to zero mph if operator doesn't resist
            if (currentSpeed < 3)
                rBody.AddForce(-rBody.velocity * Time.fixedDeltaTime / 5.0f, ForceMode.VelocityChange);
        }

        //Update traction
        UpdateTraction();
    }

    public void SetPower(bool turnOn)
    {
        if (on == turnOn)
            return;

        if(turnOn) //Turn on
        {
            generalAudio.PlayOneShot(powerOn);

            engineAudio.Play();
        }
        else //Turn off
        {
            generalAudio.PlayOneShot(powerOff);

            engineAudio.pitch = 1.0f;
            engineAudio.Pause();

            gasPedal = 0.0f;
        }

        on = turnOn;
    }

    public void UpdateSpeedometer()
    {
        if (currentSpeed != speedometerReading)
        {
            speedometerReading = currentSpeed;
            speedometer.text = speedometerReading + " mph";
        }
    }

    public void UpdateGearIndicator()
    {
        if(gearNumber == 0)
            gearIndicator.text = "Park";
        else
            gearIndicator.text = "Gear " + gearNumber;
    }

    public bool Grounded(float errorMargin)
    {
        return Physics.Raycast(transform.position + Vector3.one * floorPosition,
            Vector3.down, errorMargin);
    }

    public void ChangeGear(bool goUpOne, bool updateIndicator)
    {
        //Change gear number
        if (goUpOne)
        {
            if(++gearNumber >= gears.Length)
            {
                gearNumber = gears.Length - 1;
                generalAudio.PlayOneShot(gearStuck);
            }
        }
        else if(--gearNumber < 0)
        {
            gearNumber = 0;
            generalAudio.PlayOneShot(gearStuck);
        }

        //Update effects
        maxSpeed = gears[gearNumber];

        generalAudio.PlayOneShot(gearShift);
        if (updateIndicator)
            UpdateGearIndicator();
    }

    private void AddCollidersRecursive(Transform t)
    {
        foreach(Collider c in t.gameObject.GetComponents<Collider>())
            vehicleColliders.Add(c);

        foreach (Transform child in t)
            AddCollidersRecursive(child);
    }

    public void SetPassengerCollisionRecursive(Transform passengerTransform, bool ignoreCollision)
    {
        Collider passengerCollider = passengerTransform.gameObject.GetComponent<Collider>();
        if (passengerCollider)
        {
            foreach (Collider vehicleCollider in vehicleColliders)
                Physics.IgnoreCollision(passengerCollider, vehicleCollider, ignoreCollision);
        }

        foreach (Transform child in passengerTransform)
            SetPassengerCollisionRecursive(child, ignoreCollision);
    }

    private void UpdateTraction ()
    {
        if (tractionControl && currentSpeed > 0)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(rBody.velocity);
            if (localVelocity.x < 0) //Eliminate negative velocity
            {
                localVelocity.x += Time.fixedDeltaTime * traction;

                if (localVelocity.x > 0) //Boundary check
                    localVelocity.x = 0;
                //else if (!drifting && localVelocity.x < -50)
                //    StartCoroutine(DriftingSFX());
            }
            else //Eliminate positive velocity
            {
                localVelocity.x -= Time.fixedDeltaTime * traction;

                if (localVelocity.x < 0) //Boundary check
                    localVelocity.x = 0;
                //else if (!drifting && localVelocity.x > 50)
                //    StartCoroutine(DriftingSFX());
            }
            rBody.velocity = transform.TransformDirection(localVelocity);
        }
    }
}
