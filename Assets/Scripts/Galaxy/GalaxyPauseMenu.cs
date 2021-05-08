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

    [Header("Text Components")]

    [SerializeField, Tooltip("This text displays the current version of the game as a whole (Ex: Skunk Bomb v0.0.1).")] private Text versionText = null;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the text that displays the current version of the game.
        UpdateVersionText();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    //Closes the pause menu by deactivating its game object.
    private void Close()
    {
        //Deactivates the pause menu's game object.
        gameObject.SetActive(false);
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

    }

    //Exits to the main menu and plays the appropriate sound effect.
    public void ClickExitToMenuButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Exits to the main menu.
        ExitToMenu();
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
