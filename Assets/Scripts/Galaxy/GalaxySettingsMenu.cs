using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxySettingsMenu : GalaxyPopupBehaviour
{
    public enum SettingsMode
    {
        Game,
        Video,
        Audio
    }

    [Header("Components")]

    [SerializeField] private List<Button> settingsModeButtons = new List<Button>();
    [SerializeField] private List<GameObject> settingsModeScrollLists = new List<GameObject>();

    [Header("Coloring Options")]

    [SerializeField] private Color unselectedButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = Color.black;

    [Header("SFX Options")]

    [SerializeField] private AudioClip selectSettingsModeSFX = null;
    [SerializeField] private AudioClip hoverSettingsModeButtonSFX = null;

    /// <summary>
    /// Read only property that indicates whether the settings menu has been closed on the current frame.
    /// </summary>
    public static bool closedOnFrame { get => closedOnFrameVar; private set => closedOnFrameVar = value; }
    private static bool closedOnFrameVar = false;

    /// <summary>
    /// Property that should be used to both access and set the settings mode of the settings menu (Game, Video, or Audio).
    /// </summary>
    public SettingsMode settingsMode
    {
        get => settingsModeVar;
        set
        {
            settingsModeButtons[(int)settingsModeVar].image.color = unselectedButtonColor;
            settingsModeScrollLists[(int)settingsModeVar].SetActive(false);
            settingsModeVar = value;
            settingsModeButtons[(int)value].image.color = selectedButtonColor;
            settingsModeScrollLists[(int)value].SetActive(true);
        }
    }
    private SettingsMode settingsModeVar = SettingsMode.Game;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Sets the variable that indicates whether the settings menu has been closed on the current frame to false (essentially resetting it).
    /// </summary>
    public static void ResetClosedOnFrameBool()
    {
        closedOnFrame = false;
    }

    public override void Close()
    {
        //Executes base popup behaviour closing logic.
        base.Close();

        //Logs that the settings menu has been closed on the current frame.
        closedOnFrame = true;
    }

    public override void Open()
    {
        base.Open();

        //Resets the settings mode of the menu.
        settingsMode = 0;

        //SetDraggable(!GalaxyPauseMenu.IsOpen);
    }

    /// <summary>
    /// This method is called through an event trigger whenever a settings mode button is pressed and sets the settings mode to the specified value.
    /// </summary>
    /// <param name="newSettingsMode"></param>
    public void OnClickSettingsModeButton(int newSettingsMode)
    {
        if(settingsMode != (SettingsMode)newSettingsMode)
        {
            settingsMode = (SettingsMode)newSettingsMode;
            AudioManager.PlaySFX(selectSettingsModeSFX);
        }
    }

    /// <summary>
    /// This method is called through an event trigger whenever the pointer enters a settings mode button (game, video, or audio) and plays a sound effect.
    /// </summary>
    public void OnPointerEnterSettingsModeButton()
    {
        AudioManager.PlaySFX(hoverSettingsModeButtonSFX);
    }
}
