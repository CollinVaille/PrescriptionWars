using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
