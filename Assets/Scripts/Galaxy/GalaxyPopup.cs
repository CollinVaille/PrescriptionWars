using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPopup : MonoBehaviour
{
    public Text headLineText;
    public Image bodyImage;
    public Text bodyText;

    public float popupScaleIncreaseRate;

    public int popupIndex;

    bool answerRequired;
    bool mouseOverPopup;
    bool beingMoved;

    Vector2 mouseToMenuDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.parent.childCount - 1 && !GalaxyManager.popupClosedOnFrame && !GalaxyConfirmationPopup.galaxyConfirmationPopup.gameObject.activeInHierarchy)
        {
            ClosePopup();
        }

        if (transform.localScale.x < 1 || transform.localScale.y < 1)
        {
            transform.localScale = new Vector3(transform.localScale.x + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.y + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.z);

            if (transform.localScale.x > 1)
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            if (transform.localScale.y > 1)
                transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
        }

        //Deals with the popup being dragged.
        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -290)
            {
                transform.localPosition = new Vector2(-290, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < -214)
                    mouseToMenuDistance.x = -214;
            }
            //Right barrier.
            if (transform.localPosition.x > 290)
            {
                transform.localPosition = new Vector2(290, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > 214)
                    mouseToMenuDistance.x = 214;
            }
            //Top barrier.
            if (transform.localPosition.y > 62)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 62);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > 257)
                    mouseToMenuDistance.y = 257;
            }
            //Bottom barrier.
            if (transform.localPosition.y < -95)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -95);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < -253)
                    mouseToMenuDistance.y = -253;
            }
        }

        if (mouseOverPopup && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();
    }

    public void CreatePopup(GalaxyPopupData popupData, int indexOfPopup)
    {
        headLineText.text = popupData.headLine;
        bodyImage.sprite = GalaxyPopupManager.GetPopupSpriteFromName(popupData.spriteName);
        bodyText.text = popupData.bodyText;
        answerRequired = popupData.answerRequired;

        popupIndex = indexOfPopup;
    }

    public bool IsAnswerRequired()
    {
        return answerRequired;
    }

    public void PointerDownPopup()
    {
        //Tells the update function that the player is dragging the menu.
        beingMoved = true;

        //Tells the update function the set difference between the mouse position and the menu's position.
        mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;
        mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;
    }

    public void PointerUpPopup()
    {
        //Tells the update function that the player is no longer dragging the menu.
        beingMoved = false;

        //Resets the vector that says the difference between the mouse position and the menu's position.
        mouseToMenuDistance = Vector2.zero;
    }

    public void ToggleMouseOverPopup()
    {
        mouseOverPopup = !mouseOverPopup;
    }

    public bool IsMouseOverPopup()
    {
        return mouseOverPopup;
    }

    public void ClosePopup()
    {
        GalaxyPopupManager.ClosePopup(popupIndex);
    }
}
