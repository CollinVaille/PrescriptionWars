using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExpandableUnitListButton : UnitListButton
{
    [Header("Expandable Button SFX Options")]

    [SerializeField, Tooltip("The sound effect that should be played whenever the expandable unit list button expands to reveal its child buttons.")] private AudioClip expandSFX = null;
    /// <summary>
    /// The sound effect that should be played whenever the expandable unit list button expands to reveal its child buttons.
    /// </summary>
    protected AudioClip ExpandSFX { get => expandSFX; }

    [SerializeField, Tooltip("The sound effect that should be played whenever the expandable unit list button collapses and hides its child buttons.")] private AudioClip collapseSFX = null;
    /// <summary>
    /// The sound effect that should be played whenever the expandable unit list button collapses and hides its child buttons.
    /// </summary>
    protected AudioClip CollapseSFX { get => collapseSFX; }

    [Header("Expandable Button Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField, Tooltip("Indicates whether the expandable unit list button is expanded and showing its child buttons or collapsed and not showing its child buttons.")] private protected bool expanded = false;
    /// <summary>
    /// Indicates whether the expandable unit list button is expanded or not.
    /// </summary>
    public bool Expanded { get => expanded; }

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Returns the prefab for the child unit list button type (Ex: Returns squad button prefab for army buttons).
    /// </summary>
    private GameObject ChildPrefab
    {
        get
        {
            switch (TypeOfButton)
            {
                case ButtonType.Army:
                    return ArmyManagementMenu.SquadButtonPrefab;
                case ButtonType.Squad:
                    return ArmyManagementMenu.PillButtonPrefab;

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Returns a list that contains all of the unit list buttons that are a child to this unit list button.
    /// This list cannot be edited directly and adding or removing elements from this list will do nothing.
    /// </summary>
    protected List<UnitListButton> ChildButtons
    {
        get
        {
            //Gathers all of the child buttons of the unit list button into a list.
            List<UnitListButton> childButtons = new List<UnitListButton>();
            for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
            {
                //Gets the unit list button component from the gameobject at the specified sibling index.
                UnitListButton possibleChildUnitListButton = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
                //Breaks out of the for loop if the type of button of the possible child button is not a child button type of this unit list button.
                if ((int)possibleChildUnitListButton.TypeOfButton <= (int)TypeOfButton)
                    break;
                //Adds the child unit list button to the list of child unit list buttons of this unit list button.
                childButtons.Add(possibleChildUnitListButton);
            }

            //Returns the list that contains all of the child unit list buttons of this unit list button.
            return childButtons;
        }
    }

    /// <summary>
    /// This method is called through the on click method whenever it is detetcted that the expandable unit list button was clicked while not being dragged.
    /// This method expands or collapses the expandable unit list button and selects it to show up in the unit inspector.
    /// </summary>
    public override void OnClickWithoutDrag()
    {
        base.OnClickWithoutDrag();

        if (!expanded)
        {
            if(TypeOfButton == ButtonType.Army)
            {
                if(gameObject.GetComponent<ArmyButton>().AssignedArmy.squadCount > 0)
                    Expand();
            }
            else if (TypeOfButton == ButtonType.Squad)
            {
                if (gameObject.GetComponent<SquadButton>().AssignedSquad.pillCount > 0)
                    Expand();
            }
        }
        else
        {
            Collapse();
        }
    }

    /// <summary>
    /// This method is called through the starting of a drag on a expandable unit list button and saves the initial y position of the actual button component and collapses the expandable unit list button if it is expanded.
    /// </summary>
    public override void OnBeginDrag(PointerEventData pointerEventData)
    {
        //Ignores any begin drag event if it is not done by the left mouse button.
        if (pointerEventData.button != PointerEventData.InputButton.Left)
            return;

        //Saves the initial y position of the actual button component.
        base.OnBeginDrag(pointerEventData);

        //Collapses the expandable unit list button if it is expanded.
        if (expanded)
            Collapse();
    }

    /// <summary>
    /// This method should be called in order to expand the unit list button and reveal the appropriate child buttons.
    /// </summary>
    public virtual void Expand(bool playSFX = true)
    {
        //Returns if the button has already been expanded.
        if (Expanded)
            return;

        //Gets the total amount of child buttons that need to be created.
        int childCount = 0;
        if (TypeOfButton == ButtonType.Army)
            childCount = gameObject.GetComponent<ArmyButton>().AssignedArmy.squadCount;
        else if (TypeOfButton == ButtonType.Squad)
            childCount = gameObject.GetComponent<SquadButton>().AssignedSquad.pillCount;
        //Ensures that the button actually has child buttons to display before going through the rest of the expanding logic.
        if (childCount > 0)
        {
            //Creates the child unit list buttons.
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                //Instantiates a new unit list button from the child unit list button prefab.
                GameObject childButton = Instantiate(ChildPrefab);
                //Sets the parent of the child unit list button.
                childButton.transform.SetParent(ArmyManagementMenu.UnitListButtonParent);
                //Sets the sibling index of the child unit button.
                childButton.transform.SetSiblingIndex(transform.GetSiblingIndex() + (childIndex + 1));
                //Resets the scale of the child unit list button to 1 in order to avoid any unity shenanigans.
                childButton.transform.localScale = Vector3.one;
                //Initializes some script values in the new child unit list button.
                switch (TypeOfButton)
                {
                    case ButtonType.Army:
                        SquadButton squadButtonScript = childButton.GetComponent<SquadButton>();
                        squadButtonScript.Initialize(ArmyManagementMenu, gameObject.GetComponent<ArmyButton>().AssignedArmy.GetSquadAt(childIndex));
                        break;
                    case ButtonType.Squad:
                        PillButton pillButtonScript = childButton.GetComponent<PillButton>();
                        pillButtonScript.Initialize(ArmyManagementMenu, gameObject.GetComponent<SquadButton>().AssignedSquad.GetPillAt(childIndex));
                        break;
                }
            }

            //Adds the appropriate amount of spacing between the expandable unit list button and the child unit list buttons.
            SpacingUpdateRequiredNextFrame = true;

            //Logs that the expandable unit list button button is currently expanded.
            expanded = true;

            //Plays the appropriate sound effect.
            if(playSFX)
                AudioManager.PlaySFX(ExpandSFX);
        }
    }

    /// <summary>
    /// This method should be called in order to expand the unit list button and force log that it is expanded.
    /// </summary>
    /// <param name="playSFX"></param>
    public virtual void ForceExpand(bool playSFX = true)
    {
        //Expands to reveal the child buttons of this expandable unit list button.
        Expand();

        //Force logs that this expandable unit list button is expanded.
        expanded = true;
    }

    /// <summary>
    /// This method should be called in order to expand this button and all applicable child buttons.
    /// </summary>
    public virtual void ExpandAll()
    {
        //Expands this button without playing the expanding sound effect.
        Expand(false);
        //Finds the number of child buttons this button has.
        int numberOfChildButtons = 0;
        if (TypeOfButton == ButtonType.Army)
            numberOfChildButtons = gameObject.GetComponent<ArmyButton>().AssignedArmy.squadCount;
        else if (TypeOfButton == ButtonType.Squad)
            numberOfChildButtons = gameObject.GetComponent<SquadButton>().AssignedSquad.pillCount;
        else
            Debug.Log("Expand all logic not implemented for unit list button type: " + TypeOfButton.ToString());
        //Finds each expandable child button of this expandable unit list button.
        List<ExpandableUnitListButton> expandableChildButtons = new List<ExpandableUnitListButton>();
        for(int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex <= transform.GetSiblingIndex() + numberOfChildButtons; siblingIndex++)
        {
            ExpandableUnitListButton expandableChildButton = transform.parent.GetChild(siblingIndex).GetComponent<ExpandableUnitListButton>();
            if (expandableChildButton != null)
                expandableChildButtons.Add(expandableChildButton);
        }
        //Loops through each expandable child button and calls its expand all function.
        foreach(ExpandableUnitListButton expandableChildButton in expandableChildButtons)
        {
            expandableChildButton.ExpandAll();
        }
    }

    /// <summary>
    /// This method should be called in order to collapse the unit list button and destroy the child buttons.
    /// </summary>
    public virtual void Collapse(bool playSFX = true)
    {
        //Returns if the button is already collapsed.
        if (!Expanded)
            return;

        //Destroys all of the unit list buttons that are a result of the expandable unit list button being expanded.
        foreach (UnitListButton childUnitListButton in ChildButtons)
        {
            ArmyManagementMenu.UnitListButtonDestroyer.AddUnitListButtonToDestroy(childUnitListButton);
        }

        //Updates the amount of spacing between the squad button and the buttons that follow.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the squad button is currently collapsed (not expanded).
        expanded = false;

        //Plays the appropriate sound effect.
        if(playSFX)
            AudioManager.PlaySFX(CollapseSFX);
    }
}