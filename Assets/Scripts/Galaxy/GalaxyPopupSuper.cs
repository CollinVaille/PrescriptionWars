using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GalaxyPopupSuper : MonoBehaviour
{
    public enum PopupOpeningAnimationType
    {
        Instant,        //Popup is instantly fully open as soon as it is launched.
        Expand      //Popup scale starts at 0 and increases by a constant rate until it is at 1.
    }
    //Indicates what type of opening animation the popup will have.
    public PopupOpeningAnimationType openingAnimationType;

    //Sound effect that will be played as soon as the popup has been opened.
    public AudioClip openPopupSFX;
    //Sound effect that will be played when the popup is closed.
    public AudioClip closePopupSFX;

    //Indicates whether the logic for the popup opening is called in the start method.
    public bool popupOpensAtStart;
    //Indicates whether the mouse is over the popup.
    bool mouseOverPopup;
    //Indicates whether the popup is being moved/dragged.
    bool beingMoved;

    //Indicates the set distance between the mouse and the popup's location (used for dragging popups and probably shouldn't be messed with otherwise).
    Vector2 mouseToMenuDistance;
    //Indicates the width and height of the popup (x: width, y: height).
    public Vector2 popupWidthAndHeight;

    //Indicates the constant rate at which the scale of the popup will increase if it has an expand opening animation type.
    public float popupScaleIncreaseRate = 3.0f;

    // Start is called before the first frame update
    public virtual void Start()
    {
        //Executes the logic for opening the popup if the logic for opening the popup is supposed to be executed in the start method.
        if (popupOpensAtStart)
            Open();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //Closes the popup if the popup should close due to the player pressing escape.
        if (ShouldClose())
        {
            Close();
        }

        //Deals with the popup opening animation.
        if (!IsOpeningAnimationDone())
        {
            if(openingAnimationType == PopupOpeningAnimationType.Expand)
            {
                transform.localScale = new Vector3(transform.localScale.x + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.y + (popupScaleIncreaseRate * Time.deltaTime), transform.localScale.z);

                if (transform.localScale.x > 1)
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                if (transform.localScale.y > 1)
                    transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            }
        }

        //Deals with the popup being dragged.
        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -1 * (291 + ((221.16664f / 2) - (popupWidthAndHeight.x / 2))))
            {
                transform.localPosition = new Vector2(-1 * (291 + ((221.16664f / 2) - (popupWidthAndHeight.x / 2))), transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < GalaxyManager.galaxyCamera.pixelWidth * -1 * (.13545f * (popupWidthAndHeight.x / 221.16664f)))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * -1 * (.13545f * (popupWidthAndHeight.x / 221.16664f));
            }
            //Right barrier.
            if (transform.localPosition.x > 291 + ((221.16664f / 2) - (popupWidthAndHeight.x / 2)))
            {
                transform.localPosition = new Vector2(291 + ((221.16664f / 2) - (popupWidthAndHeight.x / 2)), transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > GalaxyManager.galaxyCamera.pixelWidth * (.13545f * (popupWidthAndHeight.x / 221.16664f)))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * (.13545f * (popupWidthAndHeight.x / 221.16664f));
            }
            //Top barrier.
            if (transform.localPosition.y > 67.5f + ((255.3096f / 2) - (popupWidthAndHeight.y / 2)))
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 67.5f + ((255.3096f / 2) - (popupWidthAndHeight.y / 2)));

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > GalaxyManager.galaxyCamera.pixelHeight * (.2771f * (popupWidthAndHeight.y / 255.3096f)))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * (.2771f * (popupWidthAndHeight.y / 255.3096f));
            }
            //Bottom barrier.
            if (transform.localPosition.y < -1 * (99 + ((255.3096f / 2) - (popupWidthAndHeight.y / 2))))
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -1 * (99 + ((255.3096f / 2) - (popupWidthAndHeight.y / 2))));

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < GalaxyManager.galaxyCamera.pixelHeight * -1 * (.2771f * (popupWidthAndHeight.y / 255.3096f)))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * -1 * (.2771f * (popupWidthAndHeight.y / 255.3096f));
            }
        }

        //If the popup is clicked, it is brought to the top of the popup hierarchy.
        if (mouseOverPopup && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();
    }

    //Indicates whether the popup should close due to the player pressing escape.
    public virtual bool ShouldClose()
    {
        return Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.parent.childCount - 1 && !GalaxyManager.popupClosedOnFrame && !GalaxyConfirmationPopup.IsAGalaxyConfirmationPopupOpen();
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

    //Toggles the boolean that indicates whether the mouse is over the popup or not.
    public void ToggleMouseOverPopup()
    {
        mouseOverPopup = !mouseOverPopup;
    }

    //Gets the boolean that indicates whether the mouse is over the popup or not.
    public bool IsMouseOverPopup()
    {
        return mouseOverPopup;
    }

    //Sets the boolean that indicates whether the mouse is over the popup or not to the specified value.
    public void SetMouseOverPopup(bool value)
    {
        mouseOverPopup = value;
    }

    //Plays the sound effect for whenever the popup opens.
    void PlayOpenPopupSFX()
    {
        if(openPopupSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(openPopupSFX);
    }

    //Plays the sound effect for whenever the popup closes.
    void PlayClosePopupSFX()
    {
        if (closePopupSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(closePopupSFX);
    }

    //Deactivates the popup game object and sends it to the back of the popups priority hierarchy, but does not destroy the game object or do any specific logic that might be needed for some popups, and to add more logic you will need to add an override method for this in the subclass.
    public virtual void Close()
    {
        //Logs with the galaxy manager that a popup has been closed on this frame (so that other popups will not close on the same frame because of the escape key being pressed).
        GalaxyManager.popupClosedOnFrame = true;

        // Resets whether the popup is being dragged by the player.
        PointerUpPopup();

        //Places the popup at the top of the popup parent object's hierarchy (last priority).
        transform.SetSiblingIndex(0);

        //Deactivates the whole popup.
        transform.gameObject.SetActive(false);

        //Plays the close popup sound effect.
        PlayClosePopupSFX();
    }

    public virtual void Open()
    {
        //Activates the popup game object.
        gameObject.SetActive(true);

        //Brings the popup to the top of the popups priority hierarchy (bottom of the hierarchy when viewed in the editor).
        transform.SetAsLastSibling();

        //Resets the position of the popup.
        transform.localPosition = new Vector3(0, 0, 0);

        //Starts the opening animation of the popup.
        if (openingAnimationType == PopupOpeningAnimationType.Expand)
            transform.localScale = new Vector3(0, 0, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);

        //Plays the sound effect for whenever the popup opens.
        PlayOpenPopupSFX();
    }

    //Indicates whether the opening animation for the popup is done.
    public bool IsOpeningAnimationDone()
    {
        if(openingAnimationType == PopupOpeningAnimationType.Expand)
            return transform.localScale.x >= 1 && transform.localScale.y >= 1;
        return true;
    }
}