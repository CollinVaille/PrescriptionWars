using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarNameGenerator : MonoBehaviour
{
    private static string[] starNames, boringPillNames;
    private static List<string> starNamesGenerated = new List<string>();

    /// <summary>
    /// Public static method that returns a string that indicates a unique star name.
    /// </summary>
    /// <returns></returns>
    public static string GenerateStarName()
    {
        if (starNames == null)
            ReadInStarNames();

        string starName = "";

        if (UnityEngine.Random.Range(0, 6) != 0) //Normal random name
        {
            //Pick a random name
            starName = starNames[UnityEngine.Random.Range(0, starNames.Length)];

            if (UnityEngine.Random.Range(0, 20) == 0)
                starName += " " + UnityEngine.Random.Range(1, 1000);
        }
        else if (UnityEngine.Random.Range(0, 3) == 0) //"Bob's Star"
        {
            starName = boringPillNames[UnityEngine.Random.Range(0, boringPillNames.Length)];

            if (starName.EndsWith("s"))
                starName += "' Star";
            else
                starName += "'s Star";
        }
        else if (UnityEngine.Random.Range(0, 3) == 0) //Catalogue name, i.e.: "HR 700"
        {
            int numberOfCharacters = UnityEngine.Random.Range(2, 4);
            for (int x = 0; x < numberOfCharacters; x++)
                starName += (char)(UnityEngine.Random.Range(65, 91));

            starName += " " + UnityEngine.Random.Range(1, 1000);
        }
        else if (UnityEngine.Random.Range(0, 3) == 0) //Variable star designation naming format. ex: "T Pyxidis"
        {
            string[] genitiveGreek = new string[] { "Pyxidis", "Serpentis", "Phoenicis", "Octantis", "Lyrae", "Leonis", "Horologii", "Herculis",
            "Geminorum", "Cygni"};

            //[R-Z] [genitive greek]
            if (UnityEngine.Random.Range(0, 3) != 0)
                starName = (char)(UnityEngine.Random.Range(82, 91)) + " " + genitiveGreek[UnityEngine.Random.Range(0, genitiveGreek.Length)];

            //[R-Z][R-Z] [genitive greek]
            else
                starName = (char)(UnityEngine.Random.Range(82, 91)) + "" + (char)(UnityEngine.Random.Range(82, 91)) +
                    " " + genitiveGreek[UnityEngine.Random.Range(0, genitiveGreek.Length)];
        }
        else if (UnityEngine.Random.Range(0, 2) == 0) //Greek letter + astrological/zodiac/birth sign name
        {
            string[] greekLetters = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega"};

            string[] zodiacSigns = new string[] { "Carinae", "Tauri", "Pegasi", "Centauri", "Scuti", "Orionis", "Scorpius",
            "Geminorum"};

            starName = greekLetters[UnityEngine.Random.Range(0, greekLetters.Length)] + " "
                + zodiacSigns[UnityEngine.Random.Range(0, zodiacSigns.Length)];
        }
        else //Some prefix + major or minor name
        {
            string[] prefixes = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega", "Ursa", "Virgo", "Canis", "Pisces", "Saega", "Polis"};

            if (UnityEngine.Random.Range(0, 2) == 0)
                starName = prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + " Major";
            else
                starName = prefixes[UnityEngine.Random.Range(0, prefixes.Length)] + " Minor";
        }

        if (StarNameRedundant(starName))
            return GenerateStarName();

        starNamesGenerated.Add(starName);
        return starName;
    }

    /// <summary>
    /// Method that detects if the star name is already being used by another star.
    /// </summary>
    /// <param name="starName"></param>
    /// <returns></returns>
    private static bool StarNameRedundant(string starName)
    {
        foreach (string starNameGenerated in starNamesGenerated)
        {
            if (starName.Equals(starNameGenerated))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reads in the text file that stores star names.
    /// </summary>
    private static void ReadInStarNames()
    {
        starNames = GeneralHelperMethods.GetLinesFromFile("Location Names/Star Names");
        boringPillNames = GeneralHelperMethods.GetLinesFromFile("Pill Names/Boring Names");
    }


    /// <summary>
    /// Public static method that should be called before the galaxy generates to reset the list of star names that have already been generated.
    /// </summary>
    /// <param name="saveGameData"></param>
    public static void ResetStarNamesGenerated(GalaxyData saveGameData)
    {
        starNamesGenerated = new List<string>();
        if (saveGameData != null)
        {
            foreach (GalaxySolarSystemData solarSystemData in saveGameData.solarSystems)
            {
                //TODO
                //starNamesGenerated.Add(solarSystemData.star.???STARNAME???);
            }
        }
    }
}
