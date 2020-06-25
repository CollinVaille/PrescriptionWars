using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatConsole : MonoBehaviour
{
    public InputField commandInputField;

    public Text commandHistoryText;

    List<string> previousCommands;

    int previousCommandSelected;

    // Start is called before the first frame update
    void Start()
    {
        previousCommands = new List<string>();

        previousCommandSelected = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(commandInputField.isFocused && Input.GetKeyDown(KeyCode.UpArrow) && previousCommands.Count > 0)
        {
            commandInputField.text = previousCommands[previousCommandSelected];

            if (previousCommandSelected > 0)
                previousCommandSelected--;
        }
        if (!commandInputField.isFocused)
        {
            previousCommandSelected = previousCommands.Count - 1;
        }
    }

    public void EnterCommand()
    {
        if(commandInputField.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            previousCommands.Add(commandInputField.text);
            commandHistoryText.text += "\n" + commandInputField.text;

            RunThroughCommands(commandInputField.text);

            commandInputField.text = "";
        }
    }

    void RunThroughCommands(string command)
    {
        if (command.ToLower().StartsWith("play_empire_culture"))
        {
            ChangeEmpireBasedOnCulture(command);
        }
        else if (command.ToLower().StartsWith("clear"))
        {
            ClearConsole();
        }
        else if (command.ToLower().StartsWith("credits"))
        {
            AddCredits(command);
        }
        else if (command.ToLower().StartsWith("play_empire_id"))
        {
            ChangeEmpireBasedOnID(command);
        }
        else if (command.ToLower().StartsWith("set_planet_culture"))
        {
            SetPlanetCulture(command);
        }
        else if (command.ToLower().StartsWith("prescriptions"))
        {
            AddPrescriptions(command);
        }
        else
        {
            commandHistoryText.text += "\nInvalid Command";
        }
    }

    void AddPrescriptions(string command)
    {
        try
        {
            Empire.empires[GalaxyManager.playerID].prescriptions += int.Parse(command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)));
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Amount";
            return;
        }

        commandHistoryText.text += "\nSuccess";
    }

    void SetPlanetCulture(string command)       //Doesn't work atm.
    {
        int index = -1;

        string planetName;

        try
        {
            planetName = command.Substring(command.IndexOf(' ') + 1, command.IndexOf(' ', command.IndexOf(' ') + 1) - (command.IndexOf(' ') + 1)).ToLower();

            for(int x = 0; x < GalaxyManager.planets.Count; x++)
            {
                if (GalaxyManager.planets[x].GetComponent<PlanetIcon>().name.ToLower().Equals(planetName))      //Gets hung up here.
                {
                    index = x;
                }
            }
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Planet Name";
            return;
        }

        if(index == -1)
        {
            commandHistoryText.text += "\nInvalid Planet Name";
            return;
        }

        try
        {
            if (command.Substring(command.IndexOf(' ', command.IndexOf(' ') + 1) + 1, command.Length - (command.IndexOf(' ', command.IndexOf(' ') + 1) + 1)).ToLower().Equals("red"))
            {
                GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Red;
            }
            else if (command.Substring(command.IndexOf(' ', command.IndexOf(' ') + 1) + 1, command.Length - (command.IndexOf(' ', command.IndexOf(' ') + 1) + 1)).ToLower().Equals("green"))
            {
                GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Green;
            }
            else if (command.Substring(command.IndexOf(' ', command.IndexOf(' ') + 1) + 1, command.Length - (command.IndexOf(' ', command.IndexOf(' ') + 1) + 1)).ToLower().Equals("blue"))
            {
                GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Blue;
            }
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Culture";
            return;
        }

        commandHistoryText.text += "\nSuccess";
        Debug.Log(GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture);
    }

    void ChangeEmpireBasedOnID(string command)
    {
        int id;

        try
        {
            id = int.Parse(command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)));
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nID Invalid";
            return;
        }

        if(id >= 0 && id < Empire.empires.Count)
        {
            GalaxyManager.playerID = id;
            commandHistoryText.text += "\nSuccess";
        }
        else
        {
            commandHistoryText.text += "\nID Out Of Range";
        }
    }

    void AddCredits(string command)
    {
        try
        {
            Empire.empires[GalaxyManager.playerID].credits += int.Parse(command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)));
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Amount";
            return;
        }

        commandHistoryText.text += "\nSuccess";
    }

    public void ClearConsole()
    {
        commandHistoryText.text = "Command History:";
    }

    void ChangeEmpireBasedOnCulture(string command)
    {
        Empire.Culture selectedCulture = Empire.Culture.Red;

        bool commandSuccessful = false;

        if (command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)).ToLower().Equals("red"))
        {
            selectedCulture = Empire.Culture.Red;
            commandSuccessful = true;
        }
        else if (command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)).ToLower().Equals("green"))
        {
            selectedCulture = Empire.Culture.Green;
            commandSuccessful = true;
        }
        else if (command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)).ToLower().Equals("blue"))
        {
            selectedCulture = Empire.Culture.Blue;
            commandSuccessful = true;
        }

        if (commandSuccessful)
        {
            for (int x = 0; x < Empire.empires.Count; x++)
            {
                if (Empire.empires[x].empireCulture == selectedCulture)
                {
                    GalaxyManager.playerID = x;
                    commandHistoryText.text += "\nSuccess";
                }
            }
        }
        else
        {
            commandHistoryText.text += "\nInvalid Culture";
        }
    }
}
