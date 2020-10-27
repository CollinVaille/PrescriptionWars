using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyManager : MonoBehaviour
{
    public GameObject commandConsole;
    public GameObject researchView;
    public GameObject endTurnButton;
    public GalaxyConfirmationPopup galaxyConfirmationPopup;

    //Audio stuff.
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip switchToResearchViewSFX;
    public AudioClip techFinishedSFX;
    public AudioClip endTurnSFX;

    public static int playerID = 0;
    public static int turnNumber = 0;

    public static List<PlanetIcon> planets;

    public static List<Sprite> flagSymbols;

    public static bool observationModeEnabled = false;
    public static bool popupClosedOnFrame = false;

    public static GameObject planetManagementMenu;
    public static GalaxyManager galaxyManager;

    public static List<Material> empireMaterials = new List<Material>() { null, null, null, null, null};

    public static Camera galaxyCamera;

    public static void Initialize(List<PlanetIcon> planetList, List<Sprite> flagSymbolsList, GameObject menuOfPlanetManagement, Camera galaxyCam)
    {
        planets = planetList;
        flagSymbols = flagSymbolsList;
        planetManagementMenu = menuOfPlanetManagement;
        galaxyCamera = galaxyCam;
    }

    // Start is called before the first frame update
    void Start()
    {
        galaxyManager = this;
        GalaxyConfirmationPopup.galaxyConfirmationPopup = galaxyConfirmationPopup;
    }

    // Update is called once per frame
    void Update()
    {
        popupClosedOnFrame = false;

        for(int x = 0; x < Empire.empires.Count; x++)
        {
            if (x == playerID)
                Empire.empires[x].playerEmpire = true;
            else
                Empire.empires[x].playerEmpire = false;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote) && !GalaxyConfirmationPopup.galaxyConfirmationPopup.gameObject.activeInHierarchy)
        {
            commandConsole.GetComponent<CheatConsole>().ToggleConsole();
        }
    }

    public void SwitchToResearchView()
    {
        //Closes the planet management menu if it is open.
        if (planetManagementMenu.activeInHierarchy)
            planetManagementMenu.GetComponent<PlanetManagementMenu>().CloseMenu();

        //Turns on the research view.
        researchView.SetActive(true);
        //Switches the skybox material to the one assigned to the research view.
        RenderSettings.skybox = researchView.GetComponent<TechInterface>().skyboxMaterial;
        //Turns off the galaxy view.
        transform.gameObject.SetActive(false);

        //Plays the switch to research view sound effect.
        sfxSource.PlayOneShot(switchToResearchViewSFX);
    }

    public void EndTurn()
    {
        if(!RightSideNotificationManager.ContainsNonDismissableNotification() && !GalaxyPopupManager.ContainsNonDismissablePopup())
        {
            //Dismisses all right side notifications that still exist and do not require an answer.
            RightSideNotificationManager.DismissAllNotifications();
            //Closes all popups that still exist and do not require an answer.
            GalaxyPopupManager.CloseAllPopups();

            //Plays the end turn sound effect.
            sfxSource.PlayOneShot(endTurnSFX);

            //Closes the planet management menu if it is currently open.
            if (planetManagementMenu.activeInHierarchy)
                planetManagementMenu.GetComponent<PlanetManagementMenu>().CloseMenu();

            //Everyone makes their moves for the turn.
            for (int x = 0; x < Empire.empires.Count; x++)
            {
                if (x != playerID || observationModeEnabled)
                    Empire.empires[x].PlayAI();
            }

            //Stuff is calculated and added after everyone's turn.
            foreach (Empire empire in Empire.empires)
            {
                empire.EndTurn();
            }

            //Logs that a turn has been completed.
            turnNumber++;
        }
    }
}

public class Empire
{
    public enum Culture
    {
        Red,
        Green,
        Blue,
        Purple,
        Gold
    }

    public static List<Empire> empires;

    //Planets
    public List<int> planetsOwned;

    //General Information
    public string empireName;
    public Culture empireCulture;
    public Color empireColor;
    public int empireID;
    public bool playerEmpire;
    public bool receivesResearchEffects = true;

    //Flags
    public Flag empireFlag;

    //Tech
    public TechManager techManager;

    //Resources
    public float credits;
    public float prescriptions;
    public float science;

    public float baseCreditsPerTurn;
    public float basePresciptionsPerTurn;
    public float baseSciencePerTurn;

    public Color GetLabelColor()
    {
        Color labelColor = empireColor;

        if(empireCulture == Culture.Red || empireCulture == Culture.Green || empireCulture == Culture.Blue)
        {
            labelColor.r += 0.3f;
            labelColor.g += 0.3f;
            labelColor.b += 0.3f;
        }

        return labelColor;
    }

    public float GetCreditsPerTurn()
    {
        //Accounts for the base credits per turn of the empire.
        float creditsPerTurn = baseCreditsPerTurn;

        //Accounts for credits generated by the empire's planets.
        for(int x = 0; x < planetsOwned.Count; x++)
        {
            creditsPerTurn += GalaxyManager.planets[planetsOwned[x]].creditsPerTurn();
        }

        return creditsPerTurn;
    }

    public float GetPrescriptionsPerTurn()
    {
        //Accounts for the base prescriptions per turn of the empire.
        float prescriptionsPerTurn = basePresciptionsPerTurn;

        //Accounts for the prescriptions generated by the empire's planets.
        for (int x = 0; x < planetsOwned.Count; x++)
        {
            prescriptionsPerTurn += GalaxyManager.planets[planetsOwned[x]].prescriptionsPerTurn();
        }

        return prescriptionsPerTurn;
    }

    public float GetSciencePerTurn()
    {
        //Accounts for the base science per turn of the empire.
        float sciencePerTurn = baseSciencePerTurn;

        //Accounts for the science per turn generated by the empire's planets.
        for(int x = 0; x < planetsOwned.Count; x++)
        {
            sciencePerTurn += GalaxyManager.planets[planetsOwned[x]].sciencePerTurn();
        }

        return sciencePerTurn;
    }

    public float GetProductionPerTurn()
    {
        float productionPerTurn = 0.0f;

        for(int x = 0; x < planetsOwned.Count; x++)
        {
            productionPerTurn += GalaxyManager.planets[planetsOwned[x]].productionPerTurn();
        }

        return productionPerTurn;
    }

    public void PlayAI()
    {
        //Checks to make sure that a tech is selected.
        if(techManager.techTotemSelected < 0 || techManager.techTotemSelected >= techManager.techTotems.Count)
        {
            //Determines the lowest level of tech the ai can pick.
            int lowestLevelPossible = 0;
            bool oneTotemEvaluated = false;
            for(int x = 0; x < techManager.techTotems.Count; x++)
            {
                if(techManager.techTotems[x].techsAvailable.Count > 0)
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
                    if(techManager.techTotems[x].techsAvailable.Count > 0)
                    {
                        if (Tech.entireTechList[techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed]].level == lowestLevelPossible)
                            possibleTechTotems.Add(x);
                    }
                }

                //Picks a random tech totem to research out of the list that was just generated above.
                techManager.techTotemSelected = possibleTechTotems[Random.Range(0, possibleTechTotems.Count)];
            }
            else
            {
                techManager.techTotemSelected = -1;
            }
        }
    }

    public void EndTurn()
    {
        foreach(int planetID in planetsOwned)
        {
            PlanetIcon planetScript = GalaxyManager.planets[planetID];

            planetScript.EndTurn();
            techManager.EndTurn();
        }
    }
}

public class TechManager
{
    public List<TechTotem> techTotems = new List<TechTotem>();

    public int techTotemSelected = -1;
    public int ownerEmpireID;

    public float baseCreditsProductionAmount = 0.0f;
    public float tradePostCreditsProductionAmount = 0.0f;
    public float baseProductionProductionAmount = 0.0f;
    public float researchFacilityScienceProductionAmount = 0.0f;
    float riotShieldEnabledAmount = 0.0f;
    float repairToolsEnabledAmount = 0.0f;
    float automaticWeaponsEnabledAmount = 0.0f;


    public bool riotShieldsEnabled = false;
    public bool repairToolsEnabled = false;
    public bool automaticWeaponsEnabled = false;

    public void EndTurn()
    {
        bool researchingSomething = false;
        //Detects if a valid tech totem is selected.
        if (techTotemSelected > -1 && techTotemSelected < techTotems.Count)
        {
            if (techTotems[techTotemSelected].techsAvailable.Count > 0)
            {
                //Detects if the empire has enough science to complete the selected tech.
                if (Empire.empires[ownerEmpireID].science >= Tech.entireTechList[techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed]].cost)
                {
                    //Removes the tech's cost from the total science.
                    Empire.empires[ownerEmpireID].science -= Tech.entireTechList[techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed]].cost;

                    //Creates the right side notification that tells the user that they have finished the tech that they were researching.
                    if (ownerEmpireID == GalaxyManager.playerID)
                        CreateResearchCompletedNotification(Tech.entireTechList[techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed]]);

                    //Removes the completed tech from the available techs list and adds it to the techs completed list.
                    techTotems[techTotemSelected].techsCompleted.Add(techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed]);
                    techTotems[techTotemSelected].techsAvailable.RemoveAt(techTotems[techTotemSelected].techDisplayed);

                    //Randomizes what tech will be displayed next.
                    techTotems[techTotemSelected].RandomizeTechDisplayed();

                    //Makes it to where no tech totem is selected.
                    techTotemSelected = -1;

                    //Updates the effects the empire gets from its technology.
                    UpdateTechnologyEffects();

                    //Plays the tech finished sound effect if it is the player that has completed the tech.
                    if(ownerEmpireID == GalaxyManager.playerID)
                        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(GalaxyManager.galaxyManager.techFinishedSFX, 0.5f);
                }

                researchingSomething = true;
            }
        }

        if (!researchingSomething)
        {
            //If the player has nothing researching for an entire turn, the science is set back to zero as a sort of punishment. :)
            Empire.empires[ownerEmpireID].science = 0;
        }
    }

    void CreateResearchCompletedNotification(Tech completedTech)
    {
        GalaxyPopupData researchCompletedPopup = new GalaxyPopupData();
        researchCompletedPopup.headLine = "Research Completed";
        researchCompletedPopup.spriteName = "Research Facility";
        researchCompletedPopup.bodyText = "Our glorious empire has finished researching the " + completedTech.name + " tech. " + completedTech.name + ": " + completedTech.description;
        GalaxyPopupOptionData option = new GalaxyPopupOptionData();
        option.mainText = "We grow stronger by the day.";
        option.effectDescriptionText = "The " + completedTech.name + " tech has been researched.";

        GalaxyPopupOptionEffect testEffect = new GalaxyPopupOptionEffect();
        testEffect.effectType = GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.ConquerPlanet;
        //testEffect.effectAmount = 100;
        testEffect.effectDependencies.Add(Random.Range(0, GalaxyManager.planets.Count));
        testEffect.effectDependencies.Add(GalaxyManager.playerID);
        option.effects.Add(testEffect);

        researchCompletedPopup.options.Add(option);
        researchCompletedPopup.answerRequired = false;
        researchCompletedPopup.specialOpenSFXName = "Chemistry Bubbles";

        RightSideNotificationManager.CreateNewRightSideNotification("Science Icon", "Research Completed", researchCompletedPopup);
    }

    public void UpdateTechnologyEffects()
    {
        baseCreditsProductionAmount = 0.0f;
        tradePostCreditsProductionAmount = 0.0f;
        baseProductionProductionAmount = 0.0f;
        researchFacilityScienceProductionAmount = 0.0f;
        riotShieldEnabledAmount = 0.0f;
        repairToolsEnabledAmount = 0.0f;
        automaticWeaponsEnabledAmount = 0.0f;

        riotShieldsEnabled = false;
        repairToolsEnabled = false;
        automaticWeaponsEnabled = false;

        if (Empire.empires[ownerEmpireID].receivesResearchEffects)
        {
            foreach (TechTotem totem in techTotems)
            {
                foreach (int indexCompleted in totem.techsCompleted)
                {
                    foreach (TechEffect techEffect in Tech.entireTechList[indexCompleted].effects)
                    {
                        TechEffect.TechEffectType techEffectType = techEffect.effectType;

                        switch (techEffectType)
                        {
                            case TechEffect.TechEffectType.BaseCreditsProduction:
                                baseCreditsProductionAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.TradePostCreditsProduction:
                                tradePostCreditsProductionAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.BaseProductionProduction:
                                baseProductionProductionAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.EnableRiotShields:
                                riotShieldEnabledAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.ResearchFacilityScienceProduction:
                                researchFacilityScienceProductionAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.EnableRepairTools:
                                repairToolsEnabledAmount += techEffect.amount;
                                break;
                            case TechEffect.TechEffectType.EnableAutomaticWeapons:
                                automaticWeaponsEnabledAmount += techEffect.amount;
                                break;

                        }
                    }
                }
            }

            if (riotShieldEnabledAmount > 0)
                riotShieldsEnabled = true;
            if (repairToolsEnabledAmount > 0)
                repairToolsEnabled = true;
            if (automaticWeaponsEnabledAmount > 0)
                automaticWeaponsEnabled = true;
        }
    }
}

public class TechTotem
{
    public List<int> techsCompleted = new List<int>();
    public List<int> techsAvailable = new List<int>();

    public string name;

    public int techDisplayed;

    public void RandomizeTechDisplayed()
    {
        if (techsAvailable.Count > 0)
        {
            int lowestPossibleLevel = 0;
            for (int x = 0; x < techsAvailable.Count; x++)
            {
                if (Tech.entireTechList[techsAvailable[x]].level < lowestPossibleLevel || x == 0)
                    lowestPossibleLevel = Tech.entireTechList[techsAvailable[x]].level;
            }

            List<int> possibleTechs = new List<int>();
            for (int x = 0; x < techsAvailable.Count; x++)
            {
                if (Tech.entireTechList[techsAvailable[x]].level == lowestPossibleLevel)
                    possibleTechs.Add(x);
            }

            techDisplayed = possibleTechs[Random.Range(0, possibleTechs.Count)];
        }
        else
        {
            techDisplayed = -1;
        }
    }

    public List<int> GetTechsInOrderList()
    {
        List<int> techsInOrder = new List<int>();
        List<int> techsLeftToAdd = new List<int>();
        foreach(int techNum in techsAvailable)
        {
            techsLeftToAdd.Add(techNum);
        }

        //Determines the lowest level of tech in the list.
        int lowestLevel = 0;
        for(int x = 0; x < techsLeftToAdd.Count; x++)
        {
            if (x == 0 || Tech.entireTechList[techsLeftToAdd[x]].level < lowestLevel)
                lowestLevel = Tech.entireTechList[techsLeftToAdd[x]].level;
        }

        int checkingLevel = lowestLevel;
        while(techsLeftToAdd.Count > 0)
        {
            for(int x = 0; x < techsLeftToAdd.Count; x++)
            {
                if(Tech.entireTechList[techsLeftToAdd[x]].level == checkingLevel)
                {
                    techsInOrder.Add(techsLeftToAdd[x]);
                    techsLeftToAdd.RemoveAt(x);
                    x--;
                }
            }

            checkingLevel++;
        }

        return techsInOrder;
    }
}

[System.Serializable]
public class Tech
{
    public static List<Tech> entireTechList = new List<Tech>();

    public string name;
    public string description;
    public string totemName;

    public int level;
    public int spriteNum;

    public float cost;

    public List<TechEffect> effects;
}

[System.Serializable]
public class TechEffect
{
    public enum TechEffectType
    {
        BaseCreditsProduction,
        TradePostCreditsProduction,
        BaseProductionProduction,
        EnableRiotShields,
        ResearchFacilityScienceProduction,
        EnableRepairTools,
        EnableAutomaticWeapons
    }

    public TechEffectType effectType;

    public float amount;
}