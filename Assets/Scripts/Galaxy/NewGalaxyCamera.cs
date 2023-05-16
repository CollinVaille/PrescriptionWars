using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGalaxyCamera : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField, Tooltip("Indicates the minimum (x) and maximum (y) speeds at which the player can scroll and zoom in the camera into the galaxy.")] private Vector2Int zoomSpeedRange = new Vector2Int(2, 50);
    [SerializeField, Tooltip("Indictates what the minimum (x) and maximum (y) amounts (camera y position) are that the player can zoom.")] private Vector2 zoomRange = new Vector2(1654, 0.5f);
    [SerializeField, Tooltip("Indicates the minimum (x) and maximum (y) speeds at which the player can drag the camera within the galaxy, which scales down with the zoom percentage.")] private Vector2Int dragSpeedRange = new Vector2Int(2, 30);
    [SerializeField, Tooltip("Indicates the zoom percentage at which the planets should become visible to the player.")] private float planetsVisibleZoomPercentage = 0.95f;
    [SerializeField, Tooltip("Indicates the zoom percentage at which the planetary orbit outlines become visible to the player.")] private float planetaryOrbitOutlinesVisibleZoomPercentage = 0.90f;
    [SerializeField, Tooltip("Indicates the zoom percentage at which the star name labels become visible to the player.")] private float starNameLabelsVisibleZoomPercentage = 0.90f;

    //Non-inspector variables.

    /// <summary>
    /// Private reference of the actual camera component that is attached to the same game object as this galaxy camera script.
    /// </summary>
    private Camera cameraComponentVar = null;
    /// <summary>
    /// Property that should be accessed in order to obtain the actual camera component that is attached to the same game object as this galaxy camera script.
    /// </summary>
    private Camera cameraComponent
    {
        get
        {
            if (cameraComponentVar != null)
                return cameraComponentVar;
            cameraComponentVar = gameObject.GetComponent<Camera>();
            return cameraComponentVar;
        }
    }

    /// <summary>
    /// Public static property that should be accessed in order to determine how far, percentage wise (0-1 with 0 representing fully zoomed out), the galaxy camera is zoomed in.
    /// </summary>
    public static float zoomPercentage { get => galaxyCamera == null ? 0 : 1 - ((galaxyCamera.transform.localPosition.y - galaxyCamera.zoomRange.y) / (galaxyCamera.zoomRange.x - galaxyCamera.zoomRange.y));}

    /// <summary>
    /// Private float property that should be accessed in order to determine the speed at which the camera is able to be zoomed in and out by the player scrolling, which depends on the zoom percentage and the allowed zoom speed range.
    /// </summary>
    private float zoomSpeed { get => zoomSpeedRange.y - (zoomPercentage * (zoomSpeedRange.y - zoomSpeedRange.x)); }

    /// <summary>
    /// Private float property that should be accessed in order to determine the speed at which the camera is able to be dragged by the mouse, which depends on the zoom percentage and the allowed drag speed range.
    /// </summary>
    private float dragSpeed { get => dragSpeedRange.y - (zoomPercentage * (dragSpeedRange.y - dragSpeedRange.x)); }

    /// <summary>
    /// Public static property that should be accessed in order to determine if the planets in the galaxy are visible to the player based on the zoom percentage of the galaxy camera.
    /// </summary>
    public static bool planetsVisible { get => galaxyCamera == null ? false : zoomPercentage >= galaxyCamera.planetsVisibleZoomPercentage; }

    /// <summary>
    /// Public static property that should be accessed in order to determine if the planetary orbit outlines in the galaxy are visible to the player based on the zoom percentage of the galaxy camera.
    /// </summary>
    public static bool planetaryOrbitOutlinesVisible { get => galaxyCamera == null ? false : zoomPercentage >= galaxyCamera.planetaryOrbitOutlinesVisibleZoomPercentage; }

    /// <summary>
    /// Public static property that should be accessed in order to determine if the star name labels in the galaxy are visible to the player based on the zoom percentage of the galaxy camera.
    /// </summary>
    public static bool starNameLabelsVisible { get => galaxyCamera == null ? false : zoomPercentage >= galaxyCamera.starNameLabelsVisibleZoomPercentage; }

    /// <summary>
    /// Private list of functions to execute whenever the zoom percentage changes.
    /// </summary>
    private List<Action> zoomFunctions = new List<Action>();

    /// <summary>
    /// Private static reference of the galaxy camera script that is an active component of the main camera object in the galaxy scene.
    /// </summary>
    private static NewGalaxyCamera galaxyCamera = null;

    /// <summary>
    /// Private variable that indicates how much the camera should move this frame due to the mouse dragging.
    /// </summary>
    private Vector3 movementVector = Vector3.zero;
    /// <summary>
    /// Private variable that holds the mouse position of the previous frame.
    /// </summary>
    private Vector3 previousMousePosition = Vector3.zero;

    /// <summary>
    /// Private variable that indicates whether or not the initial left mouse button click is valid for dragging logic (valid if initally clicked in viewport and not over UI).
    /// </summary>
    private bool initialLeftMouseButtonClickDragValid = false;

    /// <summary>
    /// Indicates whether the mouse is in the viewport of the screen and therefore visible to the player.
    /// </summary>
    public static bool isMouseInViewport { get => galaxyCamera == null ? false : Input.mousePosition.x >= 0 && Input.mousePosition.x <= galaxyCamera.cameraComponent.pixelWidth && Input.mousePosition.y >= 0 && Input.mousePosition.y <= galaxyCamera.cameraComponent.pixelHeight; }

    /// <summary>
    /// Publicly accessible property which indicates whether the mouse is over a ui element on the screen.
    /// </summary>
    public static bool isMouseOverUI { get => EventSystem.current.IsPointerOverGameObject(); }

    private void Awake()
    {
        //Sets the static reference.
        galaxyCamera = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Initial execution of each function that needs to be executed when the zoom of the galaxy camera significantly changes.
        foreach (Action zoomFunction in zoomFunctions)
            zoomFunction();
    }

    // Update is called once per frame
    private void Update()
    {
        //Checks if the left mouse button was just clicked and if so then it checks whether or not the click is a valid one to begin the dragging logic.
        if (Input.GetMouseButtonDown(0))
            initialLeftMouseButtonClickDragValid = !isMouseOverUI && isMouseInViewport;

        //Updates the amount of zoom on the galaxy camera.
        ZoomUpdate();

        //Updates the player dragging the galaxy camera around.
        DragUpdate();
    }

    /// <summary>
    /// This method should be called every frame in order to update the camera being dragged.
    /// </summary>
    private void DragUpdate()
    {
        //Resets the movement vector.
        movementVector.x = 0.0f;
        movementVector.y = 0.0f;

        //Click and drag movement
        if (initialLeftMouseButtonClickDragValid && Input.GetMouseButton(0) && NewGalaxyManager.activeInHierarchy && isMouseInViewport && !isMouseOverUI && !NewGalaxyPopupBehaviour.isAPopupBeingDragged)
        {
            movementVector.x += previousMousePosition.x - Input.mousePosition.x;
            movementVector.y += previousMousePosition.y - Input.mousePosition.y;
        }

        //Apply movement
        transform.Translate(movementVector * dragSpeed * Time.deltaTime);

        //Clean up at end of update
        previousMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// Private method that should be called every frame in order to update the zoom of the galaxy camera.
    /// </summary>
    private void ZoomUpdate()
    {
        //Returns if certain conditions are not met for the mouse wheel to affect the zoom percentage.
        if (!isMouseInViewport || isMouseOverUI || NewGalaxyPopupBehaviour.isAPopupBeingDragged)
            return;
        //Stores the initial zoom percentage.
        float initialZoomPercentage = zoomPercentage;
        //Updates the zoom amount based on the mouse wheel.
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (Input.mouseScrollDelta.y * zoomSpeed), transform.localPosition.z);
        //Ensures that the camera is not too zoomed out.
        if (transform.localPosition.y > zoomRange.x)
            transform.localPosition = new Vector3(transform.localPosition.x, zoomRange.x, transform.localPosition.z);
        //Ensures that the camera is not too zoomed in.
        if (transform.localPosition.y < zoomRange.y)
            transform.localPosition = new Vector3(transform.localPosition.x, zoomRange.y, transform.localPosition.z);
        //Executes each zoom function if the zoom percentage has meaningfully changed.
        if (!Mathf.Approximately(zoomPercentage, initialZoomPercentage))
        {
            foreach(Action zoomFunction in zoomFunctions)
            {
                zoomFunction();
            }
        }
    }

    /// <summary>
    /// This public static method should be called in order to add a function to the list of functions that need to be executed whenever the zoom percentage on the galaxy camera changes.
    /// </summary>
    /// <param name="zoomFunction"></param>
    public static void AddZoomFunction(Action zoomFunction)
    {
        if(galaxyCamera != null)
            galaxyCamera.zoomFunctions.Add(zoomFunction);
    }

    /// <summary>
    /// This public static method should be called in order to remove a function from the list of functions that need to be executed whenever the zoom percentage on the galaxy camera changes.
    /// </summary>
    /// <param name="zoomFunction"></param>
    public static void RemoveZoomFunction(Action zoomFunction)
    {
        if(galaxyCamera != null && galaxyCamera.zoomFunctions.Contains(zoomFunction))
            galaxyCamera.zoomFunctions.Remove(zoomFunction);
    }

    /// <summary>
    /// This private method is called whenever the galaxy camera instance is destroyed and resets all neeeded static variables.
    /// </summary>
    private void OnDestroy()
    {
        galaxyCamera = null;
    }
}
