using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

abstract public class GalaxyArmyManagementMenuUnitListButton : GalaxyTooltipEventsHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    public enum ButtonType
    {
        Army,
        Squad,
        Pill
    }

    [Header("Base Components")]

    [SerializeField] private Button _button = null;
    [SerializeField] private Image _leftImage = null;
    [SerializeField] private Image _experienceLevelIconImage = null;
    [SerializeField] private Text _nameText = null;
    [SerializeField] private Text _experienceLevelText = null;
    [SerializeField] private GalaxyTooltip _experienceLevelTooltip = null;

    [Header("Base Logic Options")]

    [SerializeField] private ButtonType _buttonType = 0;

    [Header("Base SFX Options")]

    [SerializeField, Tooltip("The sound effect that should be played whenever the pointer enters the unit list button.")] private AudioClip _pointerEnterSFX = null;
    [SerializeField, Tooltip("The sound effect that should be played whenever the player clicks on the unit list button.")] private AudioClip _clickSFX = null;
    [SerializeField, Tooltip("The sound effect that should be played whenever the unit list button is sucessfully dragged to a new location in the unit list.")] private AudioClip _movedSFX = null;

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Protected property that should be used in order to access the actual Unity button component of the unit list button.
    /// </summary>
    protected Button button { get => _button; }

    /// <summary>
    /// Protected property that should be used in order to access the left image component of the unit list button.
    /// </summary>
    protected Image leftImage { get => _leftImage; }

    /// <summary>
    /// Protected property that should be used in order to access the experience level icon image component of the unit list button.
    /// </summary>
    protected Image experienceLevelIconImage { get => _experienceLevelIconImage; }

    /// <summary>
    /// Protected property that should be used in order to access the name text component of the unit list button.
    /// </summary>
    protected Text nameText { get => _nameText; }

    /// <summary>
    /// Protected property that should be used in order to access the experience level text component of the unit list button.
    /// </summary>
    protected Text experienceLevelText { get => _experienceLevelText; }

    /// <summary>
    /// Protected property that should be used in order to access the experience level tooltip component of the unit list button.
    /// </summary>
    protected GalaxyTooltip experienceLevelTooltip { get => _experienceLevelTooltip; }

    /// <summary>
    /// Public property that should be used in order to access the type of unit list button (Army, Squad, Pill).
    /// </summary>
    public ButtonType buttonType { get => _buttonType; }

    /// <summary>
    /// Protected property that should be used in order to access the sound effect that should be played whenever the pointer enters the unit list button.
    /// </summary>
    protected AudioClip pointerEnterSFX { get => _pointerEnterSFX; }

    /// <summary>
    /// Protected property that should be used in order to access the sound effect that should be played whenever the player clicks on the unit list button.
    /// </summary>
    protected AudioClip clickSFX { get => _clickSFX; }

    /// <summary>
    /// Protected property that should be used in order to access the sound effect that should be played whenever the unit list button is sucessfully dragged to a new location in the unit list.
    /// </summary>
    protected AudioClip movedSFX { get => _movedSFX; }

    /// <summary>
    /// Private holder variable for the army management menu that the button belongs to.
    /// </summary>
    private GalaxyArmyManagementMenu _armyManagementMenu = null;
    /// <summary>
    /// Indicates the army management menu that the unit list button is attached to (can only be set if the unit list button has no reference).
    /// </summary>
    public GalaxyArmyManagementMenu armyManagementMenu { get => _armyManagementMenu; set { _armyManagementMenu = _armyManagementMenu == null ? value : _armyManagementMenu;  } }

    /// <summary>
    /// Public property that should be used in order to access the float value that indicates the height of the button when it was created.
    /// </summary>
    public float initialHeight { get; private set; } = 0;

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether the spacing between the unit list button and the next needs to be updated the next frame.
    /// </summary>
    private bool _spacingUpdateRequiredNextFrame = false;
    /// <summary>
    /// Private holder variable for the integer value that indicates the amount of updates that the button has gone through since it was told that a spacing update was required the next frame.
    /// </summary>
    private int updatesSinceSpacingUpdateRequiredNextFrameSet = 0;
    /// <summary>
    /// Public property that should be used in order to mutate the boolean value that indicates whether the spacing between this unit list button and the next needs to be updated the next frame.
    /// </summary>
    public bool spacingUpdateRequiredNextFrame { get => _spacingUpdateRequiredNextFrame; set { _spacingUpdateRequiredNextFrame = value; updatesSinceSpacingUpdateRequiredNextFrameSet = 0; } }

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether the unit list button is currently being dragged.
    /// </summary>
    public bool beingDragged { get; private set; } = false;
    /// <summary>
    /// Protected property that should be used in order to access and mutate the boolean value that indicates whether or not the latest button move was successful or not.
    /// </summary>
    protected bool latestButtonMoveSuccessful { get; private set; } = false;

    /// <summary>
    /// Private holder variable for the float value that indicates the local y position of the actual button component when the button component was just beginning to be dragged.
    /// </summary>
    private float beginDragLocalYPosition = 0;
    /// <summary>
    /// Private holder variable for the float value that indicates the change in position from the main button to the actual button component when the button is just beginning to be dragged.
    /// </summary>
    private float beginDragYOffset = 0;
    /// <summary>
    /// Private holder variable for the float value that indicates the Y offset from the mouse to the button when the button is being dragged.
    /// </summary>
    private float dragYOffsetFromMouse = 0;

    /// <summary>
    /// Public property that should be used in order to access the ground unit that this unit list button is assigned to represent.
    /// </summary>
    public NewGalaxyGroundUnit assignedGroundUnit
    {
        get
        {
            if (buttonType == ButtonType.Army)
                return gameObject.GetComponent<GalaxyArmyButton>().assignedArmy;
            else if (buttonType == ButtonType.Squad)
                return gameObject.GetComponent<GalaxySquadButton>().assignedSquad;
            else if (buttonType == ButtonType.Pill)
                return gameObject.GetComponent<GalaxyPillButton>().assignedPill;
            else
                return null;
        }
    }

    /// <summary>
    /// Provides the initial values of important unit list button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    protected void Initialize(GalaxyArmyManagementMenu armyManagementMenu)
    {
        //Sets the reference to the attached army management menu.
        this.armyManagementMenu = armyManagementMenu;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        //Logs the initial height of the unit list button.
        initialHeight = ((RectTransform)transform).sizeDelta.y;

        //Updates the spacing of the unit list button.
        UpdateSpacing();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //Updates the spacing between this unit list button and the next unit list button.
        if (spacingUpdateRequiredNextFrame)
        {
            if (updatesSinceSpacingUpdateRequiredNextFrameSet >= 1)
                UpdateSpacing();
            else
                updatesSinceSpacingUpdateRequiredNextFrameSet++;
        }
    }

    public override void OnTooltipOpen(GalaxyTooltip tooltip)
    {
        //Executes the base class's logic for when the tooltip opens.
        base.OnTooltipOpen(tooltip);

        //Sets the position of the experience level tooltip if it is the tooltip that opened.
        if (tooltip == experienceLevelTooltip)
            experienceLevelTooltip.Position = new Vector2(experienceLevelIconImage.transform.position.x - 35, experienceLevelIconImage.transform.position.y + 59);
    }

    public override void OnTooltipClose(GalaxyTooltip tooltip)
    {
        //Executes the base class's logic for when the tooltip closes.
        base.OnTooltipClose(tooltip);
    }

    /// <summary>
    /// This method is called through an event trigger and deselects the button component of the unit list button.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnSelect(BaseEventData eventData)
    {
        //Deselects the button component of the unit list button.
        button.OnDeselect(eventData);
    }

    /// <summary>
    /// This method is called through a button click.
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnClick()
    {
        if (!beingDragged)
            OnClickWithoutDrag();
    }

    /// <summary>
    /// This method is called through the on click method whenever it is detetcted that the unit list button was clicked while not being dragged.
    /// </summary>
    public virtual void OnClickWithoutDrag()
    {
        //Sets the unit list button as the one selected and displayed in the unit inspector.
        armyManagementMenu.unitListButtonSelected = this;

        //Plays the sound effect for being clicked.
        AudioManager.PlaySFX(clickSFX);
    }

    /// <summary>
    /// This method is called through the starting of a drag on a unit list button and saves the initial y position of the actual button component and logs that the unit list button is being dragged.
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData pointerEventData)
    {
        //Ignores any begin drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;
        //Saves the initial local y position of the actual button component before it begins to be dragged.
        beginDragLocalYPosition = (gameObject.GetComponent<GalaxyArmyManagementMenuExpandableUnitListButton>() != null && gameObject.GetComponent<GalaxyArmyManagementMenuExpandableUnitListButton>().isExpanded) ? button.transform.localPosition.y - (armyManagementMenu.spacingBetweenUnitListButtonTypes / 2) : button.transform.localPosition.y;
        //Saves the initial y offset.
        beginDragYOffset = button.transform.position.y - transform.position.y;
        //Sets the parent of the button component to the parent of buttons that are being dragged in order to ensure that the button is on top of all of the other buttons that are not being dragged.
        button.transform.SetParent(armyManagementMenu.buttonsBeingDraggedParent);
        //Saves the y offset of the button from the mouse's position.
        dragYOffsetFromMouse = button.transform.position.y - Input.mousePosition.y;
        //Logs that the unit list button is being dragged.
        beingDragged = true;

        //Sets the unit list button as the one selected and displayed in the unit inspector.
        armyManagementMenu.unitListButtonSelected = this;
    }

    /// <summary>
    /// This method is called through the dragging a unit list button and changes the y position of the button to match the y position of the mouse.
    /// </summary>
    public virtual void OnDrag(PointerEventData pointerEventData)
    {
        //Ignores any drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;
        //Changes the y position of the actual button component to match the y position of the mouse.
        button.transform.position = new Vector2(button.transform.position.x, Input.mousePosition.y + dragYOffsetFromMouse);
        //Updates the position of the experience level tooltip if it is open.
        if (experienceLevelTooltip.Open)
            experienceLevelTooltip.Position = new Vector2(experienceLevelIconImage.transform.position.x - 35, experienceLevelIconImage.transform.position.y + 59);
    }

    /// <summary>
    /// This method is called through the ending of a drag on a unit list button and determines the new position of the unit list button and logs that the unit list button is no longer being dragged.
    /// </summary>
    public virtual void OnEndDrag(PointerEventData pointerEventData)
    {
        //Ignores any end drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;
        //Executes logic based on whether or not the move of the unit was successful.
        ExecuteButtonMove();
        //Reverts the parent of the button component to be the unit list button again.
        button.transform.SetParent(transform);
        //Reverts the position of the actual button component to its local position when it was just beginning to be dragged.
        button.transform.localPosition = new Vector2(button.transform.localPosition.x, beginDragLocalYPosition);
        //Logs that the unit list button is no longer being dragged.
        beingDragged = false;
        //Informs the army manaagement menu that the latest unit list button move was successful.
        if (latestButtonMoveSuccessful)
            armyManagementMenu.OnUnitListButtonMove();
    }

    /// <summary>
    /// Trys to move the unit list button to its new position and modifies a bool on whether or not a move actually occured.
    /// </summary>
    /// <returns></returns>
    private void ExecuteButtonMove()
    {
        //Button move unsuccesful if the button that is being moved is the only button in the unit list.
        if (transform.parent.childCount <= 1)
        {
            latestButtonMoveSuccessful = false;
            return;
        }

        //Finds the unit list button above the current position where this unit list button has been dragged to.
        GalaxyArmyManagementMenuUnitListButton buttonAbove = FindButtonAbove();
        //Finds the unit list button below the current position where this unit list button has been dragged to.
        GalaxyArmyManagementMenuUnitListButton buttonBelow = FindButtonBelow();
        //Finds the new sibling index this unit list button would be at if the move was successful.
        int newSiblingIndex = buttonAbove == null ? 0 : buttonAbove.transform.GetSiblingIndex() + 1;
        //Executes unit list button movement logic based on what type of button this unit list button that was being dragged is.
        switch (buttonType)
        {
            case ButtonType.Army:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && (buttonAbove == null || buttonBelow == null || buttonBelow.buttonType == ButtonType.Army))
                {
                    latestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    latestButtonMoveSuccessful = false;
                    return;
                }
            case ButtonType.Squad:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && ((buttonAbove != null && buttonAbove.buttonType == ButtonType.Army) || (buttonBelow != null && buttonBelow.buttonType == ButtonType.Squad) || (buttonBelow != null && buttonBelow.buttonType == ButtonType.Army && buttonAbove != null)))
                {
                    if (buttonAbove != null && buttonAbove.buttonType == ButtonType.Army && !buttonAbove.gameObject.GetComponent<GalaxyArmyButton>().isExpanded)
                        buttonAbove.gameObject.GetComponent<GalaxyArmyButton>().ForceExpand(false);
                    latestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    latestButtonMoveSuccessful = false;
                    return;
                }
            case ButtonType.Pill:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && ((buttonAbove != null && buttonAbove.buttonType == ButtonType.Squad) || (buttonAbove != null && buttonAbove.buttonType == ButtonType.Pill)))
                {
                    if (buttonAbove != null && buttonAbove.buttonType == ButtonType.Squad && !buttonAbove.gameObject.GetComponent<GalaxySquadButton>().isExpanded)
                        buttonAbove.gameObject.GetComponent<GalaxySquadButton>().ForceExpand(false);
                    latestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    latestButtonMoveSuccessful = false;
                    return;
                }

            //If there is no logic implemented for a type of unit list button to move then all moves with that button will be unsuccessful.
            default:
                Debug.Log("Button move logic for button type " + buttonType.ToString() + " does not exist.");
                latestButtonMoveSuccessful = false;
                return;
        }

        //Executes logic for when the button should be moved successfully.

        //Decreases the new sibling index for this unit list button if it's original sibling index is less than the new one.
        if (transform.GetSiblingIndex() < newSiblingIndex)
            newSiblingIndex--;

        //Updates the spacing on the unit list button that used to be above this unit list button.
        if (transform.GetSiblingIndex() > 0)
            transform.parent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>().spacingUpdateRequiredNextFrame = true;
        //Updates the spacing on the unit list button that used to be below this unit list button.
        if (transform.GetSiblingIndex() < transform.parent.childCount - 1)
            transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>().spacingUpdateRequiredNextFrame = true;
        //Updates the spacing on the unit list buttons near the new sibling index for this unit list button.
        if (newSiblingIndex - 1 >= 0)
            transform.parent.GetChild(newSiblingIndex - 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>().spacingUpdateRequiredNextFrame = true;
        transform.parent.GetChild(newSiblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>().spacingUpdateRequiredNextFrame = true;
        if (newSiblingIndex + 1 < transform.parent.childCount)
            transform.parent.GetChild(newSiblingIndex + 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>().spacingUpdateRequiredNextFrame = true;

        //Stores the original sibling index of this unit list button.
        int originalSiblingIndex = transform.GetSiblingIndex();
        //Moves this unit list button to its new sibling index.
        transform.SetSiblingIndex(newSiblingIndex);

        //Updates the spacing on this unit list button after it has been moved.
        spacingUpdateRequiredNextFrame = true;

        if(armyManagementMenu.planetSelected != null)
        {
            //Saves the moving of the unit list button based on the unit list button type.
            switch (buttonType)
            {
                case ButtonType.Army:
                    //Finds the original index the army was at in the list of armies on the planet.
                    int originalArmyIndex = 0;
                    for (int armyIndex = 0; armyIndex < armyManagementMenu.planetSelected.stationedArmies.Count; armyIndex++)
                    {
                        if (armyManagementMenu.planetSelected.stationedArmies[armyIndex] == gameObject.GetComponent<GalaxyArmyButton>().assignedArmy)
                        {
                            originalArmyIndex = armyIndex;
                            break;
                        }
                    }
                    //Finds the new army index.
                    int newArmyIndex = 0;
                    for (int siblingIndex = 0; siblingIndex <= newSiblingIndex; siblingIndex++)
                    {
                        GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
                        if (buttonAtSiblingIndex.buttonType == ButtonType.Army)
                        {
                            if (buttonAtSiblingIndex.gameObject.GetComponent<GalaxyArmyButton>().assignedArmy == gameObject.GetComponent<GalaxyArmyButton>().assignedArmy)
                            {
                                break;
                            }
                            else
                            {
                                newArmyIndex++;
                            }
                        }
                    }
                    //Changes the index that the army is placed at in the list of armies on the planet.
                    armyManagementMenu.planetSelected.stationedArmies.Move(originalArmyIndex, newArmyIndex);
                    break;
                case ButtonType.Squad:
                    NewGalaxySquad squad = gameObject.GetComponent<GalaxySquadButton>().assignedSquad;
                    //Stores the original parent army button.
                    GalaxyArmyButton originalParentArmyButton = null;
                    for (int siblingIdex = 0; siblingIdex < transform.parent.childCount; siblingIdex++)
                    {
                        if (transform.parent.GetChild(siblingIdex).GetComponent<GalaxyArmyManagementMenuUnitListButton>().buttonType == ButtonType.Army && transform.parent.GetChild(siblingIdex).GetComponent<GalaxyArmyButton>().assignedArmy == squad.assignedArmy)
                        {
                            originalParentArmyButton = transform.parent.GetChild(siblingIdex).GetComponent<GalaxyArmyButton>();
                            break;
                        }
                    }
                    //Removes the squad from its current army.
                    squad.assignedArmy.squads.Remove(squad);
                    //Finds the new army the squad will be assigned to.
                    NewGalaxyArmy newArmyAssigned = null;
                    GalaxyArmyButton parentArmyButton = null;
                    for (int siblingIndex = newSiblingIndex; siblingIndex >= 0; siblingIndex--)
                    {
                        GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
                        if (buttonAtSiblingIndex.buttonType == ButtonType.Army)
                        {
                            parentArmyButton = buttonAtSiblingIndex.gameObject.GetComponent<GalaxyArmyButton>();
                            newArmyAssigned = parentArmyButton.assignedArmy;
                            break;
                        }
                    }
                    //Assigns the squad to its new army.
                    newArmyAssigned.squads.Insert(newSiblingIndex - parentArmyButton.transform.GetSiblingIndex() - 1, squad);
                    //Updates the information displayed on the new parent army button.
                    parentArmyButton.UpdateInfo();
                    //Updates the information displayed on the original parent army button.
                    originalParentArmyButton.UpdateInfo();
                    break;
                case ButtonType.Pill:
                    NewGalaxyPill pill = gameObject.GetComponent<GalaxyPillButton>().assignedPill;
                    bool squadLeader = pill.isSquadLeader;
                    //Stores the original parent buttons.
                    List<GalaxyArmyManagementMenuUnitListButton> originalParentButtons = new List<GalaxyArmyManagementMenuUnitListButton>();
                    for (int siblingIndex = 0; siblingIndex < transform.parent.childCount; siblingIndex++)
                    {
                        GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
                        if ((buttonAtSiblingIndex.buttonType == ButtonType.Army && buttonAtSiblingIndex.gameObject.GetComponent<GalaxyArmyButton>().assignedArmy == pill.assignedSquad.assignedArmy) || (buttonAtSiblingIndex.buttonType == ButtonType.Squad && buttonAtSiblingIndex.gameObject.GetComponent<GalaxySquadButton>().assignedSquad == pill.assignedSquad))
                        {
                            originalParentButtons.Add(buttonAtSiblingIndex);
                            if (originalParentButtons.Count >= 2)
                                break;
                        }
                    }
                    //Removes the pill from its current squad.
                    NewGalaxySquad originalSquad = pill.assignedSquad;
                    pill.assignedSquad.pills.Remove(pill);
                    //Finds the new squad the pill will be assigned to and the new parent buttons of this button (squad parent at index 0 and army parent at index 1).
                    NewGalaxySquad newSquadAssigned = null;
                    List<GalaxyArmyManagementMenuUnitListButton> newParentButtons = new List<GalaxyArmyManagementMenuUnitListButton>();
                    for (int siblingIndex = newSiblingIndex; siblingIndex >= 0; siblingIndex--)
                    {
                        GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
                        if (buttonAtSiblingIndex.buttonType == ButtonType.Squad && newParentButtons.Count == 0)
                        {
                            newParentButtons.Add(buttonAtSiblingIndex);
                            newSquadAssigned = buttonAtSiblingIndex.gameObject.GetComponent<GalaxySquadButton>().assignedSquad;
                        }
                        else if (buttonAtSiblingIndex.buttonType == ButtonType.Army)
                        {
                            newParentButtons.Add(buttonAtSiblingIndex);
                            break;
                        }
                    }
                    //Assigns the pill to its new squad.
                    newSquadAssigned.pills.Insert(newSiblingIndex - newParentButtons[0].transform.GetSiblingIndex() - 1, pill);
                    //Sets the pill back as the squad leader if it was before.
                    if (squadLeader && newSquadAssigned == originalSquad)
                        originalSquad.leader = pill;
                    //Updates the information displayed on the new parent buttons.
                    foreach (GalaxyArmyManagementMenuUnitListButton newParentButton in newParentButtons)
                        newParentButton.UpdateInfo();
                    //Updates the information displayed on the original parent buttons.
                    foreach (GalaxyArmyManagementMenuUnitListButton originalParentButton in originalParentButtons)
                        originalParentButton.UpdateInfo();
                    break;

                default:
                    Debug.Log("Save logic does not exist for unit list button type: " + buttonType.ToString());
                    break;
            }
        }

        //Plays the sound effect for the unit list button being successfully moved to another location in the unit list.
        AudioManager.PlaySFX(movedSFX);
    }

    /// <summary>
    /// Finds the unit list button right above the position of this unit list button and returns it.
    /// </summary>
    /// <returns></returns>
    private GalaxyArmyManagementMenuUnitListButton FindButtonAbove()
    {
        //Loops through every unit list button in the unit list backwards in order to ensure that the first unit list button above this unit list button is the one returned.
        for (int siblingIndex = transform.parent.childCount - 1; siblingIndex >= 0; siblingIndex--)
        {
            //Ignores this sibling index if the unit list button at this sibling index is this unit list button.
            if (siblingIndex == transform.GetSiblingIndex())
                continue;

            //Returns the button at the sibling index if its position is above this unit list button's position.
            GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
            if (buttonAtSiblingIndex.button.transform.position.y >= _button.transform.position.y)
                return buttonAtSiblingIndex;
        }

        //Returns null, as no unit list button was found to be above this unit list button.
        return null;
    }

    /// <summary>
    /// Finds the unit list button right below the position this unit list button was dragged to and returns it.
    /// </summary>
    /// <returns></returns>
    private GalaxyArmyManagementMenuUnitListButton FindButtonBelow()
    {
        //Loops through every unit list button in the unit list in sequential or forwards order to ensure that the first unit list button found below this unit list button is indeed the one right below it and not further below it.
        for (int siblingIndex = 0; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            //Ignores this sibling index if the unit list button at this sibling index is this unit list button.
            if (siblingIndex == transform.GetSiblingIndex())
                continue;

            //Returns the button at the sibling index if its position is below this unit list button's position.
            GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
            if (buttonAtSiblingIndex.button.transform.position.y < _button.transform.position.y)
                return buttonAtSiblingIndex;
        }

        //Returns null, as no unit list button was found to be below the position this unit list button was dragged to.
        return null;
    }

    /// <summary>
    /// This method is called whenever the pointer enters the unit list button and plays the pointer enter unit list button sound effect.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public virtual void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(pointerEnterSFX);
    }

    /// <summary>
    /// This method should be called in order to update the spacing between the unit list buttons, which is done by determining the height of each unit list button rect transform.
    /// </summary>
    private void UpdateSpacing()
    {
        RectTransform rectTransform = (RectTransform)transform;
        if (transform.GetSiblingIndex() + 1 < transform.parent.childCount && transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>().buttonType != buttonType)
        {
            if (Mathf.Approximately(rectTransform.sizeDelta.y, initialHeight))
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, initialHeight + armyManagementMenu.spacingBetweenUnitListButtonTypes);
            }
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, initialHeight);
        }

        //Weird code essential for preventing a dumb unity bug.
        float verticalLayoutGroupSpacing = armyManagementMenu.unitListVerticalLayoutGroup.spacing;
        armyManagementMenu.unitListVerticalLayoutGroup.spacing = verticalLayoutGroupSpacing + 1;
        armyManagementMenu.unitListVerticalLayoutGroup.spacing = verticalLayoutGroupSpacing;

        //Logs that the spacing has been updated.
        spacingUpdateRequiredNextFrame = false;
    }

    /// <summary>
    /// This method should be called in order to update the text on the button to accurately reflect information on the assigned ground unit.
    /// </summary>
    public virtual void UpdateInfo()
    {
        //Updates the name text of the button to reflect the name of the ground unit.
        nameText.text = assignedGroundUnit.name;
        //Updates the experience level text of the button to reflect the experience level of the ground unit. 
        experienceLevelText.text = assignedGroundUnit.experienceLevel.ToString();
    }

    /// <summary>
    /// This method should be called in order to disband the button's assigned ground unit.
    /// </summary>
    public virtual void DisbandAssignedGroundUnit()
    {
        //Finds the button above and the button below.
        GalaxyArmyManagementMenuUnitListButton buttonAbove = transform.GetSiblingIndex() > 0 ? armyManagementMenu.unitListButtonParent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>() : null;
        GalaxyArmyManagementMenuUnitListButton buttonBelow = transform.GetSiblingIndex() + 1 < transform.parent.childCount ? armyManagementMenu.unitListButtonParent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<GalaxyArmyManagementMenuUnitListButton>() : null;

        //Fills a list with all child buttons in the unit list that need to be destroyed.
        List<GalaxyArmyManagementMenuUnitListButton> childButtonsToDestroy = new List<GalaxyArmyManagementMenuUnitListButton>();
        for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
            if (buttonAtSiblingIndex.buttonType <= buttonType)
                break;
            childButtonsToDestroy.Add(buttonAtSiblingIndex);
        }

        //Destroys all neccessary child buttons in the unit list.
        foreach (GalaxyArmyManagementMenuUnitListButton childButton in childButtonsToDestroy)
        {
            armyManagementMenu.unitListButtonDestroyer.AddUnitListButtonToDestroy(childButton);
        }

        //Destroys this unit list button.
        armyManagementMenu.unitListButtonDestroyer.AddUnitListButtonToDestroy(this);

        //No button in the unit list is selected after the disbanding action is complete.
        armyManagementMenu.unitListButtonSelected = null;

        //Spacing update on button above and button below.
        if (buttonAbove != null)
            buttonAbove.spacingUpdateRequiredNextFrame = true;
        if (buttonBelow != null)
            buttonBelow.spacingUpdateRequiredNextFrame = true;
    }
}
