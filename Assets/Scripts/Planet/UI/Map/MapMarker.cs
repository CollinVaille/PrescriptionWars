using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarker : MonoBehaviour
{
    public enum MarkerType { Background, Player, City, Squad }

    //Type
    public MarkerType markerType;

    //References
    private Transform markedObject;
    private MapMarkerLOD[] markerLODs;

    public void InitializeMarker (Transform markedObject)
    {
        //Make it a child of the map menu in the canvas
        transform.SetParent(PlanetPauseMenu.pauseMenu.pauseMenus[(int)PlanetPauseMenu.MenuScreen.MapMenu], false);

        //Pair with object to mark
        if(markedObject)
        {
            name = markedObject.name + " Marker";
            this.markedObject = markedObject;
        }

        //Let the map manager know this marker exists so it can send signals back to update it when needed
        PlanetMapManager.mapManager.RegisterMapMarker(this);

        //Get references
        markerLODs = transform.GetComponentsInChildren<MapMarkerLOD>(true);

        //Initialize marker LODs
        foreach (MapMarkerLOD markerLOD in markerLODs)
            markerLOD.InitializeMarkerLOD(this);
    }

    public Transform GetMarkedObject() { return markedObject; }

    public void UpdateLODsWithNewZoomLevel(float newZoomLevel)
    {
        foreach (MapMarkerLOD markerLOD in markerLODs)
            markerLOD.UpdateLODWithNewZoomLevel(newZoomLevel);
    }
}
