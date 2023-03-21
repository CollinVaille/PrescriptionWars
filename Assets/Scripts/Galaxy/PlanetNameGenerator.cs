using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlanetNameGenerator
{
    private static string[] planetNames, planetNamePrefixes, planetNameSuffixes;
    private static List<string> planetNamesGenerated = new List<string>();

    /// <summary>
    /// Public static method that returns a string that indicates a unique planet name.
    /// </summary>
    /// <returns></returns>
    public static string GeneratePlanetName()
    {
        if (planetNames == null)
            ReadInPlanetNames();

        //Pick a random name
        string planetName = GetARandomNameThatCouldBeRedundant();

        //Add roman numeral onto end for variation
        if (UnityEngine.Random.Range(0, 30) == 0)
        {
            string[] numerals = new string[] { "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            planetName += " " + numerals[UnityEngine.Random.Range(0, numerals.Length)];
        }

        if (PlanetNameRedundant(planetName))
            return GeneratePlanetName();

        planetNamesGenerated.Add(planetName);
        return planetName;
    }

    private static string GetARandomNameThatCouldBeRedundant()
    {
        if(Random.Range(0, 4) == 0)
            return planetNames[UnityEngine.Random.Range(0, planetNames.Length)];
        else
            return planetNamePrefixes[UnityEngine.Random.Range(0, planetNamePrefixes.Length)] + planetNameSuffixes[UnityEngine.Random.Range(0, planetNameSuffixes.Length)];
    }

    /// <summary>
    /// Method that detects if the planet name is already being used by another planet.
    /// </summary>
    /// <param name="planetName"></param>
    /// <returns></returns>
    private static bool PlanetNameRedundant(string planetName)
    {
        foreach(string planetNameGenerated in planetNamesGenerated)
        {
            if (planetName.Equals(planetNameGenerated))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reads in the text file that stores planet names.
    /// </summary>
    private static void ReadInPlanetNames()
    {
        planetNames = GeneralHelperMethods.GetLinesFromFile("Location Names/Planet Names");
        planetNamePrefixes = GeneralHelperMethods.GetLinesFromFile("Location Names/Planet Name Prefixes");
        planetNameSuffixes = GeneralHelperMethods.GetLinesFromFile("Location Names/Planet Name Suffixes");
    }

    /// <summary>
    /// Public static method that should be called before the galaxy generates to reset the list of planet names that have already been generated.
    /// </summary>
    /// <param name="saveGameData"></param>
    public static void ResetPlanetNamesGenerated(GalaxyData saveGameData)
    {
        planetNamesGenerated = new List<string>();
        if(saveGameData != null)
        {
            foreach(GalaxySolarSystemData solarSystemData in saveGameData.solarSystems)
            {
                foreach(GalaxyPlanetData planetData in solarSystemData.planets)
                {
                    planetNamesGenerated.Add(planetData.planetName);
                }
            }
        }
    }
}
