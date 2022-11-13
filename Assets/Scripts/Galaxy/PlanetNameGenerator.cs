using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlanetNameGenerator
{
    private static string[] planetNames;
    private static List<string> planetNamesGenerated = new List<string>();

    /// <summary>
    /// Public static method that returns a string that indicates a unique planet name.
    /// </summary>
    /// <returns></returns>
    public static string GeneratePlanetName()
    {
        if (planetNames == null)
            ReadInPlanetNames();

        string planetName = "";

        if (UnityEngine.Random.Range(0, 6) != 0) //Normal random name
        {
            //Pick a random name
            planetName = planetNames[UnityEngine.Random.Range(0, planetNames.Length)];
        }
        else if (UnityEngine.Random.Range(0, 2) == 0) //Greek letter + astrological/zodiac/birth sign name
        {
            string[] greekLetters = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega"};

            string[] zodiacSigns = new string[] { "Carinae", "Tauri", "Pegasi", "Centauri", "Scuti", "Orionis", "Scorpius",
            "Geminorum"};

            planetName = greekLetters[UnityEngine.Random.Range(0, greekLetters.Length)] + " "
                + zodiacSigns[UnityEngine.Random.Range(0, zodiacSigns.Length)];
        }
        else if (UnityEngine.Random.Range(0, 2) == 0) //Some prefix + major or minor name
        {
            string[] prefixes = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega", "Ursa", "Virgo", "Canis", "Pisces", "Saega", "Polis"};

            if (UnityEngine.Random.Range(0, 2) == 0)
                planetName = prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + " Major";
            else
                planetName = prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + " Minor";
        }
        else //Some guy's name that wanted to shove his dick into history + some edgy sounding fate
        {
            string[] dickShovers = new string[] { "Troy's ", "Turner's ", "Coronado's ", "Septim's ", "Pelagius' ",
            "Haile's ", "Myra's ", "Midas' ", "Calypso's "};

            string[] edgyFates = new string[] { "Fall", "Demise", "Oblivion", "End", "Moon", "Shame", "Hell",
            "Garden", "Domain", "Eyrie", "Madness", "Lost Plane", "Last"};

            planetName = dickShovers[UnityEngine.Random.Range(0, dickShovers.Length)] + " "
                + edgyFates[UnityEngine.Random.Range(0, edgyFates.Length)];
        }

        //Add roman numeral onto end for variation
        if (UnityEngine.Random.Range(0, 5) == 0)
        {
            string[] numerals = new string[] { "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            planetName += " " + numerals[UnityEngine.Random.Range(0, numerals.Length)];
        }

        if (PlanetNameRedundant(planetName))
            return GeneratePlanetName();

        planetNamesGenerated.Add(planetName);
        return planetName;
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
    }

    /// <summary>
    /// Public static method that should be called before the galaxy generates to reset the list of planet name that have already been generated.
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
