using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxySquadButton : GalaxyArmyManagementMenuExpandableUnitListButton
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Private holder variable for the assigned squad that the squad button is meant to represent.
    /// </summary>
    private NewGalaxySquad _assignedSquad = null;
    /// <summary>
    /// Public property that should be used in order to access the squad that the squad button is meant to represent.
    /// </summary>
    public NewGalaxySquad assignedSquad
    {
        get => _assignedSquad;
        private set
        {
            //Does not continue any further with setting the value of the assigned squad if the button already has an assigned squad.
            if (_assignedSquad != null)
                return;

            //Sets the reference to the assigned squad.
            _assignedSquad = value;

            //Updates the button's displayed info.
            UpdateInfo();
        }
    }

    /// <summary>
    /// Provides the initial values of important squad button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedSquad"></param>
    public void Initialize(GalaxyArmyManagementMenu armyManagementMenu, NewGalaxySquad assignedSquad)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the squad button which squad it is supposed to represent.
        this.assignedSquad = assignedSquad;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        spacingUpdateRequiredNextFrame = true;
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

    public override void UpdateInfo()
    {
        base.UpdateInfo();

        //Sets the color of the squad button to match the empire color of the empire that owns the assigned squad.
        button.image.color = assignedSquad.empire.color;
        //Sets the sprite of the left image to show the icon of the army that the squad is assigned to.
        leftImage.sprite = assignedSquad.assignedArmy.iconSprite;
        //Sets the color of the left image to the appropriate color of the army's icon.
        leftImage.color = assignedSquad.color;
        //Sets the text of the name text component to be the name of the squad.
        nameText.text = assignedSquad.name;

        //Updates the info on all child pill buttons (mostly because of the squad leader).
        if(childButtons != null)
            foreach(GalaxyArmyManagementMenuUnitListButton childButton in childButtons)
                if(childButton.buttonType == ButtonType.Pill)
                    childButton.gameObject.GetComponent<GalaxyPillButton>().UpdateInfo();
    }

    public override void DisbandAssignedGroundUnit()
    {
        //Loops through each pill in the squad and removes its assigned squad as the disbanding squad.
        for (int pillIndex = assignedSquad.pills.Count - 1; pillIndex >= 0; pillIndex--)
            assignedSquad.pills[pillIndex].assignedSquad = null;
        //Removes the squad from its assigned army.
        assignedSquad.assignedArmy.squads.Remove(assignedSquad);

        //Executes the base logic for disbanding a ground unit.
        base.DisbandAssignedGroundUnit();
    }
}
