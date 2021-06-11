using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadButton : UnitListButton
{
    [Header("Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private bool expanded = false;
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

    private GalaxySquad assignedSquad = null;
    /// <summary>
    /// Indicates the squad that the squad button is supposed to represent.
    /// </summary>
    private GalaxySquad AssignedSquad
    {
        get
        {
            return assignedSquad;
        }
        set
        {
            //Does not continue any further with setting the value of the assigned squad if the button already has an assigned squad.
            if (assignedSquad != null)
                return;

            //Sets the reference to the assigned squad.
            assignedSquad = value;
            //Sets the color of the squad button to match the empire color of the empire that owns the assigned squad.
            Button.image.color = Empire.empires[assignedSquad.AssignedArmy.OwnerEmpireID].EmpireColor;
        }
    }

    /// <summary>
    /// Provides the initial values of important squad button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedSquad"></param>
    public void Initialize(ArmyManagementMenu armyManagementMenu, GalaxySquad assignedSquad)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the squad button which squad it is supposed to represent.
        AssignedSquad = assignedSquad;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        SpacingUpdateRequiredNextFrame = true;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// This method is called through a button click and shows the squad in the unit inspector and either expands or collapses the child buttons.
    /// </summary>
    public override void OnClick()
    {
        base.OnClick();

        if (!expanded)
        {
            if (assignedSquad.TotalNumberOfPills > 0)
                Expand();
        }
        else
        {
            Collapse();
        }
    }

    /// <summary>
    /// This method should be called in order to expand the squad button and reveal the appropriate pill buttons.
    /// </summary>
    private void Expand()
    {
        //Creates the pill buttons.
        for (int pillIndex = 0; pillIndex < AssignedSquad.TotalNumberOfPills; pillIndex++)
        {
            //Instantiates a new pill button from the pill button prefab.
            GameObject pillButton = Instantiate(ArmyManagementMenu.PillButtonPrefab);
            //Sets the parent of the pill button.
            pillButton.transform.SetParent(ArmyManagementMenu.UnitListButtonParent);
            //Sets the sibling index of the pill button.
            pillButton.transform.SetSiblingIndex(transform.GetSiblingIndex() + (pillIndex + 1));
            //Resets the scale of the pill button to 1 in order to avoid any unity shenanigans.
            pillButton.transform.localScale = Vector3.one;
            //Gets the pill button script component of the pill button in order to edit some values in the script.
            PillButton squadButtonScript = pillButton.GetComponent<PillButton>();
            //Assigns the appropriate pill to the pill button.
            squadButtonScript.Initialize(ArmyManagementMenu, AssignedSquad.GetPillAt(pillIndex));
        }

        //Adds the appropriate amount of spacing between the army button and the squad buttons.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the army button is currently expanded.
        expanded = true;
    }

    /// <summary>
    /// This method should be called in order to collapse the squad button and destroy the pill buttons.
    /// </summary>
    private void Collapse()
    {
        //Gathers a list of all of the unit list buttons to destroy.
        List<UnitListButton> unitListButtonsToDestroy = new List<UnitListButton>();
        for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            UnitListButton unitListButton = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if (unitListButton.TypeOfButton == ButtonType.Pill)
            {
                unitListButtonsToDestroy.Add(unitListButton);
            }
            else
            {
                break;
            }
        }
        //Destroys all of the unit list buttons that are a result of the squad button being expanded.
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
