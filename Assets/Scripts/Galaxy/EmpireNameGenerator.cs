using System.Collections.Generic;
using UnityEngine;

public class EmpireNameGenerator
{
    private static int pillNames = 9;
    private static int factionNames = 19;
    private static int adjectives = 18;

    private static List<int> availableFactions, availableAdjectives;

    private static string capitalPlanet = "";

    private static bool prospective = true;
    private static int lastProspectiveAdjectivePicker = -1;
    private static int lastProspectiveFactionNamePicker = -1;

    //Very unlikely to get duplicates. Manage cache system to guarantee uniqueness.
    //Optional parameter "capitalPlanet": If specified, there's a chance it will be included in the empire name.
    //Optional parameter "prospective": The system will remember the last prospective name generated. This can be used to generate a name for the player empire in the new game menu that you can tell the cache to reserve when you reset it for the galaxy.
    public static string GenerateEmpireName (string capitalPlanet = "", bool prospective = false)
    {
        string empireName = "";

        EmpireNameGenerator.capitalPlanet = capitalPlanet;
        EmpireNameGenerator.prospective = prospective;

        int typePicker = Random.Range(1, 7);

        switch(typePicker)
        {
            case 1: //The Evil Pill Clan
                empireName = "The " + GetAdjective(true) + " " + GetPillName(false) + " " + GetFactionName();
                break;
            case 2: //The Pill Clan of Evil
                empireName = "The " + GetPillName(false) + " " + GetFactionName() + " " + GetAdjective(false);
                break;
            case 3: //The Clan of Evil Pills
                empireName = "The " + GetFactionName() + " of " + GetAdjective(true) + " " + GetPillName(true);
                break;
            case 4: //The Evil Clan of Pills
                empireName = "The " + GetAdjective(true) + " " + GetFactionName() + " of " + GetPillName(true);
                break;
            case 5: //The Pills of the Evil Clan
                empireName = "The " + GetPillName(true) + " of the " + GetAdjective(true) + " " + GetFactionName();
                break;
            default: //The Pills of Evil Clan
                empireName = "The " + GetPillName(true) + " " + GetAdjective(false) + " " + GetFactionName();
                break;
        }

        return empireName;
    }

    //Call once at the start of a new game menu and once at the start of a new galaxy scene.
    //Only set "reserveLastProspective" to true at the start of the new galaxy scene when it wants to reserve the auto-generated player name.
    public static void ResetCache(bool reserveLastProspective = false)
    {
        InitializeFactionNames(reserveLastProspective);
        InitializeAdjectives(reserveLastProspective);    
    }

    private static string GetPillName(bool plural)
    {
        string pillName = "";

        int picker = Random.Range(1, pillNames + 1);

        switch(picker)
        {
            case 1:
                pillName = plural ? "Pills" : "Pill";
                break;
            case 2:
                pillName = plural ? "Capsules" : "Capsule";
                break;
            case 3:
                pillName = plural ? "Suppositories" : "Suppository";
                break;
            case 4:
                pillName = plural ? "Drugs" : "Drug";
                break;
            case 5:
                pillName = plural ? "Tablets" : "Tablet";
                break;
            case 6:
                pillName = plural ? "Caplets" : "Caplet";
                break;
            case 7:
                pillName = plural ? "Pellets" : "Pellet";
                break;
            case 8:
                pillName = plural ? "Boluses" : "Bolus";
                break;
            default:
                pillName = plural ? "Lozenges" : "Lozenge";
                break;
        }

        return pillName;
    }

    private static string GetFactionName()
    {
        string factionName = "";

        //Initialize list of faction names not yet taken
        if (availableFactions == null || availableFactions.Count == 0)
            InitializeFactionNames();

        //Get faction name not yet taken
        int indexOfPicker = Random.Range(0, availableFactions.Count);
        int picker = availableFactions[indexOfPicker];
        availableFactions.RemoveAt(indexOfPicker);
        if (prospective)
            lastProspectiveFactionNamePicker = picker;

        switch (picker)
        {
            case 1: factionName = "Pact"; break;
            case 2: factionName = "Guild"; break;
            case 3: factionName = "Alliance"; break;
            case 4: factionName = "Axis"; break;
            case 5: factionName = "Order"; break;
            case 6: factionName = "Nation"; break;
            case 7: factionName = "Coalition"; break;
            case 8: factionName = "Confederacy"; break;
            case 9: factionName = "Federation"; break;
            case 10: factionName = "Creed"; break;
            case 11: factionName = "Empire"; break;
            case 12: factionName = "Union"; break;
            case 13: factionName = "League"; break;
            case 14: factionName = "Inquisition"; break;
            case 15: factionName = "Conglomerate"; break;
            case 16: factionName = "Syndicate"; break;
            case 17: factionName = "Cult"; break;
            case 18: factionName = "Clan"; break;
            default: factionName = "Accord"; break;
        }

        return factionName;
    }

    private static string GetAdjective(bool prefix)
    {
        string adjective = "";

        //Chance for adjective named after capital planet
        if (prefix && !capitalPlanet.Equals("") && Random.Range(0, 2) == 0)
            return GetRandomEthnicName(capitalPlanet);

        //Otherwise it's a normal adjective...

        //Initialize list of adjectives not yet taken
        if (availableAdjectives == null || availableAdjectives.Count == 0)
            InitializeAdjectives();

        //Get adjective not yet taken
        int indexOfPicker = Random.Range(0, availableAdjectives.Count);
        int picker = availableAdjectives[indexOfPicker];
        availableAdjectives.RemoveAt(indexOfPicker);
        if (prospective)
            lastProspectiveAdjectivePicker = picker;

        switch (picker)
        {
            case 1:
                adjective = prefix ? "Holy" : "of God";
                break;
            case 2:
                adjective = prefix ? "Divine" : "from Heaven";
                break;
            case 3:
                adjective = prefix ? "Great" : "of Greatness";
                break;
            case 4:
                adjective = prefix ? "Grand" : "of Grand Prospects";
                break;
            case 5:
                adjective = prefix ? "Ancient" : "of Ancient Times";
                break;
            case 6:
                adjective = prefix ? "Necessary and Proper" : "of Due Justice";
                break;
            case 7:
                adjective = prefix ? "World Domination-Focused" : "of World Domination";
                break;
            case 8:
                adjective = prefix ? "Legendary" : "of Legends";
                break;
            case 9:
                adjective = prefix ? "Terror-Inducing" : "of Terror";
                break;
            case 10:
                adjective = prefix ? "Evil" : "of Evil";
                break;
            case 11:
                adjective = prefix ? "Vile" : "of Fire and Fury";
                break;
            case 12:
                adjective = prefix ? "Diabolical" : "from Hell";
                break;
            case 13:
                adjective = prefix ? "Angelic" : "of Angels";
                break;
            case 14:
                adjective = prefix ? "Galactic" : "of the Galaxy";
                break;
            case 15:
                adjective = prefix ? "Mom and Pop" : "of Doom";
                break;
            case 16:
                adjective = prefix ? "Goblin Smashing" : "of Fiery Justice";
                break;
            case 17:
                adjective = prefix ? "Enlightened" : "of Light";
                break;
            default:
                adjective = prefix ? "Ass-Kissing" : "of Badasses";
                break;
        }

        return adjective;
    }

    private static void InitializeFactionNames(bool reserveLastProspective = false)
    {
        //Initialize list of available faction names
        availableFactions = new List<int>(factionNames);
        for (int x = 1; x <= factionNames; x++)
        {
            if (reserveLastProspective && x == lastProspectiveFactionNamePicker)
                continue;
            else
                availableFactions.Add(x);
        }
    }

    private static void InitializeAdjectives(bool reserveLastProspective = false)
    {
        //Initialize list of available adjectives
        availableAdjectives = new List<int>(adjectives);
        for (int x = 1; x <= adjectives; x++)
        {
            if (reserveLastProspective && x == lastProspectiveAdjectivePicker)
                continue;
            else
                availableAdjectives.Add(x);
        }
    }

    //This assumes planetName is not empty
    private static string GetRandomEthnicName(string planetName)
    {
        //Remove any spaces from name by just selecting first "word"
        planetName = GetFirstWord(planetName);

        if (planetName.Length <= 6)
            return AppendEthnicSuffix(planetName);
        else
            return AppendEthnicSuffix(planetName.Substring(0, 6));
    }

    private static string GetFirstWord(string sentence)
    {
        int x = 0;
        for (; x < sentence.Length; x++)
        {
            if (sentence[x] == ' ')
                break;
        }

        if (x == sentence.Length || x == 0)
            return sentence;
        else
            return sentence.Substring(0, x);
    }

    private static string AppendEthnicSuffix(string baseName)
    {
        if (IsVowel(baseName[baseName.Length - 1]))
            return baseName + "n" + GetEthnicSuffix();
        else
            return baseName + GetEthnicSuffix();
    }

    private static string GetEthnicSuffix()
    {
        if (Random.Range(0, 2) == 0)
            return "ion";
        else
            return "ian";
    }

    private static bool IsVowel(char c)
    {
        return "aeiouAEIOU".IndexOf(c) >= 0;
    }
}
