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
    /// Public property that should be used in order to access the integer value that indicates the ID of the special pill within the special pills dictionary held within the galaxy pill manager that can be accessed via the galaxy manager.
    /// </summary>
    public int ID { get; private set; } = -1;

    /// <summary>
    /// Public property that should be used in order to access the underlying pill of the special pill.
    /// </summary>
    public NewGalaxyPill pill { get; private set; } = null;

    /// <summary>
    /// Public property that should be used by the save data in ordert to access the dictionary that stores the special pill's experience for certain skills. Any other need to deal with the special pill's experience at a skill should be done utilizing the special pill methods and not this property.
    /// </summary>
    public Dictionary<Skill, float> skillExperiences { get; private set; } = null;

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether or not the special pill is assigned a task at the moment.
    /// </summary>
    public bool isBusy { get => task != null; }
    /// <summary>
    /// Public property that should be used both to access and mutate the task that the special pill is assigned to do.
    /// </summary>
    public NewGalaxySpecialPillTask task
    {
        get => _task;
        set
        {
            //Checks if the previously assigned task is the same as the newly assigned task and returns and does nothing if so.
            if (_task == value)
                return;

            //Stores the special pill's previously assigned task in a temporary variable.
            NewGalaxySpecialPillTask previousTask = _task;
            //Mutates the special pill's assigned task to the specified task value.
            _task = value;
            //Checks if the previously assigned task still has the special pill as assigned to it and fixes that if so.
            if (previousTask != null && previousTask.assignedSpecialPill == this)
                previousTask.assignedSpecialPill = null;
            //Checks if the newly assigned task is not tracking the special pill as being assigned to it and fixes that if so.
            if (_task != null && _task.assignedSpecialPill != this)
                _task.assignedSpecialPill = this;
        }
    }
    /// <summary>
    /// Private holder variable for the task that the special pill is doing.
    /// </summary>
    private NewGalaxySpecialPillTask _task = null;

    public NewGalaxySpecialPill(NewGalaxyPill pill)
    {
        //Checks if no pill was provided as an argument and logs an error and returns if so.
        if(pill == null)
        {
            Debug.LogError("Cannot initialize a new special pill without passing in a valid pill as an argument.");
            return;
        }

        this.pill = pill;

        //Checks if the galaxy manager and therefore the pill manager has been initialized and adds the special pill to the dictionary of special pills within the galaxy if so.
        if (NewGalaxyManager.isInitialized)
            AddToPillManager();
        //If the galaxy manager and therefore the pill manager have not yet been initialized, add the AddToPillManager function to the functions that the galaxy generator should execute upon galaxy generation completion.
        else
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddToPillManager, 0);
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
    /// Private method that should be called in order to add the special pill to the list of special pills within the galaxy that exists in the pill manager that can be accessed via the galaxy manager.
    /// </summary>
    private void AddToPillManager()
    {
        //Checks if the pill manager is null or if the ID of the special pill has already been assigned and returns if so.
        if (NewGalaxyManager.pillManager == null || ID >= 0)
            return;

        //Adds the special pill to the dictionary of special pills being held by the pill manager and assigns its ID.
        ID = NewGalaxyManager.pillManager.AddSpecialPill(this);
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
    public int ID = -1;
    public Dictionary<NewGalaxySpecialPill.Skill, float> skillExperiences = null;

    public NewGalaxySpecialPillData(NewGalaxySpecialPill specialPill)
    {
        ID = specialPill.ID;

        skillExperiences = specialPill.skillExperiences;
    }
}