using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleBridgeDestinationPairing : System.IComparable
{
    public int destination1Index, destination2Index;
    public float weight;

    public PossibleBridgeDestinationPairing(int destination1Index, int destination2Index, float weight)
    {
        this.destination1Index = destination1Index;
        this.destination2Index = destination2Index;
        this.weight = weight;
    }

    public int CompareTo(object obj)
    {
        PossibleBridgeDestinationPairing otherPairing = obj as PossibleBridgeDestinationPairing;
        if (weight < otherPairing.weight)
            return -1;
        else
            return 1;
    }
}
