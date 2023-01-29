using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGalaxyPopupBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //Non-inspector variables.

    /// <summary>
    /// Private variable that indicates the offset of the pointer from the popup's position.
    /// </summary>
    private Vector2 pointerOffset = Vector2.zero;

    /// <summary>
    /// Public property that indicates whether or not the popup is being dragged by the player.
    /// </summary>
    public bool beingDragged { get; private set; } = false;

    /// <summary>
    /// Private reference variable to the parent canvas of the popup.
    /// </summary>
    private Canvas parentCanvas = null;

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

    private void Awake()
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    /// Private method that is called whenever a popup is destroyed and removes the popup from the static list of popups.
    /// </summary>
    private void OnDestroy()
    {
        //Removes the popup from the static list of popups.
        popups.Remove(this);
        //Sets the list of popups to null if there are currently no popups existing within the game.
        if (popups.Count == 0)
            popups = null;
    }
}
