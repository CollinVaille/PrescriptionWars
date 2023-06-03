using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalaxyNotification : MonoBehaviour, IPointerClickHandler
{
    [Header("Components")]

    [SerializeField] private Text _text = null;
    [SerializeField] private Image _image = null;

    [Header("SFX Options")]

    [SerializeField] private AudioClip dismissSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used in order to access and modify the string value that is represented in text to the user when they hover over the notification or when the notification is first created.
    /// </summary>
    public string text { get => _text.text; set => _text.text = value; }

    /// <summary>
    /// Public property that should be used in order to access and modify the sprite that is displayed to the user in the notification.
    /// </summary>
    public string spriteName { get => _image.sprite.name; set => _image.sprite = Resources.Load<Sprite>(spritesFolderPath + "/" + value); }

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether or not the notification can be dismissed by the player by right clicking on it.
    /// </summary>
    private bool _isDismissable = false;
    /// <summary>
    /// Public property that should be used in order to both access and modify the boolean value that indicates whether or not the notification can be dismissed by the player by right clicking on it.
    /// </summary>
    public bool isDismissable { get => _isDismissable; set => _isDismissable = value; }

    /// <summary>
    /// Public property that should be accessed in order to obtain the boolean value that indicates whether or not the notification is in the process of dismissing (if it is then it should not be saved in the save game data if applicable).
    /// </summary>
    public bool isDismissing { get; private set; }

    /// <summary>
    /// Public static property that should be accessed in order to obtain the string value that represents the path to the resources folder that contains all of the galaxy notification sprites.
    /// </summary>
    private static string spritesFolderPath { get => "Galaxy/Notifications/Sprites"; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Public method that should be called in order to dismiss the notification. A boolean value can be passed in to specify whether or not the dismissal action is forced regardless of whether or not the notification is dismissable (default is false, which is not forced).
    /// </summary>
    /// <param name="forceDismissal"></param>
    public void Dismiss(bool forceDismissal = false)
    {
        //Returns if the notification is already dismissing or is not dismissable and the dismissal call itself is not forced.
        if ((!forceDismissal && !isDismissable) || isDismissing)
            return;

        //Logs that the notification is being dismissed.
        isDismissing = true;

        //Plays the appropriate sound effect for dismissing a notification.
        AudioManager.PlaySFX(dismissSFX);
    }

    /// <summary>
    /// Public method that is called by the Unity event system using the IPointerClickHandler interface whenever the player clicks on the notification.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        //Left mouse button for opening the popup.
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {

        }
        //Right mouse button for dismissing the notification.
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            //Dismisses the notification if the notification is dismissable.
            Dismiss();
        }
    }
}

[System.Serializable]
public class GalaxyNotificationData
{
    public string text = null;
    public string spriteName = null;
    public bool isDismissable = false;

    public GalaxyNotificationData(GalaxyNotification notification)
    {
        text = notification.text;
        spriteName = notification.spriteName;
        isDismissable = notification.isDismissable;
    }
}