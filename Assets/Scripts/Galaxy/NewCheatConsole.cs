using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NewCheatConsole : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The input field that the player will use in order to input cheat commands into the cheat console.")] private InputField inputField = null;

    [Header("SFX Options")]

    [SerializeField, Tooltip("The sound effect that will be played whenever the cheat console closes.")] private AudioClip closeSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played whenever the player clicks the close button on the cheat console.")] private AudioClip clickCloseButtonSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played whenever the player hovers over the close button on the cheat console.")] private AudioClip hoverCloseButtonSFX = null;

    //Non-inspector variables.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    /// Public method that should be called by an event trigger whenever the player submits the command text that they've written inside the cheat console's input field.
    /// </summary>
    public void OnSubmitInputField()
    {
        //Splits the player's entered cheat command into its components (index == 0 : command, index > 0 : argument).
        string[] cheatCommandComponents = inputField.text.Split((char[]) null, System.StringSplitOptions.RemoveEmptyEntries);
        //Executes the logic needed for the player's entered cheat command.
        Debug.Log(OnEnterCheatCommand(cheatCommandComponents != null && cheatCommandComponents.Length >= 1 ? cheatCommandComponents[0] : string.Empty, cheatCommandComponents != null && cheatCommandComponents.Length >= 2 ? cheatCommandComponents.Skip(1).ToArray() : null));

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

        //Checks if the player entered the "help" command.
        if (command.Equals("help"))
        {
            if (args != null && args.Length > 0)
                return "Invalid cheat command format. The \"" + command + "\" cheat command does not take any arguments.";
            return "play_empire_culture <culture name> || pec <culture name>";
        }
        //Checks if the player entered the "pec" or "play_empire_culture" command.
        else if (command.Equals("pec") || command.Equals("play_empire_culture"))
        {
            if (args == null || args.Length != 1)
                return "Invalid cheat command format. The \"" + command + "\" cheat command takes one argument that is the name of the imperial culture that the player wishes to switch to the empire of.";
        }

        //Informs the player that they entered an invalid command and tells them how to obtain the list of valid cheat commands to enter.
        return "Invalid cheat command entered. Please enter a valid cheat command. Type Help for the list of valid cheat commands.";
    }
}
