using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightSideNotification : MonoBehaviour
{
    public Image foregroundImage;

    public Text notificationTopicText;

    public float notificationTopicTextOpacityChangeRate;

    bool mouseOverNotification;

    Color notificationTopicTextNewColor;

    // Start is called before the first frame update
    void Start()
    {
        notificationTopicTextNewColor = notificationTopicText.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOverNotification)
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
    }

    public void CreateNewRightSideNotification(Sprite imageSprite, string notificationTopic, int positionInQueue)
    {
        foregroundImage.sprite = imageSprite;
        notificationTopicText.text = notificationTopic;
        transform.localPosition = new Vector3(370, -130 + (positionInQueue * 55), 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void ToggleMouseOverNotification()
    {
        mouseOverNotification = !mouseOverNotification;
    }
}
