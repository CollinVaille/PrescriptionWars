using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyNotificationManager : MonoBehaviour
{
    [Header("Options")]

    [SerializeField, Tooltip("The float value that represents the amount of space between notifications in the galaxy scene.")] private float _spacing = 5;
    [SerializeField, Tooltip("The float value that represents the speed at which the notifications will move downwards when needed.")] private float _downwardsMovementSpeed = 1000;

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
    /// Private list that contains all of the notifications currently active within the galaxy view.
    /// </summary>
    private List<GalaxyNotification> notifications = null;

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
                CreateNotification(notificationSaveData);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Public method that should be called in order to create a brand new notification with the specified parameters. Method should not be used to recreate a notification from save data, the private CreateNotification() method handles that.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="spriteName"></param>
    /// <param name="isDismissable"></param>
    /// <param name="isWarning"></param>
    public void CreateNotification(string text, string spriteName, bool isDismissable = true, bool isWarning = false)
    {
        //Initializes the list of notifications if it has not yet been initialized.
        if (notifications == null)
            notifications = new List<GalaxyNotification>();

        //Instantiates a new notification from the notification prefab and adds it to the list of notifications.
        notifications.Add(Instantiate(GalaxyNotification.prefab).GetComponent<GalaxyNotification>());

        //Sets the notification's parent as the transform of the notification manager.
        notifications[notifications.Count - 1].transform.SetParent(transform);

        //Initializes the notification with the specified parameters.
        notifications[notifications.Count - 1].Initialize(text, spriteName, notifications.Count - 1, isDismissable, isWarning);
    }

    /// <summary>
    /// Private method that should be called in order to recreate a notification from its save data. Method should not be used to create a brand new notification with brand new parameters, the public CreateNotification() method handles that.
    /// </summary>
    /// <param name="notificationData"></param>
    private void CreateNotification(GalaxyNotificationData notificationData)
    {
        //Initializes the list of notifications if it has not yet been initialized.
        if (notifications == null)
            notifications = new List<GalaxyNotification>();

        //Instantiates a new notification from the notification prefab and adds it to the list of notifications.
        notifications.Add(Instantiate(GalaxyNotification.prefab).GetComponent<GalaxyNotification>());

        //Sets the notification's parent as the transform of the notification manager.
        notifications[notifications.Count - 1].transform.SetParent(transform);

        //Initializes the notification by providing it its save data.
        notifications[notifications.Count - 1].Initialize(notificationData, notifications.Count - 1);
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
}
