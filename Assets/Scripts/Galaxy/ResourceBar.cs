using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    public Image flagBackground;
    public Image flagSymbol;

    public GameObject empireNameText;

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
    }

    public void ToggleEmpireName()
    {
        empireNameText.SetActive(!empireNameText.activeInHierarchy);
    }
}
