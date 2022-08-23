using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechManager
{
    public TechManager(int ownerEmpireID)
    {
        this.ownerEmpireID = ownerEmpireID;
    }

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
                    if (ownerEmpireID == GalaxyManager.playerID)
                        GalaxyManager.galaxyManager.PlayTechFinishedSFX();
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

    private void CreateResearchCompletedNotification(Tech completedTech)
    {
        GalaxyPopupData researchCompletedPopup = new GalaxyPopupData();
        researchCompletedPopup.headLine = "Research Completed";
        researchCompletedPopup.spriteResourcesFilePath = "Tech Totems/" + completedTech.spriteName;
        researchCompletedPopup.bodyText = "Our glorious empire has finished researching the " + completedTech.name + " tech. " + completedTech.name + ": " + completedTech.description;
        researchCompletedPopup.specialOpenSFXName = "Chemistry Bubbles";
        researchCompletedPopup.answerRequired = false;
        researchCompletedPopup.spriteFit = GalaxyPopupSpriteFit.Horizontal;
        researchCompletedPopup.spritePositionPercentage = 0.25f;
        GalaxyPopupOptionData option = new GalaxyPopupOptionData();
        option.mainText = "Select new tech to research.";
        option.effectDescriptionText = "Opens the research view.";
        GalaxyPopupOptionEffect optionEffect = new GalaxyPopupOptionEffect();
        optionEffect.effectType = GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.OpenResearchView;
        option.effects.Add(optionEffect);
        researchCompletedPopup.options.Add(option);

        RightSideNotificationManager.CreateNewRightSideNotification("Science Icon", "Research Completed", true, researchCompletedPopup);
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

            techDisplayed = possibleTechs[UnityEngine.Random.Range(0, possibleTechs.Count)];
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
        foreach (int techNum in techsAvailable)
        {
            techsLeftToAdd.Add(techNum);
        }

        //Determines the lowest level of tech in the list.
        int lowestLevel = 0;
        for (int x = 0; x < techsLeftToAdd.Count; x++)
        {
            if (x == 0 || Tech.entireTechList[techsLeftToAdd[x]].level < lowestLevel)
                lowestLevel = Tech.entireTechList[techsLeftToAdd[x]].level;
        }

        int checkingLevel = lowestLevel;
        while (techsLeftToAdd.Count > 0)
        {
            for (int x = 0; x < techsLeftToAdd.Count; x++)
            {
                if (Tech.entireTechList[techsLeftToAdd[x]].level == checkingLevel)
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
    public string spriteName;

    public int level;

    public float cost;

    public List<TechEffect> effects;

    public static List<int> GetSortedTechIDListByLevel(List<int> techIDListToSort)
    {
        List<int> sortedTechIDList = new List<int>();

        List<int> techIDsLeftToSort = techIDListToSort;
        for (int techLevel = 1; techLevel > 0; techLevel++)
        {
            for (int index = techIDsLeftToSort.Count - 1; index >= 0; index--)
            {
                if (entireTechList[techIDsLeftToSort[index]].level == techLevel)
                {
                    sortedTechIDList.Add(techIDsLeftToSort[index]);
                    techIDsLeftToSort.RemoveAt(index);
                }
            }

            if (techIDListToSort.Count <= 0)
                break;
        }

        return sortedTechIDList;
    }
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