using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyButton : ExpandableUnitListButton
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    private GalaxyArmy assignedArmy = null;
    /// <summary>
    /// Indicates the army that the army button is supposed to represent.
    /// </summary>
    public GalaxyArmy AssignedArmy
    {
        get
        {
            return assignedArmy;
        }
        private set
        {
            //Does not continue any further with setting the value of the assigned army if the button already has an assigned army.
            if (assignedArmy != null)
                return;

            //Sets the reference to the assigned army.
            assignedArmy = value;
            //Sets the color of the army button to match the empire color of the empire that owns the assigned army.
            Button.image.color = Empire.empires[assignedArmy.OwnerEmpireID].EmpireColor;
            //Sets the sprite of the left image to the appropriate army icon.
            LeftImage.sprite = Resources.Load<Sprite>("Army Icons/" + assignedArmy.ArmyIcon.spriteName);
            //Sets the color of the left image to the appropriate color of the army's icon.
            LeftImage.color = assignedArmy.ArmyIcon.color;
            //Sets the text of the name text component to be the name of the army.
            NameText.text = AssignedArmy.Name;

            //Executes the logic in the base class that is necessary when the ground unit's value is set.
            OnGalaxyGroundUnitValueSet(assignedArmy);
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
    }
}
