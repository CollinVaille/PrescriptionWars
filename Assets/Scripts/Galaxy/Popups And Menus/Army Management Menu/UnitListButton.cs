using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

abstract public class UnitListButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
        if(ExperienceLevelTooltip.Open)
            ExperienceLevelTooltip.Position = new Vector2(ExperienceLevelIconImage.transform.position.x - 35, ExperienceLevelIconImage.transform.position.y + 59);
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
        //Executes the logic in the expandable unit list button class for when it is beginning to be dragged.
        ExpandableUnitListButton expandableUnitListButton = gameObject.GetComponent<ExpandableUnitListButton>();
        if (expandableUnitListButton != null && !expandableUnitListButton.ExecutingBeginDragLogic)
        {
            expandableUnitListButton.OnBeginDrag(pointerEventData);
            return;
        }

        //Ignores any begin drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;
        //Saves the initial local y position of the actual button component before it begins to be dragged.
        beginDragLocalYPosition = (gameObject.GetComponent<ExpandableUnitListButton>() != null && gameObject.GetComponent<ExpandableUnitListButton>().Expanded) ? button.transform.localPosition.y - (ArmyManagementMenu.SpacingBetweenUnitListButtonTypes / 2) : button.transform.localPosition.y;
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
    }

    /// <summary>
    /// This method is called through the ending of a drag on a unit list button and determines the new position of the unit list button and logs that the unit list button is no longer being dragged.
    /// </summary>
    public virtual void OnEndDrag(PointerEventData pointerEventData)
    {
        //Ignores any end drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;
        //Reverts the parent of the button component to be the unit list button again.
        button.transform.SetParent(transform);
        //Reverts the position of the actual button component to its local position when it was just beginning to be dragged.
        button.transform.localPosition = new Vector2(button.transform.localPosition.x, beginDragLocalYPosition);
        //Logs that the unit list button is no longer being dragged.
        beingDragged = false;
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
