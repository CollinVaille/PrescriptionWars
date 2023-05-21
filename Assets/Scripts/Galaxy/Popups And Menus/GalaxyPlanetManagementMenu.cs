using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPlanetManagementMenu : NewGalaxyPopupBehaviour
{
    [Header("Transform Components")]

    [SerializeField] private Transform tabButtonsParent = null;
    [SerializeField] private Transform tabsParent = null;
    [SerializeField] private Transform _buildingsTabScrollViewContent = null;

    [Header("Game Object Components")]

    [SerializeField] private GameObject buildingsTabBuildingInspector = null;

    [Header("Text Components")]

    [SerializeField] private Text planetNameText = null;
    [SerializeField] private Text cityNameText = null;
    [SerializeField, Tooltip("The text component in the building inspector on the buildings tab of the menu that displays the name of the type of building that is selected in the building inspector to the player.")] private Text buildingsTabBuildingInspectorBuildingTypeText = null;
    [SerializeField, Tooltip("The text component in the building inspector on the buildings tab of the menu that displays the level of the building that is selected in the building inspector to the player.")] private Text buildingsTabBuildingInspectorBuildingLevelText = null;
    [SerializeField, Tooltip("The text component in the building inspector on the buildings tab of the menu that displays the description of the type of building that is selected in the building inspector to the player.")] private Text buildingsTabBuildingInspectorBuildingTypeDescriptionText = null;

    [Header("Image Components")]

    [SerializeField] private Image coloringBackgroundImage = null;
    [SerializeField] private Image buildingsTabScrollViewHandleImage = null;
    [SerializeField] private Image buildingsTabScrollViewViewportImage = null;
    [SerializeField, Tooltip("The image component in the buildings inspector on the buildings tab of the menu that displays the icon of the type of building that is selected in the building inspector to the player.")] private Image buildingsTabBuildingInspectorBuildingTypeIconImage = null;
    [SerializeField] private Image buildingsTabBuildingInspectorUpgradeBuildingButtonImage = null;

    [Header("Unselected Tab Button Color Block")]

    [SerializeField] private ColorBlock unselectedTabButtonColorBlock;

    [Header("Selected Tab Button Color Block")]

    [SerializeField] private ColorBlock selectedTabButtonColorBlock;

    [Header("SFX Options")]

    [SerializeField] private AudioClip changeTabSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Publicly accessible property that returns the transform that serves as the parent of the buttons in the buildings tab scroll view.
    /// </summary>
    public Transform buildingsTabScrollViewContent { get => _buildingsTabScrollViewContent; }

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
            //Sets the planet selected ID variable to the specified value or -1 if the specified value is invalid.
            _planetSelectedID = value >= 0 && value < NewGalaxyManager.planets.Count ? value : -1;

            if(planetSelected != null)
            {
                //Sets the planet name text.
                planetNameText.text = planetSelected.planetName;
                //Sets the city name text.
                cityNameText.text = "City: " + planetSelected.city.name;
                //Owner related setting.
                if(planetSelected.owner != null)
                {
                    //Sets the color of the coloring background image.
                    coloringBackgroundImage.color = planetSelected.owner.color;
                    //Loops through each tab button and sets its base image color to the planet's owning empire color.
                    for (int tabButtonIndex = 0; tabButtonIndex < tabButtonsParent.childCount; tabButtonIndex++)
                        tabButtonsParent.GetChild(tabButtonIndex).GetComponent<Image>().color = planetSelected.owner.color;
                    //Sets the color of the scroll view's vertical handle on the buildings tab.
                    buildingsTabScrollViewHandleImage.color = planetSelected.owner.color;
                    //Sets the color of the scroll view's viewport on the buildings tab to the planet's owning empire color.
                    buildingsTabScrollViewViewportImage.color = new Color(planetSelected.owner.color.r, planetSelected.owner.color.g, planetSelected.owner.color.b, planetSelected.owner.color.a * (50 / 255f));
                    //Sets the color of the upgrade button on the buildings tab building inspector to the owning empire's color.
                    buildingsTabBuildingInspectorUpgradeBuildingButtonImage.color = planetSelected.owner.labelColor;
                }
                else
                {
                    //Sets the color of the coloring background image to white since there is no valid owning empire.
                    coloringBackgroundImage.color = Color.white;
                    //Loops through each tab button and sets its base image color to black since there is no valid owning empire.
                    for (int tabButtonIndex = 0; tabButtonIndex < tabButtonsParent.childCount; tabButtonIndex++)
                        tabButtonsParent.GetChild(tabButtonIndex).GetComponent<Image>().color = Color.black;
                    //Sets the color of the scroll view's vertical handle on the buildings tab.
                    buildingsTabScrollViewHandleImage.color = Color.white;
                    //Sets the color of the scroll view's viewport on the buildings tab to a transparent white since there is no valid owning empire of the planet.
                    buildingsTabScrollViewViewportImage.color = new Color(Color.white.r, Color.white.g, Color.white.b, Color.white.a * (100 / 255f));
                    //Sets the color of the upgrade button on the buildings tab building inspector to white since there is no valid owning empire.
                    buildingsTabBuildingInspectorUpgradeBuildingButtonImage.color = Color.white;
                }
            }
            else
            {
                //Sets the planet name text.
                planetNameText.text = "Planet Name";
                //Sets the city name text.
                cityNameText.text = "City: City Name";
                //Sets the color of the coloring background image.
                coloringBackgroundImage.color = Color.white;
                //Loops through each tab button and sets its base image color to black since there is no valid owning empire.
                for (int tabButtonIndex = 0; tabButtonIndex < tabButtonsParent.childCount; tabButtonIndex++)
                    tabButtonsParent.GetChild(tabButtonIndex).GetComponent<Image>().color = Color.black;
                //Sets the color of the scroll view's vertical handle on the buildings tab.
                buildingsTabScrollViewHandleImage.color = Color.white;
                //Sets the color of the scroll view's viewport on the buildings tab to a transparent white since there is no valid owning empire of the planet.
                buildingsTabScrollViewViewportImage.color = new Color(Color.white.r, Color.white.g, Color.white.b, Color.white.a * (100 / 255f));
                //Sets the color of the upgrade button on the buildings tab building inspector to white since there is no valid owning empire.
                buildingsTabBuildingInspectorUpgradeBuildingButtonImage.color = Color.white;
            }
            //Populates the buildings tab scroll view with buttons and each button represents a building in the city on the menu's assigned planet.
            PopulateBuildingsTabScrollViewButtons();
            //Ensures that no building is selected to be displayed in the buildings tab building inspector.
            buildingSelected = null;
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

    /// <summary>
    /// Private variable that holds the index of the building that is selected in the buildings tab building inspector and having its information displayed in said building inspector. -1 indicates that no building is selected at the moment.
    /// </summary>
    private int _buildingSelectedIndex = -1;
    /// <summary>
    /// Public property that should be used both to access and mutate which building is selected to have its information displayed on the planet management menu's building tab building inspector via index.
    /// </summary>
    public int buildingSelectedIndex
    {
        get => _buildingSelectedIndex;
        set
        {
            //Sets the building selected ID variable to the specified value or -1 if the specified value is invalid.
            _buildingSelectedIndex = planetSelected != null && value >= 0 && value < planetSelected.city.buildings.Count ? value : -1;

            //Gets the buildingSelected and stores it in a variable to prevent constantly reusing the property and having unneeded comparisons for null checking.
            NewGalaxyBuilding buildingSelectedVar = buildingSelected;
            //Sets the activation state of the buildings tab building inspector.
            buildingsTabBuildingInspector.SetActive(buildingSelectedVar != null);
            //Sets the content of the buildings tab building inspector to properly display the info of the building selected.
            if(buildingSelectedVar != null)
            {
                buildingsTabBuildingInspectorBuildingTypeText.text = GeneralHelperMethods.GetEnumText(buildingSelectedVar.buildingType.ToString());
                buildingsTabBuildingInspectorBuildingLevelText.text = "Level: " + buildingSelectedVar.level;
                buildingsTabBuildingInspectorBuildingTypeIconImage.sprite = buildingSelectedVar.buildingTypeIconSprite;
                buildingsTabBuildingInspectorBuildingTypeDescriptionText.text = buildingSelectedVar.buildingTypeDescription;
                buildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = true;
            }
        }
    }
    /// <summary>
    /// Public property that should be used both to access and mutate which building is selected to have its information displayed on the planet management menu's building tab building inspector. Returns null if no valid building is selected. Works on top of the buildingSelectedIndex int property.
    /// </summary>
    public NewGalaxyBuilding buildingSelected
    {
        get => planetSelected != null && buildingSelectedIndex >= 0 && buildingSelectedIndex < planetSelected.city.buildings.Count ? planetSelected.city.buildings[buildingSelectedIndex] : null;
        set
        {
            //Checks if the specified value or the planet selected on the menu is null and sets the building selected index to -1 (indicating that no valid building is selected) and returns if so.
            if(value == null || planetSelected == null)
            {
                buildingSelectedIndex = -1;
                return;
            }

            //Loops through each building in the city on the assigned planet until finding the index of the specified building. Then, the buildingSelectedIndex property is used to set the specified building as the one selected.
            for (int buildingIndex = 0; buildingIndex < planetSelected.city.buildings.Count; buildingIndex++)
            {
                if(value == planetSelected.city.buildings[buildingIndex])
                {
                    buildingSelectedIndex = buildingIndex;
                    return;
                }
            }

            //Sets the building selected index to -1 (indicating that no valid building is selected) since the specified building was not found when looping through each building in the city on the planet selected on the menu.
            buildingSelectedIndex = -1;
        }
    }

    /// <summary>
    /// Private variable that indicates whether the buildings tab building inspector description needs a formatting update the next frame in order to avoid unity scroll rect and content size fitter shenanigans.
    /// </summary>
    private bool buildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = false;
    /// <summary>
    /// Private variable that indicates how many frames have passed since the buildings tab buildings inspector description last required a formatting update.
    /// </summary>
    private int framesSinceBuildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = 0;

    //Test method that should be removed eventually.
    private void TestSetPlanet()
    {
        planetSelectedID = 0;
    }

    protected override void Awake()
    {
        base.Awake();

        //Test line that should be removed eventually.
        NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(TestSetPlanet, 3);
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
    }

    protected override void Update()
    {
        base.Update();

        //Deals with updating the formatting of the buildings tab building inspector description in order to avoid unity scroll rect and content size fitter shenanigans.
        if (buildingsTabBuildingInspectorDescriptionFormattingUpdateRequired)
        {
            if (framesSinceBuildingsTabBuildingInspectorDescriptionFormattingUpdateRequired > 0)
            {
                buildingsTabBuildingInspectorBuildingTypeDescriptionText.gameObject.SetActive(false);
                buildingsTabBuildingInspectorBuildingTypeDescriptionText.gameObject.SetActive(true);
                buildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = false;
                framesSinceBuildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = 0;
            }
            framesSinceBuildingsTabBuildingInspectorDescriptionFormattingUpdateRequired++;
        }
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

    /// <summary>
    /// Public method that should be called by a buildings tab scroll view button whenever it is clicked and selects the button's assigned building to be displayed in the building inspector.
    /// </summary>
    public void OnClickBuildingsTabScrollViewButton(int buildingsTabScrollViewButtonIndex)
    {
        buildingSelectedIndex = buildingsTabScrollViewButtonIndex;
    }

    /// <summary>
    /// Private method that should be called in order to clear the buildings tab scroll view's buttons.
    /// </summary>
    private void ClearBuildingsTabScrollViewButtons()
    {
        for(int buildingsTabScrollViewButtonIndex = buildingsTabScrollViewContent.childCount - 1; buildingsTabScrollViewButtonIndex >= 0; buildingsTabScrollViewButtonIndex--)
            Destroy(buildingsTabScrollViewContent.GetChild(buildingsTabScrollViewButtonIndex).gameObject);
    }

    /// <summary>
    /// Private method that should be called in order to populate the buildings tab scroll view buttons where each button represents a building on the planet.
    /// </summary>
    private void PopulateBuildingsTabScrollViewButtons()
    {
        //Checks if there isn't a planet selected by the planet management menu and clear the buttons and returns if so.
        if(planetSelected == null)
        {
            ClearBuildingsTabScrollViewButtons();
            return;
        }

        //Loops through each building in the city on the assigned planet and instantiates a new buildings tab scroll view button for it.
        for(int buildingIndex = 0; buildingIndex < planetSelected.city.buildings.Count; buildingIndex++)
            PlanetManagementMenuBuildingsTabScollViewButton.InstantiateNewButton(this, planetSelected.city.buildings[buildingIndex]);
    }
}
