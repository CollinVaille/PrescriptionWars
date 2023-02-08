using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationSelections
{
    public string basicCircularFoundation, basicRectangularFoundation;
    public string smallCircularFoundation, smallRectangularFoundation;
    public string largeCircularFoundation, largeRectangularFoundation;

    public FoundationSelections(FoundationOptions foundationOptions)
    {
        basicCircularFoundation = foundationOptions.basicCircularFoundation;
        basicRectangularFoundation = foundationOptions.basicRectangularFoundation;
        smallCircularFoundation = foundationOptions.smallCircularFoundations[Random.Range(0, foundationOptions.smallCircularFoundations.Length)];
        smallRectangularFoundation = foundationOptions.smallRectangularFoundations[Random.Range(0, foundationOptions.smallRectangularFoundations.Length)];
        largeCircularFoundation = foundationOptions.largeCircularFoundations[Random.Range(0, foundationOptions.largeCircularFoundations.Length)];
        largeRectangularFoundation = foundationOptions.largeRectangularFoundations[Random.Range(0, foundationOptions.largeRectangularFoundations.Length)];
    }

    public string GetFoundationPrefab(bool circular, Vector3 scale)
    {
        if (scale.y / 2.0f < 15.0f)
            return circular ? basicCircularFoundation : basicRectangularFoundation;
        else if (scale.x < 100.0f)
            return circular ? smallCircularFoundation : smallRectangularFoundation;
        else
            return circular ? largeCircularFoundation : largeRectangularFoundation;
    }
}
