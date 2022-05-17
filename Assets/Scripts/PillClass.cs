using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillClass
{
    public static Dictionary<string, PillClass> pillClasses = new Dictionary<string, PillClass>();

    public PillClass(string className, PillClassType classType, PillType initialPillType, string headGearName, string bodyGearName, string primaryName, string secondaryName)
    {
        this.className = className;
        this.classType = classType;
        this.initialPillType = initialPillType;
        this.headGearName = headGearName;
        this.bodyGearName = bodyGearName;
        this.primaryName = primaryName;
        this.secondaryName = secondaryName;
    }

    public string className;
    public PillClassType classType;
    public PillType initialPillType;
    public string headGearName;
    public string bodyGearName;
    public string primaryName;
    public string secondaryName;
    [Tooltip("The probability that a pill of this class will spawn in on the planet view.")] public float probability;

    public GameObject headGear { get => Resources.Load<GameObject>("Planet/Gear/Head Gear/" + headGearName); }
    public GameObject bodyGear { get => Resources.Load<GameObject>("Planet/Gear/Body Gear/" + bodyGearName); }
    public GameObject primary { get => Resources.Load<GameObject>("Planet/Items/" + primaryName); }
    public GameObject secondary { get => Resources.Load<GameObject>("Planet/Items/" + secondaryName); }
    public Sprite iconSprite { get => Resources.Load<Sprite>("General/Pill Class Icons/" + classType); }
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
