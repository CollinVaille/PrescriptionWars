using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RightSideNotification : MonoBehaviour
{
    public Image foregroundImage;
    public Image overlayImage;

    public Text notificationTopicText;

    public float notificationTopicTextOpacityChangeRate;
    public float notificationMoveSpeed;

    public float warningOverlaySpeed;

    private bool mouseOverNotification;
    private bool beingDismissed;
    private bool beingOpened;
    private bool reachedInitialPosition;
    private bool dismissable;

    private Color notificationTopicTextNewColor;
    private Color overlayImageNewColor;

    private int positionInNotificationQueue;

    private GalaxyPopupData popupData;

    public enum RightSideNotificationType
    {
        Normal,
        Warning
    }
    private RightSideNotificationType notificationType;

    //Warning notification specifics.
    private WarningRightSideNotificationClickEffect clickEffect;

    private bool warningOverlayOpacityIncreasing = true;

    // Start is called before the first frame update
    void Start()
    {
        notificationTopicTextNewColor = notificationTopicText.color;
        overlayImageNewColor = overlayImage.color;
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

        //Executes the logic that needs to be executed every update for warning notifications.
        if(notificationType == RightSideNotificationType.Warning)
            ExecuteWarningNotificationUpdateLogic();
    }

    private void ExecuteWarningNotificationUpdateLogic()
    {
        if (warningOverlayOpacityIncreasing)
        {
            overlayImageNewColor.a += warningOverlaySpeed * Time.deltaTime;

            if(overlayImageNewColor.a >= 1)
            {
                overlayImageNewColor.a = 1;
                warningOverlayOpacityIncreasing = false;
            }

            overlayImage.color = overlayImageNewColor;
        }
        else
        {
            overlayImageNewColor.a -= warningOverlaySpeed * Time.deltaTime;

            if (overlayImageNewColor.a <= 0)
            {
                overlayImageNewColor.a = 0;
                warningOverlayOpacityIncreasing = true;
            }

            overlayImage.color = overlayImageNewColor;
        }
    }

    public void CreateNewRightSideNotification(string spriteName, string notificationTopic, bool isDismissable, int positionInQueue, float position, GalaxyPopupData popupData)
    {
        //Loads in the appropriate sprite from the project's resources and sets it to be the sprite of the foreground image of the notification.
        foregroundImage.sprite = Resources.Load<Sprite>("Galaxy/Right Side Notifications/" + spriteName);

        //Sets the text of the notification topic text component to be the specified notification topic.
        notificationTopicText.text = notificationTopic;

        //Sets the boolean that indicates whether the notification is dismissable or not to the specified value.
        dismissable = isDismissable;

        //Sets the variable that indicates the notification's position in the notification queue.
        positionInNotificationQueue = positionInQueue;

        //Sets the actual position of the notification.
        transform.localPosition = new Vector3(370, position, 0);

        //Resets the scale of the notification.
        transform.localScale = new Vector3(1, 1, 1);

        //Stores the passed in popup data.
        this.popupData = popupData;

        //Indicates that the notification is a normal notification.
        notificationType = RightSideNotificationType.Normal;
    }

    public void CreateNewWarningRightSideNotification(string spriteName, string notificationTopic, int positionInQueue, float position, WarningRightSideNotificationClickEffect clickEffect)
    {
        //Loads in the appropriate sprite from the project's resources and sets it to be the sprite of the foreground image of the notification.
        foregroundImage.sprite = Resources.Load<Sprite>("Galaxy/Right Side Notifications/" + spriteName);

        //Sets the text of the notification topic text component to be the specified notification topic.
        notificationTopicText.text = notificationTopic;

        //Indicates that the notification cannot be dismissed.
        dismissable = false;

        //Sets the variable that indicates the notification's position in the notification queue.
        positionInNotificationQueue = positionInQueue;

        //Sets the actual position of the notification.
        transform.localPosition = new Vector3(370, position, 0);

        //Resets the scale of the notification.
        transform.localScale = new Vector3(1, 1, 1);

        //Sets the variable that indicates what effect will happen when the warning right side notification is clicked.
        this.clickEffect = clickEffect;

        //Indicates that the notification is a warning notification.
        notificationType = RightSideNotificationType.Warning;
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
                else if (notificationType == RightSideNotificationType.Warning)
                {
                    ExecuteWarningNotificationClickEffect();
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

    void ExecuteWarningNotificationClickEffect()
    {
        switch (clickEffect)
        {
            case WarningRightSideNotificationClickEffect.None:
                break;
            case WarningRightSideNotificationClickEffect.OpenResearchView:
                GalaxyManager.galaxyManager.SwitchToResearchView();
                break;

            default:
                Debug.Log("No warning notification click effect logic exists for the click effect type: " + clickEffect.ToString() + ".");
                break;
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

    public bool IsBeingDismissed()
    {
        return beingDismissed;
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

    private void OnDisable()
    {
        //Indicates that the mouse is not hovering over the notification.
        mouseOverNotification = false;

        //Sets the topic text of the notification to be completely transparent.
        notificationTopicTextNewColor.a = 0;
        notificationTopicText.color = notificationTopicTextNewColor;
    }
}

public enum WarningRightSideNotificationClickEffect
{
    None,
    OpenResearchView
}