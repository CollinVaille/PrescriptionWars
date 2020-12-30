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

    //Middle section declarations.
    //-----------------------------------------------------------------------------------
    public GameObject middleSection;

    public AudioClip disbandUnitSFX;

    public Text groundUnitNameText;

    GalaxyArmy armySelected;
    GalaxySquad squadSelected;
    GalaxyPill pillSelected;

    public enum GalaxyGroundUnitType
    {
        None,
        Army,
        Squad,
        Pill
    }
    GalaxyGroundUnitType groundUnitTypeSelected;

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

        //Executes the logic of the base class for a popup opening.
        base.Open();

        //Sets the color of the menu's foreground background image to the player empire's color.
        backgroundColorImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

        //Sets the planet name text at the top of the menu to the name of the planet that the player has selected to manage the armies on.
        planetNameText.text = GalaxyManager.planets[planetSelected].nameLabel.text;

        //Populates each scroll list with what information they need to display.
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.PopulateScrollList();
        }

        //Deactivates the middle section of the army management menu.
        SetGroundUnitTypeSelected(GalaxyGroundUnitType.None);
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

    void SetGroundUnitTypeSelected(GalaxyGroundUnitType groundUnitType)
    {
        groundUnitTypeSelected = groundUnitType;

        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.None:
                middleSection.SetActive(false);
                groundUnitNameText.text = "";
                break;
            case GalaxyGroundUnitType.Army:
                if (armySelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = armySelected.name;
                }
                break;
            case GalaxyGroundUnitType.Squad:
                if(squadSelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = squadSelected.name;
                }
                break;
            case GalaxyGroundUnitType.Pill:
                if(pillSelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = pillSelected.name;
                }
                break;
        }
    }

    public void SetArmySelected(GalaxyArmy army)
    {
        armySelected = army;
        SetGroundUnitTypeSelected(GalaxyGroundUnitType.Army);
    }

    public void SetSquadSelected(GalaxySquad squad)
    {
        squadSelected = squad;
        SetGroundUnitTypeSelected(GalaxyGroundUnitType.Squad);
    }

    public void SetPillSelected(GalaxyPill pill)
    {
        pillSelected = pill;
        SetGroundUnitTypeSelected(GalaxyGroundUnitType.Pill);
    }

    public void ClickDisbandButton()
    {
        StartCoroutine(ConfirmDisbandingAction());
    }

    IEnumerator ConfirmDisbandingAction()
    {
        GameObject confirmationPopup = Instantiate(GalaxyManager.galaxyConfirmationPopupPrefab);
        GalaxyConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyConfirmationPopup>();
        string topText = "Disband " + groundUnitTypeSelected.ToString();
        string bodyText = "Are you sure that you want to disband ";
        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.Army:
                bodyText += armySelected.name + "?";
                break;
            case GalaxyGroundUnitType.Squad:
                bodyText += squadSelected.name + "?";
                break;
            case GalaxyGroundUnitType.Pill:
                bodyText += pillSelected.name + "?";
                break;

            default:
                Debug.Log("Invalid ground unit type selected, see the ConfirmDisbandingAction() method in the ArmyManagementMenu class.");
                break;
        }
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if(confirmationPopupScript.answer == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
        {
            DisbandSelectedGroundUnit();
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    void DisbandSelectedGroundUnit()
    {
        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.Army:
                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                    scrollList.DisbandArmy(armySelected);

                for(int x = 0; x < GalaxyManager.planets[planetSelected].armies.Count; x++)
                {
                    if (GalaxyManager.planets[planetSelected].armies[x] == armySelected)
                    {
                        GalaxyManager.planets[planetSelected].armies.RemoveAt(x);
                        break;
                    }
                }

                break;
            case GalaxyGroundUnitType.Squad:
                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                    scrollList.DisbandSquad(squadSelected);

                bool squadDeleted = false;
                foreach(GalaxyArmy army in GalaxyManager.planets[planetSelected].armies)
                {
                    for(int x = 0; x < army.squads.Count; x++)
                    {
                        if(army.squads[x] == squadSelected)
                        {
                            army.squads.RemoveAt(x);
                            squadDeleted = true;
                            break;
                        }
                    }
                    if (squadDeleted)
                        break;
                }

                break;
            case GalaxyGroundUnitType.Pill:
                foreach (ArmyManagerScrollList scrollList in armyManagerScrollLists)
                    scrollList.DisbandPill(pillSelected);

                bool pillDeleted = false;
                foreach(GalaxyArmy army in GalaxyManager.planets[planetSelected].armies)
                {
                    foreach(GalaxySquad squad in army.squads)
                    {
                        for(int x = 0; x < squad.pills.Count; x++)
                        {
                            if(squad.pills[x] == pillSelected)
                            {
                                squad.pills.RemoveAt(x);
                                pillDeleted = true;
                                break;
                            }
                        }
                        if (pillDeleted)
                            break;
                    }
                    if (pillDeleted)
                        break;
                }

                break;

            default:
                Debug.Log("Invalid ground unit type, see the DisbandSelectedGroundUnit() method in the ArmyManagementMenu class.");
                break;
        }
        PlayDisbandUnitSFX();

        SetGroundUnitTypeSelected(GalaxyGroundUnitType.None);
    }

    void PlayDisbandUnitSFX()
    {
        if(disbandUnitSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(disbandUnitSFX);
    }
}
