using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDestination
{
    public Vector3 center; //In global space
    public float radius;
    public Collider[] boundaryColliders;

    //Used for bridge destinations that are simple circles or points.
    public BridgeDestination(Vector3 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    //Used for bridge destinations that have more complicated geometry like a square foundation.
    public BridgeDestination(Vector3 center, Collider[] boundaryColliders)
    {
        this.center = center;
        this.boundaryColliders = boundaryColliders;
    }

    public Vector3 GetClosestPoint(Vector3 inRelationTo)
    {
        Vector3 closestPoint;

        if (boundaryColliders != null)
            closestPoint = Foundation.GetClosestPointAmongstColliders(inRelationTo, boundaryColliders, false);
        else
            closestPoint = Vector3.MoveTowards(center, inRelationTo, radius);

        closestPoint.y = center.y;

        return closestPoint;
    }
}