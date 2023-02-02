using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDestinationPairing
{
    public Vector3 destination1, destination2; //In global space
    public Collider[] destination1Colliders, destination2Colliders;

    public BridgeDestinationPairing(Vector3 destination1, Vector3 destination2)
    {
        this.destination1 = destination1;
        this.destination2 = destination2;
    }

    public BridgeDestinationPairing(BridgeDestination bridgeDestination1, BridgeDestination bridgeDestination2)
    {
        Vector3 suggestionFrom1 = bridgeDestination1.GetClosestPoint(bridgeDestination2.center);
        Vector3 suggestionFrom2 = bridgeDestination2.GetClosestPoint(bridgeDestination1.center);

        Vector3 closestOn1To2 = bridgeDestination1.GetClosestPoint(suggestionFrom2);
        Vector3 closestOn2To1 = bridgeDestination2.GetClosestPoint(suggestionFrom1);

        float suggestion1Distance = Vector3.Distance(suggestionFrom1, closestOn2To1);
        float suggestion2Distance = Vector3.Distance(suggestionFrom2, closestOn1To2);

        if (suggestion1Distance < suggestion2Distance) //Choose suggestion 1
        {
            destination1 = suggestionFrom1;
            destination2 = closestOn2To1;
        }
        else //Choose suggestion 2
        {
            destination1 = closestOn1To2;
            destination2 = suggestionFrom2;
        }

        //---

        destination1Colliders = bridgeDestination1.boundaryColliders;
        destination2Colliders = bridgeDestination2.boundaryColliders;
    }
}