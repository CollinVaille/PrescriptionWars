using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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
            previousCommands.Add(commandInputField.text.ToLower());
            commandHistoryText.text += "\n" + commandInputField.text;

            RunThroughCommands(commandInputField.text);

            commandInputField.text = "";
        }
    }

    void RunThroughCommands(string command)
    {
        string startsWithText = command;

        if (command.Contains(" "))
        {
            startsWithText = command.Substring(0, command.IndexOf(' '));
        }

        switch (startsWithText)
        {
            case "play_empire_culture":
                ChangeEmpireBasedOnCulture(command);
                break;
            case "clear":
                ClearConsole();
                break;
            case "credits":
                AddCredits(command);
                break;
            case "play_empire_id":
                ChangeEmpireBasedOnID(command);
                break;
            /*case "set_planet_culture":
                SetPlanetCulture(command);
                break;*/
            case "prescriptions":
                AddPrescriptions(command);
                break;
            case "science":
                AddScience(command);
                break;
            case "randomize_displayed_tech":
                RandomizeDisplayedTech(command);
                break;
            case "end_turn":
                EndTurn();
                break;
            case "change_empire_name":
                ChangeEmpireName(command);
                break;
            case "observe":
                ToggleObservationMode();
                break;
            case "demolish_all_buildings":
                DemolishAllBuildings();
                break;
            case "toggle_research_effects":
                ToggleResearchEffects();
                break;
            case "conquer_planet":
                ConquerPlanet(command);
                break;

            default:
                commandHistoryText.text += "\nInvalid Command";
                break;
        }
    }

    void ConquerPlanet(string command)
    {
        string inputedPlanetName = "";

        try
        {
            inputedPlanetName = command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)).ToLower();
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Planet Name";
            return;
        }

        for(int x = 0; x < GalaxyManager.planets.Count; x++)
        {
            PlanetIcon planet = GalaxyManager.planets[x].GetComponent<PlanetIcon>();

            //string planetName = GeneralHelperMethods.RemoveCarriageReturn(planet.nameLabel.text.ToLower());
            string planetName = planet.nameLabel.text.ToLower();

            if (inputedPlanetName.Equals(planetName))
            {
                planet.ConquerPlanet(GalaxyManager.playerID);
                commandHistoryText.text += "\nSuccess";
                return;
            }
        }

        commandHistoryText.text += "\nNo " + inputedPlanetName + " Exists";
    }

    void ToggleResearchEffects()
    {
        Empire.empires[GalaxyManager.playerID].receivesResearchEffects = !Empire.empires[GalaxyManager.playerID].receivesResearchEffects;
        Empire.empires[GalaxyManager.playerID].techManager.UpdateTechnologyEffects();

        if (Empire.empires[GalaxyManager.playerID].receivesResearchEffects)
            commandHistoryText.text += "\nEnabled";
        else
            commandHistoryText.text += "\nDisabled";
    }

    void DemolishAllBuildings()
    {
        foreach(int planetIndex in Empire.empires[GalaxyManager.playerID].planetsOwned)
        {
            foreach(GalaxyCity city in GalaxyManager.planets[planetIndex].GetComponent<PlanetIcon>().cities)
            {
                city.buildingsCompleted.Clear();
            }
        }

        commandHistoryText.text += "\nSuccess";
    }

    void ToggleObservationMode()
    {
        GalaxyManager.observationModeEnabled = !GalaxyManager.observationModeEnabled;

        if (GalaxyManager.observationModeEnabled)
            commandHistoryText.text += "\nEnabled";
        else
            commandHistoryText.text += "\nDisabled";
    }

    void ChangeEmpireName(string command)
    {
        try
        {
            Empire.empires[GalaxyManager.playerID].empireName = command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1));
            commandHistoryText.text += "\nSuccess";
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Empire Name";
        }
    }

    void EndTurn()
    {
        GalaxyManager.galaxyManager.EndTurn();
    }

    void RandomizeDisplayedTech(string command)
    {
        string techTotemName = "";

        try
        {
            techTotemName = command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1));
            techTotemName.ToLower();


            foreach(TechTotem totem in Empire.empires[GalaxyManager.playerID].techManager.techTotems)
            {
                if (techTotemName.Equals(totem.name.ToLower()))
                {
                    totem.RandomizeTechDisplayed();
                    commandHistoryText.text += "\nSuccess";
                    return;
                }
            }

            commandHistoryText.text += "\nInvalid Tech Totem Name";
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Tech Totem Name";
            return;
        }
    }

    void AddScience(string command)
    {
        try
        {
            Empire.empires[GalaxyManager.playerID].science += int.Parse(command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)));
        }
        catch (Exception)
        {
            commandHistoryText.text += "\nInvalid Amount";
            return;
        }

        commandHistoryText.text += "\nSuccess";
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
            planetName = command.Substring(command.IndexOf(' ') + 1, command.IndexOf(' ', command.IndexOf(' ') + 1) - (command.IndexOf(' ') + 1));

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
            switch(command.Substring(command.IndexOf(' ', command.IndexOf(' ') + 1) + 1, command.Length - (command.IndexOf(' ', command.IndexOf(' ') + 1) + 1)).ToLower())
            {
                case "red":
                    GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Red;
                    break;
                case "green":
                    GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Green;
                    break;
                case "blue":
                    GalaxyManager.planets[index].GetComponent<PlanetIcon>().culture = Empire.Culture.Blue;
                    break;
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

    public void ToggleConsole()
    {
        transform.gameObject.SetActive(!transform.gameObject.activeInHierarchy);
    }

    void ChangeEmpireBasedOnCulture(string command)
    {
        Empire.Culture selectedCulture = Empire.Culture.Red;

        bool commandSuccessful = false;

        switch(command.Substring(command.IndexOf(' ') + 1, command.Length - (command.IndexOf(' ') + 1)).ToLower())
        {
            case "red":
                selectedCulture = Empire.Culture.Red;
                commandSuccessful = true;
                break;
            case "green":
                selectedCulture = Empire.Culture.Green;
                commandSuccessful = true;
                break;
            case "blue":
                selectedCulture = Empire.Culture.Blue;
                commandSuccessful = true;
                break;
            case "purple":
                selectedCulture = Empire.Culture.Purple;
                commandSuccessful = true;
                break;
            case "gold":
                selectedCulture = Empire.Culture.Gold;
                commandSuccessful = true;
                break;
        }

        if (commandSuccessful)
        {
            bool empireFound = false;
            for (int x = 0; x < Empire.empires.Count; x++)
            {
                if (Empire.empires[x].empireCulture == selectedCulture)
                {
                    GalaxyManager.playerID = x;
                    empireFound = true;
                    commandHistoryText.text += "\nSuccess";
                }
            }
            if (!empireFound)
                commandHistoryText.text += "\nNo Empire Is Of " + selectedCulture + " Culture.";
        }
        else
        {
            commandHistoryText.text += "\nInvalid Culture";
        }
    }
}
