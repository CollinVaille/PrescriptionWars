using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Vertical scalers are things like elevators and ladders that are used to transport pills up and down (and thus can be used to vertically scale walls/structures).
//They have an additional requirement that their vertical scale be dynamically editable. This is so city foundations of dynamic height can use them as entrances.
//Vertical scalers belong to a city. Each city keeps a list of all its vertical scalers under its vertical scaler manager.
public class VerticalScaler : MonoBehaviour
{
    //Customization
    public float width; //Width if its shape is rectangular and diameter if its shape is circular

    //Needed for save system (VerticalScalerJSON)
    [HideInInspector] public bool invertConnectorsXPosition;
    [HideInInspector] public string resourcePath;

    public void ScaleToHeightAndConnect(float heightToScaleTo, bool invertConnectorsXPosition)
    {
        //Scale it
        GetVerticalScalerImplement().ScaleToHeight(heightToScaleTo);

        //Find the connector
        Transform connector = transform.Find("Upper Level Connector");

        //Configure the connector
        this.invertConnectorsXPosition = invertConnectorsXPosition;
        if(connector)
        {
            //Position the connector vertically
            Vector3 restoredConnectorPosition = connector.localPosition;
            restoredConnectorPosition.y = heightToScaleTo;
            connector.localPosition = restoredConnectorPosition;

            //Invert the connector if needed
            if (invertConnectorsXPosition)
            {
                foreach (Transform t in connector)
                {
                    Vector3 pos = t.localPosition;
                    pos.x = -pos.x;
                    t.localPosition = pos;
                }
            }
        }
    }

    public void SetYAxisRotation(int yAxisRotation, bool inLocalSpace = true)
    {
        Vector3 verticalScalerRotation = Vector3.zero;
        verticalScalerRotation.y = yAxisRotation;

        if(inLocalSpace)
            transform.localEulerAngles = verticalScalerRotation;
        else
            transform.eulerAngles = verticalScalerRotation;
    }

    public float GetHeight()
    {
        return GetVerticalScalerImplement().GetHeight();
    }

    private IVerticalScalerImplement GetVerticalScalerImplement()
    {
        IVerticalScalerImplement verticalScalerImplement = GetComponent<IVerticalScalerImplement>();

        if (verticalScalerImplement != null)
            return verticalScalerImplement;

        return GetComponentInChildren<IVerticalScalerImplement>();
    }
}

[System.Serializable]
public class VerticalScalerJSON
{
    public string resourcePath;
    public Vector3 location;
    public int yAxisRotation;
    public float verticalScale;
    public bool invertConnectorsXPosition;

    public VerticalScalerJSON(VerticalScaler verticalScaler)
    {
        Transform verticalScalerTransform = verticalScaler.transform;

        resourcePath = verticalScaler.resourcePath;
        location = verticalScalerTransform.localPosition;
        yAxisRotation = (int)verticalScalerTransform.localEulerAngles.y;
        verticalScale = verticalScaler.GetHeight();
        invertConnectorsXPosition = verticalScaler.invertConnectorsXPosition;
    }

    public void RestoreVerticalScaler(VerticalScalerManager verticalScalerManager)
    {
        VerticalScaler verticalScaler = verticalScalerManager.InstantiateVerticalScaler(resourcePath, verticalScalerManager.city.transform);

        verticalScaler.transform.localPosition = location;
        verticalScaler.SetYAxisRotation(yAxisRotation);

        verticalScaler.ScaleToHeightAndConnect(verticalScale, invertConnectorsXPosition);
    }
}

public interface IVerticalScalerImplement
{
    void ScaleToHeight(float heightToScaleTo);

    float GetHeight();
}