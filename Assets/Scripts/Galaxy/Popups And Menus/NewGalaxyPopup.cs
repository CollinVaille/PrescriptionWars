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

    //Non-inspector variables.
    
    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that is displayed to the player in the header text component at the top of the popup.
    /// </summary>
    public string headerText { get => _headerText.text; set => _headerText.text = value; }

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
    public string bodyText { get => _bodyText.text; set { _bodyText.text = value; gameObject.name = value + " Popup"; } }

    /// <summary>
    /// Public property that should be used in order to access and mutate the string value that indicates the name of the audio clip that will be played once the popup has fully finished its opening animation. The audio clip will be loaded in from the galaxy SFX resources folder.
    /// </summary>
    public string openedSFXName { get => openedSFX == null ? null : openedSFX.name; set => openedSFX = Resources.Load<AudioClip>("Galaxy/SFX/" + value); }

    /// <summary>
    /// Private holder variable for the method that should be called on the popup manager whenever this popup is done closing.
    /// </summary>
    private Action<NewGalaxyPopup> popupManagerOnPopupClosed = null;

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
    public void Initialize(string headerText, string bodySpriteName, string bodyText, string openedSFXName, Action<NewGalaxyPopup> popupManagerOnPopupClosed)
    {
        this.headerText = headerText;
        this.bodySpriteName = bodySpriteName;
        this.bodyText = bodyText;
        this.openedSFXName = openedSFXName;

        this.popupManagerOnPopupClosed = popupManagerOnPopupClosed;

        //Resets the scale of the popup to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localScale = Vector3.one;
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

        this.popupManagerOnPopupClosed = popupManagerOnPopupClosed;

        //Resets the scale of the popup to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localScale = Vector3.one;
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
}

[System.Serializable]
public class NewGalaxyPopupData
{
    public string headerText = null;
    public string bodySpriteName = null;
    public string bodyText = null;
    public string openedSFXName = null;

    public NewGalaxyPopupData(NewGalaxyPopup popup)
    {
        headerText = popup.headerText;
        bodySpriteName = popup.bodySpriteName;
        bodyText = popup.bodyText;
        openedSFXName = popup.openedSFXName;
    }
}
