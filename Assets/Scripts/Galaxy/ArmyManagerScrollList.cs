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
                dropdownButton.transform.SetParent(dropdownButtonParent);
                dropdownButton.name = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[x].name;
                ArmyManagementScrollListButton armyDropDownButtonScript = dropdownButton.GetComponent<ArmyManagementScrollListButton>();
                armyDropDownButtonScript.index = x;
                armyDropDownButtonScript.nameText.text = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[x].name;
                armyDropDownButtonScript.scrollList = this;
                armyDropDownButtonScript.type = ArmyManagementScrollListButton.ArmyDropDownButtonType.ArmyDropDownButton;
                armyDropDownButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

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
        ArmyManagementScrollListButton.ArmyDropDownButtonType dropDownButtonType = ArmyManagementScrollListButton.ArmyDropDownButtonType.ArmyDropDownButton;
        for(int x = 0; x < dropdownButtons.Count; x++)
        {
            if(dropdownButtons[x].transform.GetSiblingIndex() == buttonSiblingIndex)
            {
                armyID = dropdownButtons[x].GetComponent<ArmyManagementScrollListButton>().index;
                dropdownButtonsIndex = x;
                alreadyExpanded = dropdownButtons[x].GetComponent<ArmyManagementScrollListButton>().expanded;
                dropDownButtonType = dropdownButtons[x].GetComponent<ArmyManagementScrollListButton>().type;
                break;
            }
        }

        if (!alreadyExpanded)
        {
            for(int siblingIndex = buttonSiblingIndex + 1; siblingIndex <= buttonSiblingIndex + GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads.Count; siblingIndex++)
            {
                GameObject squadChildButton = Instantiate(squadChildButtonPrefab);
                squadChildButton.transform.SetParent(dropdownButtonParent);
                squadChildButton.transform.SetSiblingIndex(siblingIndex);
                GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads[siblingIndex - buttonSiblingIndex - 1];
                squadChildButton.name = squad.name;
                ArmyManagementScrollListButton squadChildButtonScript = squadChildButton.GetComponent<ArmyManagementScrollListButton>();
                squadChildButtonScript.index = siblingIndex - buttonSiblingIndex - 1;
                squadChildButtonScript.nameText.text = squad.name;
                squadChildButtonScript.scrollList = this;
                squadChildButtonScript.type = ArmyManagementScrollListButton.ArmyDropDownButtonType.SquadChildButton;
                squadChildButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

                squadChildButton.transform.localScale = new Vector3(1, 1, 1);

                dropdownButtons.Add(squadChildButton);
            }

            dropdownButtons[dropdownButtonsIndex].GetComponent<ArmyManagementScrollListButton>().expanded = true;
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

            dropdownButtons[dropdownButtonsIndex].GetComponent<ArmyManagementScrollListButton>().expanded = false;
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

    public void ButtonEndDrag(ArmyManagementScrollListButton scrollListButton)
    {
        List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();
        if (scrollListButton.isDropdownButton && scrollListButton.expanded)
            childButtons = GetChildButtons(scrollListButton);

        int newSiblingIndex = GetNewSiblingIndex(scrollListButton);
        if(newSiblingIndex != scrollListButton.transform.GetSiblingIndex())
        {
            scrollListButton.transform.SetSiblingIndex(GetNewSiblingIndex(scrollListButton));
            for (int x = 0; x < childButtons.Count; x++)
            {
                childButtons[x].transform.SetSiblingIndex(scrollListButton.transform.GetSiblingIndex() + (x + 1));
            }
        }
        else
        {
            scrollListButton.transform.position = scrollListButton.initalDragPosition;
        }
    }

    int GetNewSiblingIndex(ArmyManagementScrollListButton scrollListButton)
    {
        for(int index = scrollListButton.transform.parent.childCount - 1; index >= 0; index--)
        {
            if(scrollListButton.transform.parent.GetChild(index).position.y > scrollListButton.transform.position.y)
            {
                ArmyManagementScrollListButton scrollListButtonAbove = scrollListButton.transform.parent.GetChild(index).gameObject.GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButtonAbove.type == scrollListButton.type || scrollListButtonAbove.type == ArmyManagementScrollListButton.GetParentType(scrollListButton.type))
                {
                    if (scrollListButtonAbove.isDropdownButton && scrollListButtonAbove.expanded)
                        scrollListButtonAbove.ExecuteDropDownLogic();
                    return index + 1;
                }
                return scrollListButton.transform.GetSiblingIndex();
            }
        }

        if (scrollListButton.isDropdownButton)
            return 0;
        return scrollListButton.transform.GetSiblingIndex();
    }

    public List<ArmyManagementScrollListButton> GetChildButtons(ArmyManagementScrollListButton scrollListButton)
    {
        if (!scrollListButton.isDropdownButton)
            return null;

        ArmyManagementScrollListButton.ArmyDropDownButtonType childType = ArmyManagementScrollListButton.GetChildType(scrollListButton.type);

        List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();
        for(int index = scrollListButton.transform.GetSiblingIndex() + 1; index < scrollListButton.transform.parent.childCount; index++)
        {
            ArmyManagementScrollListButton buttonToTest = scrollListButton.transform.parent.GetChild(index).gameObject.GetComponent<ArmyManagementScrollListButton>();
            if (buttonToTest.type == childType)
            {
                childButtons.Add(buttonToTest);
                continue;
            }
            break;
        }

        return childButtons;
    }
}
