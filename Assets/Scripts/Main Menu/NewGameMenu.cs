using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEditorInternal.VR;

public class NewGameMenu : MonoBehaviour, IGalaxyTooltipHandler
{
    [Header("Galaxy Tooltip Handler Options")]

    [SerializeField] private Transform tooltipsParent = null;
    public Transform TooltipsParent
    {
        get
        {
            return tooltipsParent;
        }
    }

    [Header("Old New Game Menu Settings")]

    public InputField empireNameInputField;
    public InputField numberOfPlanetsInputField;
    public InputField numberOfEmpiresInputField;

    public int minimumNumberOfPlanets;
    public int maximumNumberOfPlanets;
    public int minimumNumberOfEmpires;
    public int maximumNumberOfEmpires;

    public Dropdown empireCultureDropdown;

    [Header("Connected Menus")]

    [SerializeField]
    private GameObject empireCreationMenu = null;

    [Header("Ironpill Mode Setting")]

    [SerializeField]
    private Toggle ironpillModeToggle = null;
    [SerializeField]
    private Text ironpillModeToggleLabel = null;
    [SerializeField]
    private Image ironpillModeIconImage = null;

    [Header("Achievements")]

    [SerializeField]
    private Text achievementsStatusText = null;

    [Header("Editor")]

    [SerializeField]
    private MainMenu mainMenu = null;

    //Non-inspector variables.

    public static Empire.Culture empireCulture = Empire.Culture.Red;

    public static string empireName = "";

    public static int numberOfPlanets = 60;
    public static int numberOfEmpires = 5;

    public static bool initialized = false;
    private static bool ironmanModeEnabled = false;
    public static bool IronmanModeEnabled
    {
        get
        {
            return ironmanModeEnabled;
        }
        set
        {
            ironmanModeEnabled = value;
            newGameMenu.UpdateAchievementsStatusText();
        }
    }
    public static bool AchievementsEnabled
    {
        get
        {
            return ironmanModeEnabled;
        }
    }

    public static Flag empireFlag = new Flag();

    private static NewGameMenu newGameMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        numberOfPlanets = maximumNumberOfPlanets;
        numberOfEmpires = 3;

        numberOfPlanetsInputField.placeholder.GetComponent<Text>().text = maximumNumberOfPlanets.ToString();
        numberOfEmpiresInputField.placeholder.GetComponent<Text>().text = maximumNumberOfEmpires.ToString();

        initialized = true;
    }

    void Awake()
    {
        newGameMenu = this;

        if (MainMenu.SceneCamera == null)
        {
            if (mainMenu != null)
                mainMenu.Awake();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void UpdateAchievementsStatusText()
    {
        achievementsStatusText.text = "Achievements: " + (AchievementsEnabled ? "Enabled" : "Disabled");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(sceneName:"Galaxy");
    }

    private string RemoveNonNumbers(string s)
    {
        string output = "";

        foreach(char c in s)
        {
            if (char.IsDigit(c))
                output += c;
        }

        return output;
    }

    public void ChangeNumberOfEmpires()
    {
        if (numberOfEmpiresInputField.text.Length > 0)
            numberOfEmpiresInputField.text = RemoveNonNumbers(numberOfEmpiresInputField.text);

        if(numberOfEmpiresInputField.text.Length > 0)
        {
            if(int.Parse(numberOfEmpiresInputField.text) >= minimumNumberOfEmpires)
            {
                if (int.Parse(numberOfEmpiresInputField.text) > maximumNumberOfEmpires)
                    numberOfEmpiresInputField.text = "" + maximumNumberOfEmpires;

                numberOfEmpires = int.Parse(numberOfEmpiresInputField.text);
            }
        }
        else
        {
            numberOfEmpires = maximumNumberOfEmpires;
        }
    }

    public void ChangeNumberOfPlanets()
    {
        if (numberOfPlanetsInputField.text.Length > 0)
            numberOfPlanetsInputField.text = RemoveNonNumbers(numberOfPlanetsInputField.text);

        if(numberOfPlanetsInputField.text.Length > 0)
        {
            if (int.Parse(numberOfPlanetsInputField.text) >= minimumNumberOfPlanets)
            {
                if (int.Parse(numberOfPlanetsInputField.text) > maximumNumberOfPlanets)
                    numberOfPlanetsInputField.text = "" + maximumNumberOfPlanets;

                numberOfPlanets = int.Parse(numberOfPlanetsInputField.text);
            }
        }
        else
        {
            numberOfPlanets = maximumNumberOfPlanets;
        }
    }

    public static void UpdateEmpireFlag(int symbolSelected, Vector3 backgroundColor, Vector3 symbolColor)
    {
        empireFlag.symbolSelected = symbolSelected;
        empireFlag.backgroundColor = backgroundColor;
        empireFlag.symbolColor = symbolColor;
    }

    public void ChangeEmpireName()
    {
        empireName = empireNameInputField.text;
    }

    public void ChangeEmpireCulture()
    {
        switch (empireCultureDropdown.value)
        {
            case 0:
                empireCulture = Empire.Culture.Red;
                break;
            case 1:
                empireCulture = Empire.Culture.Green;
                break;
            case 2:
                empireCulture = Empire.Culture.Blue;
                break;
            case 3:
                empireCulture = Empire.Culture.Purple;
                break;
            case 4:
                empireCulture = Empire.Culture.Gold;
                break;
        }
    }

    public void OnIronmanModeToggleChangeValue()
    {
        ironpillModeToggleLabel.text = ironpillModeToggle.isOn ? "Enabled" : "Disabled";
        ironpillModeIconImage.color = ironpillModeToggle.isOn ? new Color(ironpillModeIconImage.color.r, ironpillModeIconImage.color.g, ironpillModeIconImage.color.b, 1) : new Color(ironpillModeIconImage.color.r, ironpillModeIconImage.color.g, ironpillModeIconImage.color.b, 128.0f / 255);
        IronmanModeEnabled = ironpillModeToggle.isOn;
    }

    public void ClickEmpireCreationButton()
    {
        empireCreationMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
