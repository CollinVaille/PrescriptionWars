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

    public float creditsPerTurn()
    {
        float credits = 0.0f;

        foreach(GalaxyCity galaxyCity in cities)
        {
            credits += galaxyCity.creditsPerTurn;
        }

        return credits;
    }

    public float prescriptionsPerTurn()
    {
        float prescriptions = 0.0f;

        foreach(GalaxyCity galaxyCity in cities)
        {
            prescriptions += galaxyCity.prescriptionsPerTurn;
        }

        return prescriptions;
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
        int numberOfCities = 2;

        if (isCapital)
            numberOfCities = 3;

        for(int x = 0; x < numberOfCities; x++)
        {
            GalaxyCity galaxyCity = new GalaxyCity();

            galaxyCity.cityName = "Test Name";
            galaxyCity.creditsPerTurn = 1.0f;
            galaxyCity.prescriptionsPerTurn = 1.0f;

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
    }

    public void SetPlanetOwner(int ownerID)
    {
        this.ownerID = ownerID;
        nameLabel.color = Empire.empires[this.ownerID].GetLabelColor();
    }

    private void OnMouseUp ()
    {
        if(ownerID == GalaxyManager.playerID && !GalaxyManager.planetManagementMenu.activeInHierarchy)
        {
            PlanetManagementMenu.planetSelected = transform.gameObject;
            GalaxyManager.togglePlanetManagementMenu = true;
        }
    }
}

public class GalaxyBuilding
{
    public enum BuildingType
    {
        ResearchFacility,
        Factory,
        Prescriptor,
        TradePost
    }

    public BuildingType type;
}

public class BuildingQueue
{
    public List<GalaxyBuilding> buildingsQueued = new List<GalaxyBuilding>();

    public List<string> GetQueueText()
    {
        List<string> queueText = new List<string>();

        foreach(GalaxyBuilding galaxyBuilding in buildingsQueued)
        {
            queueText.Add("" + galaxyBuilding.type);
        }

        return queueText;
    }
}

public class GalaxyCity
{
    //Buildings
    public List<GalaxyBuilding> buildings = new List<GalaxyBuilding>();
    public BuildingQueue buildingQueue = new BuildingQueue();

    //Information
    public string cityName;

    //Resources
    public float creditsPerTurn;
    public float prescriptionsPerTurn;

    public List<string> GetBuildingsListText()
    {
        List<string> buildingsListText = new List<string>();

        foreach (GalaxyBuilding galaxyBuilding in buildings)
        {
            buildingsListText.Add("" + galaxyBuilding.type);
        }

        return buildingsListText;
    }
}
