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
            if(resourceModifier != null)
                resourceModifier.amount = buildingTypeResourceModifierAmount;
        }
    }

    /// <summary>
    /// Private holder variable for the integer value that indicates the ID of the planet that the building's city is located on.
    /// </summary>
    private int _assignedPlanetID = -1;
    /// <summary>
    /// Public property that should be used in order to access the planet that the building's city is located on.
    /// </summary>
    public NewGalaxyPlanet assignedPlanet { get => _assignedPlanetID >= 0 && _assignedPlanetID < NewGalaxyManager.planets.Count ? NewGalaxyManager.planets[_assignedPlanetID] : null; }

    /// <summary>
    /// Private variable that holds an integer value that indicates the ID of the resource modifier that belongs to this building (-1 if no resource modifier belongs to this building).
    /// </summary>
    private int _resourceModifierID = -1;
    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates the ID of the resource modifier that belongs to this building (-1 if no resource modifier belongs to this building).
    /// </summary>
    public GalaxyResourceModifier resourceModifier { get => _resourceModifierID >= 0 && _resourceModifierID < NewGalaxyManager.resourceModifiersCount && NewGalaxyManager.resourceModifiers.ContainsKey(_resourceModifierID) ? NewGalaxyManager.resourceModifiers[_resourceModifierID] : null; }

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether or not the building is upgrading to the next level.
    /// </summary>
    private bool _upgrading = false;
    /// <summary>
    /// Public property that should be used in order to access and mutate the boolean value that indicates whether or not the building is upgrading to the next level.
    /// </summary>
    public bool upgrading
    {
        get => _upgrading;
        set
        {
            bool previouslyUpgrading = _upgrading;
            _upgrading = value;
            if (_upgrading && !previouslyUpgrading && assignedPlanet.owner != null)
                assignedPlanet.owner.credits -= upgradeCreditsCost;
            _productionTowardsUpgrading = 0;
        }
    }

    /// <summary>
    /// Private holder variable for the float value that indicates the amount of production that has accumulated through the turns in order to upgrade the building to the next level.
    /// </summary>
    private float _productionTowardsUpgrading = 0;
    /// <summary>
    /// Public property that should be used in order to access the float value that indicates the amount of production that has accumulated through the turns in order to upgrade the building to the next level.
    /// </summary>
    public float productionTowardsUpgrading { get => _productionTowardsUpgrading; }

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
    /// Public property that should be used in order to access the type of resource that the building's type of building produces.
    /// </summary>
    public GalaxyResourceType buildingTypeResourceType
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return GalaxyResourceType.Science;
                case BuildingType.TradePost:
                    return GalaxyResourceType.Credits;
                case BuildingType.Prescriptor:
                    return GalaxyResourceType.Prescriptions;
                case BuildingType.Depot:
                    return GalaxyResourceType.Production;
                default:
                    Debug.LogWarning("There is no assigned resource type for building type \"" + buildingType.ToString() + "\".");
                    return 0;
            }
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the type of mathematical operation that is performed by the resource modifier for the building's building type.
    /// </summary>
    public GalaxyResourceModifier.MathematicalOperation buildingTypeResourceModifierMathematicalOperation
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return GalaxyResourceModifier.MathematicalOperation.Addition;
                case BuildingType.TradePost:
                    return GalaxyResourceModifier.MathematicalOperation.Addition;
                case BuildingType.Prescriptor:
                    return GalaxyResourceModifier.MathematicalOperation.Addition;
                case BuildingType.Depot:
                    return GalaxyResourceModifier.MathematicalOperation.Addition;
                default:
                    Debug.LogWarning("There is no assigned resource modifier mathematical operation for building type \"" + buildingType.ToString() + "\".");
                    return 0;
            }
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the amount of the resource modifier for the building's building type also based on the building's level and possibly other factors.
    /// </summary>
    public float buildingTypeResourceModifierAmount
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return 0.25f * level;
                case BuildingType.TradePost:
                    return 0.25f * level;
                case BuildingType.Prescriptor:
                    return 0.25f * level;
                case BuildingType.Depot:
                    return 0.25f * level;
                default:
                    Debug.LogWarning("There is no assigned resource modifier amount for building type \"" + buildingType.ToString() + "\".");
                    return 0;
            }
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the production cost in order to upgrade the building to the next level.
    /// </summary>
    public float upgradeProductionCost
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return 100 + (25 * (level - 1));
                case BuildingType.TradePost:
                    return 100 + (25 * (level - 1));
                case BuildingType.Prescriptor:
                    return 100 + (25 * (level - 1));
                case BuildingType.Depot:
                    return 100 + (25 * (level - 1));
                default:
                    Debug.LogWarning("There is no assigned upgrade production cost for building type \"" + buildingType.ToString() + "\".");
                    return 0;
            }
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the credits cost in order to upgrade the building to the next level.
    /// </summary>
    public float upgradeCreditsCost
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.ResearchFacility:
                    return 25 + (5 * (level - 1));
                case BuildingType.TradePost:
                    return 25 + (5 * (level - 1));
                case BuildingType.Prescriptor:
                    return 25 + (5 * (level - 1));
                case BuildingType.Depot:
                    return 25 + (5 * (level - 1));
                default:
                    Debug.LogWarning("There is no assigned upgrade credits cost for building type \"" + buildingType.ToString() + "\".");
                    return 0;
            }
        }
    }
    /// <summary>
    /// Public property that should be used in order to access the notification data that should be used to create a new notification to inform the player of the building upgrading to the next level.
    /// </summary>
    public GalaxyNotificationData upgradedNotificationData { get => new GalaxyNotificationData(GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " Upgraded", GeneralHelperMethods.GetEnumText(buildingType.ToString()), true, false, new NewGalaxyPopupData(GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " Upgraded", GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " Constellation Background", "The " + GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " in " + assignedPlanet.city.name + ", " + assignedPlanet.planetName + " has finished upgrading from level " + (level - 1) + " to level " + level + ". This increases its output per turn of " + GeneralHelperMethods.GetEnumText(buildingTypeResourceType.ToString()) + " to " + buildingTypeResourceModifierAmount + ".", "hammering-2", new List<NewGalaxyPopupOptionData>() { new NewGalaxyPopupOptionData("Okay", "You acknowledge that the " + GeneralHelperMethods.GetEnumText(buildingType.ToString()) + " has finished upgrading to level " + level + ".", null, null) })); }

    /// <summary>
    /// Public property that should be accessed in order to obtain the sprite that indicates a building of the building's building type. Sprite is loaded in from the project resources.
    /// </summary>
    public Sprite buildingTypeSprite { get => Resources.Load<Sprite>(buildingTypeIconsFolderPath + "/" + GeneralHelperMethods.GetEnumText(buildingType.ToString())); }

    /// <summary>
    /// Private static property that should be accessed in order to obtain the string that indicates the path of the resources folder that contains all of the building type icons.
    /// </summary>
    private static string buildingTypeIconsFolderPath { get => "Galaxy/Building Type Sprites"; }

    public NewGalaxyBuilding(GalaxyBuildingData buildingData)
    {
        _buildingType = buildingData.buildingType;

        _level = buildingData.level;
        _assignedPlanetID = buildingData.assignedPlanetID;
        _resourceModifierID = buildingData.resourceModifierID;

        _upgrading = buildingData.upgrading;

        _productionTowardsUpgrading = buildingData.productionTowardsUpgrading;
    }

    public NewGalaxyBuilding(BuildingType buildingType, int assignedPlanetID, int level = 1)
    {
        _buildingType = buildingType;

        _level = level;
        _assignedPlanetID = assignedPlanetID;

        if (NewGalaxyManager.activeInHierarchy)
        {
            _resourceModifierID = (new GalaxyResourceModifier(buildingTypeResourceType, buildingTypeResourceModifierMathematicalOperation, buildingTypeResourceModifierAmount, assignedPlanet.owner)).ID;
        }
        else
        {
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 1);
        }

        _upgrading = false;

        _productionTowardsUpgrading = 0;
    }

    /// <summary>
    /// Private method that is executed by the galaxy generator whenever the galaxy is finished generating and the method has a priority index of 0 (meaning it is top priority and can be done immediately). The method properly sets the resource modifier ID integer value stored by the building if there is no valid current resource modifier.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        if(_resourceModifierID < 0)
        {
            _resourceModifierID = (new GalaxyResourceModifier(buildingTypeResourceType, buildingTypeResourceModifierMathematicalOperation, buildingTypeResourceModifierAmount, assignedPlanet.owner)).ID;
        }
    }

    /// <summary>
    /// Public method called by the owning empire right before the start of a new turn, progresses upgrading the building if applicable.
    /// </summary>
    public void OnEndTurnFinalUpdate(float production = 0)
    {
        if (upgrading)
        {
            _productionTowardsUpgrading += production;
            if (productionTowardsUpgrading >= upgradeProductionCost)
            {
                level++;
                upgrading = false;
                NewGalaxyManager.notificationManager.CreateNotification(upgradedNotificationData);
            }
        }
    }
}

[System.Serializable]
public class GalaxyBuildingData
{
    public NewGalaxyBuilding.BuildingType buildingType;

    public int level = 0;
    public int assignedPlanetID = -1;
    public int resourceModifierID = -1;

    public bool upgrading = false;

    public float productionTowardsUpgrading = 0;

    public GalaxyBuildingData(NewGalaxyBuilding building)
    {
        buildingType = building.buildingType;

        level = building.level;

        assignedPlanetID = building.assignedPlanet == null ? -1 : building.assignedPlanet.ID;

        resourceModifierID = building.resourceModifier == null ? -1 : building.resourceModifier.ID;

        upgrading = building.upgrading;

        productionTowardsUpgrading = building.productionTowardsUpgrading;
    }
}