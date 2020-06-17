using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyManager : MonoBehaviour
{
    public static int playerID = 0;

    public static List<GameObject> planets;

    public static List<Sprite> flagSymbols;

    public static void Initialize(List<GameObject> planetList, List<Sprite> flagSymbolsList)
    {
        planets = planetList;
        flagSymbols = flagSymbolsList;
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
    public enum Culture
    {
        Red,
        Green,
        Blue
    }

    public static List<Empire> empires;

    //Planets
    public List<int> planetsOwned;

    //General Information
    public string empireName;
    public Culture empireCulture;

    //Flags
    public Flag empireFlag;

    public Color GetEmpireColor()
    {
        return new Color(empireFlag.symbolColor.x, empireFlag.symbolColor.y, empireFlag.symbolColor.z, 1.0f);
    }
}
