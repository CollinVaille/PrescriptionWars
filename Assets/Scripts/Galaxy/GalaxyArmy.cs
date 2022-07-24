using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyArmy: GalaxyGroundUnit
{
    //Constructor of the galaxy army.
    public GalaxyArmy(string name, int ownerEmpireID) : this(name, ownerEmpireID, Empire.empires[ownerEmpireID].GetRandomPillSkinName(), new ArmyIcon(ArmyIconNamesLoader.armyIconNames[Random.Range(0, ArmyIconNamesLoader.armyIconNames.Length)], new Color((192 / 255.0f), (192 / 255.0f), (192 / 255.0f), 1)))
    {
        
    }

    public GalaxyArmy(string name, int ownerEmpireID, string pillSkinName) : this(name, ownerEmpireID, pillSkinName, new ArmyIcon(ArmyIconNamesLoader.armyIconNames[Random.Range(0, ArmyIconNamesLoader.armyIconNames.Length)], new Color((192 / 255.0f), (192 / 255.0f), (192 / 255.0f), 1)))
    {
        
    }

    public GalaxyArmy(string name, int ownerEmpireID, ArmyIcon armyIcon) : this(name, ownerEmpireID, Empire.empires[ownerEmpireID].GetRandomPillSkinName(), armyIcon)
    {
        
    }

    public GalaxyArmy(string name, int ownerEmpireID, string pillSkinName, ArmyIcon armyIcon)
    {
        //Assigns the name of the army.
        this.name = name;
        //Assigns the owner empire id.
        this.ownerEmpireIDVar = ownerEmpireID;

        //Assigns the material that will be applied to all pills in the army that are not part of a special squad.
        assignedPillSkinNameVar = pillSkinName;
        //Assigns the icon of the army to the specified value.
        this.armyIconVar = armyIcon;
    }

    /// <summary>
    /// Indicates the empire id of the empire that owns this army (the index of the empire in the Empire.empires list of empires).
    /// </summary>
    public int ownerEmpireID { get => ownerEmpireIDVar; }
    /// <summary>
    /// Indicates the empire that owns the army (returns null if there is no empire that actively owns the army).
    /// </summary>
    public Empire owner { get => Empire.empires != null && ownerEmpireID >= 0 && ownerEmpireID < Empire.empires.Count ? Empire.empires[ownerEmpireID] : null; }
    private int ownerEmpireIDVar;

    /// <summary>
    /// The name of the material that will be applied to all pills in the army that are not part of a special squad.
    /// </summary>
    public string assignedPillSkinName
    {
        get
        {
            return assignedPillSkinNameVar;
        }
        set
        {
            assignedPillSkinNameVar = value;
            PillViewsManager.UpdatePillViewsOfArmy(this);
        }
    }
    private string assignedPillSkinNameVar = null;

    /// <summary>
    /// The information needed in order to create the icon that represents the army.
    /// </summary>
    public ArmyIcon armyIcon { get => armyIconVar; }
    private ArmyIcon armyIconVar = null;

    /// <summary>
    /// List of all of the squads in the army.
    /// </summary>
    private List<GalaxySquad> squads = new List<GalaxySquad>();

    /// <summary>
    /// Returns the total number of squads in the list of squads (count).
    /// </summary>
    public int squadCount { get => squads.Count; }

    /// <summary>
    /// Returns the number of squads that the army can contain.
    /// </summary>
    public int squadCountLimit { get => 3; }

    /// <summary>
    /// Returns the exact amount of experience that the average pill in the army has.
    /// </summary>
    public override float experience
    {
        get
        {
            float totalExperience = 0.0f;
            int totalPills = 0;

            foreach (GalaxySquad squad in squads)
            {
                for (int pillIndex = 0; pillIndex < squad.pillCount; pillIndex++)
                {
                    totalExperience += squad.GetPillAt(pillIndex).experience;
                    totalPills++;
                }
            }

            return totalPills <= 0 ? 0 : totalExperience / totalPills;
        }
    }
    /// <summary>
    /// Returns the average experience level of the pills in the army.
    /// </summary>
    public override int experienceLevel
    {
        get
        {
            int totalExperienceLevel = 0;
            int totalPills = 0;

            foreach (GalaxySquad squad in squads)
            {
                for (int pillIndex = 0; pillIndex < squad.pillCount; pillIndex++)
                {
                    totalExperienceLevel += squad.GetPillAt(pillIndex).experienceLevel;
                    totalPills++;
                }
            }

            return totalPills <= 0 ? 0 : totalExperienceLevel / totalPills;
        }
    }

    /// <summary>
    /// Returns the total number of pills in the army (wounded or not).
    /// </summary>
    public int pillsCount
    {
        get
        {
            int totalNumberOfPills = 0;

            foreach(GalaxySquad squad in squads)
            {
                totalNumberOfPills += squad.pillCount;
            }

            return totalNumberOfPills;
        }
    }

    /// <summary>
    /// Adds the specified squad to the list of squads.
    /// </summary>
    /// <param name="squad"></param>
    public void AddSquad(GalaxySquad squad)
    {
        squads.Add(squad);
        squad.assignedArmy = this;
    }

    /// <summary>
    /// Inserts the specified squad at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="squad"></param>
    public void InsertSquad(int index, GalaxySquad squad)
    {
        if (index < squads.Count)
            squads.Insert(index, squad);
        else
            squads.Add(squad);
        squad.assignedArmy = this;
    }

    /// <summary>
    /// Removes the specified squad from the list of squads.
    /// </summary>
    /// <param name="squad"></param>
    public void RemoveSquad(GalaxySquad squad)
    {
        squad.assignedArmy = null;
        squads.Remove(squad);
    }

    /// <summary>
    /// Removes the squad at the specified index from the list of squads in the army.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveSquadAt(int index)
    {
        squads.RemoveAt(index);
    }

    /// <summary>
    /// Returns the squad at the specified index in the list of squads.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GalaxySquad GetSquadAt(int index)
    {
        return squads[index];
    }
}

public class ArmyIcon
{
    public ArmyIcon(string spriteName, Color color)
    {
        this.spriteName = spriteName;
        this.color = color;
    }

    /// <summary>
    /// Indicates the name of the png file in the resources folder that is supposed to be loaded in to represent the army icon.
    /// </summary>
    public string spriteName;
    /// <summary>
    /// Indicates the color of the image compomemt of the army icon.
    /// </summary>
    public Color color;
}