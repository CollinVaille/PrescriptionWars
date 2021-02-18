using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RightSideNotification : MonoBehaviour
{
    public Image foregroundImage;

    public Text notificationTopicText;

    public float notificationTopicTextOpacityChangeRate;
    public float notificationMoveSpeed;

    bool mouseOverNotification;
    bool beingDismissed;
    bool beingOpened;
    bool reachedInitialPosition;
    bool dismissable;

    Color notificationTopicTextNewColor;

    int positionInNotificationQueue;

    GalaxyPopupData popupData;

    // Start is called before the first frame update
    void Start()
    {
        notificationTopicTextNewColor = notificationTopicText.color;
    }

    // Update is called once per frame
    void Update()
    {
        //Deals with the transparency of the topic text.
        if ((mouseOverNotification || !reachedInitialPosition) && !beingDismissed)
        {
            notificationTopicTextNewColor.a += notificationTopicTextOpacityChangeRate * Time.deltaTime;
        }
        else
        {
            notificationTopicTextNewColor.a -= notificationTopicTextOpacityChangeRate * Time.deltaTime;
        }

        if (notificationTopicTextNewColor.a > 1.0f)
            notificationTopicTextNewColor.a = 1.0f;
        if (notificationTopicTextNewColor.a < 0.0f)
            notificationTopicTextNewColor.a = 0.0f;

        notificationTopicText.color = notificationTopicTextNewColor;

        //Deals with the y movement of the whole notification.
        if (transform.localPosition.y > -130 + (positionInNotificationQueue * 55))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (notificationMoveSpeed * Time.deltaTime), transform.localPosition.z);
            if(transform.localPosition.y <= -130 + (positionInNotificationQueue * 55))
            {
                transform.localPosition = new Vector3(transform.localPosition.x, -130 + (positionInNotificationQueue * 55), transform.localPosition.z);
                reachedInitialPosition = true;
            }
        }

        //Deals with the x movement of the whole notification.
        if (beingDismissed || beingOpened)
        {
            transform.localPosition = new Vector3(transform.localPosition.x + (notificationMoveSpeed * Time.deltaTime), transform.localPosition.y, transform.localPosition.z);
            if(transform.localPosition.x >= 425)
            {
                DismissNotification();
            }
        }
    }

    public void CreateNewRightSideNotification(string spriteName, string notificationTopic, bool isDismissable, int positionInQueue, float position, GalaxyPopupData popupData)
    {
        foregroundImage.sprite = Resources.Load<Sprite>("Galaxy/Right Side Notifications/" + spriteName);
        notificationTopicText.text = notificationTopic;
        dismissable = isDismissable;
        positionInNotificationQueue = positionInQueue;
        transform.localPosition = new Vector3(370, position, 0);
        transform.localScale = new Vector3(1, 1, 1);
        this.popupData = popupData;
    }

    public void ToggleMouseOverNotification()
    {
        mouseOverNotification = !mouseOverNotification;
    }

    public void ClickOnNotification()
    {
        if (!beingDismissed && !beingOpened)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (popupData != null)
                {
                    beingOpened = true;
                    OpenNotification();
                }
            }
            else if (Input.GetMouseButtonUp(1))
            {
                if (dismissable)
                {
                    if (popupData != null)
                    {
                        if (!popupData.answerRequired)
                            StartNotificationDismissal();
                    }
                    else
                    {
                        StartNotificationDismissal();
                    }
                }
            }
        }
    }

    public void StartNotificationDismissal()
    {
        beingDismissed = true;
    }

    void DismissNotification()
    {
        RightSideNotificationManager.DissmissNotification(positionInNotificationQueue);
    }

    public void OpenNotification()
    {
        GalaxyPopupManager.CreateNewPopup(popupData);
    }

    public void SetPositionInNotificationQueue(int position)
    {
        positionInNotificationQueue = position;
    }

    public bool GetMouseOverNotification()
    {
        return mouseOverNotification;
    }

    public bool IsDismissable()
    {
        return dismissable;
    }

    public bool IsAnswerRequired()
    {
        if(popupData != null)
        {
            return popupData.answerRequired;
        }

        return false;
    }

    public string GetNotificationTopic()
    {
        return notificationTopicText.text;
    }
}