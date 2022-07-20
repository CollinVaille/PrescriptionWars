using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsScrollList : MonoBehaviour
{
    [Header("Scroll List Option Components")]

    [SerializeField] private GameObject interiorHyperspaceLaneStaticColorOption = null;
    [SerializeField] private GameObject borderHyperspaceLaneStaticColorOption = null;

    [Header("Dropdown Components")]

    [SerializeField] private Dropdown interiorHyperspaceLaneColoringModeDropdown = null;
    [SerializeField] private Dropdown borderHyperspaceLaneColoringModeDropdown = null;

    [Header("Image Components")]

    [SerializeField] private Image interiorHyperspaceLaneStaticColorImage = null;
    [SerializeField] private Image borderHyperspaceLaneStaticColorImage = null;

    [Header("Sprite Options")]

    [SerializeField] private Sprite unselectedDropdownOptionSprite = null;
    [SerializeField] private Sprite selectedDropdownOptionSprite = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip hoverButtonSFX = null;
    [SerializeField] private AudioClip clickButtonSFX = null;
    [SerializeField] private AudioClip dropdownOptionHoverSFX = null;
    [SerializeField] private AudioClip dropdownOptionClickSFX = null;

    // Start is called before the first frame update
    void Start()
    {
        //Updates the options of the interior hyperspace lane coloring mode dropdown.
        List<string> interiorHyperspaceLaneColoringModeDropdownOptions = new List<string>();
        for (int enumIndex = 0; enumIndex < Enum.GetNames(typeof(HyperspaceLanesManager.HyperspaceLaneColoringMode)).Length; enumIndex++)
        {
            interiorHyperspaceLaneColoringModeDropdownOptions.Add(GeneralHelperMethods.GetEnumText(((HyperspaceLanesManager.HyperspaceLaneColoringMode)enumIndex).ToString()));
        }
        interiorHyperspaceLaneColoringModeDropdown.AddOptions(interiorHyperspaceLaneColoringModeDropdownOptions);

        //Updates the options of the border hyperspace lane coloring mode dropdown.
        List<string> borderHyperspaceLaneColoringModeDropdownOptions = new List<string>();
        for (int enumIndex = 0; enumIndex < Enum.GetNames(typeof(HyperspaceLanesManager.HyperspaceLaneColoringMode)).Length; enumIndex++)
        {
            borderHyperspaceLaneColoringModeDropdownOptions.Add(GeneralHelperMethods.GetEnumText(((HyperspaceLanesManager.HyperspaceLaneColoringMode)enumIndex).ToString()));
        }
        borderHyperspaceLaneColoringModeDropdown.AddOptions(borderHyperspaceLaneColoringModeDropdownOptions);

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
        interiorHyperspaceLaneColoringModeDropdown.SetValueWithoutNotify((int)GalaxyGameSettings.interiorHyperspaceLaneColoringMode);
        interiorHyperspaceLaneStaticColorOption.SetActive(GalaxyGameSettings.interiorHyperspaceLaneColoringMode == HyperspaceLanesManager.HyperspaceLaneColoringMode.Static);
        interiorHyperspaceLaneStaticColorImage.color = GalaxyGameSettings.interiorHyperspaceLaneStaticColor;
        borderHyperspaceLaneColoringModeDropdown.SetValueWithoutNotify((int)GalaxyGameSettings.borderHyperspaceLaneColoringMode);
        borderHyperspaceLaneStaticColorOption.SetActive(GalaxyGameSettings.borderHyperspaceLaneColoringMode == HyperspaceLanesManager.HyperspaceLaneColoringMode.Static);
        borderHyperspaceLaneStaticColorImage.color = GalaxyGameSettings.borderHyperspaceLaneStaticColor;
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
        GalaxyGameSettings.SaveSettings();
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the interior hyperspace lane coloring mode dropdown changes and updates the coloring mode of all interior hyperspace lanes in the galaxy.
    /// </summary>
    public void OnInteriorHyperspaceLaneColoringModeDropdownValueChange()
    {
        GalaxyGameSettings.interiorHyperspaceLaneColoringMode = (HyperspaceLanesManager.HyperspaceLaneColoringMode)interiorHyperspaceLaneColoringModeDropdown.value;
        interiorHyperspaceLaneStaticColorOption.SetActive((HyperspaceLanesManager.HyperspaceLaneColoringMode)interiorHyperspaceLaneColoringModeDropdown.value == HyperspaceLanesManager.HyperspaceLaneColoringMode.Static);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the value of the border hyperspace lane coloring mode dropdown changes and updates the coloring mode of all border hyperspace lanes in the galaxy.
    /// </summary>
    public void OnBorderHyperspaceLaneColoringModeDropdownValueChange()
    {
        GalaxyGameSettings.borderHyperspaceLaneColoringMode = (HyperspaceLanesManager.HyperspaceLaneColoringMode)borderHyperspaceLaneColoringModeDropdown.value;
        borderHyperspaceLaneStaticColorOption.SetActive((HyperspaceLanesManager.HyperspaceLaneColoringMode)borderHyperspaceLaneColoringModeDropdown.value == HyperspaceLanesManager.HyperspaceLaneColoringMode.Static);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks on the interior hyperspace lane static color picker button and launches the color picker confirmation popup.
    /// </summary>
    public void OnClickInteriorHyperspaceLaneStaticColorPickerButton()
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(clickButtonSFX);

        //Starts the coroutine.
        StartCoroutine(ConfirmChangingInteriorHyperspaceLaneStaticColorActionCoroutine());
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks on the border hyperspace lane static color picker button and launches the color picker confirmation popup.
    /// </summary>
    public void OnClickBorderHyperspaceLaneStaticColorPickerButton()
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(clickButtonSFX);

        //Starts the coroutine.
        StartCoroutine(ConfirmChangingBorderHyperspaceLaneStaticColorActionCoroutine());
    }

    /// <summary>
    /// This method should be called by starting a coroutine in the OnClickInteriorHyperspaceLaneStaticColorPickerButton method and confirms that the player wants to pick an interior hyperspace lane static color and what color they want to pick.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmChangingInteriorHyperspaceLaneStaticColorActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyColorPickerConfirmationPopup confirmationPopupScript = Instantiate(GalaxyColorPickerConfirmationPopup.galaxyColorPickerConfirmationPopupPrefab).GetComponent<GalaxyColorPickerConfirmationPopup>();
        string topText = "Change Interior Hyperspace Lane Static Color";
        string bodyText = "Are you sure that you want to change the color of all interior hyperspace lanes to the color that you are currently picking?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, GalaxyGameSettings.interiorHyperspaceLaneStaticColor);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            GalaxyGameSettings.interiorHyperspaceLaneStaticColor = confirmationPopupScript.ColorSelected;
            interiorHyperspaceLaneStaticColorImage.color = GalaxyGameSettings.interiorHyperspaceLaneStaticColor;
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method should be called by starting a coroutine in the OnClickBorderHyperspaceLaneStaticColorPickerButton method and confirms that the player wants to pick a border hyperspace lane static color and what color they want to pick.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ConfirmChangingBorderHyperspaceLaneStaticColorActionCoroutine()
    {
        //Creates the confirmation popup.
        GalaxyColorPickerConfirmationPopup confirmationPopupScript = Instantiate(GalaxyColorPickerConfirmationPopup.galaxyColorPickerConfirmationPopupPrefab).GetComponent<GalaxyColorPickerConfirmationPopup>();
        string topText = "Change Border Hyperspace Lane Static Color";
        string bodyText = "Are you sure that you want to change the color of all border hyperspace lanes to the color that you are currently picking?";
        confirmationPopupScript.CreateConfirmationPopup(topText, bodyText, GalaxyGameSettings.borderHyperspaceLaneStaticColor);

        //Waits until the player has confirmed or cancelled the action.
        yield return new WaitUntil(confirmationPopupScript.IsAnswered);

        //If the player confirms their action, it carries out the logic behind it.
        if (confirmationPopupScript.GetAnswer() == GalaxyConfirmationPopupBehaviour.GalaxyConfirmationPopupAnswer.Confirm)
        {
            GalaxyGameSettings.borderHyperspaceLaneStaticColor = confirmationPopupScript.ColorSelected;
            borderHyperspaceLaneStaticColorImage.color = GalaxyGameSettings.borderHyperspaceLaneStaticColor;
        }

        //Destroys the confirmation popup.
        confirmationPopupScript.DestroyConfirmationPopup();
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a dropdown option and does the tasks of playing the appropriate sound effect and updating the background image's sprite.
    /// </summary>
    public void OnPointerEnterDropdownOption(Image backgroundImage)
    {
        //Updates the dropdown option's background image sprite.
        backgroundImage.sprite = selectedDropdownOptionSprite;

        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(dropdownOptionHoverSFX);
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer exits a dropdown option and updates the background image's sprite.
    /// </summary>
    public void OnPointerExitDropdownOption(Image backgroundImage)
    {
        backgroundImage.sprite = unselectedDropdownOptionSprite;
    }

    /// <summary>
    /// This method is called through an event trigger whenever the player clicks on a dropdown option and plays the appropriate sound effect.
    /// </summary>
    public void OnClickDropdownOption()
    {
        AudioManager.PlaySFX(dropdownOptionClickSFX);
    }
}
