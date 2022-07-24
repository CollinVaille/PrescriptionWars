using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPlanet : MonoBehaviour
{
    [Header("Components")]

    [SerializeField] private Text nameLabel = null;

    [Header("Additional Information")]

    //Planet biome
    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private Planet.Biome biomeVar = Planet.Biome.Unknown;
    public Planet.Biome biome { get => biomeVar; }

    //Planet culture
    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private Empire.Culture cultureVar = Empire.Culture.Red;
    public Empire.Culture culture { get => cultureVar; set => cultureVar = value; }

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    public float rotationSpeed;

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int ownerIDVar = -1;
    /// <summary>
    /// Indicates the ID (index) of the empire that owns the planet.
    /// </summary>
    public int ownerID
    {
        get
        {
            return ownerIDVar;
        }
        set
        {
            ownerIDVar = value;
            nameLabel.color = Empire.empires[ownerIDVar].labelColor;
        }
    }
    /// <summary>
    /// Returns the empire that owns the planet.
    /// </summary>
    public Empire owner { get => Empire.empires[ownerID]; }

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int planetIDVar = -1;
    public int planetID { get => planetIDVar; set => planetIDVar = value; }

    public bool isCapital
    {
        get
        {
            return owner != null ? owner.capitalPlanetID == planetID : false;
        }
        set
        {
            if (owner != null)
                owner.capitalPlanetID = planetID;
        }
    }

    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private string materialName;

    public List<int> neighborPlanets { get => neighborPlanetsVar; }

    //Non-inspector variables.

    private string planetNameVar;
    public string planetName
    {
        get
        {
            return planetNameVar;
        }
        set
        {
            planetNameVar = value;
            gameObject.name = value;
            if (nameLabel != null)
                nameLabel.text = value;
            else
                AddNameLabel(value);
        }
    }

    //A list of all the planets this planet is connected to via the hyperspace lanes
    private List<int> neighborPlanetsVar = new List<int>();

    //Cities
    public List<GalaxyCity> cities = new List<GalaxyCity>();

    //Armies
    private List<GalaxyArmy> armies = new List<GalaxyArmy>();
    /// <summary>
    /// Returns the number of armies that are located on the planet.
    /// </summary>
    public int armyCount { get => armies.Count; }
    /// <summary>
    /// Indicates the maximum number of armies that are allowed to be stationed on the planet.
    /// </summary>
    public int armyCountLimit { get => 2; }
    /// <summary>
    /// Indicates whether the limit of how many armies can be stationed on the planet has been reached.
    /// </summary>
    public bool armyCountLimitReached
    {
        get
        {
            if (planetShips.Count == 0)
                return false;
            else
                return armyCount >= planetShips[0].OffsetPositions.Length || armyCount >= armyCountLimit;
        }
    }

    //Ships
    private List<PlanetShip> planetShips = new List<PlanetShip>();

    private int currentFontSize = 10, fontScale = 10000;
    private static Transform mainCamTransform = null;

    //Rings
    public GameObject rings { get => transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject; }

    //Atmosphere
    public GameObject atmosphere { get => transform.parent.GetChild(transform.GetSiblingIndex() + 2).gameObject; }

    //Resources
    public float creditsPerTurn
    {
        get
        {
            float credits = 0.0f;

            foreach (GalaxyCity galaxyCity in cities)
            {
                credits += galaxyCity.GetCreditsPerTurn(ownerIDVar);
            }

            return credits;
        }
    }

    public float prescriptionsPerTurn
    {
        get
        {
            float prescriptions = 0.0f;

            foreach (GalaxyCity galaxyCity in cities)
            {
                prescriptions += galaxyCity.GetPrescriptionsPerTurn(ownerIDVar);
            }

            return prescriptions;
        }
    }

    public float sciencePerTurn
    {
        get
        {
            float science = 0.0f;

            foreach (GalaxyCity galaxyCity in cities)
            {
                science += galaxyCity.GetSciencePerTurn(ownerIDVar);
            }

            return science;
        }
    }

    public float productionPerTurn
    {
        get
        {
            float production = 0.0f;

            foreach (GalaxyCity galaxyCity in cities)
            {
                production += galaxyCity.GetProductionPerTurn(ownerIDVar);
            }

            return production;
        }
    }

    private void Start()
    {
        AddArmy(new GalaxyArmy("Army of the South", ownerIDVar));
        armies[0].AddSquad(new GalaxySquad(owner.randomValidSquadName));
        armies[0].GetSquadAt(0).AddPill(new GalaxyPill("Bob", "Assault"));
        armies[0].GetSquadAt(0).AddPill(new GalaxyPill("Kevin", "Riot"));
        armies[0].AddSquad(new GalaxySquad(owner.randomValidSquadName));
        AddArmy(new GalaxyArmy("Army of the West", ownerIDVar));
        armies[1].AddSquad(new GalaxySquad(owner.randomValidSquadName));
        armies[1].GetSquadAt(0).AddPill(new GalaxyPill("Bob", "Officer"));
        armies[1].GetSquadAt(0).AddPill(new GalaxyPill("Kevin", "Flamethrower"));
        armies[1].AddSquad(new GalaxySquad(owner.randomValidSquadName));
    }

    public void InitializePlanet (string planetName, int planetID, Planet.Biome biome)
    {
        //Sets the name of the planet.
        planetNameVar = planetName;
        AddNameLabel(planetName);
        //Sets the planet's ID.
        this.planetIDVar = planetID;
        //Sets the planet's biome.
        this.biomeVar = biome;
    }

    public void GenerateCities(bool isCapital)
    {
        int totalBuildings = isCapital ? UnityEngine.Random.Range(15, 20) : UnityEngine.Random.Range(7, 14);

        int minimumBuildingsPerCity = 3;

        while(totalBuildings > 0)
        {
            GalaxyCity galaxyCity = new GalaxyCity();

            galaxyCity.buildingLimit = UnityEngine.Random.Range(minimumBuildingsPerCity, 7);
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
                galaxyCity.cityName = CityGenerator.GenerateCityName(biomeVar, galaxyCity.buildingLimit * 15);

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
        transform.parent.name = planetName;

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

        //Disable raycast target in order to be able to press 3D objects below it
        nameLabel.raycastTarget = false;
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
        transform.localEulerAngles += new Vector3(0, 0, rotationSpeed * Time.deltaTime);
    }

    public void ConquerPlanet(int conquerorID)
    {
        //Updates the capital planet of the former owner.
        if (isCapital)
            owner.PickNewCapital();

        //Removes the planet from the previous owner's list of owned planets.
        if(ownerIDVar != -1)
        {
            for(int x = 0; x < Empire.empires[ownerIDVar].planetsOwned.Count; x++)
            {
                if(Empire.empires[ownerIDVar].planetsOwned[x] == planetIDVar)
                {
                    Empire.empires[ownerIDVar].planetsOwned.RemoveAt(x);
                    break;
                }
            }
        }

        //Adds the planet to the new owner's list of owned planets.
        Empire.empires[conquerorID].planetsOwned.Add(planetIDVar);

        //Updates the color of the planet label.
        nameLabel.color = Empire.empires[conquerorID].labelColor;

        //Updates the material of the planet ships to match the color of the conquering empire's ships and enables if the conquering empire is the player and diables them if not.
        foreach(PlanetShip planetShip in planetShips)
        {
            planetShip.SharedMaterial = GalaxyManager.empireMaterials[(int)Empire.empires[conquerorID].empireCulture];
            planetShip.gameObject.SetActive(conquerorID == GalaxyManager.PlayerID);
        }

        //Sets the planet's owner id as the conqueror's id.
        ownerIDVar = conquerorID;

        //Updates the coloring of every hyperspace lane in the galaxy (updates every one just to be safe).
        HyperspaceLanesManager.hyperspaceLanesManager.UpdateHyperspaceLaneColoring();
    }

    private void OnMouseUpAsButton ()
    {
        if(ownerIDVar == GalaxyManager.PlayerID && !GalaxyCamera.IsMouseOverUIElement)
        {
            //Tells the planet management menu to display the information from this planet.
            PlanetManagementMenu.planetManagementMenu.PlanetSelected = this;
            
            //Closes the planet management menu if it is already open, this is purely just to reset it before we immediately reopen it.
            if (PlanetManagementMenu.planetManagementMenu.gameObject.activeInHierarchy)
            {
                PlanetManagementMenu.planetManagementMenu.Close();
            }

            //Opens the planet management menu.
            PlanetManagementMenu.planetManagementMenu.Open();
        }
    }

    public void EndTurn()
    {
        foreach(GalaxyCity city in cities)
        {
            //Adds each city's resources per turn to the empire.
            Empire.empires[ownerIDVar].Credits += city.GetCreditsPerTurn(ownerIDVar);
            Empire.empires[ownerIDVar].prescriptions += city.GetPrescriptionsPerTurn(ownerIDVar);
            Empire.empires[ownerIDVar].science += city.GetSciencePerTurn(ownerIDVar);

            //Progresses the building queue.
            city.buildingQueue.EndTurn(city.GetProductionPerTurn(ownerIDVar), city);
        }
    }

    /// <summary>
    /// Returns the army at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GalaxyArmy GetArmyAt(int index)
    {
        return armies[index];
    }

    /// <summary>
    /// Adds the specified army to the list of armies that are located on the planet and creates the planet ship to represent said army.
    /// </summary>
    /// <param name="army"></param>
    public void AddArmy(GalaxyArmy army)
    {
        //Adds the specified army to the list of armies located on the planet.
        armies.Add(army);
        //Creates the planet ship to represent the newly added army.
        CreateNewPlanetShip();
    }

    /// <summary>
    /// This method should be called whenever an army is added to the list of armies located on the planet and will create a new planet ship and put it in the appropriate position.
    /// </summary>
    private void CreateNewPlanetShip()
    {
        //Creates the planet ship.
        GameObject newShip = Instantiate(PlanetShip.planetShipPrefab);
        //Sets the parent of the planet ship.
        newShip.transform.SetParent(PlanetShip.planetShipParent);
        //Gets the planet ship script component in order to edit some variables of the planet ship.
        PlanetShip newShipScript = newShip.GetComponent<PlanetShip>();
        //Sets the location of the planet ship based off of the planet id and army index.
        newShipScript.SetLocation(planetIDVar, armies.Count - 1);
        //Sets the material of the planet ship in order for it to represent the color of the empire that owns the army and the planet.
        newShipScript.SharedMaterial = GalaxyManager.empireMaterials[(int)Empire.empires[ownerIDVar].empireCulture];
        //Adds the newly created planet ship to the list of planet ships that belong to the planet.
        planetShips.Add(newShipScript);
    }

    /// <summary>
    /// Removes the army at the specified index from the list of armies that are located on the planet.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveArmyAt(int index)
    {
        //Removes the specified army from the list of armies.
        armies.RemoveAt(index);
        //Removes the planet ship that represents the specified army from the list of planet ships and destroys it.
        PlanetShip planetShip = planetShips[index];
        planetShips.RemoveAt(index);
        planetShip.DestroyPlanetShip();
        //Updates the position of the planet ships that come after the one that was just removed.
        for(int planetShipIndex = index; planetShipIndex < planetShips.Count; planetShipIndex++)
        {
            planetShips[planetShipIndex].SetLocation(planetShipIndex);
        }
    }

    /// <summary>
    /// Removes the specified army from the list of armies located on the planet.
    /// </summary>
    /// <param name="army"></param>
    public void RemoveArmy(GalaxyArmy army)
    {
        bool armyIndexFound = false;
        for(int armyIndex = 0; armyIndex < armies.Count; armyIndex++)
        {
            if(!armyIndexFound && armies[armyIndex] == army)
            {
                //Removes the specified army from the list of armies.
                armies.RemoveAt(armyIndex);
                //Removes the planet ship that represents the specified army from the list of planet ships and destroys it.
                PlanetShip planetShip = planetShips[armyIndex];
                planetShips.RemoveAt(armyIndex);
                planetShip.DestroyPlanetShip();

                //Indicates that the specified army's index has been found.
                armyIndexFound = true;
                armyIndex--;
            }
            else if (armyIndexFound)
            {
                planetShips[armyIndex].SetLocation(armyIndex);
            }
        }
    }

    /// <summary>
    /// Modifies the list of armies and planet ships located on the planet to have one item move from one index to another (Ex: Army at index 1 changed to be located 3).
    /// </summary>
    /// <param name="originalIndex"></param>
    /// <param name="newIndex"></param>
    public void ChangeArmyIndex(int originalIndex, int newIndex)
    {
        //Ensures that valid indexes have been provided.
        if(originalIndex >= 0 && originalIndex < armies.Count && newIndex >= 0 && newIndex < armies.Count)
        {
            //Removes the army at the specified original index from the list of armies and inserts it at the specified new index.
            GalaxyArmy army = armies[originalIndex];
            armies.RemoveAt(originalIndex);
            armies.Insert(newIndex, army);
            //Removes the planet ship at the specified original index from the list of planet ships and inserts it at the specified new index.
            PlanetShip planetShip = planetShips[originalIndex];
            planetShips.RemoveAt(originalIndex);
            planetShips.Insert(newIndex, planetShip);
            //Sets the location of all of the planet ships.
            for(int planetShipIndex = 0; planetShipIndex < planetShips.Count; planetShipIndex++)
            {
                planetShips[planetShipIndex].SetLocation(planetShipIndex);
            }
        }
    }

    /// <summary>
    /// Changes the planet's material and updates the string that indicates what the name is of the applied material.
    /// </summary>
    /// <param name="materialName"></param>
    public void ChangeMaterial(string materialName)
    {
        GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + materialName);
        atmosphere.GetComponent<Renderer>().material = Resources.Load<Material>("Galaxy/Planet Materials/" + materialName + " Atmosphere");
        this.materialName = materialName;
    }
}






public struct GalaxyBuilding
{
    public enum BuildingType
    {
        ResearchFacility,
        Depot,
        Prescriptor,
        TradePost
    }
    public static int buildingTypeCount { get => Enum.GetNames(typeof(BuildingType)).Length; }

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
        if(Empire.empires[ownerID].Credits >= GalaxyBuilding.GetCreditsCost(buildingType) && buildingsCompleted.Count + buildingQueue.buildingsQueued.Count < buildingLimit)
        {
            //Subtracts the appropriate amount of credits from the empire based on the building type.
            Empire.empires[ownerID].Credits -= GalaxyBuilding.GetCreditsCost(buildingType);

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
