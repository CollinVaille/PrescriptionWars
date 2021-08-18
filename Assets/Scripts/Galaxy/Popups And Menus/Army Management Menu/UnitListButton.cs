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
    public ButtonType TypeOfButton
    {
        get
        {
            return buttonType;
        }
    }
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
    public float InitialHeight
    {
        get
        {
            return initialHeight;
        }
    }

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
    protected bool BeingDragged
    {
        get
        {
            return beingDragged;
        }
    }
    /// <summary>
    /// Indicates whether the unit list button is currently being dragged.
    /// </summary>
    private bool beingDragged = false;

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
        //Triggers the OnClickWithoutDrag() method in the expandable unit list button script if it exists.
        ExpandableUnitListButton expandableUnitListButtonComponent = gameObject.GetComponent<ExpandableUnitListButton>();
        if (expandableUnitListButtonComponent != null)
            expandableUnitListButtonComponent.OnClickWithoutDrag();
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
        if (ButtonMoveSuccessful())
        {
            Debug.Log("Move Successful");
        }
        else
        {
            Debug.Log("Move Not Successful");
        }
        //Reverts the parent of the button component to be the unit list button again.
        button.transform.SetParent(transform);
        //Reverts the position of the actual button component to its local position when it was just beginning to be dragged.
        button.transform.localPosition = new Vector2(button.transform.localPosition.x, beginDragLocalYPosition);
        //Logs that the unit list button is no longer being dragged.
        beingDragged = false;
    }

    /// <summary>
    /// Trys to move the unit list button to its new position and returns a bool on whether or not a move actually occured.
    /// </summary>
    /// <returns></returns>
    private bool ButtonMoveSuccessful()
    {
        int newButtonSiblingIndex = 0;
        while (true)
        {
            //Checks if the button's position is on top of the unit list, if so then it continues on into the if statement.
            if (button.transform.localPosition.y >= 120 + ((RectTransform)button.transform).sizeDelta.y)
            {
                //Checks if the button's type is largest unit type possible (Army), if so then it continues into the if statement.
                if (TypeOfButton == 0)
                {
                    //Indicates that the button should be moved to the very first position in the unit list.
                    newButtonSiblingIndex = 0;
                    //Breaks out of the while loop that determines the new sibling index of the button.
                    break;
                }
                //If the button's type is not the largest unit type possible (Army), then it will not be moved anywhere.
                return false;
            }

            //Checks if the button's position is below the unit list, if so then it continues on into the if statement.
            if (button.transform.localPosition.y <= -106 - ((RectTransform)button.transform).sizeDelta.y)
            {
                //Checks if the button's type is largest unit type possible (Army), if so then it continues into the if statement.
                if (TypeOfButton == 0)
                {
                    //Indicates that the button should be moved to the very last position in the unit list.
                    newButtonSiblingIndex = transform.parent.childCount - 1;
                    //Breaks out of the while loop that determines the new sibling index of the button.
                    break;
                }
                //If the button's type is not the largest unit type possible (Army), then it will not be moved anywhere.
                return false;
            }

            //Will be assigned to the unit list button that is above the location that this unit list button was dragged to.
            UnitListButton buttonAboveDragLocation = null;
            //Indicates whether the exterior while loop should be broken out of once the for loop has finished.
            bool breakOutOfExteriorWhileLoop = false;
            for(int siblingIndex = 0; siblingIndex < transform.parent.childCount; siblingIndex++)
            {
                //Skips checking the sibling index if it is the same sibling index as this unit list button.
                if (siblingIndex == transform.GetSiblingIndex())
                    continue;

                //Gets the unit list button component of the button at the specified sibling index.
                UnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                //Checks if the position this button was dragged to is above the position of the button at the specified sibling index.
                if (button.transform.position.y > buttonAtSiblingIndex.button.transform.position.y)
                {
                    //Account for sibling index possibly being 0.
                    if(siblingIndex == 0)
                    {
                        //Button move unsuccessful if the player dragged it to the top of the unit list but it is not an army button.
                        if (TypeOfButton != 0)
                            return false;

                        //Button moves to the top of the list if it is an army button.
                        newButtonSiblingIndex = 0;
                        breakOutOfExteriorWhileLoop = true;
                        break;
                    }

                    //Button move unsuccessful if its new sibling index is the same sibling index as before.
                    if (siblingIndex - 1 == transform.GetSiblingIndex())
                        return false;

                    //Executes the regular checking logic.
                    buttonAboveDragLocation = transform.parent.GetChild(siblingIndex - 1).GetComponent<UnitListButton>();
                    //Continues on into the if statement if the button above is a smaller unit list button type (Ex: button above type = squad, button type = army).
                    if(buttonAboveDragLocation.TypeOfButton > TypeOfButton)
                    {
                        //Button move unsuccessful if the button below the drag location is not the same or a bigger unit list button type.
                        if (transform.parent.GetChild(buttonAboveDragLocation.transform.GetSiblingIndex() + 1).GetComponent<UnitListButton>().TypeOfButton > TypeOfButton)
                            return false;

                        //Button moves to the sibling index after the button above where it was dragged.
                        newButtonSiblingIndex = buttonAboveDragLocation.transform.GetSiblingIndex() + 1;
                        //Breaks out of the exterior while loop after this for loop in order to avoid reaching unnecessary logic.
                        breakOutOfExteriorWhileLoop = true;
                    }
                    //Continues on into the if statement if the button above is the same unit list button type (Ex: button above type = squad, button type = squad).
                    else if (buttonAboveDragLocation.TypeOfButton == TypeOfButton)
                    {
                        //Button moves to the sibling index after the button above where it was dragged.
                        newButtonSiblingIndex = buttonAboveDragLocation.transform.GetSiblingIndex() + 1;
                        //Breaks out of the exterior while loop after this for loop in order to avoid reaching unnecessary logic.
                        breakOutOfExteriorWhileLoop = true;
                    }
                    //Continues on into the if statement if the button above is a bigger unit list button type (Ex: button above type = squad, button type = pill).
                    else if (buttonAboveDragLocation.TypeOfButton < TypeOfButton)
                    {
                        //Button move unsuccessful if the button above's type is not the parent type of this button's type (Successful Ex: button above type = army, button type = squad; Unsuccessful Ex: button above type = army, button type = pill).
                        if (buttonAboveDragLocation.TypeOfButton < TypeOfButton - 1)
                            return false;

                        //Button moves to the sibling index after the button above where it was dragged.
                        newButtonSiblingIndex = buttonAboveDragLocation.transform.GetSiblingIndex() + 1;
                        //Breaks out of the exterior while loop after this for loop in order to avoid reaching unnecessary logic.
                        breakOutOfExteriorWhileLoop = true;
                    }

                    //Corrects the new sibling index of the button in order to account for the button still being in the unit list.
                    if (transform.GetSiblingIndex() < newButtonSiblingIndex)
                        newButtonSiblingIndex--;
                }
            }
            //Breaks out of the exterior while loop if instructed to do so by logic side of the for loop above.
            if (breakOutOfExteriorWhileLoop)
                break;

            //Accounts for none of the buttons being below the dragged button's position here.
            UnitListButton buttonAtLastSiblingIndex = transform.parent.GetChild(transform.parent.childCount - 1).GetComponent<UnitListButton>();
            if (TypeOfButton == 0 || TypeOfButton == buttonAtLastSiblingIndex.TypeOfButton || TypeOfButton - 1 == buttonAtLastSiblingIndex.TypeOfButton)
            {
                //Button moves to the last position possible in the unit list.
                newButtonSiblingIndex = transform.parent.childCount - 1;
                break;
            }

            break;
        }

        Debug.Log("Moved to " + newButtonSiblingIndex);
        return true;
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
    /// This method should be called whenever the galaxy ground unit (pill, squad, or army) is set.
    /// </summary>
    /// <param name="groundUnit"></param>
    protected void OnGalaxyGroundUnitValueSet(GalaxyGroundUnit groundUnit)
    {
        ExperienceLevelText.text = groundUnit.ExperienceLevel.ToString();
    }
}
