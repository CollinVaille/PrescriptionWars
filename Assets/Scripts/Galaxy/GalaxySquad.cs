using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySquad: GalaxyGroundUnit
{
    //Constructor for the class.
    public GalaxySquad(string name)
    {
        //Sets the name of the squad to the specified name.
        Name = name;

        //Randomizes the color of the icon that represents the squad.
        RandomizeIconColor();
    }

    public GalaxySquad(string name, Color iconColor)
    {
        //Sets the name of the squad to the specified name.
        Name = name;
        //Sets the color of the icon that represents the squad to the specified color.
        this.iconColor = iconColor;
    }

    public GalaxySquad(string name, Material pillSkin)
    {
        //Sets the name of the squad to the specified name.
        Name = name;
        //Sets the assigned pill skin of the squad to the specified pill skin.
        assignedPillSkin = pillSkin;

        //Randomizes the color of the icon that represents the squad.
        RandomizeIconColor();
    }

    public GalaxySquad(string name, Color iconColor, Material pillSkin)
    {
        //Sets the name of the squad to the specified name.
        Name = name;
        //Sets the color of the icon that represents the squad to the specified color.
        this.iconColor = iconColor;
        //Sets the assigned pill skin of the squad to the specified pill skin.
        assignedPillSkin = pillSkin;
    }

    /// <summary>
    /// Indicates the color of the icon that represents the squad.
    /// </summary>
    public Color iconColor { get => iconColorVar; set => iconColorVar = value; }
    private Color iconColorVar;

    /// <summary>
    /// The material that will be assigned to all pills of this squad (will usually be null and the material assigned to pills in the army class will be used, should only be used for special squads).
    /// </summary>
    public Material assignedPillSkin { get => assignedPillSkinVar; private set => assignedPillSkinVar = value; }
    private Material assignedPillSkinVar = null;

    /// <summary>
    /// List of all of the pills in the squad.
    /// </summary>
    private List<GalaxyPill> pills = null;

    /// <summary>
    /// Returns the length (count) of the list of pills.
    /// </summary>
    public int pillCount { get => pills != null ? pills.Count : 0; }

    /// <summary>
    /// The army that this squad is assigned to.
    /// </summary>
    public GalaxyArmy assignedArmy { get => assignedArmyVar; set => assignedArmyVar = value; }
    private GalaxyArmy assignedArmyVar = null;

    /// <summary>
    /// Returns the exact amount of experience that the average pill in the squad has.
    /// </summary>
    public override float experience
    {
        get
        {
            float totalExperience = 0.0f;

            if(pills != null)
            {
                foreach (GalaxyPill pill in pills)
                {
                    totalExperience += pill.experience;
                }
            }

            return pillCount <= 0 ? 0 : totalExperience / pillCount;
        }
    }
    /// <summary>
    /// Returns the average experience level (experience casted to an int) of pills in the squad.
    /// </summary>
    public override int experienceLevel
    {
        get
        {
            int totalExperienceLevel = 0;

            if(pills != null)
            {
                foreach (GalaxyPill pill in pills)
                {
                    totalExperienceLevel += pill.experienceLevel;
                }
            }

            return pillCount <= 0 ? 0 : totalExperienceLevel / pillCount;
        }
    }

    /// <summary>
    /// Returns the pill that is the leader of the squad.
    /// </summary>
    public GalaxyPill squadLeader
    {
        get
        {
            return squadLeaderIndex >= 0 && pills != null && squadLeaderIndex < pillCount ? GetPillAt(squadLeaderIndex) : null;
        }
        set
        {
            if(value == null)
            {
                squadLeaderIndex = -1;
                return;
            }

            int newSquadLeaderIndex = -1;
            for(int index = 0; index < pillCount; index++)
            {
                if(GetPillAt(index) == value)
                {
                    newSquadLeaderIndex = index;
                    break;
                }
            }
            squadLeaderIndex = newSquadLeaderIndex;
        }
    }
    /// <summary>
    /// The pill that is the leader of the squad (will be null if no pills are in the squad).
    /// </summary>
    private int squadLeaderIndex = -1;

    /// <summary>
    /// Returns the total number of pills in the squad of the specified class.
    /// </summary>
    /// <param name="classType"></param>
    /// <returns></returns>
    public int GetNumberOfPillsOfClassType(PillClassType classType)
    {
        int pillsOfClassType = 0;

        foreach (GalaxyPill pill in pills)
        {
            if (pill.pillClassType == classType)
            {
                pillsOfClassType++;
            }
        }

        return pillsOfClassType;
    }

    /// <summary>
    /// Adds the specified squad to the list of squads.
    /// </summary>
    /// <param name="pill"></param>
    public void AddPill(GalaxyPill pill)
    {
        if (pill == null)
            return;
        if (pills == null)
            pills = new List<GalaxyPill>();

        pills.Add(pill);
        pill.assignedSquad = this;

        //Makes the pill that is being added to the squad the squad leader if there were no pills in the squad before.
        if (squadLeader == null)
            squadLeader = pill;
    }

    /// <summary>
    /// This method inserts the specified pill at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="pill"></param>
    public void InsertPill(int index, GalaxyPill pill)
    {
        GalaxyPill leader = squadLeader;
        if (pill == null)
            return;
        if (pills == null)
            pills = new List<GalaxyPill>();
        leader = squadLeader;

        if (pillCount > 0 && index < pillCount)
        {
            if (index < 0)
                index = 0;

            pills.Insert(index, pill);
            pill.assignedSquad = this;

            squadLeader = leader;
            if (squadLeader == null)
                squadLeader = pill;
        }
        else
            AddPill(pill);
    }

    /// <summary>
    /// Removes the specified squad from the list of squads.
    /// </summary>
    /// <param name="pill"></param>
    public void RemovePill(GalaxyPill pill)
    {
        if (pill == null || pills == null || !pills.Contains(pill))
            return;

        bool pillIsSquadLeader = pill == squadLeader;
        GalaxyPill originalSquadLeader = squadLeader;
        pill.assignedSquad = null;
        pills.Remove(pill);
        //Assigns the squad a new squad leader if the pill that is being removed from the squad was the squad leader.
        if (pillIsSquadLeader)
            squadLeader = pillCount > 0 ? GetPillAt(0) : null;
        else
            squadLeader = originalSquadLeader;
    }

    /// <summary>
    /// Removes the pill at the specified index from the list of pills in the squad.
    /// </summary>
    /// <param name="index"></param>
    public void RemovePillAt(int index)
    {
        if (pills == null || index < 0 || index >= pillCount)
            return;

        GalaxyPill pill = GetPillAt(index);
        bool pillIsSquadLeader = pill == squadLeader;
        GalaxyPill originalSquadLeader = squadLeader;
        pill.assignedSquad = null;
        pills.RemoveAt(index);
        //Assigns the squad a new squad leader if the pill that is being removed from the squad was the squad leader.
        if (pillIsSquadLeader)
            squadLeader = pillCount > 0 ? GetPillAt(0) : null;
        else
            squadLeader = originalSquadLeader;
    }

    /// <summary>
    /// Returns the squad at the specified index in the list of squads.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GalaxyPill GetPillAt(int index)
    {
        return pills != null && index >= 0 && index < pillCount ? pills[index] : null;
    }

    /// <summary>
    /// Randomizes the color of the icon that represents the squad.
    /// </summary>
    private void RandomizeIconColor()
    {
        iconColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
    }
}
