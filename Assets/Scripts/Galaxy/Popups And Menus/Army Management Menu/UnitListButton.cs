using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitListButton : MonoBehaviour
{
    [Header("Base Components")]

    [SerializeField] private Button button = null;
    protected Button Button
    {
        get
        {
            return button;
        }
    }

    [SerializeField] private Image leftImage = null;
    protected Image LeftImage
    {
        get
        {
            return leftImage;
        }
    }

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
    /// Indicates the y position of the actual button component when the button component was just beginning to be dragged.
    /// </summary>
    private float beginDragYPosition = 0;

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
    public virtual void OnBeginDrag()
    {
        //Saves the initial y position of the actual button component before it begins to be dragged.
        beginDragYPosition = button.transform.position.y;
        //Logs that the unit list button is being dragged.
        beingDragged = true;
    }

    /// <summary>
    /// This method is called through the dragging a unit list button and changes the y position of the button to match the y position of the mouse.
    /// </summary>
    public virtual void OnDrag()
    {
        //Changes the y position of the actual button component to match the y position of the mouse.
        button.transform.position = new Vector2(button.transform.position.x, Input.mousePosition.y);
    }

    /// <summary>
    /// This method is called through the ending of a drag on a unit list button and determines the new position of the unit list button and logs that the unit list button is no longer being dragged.
    /// </summary>
    public virtual void OnEndDrag()
    {
        //Reverts the position of the actual button component to its position when it was just beginning to be dragged.
        button.transform.position = new Vector2(button.transform.position.x, beginDragYPosition);
        //Logs that the unit list button is no longer being dragged.
        beingDragged = false;
    }

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
}
