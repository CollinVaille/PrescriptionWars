using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPillManager
{
    /// <summary>
    /// Private dictionary that holds all of the pills that exist within the galaxy.
    /// </summary>
    private Dictionary<int, NewGalaxyPill> pills = null;
    /// <summary>
    /// Public property that should be used in order to access the save data from the pills dictionary.
    /// </summary>
    public Dictionary<int, NewGalaxyPillData> pillsSaveData
    {
        get
        {
            //Checks if the pills dictionary is null and returns null if so.
            if (pills == null)
                return null;

            //Initializes a new pills save data dictionary.
            Dictionary<int, NewGalaxyPillData> pillsSaveDataDictionary = new Dictionary<int, NewGalaxyPillData>();
            //Loops through each pill ID in the pills dictionary and adds its save data to the pills save data dictionary.
            foreach (int pillID in pills.Keys)
                pillsSaveDataDictionary.Add(pillID, new NewGalaxyPillData(pills[pillID]));
            //Returns the pills save data dictionary.
            return pillsSaveDataDictionary;
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the amount of pills that have been added to the pills dictionary of all of the pills within the galaxy.
    /// </summary>
    public int pillsCount { get; private set; } = 0;

    /// <summary>
    /// Private dictionary that holds all of the special pills that exist within the galaxy.
    /// </summary>
    private Dictionary<int, NewGalaxySpecialPill> specialPills = null;
    /// <summary>
    /// Public property that should be used in order to access the amount of special pills that have been added to the special pills dictionary of all of the special pills within the galaxy.
    /// </summary>
    public int specialPillsCount { get; private set; } = 0;

    /// <summary>
    /// Private dictionary that holds all of the special pill tasks that exist within the galaxy.
    /// </summary>
    private Dictionary<int, NewGalaxySpecialPillTask> specialPillTasks = null;
    /// <summary>
    /// Public property that should be used in order to access the save data from the special pill tasks dictionary.
    /// </summary>
    public Dictionary<int, NewGalaxySpecialPillTaskData> specialPillTasksSaveData
    {
        get
        {
            //Checks if the special pill tasks dictionary is null and returns if so.
            if (specialPillTasks == null)
                return null;

            //Initializes a new special pill tasks save data dictionary.
            Dictionary<int, NewGalaxySpecialPillTaskData> specialPillTasksSaveDataDictionary = new Dictionary<int, NewGalaxySpecialPillTaskData>();
            //Loops through each special pill task ID in the special pill tasks dictionary and adds its save data to the special pill tasks save data dictionary.
            foreach (int specialPillTaskID in specialPillTasks.Keys)
                specialPillTasksSaveDataDictionary.Add(specialPillTaskID, new NewGalaxySpecialPillTaskData(specialPillTasks[specialPillTaskID]));
            //Returns the special pill tasks save data dictionary.
            return specialPillTasksSaveDataDictionary;
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates the amount of special pill tasks that have been added to the special pill tasks dictionary of all of the special pill tasks within the galaxy.
    /// </summary>
    public int specialPillTasksCount { get; private set; } = 0;

    /// <summary>
    /// Constructor that should be called via the galaxy generator in order to properly initialize the pill manager with the needed values.
    /// </summary>
    /// <param name="pillManagerData"></param>
    public GalaxyPillManager(GalaxyPillManagerData pillManagerData = null)
    {
        //Pill manager save data exists.
        if(pillManagerData != null && pillManagerData.pills != null)
        {
            pills = new Dictionary<int, NewGalaxyPill>();
            foreach (int pillID in pillManagerData.pills.Keys)
                pills.Add(pillID, new NewGalaxyPill(pillManagerData.pills[pillID], null));
            pillsCount = pillManagerData.pillsCount;
        }
    }

    /// <summary>
    /// Public method that should be used in order to add a pill to the dictionary of pills within the galaxy.
    /// </summary>
    /// <param name="pill"></param>
    /// <returns></returns>
    public int AddPill(NewGalaxyPill pill)
    {
        //Returns if the specified pill is null.
        if (pill == null)
            return -1;

        //Checks if the pills dictionary is null and initializes it if so.
        if (pills == null)
            pills = new Dictionary<int, NewGalaxyPill>();

        //Adds the specified pill into the pills dictionary at the pillsCount key.
        int pillID = pillsCount;
        pills.Add(pillID, pill);

        //Increments the pills count and returns the newly assigned ID of the pill that was just added.
        pillsCount++;
        return pillID;
    }

    /// <summary>
    /// Public method that should be used in order to remove a pill from the dictionary of pills within the galaxy via ID.
    /// </summary>
    /// <param name="pillID"></param>
    public void RemovePill(int pillID)
    {
        if (pills != null && pills.ContainsKey(pillID))
        {
            if (pills[pillID].isSpecialPill)
                RemoveSpecialPill(pills[pillID].specialPill.ID);
            pills.Remove(pillID);
        }
    }

    /// <summary>
    /// Public method that should be used in order to access the pill at the specified pillID within the dictionary of pills within the galaxy.
    /// </summary>
    /// <param name="pillID"></param>
    /// <returns></returns>
    public NewGalaxyPill GetPill(int pillID)
    {
        if (pills != null && pills.ContainsKey(pillID))
            return pills[pillID];
        return null;
    }

    /// <summary>
    /// Public method that should be used in order to add a special pill to the dictionary of special pills within the galaxy.
    /// </summary>
    /// <param name="specialPill"></param>
    /// <returns></returns>
    public int AddSpecialPill(NewGalaxySpecialPill specialPill)
    {
        //Returns if the specified special pill is null.
        if (specialPill == null)
            return -1;

        //Checks if the special pills dictionary is null and initializes it if so.
        if (specialPills == null)
            specialPills = new Dictionary<int, NewGalaxySpecialPill>();

        //Adds the specified special pill into the special pills dictionary at the specialPillsCount key.
        int specialPillID = specialPillsCount;
        specialPills.Add(specialPillID, specialPill);

        //Increments the special pills count and returns the newly assigned ID of the special pill that was just added.
        specialPillsCount++;
        return specialPillID;
    }

    /// <summary>
    /// Public method that should be used in order to remove a special pill from the dictionary of special pills within the galaxy via ID.
    /// </summary>
    /// <param name="specialPillID"></param>
    public void RemoveSpecialPill(int specialPillID)
    {
        if (specialPills != null && specialPills.ContainsKey(specialPillID))
        {
            specialPills[specialPillID].pill.OnSpecialPillRemoved();
            specialPills.Remove(specialPillID);
        }
    }

    /// <summary>
    /// Public method that should be used in order to access the special pill at the specified specialPillID within the dictionary of special pills within the galaxy.
    /// </summary>
    /// <param name="specialPillID"></param>
    /// <returns></returns>
    public NewGalaxySpecialPill GetSpecialPill(int specialPillID)
    {
        if (specialPills != null && specialPills.ContainsKey(specialPillID))
            return specialPills[specialPillID];
        return null;
    }

    /// <summary>
    /// Public method that should be used in order to add a special pill task to the dictionary of special pill tasks within the galaxy. The integer value returned is the special pill task's assigned ID.
    /// </summary>
    /// <param name="specialPillTask"></param>
    /// <returns></returns>
    public int AddSpecialPillTask(NewGalaxySpecialPillTask specialPillTask)
    {
        //Returns if the specified special pill task is null.
        if (specialPillTask == null)
            return -1;

        //Checks if the special pill tasks dictionary is null and initializes it if so.
        if (specialPillTasks == null)
            specialPillTasks = new Dictionary<int, NewGalaxySpecialPillTask>();

        //Adds the specified special pill task into the special pill tasks dictionary at the specialPillTasksCount key.
        int specialPillTaskID = specialPillTasksCount;
        specialPillTasks.Add(specialPillTaskID, specialPillTask);

        //Increments the special pill tasks count and returns the newly assigned ID of the special pill task that was just added.
        specialPillTasksCount++;
        return specialPillTaskID;
    }

    /// <summary>
    /// Public method that should be used in order to remove a special pill task from the dictionary of special pill tasks within the galaxy via ID.
    /// </summary>
    /// <param name="specialPillTaskID"></param>
    public void RemoveSpecialPillTask(int specialPillTaskID)
    {
        if(specialPillTasks != null && specialPillTasks.ContainsKey(specialPillTaskID))
        {
            specialPillTasks[specialPillTaskID].assignedSpecialPill = null;
            specialPillTasks.Remove(specialPillTaskID);
        }
    }

    /// <summary>
    /// Public method that should be used in order to access the special pill task at the specified specialPillTaskID within the dictionary of special pill tasks within the galaxy.
    /// </summary>
    /// <param name="specialPillTaskID"></param>
    /// <returns></returns>
    public NewGalaxySpecialPillTask GetSpecialPillTask(int specialPillTaskID)
    {
        if (specialPillTasks != null && specialPillTasks.ContainsKey(specialPillTaskID))
            return specialPillTasks[specialPillTaskID];
        return null;
    }
}

[System.Serializable]
public class GalaxyPillManagerData
{
    public Dictionary<int, NewGalaxyPillData> pills = null;
    public int pillsCount = -1;

    public GalaxyPillManagerData(GalaxyPillManager pillManager)
    {
        pills = pillManager.pillsSaveData;
        pillsCount = pillManager.pillsCount;
    }
}