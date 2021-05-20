using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPill
{
    //Constructor for the class.
    public GalaxyPill(string name, PillClass pillClass)
    {
        //Assigns the pill the specified name.
        this.name = name;
        //Assigns the pill the specified class.
        this.pillClass = pillClass;
        //Assigns the type of the pill to the initial pill type specified in the PillClass of the pill.
        if (pillClass != null)
            pillType = pillClass.initialPillType;
    }

    //Indicates the amount of experience that the pill has.
    private float experience;
    public float Experience
    {
        get
        {
            return experience;
        }
        set
        {
            experience = value;
        }
    }
    public int ExperienceLevel
    {
        get
        {
            return (int)experience;
        }
    }

    //Indicates the name of the pill.
    private string name;
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    //The class of the pill contains the primary and secondary weapon game objects and the head gear and body gear game objects.
    private PillClass pillClass;
    public PillClass PillClass
    {
        get
        {
            return pillClass;
        }
        set
        {
            pillClass = value;
        }
    }
    //Indicates what type of class the class of the pill is (ex: Assault or Officer).
    public PillClassType PillClassType
    {
        get
        {
            return pillClass.classType;
        }
    }

    //Indicates what type of pill the pill is (Example: Player or Bot1).
    private PillType pillType;
    public PillType PillType
    {
        get
        {
            return pillType;
        }
        set
        {
            pillType = value;
        }
    }

    //The squad that this pill is assigned to.
    private GalaxySquad assignedSquad = null;
    public GalaxySquad AssignedSquad
    {
        get
        {
            return assignedSquad;
        }
        set
        {
            assignedSquad = value;
        }
    }

    //Returns the skin material of the pill that is specified either in the squad (checked first) or the army.
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

    //Indicates whether the secondary weapon game object of the pill should be visible in pill views.
    public bool IsSecondaryVisible
    {
        get
        {
            return pillType == PillType.Player;
        }
    }
}