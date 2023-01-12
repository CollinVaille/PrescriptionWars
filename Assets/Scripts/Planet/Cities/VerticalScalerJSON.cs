using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VerticalScalerJSON
{
    public string resourcePath;
    public Vector3 location;
    public int yAxisRotation;
    public float verticalScale;
    public bool connectsOnLeftSide;

    public VerticalScalerJSON(Transform verticalScaler)
    {
        resourcePath = verticalScaler.name;

        location = verticalScaler.localPosition;
        yAxisRotation = (int)verticalScaler.localEulerAngles.y;
        verticalScale = verticalScaler.localScale.y;

        Transform connector = verticalScaler.Find("Upper Level Connector");
        if (connector)
            connectsOnLeftSide = connector.localPosition.x < 0.0f;
        else
            connectsOnLeftSide = false;
    }

    public void RestoreVerticalScaler(FoundationManager foundationManager)
    {
        //MAY CHANGE IN FUTURE!!!!!!!
        Transform verticalScaler = GameObject.Instantiate(Resources.Load<GameObject>("Planet/City/Miscellaneous/" + resourcePath)).transform;
        verticalScaler.parent = foundationManager.city.transform;

        verticalScaler.localPosition = location;

        Vector3 restoredLocalRotation = Vector3.zero;
        restoredLocalRotation.y = yAxisRotation;
        verticalScaler.localEulerAngles = restoredLocalRotation;

        Vector3 restoredLocalScale = Vector3.one;
        restoredLocalScale.y = verticalScale;
        verticalScaler.localScale = restoredLocalScale;

        if(connectsOnLeftSide)
        {
            Transform connector = verticalScaler.Find("Upper Level Connector");
            Vector3 restoredConnectorPosition = connector.localPosition;
            restoredConnectorPosition.x = -restoredConnectorPosition.x;
            connector.localPosition = restoredConnectorPosition;
        }

        if (foundationManager.verticalScalers == null)
            foundationManager.verticalScalers = new List<Transform>();
        foundationManager.verticalScalers.Add(verticalScaler);
    }
}
