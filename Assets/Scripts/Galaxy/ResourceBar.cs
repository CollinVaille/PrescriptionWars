using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [Header("Image Components")]

    public Image flagBackground;
    public Image flagSymbol;

    [Header("Text Components")]

    public Text creditsText;
    public Text prescriptionsText;
    public Text scienceText;
    public Text productionText;
    public Text turnText;

    [Header("Tooltip Components")]

    public GalaxyTooltip empireNameTooltip;

    //Non-inspector variables.

    private int flagSymbolNum = -1;

    private static ResourceBar resourceBar;

    // Start is called before the first frame update
    void Start()
    {
        UpdateTurnText();
        UpdateProductionText();
    }

    private void Awake()
    {
        resourceBar = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private static string GetResourceString(float amount)
    {
        string resourceString = "";

        if(amount < 1000)
        {
            resourceString = "" + (int)amount;
        }
        else if(amount >= 1000 && amount < 10000)
        {
            resourceString = "" + ((int)(amount / 10)) / 100.0f + "K";
        }
        else if(amount >= 10000 && amount < 100000)
        {
            resourceString = "" + ((int)(amount / 100)) / 10.0f + "K";
        }
        else if(amount >= 100000 && amount < 1000000)
        {
            resourceString = "" + (int)(amount / 1000) + "K";
        }
        else if(amount >= 1000000 && amount < 10000000)
        {
            resourceString = "" + (int)(amount / 10000) / 100.0f + "M";
        }
        else
        {
            resourceString = "" + amount;
        }

        return resourceString;
    }

    public static void UpdateAllEmpireDependantComponents()
    {
        UpdateCreditsText();
        UpdatePrescriptionsText();
        UpdateScienceText();
        UpdateFlag();
        UpdateEmpireNameTooltip();
    }

    public static void UpdateCreditsText()
    {
        resourceBar.creditsText.text = GetResourceString(Empire.empires[GalaxyManager.PlayerID].Credits);
        resourceBar.creditsText.text += " +" + (int)Empire.empires[GalaxyManager.PlayerID].GetCreditsPerTurn();
    }

    public static void UpdatePrescriptionsText()
    {
        resourceBar.prescriptionsText.text = GetResourceString(Empire.empires[GalaxyManager.PlayerID].Prescriptions);
        resourceBar.prescriptionsText.text += " +" + (int)Empire.empires[GalaxyManager.PlayerID].GetPrescriptionsPerTurn();
    }

    public static void UpdateScienceText()
    {
        resourceBar.scienceText.text = GetResourceString(Empire.empires[GalaxyManager.PlayerID].Science);
        try
        {
            resourceBar.scienceText.text += "/" + Tech.entireTechList[Empire.empires[GalaxyManager.PlayerID].techManager.techTotems[Empire.empires[GalaxyManager.PlayerID].techManager.techTotemSelected].techsAvailable[Empire.empires[GalaxyManager.PlayerID].techManager.techTotems[Empire.empires[GalaxyManager.PlayerID].techManager.techTotemSelected].techDisplayed]].cost;
        }
        catch (Exception)
        {

        }
        resourceBar.scienceText.text += " +" + (int)Empire.empires[GalaxyManager.PlayerID].GetSciencePerTurn();
    }

    public static void UpdateFlag()
    {
        //Updates the flag's symbol.
        if (resourceBar.flagSymbolNum != Empire.empires[GalaxyManager.PlayerID].EmpireFlag.symbolSelected)
        {
            resourceBar.flagSymbol.sprite = GalaxyManager.flagSymbols[Empire.empires[GalaxyManager.PlayerID].EmpireFlag.symbolSelected];
            resourceBar.flagSymbolNum = Empire.empires[GalaxyManager.PlayerID].EmpireFlag.symbolSelected;
        }

        resourceBar.flagBackground.color = Empire.empires[GalaxyManager.PlayerID].EmpireFlag.backgroundColor;
        resourceBar.flagSymbol.color = Empire.empires[GalaxyManager.PlayerID].EmpireFlag.symbolColor;
    }

    public static void UpdateEmpireNameTooltip()
    {
        resourceBar.empireNameTooltip.Text = Empire.empires[GalaxyManager.PlayerID].EmpireName;
    }

    public static void UpdateTurnText()
    {
        resourceBar.turnText.text = "Turn: " + GalaxyManager.TurnNumber;
    }

    public static void UpdateProductionText()
    {
        resourceBar.productionText.text = "+" + (int)Empire.empires[GalaxyManager.PlayerID].GetProductionPerTurn();
    }
}
