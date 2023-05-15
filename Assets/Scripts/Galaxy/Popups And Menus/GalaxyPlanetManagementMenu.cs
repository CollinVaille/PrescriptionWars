using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPlanetManagementMenu : NewGalaxyPopupBehaviour
{
    [Header("Transform Components")]

    [SerializeField] private Transform tabButtonsParent = null;
    [SerializeField] private Transform tabsParent = null;

    [Header("Text Components")]

    [SerializeField] private Text planetNameText = null;
    [SerializeField] private Text cityNameText = null;

    [Header("Image Components")]

    [SerializeField] private Image coloringBackgroundImage = null;

    [Header("Unselected Tab Button Color Block")]

    [SerializeField] private ColorBlock unselectedTabButtonColorBlock;

    [Header("Selected Tab Button Color Block")]

    [SerializeField] private ColorBlock selectedTabButtonColorBlock;

    [Header("SFX Options")]

    [SerializeField] private AudioClip changeTabSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that holds the index of the selected tab on the planet management menu.
    /// </summary>
    private int _tabSelectedIndex = -1;
    /// <summary>
    /// Private property that should be used both to access and mutate which tab is selected on the planet management menu.
    /// </summary>
    private int tabSelectedIndex
    {
        get => _tabSelectedIndex;
        set
        {
            //Returns and does nothing if the specified tab is already selected.
            if (_tabSelectedIndex == value)
                return;
            //Deselects the previously selected tab if neccessary.
            if (_tabSelectedIndex >= 0 && _tabSelectedIndex < tabsParent.childCount)
            {
                //Deselects the previously selected tab button.
                tabButtonsParent.GetChild(_tabSelectedIndex).GetComponent<Button>().colors = unselectedTabButtonColorBlock;
                //Deactivates the game object of the previously selected tab.
                tabsParent.GetChild(_tabSelectedIndex).gameObject.SetActive(false);
            }
            //Sets the specified tab index as the tab that is currently selected.
            _tabSelectedIndex = value;
            //Visibly selects the now selected tab if neccessary.
            if (_tabSelectedIndex >= 0 && _tabSelectedIndex < tabsParent.childCount)
            {
                //Shows the correct tab button as selected.
                tabButtonsParent.GetChild(_tabSelectedIndex).GetComponent<Button>().colors = selectedTabButtonColorBlock;
                //Activates the game object of the tab that is now selected.
                tabsParent.GetChild(_tabSelectedIndex).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Private variable that holds the ID of the planet that is selected and having its information displayed on this planet management menu. -1 indicates that no planet is selected at the moment.
    /// </summary>
    private int _planetSelectedID = -1;
    /// <summary>
    /// Public property that should be used both to access and mutate which planet is selected to have its information displayed on the planet management menu via ID.
    /// </summary>
    public int planetSelectedID
    {
        get => _planetSelectedID;
        set
        {
            //Sets the planet selected ID variable to the specified value.
            _planetSelectedID = value;
            //Sets the planet name text.
            planetNameText.text = planetSelected != null ? planetSelected.planetName : "Planet Name";
            //Sets the city name text.
            cityNameText.text = planetSelected != null ? planetSelected.city.name : "City: City Name";
            //Sets the color of the coloring background image.
            coloringBackgroundImage.color = planetSelected != null && planetSelected.owner != null ? planetSelected.owner.color : Color.white;
            //Loops through each tab button and sets its base image color to the planet's owning empire color or black if there is no valid owning empire.
            for(int tabButtonIndex = 0; tabButtonIndex < tabButtonsParent.childCount; tabButtonIndex++)
                tabButtonsParent.GetChild(tabButtonIndex).GetComponent<Image>().color = planetSelected != null && planetSelected.owner != null ? planetSelected.owner.color : Color.black;
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate which planet is selected to have its information displayed on the planet management menu. Returns null if no planet is selected. Works on top of the planetSelectedID int property.
    /// </summary>
    public NewGalaxyPlanet planetSelected
    {
        get => NewGalaxyManager.planets != null && planetSelectedID >= 0 && planetSelectedID < NewGalaxyManager.planets.Count ? NewGalaxyManager.planets[planetSelectedID] : null;
        set
        {
            //Sets the selected planet ID to -1 if the specified value is null or the ID of the specified planet if the specified planet reference is not null.
            planetSelectedID = value == null ? -1 : value.ID;
        }
    }

    //Test method that should be removed eventually.
    private void TestSetPlanet()
    {
        planetSelectedID = 0;
    }

    protected override void Start()
    {
        //Executes the base popup start logic.
        base.Start();

        //Sets the color block of all tab buttons to the unselected tab button color block that has its values specified through the inspector.
        for (int tabButtonIndex = 0; tabButtonIndex < tabButtonsParent.childCount; tabButtonIndex++)
            tabButtonsParent.GetChild(tabButtonIndex).GetComponent<Button>().colors = unselectedTabButtonColorBlock;
        //Sets the first tab as the selected tab.
        tabSelectedIndex = 0;

        //Test line that should be removed eventually.
        NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(TestSetPlanet, 3);
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever a tab button is clicked on by the player. The previously selected tab is unselected and the newly specified tab is then selected.
    /// </summary>
    /// <param name="tabButton"></param>
    public void OnClickTabButton(Transform tabButton)
    {
        //Gets the index (sibling index) of the tab button.
        int tabButtonIndex = tabButton.GetSiblingIndex();
        //Checks if the tab button is already selected and returns if so.
        if (tabButtonIndex == tabSelectedIndex)
            return;
        //Sets the button's assigned tab as the planet management menu's selected tab.
        tabSelectedIndex = tabButtonIndex;
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(changeTabSFX);
    }
}
