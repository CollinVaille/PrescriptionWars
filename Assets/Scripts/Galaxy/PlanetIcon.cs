using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetIcon : MonoBehaviour
{
    //Name label
    public Text nameLabel;
    private int currentFontSize = 10, fontScale = 10000;
    private static Transform mainCamTransform = null;

    //Planet biome
    public Planet.Biome biome;

    //Planet culture
    public Empire.Culture culture;

    //Planet information
    public int ownerID = -1;
    public int planetID = -1;
    public bool isCapital;

    //A list of all the planets this planet is connected to via the hyperspace lanes
    public List<int> neighborPlanets;

    public GameObject ship;

    public void GenerateShip(GameObject shipDaddy, GameObject shipPrefab)
    {
        GameObject newShip = Instantiate(shipPrefab);
        newShip.transform.parent = shipDaddy.transform;

        newShip.transform.position = new Vector3(transform.position.x + 10, transform.position.y, transform.position.z + 10);
        newShip.GetComponent<PlanetShip>().attachedPlanetID = planetID;

        newShip.GetComponent<MeshRenderer>().sharedMaterial = GalaxyManager.empireMaterials[(int)Empire.empires[ownerID].empireCulture];

        ship = newShip;
    }

    public float creditsPerTurn()
    {
        float credits = 0.0f;

        foreach(GalaxyCity galaxyCity in cities)
        {
            credits += galaxyCity.GetCreditsPerTurn(ownerID);
        }

        return credits;
    }

    public float prescriptionsPerTurn()
    {
        float prescriptions = 0.0f;

        foreach(GalaxyCity galaxyCity in cities)
        {
            prescriptions += galaxyCity.GetPrescriptionsPerTurn(ownerID);
        }

        return prescriptions;
    }

    public float sciencePerTurn()
    {
        float science = 0.0f;

        foreach(GalaxyCity galaxyCity in cities)
        {
            science += galaxyCity.GetSciencePerTurn(ownerID);
        }

        return science;
    }

    Vector3 rotation;

    //Cities
    public List<GalaxyCity> cities = new List<GalaxyCity>();

    public void InitializePlanet (string planetName)
    {
        AddNameLabel(planetName);

        //Amount the planet will rotate.
        rotation = new Vector3(0, 0, Random.Range(5, 21));
    }

    public void GenerateCities(bool isCapital)
    {
        int totalBuildings = isCapital ? Random.Range(15, 20) : Random.Range(7, 14);

        int minimumBuildingsPerCity = 3;

        while(totalBuildings > 0)
        {
            GalaxyCity galaxyCity = new GalaxyCity();

            galaxyCity.buildingLimit = Random.Range(minimumBuildingsPerCity, 7);
            totalBuildings -= galaxyCity.buildingLimit;

            //If next city is too small, then just make this the last city and give it the remaining size
            if(totalBuildings < minimumBuildingsPerCity)
            {
                galaxyCity.buildingLimit += totalBuildings;
                totalBuildings = 0;
            }

            //Generates a random city name and ensures no duplicate city names on the same planet.
            bool goodCityName = false;
            while (!goodCityName)
            {
                goodCityName = true;
                galaxyCity.cityName = CityGenerator.GenerateCityName(biome, galaxyCity.buildingLimit * 15);

                foreach(GalaxyCity city in cities)
                {
                    if (city.cityName.Equals(galaxyCity.cityName))
                        goodCityName = false;
                }
            }

            galaxyCity.baseCreditsPerTurn = 1.0f;
            galaxyCity.basePrescriptionsPerTurn = 1.0f;
            galaxyCity.baseProductionPerTurn = 1.0f;
            galaxyCity.baseSciencePerTurn = 1.0f;

            cities.Add(galaxyCity);
        }
    }

    private void AddNameLabel (string planetName)
    {
        //Create label
        nameLabel = new GameObject(name + " Label").AddComponent<Text>();

        //Make it a child of canvas
        nameLabel.transform.SetParent(GameObject.Find("Planet Labels").transform, false);

        //Add it to UI layer
        nameLabel.gameObject.layer = 5;

        //Set gameobject name
        name = planetName;

        //Set text
        nameLabel.text = planetName;
        nameLabel.gameObject.name = planetName + " Label";

        //Set font
        nameLabel.font = GalaxyMenu.galaxyMenu.planetNameFont;

        //Set font size
        fontScale = 3000;
        mainCamTransform = Camera.main.transform;

        //Long names will be invisible without this
        nameLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameLabel.verticalOverflow = VerticalWrapMode.Overflow;

        //Center text below planet
        nameLabel.alignment = TextAnchor.MiddleCenter;
    }

    private void Update ()
    {
        //Update name label
        if(nameLabel)
        {
            //Size
            currentFontSize = (int)(fontScale / mainCamTransform.position.y);
            if (currentFontSize != nameLabel.fontSize)
                nameLabel.fontSize = currentFontSize;

            //Position
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            screenPosition.y -= currentFontSize * 4;
            nameLabel.transform.position = screenPosition;
        }

        //Rotates the planet.
        transform.localEulerAngles += rotation * Time.deltaTime;

        //Updates the ship.
        if(ownerID == GalaxyManager.playerID)
        {
            if (!ship.activeInHierarchy)
                ship.SetActive(true);
        }
        else
        {
            if (ship.activeInHierarchy)
                ship.SetActive(false);
        }
    }

    public void SetPlanetOwner(int ownerID)
    {
        this.ownerID = ownerID;
        nameLabel.color = Empire.empires[this.ownerID].GetLabelColor();
    }

    public void ConquerPlanet(int conquerorID)
    {
        //Removes the planet from the previous owner's list of owned planets.
        if(ownerID != -1)
        {
            for(int x = 0; x < Empire.empires[ownerID].planetsOwned.Count; x++)
            {
                if(Empire.empires[ownerID].planetsOwned[x] == planetID)
                {
                    Empire.empires[ownerID].planetsOwned.RemoveAt(x);
                    break;
                }
            }
        }

        //Adds the planet to the new owner's list of owned planets.
        Empire.empires[conquerorID].planetsOwned.Add(planetID);

        //Updates the color of the planet label.
        nameLabel.color = Empire.empires[conquerorID].GetLabelColor();

        //Sets the planet's owner id as the conqueror's id.
        ownerID = conquerorID;
    }

    private void OnMouseUpAsButton ()
    {
        if(ownerID == GalaxyManager.playerID && !GalaxyCamera.mouseOverPlanetManagementMenu && !GalaxyCamera.mouseOverArmyManagementMenu)
        {
            //Tells the planet management menu to display the information from this planet.
            PlanetManagementMenu.planetSelected = transform.gameObject;
            
            //Closes the planet management menu if it is already open, this is purely just to reset it before we immediately reopen it.
            if (GalaxyManager.planetManagementMenu.activeInHierarchy)
            {
                GalaxyManager.planetManagementMenu.GetComponent<PlanetManagementMenu>().CloseMenu();
            }

            //Activates the planet management menu.
            PlanetManagementMenu.planetManagementMenu.gameObject.SetActive(true);
            PlanetManagementMenu.planetManagementMenu.ResetChooseCityMenu();
            PlanetManagementMenu.planetManagementMenu.UpdateUI();
            PlanetManagementMenu.planetManagementMenu.PlayOpenMenuSFX();
        }
    }

    public void EndTurn()
    {
        foreach(GalaxyCity city in cities)
        {
            //Adds each city's resources per turn to the empire.
            Empire.empires[ownerID].credits += city.GetCreditsPerTurn(ownerID);
            Empire.empires[ownerID].prescriptions += city.GetPrescriptionsPerTurn(ownerID);
            Empire.empires[ownerID].science += city.GetSciencePerTurn(ownerID);

            //Progresses the building queue.
            city.buildingQueue.EndTurn(city.GetProductionPerTurn(ownerID), city);
        }
    }
}

public class GalaxyBuilding
{
    public enum BuildingType
    {
        ResearchFacility,
        Depot,
        Prescriptor,
        TradePost
    }

    public static List<BuildingType> buildingEnums = new List<BuildingType>() {BuildingType.ResearchFacility, BuildingType.Depot, BuildingType.Prescriptor, BuildingType.TradePost };

    public static float GetCreditsCost(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.ResearchFacility:
                return 100.0f;
            case BuildingType.Depot:
                return 100.0f;
            case BuildingType.Prescriptor:
                return 100.0f;
            case BuildingType.TradePost:
                return 100.0f;

            default:
                return 1000;
        }
    }

    public static float GetBaseProductionCost(BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.ResearchFacility:
                return 10.0f;
            case BuildingType.Depot:
                return 10.0f;
            case BuildingType.Prescriptor:
                return 10.0f;
            case BuildingType.TradePost:
                return 10.0f;

            default:
                return 100.0f;
        }
    }

    public static float GetBuildingEffect(BuildingType buildingType, int ownerID)
    {
        switch (buildingType)
        {
            case BuildingType.ResearchFacility:
                return 1.0f + Empire.empires[ownerID].techManager.researchFacilityScienceProductionAmount;
            case BuildingType.Depot:
                return 1.0f;
            case BuildingType.Prescriptor:
                return 1.0f;
            case BuildingType.TradePost:
                return 1.0f + Empire.empires[ownerID].techManager.tradePostCreditsProductionAmount;

            default:
                return 0.0f;
        }
    }

    public BuildingType type;
}

public class BuildingQueue
{
    public List<GalaxyBuilding> buildingsQueued = new List<GalaxyBuilding>();

    public float production = 0.0f;

    public void AddBuildingToQueue(GalaxyBuilding.BuildingType buildingType)
    {
        GalaxyBuilding building = new GalaxyBuilding();

        building.type = buildingType;

        buildingsQueued.Add(building);
    }

    public List<string> GetQueueText()
    {
        List<string> queueText = new List<string>();

        foreach(GalaxyBuilding galaxyBuilding in buildingsQueued)
        {
            queueText.Add("" + galaxyBuilding.type);
        }

        return queueText;
    }

    public void EndTurn(float productionToAdd, GalaxyCity city)
    {
        production += productionToAdd;

        if(buildingsQueued.Count < 1)
        {
            //Sets the production back to zero if there is absolutely nothing in the queue.
            production = 0;
        }
        else
        {
            for(int x = 0; x < buildingsQueued.Count; x++)
            {
                if(production >= GalaxyBuilding.GetBaseProductionCost(buildingsQueued[x].type))
                {
                    //Removes the amount of production that it took to construct the building.
                    production -= GalaxyBuilding.GetBaseProductionCost(buildingsQueued[x].type);

                    //Adds the building to the completed buildings list in the city.
                    city.buildingsCompleted.Add(buildingsQueued[x]);

                    //Removes the building from the building queue.
                    buildingsQueued.RemoveAt(x);

                    x--;
                }
                else
                {
                    //This ensures that if the first building in the queue cannot yet be constructed but the second one can, then it won't skip over the first one and waste its production on the second (example).
                    break;
                }
            }
        }
    }
}

public class GalaxyCity
{
    //Buildings
    public List<GalaxyBuilding> buildingsCompleted = new List<GalaxyBuilding>();
    public BuildingQueue buildingQueue = new BuildingQueue();

    //Information
    public string cityName;
    public int buildingLimit;

    //Resources
    public float baseCreditsPerTurn;
    public float basePrescriptionsPerTurn;
    public float baseProductionPerTurn;
    public float baseSciencePerTurn;

    public void AddBuildingToQueue(GalaxyBuilding.BuildingType buildingType, int ownerID)
    {
        if(Empire.empires[ownerID].credits >= GalaxyBuilding.GetCreditsCost(buildingType) && buildingsCompleted.Count + buildingQueue.buildingsQueued.Count < buildingLimit)
        {
            //Subtracts the appropriate amount of credits from the empire based on the building type.
            Empire.empires[ownerID].credits -= GalaxyBuilding.GetCreditsCost(buildingType);

            //Adds a new galaxy building to the building queue with the appropriate building type.
            buildingQueue.AddBuildingToQueue(buildingType);
        }
    }

    public float GetCreditsPerTurn(int ownerID)
    {
        float credits = baseCreditsPerTurn;

        credits += Empire.empires[ownerID].techManager.baseCreditsProductionAmount;

        foreach(GalaxyBuilding building in buildingsCompleted)
        {
            if (building.type == GalaxyBuilding.BuildingType.TradePost)
                credits += GalaxyBuilding.GetBuildingEffect(GalaxyBuilding.BuildingType.TradePost, ownerID);
        }

        return credits;
    }

    public float GetPrescriptionsPerTurn(int ownerID)
    {
        float prescriptions = basePrescriptionsPerTurn;

        foreach(GalaxyBuilding building in buildingsCompleted)
        {
            if (building.type == GalaxyBuilding.BuildingType.Prescriptor)
                prescriptions += GalaxyBuilding.GetBuildingEffect(GalaxyBuilding.BuildingType.Prescriptor, ownerID);
        }

        return prescriptions;
    }

    public float GetProductionPerTurn(int ownerID)
    {
        float production = baseProductionPerTurn;

        production += Empire.empires[ownerID].techManager.baseProductionProductionAmount;

        foreach(GalaxyBuilding building in buildingsCompleted)
        {
            if (building.type == GalaxyBuilding.BuildingType.Depot)
                production += GalaxyBuilding.GetBuildingEffect(GalaxyBuilding.BuildingType.Depot, ownerID);
        }

        return production;
    }

    public float GetSciencePerTurn(int ownerID)
    {
        float science = baseSciencePerTurn;

        foreach(GalaxyBuilding building in buildingsCompleted)
        {
            if (building.type == GalaxyBuilding.BuildingType.ResearchFacility)
                science += GalaxyBuilding.GetBuildingEffect(GalaxyBuilding.BuildingType.ResearchFacility, ownerID);
        }

        return science;
    }

    public List<string> GetBuildingsListText()
    {
        List<string> buildingsListText = new List<string>();

        foreach (GalaxyBuilding galaxyBuilding in buildingsCompleted)
        {
            buildingsListText.Add("" + galaxyBuilding.type);
        }

        return buildingsListText;
    }
}
