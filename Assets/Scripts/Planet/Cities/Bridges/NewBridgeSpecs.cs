using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBridgeSpecs
{
    public Vector3 point1, point2;
    public float point1Padding = 0.5f, point2Padding = 0.5f;
    public Collider[] point1Colliders, point2Colliders;
    public int bridgePrefabIndex;
    public bool verticalScalerAtDestination1 = false, verticalScalerAtDestination2 = false;

    public NewBridgeSpecs(BridgeDestinationPairing bridgeDestinationPairing, int bridgePrefabIndex)
    {
        point1 = bridgeDestinationPairing.destination1EdgePoint;
        point2 = bridgeDestinationPairing.destination2EdgePoint;

        point1Colliders = bridgeDestinationPairing.destination1.boundaryColliders;
        point2Colliders = bridgeDestinationPairing.destination2.boundaryColliders;

        this.bridgePrefabIndex = bridgePrefabIndex;

        AdjustPointsIfNeeded(bridgeDestinationPairing);
    }

    //The purpose of this function is to adjust the position of the points if they are placed badly. Otherwise, it will leave them alone.
    private void AdjustPointsIfNeeded(BridgeDestinationPairing pairing)
    {
        AdjustForOverlappingOrMisalignedPoints(pairing);
    }

    private void AdjustForOverlappingOrMisalignedPoints(BridgeDestinationPairing pairing)
    {
        //Bridge endpoints are overlapping or misaligned. We should fix this by moving one of them closer to its centerpoint.
        if(PointsAreOverlapping(pairing) || PointsAreMisaligned(pairing))
        {
            if(point1.y < point2.y) //Move point 1 closer to its center
            {
                float previousY = point1.y;
                point1 = Vector3.MoveTowards(point2, pairing.destination1.center, 2.0f);
                point1.y = previousY - 0.01f;

                point1Padding = 0.0f;
            }
            else //Move point 2 closer to its center
            {
                float previousY = point2.y;
                point2 = Vector3.MoveTowards(point1, pairing.destination2.center, 2.0f);
                point2.y = previousY - 0.01f;

                point2Padding = 0.0f;
            }
        }
    }

    private bool PointsAreOverlapping(BridgeDestinationPairing pairing)
    {
        float distanceFromSelfToCenter = Vector3.Distance(point1, pairing.destination1.center);
        float distanceFromOtherToCenter = Vector3.Distance(point2, pairing.destination1.center);

        return distanceFromOtherToCenter < distanceFromSelfToCenter;
    }

    private bool PointsAreMisaligned(BridgeDestinationPairing pairing)
    {
        Vector3 angleBetweenClosestPoints = GetBridgeRotation();
        Vector3 angleBetweenCenters = GetLookRotation(pairing.destination1.center, pairing.destination2.center);

        return Mathf.Abs(Mathf.DeltaAngle(angleBetweenClosestPoints.y, angleBetweenCenters.y)) > 44.0f;
    }

    public Vector3 GetBridgeRotation() { return GetLookRotation(point1, point2); }

    private Vector3 GetLookRotation(Vector3 from, Vector3 to)
    {
        //This checks prevents annoying logs from showing up in the console saying "look rotation is zero"
        if (Vector3.Distance(from, to) < 0.001f)
            return Vector3.zero;
        else
            return Quaternion.LookRotation(to - from).eulerAngles;
    }
}
