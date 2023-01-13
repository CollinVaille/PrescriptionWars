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
    public string name { get => nameVar; set => nameVar = value; }

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

    public NewEmpire(EmpireData empireData)
    {
        nameVar = empireData.name;
        cultureVar = empireData.culture;
        colorVar = new Color(empireData.color[0], empireData.color[1], empireData.color[2], empireData.color[3]);
        flagVar = new NewFlag(empireData.flag);
        IDVar = empireData.ID;
        solarSystemIDsVar = empireData.solarSystemIDs;
        planetIDsVar = empireData.planetIDs;
    }

    public NewEmpire(string name, Culture culture, Color color, NewFlag flag, int ID, List<int> solarSystemIDs, List<int> planetIDs)
    {
        nameVar = name;
        cultureVar = culture;
        colorVar = color;
        flagVar = flag;
        IDVar = ID;
        solarSystemIDsVar = solarSystemIDs;
        planetIDsVar = planetIDs;
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
}

[System.Serializable]
public class EmpireData
{
    public string name;
    public NewEmpire.Culture culture;
    public float[] color;
    public FlagData flag;
    public int ID;
    public List<int> solarSystemIDs = null;
    public List<int> planetIDs = null;

    public EmpireData(NewEmpire empire)
    {
        name = empire.name;
        culture = empire.culture;
        color = new float[4] { empire.color.r, empire.color.g, empire.color.b, empire.color.a };
        flag = new FlagData(empire.flag);
        ID = empire.ID;
        solarSystemIDs = empire.solarSystemIDs;
        planetIDs = empire.planetIDs;
    }
}