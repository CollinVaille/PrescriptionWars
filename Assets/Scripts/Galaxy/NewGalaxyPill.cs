using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyPill : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public property that should be used to access and modify which class the pill belongs to.
    /// </summary>
    public NewGalaxyPillClass pillClass
    {
        get => _pillClass;
        set
        {
            _pillClass = value;
        }
    }
    /// <summary>
    /// Private holder variable for which class the pill belongs to.
    /// </summary>
    private NewGalaxyPillClass _pillClass = null;

    public NewGalaxyPill(string name, NewGalaxyPillClass pillClass, float experience = 1) : base(name, experience)
    {
        this.pillClass = pillClass;
    }

    public NewGalaxyPill(NewGalaxyPillData pillData, NewGalaxyPillClass pillClass) : base(pillData.groundUnitData)
    {
        this.pillClass = pillClass;
    }
}

[System.Serializable]
public class NewGalaxyPillData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public int pillClassIndex = -1;

    public NewGalaxyPillData(NewGalaxyPill pill)
    {
        groundUnitData = new NewGalaxyGroundUnitData(pill);
        pillClassIndex = pill.pillClass == null ? -1 : pill.pillClass.index;
    }
}
