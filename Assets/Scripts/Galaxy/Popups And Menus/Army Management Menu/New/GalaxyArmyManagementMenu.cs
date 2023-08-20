using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalaxyArmyManagementMenu : NewGalaxyPopupBehaviour, IGalaxyTooltipHandler
{
    [Header("Text Components")]

    [SerializeField] private Text titleText = null;
    [SerializeField] private Text unitInspectorGroundUnitTypeText = null;
    [SerializeField] private Text unitInspectorGroundUnitNameText = null;

    [Header("Other Components")]

    [SerializeField] private VerticalLayoutGroup _unitListVerticalLayoutGroup = null;
    [SerializeField] private UnitListButtonDestroyer _unitListButtonDestroyer = null;
    [SerializeField] private RawImage _unitListInspectorPillViewRawImage = null;

    [Header("Parents")]

    [SerializeField] private Transform _unitListButtonParent = null;
    [SerializeField] private Transform _buttonsBeingDraggedParent = null;
    [SerializeField] private Transform unitInspectorBaseParent = null;
    [SerializeField] private Transform unitInspectorArmyParent = null;
    [SerializeField] private Transform unitInspectorSquadParent = null;
    [SerializeField] private Transform unitInspectorPillParent = null;

    [Header("Prefabs")]

    [SerializeField] private GameObject armyButtonPrefab = null;
    [SerializeField] private GameObject _squadButtonPrefab = null;
    [SerializeField] private GameObject _pillButtonPrefab = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip expandAllSFX = null;
    [SerializeField] private AudioClip collapseAllSFX = null;
    [SerializeField] private AudioClip clickUnitInspectorButtonSFX = null;
    [SerializeField] private AudioClip renameGroundUnitSFX = null;
    [SerializeField] private AudioClip disbandGroundUnitSFX = null;

    [Header("Logic Options")]

    [SerializeField, Tooltip("Specifies the amount of spacing between different unit list button types in the unit list (Ex: spacing between an army button and a squad button).")] private float _spacingBetweenUnitListButtonTypes = 0;
    [SerializeField, Tooltip("The speed at which the pill in the unit inspector pill view rotates when the player is dragged on the unit inspector pill view.")] private float pillViewRotationSpeed = 2.5f;
    [SerializeField, Tooltip("The texture that the cursor will be whenever the mouse is over the unit inspector pill view.")] private Texture2D mouseOverPillViewCursor = null;

    [Header("Galaxy Tooltips")]

    [SerializeField] private Transform _tooltipsParent = null;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used in order to access the unit list vertical layout group component of the army management menu.
    /// </summary>
    public VerticalLayoutGroup unitListVerticalLayoutGroup { get => _unitListVerticalLayoutGroup; }

    /// <summary>
    /// Public property that should be used in order to access the unit list button destroyer component of the army management menu that destoys a unit list button every second or so in an organized manner.
    /// </summary>
    public UnitListButtonDestroyer unitListButtonDestroyer { get => _unitListButtonDestroyer; }

    /// <summary>
    /// Private property that should be used in order to access the unit list inspector pill view raw image of the army management menu.
    /// </summary>
    private RawImage unitListInspectorPillViewRawImage { get => _unitListInspectorPillViewRawImage; }

    /// <summary>
    /// Public property that should be used in order to access the transform that serves as the parent for all unit list buttons (army buttons, squad buttons, and pill buttons) on the army management menu.
    /// </summary>
    public Transform unitListButtonParent { get => _unitListButtonParent; }

    /// <summary>
    /// Public property that should be used in order to access the transform that serves as the parent for all unit list buttons that are being dragged by the player.
    /// </summary>
    public Transform buttonsBeingDraggedParent { get => _buttonsBeingDraggedParent; }

    /// <summary>
    /// Public property that should be used in order to access the game object that serves as the prefab that all squad buttons in the unit list on the army management menu are instantiated from.
    /// </summary>
    public GameObject squadButtonPrefab { get => _squadButtonPrefab; }

    /// <summary>
    /// Public property that should be used in order to access the game object that serves as the prefab that all pill buttons in the unit list on the army management menu are instantiated from.
    /// </summary>
    public GameObject pillButtonPrefab { get => _pillButtonPrefab; }

    /// <summary>
    /// Public property that should be used in order to access the float value that indicates the amount of spacing that should exist between unit list buttons of different types (example: the amount of spacing between an army button and a squad button).
    /// </summary>
    public float spacingBetweenUnitListButtonTypes { get => _spacingBetweenUnitListButtonTypes; }

    /// <summary>
    /// Public property that should be used in order to access the transform that serves as the parent for all tooptips that are attached to the army management menu.
    /// </summary>
    public Transform tooltipsParent { get => _tooltipsParent; }

    /// <summary>
    /// Private holder variable for the planet that the player selected to display the armies of on the army management menu. May be null if ther armies chosen are not stationed on a planet.
    /// </summary>
    private NewGalaxyPlanet _planetSelected = null;
    /// <summary>
    /// Returns the planet that the player is managing the armies on based on the planetSelectedID.
    /// </summary>
    public NewGalaxyPlanet planetSelected
    {
        get => _planetSelected;
        set
        {
            _planetSelected = value;
        }
    }

    /// <summary>
    /// Private holder variable for the pill view that is displayed by the pill view raw image in the unit inspector.
    /// </summary>
    private GalaxyPillView unitInspectorPillView = null;
    /// <summary>
    /// Private holder variable for the float value that indicates the x position of the mouse when the unit inspector pill view is just starting to be dragged.
    /// </summary>
    private float initialMouseXOnUnitInspectorPillViewDrag;
    /// <summary>
    /// Private holder variable for the float value that indicates the rotation of the pill in the unit inspector pill view when the unit inspector pill view is just starting to be dragged.
    /// </summary>
    private float initialPillRotationOnUnitInspectorPillViewDrag;
    /// <summary>
    /// Private holder variable for the boolean value that indicates whether the texture of the cursor is not the default texture.
    /// </summary>
    private bool cursorTextureChanged = false;
    /// <summary>
    /// Private holder variable for the boolean value that indicates whether the pointer is over the unit inspector pill view.
    /// </summary>
    private bool pointerOverUnitInspectorPillView = false;
    /// <summary>
    /// Private holder variable for the boolean value that indicates whether the unit inspector pill view is being dragged.
    /// </summary>
    private bool unitInspectorPillViewBeingDragged = false;

    /// <summary>
    /// Private holder variable for the unit list button that is curently selected by the player to be displayed in the army management menu's unit inspector.
    /// </summary>
    private GalaxyArmyManagementMenuUnitListButton _unitListButtonSelected = null;
    /// <summary>
    /// Indicates which button in the unit list is currently selected and being displayed in the unit inspector.
    /// </summary>
    public GalaxyArmyManagementMenuUnitListButton unitListButtonSelected
    {
        private get => _unitListButtonSelected;
        set
        {
            //Returns if the unit list button provided is already the one selected.
            if (unitListButtonSelected == value)
                return;
            //Sets the specified unit list button as the selected unit list button.
            unitListButtonSelected = value;
            //Adjusts the unit inspector to match the unit list button selected.
            if (unitListButtonSelected == null)
            {
                //Deactivates the unit inspector effectively because no unit list button is selected.
                unitInspectorBaseParent.gameObject.SetActive(false);
                unitInspectorArmyParent.gameObject.SetActive(false);
                unitInspectorSquadParent.gameObject.SetActive(false);
                unitInspectorPillParent.gameObject.SetActive(false);
                if (unitInspectorPillView != null)
                {
                    unitInspectorPillView.Delete();
                    unitListInspectorPillViewRawImage.texture = null;
                }

                return;
            }
            switch (unitListButtonSelected.buttonType)
            {
                //Activates the base and army components of the unit inspector if the unit list button selected is an army button.
                case GalaxyArmyManagementMenuUnitListButton.ButtonType.Army:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(true);
                    unitInspectorSquadParent.gameObject.SetActive(false);
                    unitInspectorPillParent.gameObject.SetActive(false);

                    //Gets the general of the army (could be null if there is no general).
                    GalaxySpecialPill general = _unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.general;
                    //Deletes/clears the pill view if the army has no general.
                    if (general == null)
                    {
                        //Deactivates the pill view because there is no pill to display.
                        unitListInspectorPillViewRawImage.gameObject.SetActive(false);

                        if (unitInspectorPillView != null)
                        {
                            unitInspectorPillView.Delete();
                            unitListInspectorPillViewRawImage.texture = null;
                        }
                    }
                    //Displays the general in the unit inspector pill view.
                    else
                    {
                        //Activates the pill view because there is a valid pill (general) to display.
                        unitListInspectorPillViewRawImage.gameObject.SetActive(true);

                        if (unitInspectorPillView == null)
                            unitInspectorPillView = PillViewsManager.GetNewPillView(general.convertedToGalaxyPill);
                        else
                            unitInspectorPillView.DisplayedPill = general.convertedToGalaxyPill;
                        unitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
                    }
                    break;
                //Activates the base and squad components of the unit inspector if the unit list button selected is a squad button.
                case UnitListButton.ButtonType.Squad:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(false);
                    unitInspectorSquadParent.gameObject.SetActive(true);
                    unitInspectorPillParent.gameObject.SetActive(false);
                    //Gets the leader of the squad (could be null if there are no pills in the squad).
                    GalaxyPill squadLeader = _unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.squadLeader;
                    //Deletes/clears the pill view if the squad has no leader.
                    if (squadLeader == null)
                    {
                        //Deactivates the pill view because there is no pill to display.
                        unitListInspectorPillViewRawImage.gameObject.SetActive(false);

                        if (unitInspectorPillView != null)
                        {
                            unitInspectorPillView.Delete();
                            unitListInspectorPillViewRawImage.texture = null;
                        }
                    }
                    //Displays the squad leader in the unit inspector pill view.
                    else
                    {
                        //Activates the pill view because there is a valid pill (squad leader) to display.
                        unitListInspectorPillViewRawImage.gameObject.SetActive(true);

                        if (unitInspectorPillView == null)
                            unitInspectorPillView = PillViewsManager.GetNewPillView(squadLeader);
                        else
                            unitInspectorPillView.DisplayedPill = squadLeader;
                        unitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
                    }
                    break;
                //Activates the base and pill components of the unit inspector if the unit list button selected is a pill button.
                case UnitListButton.ButtonType.Pill:
                    unitInspectorBaseParent.gameObject.SetActive(true);
                    unitInspectorArmyParent.gameObject.SetActive(false);
                    unitInspectorSquadParent.gameObject.SetActive(false);
                    unitInspectorPillParent.gameObject.SetActive(true);

                    //Activates the pill view because there is a valid pill to display.
                    unitListInspectorPillViewRawImage.gameObject.SetActive(true);

                    //Displays the pill in the unit inspector pill view.
                    if (unitInspectorPillView == null)
                        unitInspectorPillView = PillViewsManager.GetNewPillView(_unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill);
                    else
                        unitInspectorPillView.DisplayedPill = _unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill;
                    unitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
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
            unitInspectorGroundUnitTypeText.text = _unitListButtonSelected.TypeOfButton.ToString();
            //Sets the name of the ground unit in the unit inspector.
            unitInspectorGroundUnitNameText.text = _unitListButtonSelected.AssignedGroundUnit == null ? "Error: No Ground Unit Assigned To Unit List Button" : _unitListButtonSelected.AssignedGroundUnit.name;
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
        foreach (ArmyManagementMenu armyManagementMenuInList in armyManagementMenus)
        {
            if (armyManagementMenuInList.PlanetSelectedID == planetSelectedID)
                return;
        }
        //Instantiates a new army management menu from the army management menu prefab.
        GameObject armyManagementMenu = Instantiate(armyManagementMenuPrefab);
        //Sets the parent of the army management menu.
        armyManagementMenu.transform.SetParent(GalaxyManager.popupsParent);
        //Gets the script component of the army management menu in order to edit values.
        ArmyManagementMenu armyManagementMenuScript = armyManagementMenu.GetComponent<ArmyManagementMenu>();
        //Sets the planet selected on the army management menu.
        armyManagementMenuScript.PlanetSelectedID = planetSelectedID;
        //Adds the newly created army management menu to the list of army management menus.
        armyManagementMenus.Add(armyManagementMenuScript);
    }

    protected override void Awake()
    {
        base.Awake();

        //Adds this army management menu to the list that contains all army management menus.
        armyManagementMenus.Add(this);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Populates the unit list with the appropriate army buttons.
        if (planetSelectedID >= 0 && planetSelectedID < GalaxyManager.planets.Count)
            PopulateUnitList();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// This method should be called in order to expand all scroll list buttons.
    /// </summary>
    public void ExpandAll()
    {
        //Calls the expand all function on all of the army buttons.
        foreach (UnitListButton armyButton in GetAllUnitListButtonsOfButtonType(UnitListButton.ButtonType.Army))
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

        //Resets the unit list button selected variable so that no unit list button is currently selected.
        unitListButtonSelected = null;

        //Plays the collapse all sound effect.
        AudioManager.PlaySFX(collapseAllSFX);
    }

    /// <summary>
    /// Populates the unit list (scroll view / scroll list) with the appropriate army buttons.
    /// </summary>
    private void PopulateUnitList()
    {
        for (int armyIndex = 0; armyIndex < planetSelected.armyCount; armyIndex++)
        {
            //Instantiates the army button from the army button prefab.
            GameObject armyButton = Instantiate(armyButtonPrefab);
            //Sets the parent of the army button.
            armyButton.transform.SetParent(_unitListButtonParent);
            //Resets the scale of the army button.
            armyButton.transform.localScale = Vector3.one;
            //Gets the army button script component of the army button in order to edit some values in the script.
            ArmyButton armyButtonScript = armyButton.GetComponent<ArmyButton>();
            //Assigns the appropriate army to the army button.
            armyButtonScript.Initialize(this, planetSelected.GetArmyAt(armyIndex));
        }
    }

    /// <summary>
    /// This method is called in order to close the army management menu and also removes the army management menu from the list of army management menus.
    /// </summary>
    public override void Close()
    {
        base.Close();

        //Unselects the currently selected unit list button (mostly to delete any active pill views).
        unitListButtonSelected = null;
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
        for (int siblingIndex = 0; siblingIndex < _unitListButtonParent.childCount; siblingIndex++)
        {
            UnitListButton unitListButton = unitListButtonParent.GetChild(siblingIndex).GetComponent<UnitListButton>();
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
        unitListButtonSelected = null;
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
        if (unitInspectorPillView != null)
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
        if (unitInspectorPillView != null)
        {
            unitInspectorPillView.PillRotation = initialPillRotationOnUnitInspectorPillViewDrag - ((Input.mousePosition.x - initialMouseXOnUnitInspectorPillViewDrag) * pillViewRotationSpeed);
        }
    }

    /// <summary>
    /// This method should be called whenever the player stops dragging on the unit inspector pill view through an event trigger and ensures the cursor texture is reset.
    /// </summary>
    public void OnEndDragUnitInspectorPillView()
    {
        if (unitInspectorPillView != null)
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
        unitListButtonSelected = null;
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

        //StartCoroutine(ConfirmRenamingActionCoroutine());
    }

    /*
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
            confirmationPopupScript.SetPlaceHolderText(UnitListButtonSelected.AssignedGroundUnit.name);

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
                if (squadName.Equals(UnitListButtonSelected.AssignedGroundUnit.name))
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
    */

    /// <summary>
    /// This method should be called in order to rename the selected ground unit and update the selected unit list button's text.
    /// </summary>
    /// <param name="newName"></param>
    private void RenameSelectedGroundUnit(string newName)
    {
        //Sets the selected ground unit's name.
        unitListButtonSelected.AssignedGroundUnit.name = newName;
        //Updates the selected unit list button to accurately reflect the new name of the ground unit.
        unitListButtonSelected.UpdateInfo();
        //Updates the unit inspector to accurately reflect the new name of the ground unit.
        unitInspectorGroundUnitNameText.text = unitListButtonSelected.AssignedGroundUnit.name;

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
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        string topText = "Disband " + unitListButtonSelected.TypeOfButton.ToString();
        string bodyText = "Are you sure that you want to disband " + unitListButtonSelected.AssignedGroundUnit.name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            DisbandSelectedGroundUnit();

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called in order to disband the selected ground unit and remove it and all child buttons from the unit list.
    /// </summary>
    private void DisbandSelectedGroundUnit()
    {
        unitListButtonSelected.DisbandAssignedGroundUnit();

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
        string topText = "Change " + unitListButtonSelected.TypeOfButton.ToString() + "'s Pill Skin";
        string bodyText = "Are you sure that you want to change the assigned pill skin for " + unitListButtonSelected.AssignedGroundUnit.name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, planetSelected.owner.pillSkinNames);
        confirmationPopupScript.SetPillSkinSelected(unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.assignedPillSkinName);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
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
        if (unitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Army)
        {
            //Changes the assigned pill skin of the selected unit list button's ground unit to the pill skin specified.
            unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.assignedPillSkinName = pillSkinName;
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the assigned pill skin for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Assigned Pill Skin Logic Not Implemented For Unit List Buttons of Type: " + unitListButtonSelected.TypeOfButton.ToString() + ".");
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
        string topText = "Change " + unitListButtonSelected.AssignedGroundUnit.name + "'s Icon Color";
        string bodyText = "Are you sure that you want to change the icon color for " + unitListButtonSelected.AssignedGroundUnit.name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.iconColor);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
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
        if (unitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Squad)
        {
            //Gets the squad button component of the selected unit list button.
            SquadButton squadButtonSelected = unitListButtonSelected.gameObject.GetComponent<SquadButton>();
            //Changes the actual squad icon color.
            squadButtonSelected.AssignedSquad.iconColor = newIconColor;
            //Updates the squad button to display the new icon color.
            squadButtonSelected.UpdateInfo();
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the icon color for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Icon Color Logic Not Implemented For Unit List Buttons of Type: " + unitListButtonSelected.TypeOfButton.ToString() + ".");
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
        string topText = "Change " + unitListButtonSelected.AssignedGroundUnit.name + "'s Army Icon";
        string bodyText = "Are you sure that you want to change the army icon for " + unitListButtonSelected.AssignedGroundUnit.name + "?";
        //Fills up an array of sprites with all of the possible army icon sprites before creating the confirmation popup.
        Sprite[] armyIcons = new Sprite[ArmyIconNamesLoader.armyIconNames.Length];
        for (int armyIconIndex = 0; armyIconIndex < armyIcons.Length; armyIconIndex++)
        {
            armyIcons[armyIconIndex] = Resources.Load<Sprite>("Galaxy/Army Icons/" + ArmyIconNamesLoader.armyIconNames[armyIconIndex]);
        }
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, armyIcons, new Color((192 / 255.0f), (192 / 255.0f), (192 / 255.0f), 1), ArmyIconNamesLoader.armyIconNames);

        //Army's currently selected icon is pre-selected.
        confirmationPopupScript.SetSpriteSelected(Resources.Load<Sprite>("Galaxy/Army Icons/" + unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.armyIcon.spriteName));

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
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
        if (unitListButtonSelected.TypeOfButton == UnitListButton.ButtonType.Army)
        {
            //Gets the squad button component of the selected unit list button.
            ArmyButton armyButtonSelected = unitListButtonSelected.gameObject.GetComponent<ArmyButton>();
            //Changes the actual squad icon color.
            armyButtonSelected.AssignedArmy.armyIcon.spriteName = newArmyIconSpriteName;
            //Updates the squad button to display the new icon color.
            armyButtonSelected.UpdateInfo();
        }
        else
        {
            //Logs a warning that informs the programmer that the logic for changing the army icon for the type of unit list button selected has not been implemented yet.
            Debug.LogWarning("Change Army Icon Logic Not Implemented For Unit List Buttons of Type: " + unitListButtonSelected.TypeOfButton.ToString() + ".");
        }
    }

    /// <summary>
    /// This method should be called through an event trigger whenever the create army button at the top of the army management menu is pressed.
    /// </summary>
    public void OnClickCreateArmyButton()
    {
        //Ensures the planet selected is allowed to have another army stationed on it.
        if (!planetSelected.armyCountLimitReached)
        {
            //Creates a new army on the selected planet.
            planetSelected.AddArmy(new GalaxyArmy(planetSelected.owner.empireCulture.ToString() + " Army", planetSelected.ownerID));

            //Instantiates the army button from the army button prefab.
            GameObject armyButton = Instantiate(armyButtonPrefab);
            //Sets the parent of the army button.
            armyButton.transform.SetParent(_unitListButtonParent);
            //Resets the scale of the army button.
            armyButton.transform.localScale = Vector3.one;
            //Gets the army button script component of the army button in order to edit some values in the script.
            ArmyButton armyButtonScript = armyButton.GetComponent<ArmyButton>();
            //Assigns the appropriate army to the army button.
            armyButtonScript.Initialize(this, planetSelected.GetArmyAt(planetSelected.armyCount - 1));

            //Calls for a spacing update on the button above it if such a button exists.
            if (unitListButtonParent.childCount > 1)
                unitListButtonParent.GetChild(unitListButtonParent.childCount - 2).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
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
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Army Creation Failure", planetSelected.planetName + " has already reached the maximum number of armies allowed to be stationed on the planet. Delete or move an existing army to a different planet in order to make room for creating a new army on this planet.", true);

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
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Assign Squad Leader", unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader ? "Pill selected is already the leader of their assigned squad." : "Are you sure that you want to assign the pill " + unitListButtonSelected.AssignedGroundUnit.name + " as the squad leader of " + unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad.name + "?", unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader);

        //Waits until the player has answered the confirmation popup.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //Assigns the selected pill as the squad leader if needed.
        if (!unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.isSquadLeader && confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad.squadLeader = unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill;
            SquadButton parentSquadButton = null;
            for (int siblingIndex = unitListButtonSelected.transform.GetSiblingIndex(); siblingIndex >= 0; siblingIndex--)
            {
                UnitListButton buttonAtSiblingIndex = unitListButtonParent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                if (buttonAtSiblingIndex.TypeOfButton == UnitListButton.ButtonType.Squad && buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>().AssignedSquad == unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad)
                {
                    parentSquadButton = buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>();
                    break;
                }
            }
            if (parentSquadButton != null)
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

        StartCoroutine(ConfirmAssigningGeneralActionCoroutine());
    }

    /// <summary>
    /// This coroutine should be started whenenever the player attempts to assign a general to an army and confirms that the player wishes for the action to be carried through.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmAssigningGeneralActionCoroutine()
    {
        //Creates the confirmation popup.
        SpecialPillConfirmationPopup confirmationPopupScript = Instantiate(SpecialPillConfirmationPopup.specialPillConfirmationPopupPrefab).GetComponent<SpecialPillConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Assign General For " + unitListButtonSelected.AssignedGroundUnit.name, (int)GalaxySpecialPill.Skill.Generalship, unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.generalSpecialPillID);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.generalSpecialPillID = confirmationPopupScript.returnValue;
            //Gets the general of the army (could be null if there is no general).
            GalaxySpecialPill general = _unitListButtonSelected.gameObject.GetComponent<ArmyButton>().AssignedArmy.general;
            //Deletes/clears the pill view if the army has no general.
            if (general == null)
            {
                //Deactivates the pill view because there is no pill to display.
                unitListInspectorPillViewRawImage.gameObject.SetActive(false);

                if (unitInspectorPillView != null)
                {
                    unitInspectorPillView.Delete();
                    unitListInspectorPillViewRawImage.texture = null;
                }
            }
            //Displays the general in the unit inspector pill view.
            else
            {
                //Activates the pill view because there is a valid pill (general) to display.
                unitListInspectorPillViewRawImage.gameObject.SetActive(true);

                if (unitInspectorPillView == null)
                    unitInspectorPillView = PillViewsManager.GetNewPillView(general.convertedToGalaxyPill);
                else
                    unitInspectorPillView.DisplayedPill = general.convertedToGalaxyPill;
                unitListInspectorPillViewRawImage.texture = unitInspectorPillView.RenderTexture;
            }
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called whenever the demote special pill into service button is clicked in the unit inspector (squad button unit inspector only).
    /// </summary>
    public void OnClickDemoteSpecialPillIntoServiceButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        if (!unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.atMaximumCapacity)
            StartCoroutine(ConfirmDemotingSpecialPillIntoServiceActionCoroutine());
        else
            StartCoroutine(ConfirmDemotingSpecialPillIntoServiceFailureDueToSquadCapacityReachedActionCoroutine());
    }

    /// <summary>
    /// This coroutine should be started whenenever the player attempts to demote a special pill into service and confirms that the player wishes for the action to be carried through.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmDemotingSpecialPillIntoServiceActionCoroutine()
    {
        //Creates the confirmation popup.
        SpecialPillConfirmationPopup confirmationPopupScript = Instantiate(SpecialPillConfirmationPopup.specialPillConfirmationPopupPrefab).GetComponent<SpecialPillConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Demote Special Pill Into Service In " + unitListButtonSelected.AssignedGroundUnit.name, (int)GalaxySpecialPill.Skill.Soldiering);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            GalaxySpecialPill specialPill = GalaxyManager.planets[planetSelectedID].owner.GetSpecialPill(confirmationPopupScript.returnValue);
            if (specialPill != null)
            {
                specialPill.task = "Serving In " + unitListButtonSelected.AssignedGroundUnit.name;
                GalaxyPill galaxyPill = specialPill.convertedToGalaxyPill;
                unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.AddPill(galaxyPill);
                if (unitListButtonSelected.gameObject.GetComponent<SquadButton>().Expanded)
                {
                    GameObject pillButton = Instantiate(_pillButtonPrefab);
                    pillButton.transform.SetParent(_unitListButtonParent);
                    pillButton.transform.SetSiblingIndex(unitListButtonSelected.transform.GetSiblingIndex() + unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.pillCount);
                    pillButton.transform.localScale = Vector3.one;
                    PillButton pillButtonScript = pillButton.GetComponent<PillButton>();
                    pillButtonScript.Initialize(this, galaxyPill);
                    _unitListButtonParent.GetChild(pillButton.transform.GetSiblingIndex() - 1).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
                }
                else
                {
                    unitListButtonSelected.gameObject.GetComponent<SquadButton>().Expand(false);
                }
                unitListButtonSelected.UpdateInfo();
            }
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    private IEnumerator ConfirmDemotingSpecialPillIntoServiceFailureDueToSquadCapacityReachedActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Demotion Failure", unitListButtonSelected.AssignedGroundUnit.name + " is at maximum capacity and cannot have any more squad members.", true);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called whenever the promote to special pill button is clicked in the unit inspector (pill button unit inspector only).
    /// </summary>
    public void OnClickPromoteToSpecialPillButton()
    {
        //Plays the sound effect for pressing a button in the unit inspector.
        AudioManager.PlaySFX(clickUnitInspectorButtonSFX);

        StartCoroutine(ConfirmPromotingToSpecialPillActionCoroutine());
    }

    /// <summary>
    /// This coroutine should be started whenenever the player attempts to promote a galaxy pill to a special pill and confirms that the player wishes for the action to be carried through.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmPromotingToSpecialPillActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Promote To Special Pill", "Are you sure that you want to promote " + unitListButtonSelected.AssignedGroundUnit.name + " to a special pill? This action will remove " + unitListButtonSelected.AssignedGroundUnit.name + " from their assigned squad.");

        //Waits until the player has answered the confirmation popup.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //Assigns the selected pill as the squad leader if needed.
        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            SquadButton squadButton = null;
            for (int siblingIndex = unitListButtonSelected.transform.GetSiblingIndex(); siblingIndex >= 0; siblingIndex--)
            {
                if (_unitListButtonParent.GetChild(siblingIndex).gameObject.GetComponent<UnitListButton>().TypeOfButton == UnitListButton.ButtonType.Squad && _unitListButtonParent.GetChild(siblingIndex).gameObject.GetComponent<SquadButton>().AssignedSquad == unitListButtonSelected.gameObject.GetComponent<PillButton>().AssignedPill.assignedSquad)
                {
                    squadButton = _unitListButtonParent.GetChild(siblingIndex).gameObject.GetComponent<SquadButton>();
                    break;
                }
            }
            GalaxySpecialPill specialPill = unitListButtonSelected.GetComponent<PillButton>().AssignedPill.specialPill != null ? unitListButtonSelected.GetComponent<PillButton>().AssignedPill.specialPill : new GalaxySpecialPill(unitListButtonSelected.GetComponent<PillButton>().AssignedPill);
            unitListButtonSelected.GetComponent<PillButton>().AssignedPill.assignedSquad.RemovePill(unitListButtonSelected.GetComponent<PillButton>().AssignedPill);
            specialPill.task = null;
            UnitListButton buttonAbove = _unitListButtonParent.GetChild(unitListButtonSelected.transform.GetSiblingIndex() - 1).GetComponent<UnitListButton>(), buttonBelow = _unitListButtonParent.GetChild(unitListButtonSelected.transform.GetSiblingIndex() + 1).GetComponent<UnitListButton>();
            unitListButtonDestroyer.AddUnitListButtonToDestroy(unitListButtonSelected);
            buttonAbove.SpacingUpdateRequiredNextFrame = true;
            buttonBelow.SpacingUpdateRequiredNextFrame = true;

            //Collapses and expands the assigned squad's button in order to update the child buttons.
            squadButton.Collapse(false);
            squadButton.Expand(false);

            //Updates the info on the squad button now that a pill has been removed from the assigned squad.
            squadButton.UpdateInfo();

            //No button in the unit list is selected after the promoting action has been completed.
            unitListButtonSelected = null;
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Closes all army management menus and removes them from the static list.
    /// </summary>
    public static void CloseAll()
    {
        if (armyManagementMenus == null || armyManagementMenus.Count == 0)
            return;
        for (int menuIndex = armyManagementMenus.Count - 1; menuIndex >= 0; menuIndex--)
        {
            armyManagementMenus[menuIndex].Close();
        }
    }
}