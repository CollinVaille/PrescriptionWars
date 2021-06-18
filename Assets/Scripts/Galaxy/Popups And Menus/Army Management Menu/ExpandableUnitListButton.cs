using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandableUnitListButton : UnitListButton
{
    [Header("Expandable Button Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private protected bool expanded = false;
    public bool Expanded
    {
        get
        {
            return expanded;
        }
    }

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
    new public virtual void OnClickWithoutDrag()
    {
        if (!expanded)
        {
            if(TypeOfButton == ButtonType.Army)
            {
                if(gameObject.GetComponent<ArmyButton>().AssignedArmy.TotalNumberOfSquads > 0)
                    Expand();
            }
            else if (TypeOfButton == ButtonType.Squad)
            {
                if (gameObject.GetComponent<SquadButton>().AssignedSquad.TotalNumberOfPills > 0)
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
    new public virtual void OnBeginDrag()
    {
        //Saves the initial y position of the actual button component.
        base.OnBeginDrag();

        //Collapses the expandable unit list button if it is expanded.
        if (expanded)
            Collapse();
    }

    /// <summary>
    /// This method should be called in order to expand the unit list button and reveal the appropriate child buttons.
    /// </summary>
    public virtual void Expand()
    {
        //Gets the total amount of child buttons that need to be created.
        int childCount = 0;
        if (TypeOfButton == ButtonType.Army)
            childCount = gameObject.GetComponent<ArmyButton>().AssignedArmy.TotalNumberOfSquads;
        else if (TypeOfButton == ButtonType.Squad)
            childCount = gameObject.GetComponent<SquadButton>().AssignedSquad.TotalNumberOfPills;
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
    }

    /// <summary>
    /// This method should be called in order to collapse the unit list button and destroy the child buttons.
    /// </summary>
    public virtual void Collapse()
    {
        //Destroys all of the unit list buttons that are a result of the expandable unit list button being expanded.
        foreach (UnitListButton childUnitListButton in ChildButtons)
        {
            ArmyManagementMenu.UnitListButtonDestroyer.AddUnitListButtonToDestroy(childUnitListButton);
        }

        //Updates the amount of spacing between the squad button and the buttons that follow.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the squad button is currently collapsed (not expanded).
        expanded = false;
    }
}