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
        AssignedPillSkin = Empire.empires[this.ownerEmpireID].GetRandomPillSkin();
    }

    //Indicates the empire id of the empire that owns this army (the index of the empire in the Empire.empires list of empires).
    private int ownerEmpireID;

    //Indicates the name of the army.
    private string name;

    //The material that will be applied to all pills in the army that are not part of a special squad.
    public Material AssignedPillSkin { get; }

    //List of all of the squads in the army.
    private List<GalaxySquad> squads = new List<GalaxySquad>();

    //Returns the average experience level of the army.
    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;
        int totalPills = 0;

        foreach (GalaxySquad squad in squads)
        {
            for (int pillIndex = 0; pillIndex < squad.GetNumberOfPills(); pillIndex++)
            {
                totalExperience += squad.GetPillAt(pillIndex).GetExperienceLevel();
                totalPills++;
            }
        }

        return totalExperience / totalPills;
    }

    //Adds the specified squad to the list of squads.
    public void AddSquad(GalaxySquad squad)
    {
        squads.Add(squad);
        squad.SetAssignedArmy(this);
    }

    //This method inserts the specified squad at the specified index.
    public void InsertSquad(int index, GalaxySquad squad)
    {
        squads.Insert(index, squad);
    }

    //Removes the specified squad from the list of squads.
    public void RemoveSquad(GalaxySquad squad)
    {
        squad.SetAssignedArmy(null);
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

    //Returns the total number of squads in the list of squads.
    public int GetNumberOfSquads()
    {
        return squads.Count;
    }

    //This method returns the name of the army.
    public string GetName()
    {
        return name;
    }

    //This method sets the name of the army to the specified name.
    public void SetName(string name)
    {
        this.name = name;
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

    //The material that will be assigned to all pills of this squad (will usually be null and the material assigned to pills in the army class will be used, should only be used for special squads).
    public Material AssignedPillSkin { get; }

    //List of all of the pills in the squad.
    private List<GalaxyPill> pills = new List<GalaxyPill>();

    //The army that this squad is assigned to.
    private GalaxyArmy assignedArmy = null;

    //Returns the average experience level of the squad.
    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;

        foreach (GalaxyPill pill in pills)
        {
            totalExperience += pill.GetExperienceLevel();
        }

        return totalExperience / pills.Count;
    }

    //Returns the total number of pills in the squad of the specified class.
    public int GetNumberOfPillsWithClass(GalaxyPill.PillClass pillClass)
    {
        int pillsWithClass = 0;

        foreach (GalaxyPill pill in pills)
        {
            if (pill.GetPillClass() == pillClass)
            {
                pillsWithClass++;
            }
        }

        return pillsWithClass;
    }

    //This method sets the variable that indicates what army that this squad is assigned to equal to the specified army.
    public void SetAssignedArmy(GalaxyArmy army)
    {
        assignedArmy = army;
    }

    //Returns the army that the squad is assigned to.
    public GalaxyArmy GetAssignedArmy()
    {
        return assignedArmy;
    }

    //Adds the specified squad to the list of squads.
    public void AddPill(GalaxyPill pill)
    {
        pills.Add(pill);
        pill.SetAssignedSquad(this);
    }

    //This method inserts the specified pill at the specified index.
    public void InsertPill(int index, GalaxyPill pill)
    {
        pills.Insert(index, pill);
    }

    //Removes the specified squad from the list of squads.
    public void RemovePill(GalaxyPill pill)
    {
        pill.SetAssignedSquad(null);
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

    //Returns the total number of squads in the list of squads.
    public int GetNumberOfPills()
    {
        return pills.Count;
    }

    //This method returns the name of the squad.
    public string GetName()
    {
        return name;
    }

    //This method sets the name of the squad to the specified name.
    public void SetName(string name)
    {
        this.name = name;
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
    }

    public enum PillClass
    {
        Assault,
        Riot,
        Officer,
        Medic,
        Flamethrower,
        Rocket
    }
    //Indicates the class of the pill.
    private PillClass pillClass;

    //Indicates the amount of experience that the pill has.
    private float experience;

    //Indicates the name of the pill.
    private string name;

    //The squad that this pill is assigned to.
    private GalaxySquad assignedSquad;

    public Material Skin
    {
        get
        {
            //Returns the pill skin assigned to the squad if the squad is a special squad.
            if(assignedSquad.AssignedPillSkin != null)
                return assignedSquad.AssignedPillSkin;
            //Else if the squad is not a special squad then it returns the pill skin assigned to the army.
            return assignedSquad.GetAssignedArmy().AssignedPillSkin;
        }
    }

    //This method returns the experience level of the pill.
    public int GetExperienceLevel()
    {
        return (int)experience;
    }

    //This method sets the amount of experience that the pill has to the specified amount.
    public void SetExperience(float newExperience)
    {
        experience = newExperience;
    }

    //This method adds the specified amount of experience to the pill.
    public void AddExperience(float experienceToAdd)
    {
        experience += experienceToAdd;
    }

    //This method returns the name of the pill.
    public string GetName()
    {
        return name;
    }

    //This method sets the name of the pill to the specified name.
    public void SetName(string name)
    {
        this.name = name;
    }

    //This method assigns the pill to the specified squad.
    public void SetAssignedSquad(GalaxySquad squad)
    {
        assignedSquad = squad;
    }

    //Returns the class of the pill.
    public PillClass GetPillClass()
    {
        return pillClass;
    }
}