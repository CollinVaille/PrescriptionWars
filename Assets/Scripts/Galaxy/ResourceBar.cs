using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    public Image flagBackground;
    public Image flagSymbol;

    public GameObject empireNameText;
    public GameObject creditsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        flagSymbol.sprite = GalaxyManager.flagSymbols[Empire.empires[GalaxyManager.playerID].empireFlag.symbolSelected];
        flagBackground.color = new Color(Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.x, Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.y, Empire.empires[GalaxyManager.playerID].empireFlag.backgroundColor.z, 1.0f);
        flagSymbol.color = new Color(Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.x, Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.y, Empire.empires[GalaxyManager.playerID].empireFlag.symbolColor.z, 1.0f);
        empireNameText.GetComponent<Text>().text = Empire.empires[GalaxyManager.playerID].empireName;
        empireNameText.GetComponent<Shadow>().effectColor = Empire.empires[GalaxyManager.playerID].GetEmpireColor();
        creditsText.GetComponent<Text>().text = GetCreditsString();
    }

    string GetCreditsString()
    {
        string creditsString = "";

        if(Empire.empires[GalaxyManager.playerID].credits < 1000)
        {
            creditsString = "" + (int)Empire.empires[GalaxyManager.playerID].credits;
        }
        else if(Empire.empires[GalaxyManager.playerID].credits >= 1000 && Empire.empires[GalaxyManager.playerID].credits < 10000)
        {
            creditsString = "" + ((int)(Empire.empires[GalaxyManager.playerID].credits / 10)) / 100.0f + "K";
        }
        else if(Empire.empires[GalaxyManager.playerID].credits >= 10000 && Empire.empires[GalaxyManager.playerID].credits < 100000)
        {
            creditsString = "" + ((int)(Empire.empires[GalaxyManager.playerID].credits / 100)) / 10.0f + "K";
        }
        else if(Empire.empires[GalaxyManager.playerID].credits >= 100000 && Empire.empires[GalaxyManager.playerID].credits < 1000000)
        {
            creditsString = "" + (int)(Empire.empires[GalaxyManager.playerID].credits / 1000) + "K";
        }
        else if(Empire.empires[GalaxyManager.playerID].credits >= 1000000 && Empire.empires[GalaxyManager.playerID].credits < 10000000)
        {
            creditsString = "" + (int)(Empire.empires[GalaxyManager.playerID].credits / 10000) / 100.0f + "M";
        }
        else
        {
            creditsString = "" + Empire.empires[GalaxyManager.playerID].credits;
        }

        creditsString += " +" + (int)Empire.empires[GalaxyManager.playerID].GetCreditsPerTurn();

        return creditsString;
    }

    public void ToggleEmpireName()
    {
        empireNameText.SetActive(!empireNameText.activeInHierarchy);
    }
}
