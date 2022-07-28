using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArmyManagementMenu : GalaxyPopupBehaviour, IGalaxyTooltipHandler
{
    [Header("Text Components")]

    [SerializeField] private Text titleText = null;
    [SerializeField] private Text unitInspectorGroundUnitTypeText;
    [SerializeField] private Text unitInspectorGroundUnitNameText;

    [Header("Other Components")]

    [SerializeField] private VerticalLayoutGroup unitListVerticalLayoutGroup = null;
    public VerticalLayoutGroup UnitListVerticalLayoutGroup
    {
        get
        {
            return unitListVerticalLayoutGroup;
        }
    }

    [SerializeField] private UnitListButtonDestroyer unitListButtonDestroyer = null;
    public UnitListButtonDestroyer UnitListButtonDestroyer
    {
        get
        {
            return unitListButtonDestroyer;
        }
    }

    [SerializeField] private RawImage unitListInspectorPillViewRawImage = null;
    private RawImage UnitListInspectorPillViewRawImage
    {
        get
        {
            return unitListInspectorPillViewRawImage;
        }
    }

    [Header("Parents")]

    [SerializeField] private Transform unitListButtonParent = null;
    public Transform UnitListButtonParent { get => unitListButtonParent; }

    [SerializeField] private Transform buttonsBeingDraggedParent = null;
    public Transform ButtonsBeingDraggedParent { get => buttonsBeingDraggedParent; }

    [SerializeField] private Transform unitInspectorBaseParent = null;
    [SerializeField] private Transform unitInspectorArmyParent = null;
    [SerializeField] private Transform unitInspectorSquadParent = null;
    [SerializeField] private Transform unitInspectorPillParent = null;

    [Header("Prefabs")]

    [SerializeField] private GameObject armyButtonPrefab = null;
    [SerializeField] private GameObject squadButtonPrefab = null;
    public GameObject SquadButtonPrefab { get => squadButtonPrefab; }
    [SerializeField] private GameObject pillButtonPrefab = null;
    public GameObject PillButtonPrefab { get => pillButtonPrefab; }

    [Header("SFX Options")]

    [SerializeField] private AudioClip expandAllSFX = null;
    [SerializeField] private AudioClip collapseAllSFX = null;
    [SerializeField] private AudioClip clickUnitInspectorButtonSFX = null;
    [SerializeField] private AudioClip renameGroundUnitSFX = null;
    [SerializeField] private AudioClip disbandGroundUnitSFX = null;

    [Header("Logic Options")]

    [SerializeField, Tooltip("Specifies the amount of spacing between different unit list button types in the unit list (Ex: spacing between an army button and a squad button).") ] private float spacingBetweenUnitListButtonTypes = 0;
    public float SpacingBetweenUnitListButtonTypes
    {
        get
        {
            return spacingBetweenUnitListButtonTypes;
        }
    }

    [SerializeField, Tooltip("The speed at which the pill in the unit inspector pill view rotates when the player is dragged on the unit inspector pill view.")] private float pillViewRotationSpeed = 2.5f;
    [SerializeField, Tooltip("The texture that the cursor will be whenever the mouse is over the unit inspector pill view.")] private Texture2D mouseOverPillViewCursor = null;

    [Header("Galaxy Tooltips")]

    [SerializeField] private Transform tooltipsParent = null;
    public Transform TooltipsParent { get => tooltipsParent; }

    //Non-inspector variables.

    private int planetSelectedID = -1;
    /// <summary>
    /// Indicates what planet the player is managing the armies of.
    /// Updates the title text of the menu to accurately reflect this.
    /// </summary>
    public int PlanetSelectedID
    {
        get
        {
            return planetSelectedID;
        }
        set
        {
            //Sets the selected planet to the specified planet.
            planetSelectedID = value;

            //Updates the title text of the army management menu to accurately reflect what planet the player is managing the armies of.
            titleText.text = PlanetSelected.planetName + " Army Management";
        }
    }

    /// <summary>
    /// Returns the planet that the player is managing the armies on based on the planetSelectedID.
    /// </summary>
    public GalaxyPlanet PlanetSelected
    {
        get
        {
            return GalaxyManager.planets[planetSelectedID];
        }
    }

    /// <summary>
    /// The pill view that is displayed by the pill view raw image in the unit inspector.
    /// </summary>
    private PillView unitInspectorPillView = null;
    /// <summary>
    /// Indicates the x position of the mouse when the unit inspector pill view is just starting to be dragged.
    /// </summary>
    private float initialMouseXOnUnitInspectorPillViewDrag;
    /// <summary>
    /// Indicates the rotation of the pill in the unit inspector pill view when the unit inspector pill view is just starting to be dragged.
    /// </summary>
    private float initialPillRotationOnUnitInspectorPillViewDrag;
    /// <summary>
    /// Indicates whether the texture of the cursor is not the default texture.
    /// </summary>
    private bool cursorTextureChanged = false;
    /// <summary>
    /// Indicates whether the pointer is over the unit inspector pill view.
    /// </summary>
    private bool pointerOverUnitInspectorPillView = false;
    /// <summary>
    /// Indicates whether the unit inspector pill view is being dragged.
    /// </summary>
    private bool unitInspectorPillViewBeingDragged = false;

    private UnitListButton unitListButtonSelected = null;
    /// <summary>
    /// Indicates which button in the unit list is currently selected and being displayed in the unit inspector.
    /// </summary>
    public UnitListButton UnitListButtonSelected
    {
        private get
        {
            return unitListButtonSelected;
        }
        set
        {
            //Returns if the unit list button provided is already the one selected.
            if (unitListButtonSelected == value)
                return;
            //Sets the specified unit list button as the selected unit list button.
            unitListButtonSelected = value;
            //Adjusts the unit inspector to match the unit list button selected.
            if(unitListButtonSelected == null)
            {
                //Deactivates the unit inspector effectively because no unit list button is selected.
                unitInspectorBaseParent.gameObject.SetActive(false);
                unitInspectorArmyParent.gameObject.SetActive(false);
                unitInspectorSquadParent.gameObject.SetActive(false);
                unitInspectorPillParent.gameObject.SetActive(false);
                if (unitInspectorPillView != null)
                {
                    unitInspectorPillView.Delete();
                    UnitListInspectorPillViewRawImage.texture = null;
                }
    
                return;
            }
            switch (unitListButtonSelected.TypeOfButton)
            {
                //Activates the base and army components of the unit inspector if the unit list button selected is an army button.
                case UnitListButton.ButtonType.Army:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(true);
                    unitInspectorSquadParent.gameObject.SetActive(false);
                    unitInspectorPillParent.gameObject.SetActive(false);

                    //Deactivates the pill view because there is not a valid pill (general) to display.
                    UnitListInspectorPillViewRawImage.gameObject.SetActive(false);

                    if (unitInspectorPillView != null)
                    {
                        unitInspectorPillView.Delete();
                        UnitListInspectorPillViewRawImage.texture = null;
                    }
                    break;
                //Activates the base and squad components of the unit inspector if the unit list button selected is a squad button.
                case UnitListButton.ButtonType.Squad:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(false);
                    unitInspectorSquadParent.gameObject.SetActive(true);
                    unitInspectorPillParent.gameObject.SetActive(false);
                    //Gets the leader of the squad (could be null if there are no pills in the squad).
                    GalaxyPill squadLeader = unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.squadLeader;
                    //Deletes/clears the pill view if the squad has no leader.
                    if (squadLeader == null)
                    {
                        //Deactivates the pill view because there is no pill to display.
                        UnitListInspectorPillViewRawImage.gameObject.SetActive(false);

                        if(unitInspectorPillView != null)
                        {
                            unitInspectorPillView.Delete();
                            UnitListInspectorPillViewRawImage.texture = null;
                        }
                    }
                    //Displays the squad leader in the unit inspector pill view.
                    else
                    {
                        //Activates the pill view because there is a valid pill (squad leader) to display.
                        UnitListInspectorPillViewRawImage.gameObject.SetActive(true);

                        if (unitInspectorPillView == null)
                            unitInspectorPillView = PillViewsManager.GetNewPillView(squadLeader);
                        else
                            unitInspectorPillView.DisplayedPill = squadLeader;
                        UnitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
                    }
                    break;
                //Activates the base and pill components of the unit inspector if the unit list button selected is a pill button.
                case UnitListButton.ButtonType.Pill:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(false);
                    unitInspectorSquadParent.gameObject.SetActive(false);
                    unitInspectorPillParent.gameObject.SetActive(true);

                    //Activates the pill view because there is a valid pill to display.
                    UnitListInspectorPillViewRawImage.gameObject.SetActive(true);

                    //Displays the pill in the unit inspector pill view.
                    if (unitInspectorPillView == null)
                        unitInspectorPillView = PillViewsManager.GetNewPillView(unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill);
                    else
                        unitInspectorPillView.DisplayedPill = unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill;
                    UnitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
                    break;

                //Deactivates the unit inspector effectively if the button type of the unit list button selected is an unknown type.
                default:
                    unitInspectorBaseParent.gameObject.SetActive(false);
                    unitInspectorArmyParent.gameObject.SetActive(false);
                    unitInspectorSquadParent.gameObject.SetActive(false);
                    unitInspectorPillParent.gameObject.SetActive(false);
                    break;
            }
            //Sets the type of the ground unit in the unit inspector.
            unitInspectorGroundUnitTypeText.text = unitListButtonSelected.TypeOfButton.ToString();
            //Sets the name of the ground unit in the unit inspector.
            unitInspectorGroundUnitNameText.text = unitListButtonSelected.AssignedGroundUnit == null ? "Error: No Ground Unit Assigned To Unit List Button" : unitListButtonSelected.AssignedGroundUnit.Name;
        }
    }

    /// <summary>
    /// The prefab that the army management menu is instantiated from.
    /// </summary>
    public static GameObject armyManagementMenuPrefab = null;

    /// <summary>
    /// List that contains all open army management menus.
    /// </summary>
    private static List<ArmyManagementMenu> armyManagementMenus = new List<ArmyManagementMenu>();

    /// <summary>
    /// Instantiates a new army management menu from the army management menu prefab and adjusts its values based on the given specifications.
    /// </summary>
    public static void CreateNewArmyManagementMenu(int planetSelectedID)
    {
        //Ensures that there won't be two army management menus open for the same planet.
        foreach(ArmyManagementMenu armyManagementMenuInList in armyManagementMenus)
        {
            if (armyManagementMenuInList.PlanetSelectedID == planetSelectedID)
                return;
        }
        //Instantiates a new army management menu from the army management menu prefab.
        GameObject armyManagementMenu = Instantiate(armyManagementMenuPrefab);
        //Sets the parent of the army management menu.
        armyManagementMenu.transform.SetParent(GalaxyManager.PopupsParent);
        //Gets the script component of the army management menu in order to edit values.
        ArmyManagementMenu armyManagementMenuScript = armyManagementMenu.GetComponent<ArmyManagementMenu>();
        //Sets the planet selected on the army management menu.
        armyManagementMenuScript.PlanetSelectedID = planetSelectedID;
        //Adds the newly created army management menu to the list of army management menus.
        armyManagementMenus.Add(armyManagementMenuScript);
    }

    public override void Awake()
    {
        base.Awake();

        //Adds this army management menu to the list that contains all army management menus.
        armyManagementMenus.Add(this);
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Populates the unit list with the appropriate army buttons.
        if(planetSelectedID >= 0 && planetSelectedID < GalaxyManager.planets.Count)
            PopulateUnitList();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// This method should be called in order to expand all scroll list buttons.
    /// </summary>
    public void ExpandAll()
    {
        //Calls the expand all function on all of the army buttons.
        foreach(UnitListButton armyButton in GetAllUnitListButtonsOfButtonType(UnitListButton.ButtonType.Army))
            armyButton.gameObject.GetComponent<ExpandableUnitListButton>().ExpandAll();

        //Plays the expand all sound effect.
        AudioManager.PlaySFX(expandAllSFX);
    }

    /// <summary>
    /// This method should be called in order to collapse all scroll list buttons.
    /// </summary>
    public void CollapseAll()
    {
        //Calls the collapse function on all of the army buttons.
        foreach (UnitListButton armyButton in GetAllUnitListButtonsOfButtonType(UnitListButton.ButtonType.Army))
            armyButton.gameObject.GetComponent<ExpandableUnitListButton>().Collapse(false);

        //Plays the collapse all sound effect.
        AudioManager.PlaySFX(collapseAllSFX);
    }

    /// <summary>
    /// Populates the unit list (scroll view / scroll list) with the appropriate army buttons.
    /// </summary>
    private void PopulateUnitList()
    {
        for(int armyIndex = 0; armyIndex < PlanetSelected.armyCount; armyIndex++)
        {
            //Instantiates the army button from the army button prefab.
            GameObject armyButton = Instantiate(armyButtonPrefab);
            //Sets the parent of the army button.
            armyButton.transform.SetParent(unitListButtonParent);
            //Resets the scale of the army button.
            armyButton.transform.localScale = Vector3.one;
            //Gets the army button script component of the army button in order to edit some values in the script.
            ArmyButton armyButtonScript = armyButton.GetComponent<ArmyButton>();
            //Assigns the appropriate army to the army button.
            armyButtonScript.Initialize(this, PlanetSelected.GetArmyAt(armyIndex));
        }
    }

    /// <summary>
    /// This method is called in order to close the army management menu and also removes the army management menu from the list of army management menus.
    /// </summary>
    public override void Close()
    {
        base.Close();

        //Unselects the currently selected unit list button (mostly to delete any active pill views).
        UnitListButtonSelected = null;
        //Removes this army management menu from the list of army management menus.
        armyManagementMenus.Remove(this);
        //Destroys the army management menu.
        Destroy(gameObject);
    }

    private List<UnitListButton> GetAllUnitListButtonsOfButtonType(UnitListButton.ButtonType buttonType)
    {
        //Creates the list store of the appropriate unit list buttons in.
        List<UnitListButton> buttonsOfButtonType = new List<UnitListButton>();
        //Finds all unit list buttons of the specified button type and adds them to the list.
        for (int siblingIndex = 0; siblingIndex < unitListButtonParent.childCount; siblingIndex++)
        {
            UnitListButton unitListButton = UnitListButtonParent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if (unitListButton.TypeOfButton == buttonType)
                buttonsOfButtonType.Add(unitListButton);
        }
        //Returns the list of buttons of the specified button type.
        return buttonsOfButtonType;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the background of the unit list is clicked on.
    /// </summary>
    public void OnClickUnitListBackground()
    {
        UnitListButtonSelected = null;
    }

    /// <summary>
    /// This method is called whenever a unit list button in the unit list is moved.
    /// </summary>
    public void OnUnitListButtonMove()
    {
        //Updates the pill view to match the correct visual representation of the pill after the selected unit list button was moved.
        if (unitInspectorPillView != null)
            unitInspectorPillView.UpdatePillView();
    }

    /// <summary>
    /// This method should be called whenever the player starts dragging on the unit inspector pill view through an event trigger and begins the pill rotation logic.
    /// </summary>
    public void OnBeginDragUnitInspectorPillView()
    {
        if(unitInspectorPillView != null)
        {
            initialMouseXOnUnitInspectorPillViewDrag = Input.mousePosition.x;
            initialPillRotationOnUnitInspectorPillViewDrag = unitInspectorPillView.PillRotation;
            //Logs that the unit inspector pill view is being dragged.
            unitInspectorPillViewBeingDragged = true;
        }
    }

    /// <summary>
    /// This method should be called whenever the player is dragging on the unit list inspector pill view through an event trigger and executes pill rotation logic.
    /// </summary>
    public void OnDragUnitInspectorPillView()
    {
        if(unitInspectorPillView != null)
        {
            unitInspectorPillView.PillRotation = initialPillRotationOnUnitInspectorPillViewDrag - ((Input.mousePosition.x - initialMouseXOnUnitInspectorPillViewDrag) * pillViewRotationSpeed);
        }
    }

    /// <summary>
    /// This method should be called whenever the player stops dragging on the unit inspector pill view through an event trigger and ensures the cursor texture is reset.
    /// </summary>
    public void OnEndDragUnitInspectorPillView()
    {
        if(unitInspectorPillView != null)
        {
            //Logs that the unit inspector pill view is no longer being dragged.
            unitInspectorPillViewBeingDragged = false;
            //Resets the texture of the cursor if the pointer is not over the unit inspector pill view.
            if (!pointerOverUnitInspectorPillView)
            {
                //Resets the cursor texture.
                GeneralHelperMethods.ResetCursorTexture();
                //Logs that the cursor texture has been reset.
                cursorTextureChanged = false;
            }
        }
    }

    /// <summary>
    /// This method should be called whenever the pointer enters the unit inspector pill view through an event trigger and changes the cursor texture.
    /// </summary>
    public void OnPointerEnterUnitInspectorPillView()
    {
        //Changes the cursor texture.
        Cursor.SetCursor(mouseOverPillViewCursor, new Vector2(0, 10), CursorMode.Auto);
        //Logs that the cursor texture was changed.
        cursorTextureChanged = true;
        //Logs that the cursor is now over the unit inspector pill view.
        pointerOverUnitInspectorPillView = true;
    }

    /// <summary>
    /// This method should be called whenever the pointer exits the unit inspector pill view through an event trigger and resets the cursor texture.
    /// </summary>
    public void OnPointerExitUnitInspectorPillView()
    {
        //Resets the texture of the cursor if the pill unit inspector pill view is not being dragged.
        if (!unitInspectorPillViewBeingDragged)
        {
            //Resets the cursor texture.
            GeneralHelperMethods.ResetCursorTexture();
            //Logs that the cursor texture has been reset.
            cursorTextureChanged = false;
        }

        //Logs that the cursor is now no longer over the unit inspector pill view.
        pointerOverUnitInspectorPillView = false;
    }

    private void OnDisable()
    {
        //Ensures that the cursor texture is the default cursor texture.
        if (cursorTextureChanged)
        {
            //Resets the cursor texture.
            GeneralHelperMethods.ResetCursorTexture();
            //Logs that the cursor texture has been reset.
            cursorTextureChanged = false;
        }
        //Logs that the cursor is now no longer over the unit inspector pill view.
        pointerOverUnitInspectorPillView = false;
        //Logs that the unit inspector pill view is no longer being dragged.
        unitInspectorPillViewBeingDragged = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        //Ensures that the cursor texture is the default cursor texture.
        if (cursorTextureChanged)
        {
            //Resets the cursor texture.
            GeneralHelperMethods.ResetCursorTexture();
            //Logs that the cursor texture has been reset.
            cursorTextureChanged = false;
        }
        //Unselects the currently selected unit list button (mostly to delete any active pill views).
        UnitListButtonSelected = null;
        //Removes this army management menu from the list of army management menus.
        armyManagementMenus.Remove(this);
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the rename button in the base section of the unit inspector is clicked.
    /// </summary>
    public void OnClickRenameButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmRenamingActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the rename button in the unit inspector and confirms that the player wants to rename the selected ground unit.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmRenamingActionCoroutine()
    {
        if(UnitListButtonSelected.TypeOfButton != UnitListButton.ButtonType.Squad)
        {
            //Creates the confirmation popup.
            GalaxyInputFieldConfirmationPopup confirmationPopupScript = Instantiate(GalaxyInputFieldConfirmationPopup.galaxyInputFieldConfirmationPopupPrefab).GetComponent<GalaxyInputFieldConfirmationPopup>();
            string topText = "Rename " + UnitListButtonSelected.TypeOfButton.ToString();
            confirmationPopupScript.CreateConfirmationPopup(topText);
            confirmationPopupScript.SetPlaceHolderText(UnitListButtonSelected.AssignedGroundUnit.Name);

            //Waits until the player has confirmed or cancelled the action.
            yield return new WaitUntil(confirmationPopupScript.IsAnswered);

            //If the player confirmed their action, it carries out the logic behind it.
            if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
                RenameSelectedGroundUnit(confirmationPopupScript.GetInputFieldText());

            //Destroys the confirmation popup.
            confirmationPopupScript.DestroyConfirmationPopup();
        }
        else
        {
            //Creates the confirmation popup.
            GalaxyDropdownConfirmationPopup confirmationPopupScript = Instantiate(GalaxyDropdownConfirmationPopup.galaxyDropdownConfirmationPopupPrefab).GetComponent<GalaxyDropdownConfirmationPopup>();
            string topText = "Rename " + UnitListButtonSelected.TypeOfButton.ToString();
            confirmationPopupScript.CreateConfirmationPopup(topText);

            //Adds the player empire's valid squad names as dropdown options.
            foreach(string squadName in PlanetSelected.owner.validSquadNames)
            {
                confirmationPopupScript.AddDropdownOption(squadName);
                //Makes the squad's current name the preselected option.
                if (squadName.Equals(UnitListButtonSelected.AssignedGroundUnit.Name))
                    confirmationPopupScript.SetDropdownOptionSelected(squadName);
            }

            //Waits until the player has confirmed or cancelled the action.
            yield return new WaitUntil(confirmationPopupScript.IsAnswered);

            //If the player confirmed their action, it carries out the logic behind it.
            if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
                RenameSelectedGroundUnit(confirmationPopupScript.GetReturnValue());

            //Destroys the confirmation popup.
            confirmationPopupScript.DestroyConfirmationPopup();
        }
    }

    /// <summary>
    /// This method should be called in order to rename the selected ground unit and update the selected unit list button's text.
    /// </summary>
    /// <param name="newName"></param>
    private void RenameSelectedGroundUnit(string newName)
    {
        //Sets the selected ground unit's name.
        UnitListButtonSelected.AssignedGroundUnit.Name = newName;
        //Updates the selected unit list button to accurately reflect the new name of the ground unit.
        UnitListButtonSelected.UpdateInfo();
        //Updates the unit inspector to accurately reflect the new name of the ground unit.
        unitInspectorGroundUnitNameText.text = UnitListButtonSelected.AssignedGroundUnit.Name;

        //Plays the sound effect for renaming a ground unit.
        AudioManager.PlaySFX(renameGroundUnitSFX);
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the disband button in the base section of the unit inspector is clicked.
    /// </summary>
    public void OnClickDisbandButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmDisbandingActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the disband button in the unit inspector and confirms that the player wants to disband the selected ground unit.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmDisbandingActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        string topText = "Disband " + UnitListButtonSelected.TypeOfButton.ToString();
        string bodyText = "Are you sure that you want to disband " + UnitListButtonSelected.AssignedGroundUnit.Name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            DisbandSelectedGroundUnit();

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called in order to disband the selected ground unit and remove it and all child buttons from the unit list.
    /// </summary>
    private void DisbandSelectedGroundUnit()
    {
        UnitListButtonSelected.DisbandAssignedGroundUnit();

        //Plays the sound effect for disbanding a ground unit.
        AudioManager.PlaySFX(disbandGroundUnitSFX);
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the change assigned pill skin button in the army section of the unit inspector is clicked.
    /// </summary>
    public void OnClickChangeAssignedPillSkinButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmChangingAssignedPillSkinActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the change assigned pill skin button in the unit inspector and confirms that the player wants to change the selected ground unit's assigned pill skin.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmChangingAssignedPillSkinActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyPillSkinConfirmationPopup confirmationPopupScript = Instantiate(GalaxyPillSkinConfirmationPopup.galaxyPillSkinConfirmationPopupPrefab).GetComponent<GalaxyPillSkinConfirmationPopup>();
        string topText = "Change " + UnitListButtonSelected.TypeOfButton.ToString() + "'s Pill Skin";
        string bodyText = "Are you sure that you want to change the assigned pill skin for " + UnitListButtonSelected.AssignedGroundUnit.Name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, PlanetSelected.owner.pillSkinNames);
        confirmationPopupScript.SetPillSkinSelected(UnitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.assignedPillSkinName);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            ChangeSelectedGroundUnitAssignedPillSkin(confirmationPopupScript.ReturnValue);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called in order to change the assigned pill skin of the ground unit selected in the unit list and being viewed in the unit inspector.
    /// </summary>
    /// <param name="pillSkinName"></param>
    private void ChangeSelectedGroundUnitAssignedPillSkin(string pillSkinName)
    {
        //Checks that the unit list button selected is an army button.
        if(UnitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Army)
        {
            //Changes the assigned pill skin of the selected unit list button's ground unit to the pill skin specified.
            UnitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.assignedPillSkinName = pillSkinName;
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the assigned pill skin for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Assigned Pill Skin Logic Not Implemented For Unit List Buttons of Type: " + UnitListButtonSelected.TypeOfButton.ToString() + ".");
        }
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the change icon color button in the squad section of the unit inspector is clicked.
    /// </summary>
    public void OnClickChangeIconColor()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmChangingIconColorActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the change icon color button in the unit inspector and confirms that the player wants to change the selected ground unit's icon color.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmChangingIconColorActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyColorPickerConfirmationPopup confirmationPopupScript = Instantiate(GalaxyColorPickerConfirmationPopup.galaxyColorPickerConfirmationPopupPrefab).GetComponent<GalaxyColorPickerConfirmationPopup>();
        string topText = "Change " + UnitListButtonSelected.AssignedGroundUnit.Name + "'s Icon Color";
        string bodyText = "Are you sure that you want to change the icon color for " + UnitListButtonSelected.AssignedGroundUnit.Name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, UnitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.iconColor);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            ChangeSelectedGroundUnitIconColor(confirmationPopupScript.ColorSelected);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called in order to change the icon color of the ground unit selected in the unit list and being viewed in the unit inspector.
    /// </summary>
    /// <param name="newIconColor"></param>
    private void ChangeSelectedGroundUnitIconColor(Color newIconColor)
    {
        //Checks that the unit list button selected is a squad button.
        if (UnitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Squad)
        {
            //Gets the squad button component of the selected unit list button.
            SquadButton squadButtonSelected = UnitListButtonSelected.gameObject.GetComponent<SquadButton>();
            //Changes the actual squad icon color.
            squadButtonSelected.AssignedSquad.iconColor = newIconColor;
            //Updates the squad button to display the new icon color.
            squadButtonSelected.UpdateInfo();
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the icon color for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Icon Color Logic Not Implemented For Unit List Buttons of Type: " + UnitListButtonSelected.TypeOfButton.ToString() + ".");
        }
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the change army icon button in the army section of the unit inspector is clicked.
    /// </summary>
    public void OnClickChangeArmyIcon()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmChangingArmyIconActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the change army icon button in the unit inspector and confirms that the player wants to change the selected ground unit's army icon.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmChangingArmyIconActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxySpritePickerConfirmationPopup confirmationPopupScript = Instantiate(GalaxySpritePickerConfirmationPopup.galaxySpritePickerConfirmationPopupPrefab).GetComponent<GalaxySpritePickerConfirmationPopup>();
        string topText = "Change " + UnitListButtonSelected.AssignedGroundUnit.Name + "'s Army Icon";
        string bodyText = "Are you sure that you want to change the army icon for " + UnitListButtonSelected.AssignedGroundUnit.Name + "?";
        //Fills up an array of sprites with all of the possible army icon sprites before creating the confirmation popup.
        Sprite[] armyIcons = new Sprite[ArmyIconNamesLoader.armyIconNames.Length];
        for (int armyIconIndex = 0; armyIconIndex < armyIcons.Length; armyIconIndex++)
        {
            armyIcons[armyIconIndex] = Resources.Load<Sprite>("Galaxy/Army Icons/" + ArmyIconNamesLoader.armyIconNames[armyIconIndex]);
        }
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, armyIcons, new Color((192 / 255.0f), (192 / 255.0f), (192 / 255.0f), 1), ArmyIconNamesLoader.armyIconNames);

        //Army's currently selected icon is pre-selected.
        confirmationPopupScript.SetSpriteSelected(Resources.Load<Sprite>("Galaxy/Army Icons/" + UnitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.armyIcon.spriteName));

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            ChangeSelectedGroundUnitArmyIcon(confirmationPopupScript.SpriteSelectedName);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called in order to change the army icon of the ground unit selected in the unit list and being viewed in the unit inspector.
    /// </summary>
    /// <param name="newArmyIconSprite"></param>
    private void ChangeSelectedGroundUnitArmyIcon(string newArmyIconSpriteName)
    {
        //Checks that the unit list button selected is an army button.
        if (UnitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Army)
        {
            //Gets the squad button component of the selected unit list button.
            ArmyButton armyButtonSelected = UnitListButtonSelected.gameObject.GetComponent<ArmyButton>();
            //Changes the actual squad icon color.
            armyButtonSelected.AssignedArmy.armyIcon.spriteName = newArmyIconSpriteName;
            //Updates the squad button to display the new icon color.
            armyButtonSelected.UpdateInfo();
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the army icon for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Army Icon Logic Not Implemented For Unit List Buttons of Type: " + UnitListButtonSelected.TypeOfButton.ToString() + ".");
        }
    }

    /// <summary>
    /// This method should be called through an event trigger whenever the create army button at the top of the army management menu is pressed.
    /// </summary>
    public void OnClickCreateArmyButton()
    {
        //Ensures the planet selected is allowed to have another army stationed on it.
        if (!PlanetSelected.armyCountLimitReached)
        {
            //Creates a new army on the selected planet.
            PlanetSelected.AddArmy(new GalaxyArmy(PlanetSelected.owner.empireCulture.ToString() + " Army", PlanetSelected.ownerID));

            //Instantiates the army button from the army button prefab.
            GameObject armyButton = Instantiate(armyButtonPrefab);
            //Sets the parent of the army button.
            armyButton.transform.SetParent(unitListButtonParent);
            //Resets the scale of the army button.
            armyButton.transform.localScale = Vector3.one;
            //Gets the army button script component of the army button in order to edit some values in the script.
            ArmyButton armyButtonScript = armyButton.GetComponent<ArmyButton>();
            //Assigns the appropriate army to the army button.
            armyButtonScript.Initialize(this, PlanetSelected.GetArmyAt(PlanetSelected.armyCount - 1));

            //Calls for a spacing update on the button above it if such a button exists.
            if (UnitListButtonParent.childCount > 1)
                UnitListButtonParent.GetChild(UnitListButtonParent.childCount - 2).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
        }
        else
        {
            //Informs the player that the attempt to create a new army on the selected planet was a failure due to the max army count already having been reached on the planet selected.
            StartCoroutine(ConfirmArmyCreationFailureDueToMaxArmyCount());
        }
    }

    /// <summary>
    /// This coroutine should be started whenever the player attempts to create a new army when the planet selected has already reached its max army count and ensures the player acknowledges the max army count has been reached.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmArmyCreationFailureDueToMaxArmyCount()
    {
        //Creates the confirmation popup to confirm the user acknowledges the failure to create an army.
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Army Creation Failure", PlanetSelected.planetName + " has already reached the maximum number of armies allowed to be stationed on the planet. Delete or move an existing army to a different planet in order to make room for creating a new army on this planet.", true);

        //Waits until the player has answered the confirmation popup and acknowledged the failure to create a new army.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called whenever the assign squad leader button is clicked in the unit inspector (pill button unit inspector only).
    /// </summary>
    public void OnClickAssignSquadLeaderButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmAssigningSquadLeaderActionCoroutine());
    }

    /// <summary>
    /// This coroutine should be started whenenever the player attempts to assign a pill in a squad as the squad leader and confirms that they wish for this action to be carried through.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmAssigningSquadLeaderActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Assign Squad Leader", UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader ? "Pill selected is already the leader of their assigned squad." : "Are you sure that you want to assign the pill " + UnitListButtonSelected.AssignedGroundUnit.Name + " as the squad leader of " + UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad.Name + "?", UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader);

        //Waits until the player has answered the confirmation popup.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //Assigns the selected pill as the squad leader if needed.
        if (!UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader && confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad.squadLeader = UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill;
            SquadButton parentSquadButton = null;
            for(int siblingIndex = UnitListButtonSelected.transform.GetSiblingIndex(); siblingIndex >= 0; siblingIndex--)
            {
                UnitListButton buttonAtSiblingIndex = UnitListButtonParent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                if(buttonAtSiblingIndex.TypeOfButton == UnitListButton.ButtonType.Squad && buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>().AssignedSquad == UnitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad)
                {
                    parentSquadButton = buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>();
                    break;
                }
            }
            if(parentSquadButton != null)
            {
                parentSquadButton.UpdateInfo();
            }
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called whenever the assign general button is clicked in the unit inspector (army button unit inspector only).
    /// </summary>
    public void OnClickAssignGeneralButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);


    }

    /// <summary>
    /// Closes all army management menus and removes them from the static list.
    /// </summary>
    public static void CloseAll()
    {
        if (armyManagementMenus == null || armyManagementMenus.Count == 0)
            return;
        for(int menuIndex = armyManagementMenus.Count - 1; menuIndex >= 0; menuIndex--)
        {
            armyManagementMenus[menuIndex].Close();
        }
    }
}