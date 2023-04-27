using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoundationShape { Rectangular, Circular, Torus }

public class FoundationSelections
{
    public string testFoundation;
    public string basicCircularFoundation, basicRectangularFoundation, basicTorusFoundation;
    public string smallCircularFoundation, smallRectangularFoundation;
    public string largeCircularFoundation, largeRectangularFoundation, largeTorusFoundation;

    public int foundationHeight;

    public FoundationSelections(FoundationOptions foundationOptions)
    {
        testFoundation = foundationOptions.testFoundation;

        basicCircularFoundation = foundationOptions.basicCircularFoundation;
        basicRectangularFoundation = foundationOptions.basicRectangularFoundation;
        basicTorusFoundation = foundationOptions.basicTorusFoundation;

        smallCircularFoundation = foundationOptions.smallCircularFoundations[Random.Range(0, foundationOptions.smallCircularFoundations.Length)];
        smallRectangularFoundation = foundationOptions.smallRectangularFoundations[Random.Range(0, foundationOptions.smallRectangularFoundations.Length)];

        largeCircularFoundation = foundationOptions.largeCircularFoundations[Random.Range(0, foundationOptions.largeCircularFoundations.Length)];
        largeRectangularFoundation = foundationOptions.largeRectangularFoundations[Random.Range(0, foundationOptions.largeRectangularFoundations.Length)];
        largeTorusFoundation = foundationOptions.largeTorusFoundations[Random.Range(0, foundationOptions.largeTorusFoundations.Length)];

        foundationHeight = DetermineFoundationHeightFromOptions(foundationOptions.heightOptions);
    }

    private int DetermineFoundationHeightFromOptions(FoundationHeightOption[] heightOptions)
    {
        for(int x = 0; x < heightOptions.Length; x++)
        {
            if(heightOptions[x].chance > Random.Range(0.0f, 1.0f))
                return GeneralHelperMethods.GetRandomValueFromRange(heightOptions[x].heightRange);
        }

        //Default to zero height
        return 0;
    }

    public string GetFoundationPrefab(FoundationShape foundationShape, Vector3 scale)
    {
        if(!string.IsNullOrEmpty(testFoundation))
            return testFoundation;

        if (scale.y / 2.0f < 15.0f) //Foundation is short so just choose a basic shape. Fancy patterns would look smashed with this y scale
        {
            if (foundationShape == FoundationShape.Rectangular)
                return basicRectangularFoundation;
            else if (foundationShape == FoundationShape.Circular)
                return basicCircularFoundation;
            else
                return basicTorusFoundation;
        }
        else if (scale.x < 100.0f) //Small foundations
        {
            if (foundationShape == FoundationShape.Rectangular)
                return smallRectangularFoundation;
            else if (foundationShape == FoundationShape.Circular)
                return smallCircularFoundation;
            else
                return basicTorusFoundation;
        }
        else //Large foundations
        {
            if (foundationShape == FoundationShape.Rectangular)
                return largeRectangularFoundation;
            else if (foundationShape == FoundationShape.Circular)
                return largeCircularFoundation;
            else
                return largeTorusFoundation;
        }
    }
}
