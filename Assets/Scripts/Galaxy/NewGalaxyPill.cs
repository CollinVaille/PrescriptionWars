using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyPill : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public property that should be used in order to access the unique ID of the pill. Only privately mutable for obvious reasons.
    /// </summary>
    public int ID { get; private set; }

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

    /// <summary>
    /// Public property that should be used both in order to access and mutate the squad that the pill is assigned to.
    /// </summary>
    public NewGalaxySquad assignedSquad
    {
        get => _assignedSquad;
        set
        {
            //Returns if the specified squad is the squad that the pill is already assigned to.
            if (value == _assignedSquad)
                return;

            //Stores the previously assigned squad to a temporary variable.
            NewGalaxySquad previousSquad = _assignedSquad;
            //Sets the pill's assigned squad as the specified value.
            _assignedSquad = value;

            //Removes the pill from its previous squad if necessary.
            if (previousSquad != null && previousSquad.pills.Contains(this))
                previousSquad.pills.Remove(this);

            //Adds the pill to the specified squad's collection of pills if necessary.
            if (_assignedSquad != null && !_assignedSquad.pills.Contains(this))
                _assignedSquad.pills.Add(this);
        }
    }
    /// <summary>
    /// Private holder variable for the squad that the pill is assigned to.
    /// </summary>
    private NewGalaxySquad _assignedSquad = null;

    /// <summary>
    /// Public property that should be used both in order to access and mutate the amount of experience that the pill has. The pill specific override for the base ground unit experience property also updates the experience of the pill's assigned squad if applicable.
    /// </summary>
    public override float experience { get => base.experience; set { base.experience = value; if (assignedSquad != null) assignedSquad.UpdateExperience(); } }

    public NewGalaxyPill(string name, NewGalaxyPillClass pillClass, float experience = 1) : base(name, experience)
    {
        //Checks if the galaxy manager's pill manager is not null and adds the pill to the dictionary of pills being tracked by the pill manager and assigns the pill's ID if so.
        if (NewGalaxyManager.pillManager != null)
            AddToPillManager();
        //If the pill manager is null, the galaxy is assumedly not finished generating so the AddToPillManager function should be added to the functions executed by the galaxy generator once the galaxy has finished generating.
        else
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddToPillManager, 0);

        this.pillClass = pillClass;
    }

    public NewGalaxyPill(NewGalaxyPillData pillData, NewGalaxyPillClass pillClass) : base(pillData.groundUnitData)
    {
        ID = pillData.ID;

        this.pillClass = pillClass;
    }

    /// <summary>
    /// Private method that should be called in order to add the pill to the dictionary of pills being held by the galaxy manager's pill manager and also to assign its ID.
    /// </summary>
    private void AddToPillManager()
    {
        //Returns if the galaxy manager's pill manager that contains all of the pills within the galaxy is null or if the pill has already been added to the pill manager and assigned an ID.
        if (NewGalaxyManager.pillManager == null || ID >= 0)
            return;

        //Adds the pill to the dictionary of pills being held by the pill manager and assigns its ID.
        ID = NewGalaxyManager.pillManager.AddPill(this);
    }
}

[System.Serializable]
public class NewGalaxyPillData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public int ID = -1;
    public int pillClassIndex = -1;

    public NewGalaxyPillData(NewGalaxyPill pill)
    {
        groundUnitData = new NewGalaxyGroundUnitData(pill);
        ID = pill.ID;
        pillClassIndex = pill.pillClass == null ? -1 : pill.pillClass.index;
    }
}
