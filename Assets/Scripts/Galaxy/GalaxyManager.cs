using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyManager : MonoBehaviour
{
    public GameObject commandConsole;

    public static int playerID = 0;

    public static List<GameObject> planets;

    public static List<Sprite> flagSymbols;

    public static bool togglePlanetManagementMenu;

    public GameObject planetManagementMenu;

    public static void Initialize(List<GameObject> planetList, List<Sprite> flagSymbolsList)
    {
        planets = planetList;
        flagSymbols = flagSymbolsList;

        togglePlanetManagementMenu = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int x = 0; x < Empire.empires.Count; x++)
        {
            if (x == playerID)
                Empire.empires[x].playerEmpire = true;
            else
                Empire.empires[x].playerEmpire = false;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            commandConsole.SetActive(!commandConsole.activeInHierarchy);
        }

        if (togglePlanetManagementMenu)
        {
            planetManagementMenu.SetActive(true);
            togglePlanetManagementMenu = false;
        }
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
    public bool playerEmpire;
    public Color empireColor;

    //Flags
    public Flag empireFlag;

    //Resources
    public float credits;
    public float prescriptions;

    public Color GetLabelColor()
    {
        Color labelColor = empireColor;

        labelColor.r += 0.3f;
        labelColor.g += 0.3f;
        labelColor.b += 0.3f;

        return labelColor;
    }

    public float GetCreditsPerTurn()
    {
        float creditsPerTurn = 0.0f;

        for(int x = 0; x < planetsOwned.Count; x++)
        {
            creditsPerTurn += GalaxyManager.planets[planetsOwned[x]].GetComponent<PlanetIcon>().creditsPerTurn;
        }

        return creditsPerTurn;
    }

    public float GetPrescriptionsPerTurn()
    {
        float prescriptionsPerTurn = 0.0f;

        for (int x = 0; x < planetsOwned.Count; x++)
        {
            prescriptionsPerTurn += GalaxyManager.planets[planetsOwned[x]].GetComponent<PlanetIcon>().prescriptionsPerTurn;
        }

        return prescriptionsPerTurn;
    }
}
