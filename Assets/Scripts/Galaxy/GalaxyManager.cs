using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyManager : MonoBehaviour
{
    public static List<GameObject> planets;

    public static void Initialize(List<GameObject> planetList)
    {
        planets = planetList;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Empire
{
    public static List<Empire> empires;

    //Planets
    public List<int> planetsOwned;

    //Flags
    public int symbolSelected;
    public Vector3 backgroundColor;
    public Vector3 symbolColor;
}
