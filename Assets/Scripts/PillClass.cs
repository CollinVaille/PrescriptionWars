using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillClass
{
    public string className;
    public GameObject primary;
    public GameObject secondary;
    public GameObject bodyGear;
    public GameObject headGear;
    [Tooltip("The probability that a pill of this class will spawn in on the planet view.")] public float probability;

    public PillClass(string className, GameObject primary, GameObject secondary, GameObject bodyGear, GameObject headGear)
    {
        this.className = className;
        this.primary = primary;
        this.secondary = secondary;
        this.bodyGear = bodyGear;
        this.headGear = headGear;
    }
}
