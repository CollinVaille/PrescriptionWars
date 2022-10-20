using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewEmpire
{
    /// <summary>
    /// Private variable used to hold the name of the empire, but the name should ideally be accessed and mutated through the public property.
    /// </summary>
    private string nameVar = null;
    /// <summary>
    /// Publicly property that should be used both to access and mutate the name of the empire.
    /// </summary>
    public string name { get => nameVar; set => nameVar = value; }

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
        IDVar = empireData.ID;
        solarSystemIDsVar = empireData.solarSystemIDs;
        planetIDsVar = empireData.planetIDs;
    }

    public NewEmpire(string name, int ID, List<int> solarSystemIDs, List<int> planetIDs)
    {
        nameVar = name;
        IDVar = ID;
        solarSystemIDsVar = solarSystemIDs;
        planetIDsVar = planetIDs;
    }
}

[System.Serializable]
public class EmpireData
{
    public string name;
    public int ID;
    public List<int> solarSystemIDs = null;
    public List<int> planetIDs = null;

    public EmpireData(NewEmpire empire)
    {
        name = empire.name;
        ID = empire.ID;
        solarSystemIDs = empire.solarSystemIDs;
        planetIDs = empire.planetIDs;
    }
}