﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : MonoBehaviour
{
    //Audio stuff.
    public AudioSource sfxSource;

    public AudioClip openMenuAudioClip;
    public AudioClip clickOnCityAudioClip;
    public AudioClip clickOnTabAudioClip;
    public AudioClip clickThreeAudioClip;
    public AudioClip cancelAudioClip;
    public AudioClip demolishAudioClip;

    //General stuff.
    public Image foregroundImage;

    public List<Image> dividers;

    public Image tabUnderlineImage;

    public Text planetNameText;

    public static GameObject planetSelected;

    public List<Shadow> shadows;

    public List<GameObject> tabs;

    public Sprite unselectedButtonSprite;
    public Sprite selectedButtonSprite;

    //Choose city menu stuff.
    public GameObject chooseCityMenu;
    public List<GameObject> cities;
    public List<Image> cityImages;
    public List<Text> cityTexts;

    //City management menu stuff here.
    public GameObject cityManagementMenu;
    public Scrollbar buildingsCompletedScrollbar;
    public List<Image> demolishBuildingImages;
    int buildingsListTextStartIndex;
    public Scrollbar buildingQueueScrollbar;
    public List<Image> cancelBuildingQueuedImages;
    int buildingQueueListTextStartIndex;
    public Text buildingsListText;
    public Text buildingQueueListText;
    public Text buildingsLimitText;
    public Text buildButtonText;
    public Text buildingDescriptionText;
    public Text buildingTitleText;
    public Text buildingCostText;
    public List<Button> cityManagementMenuButtons;
    public int buildingSelected;
    int buildingDisplayed = -1;
    public List<Sprite> buildingSprites;
    public List<string> buildingDescriptions;
    public Image buildingImage;

    //City sprite here.
    public Sprite desertCitySprite;
    public Sprite frozenCitySprite;
    public Sprite temperateCitySprite;
    public Sprite swampCitySprite;
    public Sprite hellCitySprite;
    public Sprite spiritCitySprite;

    //Info tab stuff here.
    public Text infoCultureText;
    public Text infoCitiesText;
    public Text infoCapitalText;
    public Text infoIncomeText;
    public Text infoPrescriptionText;

    public float updatesPerSecond;
    float timer;

    public int citySelected;

    //Menu dragging stuff here.
    bool beingMoved = false;
    Vector2 mouseToMenuDistance;

    public static PlanetManagementMenu planetManagementMenu;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //Closes the whole planet management menu if the user presses escape.
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.parent.childCount - 1 && !GalaxyManager.popupClosedOnFrame && !GalaxyConfirmationPopup.galaxyConfirmationPopup.gameObject.activeInHierarchy)
        {
            CloseMenu();
        }

        //Deals with the planet management menu being dragged by the player.
        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -175)
            {
                transform.localPosition = new Vector2(-175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < -440)
                    mouseToMenuDistance.x = -440;
            }
            //Right barrier.
            if (transform.localPosition.x > 175)
            {
                transform.localPosition = new Vector2(175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > 440)
                    mouseToMenuDistance.x = 440;
            }
            //Top barrier.
            if (transform.localPosition.y > 30)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 30);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > 323)
                    mouseToMenuDistance.y = 323;
            }
            //Bottom barrier.
            if (transform.localPosition.y < -62)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -62);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < -323)
                    mouseToMenuDistance.y = -323;
            }
        }

        //Brings the planet management menu above all of the other pop-ups if it is being pressed on.
        if (GalaxyCamera.mouseOverPlanetManagementMenu && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();

        //Updates the ui if the appropriate amount of time has passes (to increase performance).
        timer += Time.deltaTime;
        if(timer >= (1 / updatesPerSecond))
        {
            UpdateUI();

            timer = 0.0f;
        }
    }

    public void UpdateUI()
    {
        //Sets the color of the foreground and all of the dividers based on the player empire's label color.
        foregroundImage.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        foreach (Image divider in dividers)
        {
            divider.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        }

        buildingsCompletedScrollbar.image.color = Empire.empires[GalaxyManager.playerID].empireColor;
        buildingQueueScrollbar.image.color = Empire.empires[GalaxyManager.playerID].empireColor;
        buildButtonText.color = Empire.empires[GalaxyManager.playerID].empireColor;
        tabUnderlineImage.color = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        buildingCostText.color = Empire.empires[GalaxyManager.playerID].empireColor;

        //UI components that require a valid planet to be selcted.
        if (planetSelected != null)
        {
            PlanetIcon planetSelectedScript = planetSelected.GetComponent<PlanetIcon>();

            planetNameText.text = planetSelectedScript.nameLabel.text;

            if (tabs[0].activeInHierarchy || timer < (1 / updatesPerSecond))
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
                    planetNameText.text = planetSelectedScript.cities[citySelected].cityName + ", " + planetSelectedScript.nameLabel.text;
                }

                //Updates the tabs unerline image location.
                if (tabs[0].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != -105)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(-105, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
            if(tabs[1].activeInHierarchy || timer < (1 / updatesPerSecond))
            {
                if (tabs[1].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != 0)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(0, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
            if (tabs[2].activeInHierarchy || timer < (1 / updatesPerSecond))
            {
                infoCultureText.text = "Culture: " + planetSelectedScript.culture;
                infoCitiesText.text = "Cities: " + planetSelectedScript.cities.Count;
                infoCapitalText.text = "Capital: " + planetSelectedScript.isCapital;
                infoIncomeText.text = "Income: " + planetSelectedScript.creditsPerTurn();
                infoPrescriptionText.text = "Prescription: " + planetSelectedScript.prescriptionsPerTurn();

                if(tabs[2].activeInHierarchy && tabUnderlineImage.gameObject.transform.localPosition.x != 105)
                {
                    tabUnderlineImage.gameObject.transform.localPosition = new Vector3(105, tabUnderlineImage.gameObject.transform.localPosition.y, tabUnderlineImage.gameObject.transform.localPosition.z);
                }
            }
        }
    }

    public void PointerDownPlanetManagementMenu()
    {
        //Tells the update function that the player is dragging the menu.
        beingMoved = true;

        //Tells the update function the set difference between the mouse position and the menu's position.
        mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;
        mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;
    }

    public void PointerUpPlanetManagementMenu()
    {
        //Tells the update function that the player is no longer dragging the menu.
        beingMoved = false;

        //Resets the vector that says the difference between the mouse position and the menu's position.
        mouseToMenuDistance = Vector2.zero;
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
        //Figures out which index to remove at.
        int indexToDemolish = buildingsListTextStartIndex + num;

        //Creates a confirmation pop-up to query the user on whether or not they actually want to demolish that building.
        GalaxyConfirmationPopup.NewDemolishBuildingConfirmation(planetSelected.GetComponent<PlanetIcon>().planetID, citySelected, indexToDemolish);
    }

    public void DemolishBuilding(int indexToDemolish)
    {
        //Removes the building at the requested index.
        planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingsCompleted.RemoveAt(indexToDemolish);

        //Plays the demolish and cancel sound effects.
        sfxSource.PlayOneShot(cancelAudioClip);
        sfxSource.PlayOneShot(demolishAudioClip);

        //Updates the ui.
        UpdateUI();
    }

    public void ConfirmCancelBuildingQueued(int num)
    {
        //Figures out which index to remove at.
        int indexToCancel = buildingQueueListTextStartIndex + num;

        //Creates a confirmation pop-up to query the user on whether or not they actually want to cencel that queued building.
        GalaxyConfirmationPopup.NewCancelBuildingQueuedConfirmation(planetSelected.GetComponent<PlanetIcon>().planetID, citySelected, indexToCancel);
    }

    public void CancelBuildingQueued(int indexToCancel)
    {
        //Refunds the player of the credits that they spent putting that building into the building queue.
        Empire.empires[GalaxyManager.playerID].credits += GalaxyBuilding.GetCreditsCost(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued[indexToCancel].type);

        //Removes the building in the queue at the requested index.
        planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.RemoveAt(indexToCancel);

        //Plays the removal/cancel sound effect.
        sfxSource.PlayOneShot(cancelAudioClip);

        //Updates the ui.
        UpdateUI();
    }

    public void PlayOpenMenuSFX()
    {
        sfxSource.PlayOneShot(openMenuAudioClip);
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
        sfxSource.PlayOneShot(clickThreeAudioClip);
    }

    //Adds a new galaxy building to a city's building queue.
    public void AddBuildingToQueue()
    {
        if(planetSelected != null && planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingsCompleted.Count + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingLimit && Empire.empires[GalaxyManager.playerID].credits >= GalaxyBuilding.GetCreditsCost((GalaxyBuilding.BuildingType)buildingSelected))
        {
            //Adds a building of the specified type to the building queue.
            planetSelected.GetComponent<PlanetIcon>().cities[citySelected].AddBuildingToQueue((GalaxyBuilding.BuildingType)buildingSelected, GalaxyManager.playerID);

            //Plays the add to queue sound effect.
            sfxSource.PlayOneShot(clickThreeAudioClip);
        }
    }

    /*public void OnButtonEnter(Image buttonImage)
    {
        buttonImage.sprite = selectedButtonSprite;
    }

    public void OnButtonExit(Image buttonImage)
    {
        buttonImage.sprite = unselectedButtonSprite;
    }*/

    public void ClickOnCity(int cityNum)
    {
        //Sets which city the player selected.
        citySelected = cityNum;

        //Changes the menu and updates the ui.
        chooseCityMenu.SetActive(false);
        cityManagementMenu.SetActive(true);
        UpdateUI();

        //Plays sound effect.
        sfxSource.PlayOneShot(clickOnCityAudioClip);
    }

    public void ResetChooseCityMenu()
    {
        //Sets the correct number of cities active.
        for(int x = 0; x < cities.Count; x++)
        {
            if (x < planetSelected.GetComponent<PlanetIcon>().cities.Count)
                cities[x].SetActive(true);
            else
                cities[x].SetActive(false);
        }

        //Resets the image of each city based on the biome.
        foreach(Image cityImage in cityImages)
        {
            cityImage.sprite = GetCityImage(planetSelected.GetComponent<PlanetIcon>().biome);
        }

        //Resets the name of each city.
        for(int x = 0; x < cityTexts.Count; x++)
        {
            if(cities[x].activeInHierarchy)
                cityTexts[x].text = planetSelected.GetComponent<PlanetIcon>().cities[x].cityName;
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
            case Planet.Biome.Desert:
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

        List<string> getBuildingsListText = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText();

        if (planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count <= 4)
        {
            buildingsListTextStartIndex = 0;

            for(int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count; x++)
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
            int possibleValues = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count - 3;

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

        List<string> buildingQueueText = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText();

        if (planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count <= 4)
        {
            buildingQueueListTextStartIndex = 0;

            for (int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count; x++)
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
            int possibleValues = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count - 3;

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
        sfxSource.PlayOneShot(clickOnTabAudioClip);
    }

    public void OpenMenu()
    {
        //Activates the planet management menu gameobject.
        transform.gameObject.SetActive(true);
        //Brings the planet mangement menu on top of all of the other pop-ups.
        transform.SetAsLastSibling();
        //Resets the choose city menu.
        ResetChooseCityMenu();
        //Updates the ui elements of the whole menu.
        UpdateUI();
        //Plays the sound effect for opening the planet management menu.
        PlayOpenMenuSFX();
    }

    public void CloseMenu()
    {
        //Logs with the galaxy manager that a popup has been closed on this frame (so that other popups will not close on the same frame because of the escape key being pressed).
        GalaxyManager.popupClosedOnFrame = true;

        //Resets whether the planet management menu is being dragged by the player.
        PointerUpPlanetManagementMenu();

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

        //Places the planet management menu at the top of the pop-ups object's hierarchy (last priority).
        transform.SetSiblingIndex(0);

        //Deactivates the whole planet management menu.
        transform.gameObject.SetActive(false);

        //Plays the sound effect.
        PlayOpenMenuSFX();
    }

    public void ToggleShadow(Shadow shadow)
    {
        shadow.effectColor = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        shadow.enabled = !shadow.enabled;
    }
}
