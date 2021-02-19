using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightSideNotificationManager : MonoBehaviour
{
    public GameObject rideSideNotificationPrefab;

    public static List<RightSideNotification> rightSideNotifications = new List<RightSideNotification>();

    public static RightSideNotificationManager rightSideNotificationManager;

    // Start is called before the first frame update
    void Start()
    {
        /*GalaxyPopupData newPopUpData = new GalaxyPopupData();
        newPopUpData.headLine = "Test Notification";
        newPopUpData.spriteName = "Research Completed";
        newPopUpData.bodyText = "This is a test notification.";
        newPopUpData.specialOpenSFXName = "Research Completed";
        newPopUpData.answerRequired = true;
        GalaxyPopupOptionData galaxyPopupOptionData = new GalaxyPopupOptionData();
        galaxyPopupOptionData.mainText = "This is merely a test?";
        galaxyPopupOptionData.effectDescriptionText = "Nothing happens.";
        GalaxyPopupOptionEffect galaxyPopupOptionEffect = new GalaxyPopupOptionEffect();
        galaxyPopupOptionEffect.effectType = GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.None;
        galaxyPopupOptionData.effects.Add(galaxyPopupOptionEffect);
        newPopUpData.options.Add(galaxyPopupOptionData);
        GalaxyPopupOptionData gp = new GalaxyPopupOptionData();
        gp.mainText = "Hello";
        gp.effectDescriptionText = "Nothing happens here either.";
        GalaxyPopupOptionEffect gpfx = new GalaxyPopupOptionEffect();
        gpfx.effectType = GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.None;
        gp.effects.Add(gpfx);
        newPopUpData.options.Add(gp);
        CreateNewRightSideNotification("Research Completed", "Test Notification 1", newPopUpData);
        CreateNewRightSideNotification(null, "Test Notification 2", newPopUpData);*/
    }

    void Awake()
    {
        rightSideNotificationManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        GalaxyCamera.mouseOverRightSideNotification = false;
        foreach(RightSideNotification notification in rightSideNotifications)
        {
            if (notification.GetMouseOverNotification())
            {
                GalaxyCamera.mouseOverRightSideNotification = true;
                break;
            }
        }
    }

    public static void CreateNewRightSideNotification(string spriteName, string notificationTopic, bool dismissable, GalaxyPopupData popupData)
    {
        GameObject rightSideNotification = Instantiate(rightSideNotificationManager.rideSideNotificationPrefab);
        rightSideNotification.transform.parent = rightSideNotificationManager.transform;

        //Determines the starting y position of each notification.
        float position = 250.0f;
        if(rightSideNotifications.Count > 0)
        {
            if(rightSideNotifications[rightSideNotifications.Count - 1].transform.localPosition.y >= 250.0f)
            {
                position = rightSideNotifications[rightSideNotifications.Count - 1].transform.localPosition.y + 55;
            }
        }

        //Creates the contents of the notification.
        rightSideNotification.GetComponent<RightSideNotification>().CreateNewRightSideNotification(spriteName, notificationTopic, dismissable, rightSideNotifications.Count, position, popupData);

        //Adds the notification to the notifications list.
        rightSideNotifications.Add(rightSideNotification.GetComponent<RightSideNotification>());
    }

    public static void CreateNewWarningRightSideNotification(string spriteName, string notificationTopic, WarningRightSideNotificationClickEffect clickEffect)
    {
        GameObject rightSideNotification = Instantiate(rightSideNotificationManager.rideSideNotificationPrefab);
        rightSideNotification.transform.parent = rightSideNotificationManager.transform;

        //Determines the starting y position of each notification.
        float position = 250.0f;
        if (rightSideNotifications.Count > 0)
        {
            if (rightSideNotifications[rightSideNotifications.Count - 1].transform.localPosition.y >= 250.0f)
            {
                position = rightSideNotifications[rightSideNotifications.Count - 1].transform.localPosition.y + 55;
            }
        }

        //Creates the contents of the notification.
        rightSideNotification.GetComponent<RightSideNotification>().CreateNewWarningRightSideNotification(spriteName, notificationTopic, rightSideNotifications.Count, position, clickEffect);

        //Adds the notification to the notifications list.
        rightSideNotifications.Add(rightSideNotification.GetComponent<RightSideNotification>());
    }

    public static void DissmissNotification(int position)
    {
        for(int x = position + 1; x < rightSideNotifications.Count; x++)
        {
            rightSideNotifications[x].SetPositionInNotificationQueue(x - 1);
        }

        RightSideNotification notificationDismissed = rightSideNotifications[position];
        rightSideNotifications.RemoveAt(position);
        Destroy(notificationDismissed.gameObject);
    }

    public static bool ContainsNotificationWithAnswerRequired()
    {
        foreach(RightSideNotification rightSideNotification in rightSideNotifications)
        {
            if (rightSideNotification.IsAnswerRequired())
                return true;
        }

        return false;
    }

    public static void DismissAllNotifications(bool forceDismissal)
    {
        foreach(RightSideNotification notification in rightSideNotifications)
        {
            if(forceDismissal || notification.IsDismissable())
                notification.StartNotificationDismissal();
        }
    }

    public static bool NotificationExistsOfTopic(string notificationTopic)
    {
        foreach(RightSideNotification notification in rightSideNotifications)
        {
            if (notification.GetNotificationTopic().ToLower().Equals(notificationTopic.ToLower()))
                return true;
        }

        return false;
    }

    public static void DismissNotificationsOfTopic(string notificationTopic)
    {
        foreach (RightSideNotification notification in rightSideNotifications)
        {
            if (notification.GetNotificationTopic().ToLower().Equals(notificationTopic.ToLower()))
                notification.StartNotificationDismissal();
        }
    }
}
