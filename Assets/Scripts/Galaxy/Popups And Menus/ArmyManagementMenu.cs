using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : GalaxyPopupBehaviour
{
    public Image backgroundColorImage;

    public Text planetNameText;

    public int planetSelected;

    public static ArmyManagementMenu armyManagementMenu;

    public List<ArmyManagerScrollList> armyManagerScrollLists;

    public List<Image> scrollBarHandleImages;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Open()
    {
        //Test code.
        GalaxyPill bob = new GalaxyPill();
        bob.name = "Bob";
        bob.pillClass = GalaxyPill.PillClass.Assault;
        GalaxySquad deltaSquad = new GalaxySquad();
        deltaSquad.name = "Delta Squad";
        deltaSquad.pills.Add(bob);
        GalaxyArmy armyOfTheSouth = new GalaxyArmy();
        armyOfTheSouth.name = "Army " + (GalaxyManager.planets[planetSelected].armies.Count + 1);
        armyOfTheSouth.squads.Add(deltaSquad);
        GalaxyManager.planets[planetSelected].armies.Add(armyOfTheSouth);

        //Executes the logic of the super class for a popup opening.
        base.Open();

        //Sets the color of the menu's foreground background image to the player empire's color.
        backgroundColorImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

        //Sets the color of every scroll bar handle to the player empire's color.
        foreach(Image scrollBarHandleImage in scrollBarHandleImages)
        {
            scrollBarHandleImage.color = Empire.empires[GalaxyManager.playerID].empireColor;
        }

        //Sets the planet name text at the top of the menu to the name of the planet that the player has selected to manage the armies on.
        planetNameText.text = GalaxyManager.planets[planetSelected].nameLabel.text;

        //Populates each scroll list with what information they need to display.
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.PopulateScrollList();
        }
    }

    public override void Close()
    {
        //Executes the logic of the super class for a popup closing.
        base.Close();

        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }
    }

    public void ClearAllScrollLists()
    {
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }
    }
}
