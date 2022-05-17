using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPill: GalaxyGroundUnit
{
    //Constructor for the class.
    public GalaxyPill(string name, PillClass pillClass)
    {
        //Assigns the pill the specified name.
        this.name = name;
        //Assigns the pill the specified class.
        className = pillClass.className;
        //Assigns the type of the pill to the initial pill type specified in the PillClass of the pill.
        if (PillClass != null)
            pillType = PillClass.initialPillType;
    }

    public GalaxyPill(string name, string className)
    {
        //Assigns the pill the specified name.
        this.name = name;
        //Assigns the pill the specified class.
        this.className = className;
        //Assigns the type of the pill to the initial pill type specified in the PillClass of the pill.
        if (PillClass != null)
            pillType = PillClass.initialPillType;
    }

    /// <summary>
    /// Indicates the amount of experience that the pill has.
    /// </summary>
    private float experience;
    public override float Experience { get => experience; set => experience = value; }
    public override int ExperienceLevel { get => (int)experience; }

    /// <summary>
    /// The class of the pill contains the primary and secondary weapon game objects and the head gear and body gear game objects.
    /// </summary>
    public PillClass PillClass { get => PillClass.pillClasses.ContainsKey(className) ? PillClass.pillClasses[className] : null; set => className = value.className; }
    /// <summary>
    /// Indicates what type of class the class of the pill is (ex: Assault or Officer).
    /// </summary>
    public PillClassType PillClassType { get => PillClass.pillClasses[className].classType; }
    private string className = null;

    /// <summary>
    /// Indicates what type of pill the pill is (Example: Player or Bot1).
    /// </summary>
    public PillType PillType { get => pillType; set => pillType = value; }
    private PillType pillType;

    /// <summary>
    /// The squad that this pill is assigned to.
    /// </summary>
    public GalaxySquad AssignedSquad { get => assignedSquad; set => assignedSquad = value; }
    private GalaxySquad assignedSquad = null;

    /// <summary>
    /// Returns the skin material of the pill that is specified either in the squad (checked first) or the army.
    /// </summary>
    public Material Skin
    {
        get
        {
            //Returns the pill skin assigned to the squad if the squad is a special squad.
            if (assignedSquad.AssignedPillSkin != null)
                return assignedSquad.AssignedPillSkin;
            //Else if the squad is not a special squad then it returns the pill skin assigned to the army.
            return assignedSquad.AssignedArmy.AssignedPillSkin;
        }
    }

    /// <summary>
    /// Indicates whether the secondary weapon game object of the pill should be visible in pill views.
    /// </summary>
    public bool IsSecondaryVisible { get => pillType == PillType.Player; }
}