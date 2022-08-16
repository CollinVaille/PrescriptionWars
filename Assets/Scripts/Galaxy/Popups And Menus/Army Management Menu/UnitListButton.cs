using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

abstract public class UnitListButton : GalaxyTooltipEventsHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    [Header("Base Components")]

    [SerializeField] private Button button = null;
    protected Button Button { get => button; }

    [SerializeField] private Image leftImage = null;
    protected Image LeftImage { get => leftImage; }

    [SerializeField] private Image experienceLevelIconImage = null;
    protected Image ExperienceLevelIconImage { get => experienceLevelIconImage; }

    [SerializeField] private Text nameText = null;
    protected Text NameText { get => nameText; }

    [SerializeField] private Text experienceLevelText = null;
    protected Text ExperienceLevelText { get => experienceLevelText; }

    [SerializeField] private GalaxyTooltip experienceLevelTooltip = null;
    protected GalaxyTooltip ExperienceLevelTooltip { get => experienceLevelTooltip; }

    /*[Header("Base Tooltips")]

    [SerializeField] private GalaxyTooltip leftImageTooltip = null;
    protected GalaxyTooltip LeftImageTooltip { get => leftImageTooltip; }*/

    [Header("Base Logic Options")]

    [SerializeField] private ButtonType buttonType = 0;
    public ButtonType TypeOfButton { get => buttonType; }
    public enum ButtonType
    {
        Army,
        Squad,
        Pill
    }

    [Header("Base SFX Options")]

    [SerializeField, Tooltip("The sound effect that should be played whenever the pointer enters the unit list button.")] private AudioClip pointerEnterSFX = null;
    /// <summary>
    /// The sound effect that should be played whenever the pointer enters the unit list button.
    /// </summary>
    protected AudioClip PointerEnterSFX { get => pointerEnterSFX; }

    [SerializeField, Tooltip("The sound effect that should be played whenever the player clicks on the unit list button.")] private AudioClip clickSFX = null;
    /// <summary>
    /// The sound effect that should be played whenever the player clicks on the unit list button.
    /// </summary>
    protected AudioClip ClickSFX { get => clickSFX; }

    [SerializeField, Tooltip("The sound effect that should be played whenever the unit list button is sucessfully dragged to a new location in the unit list.")] private AudioClip movedSFX = null;
    /// <summary>
    /// The sound effect that should be played whenever the unit list button is sucessfully dragged to a new location in the unit list.
    /// </summary>
    protected AudioClip MovedSFX { get => movedSFX; }

    //------------------------
    //Non-inspector variables.
    //------------------------

    private ArmyManagementMenu armyManagementMenu = null;
    /// <summary>
    /// Indicates the army management menu that the unit list button is attached to (can only be set if the unit list button has no reference).
    /// </summary>
    public ArmyManagementMenu ArmyManagementMenu
    {
        get
        {
            return armyManagementMenu;
        }
        set
        {
            //Sets the unit list button's reference to the army management menu if the there currently is no reference of the army management menu.
            armyManagementMenu = armyManagementMenu == null ? value : armyManagementMenu;
        }
    }

    /// <summary>
    /// Indicates the height of the button when it was created.
    /// </summary>
    private float initialHeight = 0;
    public float InitialHeight { get => initialHeight; }

    /// <summary>
    /// Indicates whether the spacing between this unit list button and the next needs to be updated the next frame.
    /// </summary>
    private bool spacingUpdateRequiredNextFrame = false;
    /// <summary>
    /// Indicates the amount of updates that the button has gone through since it was told that a spacing update was required the next frame.
    /// </summary>
    private int updatesSinceSpacingUpdateRequiredNextFrameSet = 0;
    /// <summary>
    /// Indicates whether the spacing between this unit list button and the next needs to be updated the next frame.
    /// </summary>
    public bool SpacingUpdateRequiredNextFrame
    {
        set
        {
            spacingUpdateRequiredNextFrame = value;

            updatesSinceSpacingUpdateRequiredNextFrameSet = 0;
        }
    }

    /// <summary>
    /// Indicates whether the unit list button is currently being dragged.
    /// </summary>
    protected bool BeingDragged { get => beingDragged; }
    /// <summary>
    /// Indicates whether the unit list button is currently being dragged.
    /// </summary>
    private bool beingDragged = false;
    /// <summary>
    /// Indicates whether or not the latest button move was successful or not.
    /// </summary>
    protected bool LatestButtonMoveSuccessful { get => latestButtonMoveSuccessful; private set => latestButtonMoveSuccessful = value; }
    /// <summary>
    /// Indicates whether or not the latest button move was successful or not.
    /// </summary>
    private bool latestButtonMoveSuccessful = false;

    /// <summary>
    /// Indicates the local y position of the actual button component when the button component was just beginning to be dragged.
    /// </summary>
    private float beginDragLocalYPosition = 0;
    /// <summary>
    /// Indicates the change in position from the main button to the actual button component when the button is just beginning to be dragged.
    /// </summary>
    private float beginDragYOffset = 0;
    /// <summary>
    /// Indicates the Y offset from the mouse to the button when the button is being dragged.
    /// </summary>
    private float dragYOffsetFromMouse = 0;

    /// <summary>
    /// Indicates the ground unit that this unit list button is assigned to.
    /// </summary>
    public GalaxyGroundUnit AssignedGroundUnit
    {
        get
        {
            if (TypeOfButton == ButtonType.Army)
                return gameObject.GetComponent<ArmyButton>().AssignedArmy;
            else if (TypeOfButton == ButtonType.Squad)
                return gameObject.GetComponent<SquadButton>().AssignedSquad;
            else if (TypeOfButton == ButtonType.Pill)
                return gameObject.GetComponent<PillButton>().AssignedPill;
            else
                return null;
        }
    }

    /// <summary>
    /// Provides the initial values of important unit list button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    protected void Initialize(ArmyManagementMenu armyManagementMenu)
    {
        //Sets the reference to the attached army management menu.
        ArmyManagementMenu = armyManagementMenu;
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

        //Updates the position of the experience level tooltip if it is open.
        //if(ExperienceLevelTooltip.Open)
            //ExperienceLevelTooltip.Position = new Vector2(ExperienceLevelIconImage.transform.position.x - 35, ExperienceLevelIconImage.transform.position.y + 59);
    }

    public override void OnTooltipOpen(GalaxyTooltip tooltip)
    {
        //Executes the base class's logic for when the tooltip opens.
        base.OnTooltipOpen(tooltip);

        //Sets the position of the experience level tooltip if it is the tooltip that opened.
        if(tooltip == ExperienceLevelTooltip)
            ExperienceLevelTooltip.Position = new Vector2(ExperienceLevelIconImage.transform.position.x - 35, ExperienceLevelIconImage.transform.position.y + 59);
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
        ArmyManagementMenu.UnitListButtonSelected = this;

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
        beginDragLocalYPosition = (gameObject.GetComponent<ExpandableUnitListButton>() != null && gameObject.GetComponent<ExpandableUnitListButton>().Expanded) ? button.transform.localPosition.y - (ArmyManagementMenu.SpacingBetweenUnitListButtonTypes / 2) : button.transform.localPosition.y;
        //Saves the initial y offset.
        beginDragYOffset = button.transform.position.y - transform.position.y;
        //Sets the parent of the button component to the parent of buttons that are being dragged in order to ensure that the button is on top of all of the other buttons that are not being dragged.
        button.transform.SetParent(ArmyManagementMenu.ButtonsBeingDraggedParent);
        //Saves the y offset of the button from the mouse's position.
        dragYOffsetFromMouse = button.transform.position.y - Input.mousePosition.y;
        //Logs that the unit list button is being dragged.
        beingDragged = true;

        //Sets the unit list button as the one selected and displayed in the unit inspector.
        ArmyManagementMenu.UnitListButtonSelected = this;
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
        if (ExperienceLevelTooltip.Open)
            ExperienceLevelTooltip.Position = new Vector2(ExperienceLevelIconImage.transform.position.x - 35, ExperienceLevelIconImage.transform.position.y + 59);
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
        if (LatestButtonMoveSuccessful)
            ArmyManagementMenu.OnUnitListButtonMove();
    }

    /// <summary>
    /// Trys to move the unit list button to its new position and modifies a bool on whether or not a move actually occured.
    /// </summary>
    /// <returns></returns>
    private void ExecuteButtonMove()
    {
        //Button move unsuccesful if the button that is being moved is the only button in the unit list.
        if(transform.parent.childCount <= 1)
        {
            LatestButtonMoveSuccessful = false;
            return;
        }

        //Finds the unit list button above the current position where this unit list button has been dragged to.
        UnitListButton buttonAbove = FindButtonAbove();
        //Finds the unit list button below the current position where this unit list button has been dragged to.
        UnitListButton buttonBelow = FindButtonBelow();
        //Finds the new sibling index this unit list button would be at if the move was successful.
        int newSiblingIndex = buttonAbove == null ? 0 : buttonAbove.transform.GetSiblingIndex() + 1;
        //Executes unit list button movement logic based on what type of button this unit list button that was being dragged is.
        switch (TypeOfButton)
        {
            case ButtonType.Army:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && (buttonAbove == null || buttonBelow == null || buttonBelow.TypeOfButton == ButtonType.Army))
                {
                    LatestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    LatestButtonMoveSuccessful = false;
                    return;
                }
            case ButtonType.Squad:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && ((buttonAbove != null && buttonAbove.TypeOfButton == ButtonType.Army) || (buttonBelow != null && buttonBelow.TypeOfButton == ButtonType.Squad) || (buttonBelow != null && buttonBelow.TypeOfButton == ButtonType.Army && buttonAbove != null)))
                {
                    if (buttonAbove != null && buttonAbove.TypeOfButton == ButtonType.Army && !buttonAbove.gameObject.GetComponent<ArmyButton>().Expanded)
                        buttonAbove.gameObject.GetComponent<ArmyButton>().ForceExpand(false);
                    LatestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    LatestButtonMoveSuccessful = false;
                    return;
                }
            case ButtonType.Pill:
                //Continues on into the if statement if this unit list button was dragged to a valid location in the unit list.
                if (newSiblingIndex != transform.GetSiblingIndex() && ((buttonAbove != null && buttonAbove.TypeOfButton == ButtonType.Squad) || (buttonAbove != null && buttonAbove.TypeOfButton == ButtonType.Pill)))
                {
                    if (buttonAbove != null && buttonAbove.TypeOfButton == ButtonType.Squad && !buttonAbove.gameObject.GetComponent<SquadButton>().Expanded)
                        buttonAbove.gameObject.GetComponent<SquadButton>().ForceExpand(false);
                    LatestButtonMoveSuccessful = true;
                    break;
                }
                //This unit list button was dragged to an invalid location in the unit list.
                else
                {
                    LatestButtonMoveSuccessful = false;
                    return;
                }

            //If there is no logic implemented for a type of unit list button to move then all moves with that button will be unsuccessful.
            default:
                Debug.Log("Button move logic for button type " + TypeOfButton.ToString() + " does not exist.");
                LatestButtonMoveSuccessful = false;
                return;
        }

        //Executes logic for when the button should be moved successfully.

        //Decreases the new sibling index for this unit list button if it's original sibling index is less than the new one.
        if (transform.GetSiblingIndex() < newSiblingIndex)
            newSiblingIndex--;

        //Updates the spacing on the unit list button that used to be above this unit list button.
        if (transform.GetSiblingIndex() > 0)
            transform.parent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
        //Updates the spacing on the unit list button that used to be below this unit list button.
        if (transform.GetSiblingIndex() < transform.parent.childCount - 1)
            transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
        //Updates the spacing on the unit list buttons near the new sibling index for this unit list button.
        if(newSiblingIndex - 1 >= 0)
            transform.parent.GetChild(newSiblingIndex - 1).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
        transform.parent.GetChild(newSiblingIndex).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;
        if (newSiblingIndex + 1 < transform.parent.childCount)
            transform.parent.GetChild(newSiblingIndex + 1).GetComponent<UnitListButton>().SpacingUpdateRequiredNextFrame = true;

        //Stores the original sibling index of this unit list button.
        int originalSiblingIndex = transform.GetSiblingIndex();
        //Moves this unit list button to its new sibling index.
        transform.SetSiblingIndex(newSiblingIndex);

        //Updates the spacing on this unit list button after it has been moved.
        SpacingUpdateRequiredNextFrame = true;

        //Saves the moving of the unit list button based on the unit list button type.
        switch (TypeOfButton)
        {
            case ButtonType.Army:
                //Finds the original index the army was at in the list of armies on the planet.
                int originalArmyIndex = 0;
                for(int armyIndex = 0; armyIndex < ArmyManagementMenu.PlanetSelected.armyCount; armyIndex++)
                {
                    if(ArmyManagementMenu.PlanetSelected.GetArmyAt(armyIndex) == gameObject.GetComponent<ArmyButton>().AssignedArmy)
                    {
                        originalArmyIndex = armyIndex;
                        break;
                    }
                }
                //Finds the new army index.
                int newArmyIndex = 0;
                for(int siblingIndex = 0; siblingIndex <= newSiblingIndex; siblingIndex++)
                {
                    UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                    if(buttonAtSiblingIndex.TypeOfButton == ButtonType.Army)
                    {
                        if(buttonAtSiblingIndex.gameObject.GetComponent<ArmyButton>().AssignedArmy == gameObject.GetComponent<ArmyButton>().AssignedArmy)
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
                ArmyManagementMenu.PlanetSelected.ChangeArmyIndex(originalArmyIndex, newArmyIndex);
                break;
            case ButtonType.Squad:
                GalaxySquad squad = gameObject.GetComponent<SquadButton>().AssignedSquad;
                //Stores the original parent army button.
                ArmyButton originalParentArmyButton = null;
                for(int siblingIdex = 0; siblingIdex < transform.parent.childCount; siblingIdex++)
                {
                    if (transform.parent.GetChild(siblingIdex).GetComponent<UnitListButton>().TypeOfButton == ButtonType.Army && transform.parent.GetChild(siblingIdex).GetComponent<ArmyButton>().AssignedArmy == squad.assignedArmy)
                    {
                        originalParentArmyButton = transform.parent.GetChild(siblingIdex).GetComponent<ArmyButton>();
                        break;
                    }
                }
                //Removes the squad from its current army.
                squad.assignedArmy.RemoveSquad(squad);
                //Finds the new army the squad will be assigned to.
                GalaxyArmy newArmyAssigned = null;
                ArmyButton parentArmyButton = null;
                for(int siblingIndex = newSiblingIndex; siblingIndex >= 0; siblingIndex--)
                {
                    UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                    if (buttonAtSiblingIndex.TypeOfButton == ButtonType.Army)
                    {
                        parentArmyButton = buttonAtSiblingIndex.gameObject.GetComponent<ArmyButton>();
                        newArmyAssigned = parentArmyButton.AssignedArmy;
                        break;
                    }
                }
                //Assigns the squad to its new army.
                newArmyAssigned.InsertSquad(newSiblingIndex - parentArmyButton.transform.GetSiblingIndex() - 1, squad);
                //Updates the information displayed on the new parent army button.
                parentArmyButton.UpdateInfo();
                //Updates the information displayed on the original parent army button.
                originalParentArmyButton.UpdateInfo();
                break;
            case ButtonType.Pill:
                GalaxyPill pill = gameObject.GetComponent<PillButton>().AssignedPill;
                bool squadLeader = pill.isSquadLeader;
                //Stores the original parent buttons.
                List<UnitListButton> originalParentButtons = new List<UnitListButton>();
                for (int siblingIndex = 0; siblingIndex < transform.parent.childCount; siblingIndex++)
                {
                    UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                    if ((buttonAtSiblingIndex.TypeOfButton == ButtonType.Army && buttonAtSiblingIndex.gameObject.GetComponent<ArmyButton>().AssignedArmy == pill.assignedSquad.assignedArmy) || (buttonAtSiblingIndex.TypeOfButton == ButtonType.Squad && buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>().AssignedSquad == pill.assignedSquad))
                    {
                        originalParentButtons.Add(buttonAtSiblingIndex);
                        if (originalParentButtons.Count >= 2)
                            break;
                    }
                }
                //Removes the pill from its current squad.
                GalaxySquad originalSquad = pill.assignedSquad;
                pill.assignedSquad.RemovePill(pill);
                //Finds the new squad the pill will be assigned to and the new parent buttons of this button (squad parent at index 0 and army parent at index 1).
                GalaxySquad newSquadAssigned = null;
                List<UnitListButton> newParentButtons = new List<UnitListButton>();
                for(int siblingIndex = newSiblingIndex; siblingIndex >= 0; siblingIndex--)
                {
                    UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                    if(buttonAtSiblingIndex.TypeOfButton == ButtonType.Squad && newParentButtons.Count == 0)
                    {
                        newParentButtons.Add(buttonAtSiblingIndex);
                        newSquadAssigned = buttonAtSiblingIndex.gameObject.GetComponent<SquadButton>().AssignedSquad;
                    }
                    else if (buttonAtSiblingIndex.TypeOfButton == ButtonType.Army)
                    {
                        newParentButtons.Add(buttonAtSiblingIndex);
                        break;
                    }
                }
                //Assigns the pill to its new squad.
                newSquadAssigned.InsertPill(newSiblingIndex - newParentButtons[0].transform.GetSiblingIndex() - 1, pill);
                //Sets the pill back as the squad leader if it was before.
                if (squadLeader && newSquadAssigned == originalSquad)
                    originalSquad.squadLeader = pill;
                //Updates the information displayed on the new parent buttons.
                foreach (UnitListButton newParentButton in newParentButtons)
                    newParentButton.UpdateInfo();
                //Updates the information displayed on the original parent buttons.
                foreach (UnitListButton originalParentButton in originalParentButtons)
                    originalParentButton.UpdateInfo();
                break;

            default:
                Debug.Log("Save logic does not exist for unit list button type: " + TypeOfButton.ToString());
                break;
        }

        //Plays the sound effect for the unit list button being successfully moved to another location in the unit list.
        AudioManager.PlaySFX(movedSFX);
    }

    /// <summary>
    /// Finds the unit list button right above the position of this unit list button and returns it.
    /// </summary>
    /// <returns></returns>
    private UnitListButton FindButtonAbove()
    {
        //Loops through every unit list button in the unit list backwards in order to ensure that the first unit list button above this unit list button is the one returned.
        for(int siblingIndex = transform.parent.childCount - 1; siblingIndex >= 0; siblingIndex--)
        {
            //Ignores this sibling index if the unit list button at this sibling index is this unit list button.
            if(siblingIndex == transform.GetSiblingIndex())
                continue;

            //Returns the button at the sibling index if its position is above this unit list button's position.
            UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if (buttonAtSiblingIndex.button.transform.position.y >= button.transform.position.y)
                return buttonAtSiblingIndex;
        }

        //Returns null, as no unit list button was found to be above this unit list button.
        return null;
    }

    /// <summary>
    /// Finds the unit list button right below the position this unit list button was dragged to and returns it.
    /// </summary>
    /// <returns></returns>
    private UnitListButton FindButtonBelow()
    {
        //Loops through every unit list button in the unit list in sequential or forwards order to ensure that the first unit list button found below this unit list button is indeed the one right below it and not further below it.
        for(int siblingIndex = 0; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            //Ignores this sibling index if the unit list button at this sibling index is this unit list button.
            if (siblingIndex == transform.GetSiblingIndex())
                continue;

            //Returns the button at the sibling index if its position is below this unit list button's position.
            UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if (buttonAtSiblingIndex.button.transform.position.y < button.transform.position.y)
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
        AudioManager.PlaySFX(PointerEnterSFX);
    }

    /// <summary>
    /// This method is called whenever the pointer enters the left image.
    /// </summary>
    /*public virtual void OnPointerEnterLeftImage()
    {
        leftImageTooltip.InstantaneousPosition = new Vector2(LeftImage.transform.position.x - 22, LeftImage.transform.position.y + 32);
    }*/

    /// <summary>
    /// This method should be called in order to update the spacing between the unit list buttons, which is done by determining the height of each unit list button rect transform.
    /// </summary>
    private void UpdateSpacing()
    {
        RectTransform rectTransform = (RectTransform)transform;
        if (transform.GetSiblingIndex() + 1 < transform.parent.childCount && transform.parent.GetChild(transform.GetSiblingIndex() + 1).GetComponent<UnitListButton>().TypeOfButton != TypeOfButton)
        {
            if (Mathf.Approximately(rectTransform.sizeDelta.y, InitialHeight))
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, InitialHeight + ArmyManagementMenu.SpacingBetweenUnitListButtonTypes);
            }
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, InitialHeight);
        }

        //Weird code essential for preventing a dumb unity bug.
        float verticalLayoutGroupSpacing = ArmyManagementMenu.UnitListVerticalLayoutGroup.spacing;
        ArmyManagementMenu.UnitListVerticalLayoutGroup.spacing = verticalLayoutGroupSpacing + 1;
        ArmyManagementMenu.UnitListVerticalLayoutGroup.spacing = verticalLayoutGroupSpacing;

        //Logs that the spacing has been updated.
        spacingUpdateRequiredNextFrame = false;
    }

    /// <summary>
    /// This method should be called in order to update the text on the button to accurately reflect information on the assigned ground unit.
    /// </summary>
    public virtual void UpdateInfo()
    {
        //Updates the name text of the button to reflect the name of the ground unit.
        NameText.text = AssignedGroundUnit.name;
        //Updates the experience level text of the button to reflect the experience level of the ground unit. 
        ExperienceLevelText.text = AssignedGroundUnit.experienceLevel.ToString();
    }

    /// <summary>
    /// This method should be called in order to disband the button's assigned ground unit.
    /// </summary>
    public virtual void DisbandAssignedGroundUnit()
    {
        //Fills a list with all child buttons in the unit list that need to be destroyed.
        List<UnitListButton> childButtonsToDestroy = new List<UnitListButton>();
        for(int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if (buttonAtSiblingIndex.TypeOfButton <= TypeOfButton)
                break;
            childButtonsToDestroy.Add(buttonAtSiblingIndex);
        }

        //Destroys all neccessary child buttons in the unit list.
        foreach(UnitListButton childButton in childButtonsToDestroy)
        {
            Destroy(childButton.gameObject);
        }

        //Destroys this unit list button.
        Destroy(gameObject);

        //No button in the unit list is selected after the disbanding action is complete.
        ArmyManagementMenu.UnitListButtonSelected = null;
    }
}
