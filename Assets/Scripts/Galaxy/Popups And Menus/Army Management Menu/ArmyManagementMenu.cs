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
            titleText.text = PlanetSelected.Name + " Army Management";
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
                    GalaxyPill squadLeader = unitListButtonSelected.gameObject.GetComponent<SquadButton>().AssignedSquad.SquadLeader;
                    //Deletes/clears the pill view if the squad has no leader.
                    if (squadLeader == null)
                    {
                        if(unitInspectorPillView != null)
                        {
                            unitInspectorPillView.Delete();
                            UnitListInspectorPillViewRawImage.texture = null;
                        }
                    }
                    //Displays the squad leader in the unit inspector pill view.
                    else
                    {
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
        for(int armyIndex = 0; armyIndex < PlanetSelected.GetArmiesCount(); armyIndex++)
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

        //Removes the army management menu from the list of army management menus.
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

    private void OnDestroy()
    {
        //Ensures that the cursor texture is the default cursor texture.
        if (cursorTextureChanged)
        {
            //Resets the cursor texture.
            GeneralHelperMethods.ResetCursorTexture();
            //Logs that the cursor texture has been reset.
            cursorTextureChanged = false;
        }
    }

    /// <summary>
    /// This method should be called using an event trigger whenever the rename button in the base section of the unit inspector is clicked.
    /// </summary>
    public void OnClickRenameButton()
    {
        StartCoroutine(ConfirmRenamingActionCoroutine());
    }

    /// <summary>
    /// This method should be called on the click of the rename button in the unit inspector and confirms that the player wants to rename the selected ground unit.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmRenamingActionCoroutine()
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
    }
}