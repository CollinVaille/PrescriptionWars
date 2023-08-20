using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPillButton : GalaxyArmyManagementMenuUnitListButton
{
    [Header("Components")]

    [SerializeField] private Image _rightImage = null;
    [SerializeField] private GalaxyTooltip _leftImageTooltip = null;
    [SerializeField] private GalaxyTooltip _rightImageTooltip = null;

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Protected property that should be used in order to access the right image component of the pill button.
    /// </summary>
    protected Image rightImage { get => _rightImage; }

    /// <summary>
    /// Protected property that should be used in order to access the left image tooltip component of the pill button.
    /// </summary>
    protected GalaxyTooltip leftImageTooltip { get => _leftImageTooltip; }

    /// <summary>
    /// Protected property that should be used in order to access the right image tooltip component of the pill button.
    /// </summary>
    protected GalaxyTooltip rightImageTooltip { get => _rightImageTooltip; }

    /// <summary>
    /// Private holder variable for the pill that the pill button is meant to be representing.
    /// </summary>
    private NewGalaxyPill _assignedPill = null;
    /// <summary>
    /// Indicates the pill that the pill button is supposed to represent.
    /// </summary>
    public NewGalaxyPill assignedPill
    {
        get => _assignedPill;
        private set
        {
            //Does not continue any further with setting the value of the assigned pill if the button already has an assigned pill.
            if (_assignedPill != null)
                return;

            //Sets the reference to the assigned pill.
            _assignedPill = value;

            //Updates the button's displayed info.
            UpdateInfo();
        }
    }

    /// <summary>
    /// Provides the initial values of important pill button variables.
    /// </summary>
    /// <param name="armyManagementMenu"></param>
    /// <param name="assignedPill"></param>
    public void Initialize(GalaxyArmyManagementMenu armyManagementMenu, NewGalaxyPill assignedPill)
    {
        //Initializes the important variables of the unit list button base class.
        Initialize(armyManagementMenu);

        //Assigns the pill button which pill it is supposed to represent.
        this.assignedPill = assignedPill;
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

    public override void UpdateInfo()
    {
        base.UpdateInfo();

        //Sets the color of the pill button to match the empire color of the empire that owns the assigned pill.
        button.image.color = assignedPill.assignedSquad.assignedArmy.empire.color;
        //Sets the left image's sprite to the pill's class type sprite.
        if(assignedPill != null && assignedPill.pillClass != null && assignedPill.pillClass.iconSprite != null)
        {
            leftImage.sprite = assignedPill.pillClass.iconSprite;
            leftImage.gameObject.GetComponent<GalaxyTooltip>().Text = "Class Name: " + assignedPill.pillClass.name + "\nClass Type: " + assignedPill.pillClass.classType;
            leftImage.gameObject.SetActive(true);
        }
        else
        {
            leftImage.sprite = null;
            leftImage.gameObject.SetActive(false);
        }
        //Toggles the right image depending on whether or not the assigned pill is a squad leader.
        rightImage.gameObject.SetActive(assignedPill != null && assignedPill.isSquadLeader);
    }

    public override void DisbandAssignedGroundUnit()
    {
        //Removes the pill from its assigned squad.
        assignedPill.assignedSquad.pills.Remove(assignedPill);

        //Executes the base logic for disbanding a ground unit.
        base.DisbandAssignedGroundUnit();
    }

    public override void OnTooltipOpen(GalaxyTooltip tooltip)
    {
        //Executes the base class's logic for when the tooltip opens.
        base.OnTooltipOpen(tooltip);

        //Sets the position of the left image tooltip if it is the tooltip that opened.
        if (tooltip == leftImageTooltip)
            leftImageTooltip.Position = new Vector2(leftImage.transform.position.x - 35, leftImage.transform.position.y + 77);
        //Sets the position of the right image tooltip if it is the tooltip that opened.
        else if (tooltip == rightImageTooltip)
            rightImageTooltip.Position = new Vector2(rightImage.transform.position.x - 35, rightImage.transform.position.y + 59);
    }

    public override void OnTooltipClose(GalaxyTooltip tooltip)
    {
        //Executes the base class's logic for when the tooltip closes.
        base.OnTooltipClose(tooltip);
    }
}
