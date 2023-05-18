using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenuBuildingsTabScollViewButton : MonoBehaviour
{
    [Header("Image Components")]

    [SerializeField, Tooltip("The leftmost image on the button that displays an icon that indicates the building type to the player.")] private Image buildingTypeIconImage = null;

    [Header("Text Components")]

    [SerializeField, Tooltip("The middle text on the button that displays the assigned building's building type name.")] private Text buildingTypeText = null;
    [SerializeField, Tooltip("The rightmost text on the button that displays the assigned building's level number.")] private Text buildingLevelText = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that holds a reference to the planet management menu that this button belongs.
    /// </summary>
    private GalaxyPlanetManagementMenu planetManagementMenu = null;

    /// <summary>
    /// Private static property that should be accessed in order to obtain the game object that serves as the prefab that all buttons are instantiated from.
    /// </summary>
    private static GameObject prefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Planet Management Menu/Buildings Tab Scroll View Button"); }

    /// <summary>
    /// Public static method that should be called in order to instantiate a new button for a specified planet management menu.
    /// </summary>
    /// <param name="planetManagementMenu"></param>
    /// <param name="building"></param>
    public static void InstantiateNewButton(GalaxyPlanetManagementMenu planetManagementMenu, NewGalaxyBuilding building)
    {
        Instantiate(prefab).GetComponent<PlanetManagementMenuBuildingsTabScollViewButton>().Initialize(planetManagementMenu, building);

    }

    /// <summary>
    /// Private method that should be called by the public static method InstantiateNewButton to initialize each button after it is instantiated from the prefab.
    /// </summary>
    /// <param name="planetManagementMenu"></param>
    /// <param name="building"></param>
    private void Initialize(GalaxyPlanetManagementMenu planetManagementMenu, NewGalaxyBuilding building)
    {
        //Checks if either the given planet management menu or building are null and returns if so since they are essential for making a proper button.
        if (planetManagementMenu == null || building == null)
            return;

        //Assigns the local reference to the planet management menu that the button belongs to.
        this.planetManagementMenu = planetManagementMenu;

        //Initializes the component values of the button to represent the specified building.
        buildingTypeIconImage.sprite = building.buildingTypeIconSprite;
        buildingTypeText.text = GeneralHelperMethods.GetEnumText(building.buildingType.ToString());
        buildingLevelText.text = GeneralHelperMethods.ConvertArabicIntToRomanNumeralString(building.level);

        //Sets the parent of the button to place the button in the planet management menu's buildings tab scroll view.
        transform.SetParent(planetManagementMenu.buildingsTabScrollViewContent);
        //Resets the scale of the button to avoid any shenanigans.
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the button is clicked and informs the button's assigned planet management menu that the button was pressed.
    /// </summary>
    public void OnClickButton()
    {
        planetManagementMenu.OnClickBuildingsTabScrollViewButton(transform.GetSiblingIndex());
    }
}
