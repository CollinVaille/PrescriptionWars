using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySpecialPill
{
    public enum Skill
    {
        Soldiering,
        Generalship
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the name of the special pill.
    /// </summary>
    public string name { get => nameVar; set => nameVar = value; }
    private string nameVar = null;

    /// <summary>
    /// Public property that should be used both to access and mutate the class of the special pill (the class mainly defines their gear).
    /// </summary>
    public PillClass pillClass { get => PillClass.pillClasses.ContainsKey(pillClassName) ? PillClass.pillClasses[pillClassName] : null; set => pillClassName = value != null ? value.className : null; }
    private string pillClassName = null;

    /// <summary>
    /// Public property that should be used both to access and mutate the skin of the special pill.
    /// </summary>
    public Material skin { get => assignedEmpire == null ? null : Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(assignedEmpire.empireCulture.ToString()) + "/" + skinName); set => skinName = value == null ? null : value.name; }
    private string skinName = null;

    /// <summary>
    /// Public property that should be used both to access and mutate the empire that the special pill is assigned to (returns null if the special pill is not assigned to an empire).
    /// </summary>
    public Empire assignedEmpire
    {
        get => assignedEmpireID >= 0 && assignedEmpireID < Empire.empires.Count ? Empire.empires[assignedEmpireID] : null;
        set
        {
            if (assignedEmpire != null && assignedEmpire != value)
            {
                assignedEmpire.RemoveSpecialPill(specialPillID);
                specialPillID = -1;
            }
            assignedEmpireID = value != null ? value.empireID : -1;
            if(value != null)
            {
                specialPillID = assignedEmpire.specialPillsCount;
                assignedEmpire.AddSpecialPill(this);
            }
            CheckExperienceIsWithinBounds();
        }
    }
    /// <summary>
    /// Private int that holds the empire ID of the special pill's assigned empire (-1 if there is no assigned empire).
    /// </summary>
    private int assignedEmpireID = -1;

    /// <summary>
    /// Public int property that should be used to access the special pill's id in their empire's special pills dictionary.
    /// </summary>
    public int specialPillID { get => specialPillIDVar; private set => specialPillIDVar = value; }
    /// <summary>
    /// Private int that holds the special pill's id in their empire's special pills dictionary.
    /// </summary>
    private int specialPillIDVar = -1;

    /// <summary>
    /// Dictionary containing the special pill's experience in each type of skill (null if has no experience with any skill).
    /// </summary>
    private Dictionary<Skill, float> experience = null;

    /// <summary>
    /// Public dictionary property that contains the minimum and maximum amount of experience possible for each skill with all possible bonuses factored in.
    /// </summary>
    public Dictionary<Skill, Vector2Int> experienceBounds
    {
        get
        {
            Dictionary<Skill, Vector2Int> experienceBoundsTemp = new Dictionary<Skill, Vector2Int>(baseExperienceBounds);
            experienceBoundsTemp[Skill.Soldiering] = assignedEmpire != null ? experienceBoundsTemp[Skill.Soldiering] + assignedEmpire.pillExperienceBoundingEffects : experienceBoundsTemp[Skill.Soldiering];
            return experienceBoundsTemp;
        }
    }

    /// <summary>
    /// Dictionary containing the minimum and maximum amount of experience initially possible for each skill without factoring in any bonuses.
    /// </summary>
    private static readonly Dictionary<Skill, Vector2Int> baseExperienceBounds = new Dictionary<Skill, Vector2Int>() { { Skill.Soldiering, GalaxyPill.baseExperienceBounds }, { Skill.Generalship, new Vector2Int(1, 5) } };

    /// <summary>
    /// Public property that should be used to convert this special pill into a regular galaxy pill soldier (Warning: this process will get rid of the special pill's specified skin and all experience except for soldiering).
    /// </summary>
    public GalaxyPill convertedToGalaxyPill
    {
        get
        {
            GalaxyPill galaxyPill = new GalaxyPill(name, pillClassName);
            galaxyPill.experience = GetExperience(Skill.Soldiering);
            return galaxyPill;
        }
    }

    public GalaxySpecialPill(GalaxyPill pillSoldier) : this(pillSoldier.Name, pillSoldier.pillClass.className, pillSoldier.Skin, new Dictionary<Skill, float>() { { Skill.Soldiering, pillSoldier.experience } }, pillSoldier.assignedSquad.assignedArmy.owner)
    {

    }

    public GalaxySpecialPill(string name, string pillClassName, Material skin, Dictionary<Skill, float> experience, Empire assignedEmpire)
    {
        this.name = name;
        this.pillClassName = pillClassName;
        this.skin = skin;
        this.experience = experience;
        this.assignedEmpire = assignedEmpire;
    }

    /// <summary>
    /// Returns a float that indicates how much experience the special pill has in the specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float GetExperience(Skill skill)
    {
        if (experience == null || !experience.ContainsKey(skill))
            return experienceBounds[skill].x;
        return experience[skill];
    }

    /// <summary>
    /// Returns a float that indicates how much experience the special pill has in the specified skill (experience float rounded to an int).
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float GetExperienceLevel(Skill skill)
    {
        return (int)GetExperience(skill);
    }

    /// <summary>
    /// Public void method that should be used to set how much experience the special pill has with the specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experienceAmount"></param>
    public void SetExperience(Skill skill, float experienceAmount)
    {
        //Initializes the experience dictionary if it wasn't already initialized.
        experience = experience == null ? new Dictionary<Skill, float>() : experience;

        //Sets the special pill's experience to the specified amount with the specified skill.
        if (experience.ContainsKey(skill))
            experience[skill] = experienceAmount;
        else
            experience.Add(skill, experienceAmount);

        //Ensures that the special pill's experience with the specified skill stays within the possible experience bounds.
        CheckExperienceIsWithinBounds(new List<Skill>() { skill });
    }

    /// <summary>
    /// Private void method that should be called in order to ensure that the special pill's experience in each skill specified stays within the possible bounds (checks all skills if no specified skills are specified).
    /// </summary>
    private void CheckExperienceIsWithinBounds(List<Skill> skills = null)
    {
        List<Skill> skillsToCheck = skills;
        if(skillsToCheck == null)
        {
            skillsToCheck = new List<Skill>();
            foreach (Skill skill in Enum.GetValues(typeof(Skill)))
            {
                skillsToCheck.Add(skill);
            }
        }
        foreach(Skill skill in skillsToCheck)
        {
            if (GetExperience(skill) < experienceBounds[skill].x)
                experience[skill] = experienceBounds[skill].x;
            else if (GetExperience(skill) > experienceBounds[skill].y)
                experience[skill] = experienceBounds[skill].y;
        }
    }
}
