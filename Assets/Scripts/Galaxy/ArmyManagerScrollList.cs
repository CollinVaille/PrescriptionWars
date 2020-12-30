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

    //List<GameObject> dropdownButtons = new List<GameObject>();

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
                //armyDropDownButtonScript.index = x;
                armyDropDownButtonScript.nameText.text = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[x].name;
                armyDropDownButtonScript.scrollList = this;
                armyDropDownButtonScript.type = ArmyManagementScrollListButton.ArmyDropDownButtonType.ArmyDropDownButton;
                armyDropDownButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

                dropdownButton.transform.localScale = new Vector3(1, 1, 1);

                //dropdownButtons.Add(dropdownButton);
            }
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {

        }
    }

    //This method needs to be completely reworked.
    public void ClickDropDownButton(int buttonSiblingIndex)
    {
        ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(buttonSiblingIndex).GetComponent<ArmyManagementScrollListButton>();

        if (!scrollListButton.isDropdownButton)
        {
            Debug.Log("Something went wrong. You are attempting to run dropdown button logic on a button that is not a dropdown.");
            return;
        }

        if (!scrollListButton.expanded)
        {
            int requiredID = scrollListButton.GetDataIndex();
            for(int siblingIndex = buttonSiblingIndex + 1; siblingIndex <= buttonSiblingIndex + GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[requiredID].squads.Count; siblingIndex++)
            {
                GameObject childButton;

                switch (ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                {
                    case ArmyManagementScrollListButton.ArmyDropDownButtonType.SquadChildButton:
                        childButton = Instantiate(squadChildButtonPrefab);
                        break;

                    default:
                        childButton = Instantiate(squadChildButtonPrefab);
                        break;
                }
                
                childButton.transform.SetParent(dropdownButtonParent);
                childButton.transform.SetSiblingIndex(siblingIndex);
                ArmyManagementScrollListButton childButtonScript = childButton.GetComponent<ArmyManagementScrollListButton>();

                switch (ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                {
                    case ArmyManagementScrollListButton.ArmyDropDownButtonType.SquadChildButton:
                        GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[requiredID].squads[siblingIndex - buttonSiblingIndex - 1];
                        childButton.name = squad.name;
                        childButtonScript.nameText.text = squad.name;
                        break;
                }

                childButtonScript.scrollList = this;
                childButtonScript.type = ArmyManagementScrollListButton.GetChildType(scrollListButton.type);
                childButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

                childButton.transform.localScale = new Vector3(1, 1, 1);
            }
            scrollListButton.expanded = true;
            return;
        }

        //This code will execute if the dropdown button has already been expanded.
        List<GameObject> childButtons = new List<GameObject>();
        for(int siblingIndex = buttonSiblingIndex + 1; siblingIndex < dropdownButtonParent.childCount; siblingIndex++)
        {
            GameObject buttonAtSiblingIndex = dropdownButtonParent.GetChild(siblingIndex).gameObject;

            if (buttonAtSiblingIndex.GetComponent<ArmyManagementScrollListButton>().type != ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                break;

            childButtons.Add(buttonAtSiblingIndex);
        }

        foreach(GameObject childButton in childButtons)
        {
            Destroy(childButton);
        }

        scrollListButton.expanded = false;
    }

    public void ClearScrollList()
    {
        for(int x = dropdownButtonParent.childCount - 1; x >= 0; x--)
        {
            GameObject dropdownButton = dropdownButtonParent.GetChild(x).gameObject;
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
        //Creates the list of data needed for determining how to save the data that concerns how the player dragged the button.
        List<int> indexesNeededToSaveData = new List<int>();

        //Stores the initial data location of the button that the player was dragging.
        if(!scrollListButton.isDropdownButton)
            indexesNeededToSaveData.Add(scrollListButton.GetParentButtonDataIndex());
        indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());

        //Adds all of the child buttons of the button that was being dragged into a list.
        List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();
        if (scrollListButton.isDropdownButton && scrollListButton.expanded)
            childButtons = GetChildButtons(scrollListButton);

        bool newSiblingIndexLower = false;
        int newSiblingIndex = GetNewSiblingIndex(scrollListButton);
        if (newSiblingIndex > scrollListButton.transform.GetSiblingIndex())
        {
            newSiblingIndex--;
            newSiblingIndexLower = true;
        }
        if(newSiblingIndex != scrollListButton.transform.GetSiblingIndex())
        {
            scrollListButton.transform.SetSiblingIndex(newSiblingIndex);
            for (int x = 0; x < childButtons.Count; x++)
            {
                int newSiblingIndexLowerConsiderationFactor = 0;
                if (newSiblingIndexLower)
                    newSiblingIndexLowerConsiderationFactor = -1;
                childButtons[x].transform.SetSiblingIndex(scrollListButton.transform.GetSiblingIndex() + (x + 1) + newSiblingIndexLowerConsiderationFactor);
            }

            //Stores the final data location of the button that the player was dragging.
            if(!scrollListButton.isDropdownButton)
                indexesNeededToSaveData.Add(scrollListButton.GetParentButtonDataIndex());
            indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());
            //Saves the data needed concerning the button that the player was dragging.
            SaveButtonDragData(scrollListButton.type, indexesNeededToSaveData);
        }
        else
        {
            scrollListButton.transform.position = scrollListButton.initalDragPosition;
        }
    }

    void SaveButtonDragData(ArmyManagementScrollListButton.ArmyDropDownButtonType buttonType, List<int> indexesNeededToSaveData)
    {
        if(buttonType == ArmyManagementScrollListButton.ArmyDropDownButtonType.ArmyDropDownButton)
        {
            GalaxyArmy army = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]];
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies.RemoveAt(indexesNeededToSaveData[0]);
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies.Insert(indexesNeededToSaveData[1], army);
            return;
        } 
        else if (buttonType == ArmyManagementScrollListButton.ArmyDropDownButtonType.SquadChildButton)
        {
            GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads[indexesNeededToSaveData[1]];
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads.RemoveAt(indexesNeededToSaveData[1]);
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[2]].squads.Insert(indexesNeededToSaveData[3], squad);
            return;
        }
    }

    int GetNewSiblingIndex(ArmyManagementScrollListButton scrollListButton)
    {
        for(int index = scrollListButton.transform.parent.childCount - 1; index >= 0; index--)
        {
            if(scrollListButton.transform.parent.GetChild(index).position.y > scrollListButton.transform.position.y)
            {
                ArmyManagementScrollListButton scrollListButtonAbove = scrollListButton.transform.parent.GetChild(index).gameObject.GetComponent<ArmyManagementScrollListButton>();

                if (scrollListButton.isDropdownButton)
                {
                    if (scrollListButtonAbove.isDropdownButton)
                    {
                        if (scrollListButtonAbove.expanded)
                        {
                            for(int x = index + 1; x < scrollListButton.transform.parent.childCount; x++)
                            {
                                if (scrollListButton.transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>().type != ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                                    return x;
                            }
                            return index + 1;
                        }
                        else
                        {
                            return index + 1;
                        }
                    }
                    else
                    {
                        for(int x = index + 1; x < scrollListButton.transform.parent.childCount; x++)
                        {
                            if (scrollListButton.transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>().isDropdownButton)
                                return x;
                        }
                        return index + 1;
                    }
                }
                else
                {
                    if (scrollListButtonAbove.isDropdownButton)
                    {
                        if (scrollListButtonAbove.expanded)
                        {
                            return index + 1;
                        }
                        else
                        {
                            scrollListButtonAbove.ExecuteDropDownLogic();
                            for(int x = index + 1; x < scrollListButton.transform.parent.childCount; x++)
                            {
                                if (scrollListButton.transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>().isDropdownButton)
                                    return x;
                            }
                            return scrollListButton.transform.parent.childCount;
                        }
                    }
                    else
                    {
                        return index + 1;
                    }
                }
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

    public void DisbandArmy(GalaxyArmy disbandingArmy)
    {
        if (mode == ArmyManagerScrollListMode.Army)
        {
            int indexOfArmyButton = 0;

            for(int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyDropDownButtonType.ArmyDropDownButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetDataIndex()] == disbandingArmy)
                    {
                        indexOfArmyButton = x;
                        break;
                    }
                }
            }

            List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();

            for(int x = indexOfArmyButton + 1; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type != ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                    break;
                childButtons.Add(scrollListButton);
            }

            foreach(ArmyManagementScrollListButton childButton in childButtons)
            {
                Destroy(childButton.gameObject);
            }
            Destroy(dropdownButtonParent.GetChild(indexOfArmyButton).gameObject);
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {

        }
    }

    public void DisbandSquad(GalaxySquad disbandingSquad)
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            for(int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if(scrollListButton.type == ArmyManagementScrollListButton.ArmyDropDownButtonType.SquadChildButton)
                {
                    if(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentButtonDataIndex()].squads[scrollListButton.GetDataIndex()] == disbandingSquad)
                    {
                        Destroy(scrollListButton.gameObject);
                        return;
                    }
                }
            }
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {

        }
    }

    public void DisbandPill(GalaxyPill disbandingPill)
    {
        if(mode == ArmyManagerScrollListMode.Squad)
        {

        }
    }
}
