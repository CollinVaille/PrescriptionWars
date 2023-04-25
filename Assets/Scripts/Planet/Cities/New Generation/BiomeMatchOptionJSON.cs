using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ListMatchModeType { Blacklist, Whitelist }

[System.Serializable]
public class BiomeMatchOptionJSON
{
    public string matchName;

    public string biomeListType;
    public string[] biomeList;

    public string subBiomeListType;
    public string[] subBiomeList;

    //When multiple options could work, we use this probability field to determine which one will "win".
    public float conflictResolutionChance = 1.0f;



    //---



    public static BiomeMatchOptionJSON GetBestMatch(string biomeName, string subBiomeName, BiomeMatchOptionJSON[] possibleMatches, BiomeMatchOptionJSON defaultMatch)
    {
        List<BiomeMatchOptionJSON> matches = new List<BiomeMatchOptionJSON>();
        float totalProbability = 0.0f;

        foreach(BiomeMatchOptionJSON possibleMatch in possibleMatches)
        {
            if (possibleMatch == null)
                continue;

            if (possibleMatch.IsMatch(biomeName, subBiomeName))
            {
                matches.Add(possibleMatch);
                totalProbability += possibleMatch.conflictResolutionChance;
            }
        }

        if (Mathf.Approximately(totalProbability, 0.0f) || matches.Count == 0)
            return defaultMatch;
        else
        {
            float selection = Random.Range(0.0f, totalProbability);
            float passed = 0.0f;
            BiomeMatchOptionJSON theMatch = null;

            for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                theMatch = matches[matchIndex];

                if (selection >= passed && selection < passed + theMatch.conflictResolutionChance)
                    break;

                passed += theMatch.conflictResolutionChance;
            }

            return theMatch;
        }
    }

    private bool IsMatch(string biomeName, string subBiomeName)
    {
        return InputMatchesList(biomeName, biomeList, GetMatchType(biomeListType)) && InputMatchesList(subBiomeName, subBiomeList, GetMatchType(subBiomeListType));
    }

    private static ListMatchModeType GetMatchType(string matchTypeAsString, ListMatchModeType defaultValue = ListMatchModeType.Blacklist)
    {
        if (System.Enum.TryParse<ListMatchModeType>(matchTypeAsString, out ListMatchModeType matchType))
            return matchType;
        else
            return defaultValue;
    }

    private static bool InputMatchesList(string inputToMatch, string[] listToMatchAgainst, ListMatchModeType matchMode)
    {
        bool inputInList = InputInList(inputToMatch, listToMatchAgainst);

        if (matchMode == ListMatchModeType.Blacklist)
            return !inputInList;
        else //Whitelist
            return inputInList;
    }

    private static bool InputInList(string inputToMatch, string[] listToMatchAgainst)
    {
        if (listToMatchAgainst == null || listToMatchAgainst.Length == 0)
            return false;
        else
        {
            for(int x = 0; x < listToMatchAgainst.Length; x++)
            {
                if (string.Equals(inputToMatch, listToMatchAgainst[x], System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
