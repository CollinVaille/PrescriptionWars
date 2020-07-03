﻿using System.Collections;
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
    public float creditsPerTurn;
    public float prescriptionsPerTurn;
    Vector3 rotation;

    //Buildings
    public List<GalaxyBuilding> buildings = new List<GalaxyBuilding>();
    public BuildingQueue buildingQueue = new BuildingQueue();

    public List<string> GetBuildingsListText()
    {
        List<string> buildingsListText = new List<string>();

        foreach(GalaxyBuilding galaxyBuilding in buildings)
        {
            buildingsListText.Add("" + galaxyBuilding.type);
        }

        return buildingsListText;
    }

    public void InitializePlanet (string planetName)
    {
        AddNameLabel(planetName);

        //Amount the planet will rotate.
        rotation = new Vector3(0, 0, Random.Range(5, 21));

        //For testing purposes.
        /*int numberOfBuildings = 4;
        for(int x = 0; x < numberOfBuildings; x++)
        {
            GalaxyBuilding galaxyBuilding = new GalaxyBuilding();
            galaxyBuilding.type = GalaxyBuilding.BuildingType.ResearchFacility;
            buildings.Add(galaxyBuilding);
        }*/
        GalaxyBuilding galaxyBuilding = new GalaxyBuilding();
        galaxyBuilding.type = GalaxyBuilding.BuildingType.Factory;
        buildings.Add(galaxyBuilding);
        GalaxyBuilding galaxyBuilding1 = new GalaxyBuilding();
        galaxyBuilding1.type = GalaxyBuilding.BuildingType.Prescriptor;
        buildings.Add(galaxyBuilding1);
        GalaxyBuilding galaxyBuilding2 = new GalaxyBuilding();
        galaxyBuilding2.type = GalaxyBuilding.BuildingType.ResearchFacility;
        buildings.Add(galaxyBuilding2);
        GalaxyBuilding galaxyBuilding3 = new GalaxyBuilding();
        galaxyBuilding3.type = GalaxyBuilding.BuildingType.TradePost;
        buildings.Add(galaxyBuilding3);
        GalaxyBuilding galaxyBuilding4 = new GalaxyBuilding();
        galaxyBuilding4.type = GalaxyBuilding.BuildingType.Factory;
        buildings.Add(galaxyBuilding4);
        GalaxyBuilding galaxyBuilding5 = new GalaxyBuilding();
        galaxyBuilding5.type = GalaxyBuilding.BuildingType.Prescriptor;
        buildings.Add(galaxyBuilding5);
        GalaxyBuilding galaxyBuilding6 = new GalaxyBuilding();
        galaxyBuilding6.type = GalaxyBuilding.BuildingType.ResearchFacility;
        buildings.Add(galaxyBuilding6);
        GalaxyBuilding galaxyBuilding7 = new GalaxyBuilding();
        galaxyBuilding7.type = GalaxyBuilding.BuildingType.TradePost;
        buildings.Add(galaxyBuilding7);
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
