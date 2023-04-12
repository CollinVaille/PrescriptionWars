using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GalaxyData
{
    //Necessary data.
    public List<GalaxySolarSystemData> solarSystems = null;
    public List<EmpireData> empires = null;
    public List<HyperspaceLaneData> hyperspaceLanes = null;
    public int playerID = -1;
    public int turnNumber = -1;
    public float galaxyLocalYRotation = 0;
    public bool observationModeEnabled = false;

    //Info.
    public string galaxyShape = null;
    public string saveName = null;

    public GalaxyData(NewGalaxyManager galaxyManager)
    {
        if (galaxyManager == null)
        {
            Debug.LogError("Galaxy save data cannot be created without a valid non-null galaxy manager passed into the constructor.");
            return;
        }

        solarSystems = new List<GalaxySolarSystemData>();
        foreach(GalaxySolarSystem solarSystem in NewGalaxyManager.solarSystems)
            solarSystems.Add(new GalaxySolarSystemData(solarSystem));

        empires = new List<EmpireData>();
        foreach (NewEmpire empire in NewGalaxyManager.empires)
            empires.Add(new EmpireData(empire));

        hyperspaceLanes = new List<HyperspaceLaneData>();
        foreach (HyperspaceLane hyperspaceLane in NewGalaxyManager.hyperspaceLanes)
            hyperspaceLanes.Add(new HyperspaceLaneData(hyperspaceLane));

        playerID = NewGalaxyManager.playerID;

        turnNumber = NewGalaxyManager.turnNumber;

        galaxyLocalYRotation = NewGalaxyManager.galaxyManager.transform.localRotation.eulerAngles.y;

        observationModeEnabled = NewGalaxyManager.observationModeEnabled;

        galaxyShape = NewGalaxyManager.galaxyShape;

        saveName = NewGalaxyManager.saveName;
    }
}