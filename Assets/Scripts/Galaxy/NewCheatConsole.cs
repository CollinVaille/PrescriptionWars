using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NewCheatConsole : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The input field that the player will use in order to input cheat commands into the cheat console.")] private InputField inputField = null;
    [SerializeField, Tooltip("The transform of the game object that serves as the parent for all cheat console history logs.")] private Transform historyLogParent = null;
    [SerializeField, Tooltip("The vertical layout group that manages the spacing between cheat console history logs.")] private VerticalLayoutGroup historyLogVerticalLayoutGroup = null;

    [Header("SFX Options")]

    [SerializeField, Tooltip("The sound effect that will be played whenever the cheat console closes.")] private AudioClip closeSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played whenever the player clicks the close button on the cheat console.")] private AudioClip clickCloseButtonSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played whenever the player hovers over the close button on the cheat console.")] private AudioClip hoverCloseButtonSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Private variable that indicates whether the cheat console's history needs a spacing update the next frame.
    /// </summary>
    private bool historyLogSpacingUpdateRequired = false;
    /// <summary>
    /// Private variable that indicates how many frames have passed since the cheat console's history logs last required a spacing update.
    /// </summary>
    private int framesSinceHistoryLogSpacingUpdateRequired = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Just messes with the spacing value of the vertical layout group in order to update the spacing of the cheat console's history.
        if (historyLogSpacingUpdateRequired)
        {
            if(framesSinceHistoryLogSpacingUpdateRequired > 0)
            {
                historyLogVerticalLayoutGroup.spacing++;
                historyLogVerticalLayoutGroup.spacing--;
                historyLogSpacingUpdateRequired = false;
                framesSinceHistoryLogSpacingUpdateRequired = 0;
            }
            framesSinceHistoryLogSpacingUpdateRequired++;
        }
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the player hovers over the close button and this method then plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterCloseButton()
    {
        //Plays the appropriate sound effect for the player hovering over the close button.
        AudioManager.PlaySFX(hoverCloseButtonSFX);
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the player presses the close button and this method closes the cheat console and plays the appropriate sound effects.
    /// </summary>
    public void OnClickCloseButton()
    {
        //Plays the appropriate sound effect for clicking the close button.
        AudioManager.PlaySFX(clickCloseButtonSFX);

        //Closes the cheat console.
        Close();
    }

    /// <summary>
    /// Public method that should be called in order to close the cheat console and play the appropriate sound effect.
    /// </summary>
    public void Close()
    {
        //Returns if the cheat console is not currently open because theres no open cheat console to close.
        if (!gameObject.activeSelf)
            return;

        //Deactivates the cheat console game object.
        gameObject.SetActive(false);
        //Plays the appropriate sound effect for closing the cheat console.
        AudioManager.PlaySFX(closeSFX);
    }

    /// <summary>
    /// Public method that should be called in order to open the cheat console.
    /// </summary>
    public void Open()
    {
        //Returns if the cheat console is currently open because theres no closed cheat console to open.
        if (gameObject.activeSelf)
            return;

        //Activates the cheat console game object.
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Public method that should be called in order to toggle the cheat console between open or closed (will open if currently closed, will close if currently open).
    /// </summary>
    public void Toggle()
    {
        //Closes the cheat console if it's currently open.
        if (gameObject.activeSelf)
            Close();
        //Opens the cheat console if it's currently closed.
        else
            Open();
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the player submits the command text that they've written inside the cheat console's input field.
    /// </summary>
    public void OnSubmitInputField()
    {
        //Splits the player's entered cheat command into its components (index == 0 : command, index > 0 : argument).
        string[] cheatCommandComponents = inputField.text.Split((char[]) null, System.StringSplitOptions.RemoveEmptyEntries);
        //Executes the logic needed for the player's entered cheat command and logs it to the history log scroll list for the player to see.
        CheatConsoleHistoryLog.InstantiateLog(historyLogParent, inputField.text, OnEnterCheatCommand(cheatCommandComponents != null && cheatCommandComponents.Length >= 1 ? cheatCommandComponents[0] : string.Empty, cheatCommandComponents != null && cheatCommandComponents.Length >= 2 ? cheatCommandComponents.Skip(1).ToArray() : null));
        //Indicates that a spacing update between cheat console history logs is required the next frame and resets the frame counter variable.
        historyLogSpacingUpdateRequired = true;
        framesSinceHistoryLogSpacingUpdateRequired = 0;

        //Resets the input field's inputted text since a command was just entered.
        inputField.text = string.Empty;
    }

    /// <summary>
    /// Private method that should be called if the player enters a cheat command that passes the initial validation checks.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private string OnEnterCheatCommand(string command, string[] args)
    {
        //Converts the entered cheat command to all lowercase for easier checking.
        command = command.ToLower();

        switch (command.ToLower())
        {
            //Checks if the player entered the "help" command.
            case "help":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args != null && args.Length > 0)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command does not take any arguments.";
                return "Cheat commands:\nclear\nobserve\nplay_empire_culture <culture name> || pec <culture name>\nplay_empire_id <empire ID> || peid <empire ID>\nset_credits <credits amount> || sc <credits amount>\nset_prescriptions <prescriptions amount> || sp <prescriptions amount>";
            //Checks if the player entered the "clear" command.
            case "clear":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args != null && args.Length > 0)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command does not take any arguments.";
                //Clears the cheat console's history and returns with a successful message.
                ClearHistory();
                return "Success. The cheat console's history has been cleared.";
            //Checks if the player entered the "observe" command.
            case "observe":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args != null && args.Length > 0)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command does not take any arguments.";
                //Toggles whether or not the game is in observation mode where a bot also controls the player's empire.
                NewGalaxyManager.observationModeEnabled = !NewGalaxyManager.observationModeEnabled;
                return NewGalaxyManager.observationModeEnabled ? "Success. The player is now in observation mode and is observing the empire belonging to empire ID " + NewGalaxyManager.playerID + "." : "Success. The player is no longer in observation mode and is fully controlling the empire belonging to empire ID " + NewGalaxyManager.playerID + ".";
            //Checks if the player entered the "pec" or "play_empire_culture" command.
            case "pec":
            case "play_empire_culture":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args == null || args.Length != 1)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command takes one argument that is the name of the imperial culture that the player wishes to switch to the empire of.";

                //Determines which culture the player was specifying in their entered cheat command.
                NewEmpire.Culture cultureSelected = 0;
                bool cultureFound = false;
                foreach(NewEmpire.Culture culture in Enum.GetValues(typeof(NewEmpire.Culture)))
                {
                    if (culture.ToString().ToLower().Equals(args[0].ToLower()))
                    {
                        cultureSelected = culture;
                        cultureFound = true;
                        break;
                    }
                }

                //Checks if the player's specified culture was found to be a possible culture enum value and returns and informs the user if not.
                if (!cultureFound)
                    return "Invalid cheat command argument. \"" + args[0] + "\" is not a valid empire culture.";

                //Loops through each empire until finding one of the specified culture, then it switches the player's control over to that empire (if not already controlled by the player) and returns.
                foreach(NewEmpire empire in NewGalaxyManager.empires)
                {
                    if(empire.culture == cultureSelected)
                    {
                        //Checks if the player is already controlling the specified empire and returns and tells the player if so.
                        if (NewGalaxyManager.playerID == empire.ID)
                            return "Player is already controlling the \"" + GeneralHelperMethods.GetEnumText(cultureSelected.ToString()) + "\" empire.";

                        //Switches the player's control over to the specified empire and returns and tells the player.
                        NewGalaxyManager.playerID = empire.ID;
                        return "Success. Player has switched over to control the \"" + GeneralHelperMethods.GetEnumText(cultureSelected.ToString()) + "\" empire.";
                    }
                }

                //Returns and informs the player that there is no valid empire of the culture they selected to take control of.
                return "Invalid cheat command argument. There is no valid empire to take control of that is of culture \"" + args[0] + ".\"";
            //Checks if the player entered the "peid" or "play_empire_id" command.
            case "peid":
            case "play_empire_id":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args == null || args.Length != 1)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command takes one argument that is the ID of the empire that the player wishes to switch to controlling.";

                //Converts the first argument string into an int, if not possible then return and tell the user they failed to enter a valid integer.
                int IDSelected = -1;
                if(!Int32.TryParse(args[0], out IDSelected))
                    return "Invalid cheat command argument. \"" + args[0] + "\" is not a valid integer.";

                //Checks if the empire ID selected by the user is a valid empire ID, return and tell the user they failed to select a valid empire ID if not.
                if(IDSelected < 0 || IDSelected >= NewGalaxyManager.empires.Count)
                    return "Invalid cheat command argument. \"" + args[0] + "\" is not a valid empire ID.";

                //Checks if the player is already controlling the empire belonging to the specified empire ID and returns and tells the player if so.
                if(NewGalaxyManager.playerID == IDSelected)
                    return "Player is already controlling the empire belonging to empire ID \"" + IDSelected + ".\"";

                //Switches the player's control over to the empire belonging to the player's specified empire ID and returns and tells the user that their cheat command was successful.
                NewGalaxyManager.playerID = IDSelected;
                return "Success. Player has switched over to control the empire belonging to empire ID \"" + IDSelected + ".\"";
            //Checks if the player entered the "sc" or "set_credits" command.
            case "sc":
            case "set_credits":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args == null || args.Length != 1)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command takes one argument that is a float that expresses the value to which the player's credits should be set to.";

                //Converts the first argument string into a float, if not possible then return and tell the user they failed to enter a valid float.
                float credits = 0;
                if (!float.TryParse(args[0], out credits))
                    return "Invalid cheat command argument. \"" + args[0] + "\" is not a valid float.";

                //Sets the player empire's number of credits to the value specified by the player in the first command argument.
                NewGalaxyManager.playerEmpire.credits = credits;
                //Returns and tells the player their credits were successfully set to the specified value.
                return "Success. The player's empire now has " + NewGalaxyManager.playerEmpire.credits + " credits at its disposal.";
            //Checks if the player entered the "sp" or "set_prescriptions" command.
            case "sp":
            case "set_prescriptions":
                //Checks if the argument count is incorrect and returns and tells the player if so.
                if (args == null || args.Length != 1)
                    return "Invalid cheat command format. The \"" + command + "\" cheat command takes one argument that is a float that expresses the value to which the player's precriptions should be set to.";

                //Converts the first argument string into a float, if not possible then return and tell the user they failed to enter a valid float.
                float prescriptions = 0;
                if (!float.TryParse(args[0], out prescriptions))
                    return "Invalid cheat command argument. \"" + args[0] + "\" is not a valid float.";

                //Sets the player empire's number of prescriptions to the value specified by the player in the first command argument.
                NewGalaxyManager.playerEmpire.prescriptions = prescriptions;
                //Returns and tells the player their prescriptions were successfully set to the specified value.
                return "Success. The player's empire now has " + NewGalaxyManager.playerEmpire.prescriptions + " prescriptions at its disposal.";
        }

        //Informs the player that they entered an invalid command and tells them how to obtain the list of valid cheat commands to enter.
        return "Invalid cheat command entered. Please enter a valid cheat command. Type Help for the list of valid cheat commands.";
    }

    /// <summary>
    /// Public method that should be called in order to clear the cheat console history and destroy all history log objects.
    /// </summary>
    public void ClearHistory()
    {
        for(int historyLogIndex = historyLogParent.childCount - 1; historyLogIndex >= 0; historyLogIndex--)
            Destroy(historyLogParent.GetChild(historyLogIndex).gameObject);
    }
}
