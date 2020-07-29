using System.Collections;
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

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //Closes the whole planet management menu if the user presses escape.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }

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
                    buildingTitleText.text = GetEnumText("" + GalaxyBuilding.buildingEnums[buildingSelected]);
                    buildingDescriptionText.text = buildingDescriptions[buildingSelected];
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

    public void DemolishBuilding(int num)
    {
        //Figures out which index to remove at.
        int indexToDemolish = buildingsListTextStartIndex + num;

        //Removes the building at the requested index.
        planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingsCompleted.RemoveAt(indexToDemolish);

        //Plays the demolish and cancel sound effects.
        sfxSource.PlayOneShot(cancelAudioClip);
        sfxSource.PlayOneShot(demolishAudioClip);

        //Updates the ui.
        UpdateUI();
    }

    public void CancelBuildingQueued(int num)
    {
        //Figures out which index to remove at.
        int indexToCancel = buildingQueueListTextStartIndex + num;

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

    string GetEnumText(string text)
    {
        List<char> charList = new List<char>();
        char[] charArray = text.ToCharArray();
        foreach(char c in charArray)
        {
            charList.Add(c);
        }

        for(int x = text.Length - 1; x > -1; x--)
        {
            if (char.IsUpper(charList[x]) && x != 0)
            {
                charList.Insert(x, ' ');
            }
        }

        string finalText = "";
        foreach(char c in charList)
        {
            finalText += c;
        }

        return finalText;
    }

    public void ChangeBuildingSelected(string direction)
    {
        if (direction.Equals("right"))
        {
            if(buildingSelected < GalaxyBuilding.buildingEnums.Count - 1)
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
        if(planetSelected != null && planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingsCompleted.Count + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingLimit)
        {
            //Creates the galaxy building.
            GalaxyBuilding galaxyBuilding = new GalaxyBuilding();
            galaxyBuilding.type = GalaxyBuilding.buildingEnums[buildingSelected];

            //Adds the galaxy building to the city's building queue.
            planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Add(galaxyBuilding);

            //Plays the add to queue sound effect.
            sfxSource.PlayOneShot(clickThreeAudioClip);
        }
    }

    public void OnButtonEnter(Image buttonImage)
    {
        buttonImage.sprite = selectedButtonSprite;
    }

    public void OnButtonExit(Image buttonImage)
    {
        buttonImage.sprite = unselectedButtonSprite;
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

        if(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count <= 4)
        {
            buildingsListTextStartIndex = 0;

            for(int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count; x++)
            {
                if(x == 0)
                {
                    buildingsListText.text = GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x]);
                }
                else
                {
                    buildingsListText.text += "\n" + GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x]);
                }
            }
        }
        else
        {
            int possibleValues = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count - 3;

            int closestIndex = 0;

            for(int x = 0; x < GetValueNumbers(possibleValues).Count; x++)
            {
                if(x == 0)
                {
                    closestIndex = 0;
                }
                else
                {
                    if(Mathf.Abs(GetValueNumbers(possibleValues)[x] - buildingsCompletedScrollbar.value) < Mathf.Abs(GetValueNumbers(possibleValues)[closestIndex] - buildingsCompletedScrollbar.value))
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
                    buildingsListText.text = GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x]);
                }
                else
                {
                    buildingsListText.text += "\n" + GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x]);
                }
            }
        }
    }

    public void SetBuildingQueueListText()
    {
        buildingQueueListText.text = "";

        if (planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count <= 4)
        {
            buildingQueueListTextStartIndex = 0;

            for (int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count; x++)
            {
                if (x == 0)
                {
                    buildingQueueListText.text = GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x]);
                }
                else
                {
                    buildingQueueListText.text += "\n" + GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x]);
                }
            }
        }
        else
        {
            int possibleValues = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count - 3;

            int closestIndex = 0;

            for (int x = 0; x < GetValueNumbers(possibleValues).Count; x++)
            {
                if (x == 0)
                {
                    closestIndex = 0;
                }
                else
                {
                    if (Mathf.Abs(GetValueNumbers(possibleValues)[x] - buildingQueueScrollbar.value) < Mathf.Abs(GetValueNumbers(possibleValues)[closestIndex] - buildingQueueScrollbar.value))
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
                    buildingQueueListText.text = GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x]);
                }
                else
                {
                    buildingQueueListText.text += "\n" + GetEnumText(planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x]);
                }
            }
        }
    }

    public List<float> GetValueNumbers(int num)
    {
        List<float> valueNumbers = new List<float>();

        for(int x = 0; x < num; x++)
        {
            valueNumbers.Add(1.0f / (num - 1) * x);
        }

        return valueNumbers;
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

    public void CloseMenu()
    {
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
