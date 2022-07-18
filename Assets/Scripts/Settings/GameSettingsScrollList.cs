using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingsScrollList : MonoBehaviour
{
    [Header("SFX Options")]

    [SerializeField] private AudioClip hoverButtonSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Loads all active game settings into the scroll list's options.
    /// </summary>
    private void LoadSettings()
    {
        
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a button and plays the appropriate sound effect.
    /// </summary>
    public void OnPointerEnterButton()
    {
        AudioManager.PlaySFX(hoverButtonSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks the save settings button and saves the game settings and plays the appropriate sound effect for clicking the button.
    /// </summary>
    public void OnClickSaveSettingsButton()
    {
        //Plays the sound effect for clicking a button.
        AudioManager.PlaySFX(clickButtonSFX);

        //Saves all audio settings.
        SaveSettings();
    }

    /// <summary>
    /// Saves all game settings.
    /// </summary>
    private void SaveSettings()
    {
        
    }
}
