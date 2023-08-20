using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalaxyArmyButton : GalaxyArmyManagementMenuExpandableUnitListButton
{
    [Header("Army Button Components")]

    [SerializeField] private Image squadsCountIconImage = null;
    [SerializeField] private Text squadsCountText = null;
    [SerializeField] private GalaxyTooltip squadsCountTooltip = null; 

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Private holder variable for the army assigned to the army button to represent.
    /// </summary>
    private NewGalaxyArmy _assignedArmy = null;
    /// <summary>
    /// Public property that should be used in order to access the army that the army button is assigned to represent.
    /// </summary>
    public NewGalaxyArmy assignedArmy
    {
        get => _assignedArmy;
        private set
        {
            //Does not continue any further with setting the value of the assigned army if the button already has an assigned army.
            if (_assignedArmy != null)
                return;

            //Sets the reference to the assigned army.
            _assignedArmy = value;

            //Updates the button's displayed info.
            UpdateInfo();
        }
    }

    /// <summary>
    /// Provides the initial values of important army button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedArmy"></param>
    public void Initialize(GalaxyArmyManagementMenu armyManagementMenu, NewGalaxyArmy assignedArmy)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the army button which army it is supposed to represent.
        this.assignedArmy = assignedArmy;
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
        button.image.color = assignedArmy.empire.color;
        //Sets the sprite of the left image to the appropriate army icon.
        leftImage.sprite = assignedArmy.iconSprite;
        //Sets the color of the left image to the appropriate color of the army's icon.
        leftImage.color = Color.gray;
        //Sets the text of the squads count text to accurately represent how many squads are in the army and how many squads the army could possibly have.
        squadsCountText.text = "(" + assignedArmy.squads.Count + "/" + assignedArmy.maxSquadsCount + ")";

        //Updates the info on all of the child buttons (essential for updating the army icon sprite).
        for (int siblingIndex = transform.GetSiblingIndex() + 1; siblingIndex < transform.parent.childCount; siblingIndex++)
        {
            //Gets the unit list button at the current sibling index.
            GalaxyArmyManagementMenuUnitListButton buttonAtSiblingIndex = transform.parent.GetChild(siblingIndex).GetComponent<GalaxyArmyManagementMenuUnitListButton>();

            //Breaks out of the loop for updating child button info if the unit list button at the current sibling index is not a child button.
            if (buttonAtSiblingIndex.buttonType <= buttonType)
                break;

            //Updates the info on the button at the current sibling index because it is a child button.
            buttonAtSiblingIndex.UpdateInfo();
        }
    }

    public override void DisbandAssignedGroundUnit()
    {
        //Loops through each squad and pill within the army and unassigns it from its parent ground unit.
        for(int squadIndex = assignedArmy.squads.Count - 1; squadIndex >= 0; squadIndex--)
        {
            for(int pillIndex = assignedArmy.squads[squadIndex].pills.Count - 1; pillIndex >= 0; pillIndex--)
                assignedArmy.squads[squadIndex].pills[pillIndex].assignedSquad = null;
            assignedArmy.squads[squadIndex].assignedArmy = null;
        }
        //Checks if there is a planet attached to the army management menu and removes the army from the planet's list of stationed armies on the planet if so.
        if (armyManagementMenu.planetSelected != null)
            armyManagementMenu.planetSelected.stationedArmies.Remove(assignedArmy);
        //Removes the army from its empire's army manager.
        assignedArmy.empire.armiesManager.armies.Remove(assignedArmy);

        //Executes the base logic for disbanding a ground unit.
        base.DisbandAssignedGroundUnit();
    }
}
