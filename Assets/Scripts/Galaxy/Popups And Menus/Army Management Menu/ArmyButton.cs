using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyButton : UnitListButton
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

    private GalaxyArmy assignedArmy = null;
    /// <summary>
    /// Indicates the army that the army button is supposed to represent.
    /// </summary>
    private GalaxyArmy AssignedArmy
    {
        get
        {
            return assignedArmy;
        }
        set
        {
            //Does not continue any further with setting the value of the assigned army if the button already has an assigned army.
            if (assignedArmy != null)
                return;

            //Sets the reference to the assigned army.
            assignedArmy = value;
            //Sets the color of the army button to match the empire color of the empire that owns the assigned army.
            Button.image.color = Empire.empires[assignedArmy.OwnerEmpireID].EmpireColor;
        }
    }

    /// <summary>
    /// Provides the initial values of important army button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedArmy"></param>
    public void Initialize(ArmyManagementMenu armyManagementMenu, GalaxyArmy assignedArmy)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the army button which army it is supposed to represent.
        AssignedArmy = assignedArmy;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// This method is called through a button click and shows the army in the unit inspector and either expands or collapses the child buttons.
    /// </summary>
    public override void OnClick()
    {
        base.OnClick();

        if (!expanded)
        {
            if(assignedArmy.TotalNumberOfSquads > 0)
                Expand();
        }
        else
        {
            Collapse();
        }
    }

    /// <summary>
    /// This method should be called in order to expand the army button and reveal the appropriate squad buttons.
    /// </summary>
    private void Expand()
    {
        //Creates the squad buttons.
        for(int squadIndex = 0; squadIndex < AssignedArmy.TotalNumberOfSquads; squadIndex++)
        {
            //Instantiates a new squad button from the squad button prefab.
            GameObject squadButton = Instantiate(ArmyManagementMenu.SquadButtonPrefab);
            //Sets the parent of the squad button.
            squadButton.transform.SetParent(ArmyManagementMenu.UnitListButtonParent);
            //Sets the sibling index of the squad button.
            squadButton.transform.SetSiblingIndex(transform.GetSiblingIndex() + (squadIndex + 1));
            //Resets the scale of the squad button to 1 in order to avoid any unity shenanigans.
            squadButton.transform.localScale = Vector3.one;
            //Gets the squad button script component of the squad button in order to edit some values in the script.
            SquadButton squadButtonScript = squadButton.GetComponent<SquadButton>();
            //Assigns the appropriate squad to the squad button.
            squadButtonScript.Initialize(ArmyManagementMenu, AssignedArmy.GetSquadAt(squadIndex));
        }

        //Adds the appropriate amount of spacing between the army button and the squad buttons.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the army button is currently expanded.
        expanded = true;
    }

    /// <summary>
    /// This method should be called in order to collapse the army button and destroy the squad buttons.
    /// </summary>
    private void Collapse()
    {
        //Gathers a list of all of the unit list buttons to destroy.
        List<UnitListButton> unitListButtonsToDestroy = new List<UnitListButton>();
        for(int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            UnitListButton unitListButton = transform.parent.GetChild(siblingIndex).GetComponent<UnitListButton>();
            if(unitListButton.TypeOfButton != TypeOfButton)
            {
                unitListButtonsToDestroy.Add(unitListButton);
            }
            else
            {
                break;
            }
        }
        //Destroys all of the unit list buttons that are a result of the army button being expanded.
        foreach(UnitListButton unitListButtonToDestroy in unitListButtonsToDestroy)
        {
            Destroy(unitListButtonToDestroy.gameObject);
        }

        //Updates the amount of spacing between the army button and the buttons that follow.
        SpacingUpdateRequiredNextFrame = true;

        //Logs that the army button is currently collapsed (not expanded).
        expanded = false;
    }
}
