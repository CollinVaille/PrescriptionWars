using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManagerScrollList : MonoBehaviour
{
    public enum ArmyManagerScrollListMode
    {
        Army,
        Squad
    }
    public ArmyManagerScrollListMode mode = ArmyManagerScrollListMode.Army;

    public GameObject armyDropdownButtonPrefab;
    public GameObject squadChildButtonPrefab;
    public Transform dropdownButtonParent;

    List<GameObject> dropdownButtons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateScrollList()
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            for(int x = 0; x < GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies.Count; x++)
            {
                GameObject dropdownButton = Instantiate(armyDropdownButtonPrefab);
                dropdownButton.transform.parent = dropdownButtonParent;
                dropdownButton.name = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[x].name;
                ArmyDropDownButton armyDropDownButtonScript = dropdownButton.GetComponent<ArmyDropDownButton>();
                armyDropDownButtonScript.index = x;
                armyDropDownButtonScript.nameText.text = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[x].name;
                armyDropDownButtonScript.scrollList = this;
                armyDropDownButtonScript.type = ArmyDropDownButton.ArmyDropDownButtonType.ArmyDropDownButton;

                dropdownButton.transform.localScale = new Vector3(1, 1, 1);

                dropdownButtons.Add(dropdownButton);
            }
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {

        }
    }

    public void ClickDropDownButton(int buttonSiblingIndex)
    {
        int armyID = 0;
        int dropdownButtonsIndex = 0;
        bool alreadyExpanded = false;
        ArmyDropDownButton.ArmyDropDownButtonType dropDownButtonType = ArmyDropDownButton.ArmyDropDownButtonType.ArmyDropDownButton;
        for(int x = 0; x < dropdownButtons.Count; x++)
        {
            if(dropdownButtons[x].transform.GetSiblingIndex() == buttonSiblingIndex)
            {
                armyID = dropdownButtons[x].GetComponent<ArmyDropDownButton>().index;
                dropdownButtonsIndex = x;
                alreadyExpanded = dropdownButtons[x].GetComponent<ArmyDropDownButton>().expanded;
                dropDownButtonType = dropdownButtons[x].GetComponent<ArmyDropDownButton>().type;
                break;
            }
        }

        if (!alreadyExpanded)
        {
            for(int siblingIndex = buttonSiblingIndex + 1; siblingIndex <= buttonSiblingIndex + GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads.Count; siblingIndex++)
            {
                GameObject squadChildButton = Instantiate(squadChildButtonPrefab);
                squadChildButton.transform.parent = dropdownButtonParent;
                squadChildButton.transform.SetSiblingIndex(siblingIndex);
                GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads[siblingIndex - buttonSiblingIndex - 1];
                squadChildButton.name = squad.name;
                ArmyDropDownButton squadChildButtonScript = squadChildButton.GetComponent<ArmyDropDownButton>();
                squadChildButtonScript.index = siblingIndex - buttonSiblingIndex - 1;
                squadChildButtonScript.nameText.text = squad.name;
                squadChildButtonScript.scrollList = this;
                squadChildButtonScript.type = ArmyDropDownButton.ArmyDropDownButtonType.SquadChildButton;

                squadChildButton.transform.localScale = new Vector3(1, 1, 1);

                dropdownButtons.Add(squadChildButton);
            }

            dropdownButtons[dropdownButtonsIndex].GetComponent<ArmyDropDownButton>().expanded = true;
        }
        else
        {
            for(int siblingIndex = buttonSiblingIndex + GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads.Count; siblingIndex > buttonSiblingIndex; siblingIndex--)
            {
                for(int x = 0; x < dropdownButtons.Count; x++)
                {
                    if(dropdownButtons[x].transform.GetSiblingIndex() == siblingIndex)
                    {
                        GameObject dropdownButton = dropdownButtons[x];
                        dropdownButtons.RemoveAt(x);
                        Destroy(dropdownButton);
                        continue;
                    }
                }
            }

            dropdownButtons[dropdownButtonsIndex].GetComponent<ArmyDropDownButton>().expanded = false;
        }
    }

    public void ClearScrollList()
    {
        for(int x = dropdownButtons.Count - 1; x >= 0; x--)
        {
            GameObject dropdownButton = dropdownButtons[x];
            dropdownButtons.RemoveAt(x);
            Destroy(dropdownButton);
        }
    }

    public void SwitchMode()
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            mode = ArmyManagerScrollListMode.Squad;
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {
            mode = ArmyManagerScrollListMode.Army;
        }

        ClearScrollList();
        PopulateScrollList();
    }
}
