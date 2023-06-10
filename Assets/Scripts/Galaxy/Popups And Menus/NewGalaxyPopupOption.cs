using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class NewGalaxyPopupOption : MonoBehaviour, IPointerEnterHandler
{
    [Header("Components")]

    [SerializeField] private Text _mainText = null;
    [SerializeField] private GalaxyTooltip _tooltip = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip pointerEnterSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used in order to access and mutate the main text that is being displayed on the popup option.
    /// </summary>
    public string mainText { get => _mainText.text; set => _mainText.text = value; }

    /// <summary>
    /// Public property that should be used in order to access and mutate the text that is displayed via a tooltip whenever the player hovers over the popup option.
    /// </summary>
    public string tooltipText { get => _tooltip.Text; set => _tooltip.Text = value; }

    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that represents the name of the audio clip that is loaded in from the project resources and played whenever the player clicks the popup option button.
    /// </summary>
    public string clickSFXName { get; set; } = null;
    /// <summary>
    /// Public property that should be used in order to access the audio clip that is loaded in from the project resources and played whenever the player clicks the popup option button.
    /// </summary>
    public AudioClip clickSFX { get => Resources.Load<AudioClip>("Galaxy/SFX/" + clickSFXName); }

    /// <summary>
    /// Private holder variable for the dictionary that contains global action ID integer value keys tied to a string array of arguments to be passed in when calling the global action.
    /// </summary>
    private Dictionary<int, string[]> _globalActions = null;
    /// <summary>
    /// Public property that should be used in order to access the dictionary that contains global action ID integer value keys tied to a string array of arguments to be passed in when calling the global action.
    /// </summary>
    public Dictionary<int, string[]> globalActions { get => _globalActions; }

    /// <summary>
    /// Private holder variable for the action that is executed on the popup whenever the popup option is clicked.
    /// </summary>
    private Action<NewGalaxyPopupOption> popupOnOptionClicked = null;

    /// <summary>
    /// Public static property that should be used in order to access the game object that serves as the prefab that all popup options are instantiated from.
    /// </summary>
    public static GameObject prefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Popups/Popup Option"); }

    /// <summary>
    /// Public method that should be called in order to initialize a new popup option with the needed values.
    /// </summary>
    /// <param name="mainText"></param>
    /// <param name="tooltipText"></param>
    /// <param name="clickSFXName"></param>
    /// <param name="globalActions"></param>
    public void Initialize(string mainText, string tooltipText, string clickSFXName, Dictionary<int, string[]> globalActions, Action<NewGalaxyPopupOption> popupOnOptionClicked)
    {
        this.mainText = mainText;
        this.tooltipText = tooltipText;
        this.clickSFXName = clickSFXName;
        _globalActions = globalActions;

        this.popupOnOptionClicked = popupOnOptionClicked;

        //Resets the scale of the popup option to avoid any Unity instantiation shenanigans.
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Public method that should be called in order to initialize a new popup option with the needed values from saved popup option data.
    /// </summary>
    /// <param name="popupOptionData"></param>
    public void Initialize(NewGalaxyPopupOptionData popupOptionData, Action<NewGalaxyPopupOption> popupOnOptionClicked)
    {
        mainText = popupOptionData.mainText;
        tooltipText = popupOptionData.tooltipText;
        clickSFXName = popupOptionData.clickSFXName;
        _globalActions = popupOptionData.globalActions;

        this.popupOnOptionClicked = popupOnOptionClicked;

        //Resets the scale of the popup option to avoid any Unity instantiation shenanigans.
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Public method that should be called whenever the player clicks on the popup option and plays the appropriate sound effect before executing the global actions with the correct arguments and closing the popup.
    /// </summary>
    public void OnPointerClick()
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(clickSFX);

        //Executes the popup's global actions so that clicking on the option may actually have an effect on the game.
        ExecuteGlobalActions();

        //Executes the needed logic on the popup itself for an option being clicked.
        popupOnOptionClicked(this);
    }

    /// <summary>
    /// Public method that should be called by the IPointerEnterHandler interface whenever the player hovers over the option button and plays the appropriate sound effect.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(pointerEnterSFX);
    }

    /// <summary>
    /// Private method that should be called by the OnPointerClick method whenever the player clicks on the option and executes all of the option's global actions.
    /// </summary>
    private void ExecuteGlobalActions()
    {
        if (_globalActions != null)
            foreach (int globalActionID in _globalActions.Keys)
                if (NewGalaxyManager.GetGlobalAction(globalActionID) != null)
                    NewGalaxyManager.GetGlobalAction(globalActionID)(_globalActions[globalActionID]);
    }
}

[System.Serializable]
public class NewGalaxyPopupOptionData
{
    public string mainText = null;
    public string tooltipText = null;
    public string clickSFXName = null;
    public Dictionary<int, string[]> globalActions = null;

    public NewGalaxyPopupOptionData(NewGalaxyPopupOption popupOption)
    {
        mainText = popupOption.mainText;
        tooltipText = popupOption.tooltipText;
        clickSFXName = popupOption.clickSFXName;
        globalActions = popupOption.globalActions;
    }

    public NewGalaxyPopupOptionData(string mainText, string tooltipText, string clickSFXName, Dictionary<int, string[]> globalActions)
    {
        this.mainText = mainText;
        this.tooltipText = tooltipText;
        this.clickSFXName = clickSFXName;
        this.globalActions = globalActions;
    }
}
