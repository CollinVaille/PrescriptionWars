using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyArmy
{
    public string name;

    public List<GalaxySquad> squads;

    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;
        int totalPills = 0;

        foreach (GalaxySquad squad in squads)
        {
            foreach (GalaxyPill pill in squad.pills)
            {
                totalExperience += pill.experience;
                totalPills++;
            }
        }

        return totalExperience / totalPills;
    }
}

public class GalaxySquad
{
    public string name;

    public List<GalaxyPill> pills;

    public float GetExperienceLevel()
    {
        float totalExperience = 0.0f;

        foreach (GalaxyPill pill in pills)
        {
            totalExperience += pill.experience;
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
        Assault
    }
    public PillClass pillClass;

    public float experience;
}
