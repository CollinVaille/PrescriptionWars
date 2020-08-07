using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditorInternal.VR;

public class NewGameMenu : MonoBehaviour
{
    public InputField empireNameInputField;
    public InputField numberOfPlanetsInputField;
    public InputField numberOfEmpiresInputField;

    public int minimumNumberOfPlanets;
    public int maximumNumberOfPlanets;
    public int minimumNumberOfEmpires;
    public int maximumNumberOfEmpires;

    public Dropdown empireCultureDropdown;

    public static Empire.Culture empireCulture = Empire.Culture.Red;

    public static string empireName = "";

    public static int numberOfPlanets = 60;
    public static int numberOfEmpires = 3;

    public static bool initialized = false;

    public static Flag empireFlag = new Flag();

    // Start is called before the first frame update
    void Start()
    {
        numberOfPlanets = maximumNumberOfPlanets;
        numberOfEmpires = 3;

        numberOfPlanetsInputField.placeholder.GetComponent<Text>().text = "Number of Planets... (" + minimumNumberOfPlanets + "-" + maximumNumberOfPlanets + ")";
        numberOfEmpiresInputField.placeholder.GetComponent<Text>().text = "Number of Empires... (" + minimumNumberOfEmpires + "-" + maximumNumberOfEmpires + ")";

        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayGame()
    {
        SceneManager.LoadScene(sceneName:"Galaxy");
    }

    string RemoveNonNumbers(string s)
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
}
