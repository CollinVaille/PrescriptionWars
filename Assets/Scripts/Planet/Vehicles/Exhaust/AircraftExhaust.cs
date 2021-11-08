using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftExhaust : ExhaustManager
{
    public Vector2 startSizeXZRange = new Vector2(3.0f, 6.0f);
    public Vector2 startSizeYRange = new Vector2(1.0f, 3.0f);
    public Vector2 lifetimeRange = new Vector2(0.1f, 0.5f);

    public override void UpdateExhaustStream(ParticleSystem exhaustStream, bool backwardThrusting, float currentSpeed, float maxSpeed)
    {
        //Update exhaust speed
        ParticleSystem.MainModule mainMod = exhaustStream.main;
        mainMod.startSizeX = Mathf.Lerp(startSizeXZRange.x, startSizeXZRange.y, currentSpeed / maxSpeed);
        mainMod.startSizeZ = Mathf.Lerp(startSizeXZRange.x, startSizeXZRange.y, currentSpeed / maxSpeed);
        mainMod.startSizeY = Mathf.Lerp(startSizeYRange.x, startSizeYRange.y, currentSpeed / maxSpeed);

        ParticleSystem.MinMaxCurve lifetime = mainMod.startLifetime;
        lifetime.constantMin = Mathf.Lerp(lifetimeRange.x, lifetimeRange.y, currentSpeed / maxSpeed);
        lifetime.constantMax = Mathf.Lerp(lifetimeRange.x, lifetimeRange.y, currentSpeed / maxSpeed) + 0.05f;
        mainMod.startLifetime = lifetime;

        //Update whether exhaust is pointing forward/backward
        Vector3 exhaustRotation = exhaustStream.transform.localEulerAngles;
        if (backwardThrusting)
            exhaustRotation.y = 0;
        else
            exhaustRotation.y = 180;
        exhaustStream.transform.localEulerAngles = exhaustRotation;
    }
}