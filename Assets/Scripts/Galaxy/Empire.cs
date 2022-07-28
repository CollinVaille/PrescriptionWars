using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empire
{
    public Empire(int empireID)
    {
        //Sets the empire id of the empire.
        this.empireID = empireID;

        //Initializes the tech manager of the empire.
        techManager = new TechManager(this.empireID);
    }

    //This method is called right after the galaxy has finished generating (at the end of the start method in the galaxy generator class).
    public void OnGalaxyGenerationCompletion()
    {
        foreach(Material material in Resources.LoadAll<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(empireCulture.ToString())))
        {
            if(pillSkinNamesVar == null)
                pillSkinNamesVar = new string[0];
            Array.Resize(ref pillSkinNamesVar, pillSkinNamesVar.Length + 1);
            pillSkinNamesVar[pillSkinNamesVar.Length - 1] = material.name;
        }
    }

    /// <summary>
    /// Contains all of the possible pill skins that pills that belong to this empire can be.
    /// </summary>
    public string[] pillSkinNames { get => pillSkinNamesVar; }
    private string[] pillSkinNamesVar = null;

    public enum Culture
    {
        Red,
        Green,
        Blue,
        Purple,
        Gold,
        Silver
    }

    //Contains all of the empires in the galaxy and can be accessed from anywhere.
    public static List<Empire> empires;

    //Planets.
    public List<int> planetsOwned;

    //General Information.

    //Indicates the name of the empire.
    private string nameVar;
    public string name
    {
        get
        {
            return nameVar;
        }
        set
        {
            //Sets the name of the empire to the specified name.
            nameVar = value;
            //Updates the resource bar to accurately reflect the name the empire has if it is the player's empire.
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateEmpireNameTooltip();
        }
    }

    public Culture empireCulture;

    private Color colorVar;
    public Color color
    {
        get
        {
            return colorVar;
        }
        set
        {
            colorVar = value;
        }
    }
    public Color labelColor
    {
        get
        {
            Color labelColor = colorVar;

            if (empireCulture == Culture.Red || empireCulture == Culture.Green || empireCulture == Culture.Blue)
            {
                labelColor.r += 0.3f;
                labelColor.g += 0.3f;
                labelColor.b += 0.3f;
            }

            return labelColor;
        }
    }

    public int empireID { get; }
    public bool isPlayerEmpire { get => empireID == GalaxyManager.PlayerID; }
    public bool receivesResearchEffects = true;

    private int capitalPlanetIDVar = -1;
    public int capitalPlanetID { get => capitalPlanetIDVar; set => capitalPlanetIDVar = value; }
    public GalaxyPlanet capitalPlanet { get => GalaxyManager.planets != null && capitalPlanetID >= 0 && capitalPlanetID < GalaxyManager.planets.Count ? GalaxyManager.planets[capitalPlanetID] : null; set => capitalPlanetID = value != null ? value.planetID : capitalPlanetID; }

    //Flags.
    private Flag flagVar;
    public Flag flag
    {
        get
        {
            return flagVar;
        }
        set
        {
            //Sets the flag of the empire to the specified flag.
            flagVar = value;
            //Updates the resource bar to accurately reflect the flag the empire has if it is the player's empire.
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateFlag();
        }
    }

    //Military.
    private List<string> validSquadNamesVar = new List<string>() { "Alpha Squad", "Bravo Squad", "Charlie Squad", "Delta Squad", "Echo Squad", "Foxtrot Squad", "Golf Squad", "Hotel Squad", "India Squad", "Juliet Squad", "Kilo Squad", "Lima Squad", "Mike Squad", "November Squad", "Oscar Squad", "Papa Squad", "Quebec Squad", "Romeo Squad", "Sierra Squad", "Tango Squad", "Uniform Squad", "Victor Squad", "Whiskey Squad", "X-Ray Squad", "Yankee Squad", "Zulu Squad" };
    /// <summary>
    /// Returns the list of squad names that are valid for a squad of this empire to be named.
    /// </summary>
    public List<string> validSquadNames { get => validSquadNamesVar; private set => validSquadNamesVar = value; }
    /// <summary>
    /// Returns a random squad name from the list of valid squad names that are valid for the empire.
    /// </summary>
    public string randomValidSquadName { get => (validSquadNames == null || validSquadNames.Count == 0) ? "Squad" : validSquadNames[UnityEngine.Random.Range(0, validSquadNames.Count)]; }
    /// <summary>
    /// The x value is added to the lower bound for pill experience and the y value is added to the upper bound for pill experience.
    /// </summary>
    public Vector2Int pillExperienceBoundingEffects { get => Vector2Int.zero; }

    //Special pills.
    /// <summary>
    /// Dictionary that contains all special pills serving the empire that are attached to an int id.
    /// </summary>
    private Dictionary<int, GalaxySpecialPill> specialPills = null;
    /// <summary>
    /// Public property that returns a list with all of the special pills serving the empire in it. New special pills cannot be added to this list. A list with zero elements is returned if there are no special pills serving the empire.
    /// </summary>
    public List<GalaxySpecialPill> specialPillsList
    {
        get
        {
            List<GalaxySpecialPill> specialPillsListTemp = new List<GalaxySpecialPill>();
            if(specialPills != null)
            {
                foreach(int specialPillID in specialPills.Keys)
                {
                    specialPillsListTemp.Add(specialPills[specialPillID]);
                }
            }
            return specialPillsListTemp;
        }
    }
    /// <summary>
    /// Public property that indicates how many special pills the empire has had in total (even ones no longer serving the empire).
    /// </summary>
    public int specialPillsCount { get => specialPillsCountVar; }
    /// <summary>
    /// Private int that indicates how many special pills the empire has had in total (even ones no longer serving the empire).
    /// </summary>
    private int specialPillsCountVar = 0;

    //Tech.
    public TechManager techManager;

    //Popups.
    public List<GalaxyPopupData> popups = new List<GalaxyPopupData>();

    //Resources.

    //Indicates the number of credits that the empire has.
    private float credits;
    public float Credits
    {
        get
        {
            return credits;
        }
        set
        {
            //Sets the amount of credits the empire has to the specified value.
            credits = value;
            //Updates the resource bar to accurately reflect the amount of credits that the empire has if it is the player's empire.
            if(isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateCreditsText();
        }
    }

    //Indicates the number of prescriptions that the empire has.
    private float prescriptionsVar;
    public float prescriptions
    {
        get
        {
            return prescriptionsVar;
        }
        set
        {
            //Sets the amount of prescriptions that the empire has to the specified value.
            prescriptionsVar = value;
            //Updates the resource bar to accurately reflect the amount of prescriptions that the empire has if it is the player's empire.
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdatePrescriptionsText();
        }
    }

    //Indicates the amount of science that the empire has.
    private float scienceVar;
    public float science
    {
        get
        {
            return scienceVar;
        }
        set
        {
            //Sets the amount of science that the empire has to the specified value.
            scienceVar = value;
            //Updates the resource bar to accurately reflect the amount of science that the empire has if it is the player's empire.
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateScienceText();
        }
    }

    //Indicates the base amount of credits that the empire receives per turn.
    private float baseCreditsPerTurnVar;
    public float baseCreditsPerTurn
    {
        get
        {
            return baseCreditsPerTurnVar;
        }
        set
        {
            baseCreditsPerTurnVar = value;
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateCreditsText();
        }
    }
    //Indicates the base amount of prescriptions that the empire receives per turn.
    private float basePrescriptionsPerTurnVar;
    public float basePrescriptionsPerTurn
    {
        get
        {
            return basePrescriptionsPerTurnVar;
        }
        set
        {
            basePrescriptionsPerTurnVar = value;
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdatePrescriptionsText();
        }
    }
    //Indicates the base amount of science that the empire receives per turn.
    private float baseSciencePerTurnVar;
    public float baseSciencePerTurn
    {
        get
        {
            return baseSciencePerTurnVar;
        }
        set
        {
            baseSciencePerTurnVar = value;
            if (isPlayerEmpire && GalaxyGenerator.galaxyFinishedGenerating)
                ResourceBar.UpdateScienceText();
        }
    }

    public float GetCreditsPerTurn()
    {
        //Accounts for the base credits per turn of the empire.
        float creditsPerTurn = baseCreditsPerTurn;

        //Accounts for credits generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            creditsPerTurn += GalaxyManager.planets[planetsOwned[x]].creditsPerTurn;
        }

        return creditsPerTurn;
    }

    public float GetPrescriptionsPerTurn()
    {
        //Accounts for the base prescriptions per turn of the empire.
        float prescriptionsPerTurn = basePrescriptionsPerTurn;

        //Accounts for the prescriptions generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            prescriptionsPerTurn += GalaxyManager.planets[planetsOwned[x]].prescriptionsPerTurn;
        }

        return prescriptionsPerTurn;
    }

    public float GetSciencePerTurn()
    {
        //Accounts for the base science per turn of the empire.
        float sciencePerTurn = baseSciencePerTurn;

        //Accounts for the science per turn generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            sciencePerTurn += GalaxyManager.planets[planetsOwned[x]].sciencePerTurn;
        }

        return sciencePerTurn;
    }

    public float GetProductionPerTurn()
    {
        float productionPerTurn = 0.0f;

        for (int x = 0; x < planetsOwned.Count; x++)
        {
            productionPerTurn += GalaxyManager.planets[planetsOwned[x]].productionPerTurn;
        }

        return productionPerTurn;
    }

    //Returns a random pill skin from the empire's array of possible pill skins.
    public string GetRandomPillSkinName()
    {
        return pillSkinNamesVar[UnityEngine.Random.Range(0, pillSkinNamesVar.Length)];
    }

    public void PlayAI()
    {
        //Checks to make sure that a tech is selected.
        if (techManager.techTotemSelected < 0 || techManager.techTotemSelected >= techManager.techTotems.Count)
        {
            //Determines the lowest level of tech the ai can pick.
            int lowestLevelPossible = 0;
            bool oneTotemEvaluated = false;
            for (int x = 0; x < techManager.techTotems.Count; x++)
            {
                if (techManager.techTotems[x].techsAvailable.Count > 0)
                {
                    if (Tech.entireTechList[techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed]].level < lowestLevelPossible || !oneTotemEvaluated)
                    {
                        lowestLevelPossible = Tech.entireTechList[techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed]].level;
                        oneTotemEvaluated = true;
                    }
                }
            }

            if (oneTotemEvaluated)
            {
                //Gets a list of the tech totems whos displayed tech has the lowest possible tech level the ai can pick.
                List<int> possibleTechTotems = new List<int>();
                for (int x = 0; x < techManager.techTotems.Count; x++)
                {
                    if (techManager.techTotems[x].techsAvailable.Count > 0)
                    {
                        if (Tech.entireTechList[techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed]].level == lowestLevelPossible)
                            possibleTechTotems.Add(x);
                    }
                }

                //Picks a random tech totem to research out of the list that was just generated above.
                techManager.techTotemSelected = possibleTechTotems[UnityEngine.Random.Range(0, possibleTechTotems.Count)];
            }
            else
            {
                techManager.techTotemSelected = -1;
            }
        }
    }

    public void EndTurn()
    {
        foreach (int planetID in planetsOwned)
        {
            GalaxyPlanet planetScript = GalaxyManager.planets[planetID];

            //Runs the logic for when a turn ends for each planet in the empire.
            planetScript.EndTurn();
        }

        //Runs the logic of the empire's tech manager for when a turn ends.
        techManager.EndTurn();

        //Cycles through each popup that the ai has to answer and picks a random option for them and applies the effects of said option before removing the popup from the list of popups that the ai still has to deal with before their end turn logic is done.
        for (int x = popups.Count - 1; x >= 0; x--)
        {
            int optionChosenIndex = UnityEngine.Random.Range(0, popups[x].options.Count);

            foreach (GalaxyPopupOptionEffect effect in popups[x].options[optionChosenIndex].effects)
            {
                GalaxyPopupManager.ApplyPopupOptionEffect(effect);
            }

            popups.RemoveAt(x);
        }

        //Updates the text of the resource bar that says the production per turn of the empire if this empire is the player empire.
        if (isPlayerEmpire)
        {
            ResourceBar.UpdateProductionText();
        }
    }

    /// <summary>
    /// Picks a new capital for the empire excluding the current capital.
    /// </summary>
    public void PickNewCapital()
    {
        int newCapitalPlanetID = -1;
        foreach(int planetID in planetsOwned)
        {
            if (planetID == capitalPlanetID)
                continue;
            newCapitalPlanetID = planetID;
            break;
        }
        capitalPlanetID = newCapitalPlanetID;
    }

    /// <summary>
    /// Public void method that should be called in order to add a special pill to the list of special pills that are serving the empire.
    /// </summary>
    /// <param name="specialPill"></param>
    public void AddSpecialPill(GalaxySpecialPill specialPill)
    {
        if (specialPill == null || (specialPills != null && specialPills.ContainsKey(specialPill.specialPillID)))
            return;
        if (specialPills == null)
            specialPills = new Dictionary<int, GalaxySpecialPill>();
        specialPills.Add(specialPillsCountVar, specialPill);
        specialPill.assignedEmpire = this;
        specialPillsCountVar++;
    }

    /// <summary>
    /// Public method that returns the special pill serving the empire with the specified specialPillID (returns null if no such special pill exists).
    /// </summary>
    /// <param name="specialPillID"></param>
    /// <returns></returns>
    public GalaxySpecialPill GetSpecialPill(int specialPillID)
    {
        if (specialPills != null && specialPills.ContainsKey(specialPillID))
            return specialPills[specialPillID];
        return null;
    }

    /// <summary>
    /// Public method that should be called in order remove a special pill from the list of special pills that are serving the empire. Returns a boolean indicating whether a successful removal occured.
    /// </summary>
    /// <param name="specialPillID"></param>
    /// <returns></returns>
    public bool RemoveSpecialPill(int specialPillID)
    {
        if (specialPills != null && specialPills.ContainsKey(specialPillID))
        {
            specialPills.Remove(specialPillID);
            return true;
        }
        return false;
    }
}
