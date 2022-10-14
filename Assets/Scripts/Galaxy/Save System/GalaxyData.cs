using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GalaxyData
{
    public GalaxyData()
    {
        //Checks if the scene currently open is the galaxy and logs an error and returns if not.
        if(!SceneManager.GetActiveScene().name.Equals("New Galaxy"))
        {
            Debug.LogError("Galaxy save data object cannot be created since the galaxy scene itself isn't even open.");
            return;
        }
    }
}