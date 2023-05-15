using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyBuilding
{
    /// <summary>
    /// Public enum that indicates what type of building each building is.
    /// </summary>
    public enum BuildingType
    {
        /// <summary>
        /// Science building.
        /// </summary>
        ResearchFacility,
        /// <summary>
        /// Credits building.
        /// </summary>
        TradePost,
        /// <summary>
        /// Prescriptions building.
        /// </summary>
        Prescriptor,
        /// <summary>
        /// Production and vehicles building.
        /// </summary>
        Depot
    }

    /// <summary>
    /// Private variable that holds an enum value that indicates what type of building the building is.
    /// </summary>
    private BuildingType _buildingType;
    /// <summary>
    /// Public property that should be used to access the type of building that the building is.
    /// </summary>
    public BuildingType buildingType { get => _buildingType; }

    /// <summary>
    /// Private variable that holds an int that indicates the level of the building which affects how good it is and how much resource value it can produce.
    /// </summary>
    private int _level = 1;
    /// <summary>
    /// Public property that should be used both to access and mutate the level of the building which affects how good it is and how much resource value it can produce.
    /// </summary>
    public int level
    {
        get => _level;
        set
        {
            _level = value;
        }
    }

    public NewGalaxyBuilding(GalaxyBuildingData buildingData)
    {
        _buildingType = buildingData.buildingType;

        _level = buildingData.level;
    }

    public NewGalaxyBuilding(BuildingType buildingType, int level = 1)
    {
        _buildingType = buildingType;

        _level = level;
    }
}

[System.Serializable]
public class GalaxyBuildingData
{
    public NewGalaxyBuilding.BuildingType buildingType;

    public int level = 0;

    public GalaxyBuildingData(NewGalaxyBuilding building)
    {
        buildingType = building.buildingType;

        level = building.level;
    }
}