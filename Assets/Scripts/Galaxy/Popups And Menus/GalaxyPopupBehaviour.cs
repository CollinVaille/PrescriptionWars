using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class GalaxyPopupBehaviour : MonoBehaviour
{
    public enum PopupOpeningAnimationType
    {
        Instant,        //Popup is instantly fully open as soon as it is launched.
        Expand,      //Popup scale starts at 0 and increases by a constant rate until it is at 1.
        VerticalExpand,     //Popup y scale starts at 0 and increases by a constant rate until it is at 1.
        HorizontalExpand        //Popup x scale starts at 0 and increases by a constant rate until it is at 1.
    }

    [Header("Base Popup Logic Options")]

    //Indicates what type of opening animation the popup will have.
    public PopupOpeningAnimationType openingAnimationType = PopupOpeningAnimationType.Instant;

    //Indicates the constant rate at which the scale of the popup will increase if it has an expand opening animation type.
    [SerializeField]
    //[ConditionalField("openingAnimationType", PopupOpeningAnimationType.Instant, ConditionalFieldComparisonType.NotEqual, ConditionalFieldDisablingType.Disappear)]
    [ConditionalField("openingAnimationType", PopupOpeningAnimationType.Instant, ConditionalFieldComparisonType.NotEqual, ConditionalFieldDisablingType.ReadOnly)]
    private float openingAnimationSpeed = 3.0f;

    //Indicates whether the logic for the popup opening is called in the start method.
    [SerializeField]
    private bool opensAtStart = false;
    //Indicates whether the top barrier for the popup is limited by the resource bar.
    [SerializeField]
    private bool isResourceBarTopBarrier = false;

    //Indicates the width and height of the popup (x: width, y: height).
    [SerializeField]
    private Vector2 popupWidthAndHeight = Vector2.zero;

    [Header("Base Popup SFX Options")]

    //Sound effect that will be played as soon as the popup has been opened.
    [SerializeField]
    private AudioClip openPopupSFX = null;
    //Sound effect that will be played when the popup is closed.
    [SerializeField]
    private AudioClip closePopupSFX = null;

    [Header("Base Popup Image Components")]

    //List of all of the images on the popup that will have their color changed to the player empire's color when the popup opens.
    [SerializeField]
    private List<Image> imagesWithEmpireColor = new List<Image>();

    //Non-inspector variables.

    //Indicates whether the mouse is over the popup.
    private bool mouseOverPopup = false;
    //Indicates whether the popup is being moved/dragged.
    private bool beingMoved = false;

    //Indicates the set distance between the mouse and the popup's location (used for dragging popups and probably shouldn't be messed with otherwise).
    private Vector2 mouseToMenuDistance = Vector2.zero;

    // Start is called before the first frame update
    public virtual void Start()
    {
        //Executes the logic for opening the popup if the logic for opening the popup is supposed to be executed in the start method.
        if (opensAtStart)
            Open();
    }

    public virtual void Awake()
    {

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
                transform.localScale = new Vector3(transform.localScale.x + (openingAnimationSpeed * Time.deltaTime), transform.localScale.y + (openingAnimationSpeed * Time.deltaTime), transform.localScale.z);

                if (transform.localScale.x > 1)
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                if (transform.localScale.y > 1)
                    transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            }
            else if (openingAnimationType == PopupOpeningAnimationType.VerticalExpand)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + (openingAnimationSpeed * Time.deltaTime), transform.localScale.z);

                if (transform.localScale.y > 1)
                    transform.localScale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
            }
            else if (openingAnimationType == PopupOpeningAnimationType.HorizontalExpand)
            {
                transform.localScale = new Vector3(transform.localScale.x + (openingAnimationSpeed * Time.deltaTime), transform.localScale.y, transform.localScale.z);

                if (transform.localScale.x > 1)
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
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
            float topBarrierLimit = isResourceBarTopBarrier ? 67.5f : 99.0f;
            if (transform.localPosition.y > topBarrierLimit + ((255.3096f / 2) - (popupWidthAndHeight.y / 2)))
            {
                transform.localPosition = new Vector2(transform.localPosition.x, topBarrierLimit + ((255.3096f / 2) - (popupWidthAndHeight.y / 2)));

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

        //Brings the popup to the top of the priority hierarchy.
        transform.SetAsLastSibling();
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
        if (openingAnimationType == PopupOpeningAnimationType.Instant)
            transform.localScale = new Vector3(1, 1, 1);
        else if (openingAnimationType == PopupOpeningAnimationType.Expand)
            transform.localScale = new Vector3(0, 0, 1);
        else if (openingAnimationType == PopupOpeningAnimationType.VerticalExpand)
            transform.localScale = new Vector3(1, 0, 1);
        else if (openingAnimationType == PopupOpeningAnimationType.HorizontalExpand)
            transform.localScale = new Vector3(0, 1, 1);

        //Sets the color of every image that is supposed to be the player empire's color to the player empire's color.
        foreach (Image imageWithEmpireColor in imagesWithEmpireColor)
        {
            imageWithEmpireColor.color = Empire.empires[GalaxyManager.playerID].empireColor;
        }

        //Plays the sound effect for whenever the popup opens.
        PlayOpenPopupSFX();
    }

    //Indicates whether the opening animation for the popup is done.
    public bool IsOpeningAnimationDone()
    {
        switch (openingAnimationType)
        {
            case PopupOpeningAnimationType.Expand:
                return transform.localScale.x >= 1 && transform.localScale.y >= 1;
            case PopupOpeningAnimationType.VerticalExpand:
                return transform.localScale.y >= 1;
            case PopupOpeningAnimationType.HorizontalExpand:
                return transform.localScale.x >= 1;

            default:
                return true;
        }
    }
}