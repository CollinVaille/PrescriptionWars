using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : GalaxyPopupBehaviour
{
    [Header("Sub Menus")]

    [SerializeField] private GameObject chooseCityMenu = null;
    [SerializeField] private GameObject cityManagementMenu = null;
    [SerializeField] private List<GameObject> tabs = null;

    [Header("Sub Menu Components")]

    [SerializeField] private List<GameObject> cities = null;

    [Header("Image Components")]

    [SerializeField] private Image tabUnderlineImage = null;
    [SerializeField] private Image buildingImage = null;
    [SerializeField] private List<Image> dividers = null;
    [SerializeField] private List<Image> cityImages = null;
    [SerializeField] private List<Image> demolishBuildingImages = null;
    [SerializeField] private List<Image> cancelBuildingQueuedImages = null;

    [Header("Text Components")]

    [SerializeField] private Text planetNameText = null;
    [SerializeField] private Text buildingsListText = null;
    [SerializeField] private Text buildingQueueListText = null;
    [SerializeField] private Text buildingsLimitText = null;
    [SerializeField] private Text buildButtonText = null;
    [SerializeField] private Text buildingDescriptionText = null;
    [SerializeField] private Text buildingTitleText = null;
    [SerializeField] private Text buildingCostText = null;
    [SerializeField] private Text infoCultureText = null;
    [SerializeField] private Text infoCitiesText = null;
    [SerializeField] private Text infoCapitalText = null;
    [SerializeField] private Text infoIncomeText = null;
    [SerializeField] private Text infoPrescriptionText = null;
    [SerializeField] private List<Text> cityTexts = null;

    [Header("Scrollbar Components")]

    [SerializeField] private Scrollbar buildingsCompletedScrollbar = null;
    [SerializeField] private Scrollbar buildingQueueScrollbar = null;

    [Header("Button Components")]

    [SerializeField] private List<Button> cityManagementMenuButtons = null;

    [Header("Other Components")]

    [SerializeField] private List<Shadow> shadows = null;

    [Header("Sprite Options")]

    [SerializeField] private Sprite unselectedButtonSprite = null;
    [SerializeField] private Sprite desertCitySprite = null;
    [SerializeField] private Sprite frozenCitySprite = null;
    [SerializeField] private Sprite temperateCitySprite = null;
    [SerializeField] private Sprite swampCitySprite = null;
    [SerializeField] private Sprite hellCitySprite = null;
    [SerializeField] private Sprite spiritCitySprite = null;
    [SerializeField] private List<Sprite> buildingSprites = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip clickOnCityAudioClip = null;
    [SerializeField] private AudioClip clickOnTabAudioClip = null;
    [SerializeField] private AudioClip clickThreeAudioClip = null;
    [SerializeField] private AudioClip cancelAudioClip = null;
    [SerializeField] private AudioClip demolishAudioClip = null;

    [Header("Other Options")]

    [SerializeField] private List<string> buildingDescriptions = null;

    [Header("Additional Information")]

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private GalaxyPlanet planetSelected = null;
    public GalaxyPlanet PlanetSelected
    {
        get
        {
            return planetSelected;
        }
        set
        {
            planetSelected = value;
        }
    }

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int citySelected = 0;
    public int CitySelected
    {
        get
        {
            return citySelected;
        }
        set
        {
            citySelected = value;
        }
    }

    //Non-inspector variables.

    private int buildingSelected;
    private int buildingDisplayed = -1;
    private int buildingsListTextStartIndex = 0;
    private int buildingQueueListTextStartIndex = 0;

    public static PlanetManagementMenu planetManagementMenu;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public void UpdateUI()
    {
        //Sets the color of all of the dividers based on the player empire's label color.
        foreach (Image divider in dividers)
        {
            divider.color = Empire.empires[GalaxyManager.PlayerID].LabelColor;
        }

        buildingsCompletedScrollbar.image.color = Empire.empires[GalaxyManager.PlayerID].EmpireColor;
        buildingQueueScrollbar.image.color = Empire.empires[GalaxyManager.PlayerID].EmpireColor;
        buildButtonText.color = Empire.empires[GalaxyManager.PlayerID].EmpireColor;
        tabUnderlineImage.color = Empire.empires[GalaxyManager.PlayerID].LabelColor;
        buildingCostText.color = Empire.empires[GalaxyManager.PlayerID].EmpireColor;

        //UI components that require a valid planet to be selcted.
        if (planetSelected != null)
        {
            GalaxyPlanet planetSelectedScript = planetSelected;

            planetNameText.text = planetSelectedScript.Name;

            if (tabs[0].activeInHierarchy)
            {
                SetBuildingsListText();
                CheckDemolishBuildingSymbols();
                SetBuildingQueueListText();
                CheckCancelBuildingQueuedSymbols();
                buildingsLimitText.text = "Buildings Limit: " + planetSelectedScript.cities[citySelected].buildingLimit;
                if(buildingSelected != buildingDisplayed)
                {
                    buildingTitleText.text = GeneralHelperMethods.GetEnumText("" + (GalaxyBuilding.BuildingType)buildingSelected);
                    buildingDescriptionText.text = buildingDescriptions[buildingSelected];
                    buildingCostText.text = "" + GalaxyBuilding.GetCreditsCost((GalaxyBuilding.BuildingType)buildingSelected);
                    buildingImage.sprite = buildingSprites[buildingSelected];
                    buildingDisplayed = buildingSelected;
                }

                //Changes the planet name text to include the city name before it.
                if (cityManagementMenu.activeInHierarchy)
                {
                    planetNameText.text = planetSelectedScript.cities[citySelected].cityName + ", " + planetSelectedScript.Name;
                }

                //Updates the tabs unerline image location.
                if (tabs[0].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != -105)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(-105, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
            if(tabs[1].activeInHierarchy)
            {
                if (tabs[1].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != 0)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(0, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
            if (tabs[2].activeInHierarchy)
            {
                infoCultureText.text = "Culture: " + planetSelectedScript.Culture;
                infoCitiesText.text = "Cities: " + planetSelectedScript.cities.Count;
                infoCapitalText.text = "Capital: " + planetSelectedScript.IsCapital;
                infoIncomeText.text = "Income: " + planetSelectedScript.creditsPerTurn();
                infoPrescriptionText.text = "Prescription: " + planetSelectedScript.prescriptionsPerTurn();

                if(tabs[2].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != 105)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(105, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
        }
    }

    //Updates the demolish building symbol images.
    public void CheckDemolishBuildingSymbols()
    {
        int numberOfLines = buildingsListText.text.Split('\n').Length;

        if (buildingsListText.text.Equals(""))
            numberOfLines = 0;

        for(int x = 0; x < demolishBuildingImages.Count; x++)
        {
            if (x < numberOfLines)
            {
                if (!demolishBuildingImages[x].gameObject.activeInHierarchy)
                    demolishBuildingImages[x].gameObject.SetActive(true);
            }
            else if (demolishBuildingImages[x].gameObject.activeInHierarchy)
                demolishBuildingImages[x].gameObject.SetActive(false);
        }
    }

    //Updates the cancel queued building symbol images.
    public void CheckCancelBuildingQueuedSymbols()
    {
        int numberOfLines = buildingQueueListText.text.Split('\n').Length;

        if (buildingQueueListText.text.Equals(""))
            numberOfLines = 0;

        for(int x = 0; x < cancelBuildingQueuedImages.Count; x++)
        {
            if (x < numberOfLines)
            {
                if (!cancelBuildingQueuedImages[x].gameObject.activeInHierarchy)
                    cancelBuildingQueuedImages[x].gameObject.SetActive(true);
            }
            else if (cancelBuildingQueuedImages[x].gameObject.activeInHierarchy)
                cancelBuildingQueuedImages[x].gameObject.SetActive(false);
        }
    }

    public void ConfirmDelmolishBuilding(int num)
    {
        StartCoroutine(ConfirmDemolishBuildingCoroutine(num));
    }

    IEnumerator ConfirmDemolishBuildingCoroutine(int num)
    {
        //Figures out which index to remove at.
        int indexToDemolish = buildingsListTextStartIndex + num;

        //Creates the confirmation popup.
        GameObject confirmationPopup = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab);
        GalaxyConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyConfirmationPopup>();
        string topText = "Demolish Building";
        string bodyText = "Are you sure that you want to demolish a " + GeneralHelperMethods.GetEnumText(planetSelected.cities[citySelected].buildingsCompleted[indexToDemolish].type.ToString()) + " in the city " + planetSelected.cities[citySelected].cityName + " on planet " + planetSelected.Name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirmed their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
            DemolishBuilding(indexToDemolish);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    public void DemolishBuilding(int indexToDemolish)
    {
        //Removes the building at the requested index.
        planetSelected.cities[citySelected].buildingsCompleted.RemoveAt(indexToDemolish);

        //Plays the demolish and cancel sound effects.
        AudioManager.PlaySFX(cancelAudioClip);
        AudioManager.PlaySFX(demolishAudioClip);

        //Updates the ui.
        UpdateUI();
    }

    public void ConfirmCancelBuildingQueued(int num)
    {
        StartCoroutine(ConfirmCancelBuildingQueuedCoroutine(num));
    }

    IEnumerator ConfirmCancelBuildingQueuedCoroutine(int num)
    {
        //Figures out which index to remove at.
        int indexToCancel = buildingQueueListTextStartIndex + num;

        //Creates the confirmation popup.
        GameObject confirmationPopup = Instantiate(GalaxyConfirmationPopup.galaxyConfirmationPopupPrefab);
        GalaxyConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyConfirmationPopup>();
        string topText = "Cancel Building Queued";
        string bodyText = "Are you sure that you want to cancel building a " + GeneralHelperMethods.GetEnumText(planetSelected.cities[citySelected].buildingQueue.buildingsQueued[indexToCancel].type.ToString()) + " in the city " + planetSelected.cities[citySelected].cityName + " on planet " + planetSelected.Name + "?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirmed their action, it carries out the logic behind it.
        if(confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopup.GalaxyConfirmationPopupAnswer.Confirm)
            CancelBuildingQueued(indexToCancel);

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    public void CancelBuildingQueued(int indexToCancel)
    {
        //Refunds the player of the credits that they spent putting that building into the building queue.
        Empire.empires[GalaxyManager.PlayerID].Credits += GalaxyBuilding.GetCreditsCost(planetSelected.cities[citySelected].buildingQueue.buildingsQueued[indexToCancel].type);

        //Removes the building in the queue at the requested index.
        planetSelected.cities[citySelected].buildingQueue.buildingsQueued.RemoveAt(indexToCancel);

        //Plays the removal/cancel sound effect.
        AudioManager.PlaySFX(cancelAudioClip);

        //Updates the ui.
        UpdateUI();
    }

    public void ChangeBuildingSelected(string direction)
    {
        if (direction.Equals("right"))
        {
            if (buildingSelected < GalaxyBuilding.buildingEnums.Count - 1)
            {
                buildingSelected++;
            }
            else
            {
                buildingSelected = 0;
            }
        }
        else if (direction.Equals("left"))
        {
            if(buildingSelected > 0)
            {
                buildingSelected--;
            }
            else
            {
                buildingSelected = GalaxyBuilding.buildingEnums.Count - 1;
            }
        }

        //Plays the sound effect.
        AudioManager.PlaySFX(clickThreeAudioClip);
        //Updates the ui.
        UpdateUI();
    }

    //Adds a new galaxy building to a city's building queue.
    public void AddBuildingToQueue()
    {
        if(planetSelected != null && planetSelected.cities[citySelected].buildingsCompleted.Count + planetSelected.cities[citySelected].buildingQueue.buildingsQueued.Count < planetSelected.cities[citySelected].buildingLimit && Empire.empires[GalaxyManager.PlayerID].Credits >= GalaxyBuilding.GetCreditsCost((GalaxyBuilding.BuildingType)buildingSelected))
        {
            //Adds a building of the specified type to the building queue.
            planetSelected.cities[citySelected].AddBuildingToQueue((GalaxyBuilding.BuildingType)buildingSelected, GalaxyManager.PlayerID);

            //Plays the add to queue sound effect.
            AudioManager.PlaySFX(clickThreeAudioClip);
            //Updates the ui.
            UpdateUI();
        }
    }

    public void ClickOnCity(int cityNum)
    {
        //Sets which city the player selected.
        citySelected = cityNum;

        //Changes the menu and updates the ui.
        chooseCityMenu.SetActive(false);
        cityManagementMenu.SetActive(true);
        UpdateUI();

        //Plays sound effect.
        AudioManager.PlaySFX(clickOnCityAudioClip);
    }

    public void ResetChooseCityMenu()
    {
        //Sets the correct number of cities active.
        for(int x = 0; x < cities.Count; x++)
        {
            if (x < planetSelected.cities.Count)
                cities[x].SetActive(true);
            else
                cities[x].SetActive(false);
        }

        //Resets the image of each city based on the biome.
        foreach(Image cityImage in cityImages)
        {
            cityImage.sprite = GetCityImage(planetSelected.Biome);
        }

        //Resets the name of each city.
        for(int x = 0; x < cityTexts.Count; x++)
        {
            if(cities[x].activeInHierarchy)
                cityTexts[x].text = planetSelected.cities[x].cityName;
        }
    }

    //Returns the sample city image for the appropriate biome.
    Sprite GetCityImage(Planet.Biome biome)
    {
        switch(biome)
        {
            case Planet.Biome.Frozen:
                return frozenCitySprite;
            case Planet.Biome.Temperate:
                return temperateCitySprite;
            case Planet.Biome.SandyDesert:
                return desertCitySprite;
            case Planet.Biome.Swamp:
                return swampCitySprite;
            case Planet.Biome.Hell:
                return hellCitySprite;
            case Planet.Biome.Spirit:
                return spiritCitySprite;

            default:
                return desertCitySprite;
        }
    }

    public void SetBuildingsListText()
    {
        buildingsListText.text = "";

        List<string> getBuildingsListText = planetSelected.cities[citySelected].GetBuildingsListText();

        if (planetSelected.cities[citySelected].GetBuildingsListText().Count <= 4)
        {
            buildingsListTextStartIndex = 0;

            for(int x = 0; x < planetSelected.cities[citySelected].GetBuildingsListText().Count; x++)
            {
                if(x == 0)
                {
                    buildingsListText.text = GeneralHelperMethods.GetEnumText(getBuildingsListText[x]);
                }
                else
                {
                    buildingsListText.text += "\n" + GeneralHelperMethods.GetEnumText(getBuildingsListText[x]);
                }
            }
        }
        else
        {
            int possibleValues = planetSelected.cities[citySelected].GetBuildingsListText().Count - 3;

            int closestIndex = 0;

            for(int x = 0; x < GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues).Count; x++)
            {
                if(x == 0)
                {
                    closestIndex = 0;
                }
                else
                {
                    if(Mathf.Abs(GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues)[x] - buildingsCompletedScrollbar.value) < Mathf.Abs(GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues)[closestIndex] - buildingsCompletedScrollbar.value))
                    {
                        closestIndex = x;
                    }
                }
            }

            buildingsListTextStartIndex = closestIndex;

            for(int x = closestIndex; x < closestIndex + 4; x++)
            {
                if(x == closestIndex)
                {
                    buildingsListText.text = GeneralHelperMethods.GetEnumText(getBuildingsListText[x]);
                }
                else
                {
                    buildingsListText.text += "\n" + GeneralHelperMethods.GetEnumText(getBuildingsListText[x]);
                }
            }
        }
    }

    public void SetBuildingQueueListText()
    {
        buildingQueueListText.text = "";

        List<string> buildingQueueText = planetSelected.cities[citySelected].buildingQueue.GetQueueText();

        if (planetSelected.cities[citySelected].buildingQueue.buildingsQueued.Count <= 4)
        {
            buildingQueueListTextStartIndex = 0;

            for (int x = 0; x < planetSelected.cities[citySelected].buildingQueue.buildingsQueued.Count; x++)
            {
                if (x == 0)
                {
                    buildingQueueListText.text = GeneralHelperMethods.GetEnumText(buildingQueueText[x]);
                }
                else
                {
                    buildingQueueListText.text += "\n" + GeneralHelperMethods.GetEnumText(buildingQueueText[x]);
                }
            }
        }
        else
        {
            int possibleValues = planetSelected.cities[citySelected].buildingQueue.buildingsQueued.Count - 3;

            int closestIndex = 0;

            for (int x = 0; x < GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues).Count; x++)
            {
                if (x == 0)
                {
                    closestIndex = 0;
                }
                else
                {
                    if (Mathf.Abs(GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues)[x] - buildingQueueScrollbar.value) < Mathf.Abs(GalaxyHelperMethods.GetScrollbarValueNumbers(possibleValues)[closestIndex] - buildingQueueScrollbar.value))
                    {
                        closestIndex = x;
                    }
                }
            }

            buildingQueueListTextStartIndex = closestIndex;

            for (int x = closestIndex; x < closestIndex + 4; x++)
            {
                if (x == closestIndex)
                {
                    buildingQueueListText.text = GeneralHelperMethods.GetEnumText(buildingQueueText[x]);
                }
                else
                {
                    buildingQueueListText.text += "\n" + GeneralHelperMethods.GetEnumText(buildingQueueText[x]);
                }
            }
        }
    }

    public void ClickOnTab(int num)
    {
        if (!tabs[num].activeInHierarchy)
        {
            //Switches the active tab.
            for (int x = 0; x < tabs.Count; x++)
            {
                if (x == num)
                    tabs[x].SetActive(true);
                else
                    tabs[x].SetActive(false);
            }
        }

        //Resets the buildings tab.
        chooseCityMenu.SetActive(true);
        cityManagementMenu.SetActive(false);

        //Updates the ui.
        UpdateUI();

        //Plays the click on tab sound effect.
        AudioManager.PlaySFX(clickOnTabAudioClip);
    }

    public override void Open()
    {
        //Executes the super classes's logic for opening the popup.
        base.Open();

        //Resets the choose city menu.
        ResetChooseCityMenu();
        //Updates the ui elements of the whole menu.
        UpdateUI();
    }

    public override void Close()
    {
        //Executes the super classes's logic for closing the popup.
        base.Close();

        //Resets all of the shadows on text.
        foreach (Shadow shadow in shadows)
        {
            shadow.enabled = false;
        }

        //Resets all of the button textures.
        foreach(Button button in cityManagementMenuButtons)
        {
            button.image.sprite = unselectedButtonSprite;
        }

        //Resets the buildings tab.
        citySelected = 0;
        chooseCityMenu.SetActive(true);
        cityManagementMenu.SetActive(false);

        //Sets the selected tab back to the first tab (currently the buildings tab).
        for (int x = 0; x < tabs.Count; x++)
        {
            if (x == 0)
                tabs[x].SetActive(true);
            else
                tabs[x].SetActive(false);
        }
    }

    public void ToggleShadow(Shadow shadow)
    {
        shadow.effectColor = Empire.empires[GalaxyManager.PlayerID].LabelColor;
        shadow.enabled = !shadow.enabled;
    }
}
