using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : GalaxyPopupBehaviour
{
    [Header("Army Management Menu Image Components")]

    [SerializeField]
    private Image backgroundColorImage = null;

    [Header("Army Management Menu Raw Image Components")]

    [SerializeField]
    private RawImage pillRawImage = null;

    [Header("Army Management Menu Text Components")]

    [SerializeField]
    private Text planetNameText = null;
    [SerializeField]
    private Text groundUnitNameText = null;

    [Header("Army Management Menu Other Components")]

    [SerializeField]
    private List<ArmyManagerScrollList> armyManagerScrollLists = new List<ArmyManagerScrollList>();

    [SerializeField]
    private GameObject middleSection = null;

    [SerializeField]
    private Slider pillRotationSlider = null;

    [Header("Army Management Menu SFX Options")]

    [SerializeField]
    private AudioClip disbandUnitSFX = null;
    [SerializeField]
    private AudioClip mouseOverMiddleButtonSFX = null;
    [SerializeField]
    private AudioClip mouseClickMiddleButtonSFX = null;
    [SerializeField]
    private AudioClip renameSFX = null;

    [Header("Additional Information")]

    [ReadOnly] public int planetSelected = 0;

    //Non-inspector variables.

    public static ArmyManagementMenu armyManagementMenu;

    private GalaxyArmy armySelected = null;
    private GalaxySquad squadSelected = null;
    private GalaxyPill pillSelected = null;

    private PillView pillView = null;

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

    public override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void Open()
    {
        //Test code.
        GalaxyPill bob = new GalaxyPill("Bob", new PillClass("Test", PillClassType.Assault, PillType.Bot1, Resources.Load<GameObject>("Planet/Gear/Head Gear/Goggles"), Resources.Load<GameObject>("Planet/Gear/Body Gear/Utility Backpack"), Resources.Load<GameObject>("Items/Laser Rifle"), Resources.Load<GameObject>("Items/WD-40")));
        GalaxySquad deltaSquad = new GalaxySquad("Delta Squad");
        deltaSquad.AddPill(bob);
        GalaxyArmy armyOfTheSouth = new GalaxyArmy("Army " + (GalaxyManager.planets[planetSelected].armies.Count + 1), GalaxyManager.PlayerID);
        armyOfTheSouth.AddSquad(deltaSquad);
        GalaxyManager.planets[planetSelected].armies.Add(armyOfTheSouth);

        //Executes the logic of the base class for a popup opening.
        base.Open();

        //Sets the color of the menu's foreground background image to the player empire's color.
        backgroundColorImage.color = Empire.empires[GalaxyManager.PlayerID].empireColor;

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
                if(pillView != null)
                    pillView.Delete();
                pillRawImage.texture = null;
                pillRawImage.color = Color.clear;
                pillRotationSlider.gameObject.SetActive(false);
                ResetPillRotationSliderValue();
                break;
            case GalaxyGroundUnitType.Army:
                if (armySelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = armySelected.Name;
                    if (pillView != null)
                        pillView.Delete();
                    pillRawImage.texture = null;
                    pillRawImage.color = Color.clear;
                    pillRotationSlider.gameObject.SetActive(false);
                    ResetPillRotationSliderValue();
                }
                break;
            case GalaxyGroundUnitType.Squad:
                if(squadSelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = squadSelected.Name;
                    if (pillView != null)
                        pillView.Delete();
                    pillRawImage.texture = null;
                    pillRawImage.color = Color.clear;
                    pillRotationSlider.gameObject.SetActive(false);
                    ResetPillRotationSliderValue();
                }
                break;
            case GalaxyGroundUnitType.Pill:
                if(pillSelected != null)
                {
                    middleSection.SetActive(true);
                    groundUnitNameText.text = pillSelected.Name;
                    if (pillView != null)
                        pillView.Delete();
                    pillView = PillViewsManager.GetNewPillView(pillSelected);
                    pillRawImage.texture = pillView.RenderTexture;
                    pillRawImage.color = Color.white;
                    pillRotationSlider.gameObject.SetActive(true);
                    ResetPillRotationSliderValue();
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
        GameObject confirmationPopup = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab);
        GalaxyConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyConfirmationPopup>();
        string topText = "Disband " + GeneralHelperMethods.GetEnumText(groundUnitTypeSelected.ToString());
        string bodyText = "Are you sure that you want to disband ";
        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.Army:
                bodyText += armySelected.Name + "?";
                break;
            case GalaxyGroundUnitType.Squad:
                bodyText += squadSelected.Name + "?";
                break;
            case GalaxyGroundUnitType.Pill:
                bodyText += pillSelected.Name + "?";
                break;

            default:
                Debug.Log("Invalid ground unit type selected, see the ConfirmDisbandingAction() method in the ArmyManagementMenu class.");
                break;
        }
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if(confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
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
                    for(int x = 0; x < army.TotalNumberOfSquads; x++)
                    {
                        if(army.GetSquadAt(x) == squadSelected)
                        {
                            army.RemoveSquad(army.GetSquadAt(x));
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
                    for(int squadIndex = 0; squadIndex < army.TotalNumberOfSquads; squadIndex++)
                    {
                        for(int x = 0; x < army.GetSquadAt(squadIndex).TotalNumberOfPills; x++)
                        {
                            if(army.GetSquadAt(squadIndex).GetPillAt(x) == pillSelected)
                            {
                                army.GetSquadAt(squadIndex).RemovePill(army.GetSquadAt(squadIndex).GetPillAt(x));
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

    public void ClickRenameButton()
    {
        StartCoroutine(ConfirmRenamingAction());
    }

    IEnumerator ConfirmRenamingAction()
    {
        if(groundUnitTypeSelected != GalaxyGroundUnitType.Squad)
        {
            GameObject confirmationPopup = Instantiate(GalaxyInputFieldConfirmationPopup.galaxyInputFieldConfirmationPopupPrefab);
            GalaxyInputFieldConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyInputFieldConfirmationPopup>();
            string topText = "Rename " + GeneralHelperMethods.GetEnumText(groundUnitTypeSelected.ToString());
            confirmationPopupScript.CreateConfirmationPopup(topText);
            confirmationPopupScript.SetCharacterLimit(41);
            confirmationPopupScript.SetPlaceHolderText("Enter new " + GeneralHelperMethods.GetEnumText(groundUnitTypeSelected.ToString()) + " name...");
            switch (groundUnitTypeSelected)
            {
                case GalaxyGroundUnitType.Army:
                    confirmationPopupScript.SetInputFieldText(armySelected.Name);
                    break;
                case GalaxyGroundUnitType.Pill:
                    confirmationPopupScript.SetInputFieldText(pillSelected.Name);
                    break;

                default:
                    Debug.Log("Invalid ground unit type selected, see the ConfirmRenamingAction() method in the ArmyManagementMenu class for details.");
                    break;
            }

            yield return new WaitUntil(confirmationPopupScript.IsAnswered);

            if(confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            {
                RenameSelectedGroundUnit(confirmationPopupScript.GetInputFieldText());
            }

            confirmationPopupScript.DestroyConfirmationPopup();
        }
        else
        {
            GameObject confirmationPopup = Instantiate(GalaxyDropdownConfirmationPopup.galaxyDropdownConfirmationPopupPrefab);
            GalaxyDropdownConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyDropdownConfirmationPopup>();
            string topText = "Rename " + GeneralHelperMethods.GetEnumText(groundUnitTypeSelected.ToString());
            confirmationPopupScript.CreateConfirmationPopup(topText);
            foreach(string validSquadName in GetListOfValidSquadNames())
            {
                confirmationPopupScript.AddDropdownOption(validSquadName);
            }
            confirmationPopupScript.SetDropdownOptionSelected(squadSelected.Name);

            yield return new WaitUntil(confirmationPopupScript.IsAnswered);

            if(confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            {
                RenameSelectedGroundUnit(confirmationPopupScript.GetReturnValue());
            }

            confirmationPopupScript.DestroyConfirmationPopup();
        }
    }

    void RenameSelectedGroundUnit(string newName)
    {
        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.Army:
                armySelected.Name = newName;

                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                {
                    scrollList.RenameArmy(armySelected);
                }
                groundUnitNameText.text = armySelected.Name;

                break;
            case GalaxyGroundUnitType.Squad:
                squadSelected.Name = newName;

                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                {
                    scrollList.RenameSquad(squadSelected);
                }
                groundUnitNameText.text = squadSelected.Name;

                break;
            case GalaxyGroundUnitType.Pill:
                pillSelected.Name = newName;

                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                {
                    scrollList.RenamePill(pillSelected);
                }
                groundUnitNameText.text = pillSelected.Name;

                break;
        }

        PlayRenameSFX();
    }

    private List<string> GetListOfValidSquadNames()
    {
        List<string> listOfValidSquadNames = new List<string>();

        listOfValidSquadNames.Add("Alpha Squad");
        listOfValidSquadNames.Add("Bravo Squad");
        listOfValidSquadNames.Add("Charlie Squad");
        listOfValidSquadNames.Add("Delta Squad");
        listOfValidSquadNames.Add("Echo Squad");
        listOfValidSquadNames.Add("Foxtrot Squad");
        listOfValidSquadNames.Add("Golf Squad");
        listOfValidSquadNames.Add("Hotel Squad");

        return listOfValidSquadNames;
    }

    public void PlayMouseOverMiddleButtonSFX()
    {
        if (mouseOverMiddleButtonSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverMiddleButtonSFX);
    }

    public void PlayMouseClickMiddleButtonSFX()
    {
        if (mouseClickMiddleButtonSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseClickMiddleButtonSFX);
    }

    void PlayRenameSFX()
    {
        if (renameSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(renameSFX);
    }

    public void AddNewSquadDropdownButton(int armyID, int squadID, ArmyManagementScrollListButton squadChildButton)
    {
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.AddSquadDropdownButton(armyID, squadID, squadChildButton);
        }
    }

    public void SiblingIndexUpdate()
    {
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.SiblingIndexUpdateNextFrame();
        }
    }

    public List<ArmyManagementScrollListButton> GetSquadDropdownButtonsWithParentDataIndex(int parentDataIndex)
    {
        List<ArmyManagementScrollListButton> squadDropdownButtonsWithParentDataIndex = new List<ArmyManagementScrollListButton>();

        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            if(scrollList.mode == ArmyManagerScrollList.ArmyManagerScrollListMode.Squad)
            {
                foreach(ArmyManagementScrollListButton scrollListButton in scrollList.GetScrollListButtonsOfTypeWithParentDataIndex(ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton, parentDataIndex))
                {
                    squadDropdownButtonsWithParentDataIndex.Add(scrollListButton);
                }
            }
        }

        return squadDropdownButtonsWithParentDataIndex;
    }

    /*public bool SquadDropdownButtonExists(int dataIndex, int parentDataIndex)
    {
        bool squadDropdownButtonExists = false;

        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            if(scrollList.mode == ArmyManagerScrollList.ArmyManagerScrollListMode.Squad)
            {
                if(scrollList.ScrollListButtonExists(ArmyManagementScrollListButton.ArmyManagementButtonType.SquadDropDownButton, dataIndex, parentDataIndex))
                {
                    squadDropdownButtonExists = true;
                    break;
                }
            }
        }

        return squadDropdownButtonExists;
    }*/

    public ArmyManagementScrollListButton GetScrollListButton(ArmyManagementScrollListButton.ArmyManagementButtonType buttonType, int dataIndex, int parentDataIndex)
    {
        foreach (ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            ArmyManagementScrollListButton scrollListButton = scrollList.GetScrollListButton(buttonType, dataIndex, parentDataIndex);
            if (scrollListButton != null)
            {
                return scrollListButton;
            }
        }

        return null;
    }

    public void OnPillRotationSliderValueChanged()
    {
        pillView.PillRotation = pillRotationSlider.value * 360;
    }

    private void ResetPillRotationSliderValue()
    {
        pillRotationSlider.SetValueWithoutNotify(0);
    }
}
