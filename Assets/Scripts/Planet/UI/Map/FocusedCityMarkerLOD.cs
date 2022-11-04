using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusedCityMarkerLOD : MapMarkerLOD
{
    Vector3 labelOffsetInWorldSpace;

    public override void InitializeMarkerLOD(MapMarker marker)
    {
        base.InitializeMarkerLOD(marker);

        labelOffsetInWorldSpace = new Vector3(0.0f, 0.0f, marker.GetMarkedObject().GetComponent<City>().radius + 20.0f);
    }

    protected override Vector3 GetMarkerLocationInWorldSpace(Transform markedObject)
    {
        return markedObject.position + labelOffsetInWorldSpace;
    }
}
