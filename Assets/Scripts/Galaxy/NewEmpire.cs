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
                labelColor.r += 0.3f;
                labelColor.g += 0.3f;
                labelColor.b += 0.3f;
            }

            return labelColor;
        }
    }

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
        IDVar = empireData.ID;
        solarSystemIDsVar = empireData.solarSystemIDs;
        planetIDsVar = empireData.planetIDs;
    }

    public NewEmpire(string name, Culture culture, int ID, List<int> solarSystemIDs, List<int> planetIDs)
    {
        nameVar = name;
        cultureVar = culture;
        colorVar = GetRandomColorBasedOnCulture(culture);
        IDVar = ID;
        solarSystemIDsVar = solarSystemIDs;
        planetIDsVar = planetIDs;
    }

    /// <summary>
    /// Public static function that returns a random empire color based on the specified empire culture.
    /// </summary>
    /// <param name="culture"></param>
    /// <returns></returns>
    private static Color GetRandomColorBasedOnCulture(Culture culture)
    {
        switch (culture)
        {
            case Culture.Red:
                return new Color(UnityEngine.Random.Range(0.25f, 1.0f), 0, 0, 1);
            case Culture.Green:
                return new Color(0, UnityEngine.Random.Range(0.25f, 1.0f), 0, 1);
            case Culture.Blue:
                return new Color(0, 0, UnityEngine.Random.Range(0.25f, 1.0f), 1);
            case Culture.Purple:
                List<Color> purpleColors = new List<Color>();
                purpleColors.Add(new Color(186.0f / 255, 85.0f / 255, 211.0f / 255, 1));         //Medium Orchid
                purpleColors.Add(new Color(147.0f / 255, 112.0f / 255, 219.0f / 255, 1));        //Medium Purple
                purpleColors.Add(new Color(138.0f / 255, 43.0f / 255, 226.0f / 255, 1));         //Blue Violet
                purpleColors.Add(new Color(148.0f / 255, 0.0f / 255, 211.0f / 255, 1));          //Dark Violet
                purpleColors.Add(new Color(153.0f / 255, 50.0f / 255, 204.0f / 255, 1));         //Dark Orchid
                purpleColors.Add(new Color(139.0f / 255, 0.0f / 255, 139.0f / 255, 1));          //Dark Magenta
                purpleColors.Add(new Color(128.0f / 255, 0.0f / 255, 128.0f / 255, 1));          //Purple
                int random = UnityEngine.Random.Range(0, purpleColors.Count);
                return purpleColors[random];
            case Culture.Gold:
                List<Color> goldColors = new List<Color>();
                goldColors.Add(new Color(238.0f / 255, 232.0f / 255, 170.0f / 255, 1));          //Pale Golden Rod
                goldColors.Add(new Color(240.0f / 255, 230.0f / 255, 140.0f / 255, 1));          //Khaki
                goldColors.Add(new Color(255.0f / 255, 215.0f / 255, 0.0f / 255, 1));            //Gold
                goldColors.Add(new Color(255.0f / 255, 223.0f / 255, 0.0f / 255, 1));            //Golden Yellow
                goldColors.Add(new Color(212.0f / 255, 175.0f / 255, 55.0f / 255, 1));           //Metallic Gold
                goldColors.Add(new Color(207.0f / 255, 181.0f / 255, 59.0f / 255, 1));           //Old Gold
                goldColors.Add(new Color(197.0f / 255, 179.0f / 255, 88.0f / 255, 1));           //Vegas Gold
                int randomIndex = UnityEngine.Random.Range(0, goldColors.Count);
                return goldColors[randomIndex];
            case Culture.Silver:
                List<Color> silverColors = new List<Color>();
                silverColors.Add(new Color(211.0f / 255, 211.0f / 255, 211.0f / 255, 1));
                silverColors.Add(new Color(192.0f / 255, 192.0f / 255, 192.0f / 255, 1));
                silverColors.Add(new Color(169.0f / 255, 169.0f / 255, 169.0f / 255, 1));
                int silverRandomIndex = UnityEngine.Random.Range(0, silverColors.Count);
                return silverColors[silverRandomIndex];
            default:
                return new Color(1, 1, 1, 1);
        }
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
    public int ID;
    public List<int> solarSystemIDs = null;
    public List<int> planetIDs = null;

    public EmpireData(NewEmpire empire)
    {
        name = empire.name;
        culture = empire.culture;
        color = new float[4] { empire.color.r, empire.color.g, empire.color.b, empire.color.a };
        ID = empire.ID;
        solarSystemIDs = empire.solarSystemIDs;
        planetIDs = empire.planetIDs;
    }
}