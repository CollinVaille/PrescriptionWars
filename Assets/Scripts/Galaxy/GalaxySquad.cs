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
        IconColor = iconColor;
    }

    public GalaxySquad(string name, Material pillSkin)
    {
        //Sets the name of the squad to the specified name.
        Name = name;
        //Sets the assigned pill skin of the squad to the specified pill skin.
        AssignedPillSkin = pillSkin;

        //Randomizes the color of the icon that represents the squad.
        RandomizeIconColor();
    }

    public GalaxySquad(string name, Color iconColor, Material pillSkin)
    {
        //Sets the name of the squad to the specified name.
        Name = name;
        //Sets the color of the icon that represents the squad to the specified color.
        IconColor = iconColor;
        //Sets the assigned pill skin of the squad to the specified pill skin.
        AssignedPillSkin = pillSkin;
    }

    /// <summary>
    /// Indicates the color of the icon that represents the squad.
    /// </summary>
    public Color IconColor { get => iconColor; set => iconColor = value; }
    private Color iconColor;

    /// <summary>
    /// The material that will be assigned to all pills of this squad (will usually be null and the material assigned to pills in the army class will be used, should only be used for special squads).
    /// </summary>
    public Material AssignedPillSkin { get => assignedPillSkin; private set => assignedPillSkin = value; }
    private Material assignedPillSkin = null;

    /// <summary>
    /// List of all of the pills in the squad.
    /// </summary>
    private List<GalaxyPill> pills = new List<GalaxyPill>();

    /// <summary>
    /// Returns the length (count) of the list of pills.
    /// </summary>
    public int TotalNumberOfPills { get => pills.Count; }

    /// <summary>
    /// The army that this squad is assigned to.
    /// </summary>
    public GalaxyArmy AssignedArmy { get => assignedArmy; set => assignedArmy = value; }
    private GalaxyArmy assignedArmy = null;

    /// <summary>
    /// Returns the exact amount of experience that the average pill in the squad has.
    /// </summary>
    public override float Experience
    {
        get
        {
            float totalExperience = 0.0f;

            foreach (GalaxyPill pill in pills)
            {
                totalExperience += pill.Experience;
            }

            return pills.Count <= 0 ? 0 : totalExperience / pills.Count;
        }
    }
    /// <summary>
    /// Returns the average experience level (experience casted to an int) of pills in the squad.
    /// </summary>
    public override int ExperienceLevel
    {
        get
        {
            int totalExperienceLevel = 0;

            foreach (GalaxyPill pill in pills)
            {
                totalExperienceLevel += pill.ExperienceLevel;
            }

            return pills.Count <= 0 ? 0 : totalExperienceLevel / pills.Count;
        }
    }

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
            if (pill.PillClassType == classType)
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
        pills.Add(pill);
        pill.AssignedSquad = this;
    }

    /// <summary>
    /// This method inserts the specified pill at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="pill"></param>
    public void InsertPill(int index, GalaxyPill pill)
    {
        if (index < pills.Count)
            pills.Insert(index, pill);
        else
            pills.Add(pill);
        pill.AssignedSquad = this;
    }

    /// <summary>
    /// Removes the specified squad from the list of squads.
    /// </summary>
    /// <param name="pill"></param>
    public void RemovePill(GalaxyPill pill)
    {
        pill.AssignedSquad = null;
        pills.Remove(pill);
    }

    /// <summary>
    /// Removes the pill at the specified index from the list of pills in the squad.
    /// </summary>
    /// <param name="index"></param>
    public void RemovePillAt(int index)
    {
        pills.RemoveAt(index);
    }

    /// <summary>
    /// Returns the squad at the specified index in the list of squads.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GalaxyPill GetPillAt(int index)
    {
        return pills[index];
    }

    /// <summary>
    /// Randomizes the color of the icon that represents the squad.
    /// </summary>
    private void RandomizeIconColor()
    {
        IconColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
    }
}
