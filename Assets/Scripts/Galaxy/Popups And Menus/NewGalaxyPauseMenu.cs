using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewGalaxyPauseMenu : NewGalaxyPopupBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The text component at the bottom of the pause menu that displays the current game version to the user.")] private Text versionText = null;
    [SerializeField, Tooltip("The transform of the game object that serves as the parent of the pause menu's center buttons.")] private Transform centerButtonsParent = null;
    [SerializeField, Tooltip("The center button that the player can click on in order to save the game as an inputted save name.")] private Button saveAsButton = null;

    [Header("SFX Options")]

    [SerializeField, Tooltip("The sound effect that should be played whenever a center button (the resume button for example) is clicked.")] private AudioClip centerButtonClickSFX = null;
    [SerializeField, Tooltip("The sound effect that should be played whenever the pointer enters a center button.")] private AudioClip centerButtonHighlightedSFX = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Sets the version text.
        versionText.text = "Version: " + Application.version;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Open()
    {
        base.Open();

        //Resets each center button back to being unselected.
        for(int centerButtonSiblingIndex = 0; centerButtonSiblingIndex < centerButtonsParent.childCount; centerButtonSiblingIndex++)
            centerButtonsParent.GetChild(centerButtonSiblingIndex).GetComponent<Button>().OnDeselect(null);

        //Makes the save as button interactable if ironpill mode is disabled and not interactable if ironpill mode is enabled.
        saveAsButton.interactable = !NewGalaxyManager.ironPillModeEnabled;
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the settings button is pressed and plays the appropriate sound effect for pressing a center button before launching the settings menu.
    /// </summary>
    public void OnClickSettingsButton()
    {
        //Plays the appropriate sound effect for clicking a center button.
        AudioManager.PlaySFX(centerButtonClickSFX);

        //Opens up the settings menu if its not already open or opening.
        if(!NewGalaxyManager.settingsMenu.open && !NewGalaxyManager.settingsMenu.opening)
            NewGalaxyManager.settingsMenu.Open();
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the pointer enters a center button and plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterCenterButton()
    {
        //Plays the appropriate sound effect for the pointer entering a center button.
        AudioManager.PlaySFX(centerButtonHighlightedSFX);
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the exit to menu button is pressed and plays the appropriate sound effect for pressing a center button before launching a confirmation popup where the player can choose to either save and exit, exit without saving, or cancel.
    /// </summary>
    public void OnClickExitToMenuButton()
    {
        //Plays the appropriate sound effect for clicking a center button.
        AudioManager.PlaySFX(centerButtonClickSFX);

        //Exits to the main menu.
        StartCoroutine(ConfirmExitToMenu());
    }

    /// <summary>
    /// Private coroutine that confirms that the player wants to exit to the main menu and also asks them whether they would like to save their game.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmExitToMenu()
    {
        GalaxyDropdownConfirmationPopup confirmationPopupScript = Instantiate(GalaxyDropdownConfirmationPopup.dropdownConfirmationPopupPrefab).GetComponent<GalaxyDropdownConfirmationPopup>();
        string topText = "Exit to Menu";
        confirmationPopupScript.CreateConfirmationPopup(topText);
        confirmationPopupScript.AddDropdownOption("Exit Without Saving");
        confirmationPopupScript.AddDropdownOption("Save And Exit");
        confirmationPopupScript.SetDropdownOptionSelected("Exit Without Saving");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            if (confirmationPopupScript.returnValue.Equals("Exit Without Saving"))
            {
                ExitToMenu();
            }
            else
            {
                GalaxySaveSystem.SaveGalaxy();
                ExitToMenu();
            }
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Private method that should be called in order to exit to the main menu.
    /// </summary>
    private void ExitToMenu()
    {
        SceneManager.LoadScene(sceneName: "Main Menu");
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the exit to menu button is pressed and plays the appropriate sound effect for pressing a center button before launching a confirmation popup where the player can choose to either save and exit, exit without saving, or cancel.
    /// </summary>
    public void OnClickExitToDesktopButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(centerButtonClickSFX);

        //Exits to the desktop if the player confirms it.
        StartCoroutine(ConfirmExitToDesktopAction());
    }

    /// <summary>
    /// Private coroutine that confirms that the player wants to exit to the desktop and also asks them whether they would like to save their game.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmExitToDesktopAction()
    {
        GalaxyDropdownConfirmationPopup confirmationPopupScript = Instantiate(GalaxyDropdownConfirmationPopup.dropdownConfirmationPopupPrefab).GetComponent<GalaxyDropdownConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Exit to Desktop");
        confirmationPopupScript.AddDropdownOption("Exit Without Saving");
        confirmationPopupScript.AddDropdownOption("Save And Exit");
        confirmationPopupScript.SetDropdownOptionSelected("Exit Without Saving");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            if(confirmationPopupScript.returnValue.Equals("Exit Without Saving"))
            {
                ExitToDesktop();
            }
            else
            {
                GalaxySaveSystem.SaveGalaxy();
                ExitToDesktop();
            }
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Public method that should be called by an event trigger in the inspector whenever the save button is pressed and plays the appropriate sound effect for pressing a center button before launching a confirmation popup to confirm that the player wants to overwrite the current save file (if applicable) before then confirming that the player acknowledges they've saved the game.
    /// </summary>
    public void OnClickSaveButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(centerButtonClickSFX);

        if (GalaxySaveSystem.SaveExists(NewGalaxyManager.saveName))
            StartCoroutine(ConfirmOverwritingSaveFileAction());
        else
        {
            GalaxySaveSystem.SaveGalaxy();
            StartCoroutine(ConfirmAcknowledgementOfSavingAction());
        }
    }

    /// <summary>
    /// Private coroutine that confirms that the player wants to overwrite the current save file before then launching another confirmation popup that confirms that the player acknowledges that they've saved the game if they confirm the overwriting action.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmOverwritingSaveFileAction()
    {
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Overwrite Existing Save", "There is already a save file named \"" + NewGalaxyManager.saveName + "\" would you like to continue saving and overwrite this existing save file?");

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            GalaxySaveSystem.SaveGalaxy();
            StartCoroutine(ConfirmAcknowledgementOfSavingAction());
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Private coroutine that confirms that the player acknowledges that their game has been saved successfully.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmAcknowledgementOfSavingAction()
    {
        GalaxyConfirmationPopup confirmationPopupScript = Instantiate(GalaxyConfirmationPopup.confirmationPopupPrefab).GetComponent<GalaxyConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Game Saved", "Your game has been saved successfully.", true);

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Public method that should be called by an event trigger in the inspector whenever the save as button is clicked and it launches an input field confirmation popup that prompts the player to enter a save name before then attempting to save the game under that name.
    /// </summary>
    public void OnClickSaveAsButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(centerButtonClickSFX);

        //Starts the save as confirming coroutine.
        StartCoroutine(ConfirmSaveAsAction());
    }

    /// <summary>
    /// Private coroutine that confirms that the player wants to save the game under the name specified by them in the confirmation popup's input field before then confirming that the player wants to overwrite the existing save with that name if necessary before then launching another confirmation popup that confirms that the player acknowledges that their game has saved successfully.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmSaveAsAction()
    {
        GalaxyInputFieldConfirmationPopup confirmationPopupScript = Instantiate(GalaxyInputFieldConfirmationPopup.inputFieldConfirmationPopupPrefab).GetComponent<GalaxyInputFieldConfirmationPopup>();
        confirmationPopupScript.CreateConfirmationPopup("Save As");
        confirmationPopupScript.SetPlaceHolderText("Enter save name...");
        confirmationPopupScript.inputFieldText = NewGalaxyManager.saveName;
        confirmationPopupScript.specialCharactersAllowed = false;

        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        if (confirmationPopupScript.answer == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            NewGalaxyManager.saveName = confirmationPopupScript.inputFieldText;
            if (GalaxySaveSystem.SaveExists(NewGalaxyManager.saveName))
                StartCoroutine(ConfirmOverwritingSaveFileAction());
            else
            {
                GalaxySaveSystem.SaveGalaxy();
                StartCoroutine(ConfirmAcknowledgementOfSavingAction());
            }
        }

        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// Private method that should be called in order to close/quit the application and exit to the desktop.
    /// </summary>
    private void ExitToDesktop()
    {
        Application.Quit();
    }
}
