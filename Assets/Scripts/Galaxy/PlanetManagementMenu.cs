using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetManagementMenu : MonoBehaviour
{
    public Image foregroundImage;

    public List<Image> dividers;

    public Text planetNameText;
    public Text buildingsListText;
    public Text buildingQueueListText;

    public static GameObject planetSelected;

    public GameObject chooseCityMenu;
    public List<GameObject> cities;
    public List<Image> cityImages;
    public List<Text> cityTexts;

    public GameObject cityManagementMenu;

    public List<Shadow> shadows;

    public List<GameObject> tabs;

    public Scrollbar buildingsCompletedScrollbar;
    public Scrollbar buildingQueueScrollbar;

    //City sprite here.
    public Sprite desertCitySprite;
    public Sprite frozenCitySprite;
    public Sprite temperateCitySprite;
    public Sprite swampCitySprite;
    public Sprite hellCitySprite;
    public Sprite forestCitySprite;
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

        if (planetSelected != null)
        {
            PlanetIcon planetSelectedScript = planetSelected.GetComponent<PlanetIcon>();

            planetNameText.text = planetSelectedScript.nameLabel.text;

            if (tabs[0].activeInHierarchy || timer < (1 / updatesPerSecond))
            {
                SetBuildingsListText();
                SetBuildingQueueListText();
            }
            if (tabs[2].activeInHierarchy || timer < (1 / updatesPerSecond))
            {
                infoCultureText.text = "Culture: " + planetSelectedScript.culture;
                infoCitiesText.text = "Cities: " + planetSelectedScript.cities.Count;
                infoCapitalText.text = "Capital: " + planetSelectedScript.isCapital;
                infoIncomeText.text = "Income: " + planetSelectedScript.creditsPerTurn();
                infoPrescriptionText.text = "Prescription: " + planetSelectedScript.prescriptionsPerTurn();
            }
        }
    }

    public void ClickOnCity(int cityNum)
    {
        citySelected = cityNum;
        chooseCityMenu.SetActive(false);
        cityManagementMenu.SetActive(true);
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
            case Planet.Biome.Forest:
                return forestCitySprite;
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
            for(int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText().Count; x++)
            {
                if(x == 0)
                {
                    buildingsListText.text = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x];
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

            for(int x = closestIndex; x < closestIndex + 4; x++)
            {
                if(x == closestIndex)
                {
                    buildingsListText.text = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].GetBuildingsListText()[x];
                }
            }
        }
    }

    public void SetBuildingQueueListText()
    {
        buildingQueueListText.text = "";

        if (planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count <= 4)
        {
            for (int x = 0; x < planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.buildingsQueued.Count; x++)
            {
                if (x == 0)
                {
                    buildingQueueListText.text = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x];
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

            for (int x = closestIndex; x < closestIndex + 4; x++)
            {
                if (x == closestIndex)
                {
                    buildingsListText.text = planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x];
                }
                else
                {
                    buildingsListText.text += "\n" + planetSelected.GetComponent<PlanetIcon>().cities[citySelected].buildingQueue.GetQueueText()[x];
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
        for(int x = 0; x < tabs.Count; x++)
        {
            if (x == num)
                tabs[x].SetActive(true);
            else
                tabs[x].SetActive(false);
        }
    }

    public void CloseMenu()
    {
        //Resets all of the shadows on text.
        foreach (Shadow shadow in shadows)
        {
            shadow.enabled = false;
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
    }

    public void ToggleShadow(Shadow shadow)
    {
        shadow.effectColor = Empire.empires[GalaxyManager.playerID].GetLabelColor();
        shadow.enabled = !shadow.enabled;
    }
}
