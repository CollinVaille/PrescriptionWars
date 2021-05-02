using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyArmy
{
    //Constructor of the galaxy army.
    public GalaxyArmy(string name, int ownerEmpireID)
    {
        //Assigns the name of the army.
        this.name = name;
        //Assigns the owner empire id.
        this.ownerEmpireID = ownerEmpireID;

        //Assigns the material that will be applied to all pills in the army that are not part of a special squad.
        assignedPillSkin = Empire.empires[this.ownerEmpireID].GetRandomPillSkin();
    }

    public GalaxyArmy(string name, int ownerEmpireID, Material pillSkin)
    {
        //Assigns the name of the army.
        this.name = name;
        //Assigns the owner empire id.
        this.ownerEmpireID = ownerEmpireID;

        //Assigns the material that will be applied to all pills in the army that are not part of a special squad.
        assignedPillSkin = pillSkin;
    }

    //Indicates the empire id of the empire that owns this army (the index of the empire in the Empire.empires list of empires).
    private int ownerEmpireID;

    //Indicates the name of the army.
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

    //The material that will be applied to all pills in the army that are not part of a special squad.
    private Material assignedPillSkin = null;
    public Material AssignedPillSkin
    {
        get
        {
            return assignedPillSkin;
        }
        set
        {
            assignedPillSkin = value;
            PillViewsManager.UpdatePillViewsOfArmy(this);
        }
    }

    //List of all of the squads in the army.
    private List<GalaxySquad> squads = new List<GalaxySquad>();

    //Returns the number of squads in the list of squads (count).
    public int TotalNumberOfSquads
    {
        get
        {
            return squads.Count;
        }
    }

    //Returns the average experience level of the army.
    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;
        int totalPills = 0;

        foreach (GalaxySquad squad in squads)
        {
            for (int pillIndex = 0; pillIndex < squad.TotalNumberOfPills; pillIndex++)
            {
                totalExperience += squad.GetPillAt(pillIndex).ExperienceLevel;
                totalPills++;
            }
        }

        return totalExperience / totalPills;
    }

    //Returns the total number of pills in the army.
    public int TotalNumberOfPills
    {
        get
        {
            int totalNumberOfPills = 0;

            foreach(GalaxySquad squad in squads)
            {
                totalNumberOfPills += squad.TotalNumberOfPills;
            }

            return totalNumberOfPills;
        }
    }

    //Adds the specified squad to the list of squads.
    public void AddSquad(GalaxySquad squad)
    {
        squads.Add(squad);
        squad.AssignedArmy = this;
    }

    //This method inserts the specified squad at the specified index.
    public void InsertSquad(int index, GalaxySquad squad)
    {
        squads.Insert(index, squad);
    }

    //Removes the specified squad from the list of squads.
    public void RemoveSquad(GalaxySquad squad)
    {
        squad.AssignedArmy = null;
        squads.Remove(squad);
    }

    //Removes the squad at the specified index from the list of squads in the army.
    public void RemoveSquadAt(int index)
    {
        squads.RemoveAt(index);
    }

    //Returns the squad at the specified index in the list of squads.
    public GalaxySquad GetSquadAt(int index)
    {
        return squads[index];
    }
}





public class GalaxySquad
{
    //Constructor for the class.
    public GalaxySquad(string name)
    {
        this.name = name;
    }

    public GalaxySquad(string name, Material pillSkin)
    {
        this.name = name;
        AssignedPillSkin = pillSkin;
    }

    //Indicates the name of the squad.
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

    //The material that will be assigned to all pills of this squad (will usually be null and the material assigned to pills in the army class will be used, should only be used for special squads).
    public Material AssignedPillSkin { get; }

    //List of all of the pills in the squad.
    private List<GalaxyPill> pills = new List<GalaxyPill>();

    //Returns the length (count) of the list of pills.
    public int TotalNumberOfPills
    {
        get
        {
            return pills.Count;
        }
    }

    //The army that this squad is assigned to.
    private GalaxyArmy assignedArmy = null;
    public GalaxyArmy AssignedArmy
    {
        get
        {
            return assignedArmy;
        }
        set
        {
            assignedArmy = value;
        }
    }

    //Returns the average experience level (experience casted to an int) of the army.
    public float ExperienceLevel
    {
        get
        {
            float totalExperience = 0.0f;

            foreach (GalaxyPill pill in pills)
            {
                totalExperience += pill.ExperienceLevel;
            }

            return totalExperience / pills.Count;
        }
    }

    //Returns the total number of pills in the squad of the specified class.
    public int GetNumberOfPillsOfClassType(PillClassType classType)
    {
        int pillsOfClassType = 0;

        foreach (GalaxyPill pill in pills)
        {
            if (pill.PillClassType == classType)
            {
                pillsOfClassType++;
            }
        }

        return pillsOfClassType;
    }

    //Adds the specified squad to the list of squads.
    public void AddPill(GalaxyPill pill)
    {
        pills.Add(pill);
        pill.AssignedSquad = this;
    }

    //This method inserts the specified pill at the specified index.
    public void InsertPill(int index, GalaxyPill pill)
    {
        pills.Insert(index, pill);
    }

    //Removes the specified squad from the list of squads.
    public void RemovePill(GalaxyPill pill)
    {
        pill.AssignedSquad = null;
        pills.Remove(pill);
    }

    //Removes the pill at the specified index from the list of pills in the squad.
    public void RemovePillAt(int index)
    {
        pills.RemoveAt(index);
    }

    //Returns the squad at the specified index in the list of squads.
    public GalaxyPill GetPillAt(int index)
    {
        return pills[index];
    }
}





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
        if(pillClass != null)
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
            if(assignedSquad.AssignedPillSkin != null)
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