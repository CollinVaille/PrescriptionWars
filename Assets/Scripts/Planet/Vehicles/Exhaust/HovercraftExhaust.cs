using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HovercraftExhaust : ExhaustManager
{
    public Vector2 startSizeYRange = new Vector2(0.5f, 1.5f);
    public Vector2 simSpeedRange = new Vector2(1.0f, 21.0f);

    public override void UpdateExhaustStream(ParticleSystem exhaustStream, bool backwardThrusting, float currentSpeed, float maxSpeed)
    {
        //Update exhaust speed
        ParticleSystem.MainModule mainMod = exhaustStream.main;
        mainMod.startSizeY = Mathf.Lerp(startSizeYRange.x, startSizeYRange.y, currentSpeed / maxSpeed);
        mainMod.simulationSpeed = Mathf.Lerp(simSpeedRange.x, simSpeedRange.y, currentSpeed / maxSpeed);

        //Update whether exhaust is pointing forward/backward
        Vector3 exhaustRotation = exhaustStream.transform.localEulerAngles;
        if (backwardThrusting)
            exhaustRotation.y = 0;
        else
            exhaustRotation.y = 180;
        exhaustStream.transform.localEulerAngles = exhaustRotation;
    }
}
