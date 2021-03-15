using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillClass
{
    public PillClass(string className, PillClassType classType, PillType initialPillType, GameObject headGear, GameObject bodyGear, GameObject primary, GameObject secondary)
    {
        this.className = className;
        this.classType = classType;
        this.initialPillType = initialPillType;
        this.headGear = headGear;
        this.bodyGear = bodyGear;
        this.primary = primary;
        this.secondary = secondary;
    }

    public string className;
    public PillClassType classType;
    public PillType initialPillType;
    public GameObject headGear;
    public GameObject bodyGear;
    public GameObject primary;
    public GameObject secondary;
    [Tooltip("The probability that a pill of this class will spawn in on the planet view.")] public float probability;
}

public enum PillClassType
{
    Assault,
    Riot,
    Officer,
    Medic,
    Flamethrower,
    Rocket
}

public enum PillType
{
    Player,
    Bot1
}
