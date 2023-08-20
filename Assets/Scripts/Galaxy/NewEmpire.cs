using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEmpire
{
    public enum Culture
    {
        Red,
        Green,
        Blue,
        Purple,
        Gold,
        Silver
    }

    /// <summary>
    /// Private variable used to hold the name of the empire, but the name should ideally be accessed and mutated through the public property.
    /// </summary>
    private string nameVar = null;
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the empire.
    /// </summary>
    public string name { get => nameVar; set { nameVar = value; if (isPlayerEmpire) NewResourceBar.UpdateEmpireNameTooltip(); } }

    /// <summary>
    /// Private variable used to hold the culture of the empire, but the culture should ideally be accessed and mutated through the public property.
    /// </summary>
    private Culture cultureVar;
    /// <summary>
    /// Public property that should be used both to access and mutate the culture of the empire.
    /// </summary>
    public Culture culture { get => cultureVar; set => cultureVar = value; }

    /// <summary>
    /// Public static property that returns an int that indicates the total amount of valid cultures that an empire could possibly be.
    /// </summary>
    public static int cultureCount { get => Enum.GetNames(typeof(Culture)).Length; }

    /// <summary>
    /// Private variable that holds the color of the empire.
    /// </summary>
    private Color colorVar = Color.white;
    /// <summary>
    /// Public property that should be used to access the color of the empire.
    /// </summary>
    public Color color { get => colorVar; }
    /// <summary>
    /// Public property that should be used to access the label color of the empire which is a potentially brighter version of the empire's color that looks prettier for labels.
    /// </summary>
    public Color labelColor
    {
        get
        {
            Color labelColor = colorVar;

            if (culture == Culture.Red || culture == Culture.Green || culture == Culture.Blue)
            {
                labelColor.r += 0.35f;
                labelColor.g += 0.35f;
                labelColor.b += 0.35f;
            }

            return labelColor;
        }
    }

    /// <summary>
    /// Private variable that holds the flag of the empire.
    /// </summary>
    private NewFlag flagVar = null;
    /// <summary>
    /// Public property that should be used in order to access the flag of the empire.
    /// </summary>
    public NewFlag flag { get => flagVar; }

    /// <summary>
    /// Private variable used to hold the id (owned index in the list of empires) of the empire.
    /// </summary>
    private int IDVar = -1;
    /// <summary>
    /// Public property that should be used to access the id (owned index in the list of empires) of the empire.
    /// </summary>
    public int ID { get => IDVar; }

    /// <summary>
    /// Public property that should be used in order to access a bool that indicates whether or not the empire is the player's empire.
    /// </summary>
    public bool isPlayerEmpire { get => ID == NewGalaxyManager.playerID; }

    /// <summary>
    /// Private variable that holds the id (owned index in the list of solar systems in the galaxy manager) of every solar system that the empire owns.
    /// </summary>
    private List<int> solarSystemIDsVar = null;
    /// <summary>
    /// Public property that should be used to access the id (owned index in the list of solar systems in the galaxy manager) of every solar system that the empire owns.
    /// </summary>
    public List<int> solarSystemIDs { get => solarSystemIDsVar; }
    /// <summary>
    /// Public property that should be used to access the script of every solar system that the empire owns.
    /// </summary>
    public List<GalaxySolarSystem> solarSystems
    {
        get
        {
            if (solarSystemIDs == null)
                return null;
            List<GalaxySolarSystem> solarSystemScripts = new List<GalaxySolarSystem>();
            for(int solarSystemIDIndex = 0; solarSystemIDIndex < solarSystemIDs.Count; solarSystemIDIndex++)
                solarSystemScripts.Add(NewGalaxyManager.solarSystems[solarSystemIDs[solarSystemIDIndex]]);
            return solarSystemScripts;
        }
    }

    /// <summary>
    /// Private variable that holds the id of every planet that the empire controls.
    /// </summary>
    private List<int> planetIDsVar = null;
    /// <summary>
    /// Public property that should be used to access the id of every planet that the empire controls.
    /// </summary>
    public List<int> planetIDs { get => planetIDsVar; }

    /// <summary>
    /// Private variable that holds the id of the solar system that serves as the empire's capital system.
    /// </summary>
    private int _capitalSystemID = -1;
    /// <summary>
    /// Public property that should be used both to access and mutate the capital system of the empire via system ID.
    /// </summary>
    public int capitalSystemID
    {
        get => _capitalSystemID;
        set
        {
            //Logs a warning and returns if the empire does not own the specified system.
            if (!solarSystemIDs.Contains(value))
            {
                Debug.LogWarning("Cannot set the specified system as the capital system of the empire because the specified system is not under the empire's control.");
                return;
            }
            //Stores the system ID of the previous capital system.
            int previousCapitalSystemID = _capitalSystemID;
            //Changes the capital system ID to the specified value.
            _capitalSystemID = value;
            //Informs the previous capital system that it is no longer the capital system of the empire.
            NewGalaxyManager.solarSystems[previousCapitalSystemID].OnBecameNoncapitalSystem();
            //Informs the new capital system that it is now the capital system of the empire.
            NewGalaxyManager.solarSystems[value].OnBecameCapitalSystem();
        }
    }

    /// <summary>
    /// Private variable that holds the amount of credits (currency) that the empire has.
    /// </summary>
    private float _credits = 0;
    /// <summary>
    /// Public property that should be used both to access and mutate the amount of credits that the empire has. The setter also updates the resource bar if the empire is the player empire.
    /// </summary>
    public float credits
    {
        get => _credits;
        set
        {
            //Sets the empire's amount of credits to the specified amount.
            _credits = value;
            //Updates the number of credits displayed on the resource bar if the empire is the player empire.
            if (isPlayerEmpire)
                NewResourceBar.UpdateCredits();
        }
    }

    /// <summary>
    /// Private variable that holds the amount of prescriptions that the empire has.
    /// </summary>
    private float _prescriptions = 0;
    /// <summary>
    /// Public property that should be used both to access and mutate the amount of prescriptions that the empire has. The setter also updates the resource bar if the empire is the player empire.
    /// </summary>
    public float prescriptions
    {
        get => _prescriptions;
        set
        {
            //Sets the empire's amount of prescriptions to the specified amount.
            _prescriptions = value;
            //Updates the number of prescriptions displayed on the resource bar if the empire is the player empire.
            if (isPlayerEmpire)
                NewResourceBar.UpdatePrescriptions();
        }
    }

    /// <summary>
    /// Private holder variable for the dictionary that contains the IDs of all the resource modifiers that apply to the empire organized by resource type and mathematical operation type.
    /// </summary>
    private Dictionary<GalaxyResourceType, Dictionary<GalaxyResourceModifier.MathematicalOperation, List<GalaxyResourceModifier>>> _resourceModifiers = null;
    /// <summary>
    /// Public property that should be used in order to access the dictionary that contains the IDs of all the resource modifiers that apply to the empire organized by resource type and mathematical operation type.
    /// </summary>
    public Dictionary<GalaxyResourceType, Dictionary<GalaxyResourceModifier.MathematicalOperation, List<GalaxyResourceModifier>>> resourceModifiers { get => _resourceModifiers; }

    /// <summary>
    /// Private holder variable for the dictionary that contains the float values representing the empire's income per turn for each resource type.
    /// </summary>
    private Dictionary<GalaxyResourceType, float> resourcesPerTurn = null;

    /// <summary>
    /// Public property that should be used in order to access the list of pill classes that can be applied to pills belonging to the empire.
    /// </summary>
    public List<NewGalaxyPillClass> pillClasses { get => _pillClasses; }
    /// <summary>
    /// Private list that holds the pill classes that can be applied to pills belonging to the empire.
    /// </summary>
    private List<NewGalaxyPillClass> _pillClasses = null;

    /// <summary>
    /// Public property that should be used in order to access the list of string values that represent the names of skin materials that can be applied to pills belonging to this empire.
    /// </summary>
    public List<string> pillSkinNames { get => _pillSkinNames; }
    /// <summary>
    /// Private holder variable for the list of string values that represent the names of skin materials that can be applied to pills belonging to this empire.
    /// </summary>
    private List<string> _pillSkinNames = null;

    /// <summary>
    /// Public property that should be used in order to access the manager that manages all of the armies that belong to the empire.
    /// </summary>
    public EmpireArmiesManager armiesManager { get; private set; } = null;

    public NewEmpire(EmpireData empireData)
    {
        InitializeResourcesPerTurnDictionary();
        InitializeResourceModifiersDictionary();

        nameVar = empireData.name;
        cultureVar = empireData.culture;
        colorVar = new Color(empireData.color[0], empireData.color[1], empireData.color[2], empireData.color[3]);
        flagVar = new NewFlag(empireData.flag);
        IDVar = empireData.ID;
        _capitalSystemID = empireData.capitalSystemID;
        solarSystemIDsVar = empireData.solarSystemIDs;
        planetIDsVar = empireData.planetIDs;
        _credits = empireData.credits;
        _prescriptions = empireData.prescriptions;

        _pillSkinNames = empireData.pillSkinNames;
        _pillClasses = new List<NewGalaxyPillClass>();
        foreach (NewGalaxyPillClassData pillClassData in empireData.pillClasses)
            _pillClasses.Add(new NewGalaxyPillClass(this, pillClassData));

        armiesManager = new EmpireArmiesManager(this, empireData.armiesManager);
    }

    public NewEmpire(string name, Culture culture, Color color, NewFlag flag, int ID, int capitalSystemID, List<int> solarSystemIDs, List<int> planetIDs, float credits, float prescriptions, List<NewGalaxyPillClass> pillClasses, List<NewGalaxyArmy> armies = null)
    {
        InitializeResourcesPerTurnDictionary();
        InitializeResourceModifiersDictionary();

        nameVar = name;
        cultureVar = culture;
        colorVar = color;
        flagVar = flag;
        IDVar = ID;
        _capitalSystemID = capitalSystemID;
        solarSystemIDsVar = solarSystemIDs;
        planetIDsVar = planetIDs;
        _credits = credits;
        _prescriptions = prescriptions;

        _pillSkinNames = new List<string>();
        foreach (Material material in Resources.LoadAll<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(culture.ToString())))
            _pillSkinNames.Add(material.name);
        _pillClasses = pillClasses == null ? new List<NewGalaxyPillClass>() : pillClasses;

        armiesManager = new EmpireArmiesManager(this, armies);
    }

    /// <summary>
    /// Private method that should be called by the constructors in order to initialize the resource modifiers dictionary.
    /// </summary>
    private void InitializeResourceModifiersDictionary()
    {
        _resourceModifiers = new Dictionary<GalaxyResourceType, Dictionary<GalaxyResourceModifier.MathematicalOperation, List<GalaxyResourceModifier>>>();
        for(int resourceTypeIndex = 0; resourceTypeIndex < Enum.GetNames(typeof(GalaxyResourceType)).Length; resourceTypeIndex++)
        {
            _resourceModifiers.Add((GalaxyResourceType)resourceTypeIndex, new Dictionary<GalaxyResourceModifier.MathematicalOperation, List<GalaxyResourceModifier>>());
            for(int mathematicalOperationTypeIndex = 0; mathematicalOperationTypeIndex < Enum.GetNames(typeof(GalaxyResourceModifier.MathematicalOperation)).Length; mathematicalOperationTypeIndex++)
            {
                _resourceModifiers[(GalaxyResourceType)resourceTypeIndex].Add((GalaxyResourceModifier.MathematicalOperation)mathematicalOperationTypeIndex, new List<GalaxyResourceModifier>());
            }
        }
    }

    /// <summary>
    /// Private method that should be called by the constructors in order to initialize the resources per turn dictionary.
    /// </summary>
    private void InitializeResourcesPerTurnDictionary()
    {
        resourcesPerTurn = new Dictionary<GalaxyResourceType, float>();
        for(int resourceTypeIndex = 0; resourceTypeIndex < Enum.GetNames(typeof(GalaxyResourceType)).Length; resourceTypeIndex++)
        {
            resourcesPerTurn.Add((GalaxyResourceType)resourceTypeIndex, 0);
        }
    }

    /// <summary>
    /// Public method that should be called by a galaxy resource modifier in order to recalculate the resource per turn for a specified resource type.
    /// </summary>
    /// <param name="resourceType"></param>
    public void UpdateResourcePerTurnForResourceType(GalaxyResourceType resourceType)
    {
        //Calculates the sum of all addition resource modifiers affecting the empire for the specified resource type.
        float additionSum = 0;
        foreach(GalaxyResourceModifier resourceModifier in resourceModifiers[resourceType][GalaxyResourceModifier.MathematicalOperation.Addition])
            additionSum += resourceModifier.amount;
        //Calculates the sum of all additive multiplication modifiers affecting the empire for the specified resource type.
        float additiveMultiplicationSum = 1;
        foreach (GalaxyResourceModifier resourceModifier in resourceModifiers[resourceType][GalaxyResourceModifier.MathematicalOperation.AdditiveMultiplication])
            additiveMultiplicationSum += resourceModifier.amount;
        //Calculates the product of all multiplicative modifiers affecting the empire for the specified resource type.
        float multiplicativeMultiplicationProduct = 1;
        foreach (GalaxyResourceModifier resourceModifier in resourceModifiers[resourceType][GalaxyResourceModifier.MathematicalOperation.MultiplicativeMultiplication])
            multiplicativeMultiplicationProduct *= resourceModifier.amount;

        //Sets the resources per turn for the specified resource based on the just calculated values.
        resourcesPerTurn[resourceType] = additionSum * additiveMultiplicationSum * multiplicativeMultiplicationProduct;

        //Updates the resource bar to reflect any changes in regard to resources.
        NewResourceBar.UpdateAllResourceDependentComponents();
    }

    /// <summary>
    /// Public method that should be used in order to access the amount that the empire has in income per turn of the specified resource type.
    /// </summary>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    public float GetResourcePerTurn(GalaxyResourceType resourceType)
    {
        return resourcesPerTurn[resourceType];
    }

    /// <summary>
    /// Public method that should be called in order to add a planet to the empire's control via the planet's ID.
    /// </summary>
    /// <param name="planetID"></param>
    public void AddPlanet(int planetID)
    {
        //Logs a warning and returns if an invalid planet ID is specified.
        if(planetID < 0 || planetID >= NewGalaxyManager.planets.Count)
        {
            Debug.LogWarning("Cannot add planet with ID: " + planetID + " to empire with ID: " + ID + "'s control, invalid planet ID given.");
            return;
        }
        //Adds the planet ID to the list of planet IDs of planets that are controlled by the empire.
        planetIDsVar.Add(planetID);
        //Informs the planet itself of the owner change if it is unaware.
        if (NewGalaxyManager.planets[planetID].ownerID != ID)
            NewGalaxyManager.planets[planetID].owner = this;
    }

    /// <summary>
    /// Public method that should be called in order to add a planet to the empire's control via an object reference.
    /// </summary>
    /// <param name="planet"></param>
    public void AddPlanet(NewGalaxyPlanet planet)
    {
        //Logs a warning and returns if a null planet reference is specified.
        if(planet == null)
        {
            Debug.Log("Cannot add a null planet to empire of ID: " + ID + "'s control.");
            return;
        }
        //Adds the planet to the list of planets under the empire's control via the ID of the specified planet.
        AddPlanet(planet.ID);
    }

    /// <summary>
    /// Public method that should be called in order to remove a planet from the empire's control via the planet's ID.
    /// </summary>
    /// <param name="planetID"></param>
    public void RemovePlanet(int planetID)
    {
        //Logs a warning and returns if an invalid planet ID is specified.
        if (planetID < 0 || planetID >= NewGalaxyManager.planets.Count)
        {
            Debug.LogWarning("Cannot remove planet with ID: " + planetID + " to empire with ID: " + ID + "'s control, invalid planet ID given.");
            return;
        }
        //Logs a warning and returns if the empire does not own the specified planet.
        if (!planetIDsVar.Contains(planetID))
        {
            Debug.LogWarning("Cannot remove planet with ID: " + planetID + " from empire with ID: " + ID + "'s control, the empire does not control the specified planet.");
            return;
        }
        //Removes the planet from the list of planets under the empire's control.
        planetIDsVar.Remove(planetID);
        //Informs the planet itself of the owner change if it is unaware.
        if (NewGalaxyManager.planets[planetID].ownerID == ID)
            NewGalaxyManager.planets[planetID].owner = null;
    }

    /// <summary>
    /// Public method that should be called in order to remove a planet from the empire's control via an object reference.
    /// </summary>
    /// <param name="planet"></param>
    public void RemovePlanet(NewGalaxyPlanet planet)
    {
        //Logs a warning and returns if a null planet reference is specified.
        if (planet == null)
        {
            Debug.Log("Cannot remove a null planet from empire of ID: " + ID + "'s control.");
            return;
        }
        //Removes the planet from the list of planets under the empire's control via the ID of the specified planet.
        RemovePlanet(planet.ID);
    }

    /// <summary>
    /// Public method that is called by the galaxy manager on the final call of the EndTurnUpdate method for ending a turn.
    /// </summary>
    public void OnEndTurnFinalUpdate()
    {
        credits += GetResourcePerTurn(GalaxyResourceType.Credits);

        prescriptions += GetResourcePerTurn(GalaxyResourceType.Prescriptions);

        float production = GetResourcePerTurn(GalaxyResourceType.Production);
        List<NewGalaxyBuilding> upgradingBuildings = new List<NewGalaxyBuilding>();
        foreach(int planetID in planetIDs)
            foreach(NewGalaxyBuilding building in NewGalaxyManager.planets[planetID].city.buildings)
                if (building.upgrading)
                    upgradingBuildings.Add(building);
        foreach (int planetID in planetIDs)
            foreach (NewGalaxyBuilding building in NewGalaxyManager.planets[planetID].city.buildings)
                building.OnEndTurnFinalUpdate(building.upgrading ? production / upgradingBuildings.Count : 0);
    }
}

[System.Serializable]
public class EmpireData
{
    public string name;
    public NewEmpire.Culture culture;
    public float[] color;
    public FlagData flag;
    public int ID;
    public int capitalSystemID;
    public List<int> solarSystemIDs = null;
    public List<int> planetIDs = null;
    public float credits;
    public float prescriptions;
    public List<string> pillSkinNames = null;
    public List<NewGalaxyPillClassData> pillClasses = null;
    public EmpireArmiesManagerData armiesManager = null;

    public EmpireData(NewEmpire empire)
    {
        name = empire.name;
        culture = empire.culture;
        color = new float[4] { empire.color.r, empire.color.g, empire.color.b, empire.color.a };
        flag = new FlagData(empire.flag);
        ID = empire.ID;
        capitalSystemID = empire.capitalSystemID;
        solarSystemIDs = empire.solarSystemIDs;
        planetIDs = empire.planetIDs;
        credits = empire.credits;
        prescriptions = empire.prescriptions;
        pillSkinNames = empire.pillSkinNames;
        pillClasses = new List<NewGalaxyPillClassData>();
        foreach (NewGalaxyPillClass pillClass in empire.pillClasses)
            pillClasses.Add(new NewGalaxyPillClassData(pillClass));
        armiesManager = new EmpireArmiesManagerData(empire.armiesManager);
    }
}