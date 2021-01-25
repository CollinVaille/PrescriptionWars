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
    public GameObject squadDropdownButtonPrefab;
    public GameObject pillChildButtonPrefab;
    public Transform dropdownButtonParent;

    bool siblingIndexUpdateNextFrame;

    //List<GameObject> dropdownButtons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (siblingIndexUpdateNextFrame)
        {
            SiblingIndexUpdate();
        }
    }

    void SiblingIndexUpdate()
    {
        for(int x = 0; x < dropdownButtonParent.childCount; x++)
        {
            ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
            scrollListButton.SiblingIndexUpdate();
        }

        siblingIndexUpdateNextFrame = false;
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
                armyDropDownButtonScript.type = ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton;
                armyDropDownButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

                dropdownButton.transform.localScale = Vector3.one;

                //dropdownButtons.Add(dropdownButton);
            }
        }
    }

    public void AddSquadDropdownButton(int armyID, int squadID, ArmyManagementScrollListButton squadChildButton)
    {
        if(mode == ArmyManagerScrollListMode.Squad)
        {
            GameObject dropdownButton = Instantiate(squadDropdownButtonPrefab);
            dropdownButton.transform.SetParent(dropdownButtonParent);
            dropdownButton.name = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads[squadID].name;
            ArmyManagementScrollListButton squadDropDownButtonScript = dropdownButton.GetComponent<ArmyManagementScrollListButton>();
            squadDropDownButtonScript.nameText.text = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[armyID].squads[squadID].name;
            squadDropDownButtonScript.scrollList = this;
            squadDropDownButtonScript.type = ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton;
            squadDropDownButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;
            squadDropDownButtonScript.AddAdditionalData(armyID);
            squadDropDownButtonScript.AddAdditionalData(squadID);

            dropdownButton.transform.localScale = Vector3.one;

            squadChildButton.AddAssignedScrolllistButton(squadDropDownButtonScript);
        }
    }

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
            int childButtonsToAdd = 0;

            if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton)
                childButtonsToAdd = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetDataIndex()].squads.Count;
            else if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
                childButtonsToAdd = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()].pills.Count;

            for (int siblingIndex = buttonSiblingIndex + 1; siblingIndex <= buttonSiblingIndex + childButtonsToAdd; siblingIndex++)
            {
                GameObject childButtonPrefab = null;

                switch (ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                {
                    case ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton:
                        childButtonPrefab = squadChildButtonPrefab;
                        break;
                    case ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton:
                        childButtonPrefab = pillChildButtonPrefab;
                        break;
                }

                GameObject childButton = Instantiate(childButtonPrefab);
                childButton.transform.SetParent(dropdownButtonParent);
                childButton.transform.SetSiblingIndex(siblingIndex);
                ArmyManagementScrollListButton childButtonScript = childButton.GetComponent<ArmyManagementScrollListButton>();
                childButtonScript.scrollList = this;
                childButtonScript.type = ArmyManagementScrollListButton.GetChildType(scrollListButton.type);
                childButtonScript.buttonImage.color = Empire.empires[GalaxyManager.playerID].empireColor;
                childButton.transform.localScale = new Vector3(1, 1, 1);

                switch (ArmyManagementScrollListButton.GetChildType(scrollListButton.type))
                {
                    case ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton:
                        GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetDataIndex()].squads[siblingIndex - buttonSiblingIndex - 1];
                        childButton.name = squad.name;
                        childButtonScript.nameText.text = squad.name;
                        ArmyManagementScrollListButton squadDropdownButton = ArmyManagementMenu.armyManagementMenu.GetScrollListButton(ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton, childButtonScript.GetDataIndex(), childButtonScript.GetParentDataIndex());
                        if (squadDropdownButton != null)
                        {
                            childButtonScript.AddAssignedScrolllistButton(squadDropdownButton);

                            childButtonScript.transferArrowImage.transform.localScale = new Vector3(childButtonScript.transferArrowImage.transform.localScale.x * -1, 1, 1);
                        }
                        break;
                    case ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton:
                        GalaxyPill pill = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()].pills[siblingIndex - buttonSiblingIndex - 1];
                        childButton.name = pill.name;
                        childButtonScript.nameText.text = pill.name;
                        break;
                }
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

        if(scrollListButton.type != ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
        {
            //Stores the initial data location of the button that the player was dragging.
            if(scrollListButton.type != ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)
            {
                if (!scrollListButton.isDropdownButton)
                    indexesNeededToSaveData.Add(scrollListButton.GetParentDataIndex());
                indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());
            }
            else
            {
                indexesNeededToSaveData.Add(dropdownButtonParent.GetChild(scrollListButton.GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex());
                indexesNeededToSaveData.Add(scrollListButton.GetParentDataIndex());
                indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());
            }
        }

        //Adds all of the child buttons of the button that was being dragged into a list.
        List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();
        if (scrollListButton.isDropdownButton && scrollListButton.expanded)
            childButtons = GetChildButtons(scrollListButton);

        bool newSiblingIndexLower = false;
        int newSiblingIndex = GetNewSiblingIndex(scrollListButton);
        int oldSiblingIndex = scrollListButton.transform.GetSiblingIndex();
        if (newSiblingIndex > oldSiblingIndex)
        {
            newSiblingIndex--;
            newSiblingIndexLower = true;
        }
        if(newSiblingIndex != oldSiblingIndex)
        {
            if (mode == ArmyManagerScrollListMode.Army)
            {
                ArmyManagementMenu.armyManagementMenu.SiblingIndexUpdate();
            }

            scrollListButton.transform.SetSiblingIndex(newSiblingIndex);
            for (int x = 0; x < childButtons.Count; x++)
            {
                int newSiblingIndexLowerConsiderationFactor = 0;
                if (newSiblingIndexLower)
                    newSiblingIndexLowerConsiderationFactor = -1;
                childButtons[x].transform.SetSiblingIndex(scrollListButton.transform.GetSiblingIndex() + (x + 1) + newSiblingIndexLowerConsiderationFactor);
            }

            if(scrollListButton.type != ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
            {
                //Stores the final data location of the button that the player was dragging.
                if(scrollListButton.type != ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)
                {
                    if (!scrollListButton.isDropdownButton)
                        indexesNeededToSaveData.Add(scrollListButton.GetParentDataIndex());
                    indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());
                }
                else
                {
                    indexesNeededToSaveData.Add(dropdownButtonParent.GetChild(scrollListButton.GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex());
                    indexesNeededToSaveData.Add(scrollListButton.GetParentDataIndex());
                    indexesNeededToSaveData.Add(scrollListButton.GetDataIndex());
                }

                //Saves the data needed concerning the button that the player was dragging.
                SaveButtonDragData(scrollListButton.type, indexesNeededToSaveData);
            }
        }
        else
        {
            scrollListButton.transform.position = scrollListButton.initalDragPosition;
        }
    }

    void SaveButtonDragData(ArmyManagementScrollListButton.ArmyManagementButtonType buttonType, List<int> indexesNeededToSaveData)
    {
        if(buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton)
        {
            GalaxyArmy army = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]];
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies.RemoveAt(indexesNeededToSaveData[0]);
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies.Insert(indexesNeededToSaveData[1], army);
            return;
        } 
        else if (buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton)
        {
            GalaxySquad squad = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads[indexesNeededToSaveData[1]];
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads.RemoveAt(indexesNeededToSaveData[1]);
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[2]].squads.Insert(indexesNeededToSaveData[3], squad);
            return;
        }
        else if (buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)
        {
            GalaxyPill pill = GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads[indexesNeededToSaveData[1]].pills[indexesNeededToSaveData[2]];
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[0]].squads[indexesNeededToSaveData[1]].pills.RemoveAt(indexesNeededToSaveData[2]);
            GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[indexesNeededToSaveData[3]].squads[indexesNeededToSaveData[4]].pills.Insert(indexesNeededToSaveData[5], pill);
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

        ArmyManagementScrollListButton.ArmyManagementButtonType childType = ArmyManagementScrollListButton.GetChildType(scrollListButton.type);

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
            ArmyManagementMenu.armyManagementMenu.SiblingIndexUpdate();

            int indexOfArmyButton = 0;

            for(int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton)
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
            foreach(GalaxySquad disbandingSquad in disbandingArmy.squads)
            {
                int indexOfSquadButton = -1;

                for(int x = 0; x < dropdownButtonParent.childCount; x++)
                {
                    ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                    if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
                    {
                        if(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()] == disbandingSquad)
                        {
                            indexOfSquadButton = x;
                            break;
                        }
                    }
                }

                if (indexOfSquadButton < 0)
                    continue;

                List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();

                for(int x = indexOfSquadButton + 1; x < dropdownButtonParent.childCount; x++)
                {
                    ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                    if (scrollListButton.type != ArmyManagementScrollListButton.GetChildType(dropdownButtonParent.GetChild(indexOfSquadButton).GetComponent<ArmyManagementScrollListButton>().type))
                        break;
                    childButtons.Add(scrollListButton);
                }

                foreach(ArmyManagementScrollListButton childButton in childButtons)
                {
                    Destroy(childButton.gameObject);
                }
                Destroy(dropdownButtonParent.GetChild(indexOfSquadButton).gameObject);
            }
        }
    }

    public void DisbandSquad(GalaxySquad disbandingSquad)
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            ArmyManagementMenu.armyManagementMenu.SiblingIndexUpdate();

            for (int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if(scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton)
                {
                    if(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()] == disbandingSquad)
                    {
                        Destroy(scrollListButton.gameObject);
                        return;
                    }
                }
            }
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {
            int indexOfSquadButton = -1;

            for (int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()] == disbandingSquad)
                    {
                        indexOfSquadButton = x;
                        break;
                    }
                }
            }

            if (indexOfSquadButton < 0)
                return;

            List<ArmyManagementScrollListButton> childButtons = new List<ArmyManagementScrollListButton>();

            for (int x = indexOfSquadButton + 1; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type != ArmyManagementScrollListButton.GetChildType(dropdownButtonParent.GetChild(indexOfSquadButton).GetComponent<ArmyManagementScrollListButton>().type))
                    break;
                childButtons.Add(scrollListButton);
            }

            foreach (ArmyManagementScrollListButton childButton in childButtons)
            {
                Destroy(childButton.gameObject);
            }
            Destroy(dropdownButtonParent.GetChild(indexOfSquadButton).gameObject);
        }
    }

    public void DisbandPill(GalaxyPill disbandingPill)
    {
        if(mode == ArmyManagerScrollListMode.Squad)
        {
            for(int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if(scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)
                {
                    if(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[dropdownButtonParent.GetChild(scrollListButton.GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex()].squads[scrollListButton.GetParentDataIndex()].pills[scrollListButton.GetDataIndex()] == disbandingPill)
                    {
                        Destroy(scrollListButton.gameObject);
                        return;
                    }
                }
            }
        }
    }

    public void RenameArmy(GalaxyArmy renamingArmy)
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            for(int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetDataIndex()] == renamingArmy)
                    {
                        scrollListButton.gameObject.name = renamingArmy.name;
                        scrollListButton.nameText.text = renamingArmy.name;
                    }
                }
            }
        }
    }

    public void RenameSquad(GalaxySquad renamingSquad)
    {
        if(mode == ArmyManagerScrollListMode.Army)
        {
            for (int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()] == renamingSquad)
                    {
                        scrollListButton.gameObject.name = renamingSquad.name;
                        scrollListButton.nameText.text = renamingSquad.name;
                        return;
                    }
                }
            }
        }
        else if (mode == ArmyManagerScrollListMode.Squad)
        {
            for (int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[scrollListButton.GetParentDataIndex()].squads[scrollListButton.GetDataIndex()] == renamingSquad)
                    {
                        scrollListButton.gameObject.name = renamingSquad.name;
                        scrollListButton.nameText.text = renamingSquad.name;
                        return;
                    }
                }
            }
        }
    }

    public void RenamePill(GalaxyPill renamingPill)
    {
        if(mode == ArmyManagerScrollListMode.Squad)
        {
            for (int x = 0; x < dropdownButtonParent.childCount; x++)
            {
                ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
                if (scrollListButton.type == ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)
                {
                    if (GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[dropdownButtonParent.GetChild(scrollListButton.GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex()].squads[scrollListButton.GetParentDataIndex()].pills[scrollListButton.GetDataIndex()] == renamingPill)
                    {
                        scrollListButton.gameObject.name = renamingPill.name;
                        scrollListButton.nameText.text = renamingPill.name;
                        return;
                    }
                }
            }
        }
    }

    public void SiblingIndexUpdateNextFrame()
    {
        for(int x = 0; x < dropdownButtonParent.childCount; x++)
        {
            dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>().SiblingIndexUpdateOccuringNextFrame();
        }

        siblingIndexUpdateNextFrame = true;
    }

    public List<ArmyManagementScrollListButton> GetScrollListButtonsOfTypeWithParentDataIndex(ArmyManagementScrollListButton.ArmyManagementButtonType type, int parentDataIndex)
    {
        if ((mode == ArmyManagerScrollListMode.Army && (type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton || type == ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)) || (mode == ArmyManagerScrollListMode.Squad && (type == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton && type == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton)))
            return new List<ArmyManagementScrollListButton>();

        List<ArmyManagementScrollListButton> scrollListButtonsOfTypeWithParentDataIndex = new List<ArmyManagementScrollListButton>();

        for(int index = 0; index < dropdownButtonParent.childCount; index++)
        {
            ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(index).GetComponent<ArmyManagementScrollListButton>();
            if (scrollListButton.type == type && scrollListButton.GetParentDataIndex() == parentDataIndex)
            {
                scrollListButtonsOfTypeWithParentDataIndex.Add(scrollListButton);
            }
        }

        return scrollListButtonsOfTypeWithParentDataIndex;
    }

    public ArmyManagementScrollListButton GetScrollListButton(ArmyManagementScrollListButton.ArmyManagementButtonType buttonType, int dataIndex, int parentDataIndex)
    {
        if ((mode == ArmyManagerScrollListMode.Army && (buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton || buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.PillChildButton)) || (mode == ArmyManagerScrollListMode.Squad && (buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.ArmyDropDownButton && buttonType == ArmyManagementScrollListButton.ArmyManagementButtonType.SquadChildButton)))
            return null;

        for(int x = 0; x < dropdownButtonParent.childCount; x++)
        {
            ArmyManagementScrollListButton scrollListButton = dropdownButtonParent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();
            if(scrollListButton.type == buttonType)
            {
                if(scrollListButton.GetDataIndex() == dataIndex && scrollListButton.GetParentDataIndex() == parentDataIndex)
                {
                    return scrollListButton;
                }
            }
        }

        return null;
    }
}
