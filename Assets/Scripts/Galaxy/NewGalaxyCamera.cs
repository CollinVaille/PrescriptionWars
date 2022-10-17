using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyCamera : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField, Tooltip("Multiplier of the speed at which the camera zooms into the galaxy.")] private float scrollSpeed = 30;
    [SerializeField, Tooltip("Indictates what the minimum and maximum amounts are that the player can zoom.")] private Vector2Int zoomRange = new Vector2Int(1654, 20);

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
    public static float zoomPercentage
    {
        get
        {
            return galaxyCamera == null ? 0 : 1 - ((galaxyCamera.transform.localPosition.y - galaxyCamera.zoomRange.y) / (galaxyCamera.zoomRange.x - galaxyCamera.zoomRange.y));
        }
    }

    /// <summary>
    /// Private static reference of the galaxy camera script that is an active component of the main camera object in the galaxy scene.
    /// </summary>
    private static NewGalaxyCamera galaxyCamera = null;

    private void Awake()
    {
        //Sets the static reference.
        galaxyCamera = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Updates the amount of zoom on the galaxy camera.
        ZoomUpdate();
    }

    /// <summary>
    /// Private method that should be called every frame in order to update the zoom of the galaxy camera.
    /// </summary>
    private void ZoomUpdate()
    {
        //Updates the zoom amount based on the mouse wheel.
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (Input.mouseScrollDelta.y * scrollSpeed), transform.localPosition.z);
        //Ensures that the camera is not too zoomed out.
        if (transform.localPosition.y > zoomRange.x)
            transform.localPosition = new Vector3(transform.localPosition.x, zoomRange.x, transform.localPosition.z);
        //Ensures that the camera is not too zoomed in.
        if (transform.localPosition.y < zoomRange.y)
            transform.localPosition = new Vector3(transform.localPosition.x, zoomRange.y, transform.localPosition.z);
    }
}
