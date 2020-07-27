﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    public Image flagBackground;
    public Image flagSymbol;

    int flagSymbolNum = -1;

    public Text empireNameText;
    public Text creditsText;
    public Text prescriptionsText;
    public Text scienceText;
    public Text turnText;

    public float updatesPerSecond;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= (1 / updatesPerSecond))
        {
            //Updates the flag's symbol.
            if(flagSymbolNum != Empire.empires[GalaxyManager.playerID].empireFlag.symbolSelected)
                flagSymbol.sprite = GalaxyManager.flagSymbols[Empire.empires[GalaxyManager.playerID].empireFlag.symbolSelected];

            flagBackground.color = new Color(Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.x, Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.y, Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.z, 1.0f);
            flagSymbol.color = new Color(Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.x, Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.y, Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.z, 1.0f);
            empireNameText.text = Empire.empires[GalaxyManager.playerID].empireName;
            empireNameText.gameObject.GetComponent<Shadow>().effectColor = Empire.empires[GalaxyManager.playerID].empireColor;
            creditsText.text = GetResourceString(Empire.empires[GalaxyManager.playerID].credits);
            creditsText.text += " +" + (int)Empire.empires[GalaxyManager.playerID].GetCreditsPerTurn();
            prescriptionsText.text = GetResourceString(Empire.empires[GalaxyManager.playerID].prescriptions);
            prescriptionsText.text += " +" + (int)Empire.empires[GalaxyManager.playerID].GetPrescriptionsPerTurn();
            scienceText.text = GetResourceString(Empire.empires[GalaxyManager.playerID].science);
            scienceText.text += " +" + (int)Empire.empires[GalaxyManager.playerID].GetSciencePerTurn();
            turnText.text = "Turn: " + GalaxyManager.turnNumber;

            timer = 0.0f;
        }
    }

    string GetResourceString(float amount)
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

    public void ToggleEmpireName()
    {
        empireNameText.gameObject.SetActive(!empireNameText.gameObject.activeInHierarchy);
    }
}
