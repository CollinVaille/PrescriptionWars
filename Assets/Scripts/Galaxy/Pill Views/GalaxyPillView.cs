using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPillView : MonoBehaviour
{
    [Header("Components")]

    [SerializeField] private GameObject _pill = null;
    [SerializeField] private Camera _camera = null;

    /// <summary>
    /// Public property that should be used in order to access the bounds of the pill.
    /// </summary>
    public Bounds pillMaxBounds
    {
        get
        {
            Renderer[] renderers = _pill.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(_pill.transform.position, Vector3.zero);
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers)
                b.Encapsulate(r.bounds);
            return b;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Private method that should be called in order to update the camera to fit the whole pill into its view.
    /// </summary>
    private void UpdateCamera()
    {
        //Fetches the max bounds of the pill from the property.
        Bounds pillMaxBounds = this.pillMaxBounds;
        //Updates the camera's position on all 3 axes.
        _camera.transform.position = new Vector3(pillMaxBounds.center.x, pillMaxBounds.center.y, 0.5f + Mathf.Abs(pillMaxBounds.max.z));
        //Updates the far clip plane of the camera to capture everything no matter how far out that is under the pill on the z axis.
        _camera.farClipPlane = 1 + Mathf.Abs(pillMaxBounds.max.z) + Mathf.Abs(pillMaxBounds.min.z);
        //Calculates the camera's orthographic size needed in order to capture everything needed on the x axis.
        float xSize = Mathf.Abs(Mathf.Abs(pillMaxBounds.max.x) - Mathf.Abs(pillMaxBounds.min.x));
        //Calculates the camera's orthographic size needed in order to capture everything needed on the y axis.
        float ySize = (Mathf.Abs(pillMaxBounds.max.y) + Mathf.Abs(pillMaxBounds.min.y)) / 2;
        //Updates the camera's orthographic size to capture everything on both the x and y axes.
        _camera.orthographicSize = xSize > ySize ? xSize : ySize;
    }
}
