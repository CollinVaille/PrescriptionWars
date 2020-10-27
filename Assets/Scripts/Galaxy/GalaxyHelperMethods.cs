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
        Empire.empires[empireID].credits += amount;
    }

    public static void AddCreditsPerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].baseCreditsPerTurn += amount;
    }

    public static void AddPrescriptionsToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].prescriptions += amount;
    }

    public static void AddPresciptionsPerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].basePresciptionsPerTurn += amount;
    }

    public static void AddScienceToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].science += amount;
    }

    public static void AddSciencePerTurnToEmpire(int amount, int empireID)
    {
        Empire.empires[empireID].baseSciencePerTurn += amount;
    }

    public static void ConquerPlanet(int planetID, int conquerorID)
    {
        if(GalaxyManager.planets[planetID].ownerID != conquerorID)
        {
            GalaxyManager.planets[planetID].ConquerPlanet(conquerorID);
        }
    }
}
