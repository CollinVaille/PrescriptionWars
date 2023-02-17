using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGalaxyPauseMenu : NewGalaxyPopupBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The text component at the bottom of the pause menu that displays the current game version to the user.")] private Text versionText = null;
    [SerializeField, Tooltip("The transform of the game object that serves as the parent of the pause menu's center buttons.")] private Transform centerButtonsParent = null;

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
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the resume button is pressed and plays the sound effect for pressing a center button before resuming the game by closing the pause menu.
    /// </summary>
    public void OnClickResumeButton()
    {
        //Plays the appropriate sound effect for clicking a center button.
        AudioManager.PlaySFX(centerButtonClickSFX);
        //Closes the pause menu and resumes the game.
        Close();
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the exit to menu button is pressed and plays the appropriate sound effect for pressing a center button before launching a confirmation popup where the player can choose to either save and exit, exit without saving, or cancel.
    /// </summary>
    public void OnClickExitToMenuButton()
    {
        //Plays the appropriate sound effect for clicking a center button.
        AudioManager.PlaySFX(centerButtonClickSFX);
    }

    /// <summary>
    /// Public method that should be called by an event trigger whenever the pointer enters a center button and plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterCenterButton()
    {
        //Plays the appropriate sound effect for the pointer entering a center button.
        AudioManager.PlaySFX(centerButtonHighlightedSFX);
    }
}
