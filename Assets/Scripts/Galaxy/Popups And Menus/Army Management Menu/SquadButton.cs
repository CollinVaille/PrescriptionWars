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

        UpdateSpacing();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
