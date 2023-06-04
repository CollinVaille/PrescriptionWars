using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapMarkerLOD : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Customization
    [Tooltip("If true, the size of this marker will scale with the zoom level of the map. If false, the size of this marker will remain constant.")] public bool scaleWithZoom;
    [Tooltip("(Min, Max) map zoom range where this LOD will display.")] public Vector2 zoomRange;

    //References
    private MapMarker marker;
    private Camera mapCamera;

    //Text variables
    private Text textLabel = null;
    private int currentFontSize = 10, fontScale = 10000;

    //Rotation and scale variables
    private Vector3 rotationVector = Vector3.zero, scaleVector = Vector3.one;

    public virtual void InitializeMarkerLOD(MapMarker marker)
    {
        //Get references
        this.marker = marker;
        mapCamera = PlanetMapManager.mapManager.GetMapCamera();
        textLabel = GetComponent<Text>();

        if (textLabel)
        {
            textLabel.text = marker.GetMarkedObject().name;
            fontScale = textLabel.fontSize * 1000;
        }

        //Initialize other status variables
        scaleVector = transform.localScale * 1000;
    }

    private void Update()
    {
        Transform markedObject = marker.GetMarkedObject();

        if (!markedObject)
            return;

        //Update position
        Vector3 screenPosition = mapCamera.WorldToScreenPoint(GetMarkerLocationInWorldSpace(markedObject));
        screenPosition.z = 0;
        transform.position = screenPosition;

        //Update rotation
        if (marker.markerType == MapMarker.MarkerType.Player)
        {
            rotationVector.z = -markedObject.eulerAngles.y;
            transform.eulerAngles = rotationVector;
        }

        //Update size
        if(scaleWithZoom)
        {
            if (textLabel)
            {
                currentFontSize = (int)(fontScale / mapCamera.orthographicSize);
                if (currentFontSize != textLabel.fontSize)
                    textLabel.fontSize = currentFontSize;
            }
            else
                transform.localScale = scaleVector / mapCamera.orthographicSize;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            if (marker.markerType == MapMarker.MarkerType.Background)
                PlanetMapManager.mapManager.ResetMapCamera();
            else
                PlanetMapManager.mapManager.ZoomInOnMarkerLOD(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (marker.markerType == MapMarker.MarkerType.Background)
            return;

        PlanetMapManager.mapManager.HighlightMapMarkerLOD(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (marker.markerType == MapMarker.MarkerType.Background)
            return;

        PlanetMapManager.mapManager.RemoveHighlighting();
    }

    public void UpdateLODWithNewZoomLevel(float newZoomLevel)
    {
        transform.gameObject.SetActive(newZoomLevel >= zoomRange.x && newZoomLevel < zoomRange.y);
    }

    protected virtual Vector3 GetMarkerLocationInWorldSpace(Transform markedObject)
    {
        return markedObject.position;
    }
}
