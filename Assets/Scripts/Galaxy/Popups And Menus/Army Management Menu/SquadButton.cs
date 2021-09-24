using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadButton : ExpandableUnitListButton
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    private GalaxySquad assignedSquad = null;
    /// <summary>
    /// Indicates the squad that the squad button is supposed to represent.
    /// </summary>
    public GalaxySquad AssignedSquad
    {
        get
        {
            return assignedSquad;
        }
        private set
        {
            //Does not continue any further with setting the value of the assigned squad if the button already has an assigned squad.
            if (assignedSquad != null)
                return;

            //Sets the reference to the assigned squad.
            assignedSquad = value;
            //Sets the color of the squad button to match the empire color of the empire that owns the assigned squad.
            Button.image.color = Empire.empires[assignedSquad.AssignedArmy.OwnerEmpireID].EmpireColor;
            //Sets the sprite of the left image to show the icon of the army that the squad is assigned to.
            LeftImage.sprite = Resources.Load<Sprite>("Army Icons/" + AssignedSquad.AssignedArmy.ArmyIcon.spriteName);
            //Sets the color of the left image to the appropriate color of the army's icon.
            LeftImage.color = AssignedSquad.IconColor;
            //Sets the text of the name text component to be the name of the squad.
            NameText.text = assignedSquad.Name;

            //Executes the logic in the base class that is necessary when the ground unit's value is set.
            OnGalaxyGroundUnitValueSet(assignedSquad);
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
    }

    protected override void OnButtonMoveUpdate()
    {
        base.OnButtonMoveUpdate();

        //Sets the sprite of the left image to show the icon of the army that the squad is assigned to.
        LeftImage.sprite = Resources.Load<Sprite>("Army Icons/" + AssignedSquad.AssignedArmy.ArmyIcon.spriteName);
    }
}
