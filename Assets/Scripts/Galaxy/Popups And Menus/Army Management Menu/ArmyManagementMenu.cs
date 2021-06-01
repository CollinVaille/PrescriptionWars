using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : GalaxyPopupBehaviour
{
    [Header("Text Components")]

    [SerializeField] private Text titleText = null;

    //Non-inspector variables.

    private GalaxyPlanet planetAssigned = null;
    /// <summary>
    /// Indicates what planet the player is managing the armies of.
    /// Updates the title text of the menu to accurately reflect this.
    /// </summary>
    public GalaxyPlanet PlanetAssigned
    {
        get
        {
            return planetAssigned;
        }
        set
        {
            //Sets the assigned planet of the army management menu to the specified planet.
            planetAssigned = value;

            //Updates the title text of the army management menu to accurately reflect what planet the player is managing the armies of.
            titleText.text = planetAssigned.Name + " Army Management";
        }
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
    /// This method should be called in order to expand all scroll list buttons.
    /// </summary>
    public void ExpandAll()
    {

    }

    /// <summary>
    /// This method should be called in order to collapse all scroll list buttons.
    /// </summary>
    public void CollapseAll()
    {

    }
}
