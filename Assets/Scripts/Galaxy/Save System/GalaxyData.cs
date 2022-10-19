using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GalaxyData
{
    public List<GalaxySolarSystemData> solarSystems = null;

    public GalaxyData(NewGalaxyManager galaxyManager)
    {
        if (galaxyManager == null)
            Debug.LogError("Galaxy save data cannot be created without a valid non-null galaxy manager passed into the constructor.");

        solarSystems = new List<GalaxySolarSystemData>();
        foreach(GalaxySolarSystem solarSystem in NewGalaxyManager.solarSystems)
            solarSystems.Add(new GalaxySolarSystemData(solarSystem));
    }
}