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
    public GalaxyPill AssignedPill
    {
        get
        {
            return assignedPill;
        }
        private set
        {
            //Does not continue any further with setting the value of the assigned pill if the button already has an assigned pill.
            if (assignedPill != null)
                return;

            //Sets the reference to the assigned pill.
            assignedPill = value;

            //Updates the button's displayed info.
            UpdateInfo();
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

    public override void UpdateInfo()
    {
        base.UpdateInfo();

        //Sets the color of the pill button to match the empire color of the empire that owns the assigned pill.
        Button.image.color = Empire.empires[assignedPill.AssignedSquad.AssignedArmy.OwnerEmpireID].EmpireColor;
        //Sets the left image's sprite to the pill's class type sprite.
        if(AssignedPill != null && AssignedPill.PillClass != null && AssignedPill.PillClass.iconSprite != null)
        {
            LeftImage.sprite = AssignedPill.PillClass.iconSprite;
            LeftImage.gameObject.SetActive(true);
        }
        else
        {
            LeftImage.sprite = null;
            LeftImage.gameObject.SetActive(false);
        }
    }

    public override void DisbandAssignedGroundUnit()
    {
        //Removes the pill from its assigned squad.
        AssignedPill.AssignedSquad.RemovePill(AssignedPill);

        //Executes the base logic for disbanding a ground unit.
        base.DisbandAssignedGroundUnit();
    }
}
