using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CityNameGenerator
{
    private static List<string> cityNamesGenerated = new List<string>();

    /// <summary>
    /// Public static method that returns a string that indicates a unique city name.
    /// </summary>
    /// <returns></returns>
    public static string GenerateCityName(string planetName, Planet.Biome biome, bool capitalCity)
    {
        //if (planetNames == null)
        //    ReadInPlanetNames();

        //Pick a random name
        string cityName = GetARandomNameThatCouldBeRedundant(planetName, biome, capitalCity);

        if (CityNameRedundant(cityName))
            return GenerateCityName(planetName, biome, capitalCity);

        cityNamesGenerated.Add(cityName);
        return cityName;
    }

    /// <summary>
    /// Method that detects if the city name is already being used by another city.
    /// </summary>
    /// <param name="cityName"></param>
    /// <returns></returns>
    private static bool CityNameRedundant(string cityName)
    {
        foreach (string planetNameGenerated in cityNamesGenerated)
        {
            if (cityName.Equals(planetNameGenerated))
                return true;
        }

        return false;
    }

    private static string GetARandomNameThatCouldBeRedundant(string planetName, Planet.Biome biome, bool capitalCity)
    {
        string cityName = GetCityBaseName(planetName, biome, capitalCity);

        //Add prefixes and suffixes to city name
        GetCityPrefixesAndSuffixes(planetName, biome, capitalCity, out string prefix, out string suffix);
        cityName = MergeNamePieces(prefix, cityName);
        cityName = MergeNamePieces(cityName, suffix);

        return cityName;
    }

    private static string GetCityBaseName(string planetName, Planet.Biome biome, bool capitalCity)
    {
        string cityName = planetName;

        if(biome == Planet.Biome.Swamp)
        {
            if(Random.Range(0.0f, 1.0f) < 0.9f)
                cityName = FormNameWithLexicons(new List<string>() { "Wagga", "DiDi", "Uuka", "Wooka", "Yauli", "Uga", "Yadda", "Waka",
                    "Tata", "Zidi", "Eika", "Wi", "Jawa", "Eika", "Weigga", "Gieiga", "Eeiita", "Weiika", "Yykieka", "Wakka-waka", "Eiiga",
                    "Weika", "Ooka", "Ahga", "Eiita", "Yiekah", "Yah", "Yugha", "Lakha", "Tata", "Xita", "Uiyga"},
                    Random.Range(0.0f, 1.0f));
        }

        return cityName;
    }

    private static string FormNameWithLexicons(List<string> lexicons, float repeatProbability)
    {
        //Create name that's a random mash up of some lexicons
        string nameToReturn = lexicons[Random.Range(0, lexicons.Count)];
        string previousLexicon = "";
        for (int x = 1; x <= Random.Range(2, 4); x++)
        {
            //Determine new lexicon
            string newLexicon;
            if (!string.IsNullOrEmpty(previousLexicon) && Random.Range(0.0f, 1.0f) < repeatProbability)
                newLexicon = previousLexicon;
            else
                newLexicon = lexicons[Random.Range(0, lexicons.Count)];

            //Add lexicon to name
            if (Random.Range(0, 2) == 0)
                nameToReturn += " " + newLexicon;
            else
                nameToReturn += newLexicon.ToLower();

            //Get ready for next iteration
            previousLexicon = newLexicon;
        }

        return nameToReturn;
    }

    private static void GetCityPrefixesAndSuffixes(string planetName, Planet.Biome biome, bool capitalCity, out string prefix, out string suffix)
    {
        List<string> possibleSuffixes = new List<string>() { };
        List<string> possiblePrefixes = new List<string>() { };

        if (capitalCity)
        {
            if (planetName.Length < 8)
            {
                possiblePrefixes.AddRange(new string[] { "", "The Great " });
                possibleSuffixes.AddRange(new string[] { " Cosmopolis", " Metropolis", " Metropolitan Area" });
            }
            else
                possibleSuffixes.AddRange(new string[] { " City" }); //opolis
        }
        else
        {
            possibleSuffixes.AddRange(new string[] { " Station", " Installation" });

            if (biome == Planet.Biome.Desert)
                possibleSuffixes.AddRange(new string[] { " Spaceport", " Cosmodrome" });
            else if (biome == Planet.Biome.Swamp)
            {
                possiblePrefixes.AddRange(new string[] { "The " });
                possibleSuffixes.Clear();
                possibleSuffixes.AddRange(new string[] { " Bogtown", " Morass", " Polder", " Quagmire"});
            }
            else if (biome == Planet.Biome.Hell)
                possibleSuffixes.AddRange(new string[] { " Juncture", " Firebase", " Compound", " Complex" });
            else if (biome == Planet.Biome.Spirit)
                possibleSuffixes.AddRange(new string[] { " Aeroport", " Aerodrome" });
        }

        prefix = GetStringFromList(possiblePrefixes);
        suffix = GetStringFromList(possibleSuffixes);
    }

    private static string GetStringFromList(List<string> stringList, string defaultString = "")
    {
        if (stringList == null || stringList.Count == 0)
            return defaultString;
        else
            return stringList[Random.Range(0, stringList.Count)];
    }

    private static string MergeNamePieces(string part1, string part2)
    {
        if (string.IsNullOrEmpty(part1))
            return part2;
        else if (string.IsNullOrEmpty(part2))
            return part1;

        string mergedString;
        if (part1[part1.Length - 1] == part2[0])
            mergedString = part1 + part2.Substring(1);
        else
            mergedString = part1 + part2;

        return mergedString;
    }

    /// <summary>
    /// Public static method that should be called before the galaxy generates to reset the list of city names that have already been generated.
    /// </summary>
    /// <param name="saveGameData"></param>
    public static void ResetCityNamesGenerated(GalaxyData saveGameData)
    {
        cityNamesGenerated = new List<string>();
        if (saveGameData != null)
        {
            foreach (GalaxySolarSystemData solarSystemData in saveGameData.solarSystems)
            {
                foreach (GalaxyPlanetData planetData in solarSystemData.planets)
                {
                    cityNamesGenerated.Add(planetData.city.name);
                }
            }
        }
    }
}
