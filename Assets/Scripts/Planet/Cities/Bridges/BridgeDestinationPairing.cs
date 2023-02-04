using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDestinationPairing
{
    public BridgeDestination destination1, destination2;
    public Vector3 destination1EdgePoint, destination2EdgePoint; //In global space

    public BridgeDestinationPairing(BridgeDestination bridgeDestination1, BridgeDestination bridgeDestination2)
    {
        destination1 = bridgeDestination1;
        destination2 = bridgeDestination2;

        //---

        Vector3 suggestionFrom1 = bridgeDestination1.GetClosestPoint(bridgeDestination2.center);
        Vector3 suggestionFrom2 = bridgeDestination2.GetClosestPoint(bridgeDestination1.center);

        Vector3 closestOn1To2 = bridgeDestination1.GetClosestPoint(suggestionFrom2);
        Vector3 closestOn2To1 = bridgeDestination2.GetClosestPoint(suggestionFrom1);

        float suggestion1Distance = Vector3.Distance(suggestionFrom1, closestOn2To1);
        float suggestion2Distance = Vector3.Distance(suggestionFrom2, closestOn1To2);

        if (suggestion1Distance < suggestion2Distance) //Choose suggestion 1
        {
            destination1EdgePoint = suggestionFrom1;
            destination2EdgePoint = closestOn2To1;
        }
        else //Choose suggestion 2
        {
            destination1EdgePoint = closestOn1To2;
            destination2EdgePoint = suggestionFrom2;
        }
    }
}