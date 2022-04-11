using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommsPersonality
{
    public float pitch = Random.Range(0.85f, 1.15f);
    public float uhLikeliness = Random.Range(0.1f, 0.4f); //0.0f = never says uh, 0.5f half the time says uh, 1.0f = says uh everytime
    public Vector2 pauseLengthMultiplier = new Vector2(Random.Range(0.5f, 1.0f), Random.Range(1.0f, 1.5f)); //Average time between words
}
