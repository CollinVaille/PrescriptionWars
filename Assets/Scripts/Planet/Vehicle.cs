using System.Collections;
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

    [HideInInspector] public float gasPedal = 0.0f; //0.0f = not pressed, 1.0f = full forward, -1.0f = full backward
    [HideInInspector] public float steeringWheel = 0.0f; //0.0f = even/no rotation, 1.0f = full right, -1.0f = full left
    public float thrustPower = 2000, breakPower = 1500, turnStrength = 90;
    public float floorPosition = -0.1f;

    public int[] gears;
    public AudioClip gearShift, gearStuck, slowDown;
    private int gearNumber = 1;
    private int maxSpeed = 0, currentSpeed = 0;

    protected virtual void Start()
    {
        InitializeStaticVariables();

        generalAudio = GetComponent<AudioSource>();
        engineAudio = GetComponents<AudioSource>()[1];
        rBody = GetComponent<Rigidbody>();

        God.god.ManageAudioSource(engineAudio);

        maxSpeed = gears[gearNumber - 1];
    }

    protected virtual void FixedUpdate()
    {
        if (!on)
            return;
        
        //Update speed
        int newSpeed = (int)rBody.velocity.magnitude;
        if (newSpeed + 10 < currentSpeed)
            generalAudio.PlayOneShot(slowDown);
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

    public void UpdateGearIndicator() { gearIndicator.text = "Gear " + gearNumber; }

    public bool Grounded()
    {
        return Physics.Raycast(transform.position + Vector3.one * floorPosition,
            Vector3.down, 2.0f);
    }

    public void ChangeGear(bool goUpOne, bool updateIndicator)
    {
        //Change gear number
        if (goUpOne)
        {
            if(++gearNumber > gears.Length)
            {
                gearNumber = gears.Length;
                generalAudio.PlayOneShot(gearStuck);
            }
        }
        else if(--gearNumber < 1)
        {
            gearNumber = 1;
            generalAudio.PlayOneShot(gearStuck);
        }

        //Update effects
        maxSpeed = gears[gearNumber - 1];

        generalAudio.PlayOneShot(gearShift);
        if (updateIndicator)
            UpdateGearIndicator();
    }
}
