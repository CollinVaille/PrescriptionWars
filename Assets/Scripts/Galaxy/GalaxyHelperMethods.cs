﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyHelperMethods : MonoBehaviour
{
    public static List<float> GetScrollbarValueNumbers(int num)
    {
        List<float> valueNumbers = new List<float>();

        for (int x = 0; x < num; x++)
        {
            valueNumbers.Add(1.0f / (num - 1) * x);
        }

        return valueNumbers;
    }

    public static void AddCreditsToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].Credits += amount;
    }

    public static void AddCreditsPerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].BaseCreditsPerTurn += amount;
    }

    public static void AddPrescriptionsToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].Prescriptions += amount;
    }

    public static void AddPresciptionsPerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].BasePrescriptionsPerTurn += amount;
    }

    public static void AddScienceToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].Science += amount;
    }

    public static void AddSciencePerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].BaseSciencePerTurn += amount;
    }

    public static void ConquerPlanet(int planetID, int conquerorID)
    {
        if(GalaxyManager.planets[planetID].OwnerID != conquerorID)
        {
            GalaxyManager.planets[planetID].ConquerPlanet(conquerorID);
        }
    }

    public static GalaxyViewBehaviour GetParentGalaxyView(Transform transform)
    {
        GalaxyViewBehaviour currentParentGalaxyView = null;

        Transform nextTransformToCheck = transform.parent;
        while (currentParentGalaxyView == null)
        {
            if (nextTransformToCheck.GetComponent<GalaxyViewBehaviour>() != null)
            {
                currentParentGalaxyView = nextTransformToCheck.GetComponent<GalaxyViewBehaviour>();
                break;
            }

            if (nextTransformToCheck.parent != null)
                nextTransformToCheck = nextTransformToCheck.parent;
            else
                break;
        }

        return currentParentGalaxyView;
    }
}

[System.Serializable]
public enum DefaultOrCustomOption
{
    Default,
    Custom
}