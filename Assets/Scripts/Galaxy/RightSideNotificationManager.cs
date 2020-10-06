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
        //CreateNewRightSideNotification(null, "Test Notification 1");
        //CreateNewRightSideNotification(null, "Test Notification 2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CreateNewRightSideNotification(Sprite imageSprite, string notificationTopic)
    {
        GameObject rightSideNotification = Instantiate(rightSideNotificationManager.rideSideNotificationPrefab);
        rightSideNotification.transform.parent = rightSideNotificationManager.transform;

        rightSideNotification.GetComponent<RightSideNotification>().CreateNewRightSideNotification(imageSprite, notificationTopic, rightSideNotifications.Count);

        rightSideNotifications.Add(rightSideNotification.GetComponent<RightSideNotification>());
    }
}
