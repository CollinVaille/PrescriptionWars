using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetMapManager : MonoBehaviour
{
    //Customization
    public Material mapWater;

    //References
    public static PlanetMapManager mapManager;
    private Camera mapCamera;
    private Transform HUD, mapMenu;

    //Status variables
    private bool mapOpen = false;
    private Material realWater;
    private float realDeltaTime = 0.0f, lastFrameTime = 0.0f;
    private float mapZoom = 0.0f;
    private Vector3 movementVector = Vector3.zero;
    private Vector3 previousMousePosition = Vector3.zero;
    private Vector3 mapPosition = Vector3.zero;
    private MapMarker currentlyHighlighted;

    public void PerformSetUp(Transform HUD, Transform mapMenu)
    {
        mapManager = this;

        mapCamera = God.god.GetComponent<Camera>();

        this.HUD = HUD;
        this.mapMenu = mapMenu;
    }

    public void UpdateMap()
    {
        if (!mapOpen)
            return;

        realDeltaTime = Time.realtimeSinceStartup - lastFrameTime;

        //Map zoom
        mapZoom -= Input.GetAxis("Mouse ScrollWheel") * realDeltaTime * 20000;
        ClampAndApplyMapZoom();

        //Map click and drag movement
        if (Input.GetMouseButton(0))
        {
            //Get movement
            movementVector.x = (previousMousePosition.x - Input.mousePosition.x) * mapZoom / 10.0f;
            movementVector.z = (previousMousePosition.y - Input.mousePosition.y) * mapZoom / 10.0f;

            //Apply movement
            mapPosition += movementVector * realDeltaTime;
            ClampAndApplyCameraPosition();
        }

        //Update info for next frame
        previousMousePosition = Input.mousePosition;
        lastFrameTime = Time.realtimeSinceStartup;
    }

    public void SwitchToFromMap(bool toMap)
    {
        if (toMap)
        {
            //Load map
            if (!mapOpen)
                OpenMap();
        }
        else if (mapOpen) //Unload map
            CloseMap();
    }

    private void OpenMap()
    {
        //Update status
        mapOpen = true;
        mapZoom = mapCamera.orthographicSize;
        lastFrameTime = Time.realtimeSinceStartup;
        mapPosition = God.god.transform.position;

        //Update water
        if (Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
        {
            realWater = Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial;
            mapWater.color = HUD.Find("Underwater").GetComponent<Image>().color;
            Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = mapWater;
        }

        //Update camera
        God.god.SetActiveCamera(mapCamera, false);
    }

    private void CloseMap()
    {
        //Update status
        mapOpen = false;

        //Update water
        if (Planet.planet.hasOcean && Planet.planet.oceanType != Planet.OceanType.Frozen)
            Planet.planet.oceanTransform.GetComponent<Renderer>().sharedMaterial = realWater;

        //Update camera
        God.god.SetActiveCamera(Player.player.GetCamera(), false);
    }

    public Camera GetMapCamera() { return mapCamera; }

    public void HighlightMapMarker(MapMarker toHighlight)
    {
        Transform highlightLines = mapMenu.Find("Hover Lines");
        highlightLines.gameObject.SetActive(true);
        highlightLines.transform.position = toHighlight.transform.position;

        currentlyHighlighted = toHighlight;
    }

    public void RemoveHighlighting()
    {
        Transform highlightLines = mapMenu.Find("Hover Lines");
        highlightLines.gameObject.SetActive(false);

        currentlyHighlighted = null;
    }

    public void ZoomInOnMarker(MapMarker toZoomInOn)
    {
        //Center camera on map marker
        mapPosition = mapCamera.ScreenToWorldPoint(toZoomInOn.transform.position);
        ClampAndApplyCameraPosition();

        //Zoom in on map marker
        if(mapZoom > 400.0f)
        {
            mapZoom = 400.0f;
            ClampAndApplyMapZoom();
        }
    }

    public void ResetMapCamera()
    {
        mapZoom = 1000.0f;
        ClampAndApplyMapZoom();

        mapPosition = new Vector3(520.0f, 1500.0f, 520.0f);
        ClampAndApplyCameraPosition();

        if (currentlyHighlighted)
            RemoveHighlighting();
    }

    private void ClampAndApplyMapZoom()
    {
        mapZoom = Mathf.Clamp(mapZoom, 100, 1000);
        mapCamera.orthographicSize = mapZoom;
    }

    private void ClampAndApplyCameraPosition()
    {
        mapPosition.x = Mathf.Clamp(mapPosition.x, -480, 1520);
        mapPosition.z = Mathf.Clamp(mapPosition.z, -480, 1520);
        God.god.transform.position = mapPosition;

        //Highlighting on map markers needs to be readjusted if we move the camera
        if (currentlyHighlighted)
            HighlightMapMarker(currentlyHighlighted);
    }
}
