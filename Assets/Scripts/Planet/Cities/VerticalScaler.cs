using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Vertical scalers are things like elevators and ladders that are used to transport pills up and down (and thus can be used to vertically scale walls/structures).
//They have an additional requirement that their vertical scale be dynamically editable. This is so city foundations of dynamic height can use them as entrances.
//Vertical scalers belong to a city. Each city keeps a list of all its vertical scalers under its foundation manager (since foundations use them most).
public class VerticalScaler : MonoBehaviour
{
    //Customization
    public float width; //Width if its shape is rectangular and diameter if its shape is circular

    //Needed for save system (VerticalScalerJSON)
    [HideInInspector] public string resourcePath;

    public static VerticalScaler InstantiateVerticalScaler(string resourcePath, Transform parent, FoundationManager foundationManager)
    {
        //Instantiate it
        GameObject verticalScalerPrefab = Resources.Load<GameObject>(resourcePath);
        VerticalScaler verticalScaler = Instantiate(verticalScalerPrefab, parent).GetComponent<VerticalScaler>();

        //Configure it (not all configuration is done here. only configuration that needs to be done the same way in every situation)
        verticalScaler.name = verticalScaler.name.Substring(0, verticalScaler.name.Length - 7);
        verticalScaler.resourcePath = resourcePath;

        //Add it to the save system
        if (foundationManager.verticalScalers == null)
            foundationManager.verticalScalers = new List<VerticalScaler>();
        foundationManager.verticalScalers.Add(verticalScaler);

        //Return it
        return verticalScaler;
    }

    public void ScaleToHeightAndConnect(float heightToScaleTo, bool invertConnectorsXPosition)
    {
        //Scale it
        GetComponent<IVerticalScalerImplement>().ScaleToHeight(heightToScaleTo);

        //Find the connector
        Transform connector = transform.Find("Upper Level Connector");

        //Configure the connector
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

    public void SetYAxisRotation(int yAxisRotation)
    {
        Vector3 verticalScalerRotation = Vector3.zero;
        verticalScalerRotation.y = yAxisRotation;
        transform.localEulerAngles = verticalScalerRotation;
    }

    public float GetHeight()
    {
        return GetComponent<IVerticalScalerImplement>().GetHeight();
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
        resourcePath = verticalScaler.resourcePath;
        Transform verticalScalerTransform = verticalScaler.transform;

        location = verticalScalerTransform.localPosition;
        yAxisRotation = (int)verticalScalerTransform.localEulerAngles.y;
        verticalScale = verticalScaler.GetHeight();

        Transform connector = verticalScalerTransform.Find("Upper Level Connector");
        if (connector)
            invertConnectorsXPosition = connector.localPosition.x < 0.0f;
        else
            invertConnectorsXPosition = false;
    }

    public void RestoreVerticalScaler(FoundationManager foundationManager)
    {
        VerticalScaler verticalScaler = VerticalScaler.InstantiateVerticalScaler(resourcePath, foundationManager.city.transform, foundationManager);

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