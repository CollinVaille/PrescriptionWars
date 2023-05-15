using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyCity
{
    /// <summary>
    /// Private variable that holds the name of the city, its value should be accessed mutated via the public property.
    /// </summary>
    private string _name = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the city.
    /// </summary>
    public string name
    {
        get => _name;
        set
        {
            _name = value;
        }
    }

    /// <summary>
    /// Private list that contains all of the buildings on the planet.
    /// </summary>
    private List<NewGalaxyBuilding> _buildings = null;
    /// <summary>
    /// Public property that should be used in order to access the list that contains all of the buildings on the planet.
    /// </summary>
    public List<NewGalaxyBuilding> buildings
    {
        get => _buildings;
    }

    public NewGalaxyCity(GalaxyCityData cityData)
    {
        _name = cityData.name;

        _buildings = new List<NewGalaxyBuilding>();
        foreach (GalaxyBuildingData buildingData in cityData.buildings)
            _buildings.Add(new NewGalaxyBuilding(buildingData));
    }

    public NewGalaxyCity(string name, List<NewGalaxyBuilding> buildings)
    {
        _name = name;

        _buildings = buildings;
    }
}

[System.Serializable]
public class GalaxyCityData
{
    public string name;

    public List<GalaxyBuildingData> buildings = null;

    public GalaxyCityData(NewGalaxyCity city)
    {
        name = city.name;

        buildings = new List<GalaxyBuildingData>();
        foreach (NewGalaxyBuilding building in city.buildings)
            buildings.Add(new GalaxyBuildingData(building));
    }
}