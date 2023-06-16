using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyNotificationManager : MonoBehaviour
{
    [Header("Options")]

    [SerializeField, Tooltip("The float value that represents the amount of space between notifications in the galaxy scene.")] private float _spacing = 5;
    [SerializeField, Tooltip("The float value that represents the speed at which the notifications will move downwards when needed.")] private float _downwardsMovementSpeed = 750;
    [SerializeField, Tooltip("The float value that represents the speed at which the notifications will be dismissed.")] private float _dismissalSpeed = 500;
    [SerializeField, Tooltip("The float value that represents the speed at which the notification's text will fade in and out depending on whether the player is mousing over it or not.")] private float _textFadeSpeed = 3;
    [SerializeField, Tooltip("The sound effect that will be played only once if a notification has been created since the manager's last update function call.")] private AudioClip notificationCreatedSFX = null;

    //Non-inspector variables.

    /// <summary>
    /// Public property that should be used in order to access the float value that represents the amount of space between notifications in the galaxy scene.
    /// </summary>
    public float spacing { get => _spacing; }

    /// <summary>
    /// Public property that should be used in order to access the float value that represents the speed at which the notifications will move downwards when needed.
    /// </summary>
    public float downwardsMovementSpeed { get => _downwardsMovementSpeed; }

    /// <summary>
    /// Public property that should be used in order to access the float value that represents the speed at which the notifications will be dismissed.
    /// </summary>
    public float dismissalSpeed { get => _dismissalSpeed; }

    /// <summary>
    /// Public property that should be used in order to access the float value that represents the speed at which the notification's text will fade in and out depending on whether the player is mousing over it or not.
    /// </summary>
    public float textFadeSpeed { get => _textFadeSpeed; }

    /// <summary>
    /// Private list that contains all of the notifications currently active within the galaxy view.
    /// </summary>
    private List<GalaxyNotification> notifications = null;
    /// <summary>
    /// Public property that should be used in order to access the integer value that represents the total number of notifications that are active within the galaxy scene.
    /// </summary>
    public int notificationCount { get => notifications == null ? 0 : notifications.Count; }

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether or not a non dismissable notification is in the notification queue.
    /// </summary>
    public bool isNonDismissableNotificationInQueue
    {
        get
        {
            //Checks if the notifications list is null and returns false if so.
            if (notifications == null)
                return false;

            //Loops through each notification until finding one that is not dismissable and returning true.
            foreach (GalaxyNotification notification in notifications)
                if (!notification.isDismissable)
                    return true;

            //Returns false if no non dismissable notification could be found within the notifications list.
            return false;
        }
    }

    /// <summary>
    /// Public property that should be used in order to access the list of save data for the notifications that are active within the galaxy scene. Notifications that are in the action of being dismissed will not be in the list of save data since saving them is not neccessary.
    /// </summary>
    public List<GalaxyNotificationData> notificationsSaveData
    {
        get
        {
            //Declares and initializes a new list of notification save data.
            List<GalaxyNotificationData> notificationsSaveData = new List<GalaxyNotificationData>();

            //Returns the empty list if the notifications list is either null or empty itself.
            if (notifications == null || notifications.Count == 0)
                return notificationsSaveData;

            //Loops through each notification and adds its save data to the save data list if it is not in the action of dismissing.
            foreach (GalaxyNotification notification in notifications)
                if (!notification.isDismissing)
                    notificationsSaveData.Add(new GalaxyNotificationData(notification));

            //Returns the list of notification save data.
            return notificationsSaveData;
        }
    }

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether or not a notification has been created since the last call of the manager's update function. Is not set to true for notifications that are created from save data on manager initialization.
    /// </summary>
    public bool notificationCreatedOnFrame { get; private set; } = false;

    /// <summary>
    /// Public method that should be called in order to initialize the notification manager either at the start of a new game or with save game notification data.
    /// </summary>
    /// <param name="notificationsSaveData"></param>
    public void Initialize(List<GalaxyNotificationData> notificationsSaveData = null)
    {
        //Destroys any notifications that might exist at the moment and resets the notifications list to null.
        if(notifications != null)
            for (int notificationIndex = notifications.Count - 1; notificationIndex >= 0; notificationIndex--)
                Destroy(notifications[notificationIndex].gameObject);
        notifications = null;

        //Loops through each notification save data and recreates the notification.
        if (notificationsSaveData != null)
            foreach (GalaxyNotificationData notificationSaveData in notificationsSaveData)
                CreateNotification(notificationSaveData, true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Checks if a notification has been created since the last update function call and plays the appropriate sound effect if so before setting the boolean value back to false.
        if (notificationCreatedOnFrame)
        {
            AudioManager.PlaySFX(notificationCreatedSFX);
            notificationCreatedOnFrame = false;
        }
    }

    /// <summary>
    /// Public method that should be called in order to create a brand new notification with the specified parameters. Method should not be used to recreate a notification from save data, the private CreateNotification() method handles that.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="spriteName"></param>
    /// <param name="isDismissable"></param>
    /// <param name="isWarning"></param>
    public void CreateNotification(string text, string spriteName, bool isDismissable = true, bool isWarning = false, NewGalaxyPopupData popupData = null)
    {
        //Initializes the list of notifications if it has not yet been initialized.
        if (notifications == null)
            notifications = new List<GalaxyNotification>();

        //Instantiates a new notification from the notification prefab and adds it to the list of notifications.
        notifications.Add(Instantiate(GalaxyNotification.prefab).GetComponent<GalaxyNotification>());

        //Sets the notification's parent as the transform of the notification manager.
        notifications[notifications.Count - 1].transform.SetParent(transform);

        //Initializes the notification with the specified parameters.
        notifications[notifications.Count - 1].Initialize(text, spriteName, notifications.Count - 1, OnNotificationDismissed, isDismissable, isWarning, popupData);

        //Logs that a notification was created on this frame (needed in order to play the appropriate sound effect only once).
        notificationCreatedOnFrame = true;

        //Informs the galaxy manager of the notification count change.
        NewGalaxyManager.OnNotificationCountChange();
    }

    /// <summary>
    /// Public method that should be called in order to recreate a notification from its save data. Method should not be used to create a brand new notification with brand new parameters, the public CreateNotification() method handles that.
    /// </summary>
    /// <param name="notificationData"></param>
    public void CreateNotification(GalaxyNotificationData notificationData, bool onManagerInitialization = false)
    {
        //Initializes the list of notifications if it has not yet been initialized.
        if (notifications == null)
            notifications = new List<GalaxyNotification>();

        //Instantiates a new notification from the notification prefab and adds it to the list of notifications.
        notifications.Add(Instantiate(GalaxyNotification.prefab).GetComponent<GalaxyNotification>());

        //Sets the notification's parent as the transform of the notification manager.
        notifications[notifications.Count - 1].transform.SetParent(transform);

        //Initializes the notification by providing it its save data.
        if(onManagerInitialization)
            notifications[notifications.Count - 1].Initialize(notificationData, notifications.Count - 1, OnNotificationDismissed);
        else
        {
            notifications[notifications.Count - 1].Initialize(notificationData.text, notificationData.spriteName, notifications.Count - 1, OnNotificationDismissed, notificationData.isDismissable, notificationData.isWarning, notificationData.popupData);

            //Logs that a notification was created on this frame (needed in order to play the appropriate sound effect only once).
            notificationCreatedOnFrame = true;
        }

        //Informs the galaxy manager of the notification count change.
        NewGalaxyManager.OnNotificationCountChange();
    }

    /// <summary>
    /// Public method that should be used in order to access the notification at the specified index.
    /// </summary>
    /// <param name="notificationIndex"></param>
    /// <returns></returns>
    public GalaxyNotification GetNotificationAt(int notificationIndex)
    {
        if (notifications == null || notificationIndex < 0 || notificationIndex >= notifications.Count)
            return null;
        return notifications[notificationIndex];
    }

    /// <summary>
    /// Private method that should be called by a notification after it has finished moving into dismissal position and is about to be destroyed. Removes the specified notification from the list of notifications that are active within the galaxy scene.
    /// </summary>
    /// <param name="notification"></param>
    private void OnNotificationDismissed(GalaxyNotification notification)
    {
        if (notification != null && notifications != null && notifications.Contains(notification))
        {
            //Loops through each notification that is above the notification that is just now being fully dismissed and tells them that they need to move downwards to fill the gap.
            for (int notificationIndex = notifications.IndexOf(notification) + 1; notificationIndex < notifications.Count; notificationIndex++)
                notifications[notificationIndex].isMovingDownwards = true;

            //Removes the dismissed notification from the list of notifications that are active within the galaxy scene.
            notifications.Remove(notification);

            //Informs the galaxy manager of the notification count change.
            NewGalaxyManager.OnNotificationCountChange();
        }
    }

    /// <summary>
    /// Public method that should be called in order to dismiss all notifications in the notification queue. Boolean value is specified that indicates whether or not the dismissal action is forced on each popup regardless of whether it is dismissable or not.
    /// </summary>
    /// <param name="forceDismissal"></param>
    public void DismissAllNotifications(bool forceDismissal = false)
    {
        //Retuns if the notifications list is null and there are no valid notifications to dismiss.
        if (notifications == null)
            return;

        //Loops through each notification in the notifications list and dismisses it with the specified force dismissal boolean value.
        foreach (GalaxyNotification notification in notifications)
            notification.Dismiss(forceDismissal);
    }
}
