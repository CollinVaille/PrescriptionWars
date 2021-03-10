using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillClass
{
    public string className;
    public GameObject primary;
    public GameObject secondary;
    [Tooltip("The material that will be applied to the capsule game object.")] public Material skin;
    public GameObject bodyGear;
    public GameObject headGear;
    [Tooltip("The probability that a pill of this class will spawn in on the planet view.")] public float probability;

    public PillClass(string className, GameObject primary, GameObject secondary, Material skin, GameObject bodyGear, GameObject headGear)
    {
        this.className = className;
        this.primary = primary;
        this.secondary = secondary;
        this.skin = skin;
        this.bodyGear = bodyGear;
        this.headGear = headGear;
    }
}
