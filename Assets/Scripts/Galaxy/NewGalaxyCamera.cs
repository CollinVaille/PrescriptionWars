using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyCamera : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField, Tooltip("Multiplier of the speed at which the camera zooms into the galaxy.")] private float scrollSpeed = 30;
    [SerializeField, Tooltip("Indictates what the minimum and maximum amounts are that the player can zoom.")] private Vector2Int zoomRange = new Vector2Int(1654, 20);
    [SerializeField, Tooltip("Multiplier of the speed at which the camera can be dragged by the mouse.")]private float dragSpeed = 50;

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
    /// Indicates whether the mouse is in the viewport of the screen and therefore visible to the player.
    /// </summary>
    public static bool isMouseInViewport { get => Input.mousePosition.x >= 0 && Input.mousePosition.x <= galaxyCamera.cameraComponent.pixelWidth && Input.mousePosition.y >= 0 && Input.mousePosition.y <= galaxyCamera.cameraComponent.pixelHeight; }

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
        if (Input.GetMouseButton(0) && NewGalaxyManager.activeInHierarchy && isMouseInViewport)
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
        //Stores the initial zoom percentage.
        float initialZoomPercentage = zoomPercentage;
        //Updates the zoom amount based on the mouse wheel.
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (Input.mouseScrollDelta.y * scrollSpeed), transform.localPosition.z);
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
        galaxyCamera.zoomFunctions.Add(zoomFunction);
    }

    /// <summary>
    /// This public static method should be called in order to remove a function from the list of functions that need to be executed whenever the zoom percentage on the galaxy camera changes.
    /// </summary>
    /// <param name="zoomFunction"></param>
    public static void RemoveZoomFunction(Action zoomFunction)
    {
        galaxyCamera.zoomFunctions.Remove(zoomFunction);
    }
}
