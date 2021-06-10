﻿using System.Collections;
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
    public int OwnerEmpireID
    {
        get
        {
            return ownerEmpireID;
        }
    }

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

    //Returns the number of squads that the army can contain.
    public int NumberOfSquadsLimits
    {
        get
        {
            return 5;
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