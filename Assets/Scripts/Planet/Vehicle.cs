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

    protected virtual void Start()
    {
        InitializeStaticVariables();

        generalAudio = GetComponent<AudioSource>();
        engineAudio = GetComponents<AudioSource>()[1];
        rBody = GetComponent<Rigidbody>();

        God.god.ManageAudioSource(engineAudio);
    }

    protected virtual void FixedUpdate()
    {
        if (!on)
            return;

        engineAudio.pitch = Mathf.Max(1, rBody.velocity.magnitude / 25.0f);
    }

    public void TogglePower() { SetPower(!on); }

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
        }

        on = turnOn;
    }

    public void UpdateSpeedometer()
    {
        int newSpeed = (int)rBody.velocity.magnitude;
        if (newSpeed != speedometerReading)
        {
            speedometerReading = newSpeed;
            speedometer.text = speedometerReading + " mph";
        }
    }
}
