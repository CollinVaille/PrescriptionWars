using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyManager : MonoBehaviour
{
    public GameObject commandConsole;

    //Audio stuff.
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public static int playerID = 0;
    public static int turnNumber = 0;

    public static List<GameObject> planets;

    public static List<Sprite> flagSymbols;

    public static bool togglePlanetManagementMenu;

    public static GameObject planetManagementMenu;

    public static void Initialize(List<GameObject> planetList, List<Sprite> flagSymbolsList, GameObject menuOfPlanetManagement)
    {
        planets = planetList;
        flagSymbols = flagSymbolsList;
        planetManagementMenu = menuOfPlanetManagement;

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
            commandConsole.GetComponent<CheatConsole>().ToggleConsole();
        }

        if (togglePlanetManagementMenu)
        {
            PlanetManagementMenu planetManagementMenuScript = planetManagementMenu.GetComponent<PlanetManagementMenu>();

            planetManagementMenu.SetActive(true);
            planetManagementMenuScript.ResetChooseCityMenu();
            togglePlanetManagementMenu = false;
            planetManagementMenuScript.UpdateUI();
            planetManagementMenuScript.PlayOpenMenuSFX();
        }
    }

    public void EndTurn()
    {
        //Everyone makes their moves for the turn.
        for(int x = 0; x < Empire.empires.Count; x++)
        {
            if (x != playerID)
                Empire.empires[x].PlayAI();
        }

        //Stuff is calculated and added after everyone's turn.
        foreach(Empire empire in Empire.empires)
        {
            empire.EndTurn();
        }

        //Logs that a turn has been completed.
        turnNumber++;
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
    public Color empireColor;
    public int empireID;
    public bool playerEmpire;

    //Flags
    public Flag empireFlag;

    //Tech
    public TechManager techManager;

    //Resources
    public float credits;
    public float prescriptions;
    public float science;

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
            creditsPerTurn += GalaxyManager.planets[planetsOwned[x]].GetComponent<PlanetIcon>().creditsPerTurn();
        }

        return creditsPerTurn;
    }

    public float GetPrescriptionsPerTurn()
    {
        float prescriptionsPerTurn = 0.0f;

        for (int x = 0; x < planetsOwned.Count; x++)
        {
            prescriptionsPerTurn += GalaxyManager.planets[planetsOwned[x]].GetComponent<PlanetIcon>().prescriptionsPerTurn();
        }

        return prescriptionsPerTurn;
    }

    public void PlayAI()
    {
        //Checks to make sure that a tech is selected.
        if(techManager.techTotemSelected < 0 || techManager.techTotemSelected >= techManager.techTotems.Count)
        {
            //Determines the lowest level of tech the ai can pick.
            int lowestLevelPossible = 0;
            bool oneTotemEvaluated = false;
            for(int x = 0; x < techManager.techTotems.Count; x++)
            {
                if(techManager.techTotems[x].techsAvailable.Count > 0)
                {
                    if (techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed].level < lowestLevelPossible || !oneTotemEvaluated)
                    {
                        lowestLevelPossible = techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed].level;
                        oneTotemEvaluated = true;
                    }
                }
            }

            if (oneTotemEvaluated)
            {
                //Gets a list of the tech totems whos displayed tech has the lowest possible tech level the ai can pick.
                List<int> possibleTechTotems = new List<int>();
                for (int x = 0; x < techManager.techTotems.Count; x++)
                {
                    if(techManager.techTotems[x].techsAvailable.Count > 0)
                    {
                        if (techManager.techTotems[x].techsAvailable[techManager.techTotems[x].techDisplayed].level == lowestLevelPossible)
                            possibleTechTotems.Add(x);
                    }
                }

                //Picks a random tech totem to research out of the list that was just generated above.
                techManager.techTotemSelected = possibleTechTotems[Random.Range(0, possibleTechTotems.Count)];
            }
            else
            {
                techManager.techTotemSelected = -1;
            }
        }
    }

    public void EndTurn()
    {
        foreach(int planetID in planetsOwned)
        {
            PlanetIcon planetScript = GalaxyManager.planets[planetID].GetComponent<PlanetIcon>();

            planetScript.EndTurn();
            techManager.EndTurn(1);
        }
    }
}

public class TechManager
{
    public static List<TechTotem> initialTechTotems = new List<TechTotem>();

    public List<TechTotem> techTotems;

    public int techTotemSelected = -1;
    public int ownerEmpireID;

    public void EndTurn(float scienceToAdd)
    {
        Empire.empires[ownerEmpireID].science += scienceToAdd;

        bool researchingSomething = true;
        //Detects if a valid tech totem is selected.
        if (techTotemSelected > -1 && techTotemSelected < techTotems.Count)
        {
            if (techTotems[techTotemSelected].techsAvailable.Count > 0)
            {
                //Detects if the empire has enough science to complete the selected tech.
                if (Empire.empires[ownerEmpireID].science >= techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed].cost)
                {
                    //Removes the tech's cost from the total science.
                    Empire.empires[ownerEmpireID].science -= techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed].cost;

                    //Removes the completed tech from the available techs list and adds it to the techs completed list.
                    techTotems[techTotemSelected].techsCompleted.Add(techTotems[techTotemSelected].techsAvailable[techTotems[techTotemSelected].techDisplayed]);
                    techTotems[techTotemSelected].techsAvailable.RemoveAt(techTotems[techTotemSelected].techDisplayed);

                    //Randomizes what tech will be displayed next.
                    techTotems[techTotemSelected].RandomizeTechDisplayed();

                    //Makes it to where no tech totem is selected.
                    techTotemSelected = -1;
                }
            }
            else
                researchingSomething = false;
        }
        else
        {
            researchingSomething = false;
        }

        if (!researchingSomething)
        {
            //If the player has nothing researching for an entire turn, the science is set back to zero as a sort of punishment. :)
            Empire.empires[ownerEmpireID].science = 0;
        }
    }
}

public class TechTotem
{
    public List<Tech> techsCompleted = new List<Tech>();
    public List<Tech> techsAvailable = new List<Tech>();

    public int techDisplayed;

    public void RandomizeTechDisplayed()
    {
        if (techsAvailable.Count > 0)
        {
            int lowestPossibleLevel = 0;
            for (int x = 0; x < techsAvailable.Count; x++)
            {
                if (techsAvailable[x].level < lowestPossibleLevel || x == 0)
                    lowestPossibleLevel = techsAvailable[x].level;
            }

            List<int> possibleTechs = new List<int>();
            for (int x = 0; x < techsAvailable.Count; x++)
            {
                if (techsAvailable[x].level == lowestPossibleLevel)
                    possibleTechs.Add(x);
            }

            techDisplayed = possibleTechs[Random.Range(0, possibleTechs.Count)];
        }
        else
        {
            techDisplayed = -1;
        }
    }
}

public class Tech
{
    public string name;
    public string description;

    public int level;

    public float cost;

    public Sprite image;
}