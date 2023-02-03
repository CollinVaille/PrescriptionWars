using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGalaxyPopupBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Base Opening Options")]

    [SerializeField, Tooltip("Specifies the type of animation that will be played when the popup opens.")] protected OpeningAnimationType openingAnimationType = 0;
    [SerializeField, Tooltip("Specifies the amount of time that the opening animation will last unless the opening animation type is instant.")] protected float openingAnimationLength = 0;

    [Header("Base Closing Options")]

    [SerializeField, Tooltip("Specifies the type of animation that will be played when the popup closes.")] protected ClosingAnimationType closingAnimationType = 0;
    [SerializeField, Tooltip("Specifies the amount of time that the closing animation will last unless the closing animation type is instant.")] protected float closingAnimationLength = 0;
    [SerializeField, Tooltip("Specifies whether the popup game object should be destroyed whenever the popup is closed.")] protected bool destroyOnClose = false;

    //Non-inspector variables.

    public enum OpeningAnimationType
    {
        /// <summary>
        /// The popup instantly opens up to full size.
        /// </summary>
        Instant,
        /// <summary>
        /// The popup starts at scale 0 and gradually expands until it is scale 1 according to the opening animation length.
        /// </summary>
        Expand
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
        Shrink
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
    /// Private static list that contains all the popups that currently exist within the game.
    /// </summary>
    private static List<NewGalaxyPopupBehaviour> popups = null;

    /// <summary>
    /// Private static property that should be accessed in order to determine if any popup that exists is being dragged, which may be useful information for a camera object such as the main galaxy camera.
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

    protected virtual void Awake()
    {
        //Sets the parent canvas reference variable.
        parentCanvas = GetComponentInParent<Canvas>();
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

        if (Input.GetKeyDown(KeyCode.Escape) && isTopPopup)
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
        if (opening)
        {
            EndOpeningAnimation();
            closingAnimationType = ClosingAnimationType.Instant;
        }
        //Begins the process of closing the popup if it is not already in the process of closing.
        if(!closed && !closing)
            BeginClosingAnimation();
    }

    /// <summary>
    /// Protected method that is called whenever the popup begins its opening animation.
    /// </summary>
    protected virtual void BeginOpeningAnimation()
    {
        openingAnimationTimeElapsed = 0;
        opening = true;

        if (openingAnimationType == OpeningAnimationType.Instant)
        {
            EndOpeningAnimation();
            return;
        }
    }

    /// <summary>
    /// Protected method that is called every frame when the popup is in the middle of opening.
    /// </summary>
    protected virtual void OpeningAnimationUpdate()
    {
        openingAnimationTimeElapsed += Time.deltaTime;

        if(openingAnimationType == OpeningAnimationType.Expand)
        {
            float scale = openingAnimationLength <= 0 ? 1 : openingAnimationTimeElapsed / openingAnimationLength > 1 ? 1 : openingAnimationTimeElapsed / openingAnimationLength;
            transform.localScale = new Vector2(scale, scale);
        }

        if (openingAnimationTimeElapsed >= openingAnimationLength)
            EndOpeningAnimation();
    }

    /// <summary>
    /// Protected method that is called whenever the popup ends its opening animation.
    /// </summary>
    protected virtual void EndOpeningAnimation()
    {
        transform.localScale = Vector2.one;
        openingAnimationTimeElapsed = 0;
        opening = false;
    }

    /// <summary>
    /// Protected method that is called whenever the popup begins its closing animation.
    /// </summary>
    protected virtual void BeginClosingAnimation()
    {
        closingAnimationTimeElapsed = 0;
        closing = true;

        if(closingAnimationType == ClosingAnimationType.Instant)
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
        closingAnimationTimeElapsed += Time.deltaTime;

        if(closingAnimationType == ClosingAnimationType.Shrink)
        {
            float scale = closingAnimationLength <= 0 ? 0 : 1 - (closingAnimationTimeElapsed / closingAnimationLength) < 0 ? 0 : 1 - (closingAnimationTimeElapsed / closingAnimationLength);
            transform.localScale = new Vector2(scale, scale);
        }

        if (closingAnimationTimeElapsed >= closingAnimationLength)
            EndClosingAnimation();
    }

    /// <summary>
    /// Protected method that is called whenever the popup ends its closing animation.
    /// </summary>
    protected virtual void EndClosingAnimation()
    {
        if (destroyOnClose)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
        transform.SetAsFirstSibling();
        transform.localScale = Vector2.one;
        closingAnimationTimeElapsed = 0;
        closing = false;
    }

    /// <summary>
    /// Public method that is called by the IBeginDragHandler interface whenever the player begins to drag on the popup and remembers the mouse's offset from the popup's position.
    /// </summary>
    /// <param name="pointerEventData"></param>
    public virtual void OnBeginDrag(PointerEventData pointerEventData)
    {
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
        //Updates the position of the popup without checking for boundaries.
        transform.position = (Vector2)Input.mousePosition + pointerOffset;
        //Ensures the popup doesn't go over the screen's left boundary.
        if (transform.position.x - (((RectTransform)transform).sizeDelta.x / 2) < 0)
        {
            //Snaps the popup's position to the screen's left boundary.
            transform.position = new Vector2(((RectTransform)transform).sizeDelta.x / 2, transform.position.y);
            //Sets the pointer offset.
            pointerOffset = transform.position - Input.mousePosition;
        }
        //Ensures the popup doesn't go over the screen's right boundary.
        else if (transform.position.x + (((RectTransform)transform).sizeDelta.x / 2) > ((RectTransform)parentCanvas.transform).sizeDelta.x)
        {
            //Snaps the popup's position to the screen's right boundary.
            transform.position = new Vector2(((RectTransform)parentCanvas.transform).sizeDelta.x - (((RectTransform)transform).sizeDelta.x / 2), transform.position.y);
            //Sets the pointer offset.
            pointerOffset = transform.position - Input.mousePosition;
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
    /// Protected method that is called whenever a popup is destroyed and removes the popup from the static list of popups.
    /// </summary>
    protected void OnDestroy()
    {
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
        
    }

    /// <summary>
    /// Protected method that is called whenever the popup is first enabled or whenever it is opened.
    /// </summary>
    protected virtual void OnEnable()
    {
        //Begins the popup's opening animation.
        BeginOpeningAnimation();
    }
}
