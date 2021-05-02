using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
//using UnityEditorInternal.VR;

public class NewGameMenu : GalaxyMenuBehaviour
{
    [Header("New Game Menu Options")]

    [SerializeField]
    private AudioClip mouseEnterButtonSFX = null;
    [SerializeField]
    private AudioClip openNewMenuSFX = null;
    [SerializeField]
    private AudioClip clickToggleSFX = null;

    public int minimumNumberOfPlanets;
    public int maximumNumberOfPlanets;
    public int minimumNumberOfEmpires;
    public int maximumNumberOfEmpires;

    [Header("Game Settings")]

    [SerializeField]
    private Toggle ironpillModeToggle = null;
    [SerializeField]
    private Text ironpillModeToggleLabel = null;
    [SerializeField]
    private Image ironpillModeIconImage = null;

    [Header("Galaxy Settings")]

    public InputField numberOfPlanetsInputField;
    public InputField numberOfEmpiresInputField;

    [Header("Empire Creation")]

    [SerializeField]
    private InputField empireNameInputField = null;
    [SerializeField]
    private Dropdown empireCultureDropdown = null;
    [SerializeField]
    private Image empireFlagBackgroundImage = null;
    [SerializeField]
    private Image empireFlagSymbolImage = null;
    [SerializeField]
    private FlagCreationMenu flagCreationMenu = null;

    [Header("Achievements")]

    [SerializeField]
    private Text achievementsStatusText = null;

    [Header("Editor")]

    [SerializeField]
    private MainMenu mainMenu = null;

    //Non-inspector variables.

    private static Empire.Culture empireCulture = Empire.Culture.Red;
    public static Empire.Culture EmpireCulture
    {
        get
        {
            return empireCulture;
        }
        set
        {
            empireCulture = value;
        }
    }

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
    public override void Start()
    {
        base.Start();

        numberOfPlanets = maximumNumberOfPlanets;
        numberOfEmpires = maximumNumberOfEmpires;

        UpdateEmpireCultureDropdownValues();

        numberOfPlanetsInputField.placeholder.GetComponent<Text>().text = maximumNumberOfPlanets.ToString();
        numberOfEmpiresInputField.placeholder.GetComponent<Text>().text = maximumNumberOfEmpires.ToString();

        RandomizePlaceholderEmpireName();

        initialized = true;
    }

    public override void Awake()
    {
        base.Awake();

        newGameMenu = this;

        if (MainMenu.SceneCamera == null)
        {
            if (mainMenu != null)
                mainMenu.Awake();
        }

        EmpireNameGenerator.ResetCache(false);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    private void UpdateEmpireCultureDropdownValues()
    {
        empireCultureDropdown.options.Clear();
        for(int cultureIndex = 0; cultureIndex < Enum.GetNames(typeof(Empire.Culture)).Length; cultureIndex++)
        {
            Dropdown.OptionData empireCultureDropdownOptionData = new Dropdown.OptionData();
            empireCultureDropdownOptionData.text = ((Empire.Culture)cultureIndex).ToString();
            empireCultureDropdown.options.Add(empireCultureDropdownOptionData);
        }
        empireCultureDropdown.captionText.text = ((Empire.Culture)0).ToString();
    }

    private void UpdateAchievementsStatusText()
    {
        achievementsStatusText.text = "Achievements: " + (AchievementsEnabled ? "Enabled" : "Disabled");
    }

    public void PlayGame()
    {
        EmpireNameGenerator.ResetCache(empireNameInputField.text.Equals(""));

        //Switches to the galaxy scene.
        SceneManager.LoadScene(sceneName:"Galaxy");
    }

    public void OnEndEditNumberOfEmpiresInputField()
    {
        if(numberOfEmpiresInputField.text.Equals(""))
        {
            numberOfEmpires = maximumNumberOfEmpires;
            return;
        }

        int specifiedNumberOfEmpires = int.Parse(numberOfEmpiresInputField.text);

        if(specifiedNumberOfEmpires < minimumNumberOfEmpires)
        {
            numberOfEmpires = minimumNumberOfEmpires;
            numberOfEmpiresInputField.text = minimumNumberOfEmpires.ToString();
        }
        else if (specifiedNumberOfEmpires > maximumNumberOfEmpires)
        {
            numberOfEmpires = maximumNumberOfEmpires;
            numberOfEmpiresInputField.text = maximumNumberOfEmpires.ToString();
        }
        else
        {
            numberOfEmpires = specifiedNumberOfEmpires;
        }
    }

    public void OnEndEditEmpireNameInputField()
    {
        empireName = empireNameInputField.text;

        if (empireName.Equals(""))
        {
            empireName = empireNameInputField.placeholder.GetComponent<Text>().text;
        }
    }

    public void ChangeNumberOfPlanets()
    {
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

    public void OnEmpireCultureDropdownValueChange()
    {
        EmpireCulture = (Empire.Culture)empireCultureDropdown.value;
    }

    public void OnIronmanModeToggleChangeValue()
    {
        ironpillModeToggleLabel.text = ironpillModeToggle.isOn ? "Enabled" : "Disabled";
        ironpillModeIconImage.color = ironpillModeToggle.isOn ? new Color(ironpillModeIconImage.color.r, ironpillModeIconImage.color.g, ironpillModeIconImage.color.b, 1) : new Color(ironpillModeIconImage.color.r, ironpillModeIconImage.color.g, ironpillModeIconImage.color.b, 128.0f / 255);
        IronmanModeEnabled = ironpillModeToggle.isOn;

        AudioManager.PlaySFX(clickToggleSFX);
    }

    public void RandomizePlaceholderEmpireName()
    {
        empireName = EmpireNameGenerator.GenerateEmpireName("", true);
        empireNameInputField.placeholder.GetComponent<Text>().text = empireName;
        empireNameInputField.text = "";
    }

    private void OnEnable()
    {
        empireFlag = flagCreationMenu.EmpireFlag;

        empireFlagBackgroundImage.color = empireFlag.backgroundColor;
        empireFlagSymbolImage.sprite = flagCreationMenu.flagSymbols[empireFlag.symbolSelected];
        empireFlagSymbolImage.color = empireFlag.symbolColor;
    }

    public void ClickEmpireFlagButton()
    {
        flagCreationMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);

        AudioManager.PlaySFX(openNewMenuSFX);
    }

    public void OnMouseEnterButton()
    {
        AudioManager.PlaySFX(mouseEnterButtonSFX);
    }
}
