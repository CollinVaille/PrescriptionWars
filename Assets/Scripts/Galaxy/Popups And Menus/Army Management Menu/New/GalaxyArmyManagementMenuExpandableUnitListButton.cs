using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GalaxyArmyManagementMenuExpandableUnitListButton : GalaxyArmyManagementMenuUnitListButton
{
    [Header("Expandable Button SFX Options")]

    [SerializeField, Tooltip("The sound effect that should be played whenever the expandable unit list button expands to reveal its child buttons.")] private AudioClip _expandSFX = null;
    [SerializeField, Tooltip("The sound effect that should be played whenever the expandable unit list button collapses and hides its child buttons.")] private AudioClip _collapseSFX = null;

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Protected property that should be used in order to access the sound effect that should be played whenever the expandable unit list button expands to reveal its child buttons.
    /// </summary>
    protected AudioClip expandSFX { get => _expandSFX; }

    /// <summary>
    /// Protected property that should be used in order to access the sound effect that should be played whenever the expandable unit list button collapses and hides its child buttons.
    /// </summary>
    protected AudioClip collapseSFX { get => _collapseSFX; }

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether the expandable unit list button is expanded or not.
    /// </summary>
    public bool isExpanded { get; private set; } = false;

    /// <summary>
    /// Returns the prefab for the child unit list button type (Ex: Returns squad button prefab for army buttons).
    /// </summary>
    private GameObject childPrefab
    {
        get
        {
            switch (buttonType)
            {
                case ButtonType.Army:
                    return armyManagementMenu.squadButtonPrefab;
                case ButtonType.Squad:
                    return armyManagementMenu.pillButtonPrefab;

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Returns a list that contains all of the unit list buttons that are a child to this unit list button.
    /// This list cannot be edited directly and adding or removing elements from this list will do nothing.
    /// </summary>
    protected List<GalaxyArmyManagementMenuUnitListButton> childButtons
    {
        get
        {
            //Gathers all of the child buttons of the unit list button into a list.
            List<GalaxyArmyManagementMenuUnitListButton> childButtons = new List<GalaxyArmyManagementMenuUnitListButton>();
            for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
            {
                //Gets the unit list button component from the gameobject at the specified sibling index.
                GalaxyArmyManagementMenuUnitListButton possibleChildUnitListButton = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();
                //Breaks out of the for loop if the type of button of the possible child button is not a child button type of this unit list button.
                if ((int)possibleChildUnitListButton.buttonType <= (int)buttonType)
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

        if (!isExpanded)
        {
            if (buttonType == ButtonType.Army)
            {
                if (gameObject.GetComponent<GalaxyArmyButton>().assignedArmy.squads.Count > 0)
                    Expand();
            }
            else if (buttonType == ButtonType.Squad)
            {
                if (gameObject.GetComponent<GalaxySquadButton>().assignedSquad.pills.Count > 0)
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
        if (isExpanded)
            Collapse();
    }

    /// <summary>
    /// This method should be called in order to expand the unit list button and reveal the appropriate child buttons.
    /// </summary>
    public virtual void Expand(bool playSFX = true)
    {
        //Returns if the button has already been expanded.
        if (isExpanded)
            return;

        //Gets the total amount of child buttons that need to be created.
        int childCount = 0;
        if (buttonType == ButtonType.Army)
            childCount = gameObject.GetComponent<GalaxyArmyButton>().assignedArmy.squads.Count;
        else if (buttonType == ButtonType.Squad)
            childCount = gameObject.GetComponent<GalaxySquadButton>().assignedSquad.pills.Count;
        //Ensures that the button actually has child buttons to display before going through the rest of the expanding logic.
        if (childCount > 0)
        {
            //Creates the child unit list buttons.
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                //Instantiates a new unit list button from the child unit list button prefab.
                GameObject childButton = Instantiate(childPrefab);
                //Sets the parent of the child unit list button.
                childButton.transform.SetParent(armyManagementMenu.unitListButtonParent);
                //Sets the sibling index of the child unit button.
                childButton.transform.SetSiblingIndex(transform.GetSiblingIndex() + (childIndex + 1));
                //Resets the scale of the child unit list button to 1 in order to avoid any unity shenanigans.
                childButton.transform.localScale = Vector3.one;
                //Initializes some script values in the new child unit list button.
                switch (buttonType)
                {
                    case ButtonType.Army:
                        GalaxySquadButton squadButtonScript = childButton.GetComponent<GalaxySquadButton>();
                        squadButtonScript.Initialize(armyManagementMenu, gameObject.GetComponent<GalaxyArmyButton>().assignedArmy.squads[childIndex]);
                        break;
                    case ButtonType.Squad:
                        GalaxyPillButton pillButtonScript = childButton.GetComponent<GalaxyPillButton>();
                        pillButtonScript.Initialize(armyManagementMenu, gameObject.GetComponent<GalaxySquadButton>().assignedSquad.pills[childIndex]);
                        break;
                }
            }

            //Adds the appropriate amount of spacing between the expandable unit list button and the child unit list buttons.
            spacingUpdateRequiredNextFrame = true;

            //Logs that the expandable unit list button button is currently expanded.
            isExpanded = true;

            //Plays the appropriate sound effect.
            if (playSFX)
                AudioManager.PlaySFX(expandSFX);
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
        isExpanded = true;
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
        if (buttonType == ButtonType.Army)
            numberOfChildButtons = gameObject.GetComponent<GalaxyArmyButton>().assignedArmy.squads.Count;
        else if (buttonType == ButtonType.Squad)
            numberOfChildButtons = gameObject.GetComponent<GalaxySquadButton>().assignedSquad.pills.Count;
        else
            Debug.Log("Expand all logic not implemented for unit list button type: " + buttonType.ToString());
        //Finds each expandable child button of this expandable unit list button.
        List<GalaxyArmyManagementMenuExpandableUnitListButton> expandableChildButtons = new List<GalaxyArmyManagementMenuExpandableUnitListButton>();
        for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex <= transform.GetSiblingIndex() + numberOfChildButtons; siblingIndex++)
        {
            GalaxyArmyManagementMenuExpandableUnitListButton expandableChildButton = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuExpandableUnitListButton>();
            if (expandableChildButton != null)
                expandableChildButtons.Add(expandableChildButton);
        }
        //Loops through each expandable child button and calls its expand all function.
        foreach (GalaxyArmyManagementMenuExpandableUnitListButton expandableChildButton in expandableChildButtons)
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
        if (!isExpanded)
            return;

        //Destroys all of the unit list buttons that are a result of the expandable unit list button being expanded.
        foreach (GalaxyArmyManagementMenuUnitListButton childUnitListButton in childButtons)
        {
            armyManagementMenu.unitListButtonDestroyer.AddUnitListButtonToDestroy(childUnitListButton);
        }

        //Updates the amount of spacing between the squad button and the buttons that follow.
        spacingUpdateRequiredNextFrame = true;

        //Logs that the squad button is currently collapsed (not expanded).
        isExpanded = false;

        //Plays the appropriate sound effect.
        if (playSFX)
            AudioManager.PlaySFX(collapseSFX);
    }
}
