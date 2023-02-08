using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for encapsulating the coordinates and dimensions of each city block. The dimensions could be wrong for the blocks on the edge of the city.
//Thus, you would need to check the area coordinate system with the SafeToGenerate method before taking anything for granted.
public class CityBlock : System.IComparable<CityBlock>
{
    public Vector2Int coords = Vector2Int.zero;
    public Vector2Int dimensions = Vector2Int.zero;

    public CityBlock() { }

    public CityBlock(CityBlock other)
    {
        coords = other.coords;
        dimensions = other.dimensions;
    }

    //Order the city blocks smallest to largest
    public int CompareTo(CityBlock other)
    {
        int mySmallest = GetSmallestDimension();
        int otherSmallest = other.GetSmallestDimension();

        if (mySmallest < otherSmallest)
            return -1;
        else if (mySmallest > otherSmallest)
            return 1;
        else
        {
            int myLargest = GetLargestDimension();
            int otherLargest = other.GetLargestDimension();

            if (myLargest < otherLargest)
                return -1;
            else if (myLargest > otherLargest)
                return 1;
            else
                return 0;
        }

    }

    public int GetSmallestDimension() { return dimensions.x < dimensions.y ? dimensions.x : dimensions.y; }
    private int GetLargestDimension() { return dimensions.x < dimensions.y ? dimensions.y : dimensions.x; }
}