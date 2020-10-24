using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightSideNotificationManager : MonoBehaviour
{
    public GameObject rideSideNotificationPrefab;

    public List<Sprite> rightSideNotificationSprites;
    public List<string> rightSideNotificationSpriteNames;

    public static Dictionary<string, Sprite> rightSideNotificationSpritesDictionary = new Dictionary<string, Sprite>();

    public static List<RightSideNotification> rightSideNotifications = new List<RightSideNotification>();

    public static RightSideNotificationManager rightSideNotificationManager;

    // Start is called before the first frame update
    void Start()
    {
        GenerateRightSideNotificationSpritesDictionary();

        rightSideNotificationManager = this;
        GalaxyPopupData newPopUpData = new GalaxyPopupData();
        newPopUpData.headLine = "Test Notification";
        newPopUpData.spriteName = "Test Sprite";
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
        CreateNewRightSideNotification(null, "Test Notification 2", newPopUpData);
    }

    void GenerateRightSideNotificationSpritesDictionary()
    {
        if (rightSideNotificationSprites.Count == rightSideNotificationSpriteNames.Count)
        {
            for (int x = 0; x < rightSideNotificationSpriteNames.Count; x++)
            {
                rightSideNotificationSpritesDictionary[rightSideNotificationSpriteNames[x]] = rightSideNotificationSprites[x];
            }
        }
        else
        {
            Debug.Log("Right side notification sprites list and right side notifications sprite names list count do not match! Please fix this issue.");
        }
    }

    public static Sprite GetRightSideNotificationSpriteFromName(string rightSideNotificationSpriteName)
    {
        if(rightSideNotificationSpriteName == null)
        {
            return null;
        }

        if (rightSideNotificationSpritesDictionary.ContainsKey(rightSideNotificationSpriteName))
        {
            return rightSideNotificationSpritesDictionary[rightSideNotificationSpriteName];
        }
        else
        {
            Debug.Log("Invalid Right Side Notification Sprite Name (key does not exist in dictionary)");
            return null;
        }
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

    public static void CreateNewRightSideNotification(string spriteName, string notificationTopic, GalaxyPopupData popupData)
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
        rightSideNotification.GetComponent<RightSideNotification>().CreateNewRightSideNotification(spriteName, notificationTopic, rightSideNotifications.Count, position, popupData);

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

    public static bool ContainsNonDismissableNotification()
    {
        foreach(RightSideNotification rightSideNotification in rightSideNotifications)
        {
            if (rightSideNotification.IsAnswerRequired())
                return true;
        }

        return false;
    }
}
