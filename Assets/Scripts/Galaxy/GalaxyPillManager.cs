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
    public int pillsCount { get; private set; } = -1;

    /// <summary>
    /// Constructor that should be called via the galaxy generator in order to properly initialize the pill manager with the needed values.
    /// </summary>
    /// <param name="pillManagerData"></param>
    public GalaxyPillManager(GalaxyPillManagerData pillManagerData = null)
    {
        //No pill manager save data exists.
        if(pillManagerData == null)
        {
            pills = null;
            pillsCount = 0;
        }
        //Pill manager save data exists.
        if(pillManagerData != null)
        {
            pills = null;
            if (pillManagerData.pills != null)
            {
                pills = new Dictionary<int, NewGalaxyPill>();
                foreach (int pillID in pillManagerData.pills.Keys)
                    pills.Add(pillID, new NewGalaxyPill(pillManagerData.pills[pillID], null));
            }
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
            pills.Remove(pillID);
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