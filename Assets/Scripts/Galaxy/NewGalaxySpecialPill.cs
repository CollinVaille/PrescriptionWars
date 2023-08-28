using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxySpecialPill
{
    public enum Skill
    {
        Generalship
    }

    /// <summary>
    /// Public property that should be used in order to access the underlying pill of the special pill.
    /// </summary>
    public NewGalaxyPill pill { get; private set; } = null;

    /// <summary>
    /// Public property that should be used by the save data in ordert to access the dictionary that stores the special pill's experience for certain skills. Any other need to deal with the special pill's experience at a skill should be done utilizing the special pill methods and not this property.
    /// </summary>
    public Dictionary<Skill, float> skillExperiences { get; private set; } = null;

    public NewGalaxySpecialPill(NewGalaxyPill pill)
    {
        //Checks if no pill was provided as an argument and logs an error and returns if so.
        if(pill == null)
        {
            Debug.LogError("Cannot initialize a new special pill without passing in a valid pill as an argument.");
            return;
        }

        this.pill = pill;
    }

    public NewGalaxySpecialPill(NewGalaxyPill pill, NewGalaxySpecialPillData specialPillData)
    {
        //Checks if no pill was provided as an argument and logs an error and returns if so.
        if (pill == null)
        {
            Debug.LogError("Cannot initialize a new special pill without passing in a valid pill as an argument.");
            return;
        }

        //Checks if no special pill save data was provided as an argument and logs an error and returns if so.
        if(specialPillData == null)
        {
            Debug.LogError("Cannot initialize a new special pill from save data if the specified special pill save data is null.");
            return;
        }

        this.pill = pill;
        this.skillExperiences = specialPillData.skillExperiences;
    }

    /// <summary>
    /// Public method that should be called in order to obtain the float value that indicates the special pill's experience at the specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public float GetExperienceAtSkill(Skill skill)
    {
        //Checks if the skill experiences dictionary is null or if the special pill doesn't have any experience yet in the specified skill and returns an experience of 1 if so.
        if (skillExperiences == null || !skillExperiences.ContainsKey(skill))
            return 1;

        //Returns the special pill's experience at the specified skill.
        return skillExperiences[skill];
    }

    /// <summary>
    /// Public method that should be called in order to obtain the integer value that indicates the special pill's experience level at the specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public int GetExperienceLevelAtSkill(Skill skill)
    {
        return (int)GetExperienceAtSkill(skill);
    }

    /// <summary>
    /// Public method that should be called in order to set the special pill's experience at a specified skill to a specified float value.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experience"></param>
    public void SetExperienceAtSkill(Skill skill, float experience)
    {
        //Checks if the skills dictionary is null and initializes it if so.
        if (skillExperiences == null)
            skillExperiences = new Dictionary<Skill, float>();

        //Sets the special pill's experience at the specified skill to the specified float value in the skill experiences dictionary.
        if (skillExperiences.ContainsKey(skill))
            skillExperiences[skill] = experience;
        else
            skillExperiences.Add(skill, experience);
    }

    /// <summary>
    /// Public method that should be called in order to set the special pill's experience level at a specified skill to a specified integer value.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experienceLevel"></param>
    public void SetExperienceLevelAtSkill(Skill skill, int experienceLevel)
    {
        SetExperienceAtSkill(skill, experienceLevel);
    }

    /// <summary>
    /// Public method that should be called in order to add a specified amount of experience to the special pill's total experience for a specified skill.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="experience"></param>
    public void AddExperienceAtSkill(Skill skill, float experience)
    {
        //Checks if the skills experiences dictionary is null and initializes it if so.
        if (skillExperiences == null)
            skillExperiences = new Dictionary<Skill, float>();

        //Adds the specified amount of experience to the special pill's total experience for the specified skill.
        if (skillExperiences.ContainsKey(skill))
            skillExperiences[skill] += experience;
        else
            skillExperiences.Add(skill, 1 + experience);
    }
}

[System.Serializable]
public class NewGalaxySpecialPillData
{
    public Dictionary<NewGalaxySpecialPill.Skill, float> skillExperiences = null;

    public NewGalaxySpecialPillData(NewGalaxySpecialPill specialPill)
    {
        skillExperiences = specialPill.skillExperiences;
    }
}