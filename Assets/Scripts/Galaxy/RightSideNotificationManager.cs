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
        rightSideNotificationManager = this;
        CreateNewRightSideNotification(null, "Test Notification 1", true);
        CreateNewRightSideNotification(null, "Test Notification 2", true);
    }

    // Update is called once per frame
    void Update()
    {
        //For testing purposes.
        if (Input.GetKeyDown(KeyCode.C))
        {
            CreateNewRightSideNotification(null, "New Test Notification", true);
        }

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

    public static void CreateNewRightSideNotification(Sprite imageSprite, string notificationTopic, bool dismissable)
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
        rightSideNotification.GetComponent<RightSideNotification>().CreateNewRightSideNotification(imageSprite, notificationTopic, dismissable, rightSideNotifications.Count, position);

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
}
