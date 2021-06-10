using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : GalaxyPopupBehaviour
{
    [Header("Text Components")]

    [SerializeField] private Text titleText = null;

    [Header("Other Components")]

    [SerializeField] private VerticalLayoutGroup unitListVerticalLayoutGroup = null;
    public VerticalLayoutGroup UnitListVerticalLayoutGroup
    {
        get
        {
            return unitListVerticalLayoutGroup;
        }
    }

    [Header("Parents")]

    [SerializeField] private Transform unitListButtonParent = null;
    public Transform UnitListButtonParent
    {
        get
        {
            return unitListButtonParent;
        }
    }

    [Header("Prefabs")]

    [SerializeField] private GameObject armyButtonPrefab = null;
    [SerializeField] private GameObject squadButtonPrefab = null;
    public GameObject SquadButtonPrefab
    {
        get
        {
            return squadButtonPrefab;
        }
    }

    [Header("Options")]

    [SerializeField, Tooltip("Specifies the amount of spacing between different unit list button types in the unit list (Ex: spacing between an army button and a squad button).") ] private float spacingBetweenUnitListButtonTypes = 0;
    public float SpacingBetweenUnitListButtonTypes
    {
        get
        {
            return spacingBetweenUnitListButtonTypes;
        }
    }

    //Non-inspector variables.

    private int planetSelectedID = -1;
    /// <summary>
    /// Indicates what planet the player is managing the armies of.
    /// Updates the title text of the menu to accurately reflect this.
    /// </summary>
    public int PlanetSelectedID
    {
        get
        {
            return planetSelectedID;
        }
        set
        {
            //Sets the selected planet to the specified planet.
            planetSelectedID = value;

            //Updates the title text of the army management menu to accurately reflect what planet the player is managing the armies of.
            titleText.text = PlanetSelected.Name + " Army Management";
        }
    }

    /// <summary>
    /// Returns the planet that the player is managing the armies on based on the planetSelectedID.
    /// </summary>
    public GalaxyPlanet PlanetSelected
    {
        get
        {
            return GalaxyManager.planets[planetSelectedID];
        }
    }

    /// <summary>
    /// The prefab that the army management menu is instantiated from.
    /// </summary>
    public static GameObject armyManagementMenuPrefab = null;

    /// <summary>
    /// Instantiates a new army management menu from the army management menu prefab and adjusts its values based on the given specifications.
    /// </summary>
    public static void CreateNewArmyManagementMenu(int planetSelectedID)
    {
        //Instantiates a new army management menu from the army management menu prefab.
        GameObject armyManagementMenu = Instantiate(armyManagementMenuPrefab);
        //Sets the parent of the army management menu.
        armyManagementMenu.transform.SetParent(GalaxyManager.PopupsParent);
        //Gets the script component of the army management menu in order to edit values.
        ArmyManagementMenu armyManagementMenuScript = armyManagementMenu.GetComponent<ArmyManagementMenu>();
        //Sets the planet selected on the army management menu.
        armyManagementMenuScript.PlanetSelectedID = planetSelectedID;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //Populates the unit list with the appropriate army buttons.
        if(planetSelectedID >= 0 && planetSelectedID < GalaxyManager.planets.Count)
            PopulateUnitList();
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

    /// <summary>
    /// Populates the unit list (scroll view / scroll list) with the appropriate army buttons.
    /// </summary>
    private void PopulateUnitList()
    {
        for(int armyIndex = 0; armyIndex < PlanetSelected.GetArmiesCount(); armyIndex++)
        {
            //Instantiates the army button from the army button prefab.
            GameObject armyButton = Instantiate(armyButtonPrefab);
            //Sets the parent of the army button.
            armyButton.transform.SetParent(unitListButtonParent);
            //Resets the scale of the army button.
            armyButton.transform.localScale = Vector3.one;
            //Gets the army button script component of the army button in order to edit some values in the script.
            ArmyButton armyButtonScript = armyButton.GetComponent<ArmyButton>();
            //Assigns the appropriate army to the army button.
            armyButtonScript.Initialize(this, PlanetSelected.GetArmyAt(armyIndex));
        }
    }
}