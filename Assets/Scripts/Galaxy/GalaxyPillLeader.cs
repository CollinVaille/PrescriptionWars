using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPillLeader
{
    public enum Skill
    {
        Soldiering,
        Generalship
    }

    /// <summary>
    /// Public property that should be used both to access and mutate the name of the galaxy pill leader.
    /// </summary>
    public string name { get => nameVar; set => nameVar = value; }
    private string nameVar = string.Empty;

    /// <summary>
    /// Public property that should be used both to access and mutate the empire that the galaxy pill leader is assigned to (returns null if the galaxy pill leader is not assigned to an empire).
    /// </summary>
    public Empire assignedEmpire
    {
        get => assignedEmpireID >= 0 && assignedEmpireID < Empire.empires.Count ? Empire.empires[assignedEmpireID] : null;
        set
        {
            assignedEmpireID = value != null ? value.EmpireID : -1;
            CheckExperienceIsWithinBounds();
        }
    }
    /// <summary>
    /// Private int that holds the empire ID of the pill leader's assigned empire (-1 if there is no assigned empire).
    /// </summary>
    private int assignedEmpireID = -1;

    /// <summary>
    /// Dictionary containing the pill leader's experience in each type of skill (null if has no experience with any skill).
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
            experienceBoundsTemp[Skill.Soldiering] = experienceBoundsTemp[Skill.Soldiering] + assignedEmpire.pillExperienceBoundingEffects;
            return experienceBoundsTemp;
        }
    }

    /// <summary>
    /// Dictionary containing the minimum and maximum amount of experience initially possible for each skill without factoring in any bonuses.
    /// </summary>
    private static readonly Dictionary<Skill, Vector2Int> baseExperienceBounds = new Dictionary<Skill, Vector2Int>() { { Skill.Soldiering, GalaxyPill.baseExperienceBounds }, { Skill.Generalship, new Vector2Int(1, 5) } };

    public GalaxyPillLeader(GalaxyPill pillSoldier) : this(pillSoldier.Name, new Dictionary<Skill, float>() { { Skill.Soldiering, pillSoldier.experience } }, pillSoldier.assignedSquad.assignedArmy.owner)
    {

    }

    public GalaxyPillLeader(string name, Dictionary<Skill, float> experience, Empire assignedEmpire)
    {
        this.name = name;
        this.experience = experience;
        this.assignedEmpire = assignedEmpire;
    }

    /// <summary>
    /// Returns a float that indicates how much experience the pill leader has in the specified skill.
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
    /// Returns a float that indicates how much experience the pill leader has in the specified skill (experience float rounded to an int).
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float GetExperienceLevel(Skill skill)
    {
        return (int)GetExperience(skill);
    }

    /// <summary>
    /// Public void method that should be used to set how much experience the pill leader has with the specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experienceAmount"></param>
    public void SetExperience(Skill skill, float experienceAmount)
    {
        //Initializes the experience dictionary if it wasn't already initialized.
        experience = experience == null ? new Dictionary<Skill, float>() : experience;

        //Sets the galaxy pill leader's experience to the specified amount with the specified skill.
        if (experience.ContainsKey(skill))
            experience[skill] = experienceAmount;
        else
            experience.Add(skill, experienceAmount);

        //Ensures that the galaxy pill leader's experience with the specified skill stays within the possible experience bounds.
        CheckExperienceIsWithinBounds(new List<Skill>() { skill });
    }

    /// <summary>
    /// Private void method that should be called in order to ensure that the pill leader's experience in each skill specified stays within the possible bounds (checks all skills if no specified skills are specified).
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
