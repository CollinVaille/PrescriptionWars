using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoundationOptions
{
    public string testFoundation;
    public string basicCircularFoundation, basicRectangularFoundation, basicTorusFoundation;
    public string[] smallCircularFoundations, smallRectangularFoundations, largeCircularFoundations, largeRectangularFoundations, largeTorusFoundations;

    public FoundationHeightOption[] heightOptions;

    public void FillEmptyFieldsWithTheseValues(FoundationOptions fallbackOptions)
    {
        if (string.IsNullOrEmpty(testFoundation))
            testFoundation = fallbackOptions.testFoundation;

        if (string.IsNullOrEmpty(basicCircularFoundation))
            basicCircularFoundation = fallbackOptions.basicCircularFoundation;

        if (string.IsNullOrEmpty(basicRectangularFoundation))
            basicRectangularFoundation = fallbackOptions.basicRectangularFoundation;

        if (string.IsNullOrEmpty(basicTorusFoundation))
            basicTorusFoundation = fallbackOptions.basicTorusFoundation;

        if (smallCircularFoundations == null || smallCircularFoundations.Length == 0)
            smallCircularFoundations = fallbackOptions.smallCircularFoundations;

        if (smallRectangularFoundations == null || smallRectangularFoundations.Length == 0)
            smallRectangularFoundations = fallbackOptions.smallRectangularFoundations;

        if (largeCircularFoundations == null || largeCircularFoundations.Length == 0)
            largeCircularFoundations = fallbackOptions.largeCircularFoundations;

        if (largeRectangularFoundations == null || largeRectangularFoundations.Length == 0)
            largeRectangularFoundations = fallbackOptions.largeRectangularFoundations;

        if (largeTorusFoundations == null || largeTorusFoundations.Length == 0)
            largeTorusFoundations = fallbackOptions.largeTorusFoundations;

        if (heightOptions == null || heightOptions.Length == 0)
            heightOptions = fallbackOptions.heightOptions;
    }
}

[System.Serializable]
public class FoundationHeightOption
{
    //0.0-1.0 = 0-100% chance for this height option to be selected.
    public float chance;

    //If this height option is selected, the height will be randomized to a value within the confines of heightRange.
    public int[] heightRange;
}