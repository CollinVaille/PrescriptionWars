using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyArmy
{
    public string name;

    public List<GalaxySquad> squads = new List<GalaxySquad>();

    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;
        int totalPills = 0;

        foreach (GalaxySquad squad in squads)
        {
            foreach (GalaxyPill pill in squad.pills)
            {
                totalExperience += pill.GetExperienceLevel();
                totalPills++;
            }
        }

        return totalExperience / totalPills;
    }
}

public class GalaxySquad
{
    public string name;

    public List<GalaxyPill> pills = new List<GalaxyPill>();

    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;

        foreach (GalaxyPill pill in pills)
        {
            totalExperience += pill.GetExperienceLevel();
        }

        return totalExperience / pills.Count;
    }

    public int GetNumberOfPillsWithClass(GalaxyPill.PillClass pillClass)
    {
        int pillsWithClass = 0;

        foreach (GalaxyPill pill in pills)
        {
            if (pill.pillClass == pillClass)
            {
                pillsWithClass++;
            }
        }

        return pillsWithClass;
    }
}

public class GalaxyPill
{
    public enum PillClass
    {
        Assault,
        Riot,
        Officer,
        Medic,
        Flamethrower,
        Rocket
    }
    public PillClass pillClass;

    float experience;

    public string name;

    public int GetExperienceLevel()
    {
        return (int)experience;
    }

    public void SetExperience(float newExperience)
    {
        experience = newExperience;
    }

    public void AddExperience(float experienceToAdd)
    {
        experience += experienceToAdd;
    }
}