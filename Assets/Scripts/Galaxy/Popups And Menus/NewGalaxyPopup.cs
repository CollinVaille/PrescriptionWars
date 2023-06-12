using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NewGalaxyPopup : NewGalaxyPopupBehaviour
{
    [Header("Components")]

    [SerializeField] private Text _headerText = null;
    [SerializeField] private Image _bodyImage = null;
    [SerializeField] private Text _bodyText = null;
    [SerializeField] private Transform optionsParent = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip clickOptionButtonSFX = null;

    //Non-inspector variables.
    
    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that is displayed to the player in the header text component at the top of the popup.
    /// </summary>
    public string headerText { get => _headerText.text; set { _headerText.text = value; gameObject.name = value + " Popup"; } }

    /// <summary>
    /// Private holder variable for the string value that indicates the name of the sprite that will be applied to the body image of the popup. The name of the sprite is needed when loading in the sprite from the correct resources folder.
    /// </summary>
    private string _bodySpriteName = null;
    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that indicates the name of the sprite that will be applied to the body image of the popup. The name of the sprite is needed when loading in the sprite from the correct resources folder.
    /// </summary>
    public string bodySpriteName
    {
        get => _bodySpriteName;
        set
        {
            _bodySpriteName = value;
            _bodyImage.sprite = Resources.Load<Sprite>(bodySpritesFolderPath + "/" + _bodySpriteName);
        }
    }

    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that is displayed to the player in the body text component in the middle on the popup.
    /// </summary>
    public string bodyText { get => _bodyText.text; set => _bodyText.text = value; }

    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that indicates the name of the audio clip that will be played once the popup has fully finished its opening animation. The audio clip will be loaded in from the galaxy SFX resources folder.
    /// </summary>
    public string openedSFXName { get => openedSFX == null ? null : openedSFX.name; set => openedSFX = value == null || value.Equals(string.Empty) ? null : Resources.Load<AudioClip>("Galaxy/SFX/" + value); }

    /// <summary>
    /// Private holder variable for the method that should be called on the popup manager whenever this popup is done closing.
    /// </summary>
    private Action<NewGalaxyPopup> popupManagerOnPopupClosed = null;

    /// <summary>
    /// Private holder variable for the list of options that the player can click on to answer the popup.
    /// </summary>
    private List<NewGalaxyPopupOption> options = null;
    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates how many options there are for the player to click on to answer the popup.
    /// </summary>
    public int optionCount { get => options == null ? 0 : options.Count; }

    /// <summary>
    /// Private static property that should be used in order to access the string value that is the path to the resources folder that contains all of the popup body image sprites.
    /// </summary>
    private static string bodySpritesFolderPath { get => "Galaxy/Popup Body Sprites"; }

    /// <summary>
    /// Public property that should be used in order to access the game object that all popups in the galaxy view are instantiated from.
    /// </summary>
    public static GameObject prefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Popups/Popup"); }

    /// <summary>
    /// Public method that should be called in order to initialize the popup with the values needed in order to function as intended.
    /// </summary>
    /// <param name="headerText"></param>
    /// <param name="bodySpriteName"></param>
    /// <param name="bodyText"></param>
    /// <param name="popupManagerOnPopupClosed"></param>
    public void Initialize(string headerText, string bodySpriteName, string bodyText, string openedSFXName, List<NewGalaxyPopupOptionData> options, Action<NewGalaxyPopup> popupManagerOnPopupClosed)
    {
        this.headerText = headerText;
        this.bodySpriteName = bodySpriteName;
        this.bodyText = bodyText;
        this.openedSFXName = openedSFXName;
        if (options != null)
            foreach (NewGalaxyPopupOptionData optionData in options)
                AddOption(optionData);

        this.popupManagerOnPopupClosed = popupManagerOnPopupClosed;

        //Resets the scale and position of the popup to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Public method that should be called in order to initialize the popup with the values needed in order to function as intended. This version of the function is initialized using popup save data.
    /// </summary>
    /// <param name="popupData"></param>
    /// <param name="popupManagerOnPopupClosed"></param>
    public void Initialize(NewGalaxyPopupData popupData, Action<NewGalaxyPopup> popupManagerOnPopupClosed)
    {
        headerText = popupData.headerText;
        bodySpriteName = popupData.bodySpriteName;
        bodyText = popupData.bodyText;
        openedSFXName = popupData.openedSFXName;
        if (popupData.optionData != null)
            foreach (NewGalaxyPopupOptionData optionData in popupData.optionData)
                AddOption(optionData);

        this.popupManagerOnPopupClosed = popupManagerOnPopupClosed;

        //Resets the scale and position of the popup to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localPosition = Vector3.zero;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void EndClosingAnimation()
    {
        //Informs the popup manager that this popup has finished its closing animation and is about to be fully closed.
        popupManagerOnPopupClosed(this);

        //Executes the base end closing animation behaviour (which will destroy the popup).
        base.EndClosingAnimation();
    }

    /// <summary>
    /// Public method that returns the option at the specified index in the private list of popup options.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public NewGalaxyPopupOption GetOption(int index)
    {
        if (options == null || options.Count == 0 || index < 0 || index >= optionCount)
            return null;

        return options[index];
    }

    /// <summary>
    /// Public method that should be called in order to add a new option to the list of options that the player can click on to answer the popup.
    /// </summary>
    /// <param name="optionData"></param>
    public void AddOption(NewGalaxyPopupOptionData optionData)
    {
        //Checks if the specified option data is null and returns if so.
        if (optionData == null)
            return;

        //Checks if the options list has not yet been initialized and initializes it if so.
        if (options == null)
            options = new List<NewGalaxyPopupOption>();
        //Instantiates a new option from the option prefab and adds its option script to the list of options for this popup.
        options.Add(Instantiate(NewGalaxyPopupOption.prefab).GetComponent<NewGalaxyPopupOption>());
        //Sets the parent of the popup option.
        options[optionCount - 1].transform.SetParent(optionsParent);
        //Initializes the popup option with the value needed in order to function properly.
        options[optionCount - 1].Initialize(optionData, OnOptionClicked);
    }

    /// <summary>
    /// Private method that should be called by an option whenever it is clicked by the player. The method is passed in to the option when it is initialized.
    /// </summary>
    /// <param name="option"></param>
    private void OnOptionClicked(NewGalaxyPopupOption option)
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(clickOptionButtonSFX);

        //Closes the popup.
        Close();
    }
}

[System.Serializable]
public class NewGalaxyPopupData
{
    public string headerText = null;
    public string bodySpriteName = null;
    public string bodyText = null;
    public string openedSFXName = null;
    public List<NewGalaxyPopupOptionData> optionData = null;

    public NewGalaxyPopupData(NewGalaxyPopup popup)
    {
        headerText = popup.headerText;
        bodySpriteName = popup.bodySpriteName;
        bodyText = popup.bodyText;
        openedSFXName = popup.openedSFXName;
        optionData = null;
        if(popup.optionCount <= 0)
        {
            optionData = new List<NewGalaxyPopupOptionData>();
            for (int optionIndex = 0; optionIndex < popup.optionCount; optionIndex++)
                optionData.Add(new NewGalaxyPopupOptionData(popup.GetOption(optionIndex)));
        }
    }

    public NewGalaxyPopupData(string headerText, string bodySpriteName, string bodyText, string openedSFXName, List<NewGalaxyPopupOptionData> optionData)
    {
        this.headerText = headerText;
        this.bodySpriteName = bodySpriteName;
        this.bodyText = bodyText;
        this.openedSFXName = openedSFXName;
        this.optionData = optionData;
    }
}
