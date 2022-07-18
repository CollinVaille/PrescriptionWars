using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GalaxyPauseMenu : MonoBehaviour
{
    [Header("SFX Options")]

    [SerializeField, Tooltip("The sound effect that will be played when a button is clicked.")] private AudioClip clickButtonSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played when the pointer enters or hovers over a button.")] private AudioClip hoverButtonSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played when the pause menu closes.")] private AudioClip closeMenuSFX = null;
    [SerializeField, Tooltip("The sound effect that will be played when the pause menu opens.")] private AudioClip openMenuSFX = null;

    [Header("Text Components")]

    [SerializeField, Tooltip("This text displays the current version of the game as a whole (Ex: Skunk Bomb v0.0.1).")] private Text versionText = null;

    [Header("Other Components")]

    [SerializeField] private GalaxySettingsMenu settingsMenu = null;

    //Non-inspector variables.

    //Indicates whether the pause menu should close due to the player pressing the escape key.
    private bool ShouldCloseDueToEscape
    {
        get
        {
            return Input.GetKeyDown(KeyCode.Escape) && !GalaxyConfirmationPopupBehaviour.IsAGalaxyConfirmationPopupOpen() && (!GalaxySettingsMenu.closedOnFrame && !settingsMenu.gameObject.activeInHierarchy);
        }
    }

    //Static reference of the pause menu.
    public static GalaxyPauseMenu pauseMenu = null;

    /// <summary>
    /// Indicates whether the pause menu is currently active in the hierarchy.
    /// Note: Returns false if the internal static reference to the pause menu is null.
    /// </summary>
    public static bool isOpen { get => pauseMenu != null && pauseMenu.gameObject.activeInHierarchy; }

    private void Awake()
    {
        pauseMenu = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //Updates the text that displays the current version of the game.
        UpdateVersionText();
    }

    // Update is called once per frame
    private void Update()
    {
        //Closes the pause menu if the player is attempting to close it with the escape key.
        if (ShouldCloseDueToEscape)
        {
            Close();
        }
    }

    //Updates the version text to display the current version of the game.
    private void UpdateVersionText()
    {
        versionText.text = "Version: " + Application.version;
    }

    //Closes the menu and resumes the game by deactivating the pause menu's game object.
    public void ClickCloseButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Closes the pause menu.
        Close();
    }

    /// <summary>
    /// Public static method that should be called in order to open the pause menu.
    /// </summary>
    public static void Open()
    {
        if(pauseMenu != null && !pauseMenu.gameObject.activeInHierarchy)
        {
            pauseMenu.gameObject.SetActive(true);
            AudioManager.PlaySFX(pauseMenu.openMenuSFX);
        }
    }

    //Closes the pause menu by deactivating its game object.
    private void Close()
    {
        //Deactivates the pause menu's game object.
        gameObject.SetActive(false);

        //Plays the sound effect for the pause menu closing.
        AudioManager.PlaySFX(closeMenuSFX);
    }

    //This method is called whenever the pointer enters or hovers over a button and plays the appropriate sound effect.
    public void OnPointerEnterButton()
    {
        //Plays the sound effect for the pointer entering or hovering over a button.
        AudioManager.PlaySFX(hoverButtonSFX);
    }

    //Saves the game and plays the appropriate sound effect.
    public void ClickSaveGameButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Saves the games.
        SaveGame();
    }

    //Saves the game.
    private void SaveGame()
    {
        Debug.LogWarning("Save Game Logic For Pause Menu Not Implemented Yet.");
    }

    //Loads the game and plays the appropriate sound effect.
    public void ClickLoadGameButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Loads the game.
        LoadGame();
    }

    //Loads the game.
    private void LoadGame()
    {
        Debug.LogWarning("Load Game Logic For Pause Menu Not Implemented Yet.");
    }

    //Opens the setting menu and plays the appropriate sound effect.
    public void ClickSettingsButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Opens the settings menu.
        OpenSettingsMenu();
    }

    //Opens the settings menu by activating the settings menu's game object.
    private void OpenSettingsMenu()
    {
        settingsMenu.Open();
    }

    //Exits to the main menu and plays the appropriate sound effect.
    public void ClickExitToMenuButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Exits to the main menu.
        StartCoroutine(ConfirmExitToMenu());
    }

    //Confirms that the player wishes to exit to the main menu.
    IEnumerator ConfirmExitToMenu()
    {
        GameObject confirmationPopup = Instantiate(GalaxyDropdownConfirmationPopup.galaxyDropdownConfirmationPopupPrefab);
        GalaxyDropdownConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyDropdownConfirmationPopup>();
        string topText = "Exit to Menu";
        confirmationPopupScript.CreateConfirmationPopup(topText);
        confirmationPopupScript.AddDropdownOption("Exit Without Saving");
        confirmationPopupScript.AddDropdownOption("Save And Exit");
        confirmationPopupScript.SetDropdownOptionSelected("Exit Without Saving");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            if (confirmationPopupScript.GetReturnValue().Equals("Exit Without Saving"))
            {
                ExitToMenu();
            }
            else
            {
                SaveGame();
                ExitToMenu();
            }
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    //Exits to the main menu.
    private void ExitToMenu()
    {
        SceneManager.LoadScene(sceneName: "Main Menu");
    }

    //Exit to the desktop and plays the appropriate sound effect.
    public void ClickExitToDesktopButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Exits to the desktop if the player confirms it.
        StartCoroutine(ConfirmExitToDesktopAction());
    }

    //Confirms that the player wishes to exit to the desktop.
    IEnumerator ConfirmExitToDesktopAction()
    {
        GameObject confirmationPopup = Instantiate(GalaxyDropdownConfirmationPopup.galaxyDropdownConfirmationPopupPrefab);
        GalaxyDropdownConfirmationPopup confirmationPopupScript = confirmationPopup.GetComponent<GalaxyDropdownConfirmationPopup>();
        string topText = "Exit to Desktop";
        confirmationPopupScript.CreateConfirmationPopup(topText);
        confirmationPopupScript.AddDropdownOption("Exit Without Saving");
        confirmationPopupScript.AddDropdownOption("Save And Exit");
        confirmationPopupScript.SetDropdownOptionSelected("Exit Without Saving");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            if(confirmationPopupScript.GetReturnValue().Equals("Exit Without Saving"))
            {
                ExitToDesktop();
            }
            else
            {
                SaveGame();
                ExitToDesktop();
            }
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    //Closes or quits the application and exits to the desktop.
    private void ExitToDesktop()
    {
        Application.Quit();
    }
}
