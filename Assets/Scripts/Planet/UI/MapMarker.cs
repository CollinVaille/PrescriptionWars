using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapMarker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum MarkerType { Background, Player, City, Squad }

    //Type
    public MarkerType markerType;

    //Basic references
    private Transform markedObject;
    private Camera mapCamera;

    //Text variables
    private Text textLabel = null;
    private int currentFontSize = 10, fontScale = 10000;

    //Rotation and scale variables
    Vector3 rotationVector = Vector3.zero, scaleVector = Vector3.one;

    public void InitializeMarker (Transform markedObject)
    {
        //Remember object we're marking
        this.markedObject = markedObject;

        //Make it a child of canvas
        transform.SetParent(PlanetPauseMenu.pauseMenu.pauseMenus[(int)PlanetPauseMenu.MenuScreen.MapMenu], false);

        //Set name and text
        name = markedObject.name + " Marker";

        textLabel = GetComponent<Text>();
        if(textLabel)
        {
            textLabel.text = markedObject.name;
            fontScale = textLabel.fontSize * 1000;
        }

        //Get other references
        mapCamera = PlanetMapManager.mapManager.GetMapCamera();
        scaleVector = transform.localScale * 1000;
    }

    private void Update ()
    {
        if (!markedObject)
            return;

        //Update position
        Vector3 screenPosition = mapCamera.WorldToScreenPoint(markedObject.position);
        screenPosition.z = 0;
        transform.position = screenPosition;

        //Update rotation
        if(markerType == MarkerType.Player)
        {
            rotationVector.z = -markedObject.eulerAngles.y;
            transform.eulerAngles = rotationVector;
        }

        //Update size
        if (textLabel)
        {
            currentFontSize = (int)(fontScale / mapCamera.orthographicSize);
            if (currentFontSize != textLabel.fontSize)
                textLabel.fontSize = currentFontSize;
        }
        else
            transform.localScale = scaleVector / mapCamera.orthographicSize;
    }

    public void OnPointerClick (PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            if (markerType == MarkerType.Background)
                PlanetMapManager.mapManager.ResetMapCamera();
            else
                PlanetMapManager.mapManager.ZoomInOnMarker(this);
        }
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        if (markerType == MarkerType.Background)
            return;

        PlanetMapManager.mapManager.HighlightMapMarker(this);
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        if (markerType == MarkerType.Background)
            return;

        PlanetMapManager.mapManager.RemoveHighlighting();
    }
}
