using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillButton : UnitListButton
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    private GalaxyPill assignedPill = null;
    /// <summary>
    /// Indicates the pill that the pill button is supposed to represent.
    /// </summary>
    private GalaxyPill AssignedPill
    {
        get
        {
            return assignedPill;
        }
        set
        {
            //Does not continue any further with setting the value of the assigned pill if the button already has an assigned pill.
            if (assignedPill != null)
                return;

            //Sets the reference to the assigned pill.
            assignedPill = value;
            //Sets the color of the pill button to match the empire color of the empire that owns the assigned pill.
            Button.image.color = Empire.empires[assignedPill.AssignedSquad.AssignedArmy.OwnerEmpireID].EmpireColor;
            //Sets the text of the name text component to be the name of the pill.
            NameText.text = AssignedPill.Name;

            //Executes the logic in the base class that is necessary when the ground unit's value is set.
            OnGalaxyGroundUnitValueSet(assignedPill);
        }
    }

    /// <summary>
    /// Provides the initial values of important pill button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedPill"></param>
    public void Initialize(ArmyManagementMenu armyManagementMenu, GalaxyPill assignedPill)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the pill button which pill it is supposed to represent.
        AssignedPill = assignedPill;
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
}
