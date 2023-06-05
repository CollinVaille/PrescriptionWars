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

    [SerializeField] private AudioClip successfulDismissSFX = null;
    [SerializeField] private AudioClip unsuccessfulDismissSFX = null;

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
    private bool _isDismissable = true;
    /// <summary>
    /// Public property that should be used in order to both access and modify the boolean value that indicates whether or not the notification can be dismissed by the player by right clicking on it.
    /// </summary>
    public bool isDismissable { get => _isDismissable; set => _isDismissable = value; }

    /// <summary>
    /// Public property that should be accessed in order to obtain the boolean value that indicates whether or not the notification is in the process of dismissing (if it is then it should not be saved in the save game data if applicable).
    /// </summary>
    public bool isDismissing { get; private set; }

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether or not the notificiation is a warning notification that flashes to warn the player of something.
    /// </summary>
    private bool _isWarning = false;
    /// <summary>
    /// Public property that should be used in order to access and modify the boolean value that indicates whether or not the notificiation is a warning notification that flashes to warn the player of something.
    /// </summary>
    public bool isWarning
    {
        get => _isWarning;
        set => _isWarning = value;
    }

    /// <summary>
    /// Private holder variable for the boolean value that indicates whether or not the notification is moving downwards into its assigned position.
    /// </summary>
    private bool _isMovingDownwards = false;
    /// <summary>
    /// Public property that should be used in order to access and modify the boolean value that indicates whether or not the notification is moving downwards into its assigned position.
    /// </summary>
    public bool isMovingDownwards
    {
        get => _isMovingDownwards;
        set
        {
            //Calculates the assigned local y position and assigns the value to the appropriate variable.
            assignedLocalYPosition = transform.GetSiblingIndex() * (((RectTransform)transform).sizeDelta.y + NewGalaxyManager.notificationManager.spacing);

            //Checks if the notification is already moving downwards and has not yet reached its assigned position and returns if so.
            if (_isMovingDownwards && transform.localPosition.y > assignedLocalYPosition)
                return;

            //Sets the boolean value that indicates whether or not the notification is moving downwards into its assigned position to the specified value.
            _isMovingDownwards = value;
        }
    }

    /// <summary>
    /// Private holder variable for the float value that represents the local y position the notification is assigned to go to if it is moving downwards. May not be set properly if not having ever moved downwards.
    /// </summary>
    private float assignedLocalYPosition = 0;

    /// <summary>
    /// Public static property that should be accessed in order to obtain the string value that represents the path to the resources folder that contains all of the galaxy notification sprites.
    /// </summary>
    private static string spritesFolderPath { get => "Galaxy/Notifications/Sprites"; }

    /// <summary>
    /// Public static method that should be used in order to access the prefab that all notifications are instantiated from.
    /// </summary>
    public static GameObject prefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Notifications/Notification"); }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Deals with the notification moving downwards towards its assigned position if needed.
        if (isMovingDownwards)
        {
            //Moves the notification downwards at the correct speed.
            transform.Translate(0, -1 * NewGalaxyManager.notificationManager.downwardsMovementSpeed * Time.deltaTime, 0, Space.Self);
            //Checks if the notification is at or passed its assigned position and resets its position to its assigned position and logs that the notification is no longer moving downwards if so.
            if (transform.localPosition.y <= assignedLocalYPosition)
            {
                transform.localPosition = new Vector3(0, assignedLocalYPosition, 0);
                isMovingDownwards = false;
            }
        }
    }

    /// <summary>
    /// Public method that should be called in order to initialize the notification with the values needed in order to function as intended.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="spriteName"></param>
    /// <param name="isDismissable"></param>
    /// <param name="isWarning"></param>
    public void Initialize(string text, string spriteName, int notificationIndex, bool isDismissable = true, bool isWarning = false)
    {
        this.text = text;
        this.spriteName = spriteName;
        this.isDismissable = isDismissable;
        this.isWarning = isWarning;

        //Resets the scale of the notification to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localScale = Vector3.one;

        //Sets the position of the notification to be either right above the top of the screen or right above the previous notification if it is too close or above the top of the screen.
        float topLocalYPosition = ((RectTransform)NewGalaxyManager.notificationManager.transform).sizeDelta.y;
        GalaxyNotification previousNotification = NewGalaxyManager.notificationManager.GetNotificationAt(notificationIndex - 1);
        transform.localPosition = new Vector3(0, previousNotification != null && previousNotification.transform.localPosition.y >= topLocalYPosition - (((RectTransform)previousNotification.transform).sizeDelta.y + NewGalaxyManager.notificationManager.spacing) ? previousNotification.transform.localPosition.y + ((RectTransform)previousNotification.transform).sizeDelta.y + NewGalaxyManager.notificationManager.spacing : topLocalYPosition, 0);

        //Logs that the notification is moving downwards towards its assigned position.
        isMovingDownwards = true;
    }

    /// <summary>
    /// Public method that should be called in order to initialize the notification with the values needed in order to function as intended. This version of the function is initialized using notification save data.
    /// </summary>
    /// <param name="notificationData"></param>
    public void Initialize(GalaxyNotificationData notificationData, int notificationIndex)
    {
        text = notificationData.text;
        spriteName = notificationData.spriteName;
        isDismissable = notificationData.isDismissable;
        isWarning = notificationData.isWarning;

        //Resets the scale of the notification to fix any Unity prefab instantiation and parenting shenanigans.
        transform.localScale = Vector3.one;

        //Sets the position of the notification to its correct location instantly (instead of falling from the top of the screen like notifications created mid-game).
        GalaxyNotification previousNotification = NewGalaxyManager.notificationManager.GetNotificationAt(notificationIndex - 1);
        transform.localPosition = new Vector3(0, previousNotification != null ? previousNotification.transform.localPosition.y + ((RectTransform)previousNotification.transform).sizeDelta.y + NewGalaxyManager.notificationManager.spacing : 0, 0);

        //Logs that the notification is not moving downwards.
        isMovingDownwards = false;
    }

    /// <summary>
    /// Public method that should be called in order to dismiss the notification. A boolean value can be passed in to specify whether or not the dismissal action is forced regardless of whether or not the notification is dismissable (default is false, which is not forced).
    /// </summary>
    /// <param name="forceDismissal"></param>
    public void Dismiss(bool forceDismissal = false)
    {
        //Returns if the notification is already dismissing or is not dismissable and the dismissal call itself is not forced.
        if ((!forceDismissal && !isDismissable) || isDismissing)
        {
            //Plays the appropriate sound effect for unsuccessfully dismissing a notification.
            AudioManager.PlaySFX(unsuccessfulDismissSFX);

            //Returns out of the function.
            return;
        }

        //Logs that the notification is in the action of being dismissed.
        isDismissing = true;

        //Plays the appropriate sound effect for successfully dismissing a notification.
        AudioManager.PlaySFX(successfulDismissSFX);
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
    public bool isDismissable = true;
    public bool isWarning = false;

    public GalaxyNotificationData(GalaxyNotification notification)
    {
        text = notification.text;
        spriteName = notification.spriteName;
        isDismissable = notification.isDismissable;
        isWarning = notification.isWarning;
    }
}