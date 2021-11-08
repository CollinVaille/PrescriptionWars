using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExhaustManager : MonoBehaviour
{
    public abstract void UpdateExhaustStream(ParticleSystem exhaustStream, bool backwardThrusting, float currentSpeed, float maxSpeed);
}
