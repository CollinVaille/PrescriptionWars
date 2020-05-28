using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMarker : MonoBehaviour
{
    private Transform markedObject;
    private Camera mapCamera;

    private Text textLabel;
    private int fontSize;

    public void InitializeMarker (Transform markedObject)
    {
        //Remember object we're marking
        this.markedObject = markedObject;

        //Make it a child of canvas
        transform.SetParent(God.god.pauseMenus[(int)God.MenuScreen.MapMenu], false);

        //Set name and text
        name = markedObject.name + " Marker";
        textLabel = GetComponent<Text>();
        textLabel.text = markedObject.name;

        //Get other references
        mapCamera = God.god.mapCamera;
    }

    private void Update()
    {
        if (!markedObject)
            return;

        //Update position of map marker
        Vector3 screenPosition = mapCamera.WorldToScreenPoint(markedObject.position);
        screenPosition.z = 0;
        transform.position = screenPosition;

        //Update size of map marker text
        fontSize = (int)(10000 / mapCamera.orthographicSize);
        if(fontSize != textLabel.fontSize)
            textLabel.fontSize = fontSize;
    }

    public void OnMouseUp()
    {
        Debug.Log("Clicked " + name);
    }
}
