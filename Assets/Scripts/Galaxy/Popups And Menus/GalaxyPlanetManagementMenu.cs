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
    [SerializeField] private List<Text> buildingsTabBuildingInspectorResourceOutputTexts = null;
    [SerializeField] private List<GalaxyTooltip> buildingsTabBuildingInspectorResourceOutputAmountTooltips = null;
    [SerializeField] private Text buildingsTabBuildingInspectorUpgradeButtonText = null;

    [Header("Image Components")]

    [SerializeField] private Image coloringBackgroundImage = null;
    [SerializeField] private Image buildingsTabScrollViewHandleImage = null;
    [SerializeField] private Image buildingsTabScrollViewViewportImage = null;
    [SerializeField, Tooltip("The image component in the buildings inspector on the buildings tab of the menu that displays the icon of the type of building that is selected in the building inspector to the player.")] private Image buildingsTabBuildingInspectorBuildingTypeImage = null;
    [SerializeField] private Image buildingsTabBuildingInspectorUpgradeBuildingButtonImage = null;

    [Header("Button Components")]

    [SerializeField] private Button buildingsTabBuildingInspectorUpgradeButton = null;

    [Header("Unselected Tab Button Color Block")]

    [SerializeField] private ColorBlock unselectedTabButtonColorBlock;

    [Header("Selected Tab Button Color Block")]

    [SerializeField] private ColorBlock selectedTabButtonColorBlock;

    [Header("SFX Options")]

    [SerializeField] private AudioClip changeTabSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;
    [SerializeField] private AudioClip startUpgradingBuildingSFX = null;
    [SerializeField] private AudioClip selectBuildingSFX = null;

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
            //Stores the previously selected building index in a temporary variable.
            int previouslySelectedBuildingIndex = _buildingSelectedIndex;

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
                buildingsTabBuildingInspectorBuildingTypeImage.sprite = buildingSelectedVar.buildingTypeSprite;
                buildingsTabBuildingInspectorBuildingTypeDescriptionText.text = buildingSelectedVar.buildingTypeDescription;
                buildingsTabBuildingInspectorDescriptionFormattingUpdateRequired = true;
                for(int resourceOutputTextIndex = 0; resourceOutputTextIndex < buildingsTabBuildingInspectorResourceOutputTexts.Count; resourceOutputTextIndex++)
                    buildingsTabBuildingInspectorResourceOutputTexts[resourceOutputTextIndex].text = "0";
                buildingsTabBuildingInspectorResourceOutputTexts[(int)buildingSelectedVar.resourceModifier.resourceType].text = (buildingSelectedVar.resourceModifier.mathematicalOperation == GalaxyResourceModifier.MathematicalOperation.Addition ? "+" : "*") + buildingSelectedVar.resourceModifier.amount;
                for (int resourceOutputAmountTooltipIndex = 0; resourceOutputAmountTooltipIndex < buildingsTabBuildingInspectorResourceOutputAmountTooltips.Count; resourceOutputAmountTooltipIndex++)
                    buildingsTabBuildingInspectorResourceOutputAmountTooltips[resourceOutputAmountTooltipIndex].Text = "0 (" + GeneralHelperMethods.GetEnumText(((GalaxyResourceModifier.MathematicalOperation)0).ToString()) + ")";
                buildingsTabBuildingInspectorResourceOutputAmountTooltips[(int)buildingSelectedVar.resourceModifier.resourceType].Text = buildingSelectedVar.resourceModifier.amount + " (" + GeneralHelperMethods.GetEnumText(buildingSelectedVar.resourceModifier.mathematicalOperation.ToString()) + ")";
                buildingsTabBuildingInspectorUpgradeButton.interactable = !buildingSelectedVar.upgrading;
                buildingsTabBuildingInspectorUpgradeButtonText.text = buildingSelectedVar.upgrading ? "Building Upgrading (Progress: " + buildingSelectedVar.productionTowardsUpgrading + "/" + buildingSelectedVar.upgradeProductionCost + " Production)" : "Upgrade Building (Cost: " + buildingSelectedVar.upgradeCreditsCost + " Credits, " + buildingSelectedVar.upgradeProductionCost + " Production)";

                //Plays the appropriate sound effect if needed.
                if (_buildingSelectedIndex != previouslySelectedBuildingIndex)
                    AudioManager.PlaySFX(selectBuildingSFX);
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

    /// <summary>
    /// Private static list that contains all of the planet management menus that are currently open and active in the galaxy scene.
    /// </summary>
    private static List<GalaxyPlanetManagementMenu> planetManagementMenus = null;

    /// <summary>
    /// Private static property that should be used in order to access the game object that serves as the prefab that all planet management menus are instantiated from.
    /// </summary>
    private static GameObject prefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Planet Management Menu/Planet Management Menu"); }

    /// <summary>
    /// Public static method that returns a boolean value that indicates whether or not the specified planet is selected in an open and active planet management menu.
    /// </summary>
    /// <param name="planet"></param>
    /// <returns></returns>
    public static bool IsPlanetSelectedInAMenu(NewGalaxyPlanet planet)
    {
        return GetMenuWithPlanetSelected(planet) != null;
    }

    /// <summary>
    /// Public static method that returns the planet management menu that has the specified planet selected, if no planet management menu has the specified planet selected then a null value is returned.
    /// </summary>
    /// <param name="planet"></param>
    /// <returns></returns>
    public static GalaxyPlanetManagementMenu GetMenuWithPlanetSelected(NewGalaxyPlanet planet)
    {
        if (planetManagementMenus == null)
            return null;

        int planetID = planet == null ? -1 : planet.ID;
        foreach (GalaxyPlanetManagementMenu planetManagementMenu in planetManagementMenus)
            if (planetManagementMenu.planetSelectedID == planetID)
                return planetManagementMenu;

        return null;
    }

    /// <summary>
    /// Public static method that should be called in order to open up a new planet management menu with the specified planet selected. If a planet management menu with the specified planet selected already exists, then it is made the top popup.
    /// </summary>
    /// <param name="planet"></param>
    public static void OpenPlanetManagementMenu(NewGalaxyPlanet planet)
    {
        //Returns if there is no active galaxy scene.
        if (!NewGalaxyManager.activeInHierarchy)
            return;

        //Determines if a planet management menu with the specified planet selected already exists and makes it the top popup and returns if so.
        GalaxyPlanetManagementMenu planetManagementMenu = GetMenuWithPlanetSelected(planet);
        if (planetManagementMenu != null)
        {
            planetManagementMenu.transform.SetAsLastSibling();
            return;
        }

        //Instantiates a new planet management menu from the prefab.
        planetManagementMenu = Instantiate(prefab).GetComponent<GalaxyPlanetManagementMenu>();
        //Sets the parent of the planet management menu as the popups parent transform.
        planetManagementMenu.transform.SetParent(NewGalaxyManager.popupsParent);
        //Resets the position of the planet management menu.
        planetManagementMenu.transform.localPosition = Vector3.zero;
        //Sets the selected planet of the planet management menu.
        planetManagementMenu.planetSelected = planet;
    }

    protected override void Awake()
    {
        base.Awake();

        //Initializes the static list of planet management menus if it has not yet been initialized.
        if (planetManagementMenus == null)
            planetManagementMenus = new List<GalaxyPlanetManagementMenu>();
        //Adds this planet management menu to the static list of planet management menus.
        planetManagementMenus.Add(this);
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
        //Clears the buildings tab scroll view buttons.
        ClearBuildingsTabScrollViewButtons();

        //Checks if there isn't a planet selected by the planet management menu and returns if so.
        if (planetSelected == null)
            return;

        //Loops through each building in the city on the assigned planet and instantiates a new buildings tab scroll view button for it.
        for(int buildingIndex = 0; buildingIndex < planetSelected.city.buildings.Count; buildingIndex++)
            PlanetManagementMenuBuildingsTabScollViewButton.InstantiateNewButton(this, planetSelected.city.buildings[buildingIndex]);
    }

    /// <summary>
    /// Public method that should be called through an event trigger whenever the buildings tab building inspector upgrade button is clicked and launches a confirmation popup either confirming that the player wishes to upgrade the building or informing the player that they do not have the required amount of credits.
    /// </summary>
    public void OnClickBuildingsTabBuildingInspectorUpgradeButton()
    {
        if (planetSelected.owner.credits < buildingSelected.upgradeCreditsCost)
            StartCoroutine(ConfirmAcknowledgementOfNeedingCreditsToUpgradeBuildingAction());
        else
            StartCoroutine(ConfirmUpgradingSelectedBuildingAction());

        //Plays the appropriate sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);
    }

    /// <summary>
    /// Private coroutine that confirms that the player acknowledges that they do not have the credits required in order to upgrade the currently selected building.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmAcknowledgementOfNeedingCreditsToUpgradeBuildingAction()
    {
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Not Enough Credits", "You do not have enough credits to upgrade this building to the next level at the moment.", true);

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Private coroutine that confirms that the player wants to upgrade the currently selected building to the next level.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmUpgradingSelectedBuildingAction()
    {
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Upgrade Building", "Are you sure that you want to start upgrading this " + GeneralHelperMethods.GetEnumText(buildingSelected.buildingType.ToString()) + " to level " + (buildingSelected.level + 1) + " for " + buildingSelected.upgradeCreditsCost + " credits?");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
            StartUpgradingSelectedBuilding();

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Private method that should be called by the ConfirmUpgradingSelectedBuildingAction coroutine after the player confirms that they wish to start upgrading the selected building and starts upgrading the selected building and updates the UI and plays the appropriate sound effect.
    /// </summary>
    private void StartUpgradingSelectedBuilding()
    {
        buildingSelected.upgrading = true;
        buildingsTabBuildingInspectorUpgradeButton.interactable = false;
        buildingsTabBuildingInspectorUpgradeButtonText.text = "Building Upgrading (Progress: " + buildingSelected.productionTowardsUpgrading + "/" + buildingSelected.upgradeProductionCost + " Production)";

        //Plays the appropriate sound effect for starting to upgrade a building.
        AudioManager.PlaySFX(startUpgradingBuildingSFX);
    }

    /// <summary>
    /// Protected method that is called whenever the planet management menu is closed or destroyed by other means and removes the planet management menu from the static list of planet management menus as well as removes the base popup behaviour from the static list of popup behaviours.
    /// </summary>
    protected override void OnDestroy()
    {
        //Executes the base OnDestroy behaviour.
        base.OnDestroy();

        //Removes the planet management menu from the static list of planet management menus.
        planetManagementMenus.Remove(this);
        //Sets the static list of planet management menus to null if there are currently no planet management menus active and open within the game.
        if (planetManagementMenus.Count == 0)
            planetManagementMenus = null;
    }
}
