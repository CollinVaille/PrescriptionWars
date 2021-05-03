using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : GalaxyPopupBehaviour
{
    [Header("Army Management Menu Raw Image Components")]

    [SerializeField] private RawImage pillRawImage = null;

    [Header("Army Management Menu Text Components")]

    [SerializeField] private Text planetNameText = null;
    [SerializeField] private Text groundUnitNameText = null;

    [Header("Army Management Menu Other Components")]

    [SerializeField] private List<ArmyManagerScrollList> armyManagerScrollLists = new List<ArmyManagerScrollList>();

    [SerializeField] private GameObject middleSection = null;

    [Header("Army Management Menu Middle Buttons")]

    [SerializeField] private Button renameButton = null;
    [SerializeField] private Button disbandButton = null;
    [SerializeField] private Button changeAssignedPillSkinButton = null;

    [Header("Army Management Menu SFX Options")]

    [SerializeField] private AudioClip disbandUnitSFX = null;
    [SerializeField] private AudioClip mouseOverMiddleButtonSFX = null;
    [SerializeField] private AudioClip mouseClickMiddleButtonSFX = null;
    [SerializeField] private AudioClip renameSFX = null;

    [Header("Army Management Pill View Options")]

    [SerializeField] private Texture2D mouseOverPillViewCursor = null;

    [SerializeField] private float pillViewRotationSpeed = 0;

    [Header("Additional Information")]

    [SerializeField, ReadOnly] private int planetSelected = 0;
    public int PlanetSelected
    {
        get
        {
            return planetSelected;
        }
        set
        {
            planetSelected = value;
        }
    }

    //Non-inspector variables.

    private static ArmyManagementMenu armyManagementMenu = null;
    public static ArmyManagementMenu Menu
    {
        get
        {
            return armyManagementMenu;
        }
    }

    private GalaxyArmy armySelected = null;
    private GalaxySquad squadSelected = null;
    private GalaxyPill pillSelected = null;

    private PillView pillView = null;

    private float initialMouseXOnPillViewDrag;
    private float initialPillRotationOnPillViewDrag;

    public enum GalaxyGroundUnitType
    {
        None,
        Army,
        Squad,
        Pill
    }
    GalaxyGroundUnitType groundUnitTypeSelected;

    public static void InitializeFromGalaxyGenerator(ArmyManagementMenu armyManagementMenu)
    {
        ArmyManagementMenu.armyManagementMenu = armyManagementMenu;
    }

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
        GalaxyArmy armyOfTheSouth = new GalaxyArmy("Army " + (GalaxyManager.planets[planetSelected].Armies.Count + 1), GalaxyManager.PlayerID);
        armyOfTheSouth.AddSquad(deltaSquad);
        GalaxyManager.planets[planetSelected].Armies.Add(armyOfTheSouth);

        //Executes the logic of the base class for a popup opening.
        base.Open();

        //Sets the planet name text at the top of the menu to the name of the planet that the player has selected to manage the armies on.
        planetNameText.text = GalaxyManager.planets[planetSelected].Name;

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

        //Clears each scroll list in the army management menu.
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }

        //Resets the cursor.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ClearAllScrollLists()
    {
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }
    }

    private void SetGroundUnitTypeSelected(GalaxyGroundUnitType groundUnitType)
    {
        groundUnitTypeSelected = groundUnitType;

        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.None:
                //Middle section.
                middleSection.SetActive(false);

                //Unit name.
                groundUnitNameText.text = "";

                //Pill view.
                if(pillView != null)
                    pillView.Delete();
                pillRawImage.texture = null;
                pillRawImage.gameObject.SetActive(false);
                break;
            case GalaxyGroundUnitType.Army:
                if (armySelected != null)
                {
                    //Middle section.
                    middleSection.SetActive(true);

                    //Unit name.
                    groundUnitNameText.text = armySelected.Name;

                    //Pill view.
                    if (pillView != null)
                        pillView.Delete();
                    pillRawImage.texture = null;
                    pillRawImage.gameObject.SetActive(false);

                    //Middle buttons.
                    renameButton.gameObject.SetActive(true);
                    renameButton.transform.localPosition = new Vector2(-30, renameButton.transform.localPosition.y);
                    disbandButton.gameObject.SetActive(true);
                    disbandButton.transform.localPosition = new Vector2(0, disbandButton.transform.localPosition.y);
                    changeAssignedPillSkinButton.gameObject.SetActive(true);
                    changeAssignedPillSkinButton.transform.localPosition = new Vector2(30, changeAssignedPillSkinButton.transform.localPosition.y);
                }
                break;
            case GalaxyGroundUnitType.Squad:
                if(squadSelected != null)
                {
                    //Middle section.
                    middleSection.SetActive(true);

                    //Unit name.
                    groundUnitNameText.text = squadSelected.Name;

                    //Pill view.
                    if (pillView != null)
                        pillView.Delete();
                    pillRawImage.texture = null;
                    pillRawImage.gameObject.SetActive(false);

                    //Middle buttons.
                    renameButton.gameObject.SetActive(true);
                    renameButton.transform.localPosition = new Vector2(-15, renameButton.transform.localPosition.y);
                    disbandButton.gameObject.SetActive(true);
                    disbandButton.transform.localPosition = new Vector2(15, disbandButton.transform.localPosition.y);
                    changeAssignedPillSkinButton.gameObject.SetActive(false);
                }
                break;
            case GalaxyGroundUnitType.Pill:
                if(pillSelected != null)
                {
                    //Middle section.
                    middleSection.SetActive(true);

                    //Unit name.
                    groundUnitNameText.text = pillSelected.Name;

                    //Pill view.
                    if (pillView != null)
                    {
                        pillView.DisplayedPill = pillSelected;
                        pillView.PillRotation = 0;
                    }
                    else
                    {
                        pillView = PillViewsManager.GetNewPillView(pillSelected);
                        pillRawImage.texture = pillView.RenderTexture;
                        pillRawImage.gameObject.SetActive(true);
                    }

                    //Middle buttons.
                    renameButton.gameObject.SetActive(true);
                    renameButton.transform.localPosition = new Vector2(-15, renameButton.transform.localPosition.y);
                    disbandButton.gameObject.SetActive(true);
                    disbandButton.transform.localPosition = new Vector2(15, disbandButton.transform.localPosition.y);
                    changeAssignedPillSkinButton.gameObject.SetActive(false);
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

    private void DisbandSelectedGroundUnit()
    {
        switch (groundUnitTypeSelected)
        {
            case GalaxyGroundUnitType.Army:
                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                    scrollList.DisbandArmy(armySelected);

                for(int x = 0; x < GalaxyManager.planets[planetSelected].Armies.Count; x++)
                {
                    if (GalaxyManager.planets[planetSelected].Armies[x] == armySelected)
                    {
                        GalaxyManager.planets[planetSelected].Armies.RemoveAt(x);
                        break;
                    }
                }

                break;
            case GalaxyGroundUnitType.Squad:
                foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
                    scrollList.DisbandSquad(squadSelected);

                bool squadDeleted = false;
                foreach(GalaxyArmy army in GalaxyManager.planets[planetSelected].Armies)
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
                foreach(GalaxyArmy army in GalaxyManager.planets[planetSelected].Armies)
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
        AudioManager.PlaySFX(disbandUnitSFX);
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

    private void RenameSelectedGroundUnit(string newName)
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

    public void ClickChangeAssignedPillSkinButton()
    {
        StartCoroutine(ConfirmChangingAssignedPillSkinAction());
    }

    IEnumerator ConfirmChangingAssignedPillSkinAction()
    {
        GameObject confirmationPopup = Instantiate(GalaxyPillSkinConfirmationPopup.galaxyPillSkinConfirmationPopupPrefab);
        GalaxyPillSkinConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyPillSkinConfirmationPopup>();
        string topText = "Change Assigned Pill Skin";
        string bodyText = "You are changing the assigned pill skin for " + armySelected.Name;
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, GalaxyManager.pillMaterials[Empire.empires[GalaxyManager.PlayerID].empireCulture]);
        confirmationPopupScript.SetPillSkinSelected(armySelected.AssignedPillSkin);

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            ChangeAssignedPillSkin(confirmationPopupScript.ReturnValue);
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    private void ChangeAssignedPillSkin(Material newPillSkin)
    {
        armySelected.AssignedPillSkin = newPillSkin;
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
        AudioManager.PlaySFX(mouseOverMiddleButtonSFX);
    }

    public void PlayMouseClickMiddleButtonSFX()
    {
        AudioManager.PlaySFX(mouseClickMiddleButtonSFX);
    }

    void PlayRenameSFX()
    {
        AudioManager.PlaySFX(renameSFX);
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

    public void MouseEnterPillViewRawImage()
    {
        Cursor.SetCursor(mouseOverPillViewCursor, new Vector2(0, 10), CursorMode.Auto);
    }

    public void MouseExitPillViewRawImage()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void BeginDragPillViewRawImage()
    {
        initialMouseXOnPillViewDrag = Input.mousePosition.x;
        initialPillRotationOnPillViewDrag = pillView.PillRotation;
    }

    public void DragPillViewRawImage()
    {
        pillView.PillRotation = initialPillRotationOnPillViewDrag - ((Input.mousePosition.x - initialMouseXOnPillViewDrag) * pillViewRotationSpeed);
    }
}
