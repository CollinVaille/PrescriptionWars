using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArmyButton : ExpandableUnitListButton
{
    [Header("Army Button Components")]

    [SerializeField] private Image squadsCountIconImage = null;
    [SerializeField] private Text squadsCountText = null;
    [SerializeField] private GalaxyTooltip squadsCountTooltip = null; 

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

            //Updates the button's displayed info.
            UpdateInfo();
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

    public override void OnTooltipOpen(GalaxyTooltip tooltip)
    {
        base.OnTooltipOpen(tooltip);

        //Sets the position of the squads count tooltip if it is open.
        if(tooltip == squadsCountTooltip)
            squadsCountTooltip.Position = new Vector2(squadsCountIconImage.transform.position.x - 35, squadsCountIconImage.transform.position.y + 59);
    }

    public override void OnDrag(PointerEventData pointerEventData)
    {
        base.OnDrag(pointerEventData);

        //Updates the position of the squads count tooltip if it is open.
        if (squadsCountTooltip.Open)
            squadsCountTooltip.Position = new Vector2(squadsCountIconImage.transform.position.x - 35, squadsCountIconImage.transform.position.y + 59);
    }

    public override void UpdateInfo()
    {
        base.UpdateInfo();

        //Sets the color of the army button to match the empire color of the empire that owns the assigned army.
        Button.image.color = Empire.empires[assignedArmy.OwnerEmpireID].EmpireColor;
        //Sets the sprite of the left image to the appropriate army icon.
        LeftImage.sprite = Resources.Load<Sprite>("Galaxy/Army Icons/" + assignedArmy.ArmyIcon.spriteName);
        //Sets the color of the left image to the appropriate color of the army's icon.
        LeftImage.color = assignedArmy.ArmyIcon.color;
        //Sets the text of the squads count text to accurately represent how many squads are in the army and how many squads the army could possibly have.
        squadsCountText.text = "(" + assignedArmy.SquadsCount + "/" + assignedArmy.NumberOfSquadsLimits + ")";
    }

    public override void DisbandAssignedGroundUnit()
    {
        //Removes the army from the list of armies on the selected planet.
        ArmyManagementMenu.PlanetSelected.RemoveArmy(AssignedArmy);

        //Executes the base logic for disbanding a ground unit.
        base.DisbandAssignedGroundUnit();
    }
}
