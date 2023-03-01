using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewGalaxyPopupBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Base Opening Options")]

    [SerializeField, Tooltip("Specifies the type of animation that will be played when the popup opens.")] private OpeningAnimationType openingAnimationType = 0;
    [SerializeField, Tooltip("Specifies the amount of time that the opening animation will last unless the opening animation type is instant.")] private float openingAnimationLength = 0;
    [SerializeField, Tooltip("Specifies the sound effect that is played when the popup is just starting to open.")] private AudioClip openingSFX = null;
    [SerializeField, Tooltip("Specifies the sound effect that is played when the popup finishes opening.")] private AudioClip openedSFX = null;

    [Header("Base Closing Options")]

    [SerializeField, Tooltip("Specifies the type of animation that will be played when the popup closes.")] private ClosingAnimationType closingAnimationType = 0;
    [SerializeField, Tooltip("Specifies the amount of time that the closing animation will last unless the closing animation type is instant.")] private float closingAnimationLength = 0;
    [SerializeField, Tooltip("Specifies the sound effect that is played when the popup is just starting to close.")] private AudioClip closingSFX = null;
    [SerializeField, Tooltip("Specifies the sound effect that is played when the popup finishes closing.")] private AudioClip closedSFX = null;
    [SerializeField, Tooltip("Specifies the sound effecr that is played when the popup's close button is clicked by the player.")] private AudioClip clickCloseButtonSFX = null;
    [SerializeField, Tooltip("Specifies whether the popup game object should be destroyed whenever the popup is closed.")] private bool destroyOnClose = false;

    [Header("Other Popup Options")]

    [SerializeField, Tooltip("Specifies whether the popup can be dragged around the screen by the player.")] private bool isDraggable = true;
    [SerializeField, Tooltip("Specifies whether the popup will have an extra background image that serves as a giant raycast blocker.")] private bool raycastBlockerBackground = false;

    //Non-inspector variables.

    public enum OpeningAnimationType
    {
        /// <summary>
        /// The popup instantly opens up to its normal state.
        /// </summary>
        Instant,
        /// <summary>
        /// The popup starts at scale 0 and gradually expands until it is scale 1 according to the opening animation length.
        /// </summary>
        Expand,
        /// <summary>
        /// The popup starts at opacity 0 and gradually fades in until it is at opacity 1 according to the opening animation length.
        /// </summary>
        Fade
    }

    public enum ClosingAnimationType
    {
        /// <summary>
        /// The popup instantly closes.
        /// </summary>
        Instant,
        /// <summary>
        /// The popup starts at scale 1 and gradually shrinks until it is scale 0 according to the closing animation length.
        /// </summary>
        Shrink,
        /// <summary>
        /// The popup starts at opacity 1 and gradually fades out until it is at opacity 0 according to the closing animation length.
        /// </summary>
        Fade
    }

    /// <summary>
    /// Private variable that indicates how much time has elapsed since the opening animation started and should be used in coordination with the openingAnimationLength variable.
    /// </summary>
    protected float openingAnimationTimeElapsed = 0;

    /// <summary>
    /// Private variable that indicates how much time has elapsed since the closing animation started and should be used in coordination with the closingAnimationLength variable.
    /// </summary>
    protected float closingAnimationTimeElapsed = 0;

    /// <summary>
    /// Publicly accessible and privately mutable property that indicates whether or not the popup is opening and playing its opening animation. 
    /// </summary>
    public bool opening { get; private set; }

    /// <summary>
    /// Publicly accessible and privately mutable property that indicates whether or not the popup is opening and playing its closing animation. 
    /// </summary>
    public bool closing { get; private set; }

    /// <summary>
    /// Publicly accessible property that indicates whether or not the popup is open.
    /// </summary>
    public bool open { get => gameObject.activeSelf; }

    /// <summary>
    /// Publicly accessible property that indicates whether or not the popup is closed.
    /// </summary>
    public bool closed { get => !gameObject.activeSelf; }

    /// <summary>
    /// Private variable that indicates the offset of the pointer from the popup's position.
    /// </summary>
    protected Vector2 pointerOffset = Vector2.zero;

    /// <summary>
    /// Public property that indicates whether or not the popup is being dragged by the player.
    /// </summary>
    public bool beingDragged { get; private set; } = false;

    /// <summary>
    /// Private reference variable to the parent canvas of the popup.
    /// </summary>
    protected Canvas parentCanvas = null;

    /// <summary>
    /// Public property that indicates whether or not the popup is the top popup (the top popup is the one with the last sibling index).
    /// </summary>
    public bool isTopPopup { get => transform.GetSiblingIndex() == transform.parent.childCount - 1; }

    /// <summary>
    /// Protected property that should be accessed in order to obtain the popup's individual canvas group which could be useful for messing with the overall opacity for fading opening and closing animations.
    /// </summary>
    protected CanvasGroup canvasGroup { get; private set; } = null;

    /// <summary>
    /// Protected property that should be accessed in order to obtain the popup's raycast blocker background image if it exists.
    /// </summary>
    protected Image raycastBlockerBackgroundImage { get; private set; } = null;

    /// <summary>
    /// Private static list that contains all the popups that currently exist within the game.
    /// </summary>
    private static List<NewGalaxyPopupBehaviour> popups = null;

    /// <summary>
    /// Public static property that should be accessed in order to determine if any popup that exists is currently open and activated.
    /// </summary>
    public static bool isAPopupOpen
    {
        get
        {
            if (popups == null)
                return false;
            foreach (NewGalaxyPopupBehaviour popup in popups)
                if (popup.open)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Public static property that should be accessed in order to determine if any popup that exists is being dragged, which may be useful information for a camera object such as the main galaxy camera.
    /// </summary>
    public static bool isAPopupBeingDragged
    {
        get
        {
            if (popups == null)
                return false;
            foreach (NewGalaxyPopupBehaviour popup in popups)
                if (popup.beingDragged)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Private static property that holds the game time when the previous popup was closed (initialized to 0).
    /// </summary>
    private static float previousPopupCloseTime { get; set; } = 0;

    /// <summary>
    /// Public static property that should be accessed in order to determine if a popup has been closed on the current frame.
    /// </summary>
    public static bool popupClosedOnFrame { get => Mathf.Approximately(previousPopupCloseTime, Time.time); }

    protected virtual void Awake()
    {
        //Sets the parent canvas reference variable.
        parentCanvas = GetComponentInParent<Canvas>();
        //Sets the canvas group reference property.
        canvasGroup = GetComponent<CanvasGroup>() == null ? gameObject.AddComponent<CanvasGroup>() : GetComponent<CanvasGroup>();
        //Initializes the list of popups if it has not been initialized yet.
        if (popups == null)
            popups = new List<NewGalaxyPopupBehaviour>();
        //Adds the popup to the list of popups.
        popups.Add(this);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (opening)
            OpeningAnimationUpdate();
        else if (closing)
            ClosingAnimationUpdate();

        if (Input.GetKeyDown(KeyCode.Escape) && isTopPopup && !GalaxyConfirmationPopupBehaviour.isAConfirmationPopupOpen && !popupClosedOnFrame)
            Close();
    }

    /// <summary>
    /// Public method that should be called in order to open and enable the popup.
    /// </summary>
    public virtual void Open()
    {
        //Enables the popup game object if the popup is not already open, which will call the OnEnable method.
        if (!open && !opening)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// Public method that should be called in order to close and either disable or destroy the popup.
    /// </summary>
    public virtual void Close()
    {
        //Ends the current opening animation if there is one and also switches the closing animation type to instant if the popup was closing.
        ClosingAnimationType closingAnimationTypeTemp = closingAnimationType;
        if (opening)
        {
            EndOpeningAnimation();
            closingAnimationType = ClosingAnimationType.Instant;
        }
        //Begins the process of closing the popup if it is not already in the process of closing.
        if(!closed && !closing)
        {
            //Destroys the raycast blocker background image if it exists.
            if (raycastBlockerBackgroundImage != null)
                Destroy(raycastBlockerBackgroundImage.gameObject);
            BeginClosingAnimation();
        }
        //Restores the initial closing animation type in case it was tampered with due to the close function being called while the popup was still opening.
        closingAnimationType = closingAnimationTypeTemp;
    }

    /// <summary>
    /// Protected method that is called whenever the popup begins its opening animation.
    /// </summary>
    protected virtual void BeginOpeningAnimation()
    {
        //Plays the appropriate sound effect for when the opening animation is just starting.
        AudioManager.PlaySFX(openingSFX);
        //Logs that no time has elapsed as of yet since the popup has begun opening.
        openingAnimationTimeElapsed = 0;
        //Logs that the popup is currently opening.
        opening = true;

        //Ends the opening animation instantly if the opening animation type is instant.
        if (openingAnimationType == OpeningAnimationType.Instant)
        {
            EndOpeningAnimation();
        }
        else if (openingAnimationType == OpeningAnimationType.Expand)
        {
            transform.localScale = Vector3.zero;
        }
        else if (openingAnimationType == OpeningAnimationType.Fade)
        {
            canvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// Protected method that is called every frame when the popup is in the middle of opening.
    /// </summary>
    protected virtual void OpeningAnimationUpdate()
    {
        //Adds the amount of time thats passed since the last frame update to the amount of time that the opening animation has been playing.
        openingAnimationTimeElapsed += Time.deltaTime;

        //Adjusts the scale of the popup based on how much opening animation time has elapsed if the opening animation type is expand.
        if(openingAnimationType == OpeningAnimationType.Expand)
        {
            float scale = openingAnimationLength <= 0 ? 1 : openingAnimationTimeElapsed / openingAnimationLength > 1 ? 1 : openingAnimationTimeElapsed / openingAnimationLength;
            transform.localScale = new Vector2(scale, scale);
        }
        //Adjusts the opacity of the popup's canvas group based on how much opening animation time has elapsed if the opening animation type is fade.
        else if (openingAnimationType == OpeningAnimationType.Fade)
        {
            canvasGroup.alpha = openingAnimationTimeElapsed / openingAnimationLength;
        }

        //Ends the opening animation if the required length of time has passed.
        if (openingAnimationTimeElapsed >= openingAnimationLength)
            EndOpeningAnimation();
    }

    /// <summary>
    /// Protected method that is called whenever the popup ends its opening animation.
    /// </summary>
    protected virtual void EndOpeningAnimation()
    {
        //Resets the scale of the popup.
        transform.localScale = Vector2.one;
        //Resets the opacity of the popup's canvas group.
        canvasGroup.alpha = 1;
        //Resets the variable that indicates how much time has passed since the opening animation started.
        openingAnimationTimeElapsed = 0;
        //Logs that the popup is no longer in the process of opening.
        opening = false;
        //Plays the appropriate sound effect for when the popup has fully finished opening.
        AudioManager.PlaySFX(openedSFX);
    }

    /// <summary>
    /// Protected method that is called whenever the popup begins its closing animation.
    /// </summary>
    protected virtual void BeginClosingAnimation()
    {
        //Plays the appropriate sound effect for when the closing animation is just starting.
        AudioManager.PlaySFX(closingSFX);
        //Logs that no time has elapsed as of yet since the popup has begun closing.
        closingAnimationTimeElapsed = 0;
        //Logs that the popup is currently closing.
        closing = true;

        //Ends the closing animation instantly if the closing animation type is instant.
        if (closingAnimationType == ClosingAnimationType.Instant)
        {
            EndClosingAnimation();
            return;
        }
    }

    /// <summary>
    /// Protected method that is called every frame when the popup is in the middle of closing.
    /// </summary>
    protected virtual void ClosingAnimationUpdate()
    {
        //Adds the amount of time thats passed since the last frame update to the amount of time that the closing animation has been playing.
        closingAnimationTimeElapsed += Time.deltaTime;

        //Adjusts the scale of the popup based on how much closing animation time has elapsed if the closing animation type is shrink.
        if (closingAnimationType == ClosingAnimationType.Shrink)
        {
            float scale = closingAnimationLength <= 0 ? 0 : 1 - (closingAnimationTimeElapsed / closingAnimationLength) < 0 ? 0 : 1 - (closingAnimationTimeElapsed / closingAnimationLength);
            transform.localScale = new Vector2(scale, scale);
        }
        //Adjusts the opacity of the popup's canvas group based on how much closing animation time has elapsed if the closing animation type is fade.
        else if (closingAnimationType == ClosingAnimationType.Fade)
        {
            canvasGroup.alpha = 1 - (closingAnimationTimeElapsed / closingAnimationLength);
        }

        //Ends the closing animation if the required length of time has passed.
        if (closingAnimationTimeElapsed >= closingAnimationLength)
            EndClosingAnimation();
    }

    /// <summary>
    /// Protected method that is called whenever the popup ends its closing animation.
    /// </summary>
    protected virtual void EndClosingAnimation()
    {
        //Logs the time at the beginning of the frame as the time when this popup was closed.
        previousPopupCloseTime = Time.time;

        //Destroys the popup's entire game object if the destroy on close option is enabled/true.
        if (destroyOnClose)
        {
            Destroy(gameObject);
            return;
        }

        //Deactivates the popup's entire game object.
        gameObject.SetActive(false);
        //Sets the popup as the first child of the popups parent object since the last child is the one that is currently selected and the popup is closing.
        transform.SetAsFirstSibling();
        //Resets the scale of the popup.
        transform.localScale = Vector2.one;
        //Resets the opacity/alpha of the popup's individual canvas group.
        canvasGroup.alpha = 1;
        //Resets the variable that indicates how much time has passed since the closing animation started.
        closingAnimationTimeElapsed = 0;
        //Logs that the popup is no longer in the process of closing.
        closing = false;
        //Plays the appropriate sound effect for when the popup has fully finished closing.
        AudioManager.PlaySFX(closedSFX);
    }

    /// <summary>
    /// Public method that is called by the IBeginDragHandler interface whenever the player begins to drag on the popup and remembers the mouse's offset from the popup's position.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public virtual void OnBeginDrag(PointerEventData pointerEventData)
    {
        //Returns if the popup is not able to be dragged by the player.
        if (!isDraggable)
            return;
        //Sets the pointer offset.
        pointerOffset = transform.position - Input.mousePosition;
        //Logs that the popup is being dragged by the player.
        beingDragged = true;
    }

    /// <summary>
    /// Public method that is called by the IDragHandler interface whenever the player is dragging the popup and updates the popup's position relative to the mouse's position.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public virtual void OnDrag(PointerEventData pointerEventData)
    {
        //Ends the dragging and returns if the popup is not allowed to be dragged around the screen by the player.
        if(!isDraggable && beingDragged)
        {
            OnEndDrag(pointerEventData);
            return;
        }
        //Returns if the popup is not allowed to be dragged around the screen by the player.
        if (!isDraggable)
            return;
        //Updates the position of the popup without checking for boundaries.
        transform.position = (Vector2)Input.mousePosition + pointerOffset;
        //Ensures the popup doesn't go over the screen's left boundary.
        if (transform.position.x - (((RectTransform)transform).sizeDelta.x / 2) < 0)
        {
            //Snaps the popup's position to the screen's left boundary.
            transform.position = new Vector2(((RectTransform)transform).sizeDelta.x / 2, transform.position.y);
            //Sets the pointer offset.
            pointerOffset = new Vector2(transform.position.x - Input.mousePosition.x, pointerOffset.y);
            if (pointerOffset.x > ((RectTransform)transform).sizeDelta.x / 2)
                pointerOffset = new Vector2(((RectTransform)transform).sizeDelta.x / 2, pointerOffset.y);
        }
        //Ensures the popup doesn't go over the screen's right boundary.
        else if (transform.position.x + (((RectTransform)transform).sizeDelta.x / 2) > ((RectTransform)parentCanvas.transform).sizeDelta.x)
        {
            //Snaps the popup's position to the screen's right boundary.
            transform.position = new Vector2(((RectTransform)parentCanvas.transform).sizeDelta.x - (((RectTransform)transform).sizeDelta.x / 2), transform.position.y);
            //Sets the pointer offset.
            pointerOffset = new Vector2(transform.position.x - Input.mousePosition.x, pointerOffset.y);
            if (pointerOffset.x < -1 * (((RectTransform)transform).sizeDelta.x / 2))
                pointerOffset = new Vector2(-1 * (((RectTransform)transform).sizeDelta.x / 2), pointerOffset.y);
        }
        //Ensures the popup doesn't go over the screen's top boundary.
        float resourceBarHeight = NewGalaxyManager.activeInHierarchy ? NewResourceBar.resourceBarHeight : 0;
        if(transform.position.y + (((RectTransform)transform).sizeDelta.y / 2) > ((RectTransform)parentCanvas.transform).sizeDelta.y - resourceBarHeight)
        {
            //Snaps the popup's position to the screen's top boundary.
            transform.position = new Vector2(transform.position.x, ((RectTransform)parentCanvas.transform).sizeDelta.y - resourceBarHeight - (((RectTransform)transform).sizeDelta.y / 2));
            //Sets the pointer offset.
            pointerOffset = new Vector2(pointerOffset.x, transform.position.y - Input.mousePosition.y);
            if (pointerOffset.y < -1 * (((RectTransform)transform).sizeDelta.y / 2))
                pointerOffset = new Vector2(pointerOffset.x, -1 * (((RectTransform)transform).sizeDelta.y / 2));
        }
        //Ensures the popup doesn't go over the screen's bottom boundary.
        else if (transform.position.y - (((RectTransform)transform).sizeDelta.y / 2) < 0)
        {
            //Snaps the popup's position to the screen's bottom boundary.
            transform.position = new Vector2(transform.position.x, ((RectTransform)transform).sizeDelta.y / 2);
            //Sets the pointer offset.
            pointerOffset = new Vector2(pointerOffset.x, transform.position.y - Input.mousePosition.y);
            if (pointerOffset.y > ((RectTransform)transform).sizeDelta.y / 2)
                pointerOffset = new Vector2(pointerOffset.x, ((RectTransform)transform).sizeDelta.y / 2);
        }
    }

    /// <summary>
    /// Public method that is called by the IEndDragHandler interface whenever the player stops dragging the popup and resets some variables.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public virtual void OnEndDrag(PointerEventData pointerEventData)
    {
        //Resets the pointer offset.
        pointerOffset = Vector2.zero;
        //Logs that the popup is no longer being dragged by the player.
        beingDragged = false;
    }

    /// <summary>
    /// Public method that should be called by an event trigger created in the inspector whenever the player clicks the popup's close button and it plays the appropriate sound effects and closes the popup.
    /// </summary>
    public virtual void OnClickCloseButton()
    {
        //Plays the appropriate sound effect.
        AudioManager.PlaySFX(clickCloseButtonSFX);
        //Closes the popup.
        Close();
    }

    /// <summary>
    /// Protected method that is called whenever a popup is destroyed and removes the popup from the static list of popups.
    /// </summary>
    protected void OnDestroy()
    {
        //Destroys the raycast blocker background image if it still exists for whatever reason.
        if (raycastBlockerBackgroundImage != null)
            Destroy(raycastBlockerBackgroundImage.gameObject);
        //Removes the popup from the static list of popups.
        popups.Remove(this);
        //Sets the list of popups to null if there are currently no popups existing within the game.
        if (popups.Count == 0)
            popups = null;
    }

    /// <summary>
    /// Protected method that is called whenever the popup is disabled.
    /// </summary>
    protected virtual void OnDisable()
    {
        //Destroys the raycast blocker background image if it still exists for whatever reason.
        if (raycastBlockerBackgroundImage != null)
            Destroy(raycastBlockerBackgroundImage.gameObject);
    }

    /// <summary>
    /// Protected method that is called whenever the popup is first enabled or whenever it is opened.
    /// </summary>
    protected virtual void OnEnable()
    {
        //Makes this popup the top popup.
        transform.SetAsLastSibling();
        //Instantiates a new raycast blocker background if needed.
        if (raycastBlockerBackground)
            CreateRaycastBlockerBackgroundImage();
        //Begins the popup's opening animation.
        BeginOpeningAnimation();
    }

    /// <summary>
    /// Protected method that is called by the OnEnable method whenever the popup is opened and a raycast blocker background image is supposed to be created for the popup, which prevents any raycast targets other than the ones on the popup from being a problem.
    /// </summary>
    protected virtual void CreateRaycastBlockerBackgroundImage()
    {
        //Creates a new game object with an image component.
        raycastBlockerBackgroundImage = (new GameObject()).AddComponent<Image>();
        //Names the game object.
        raycastBlockerBackgroundImage.gameObject.name = gameObject.name + " Raycast Blocker Background Image";
        //Sets the size of the blocker to match the size of the parent canvas and fill the entire screen.
        raycastBlockerBackgroundImage.rectTransform.sizeDelta = ((RectTransform)parentCanvas.transform).sizeDelta;
        //Sets the color of the image to be very transparent gray.
        raycastBlockerBackgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 100f / 255f);
        //Specifies that the image is definitely supposed to be a raycast target itself.
        raycastBlockerBackgroundImage.raycastTarget = true;
        //Sets the image's parent as the popups parent.
        raycastBlockerBackgroundImage.transform.SetParent(transform.parent);
        //Sets the sibling index of the background image to make it one below this popup.
        raycastBlockerBackgroundImage.transform.SetSiblingIndex(transform.GetSiblingIndex());
        //Resets the scale of the raycast blocker background image.
        raycastBlockerBackgroundImage.transform.localScale = Vector3.one;
        //Resets the position of the raycast blocker background image.
        raycastBlockerBackgroundImage.transform.localPosition = Vector3.zero;
    }
}
