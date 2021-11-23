using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empire
{
    public Empire(int empireID)
    {
        //Sets the empire id of the empire.
        EmpireID = empireID;

        //Initializes the tech manager of the empire.
        techManager = new TechManager(this.EmpireID);
    }

    //This method is called right after the galaxy has finished generating (at the end of the start method in the galaxy generator class).
    public void OnGalaxyGenerationCompletion()
    {
        pillSkins = Resources.LoadAll<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(empireCulture.ToString()));
    }

    //Contains all of the possible pill skins that pills that belong to this empire can be.
    private Material[] pillSkins;

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
    private string empireName;
    public string EmpireName
    {
        get
        {
            return empireName;
        }
        set
        {
            //Sets the name of the empire to the specified name.
            empireName = value;
            //Updates the resource bar to accurately reflect the name the empire has if it is the player's empire.
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateEmpireNameTooltip();
        }
    }
    public Culture empireCulture;

    private Color empireColor;
    public Color EmpireColor
    {
        get
        {
            return empireColor;
        }
        set
        {
            empireColor = value;
        }
    }
    public Color LabelColor
    {
        get
        {
            Color labelColor = empireColor;

            if (empireCulture == Culture.Red || empireCulture == Culture.Green || empireCulture == Culture.Blue)
            {
                labelColor.r += 0.3f;
                labelColor.g += 0.3f;
                labelColor.b += 0.3f;
            }

            return labelColor;
        }
    }

    public int EmpireID { get; }
    public bool IsPlayerEmpire
    {
        get
        {
            return EmpireID == GalaxyManager.PlayerID;
        }
    }
    public bool receivesResearchEffects = true;

    //Flags.
    private Flag empireFlag;
    public Flag EmpireFlag
    {
        get
        {
            return empireFlag;
        }
        set
        {
            //Sets the flag of the empire to the specified flag.
            empireFlag = value;
            //Updates the resource bar to accurately reflect the flag the empire has if it is the player's empire.
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateFlag();
        }
    }

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
            if(IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateCreditsText();
        }
    }

    //Indicates the number of prescriptions that the empire has.
    private float prescriptions;
    public float Prescriptions
    {
        get
        {
            return prescriptions;
        }
        set
        {
            //Sets the amount of prescriptions that the empire has to the specified value.
            prescriptions = value;
            //Updates the resource bar to accurately reflect the amount of prescriptions that the empire has if it is the player's empire.
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdatePrescriptionsText();
        }
    }

    //Indicates the amount of science that the empire has.
    private float science;
    public float Science
    {
        get
        {
            return science;
        }
        set
        {
            //Sets the amount of science that the empire has to the specified value.
            science = value;
            //Updates the resource bar to accurately reflect the amount of science that the empire has if it is the player's empire.
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateScienceText();
        }
    }

    //Indicates the base amount of credits that the empire receives per turn.
    private float baseCreditsPerTurn;
    public float BaseCreditsPerTurn
    {
        get
        {
            return baseCreditsPerTurn;
        }
        set
        {
            baseCreditsPerTurn = value;
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateCreditsText();
        }
    }
    //Indicates the base amount of prescriptions that the empire receives per turn.
    private float basePrescriptionsPerTurn;
    public float BasePrescriptionsPerTurn
    {
        get
        {
            return basePrescriptionsPerTurn;
        }
        set
        {
            basePrescriptionsPerTurn = value;
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdatePrescriptionsText();
        }
    }
    //Indicates the base amount of science that the empire receives per turn.
    private float baseSciencePerTurn;
    public float BaseSciencePerTurn
    {
        get
        {
            return baseSciencePerTurn;
        }
        set
        {
            baseSciencePerTurn = value;
            if (IsPlayerEmpire && GalaxyGenerator.GalaxyFinishedGenerating)
                ResourceBar.UpdateScienceText();
        }
    }

    public float GetCreditsPerTurn()
    {
        //Accounts for the base credits per turn of the empire.
        float creditsPerTurn = BaseCreditsPerTurn;

        //Accounts for credits generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            creditsPerTurn += GalaxyManager.planets[planetsOwned[x]].CreditsPerTurn;
        }

        return creditsPerTurn;
    }

    public float GetPrescriptionsPerTurn()
    {
        //Accounts for the base prescriptions per turn of the empire.
        float prescriptionsPerTurn = BasePrescriptionsPerTurn;

        //Accounts for the prescriptions generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            prescriptionsPerTurn += GalaxyManager.planets[planetsOwned[x]].PrescriptionsPerTurn;
        }

        return prescriptionsPerTurn;
    }

    public float GetSciencePerTurn()
    {
        //Accounts for the base science per turn of the empire.
        float sciencePerTurn = BaseSciencePerTurn;

        //Accounts for the science per turn generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            sciencePerTurn += GalaxyManager.planets[planetsOwned[x]].SciencePerTurn;
        }

        return sciencePerTurn;
    }

    public float GetProductionPerTurn()
    {
        float productionPerTurn = 0.0f;

        for (int x = 0; x < planetsOwned.Count; x++)
        {
            productionPerTurn += GalaxyManager.planets[planetsOwned[x]].ProductionPerTurn;
        }

        return productionPerTurn;
    }

    //Returns a random pill skin from the empire's array of possible pill skins.
    public Material GetRandomPillSkin()
    {
        return pillSkins[Random.Range(0, pillSkins.Length)];
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
        if (IsPlayerEmpire)
        {
            ResourceBar.UpdateProductionText();
        }
    }
}
