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

    /// <summary>
    /// Public property that should be used in order to access the description of the building's building type and know what it does.
    /// </summary>
    public string buildingTypeDescription
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return "Science focused building that outputs +0.25 science per turn per building level to the owning empire. Useful to upgrade if attempting to tech up faster and get a technological advantage over the other empires.";
                case BuildingType.TradePost:
                    return "Credits focused building that outputs +0.25 credits per turn per building level to the owning empire. Useful to upgrade in order to properly fund your empire while you create and maintain armies that require pay.";
                case BuildingType.Prescriptor:
                    return "Prescriptions focused building that ouputs +0.25 prescriptions per turn per building level to the owning empire. Useful to upgrade in order to be able to create and sustain armies for conquest and defense, as the prescriptions resource effectively functions as manpower/pillpower.";
                case BuildingType.Depot:
                    return "Production and vehicles focused building that outputs +0.25 production per turn per building level to the owning planet as well as allowing for more vehicles to be spawned in on the planet view for the defenders. Useful to upgrade in order to be able to upgrade other buildings more quickly and strengthen the planet's defense by giving the defenders more vehicles to work with.";
                default:
                    return "Description for building type \"" + GeneralHelperMethods.GetEnumText(buildingType.ToString()) + "\" has not been written as of yet.";
            }
        }
    }

    /// <summary>
    /// Public property that should be accessed in order to obtain the sprite that indicates a building of the building's building type. Sprite is loaded in from the project resources.
    /// </summary>
    public Sprite buildingTypeIconSprite { get => Resources.Load<Sprite>(buildingTypeIconsFolderPath + "/" + GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " Icon"); }

    /// <summary>
    /// Private static property that should be accessed in order to obtain the string that indicates the path of the resources folder that contains all of the building type icons.
    /// </summary>
    private static string buildingTypeIconsFolderPath { get => "Galaxy/Icons/Building Type Icons"; }

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