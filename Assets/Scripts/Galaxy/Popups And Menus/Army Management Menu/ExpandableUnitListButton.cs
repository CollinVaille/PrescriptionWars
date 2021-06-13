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

    new public virtual void OnClick()
    {
        base.OnClick();

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
        //Gathers a list of all of the unit list buttons to destroy.
        List<UnitListButton> unitListButtonsToDestroy = new List<UnitListButton>();
        for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            UnitListButton unitListButton = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if ((int)unitListButton.TypeOfButton > (int)TypeOfButton)
            {
                unitListButtonsToDestroy.Add(unitListButton);
            }
            else
            {
                break;
            }
        }
        //Destroys all of the unit list buttons that are a result of the expandable unit list button being expanded.
        foreach (UnitListButton unitListButtonToDestroy in unitListButtonsToDestroy)
        {
            Destroy(unitListButtonToDestroy.gameObject);
        }

        //Updates the amount of spacing between the squad button and the buttons that follow.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the squad button is currently collapsed (not expanded).
        expanded = false;
    }
}
