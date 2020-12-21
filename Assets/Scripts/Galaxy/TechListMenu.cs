using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechListMenu : GalaxyPopupSuper
{
    //Text that appears at the top of the tech list menu, contains the name of the tech totem that is selected.
    public Text topText;
    //Text that appears on the left hand side of the tech list menu, contains the names of all of the techs that are avaialble to be researched in the tech totem that is currently selected.
    public Text techNamesListText;
    //Text that appears in the middle of the tech list menu, contains that levels of all of the techs that are available to be researched in the tech totem that is currently selected.
    public Text techLevelsListText;
    //Text that appears on the right hand side of the tech list menu, contains that costs of all of the techs that are available to be researched in the tech totem that is currently selected.
    public Text techCostsListText;

    //Scrollbar that allows the user to scroll down the list of techs available in the selected tech totem.
    public Scrollbar scrollbar;

    //Indicates the id of the tech totem that has its tech being displayed on this menu.
    int techTotemSelected;

    //List of all of the tech list menus.
    static List<TechListMenu> techListMenus = new List<TechListMenu>();

    // Start is called before the first frame update
    public override void Start()
    {
        //Executes the super class's start logic.
        base.Start();

        //Updates the ui of the tech list menu.
        UpdateUI();
    }

    // Update is called once per frame
    public override void Update()
    {
        //Executes the super class's update logic.
        base.Update();
    }

    public override void Close()
    {
        //Executes the super class's closing logic.
        base.Close();

        techListMenus.Remove(this);
        Destroy(gameObject);
    }

    //Updates the ui of the tech list menu.
    public void UpdateUI()
    {
        techNamesListText.text = "";
        techLevelsListText.text = "";
        techCostsListText.text = "";

        topText.text = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemSelected].name;
        if (Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemSelected].techsAvailable.Count <= 10)
        {
            List<int> techListInOrder = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemSelected].GetTechsInOrderList();

            for (int x = 0; x < techListInOrder.Count; x++)
            {
                if (x > 0)
                {
                    techNamesListText.text += "\n";
                    techLevelsListText.text += "\n";
                    techCostsListText.text += "\n";
                }
                techNamesListText.text += Tech.entireTechList[techListInOrder[x]].name;
                techLevelsListText.text += Tech.entireTechList[techListInOrder[x]].level;
                techCostsListText.text += Tech.entireTechList[techListInOrder[x]].cost;
            }
        }
        else
        {
            List<float> possibleValues = GalaxyHelperMethods.GetScrollbarValueNumbers(Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemSelected].techsAvailable.Count - 9);
            int closestIndex = 0;
            for (int x = 1; x < possibleValues.Count; x++)
            {
                if (Mathf.Abs(possibleValues[x] - scrollbar.value) <= Mathf.Abs(possibleValues[closestIndex] - scrollbar.value))
                    closestIndex = x;
            }

            List<int> techListInOrder = Empire.empires[GalaxyManager.playerID].techManager.techTotems[techTotemSelected].GetTechsInOrderList();

            bool firstLoop = true;
            for (int x = closestIndex; x < closestIndex + 10; x++)
            {
                if (!firstLoop)
                {
                    techNamesListText.text += "\n";
                    techLevelsListText.text += "\n";
                    techCostsListText.text += "\n";
                }
                techNamesListText.text += Tech.entireTechList[techListInOrder[x]].name;
                techLevelsListText.text += Tech.entireTechList[techListInOrder[x]].level;
                techCostsListText.text += Tech.entireTechList[techListInOrder[x]].cost;

                firstLoop = false;
            }
        }
    }

    //Gets the variable that indicates the id of the tech totem that has its tech being displayed on this menu.
    public int GetTechTotemSelected()
    {
        return techTotemSelected;
    }

    //Sets the variable that indicates the id of the tech totem that has its tech being displayed on this menu.
    public void SetTechTotemSelected(int newTechTotemSelectedID)
    {
        techTotemSelected = newTechTotemSelectedID;
    }

    //Closes all tech list menus that are open.
    public static void CloseAllTechListMenus()
    {
        foreach(TechListMenu techListMenu in techListMenus)
        {
            techListMenu.Close();
        }
    }

    //Indicates whether a tech list menu is open or not.
    public static bool IsATechListMenuOpen()
    {
        return techListMenus.Count > 0;
    }

    //Indicates whether a tech list menu exists of the specified id's tech totem.
    public static bool IsATechListMenuOfTechTotem(int techTotemID)
    {
        foreach(TechListMenu techListMenu in techListMenus)
        {
            if (techListMenu.GetTechTotemSelected() == techTotemID)
                return true;
        }

        return false;
    }

    //Returns the tech list menu of the specified tech totem id.
    public static TechListMenu GetTechListMenuOfTechTotem(int techTotemID)
    {
        foreach(TechListMenu techListMenu in techListMenus)
        {
            if (techListMenu.GetTechTotemSelected() == techTotemID)
                return techListMenu;
        }

        return null;
    }

    //Creates a new tech list menu of the specified tech totem.
    public static void CreateNewTechListMenu(int techTotemSelected)
    {
        //Makes sure that the tech totem id that is passed in corresponds to a valid tech totem.
        if(techTotemSelected >= 0 && techTotemSelected < Empire.empires[GalaxyManager.playerID].techManager.techTotems.Count)
        {
            //If a tech list menu of the specified tech totem already exists, then it will bring that tech list menu to the front.
            if (IsATechListMenuOfTechTotem(techTotemSelected))
            {
                GetTechListMenuOfTechTotem(techTotemSelected).transform.SetAsLastSibling();
                return;
            }

            //Instantiates a new tech list menu instance from the tech list menu prefab.
            GameObject newTechListMenu = Instantiate(TechInterface.techListMenuPrefabGlobal);
            //Extracts the tech list menu script component from the new tech list menu game object.
            TechListMenu newTechListMenuScript = newTechListMenu.GetComponent<TechListMenu>();
            //Tells the new tech list menu what tech totem it is displaying the techs for.
            newTechListMenuScript.SetTechTotemSelected(techTotemSelected);
            //Sets the new tech list menu's parent to the popups game object in the research view.
            newTechListMenu.transform.SetParent(TechInterface.popupsParentGlobal);

            //Adds the new tech list menu to the list of tech list menus.
            techListMenus.Add(newTechListMenuScript);
        }
    }
}
