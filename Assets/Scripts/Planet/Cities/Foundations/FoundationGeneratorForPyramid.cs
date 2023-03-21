using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForPyramid : MonoBehaviour
{
    //References
    private City city;
    private FoundationManager foundationManager;
    private float longestBuildingLength = 69.69f;

    public FoundationGeneratorForPyramid(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;

        longestBuildingLength = city.buildingManager.GetLongestBuildingLength();
    }

    public void GenerateNewPyramidFoundations()
    {
        //Start with buildings being allowed to generate nowhere
        city.areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.LackingRequiredFoundation, AreaManager.AreaReservationType.Open);

        //Begin the pyramid construction process by generating the bottom-most level and then recursing from there
        Vector3 foundationScale = Vector3.one * city.radius * 2.15f;
        foundationScale.y = foundationManager.foundationHeight;
        GeneratePyramidLevelRecursive(foundationScale, city.circularCity ? FoundationShape.Circular : FoundationShape.Rectangular, Vector3.zero, longestBuildingLength, 0);
    }

    private void GeneratePyramidLevelRecursive(Vector3 foundationScale, FoundationShape foundationShape, Vector3 foundationLocalPosition, float stepLength, int levelsGenerated)
    {
        if (stepLength * 2.25f > foundationScale.x && levelsGenerated >= 1)
            return;

        //Generate a single level
        GeneratePyramidLevel(foundationScale, foundationShape, foundationLocalPosition, 5.0f);
        levelsGenerated++;

        //And then recurse until we get to the top of the pyramid
        foundationLocalPosition.y += foundationManager.foundationHeight * 0.5f;
        foundationScale.x -= stepLength * 2.0f;
        foundationScale.z -= stepLength * 2.0f;
        GeneratePyramidLevelRecursive(foundationScale, foundationShape, foundationLocalPosition, stepLength, levelsGenerated);
    }

    private void GeneratePyramidLevel(Vector3 foundationScale, FoundationShape foundationShape, Vector3 foundationLocalPosition, float bufferLength)
    {
        //Precompute some information about the positioning
        Vector3 centerInAreas = city.areaManager.LocalCoordToAreaCoord(foundationLocalPosition);

        //---

        //Reserve the outer perimeter (area on the very edges of the pyramid level) as being off-limits for buildings
        int outerPerimeterAreasLong = Mathf.CeilToInt((foundationScale.x + bufferLength * 2.0f) / city.areaManager.areaSize);
        if(foundationShape == FoundationShape.Rectangular)
        {
            Vector2Int outerPerimeter = new Vector2Int((int)(centerInAreas.x - outerPerimeterAreasLong / 2), (int)(centerInAreas.z - outerPerimeterAreasLong / 2));
            city.areaManager.ReserveAreasRegardlessOfType(outerPerimeter.x, outerPerimeter.y, outerPerimeterAreasLong, AreaManager.AreaReservationType.ReservedForExtraPerimeter);
        }
        else //Circular
            city.areaManager.ReserveAreasWithinThisCircle((int)centerInAreas.x, (int)centerInAreas.z, outerPerimeterAreasLong / 2, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);

        //Generate the foundation for this pyramid level
        foundationManager.GenerateNewFoundation(foundationLocalPosition, foundationScale, city.circularCity ? FoundationShape.Circular : FoundationShape.Rectangular, false);

        //Reserve the inner perimeter (area on top of it) as able to accommodate buildings
        int innerPerimeterAreasLong = Mathf.CeilToInt((foundationScale.x - bufferLength * 2.0f) / city.areaManager.areaSize);
        if (foundationShape == FoundationShape.Rectangular)
        {
            Vector2Int innerPerimeter = new Vector2Int((int)(centerInAreas.x - innerPerimeterAreasLong / 2), (int)(centerInAreas.z - innerPerimeterAreasLong / 2));
            city.areaManager.ReserveAreasRegardlessOfType(innerPerimeter.x, innerPerimeter.y, innerPerimeterAreasLong, AreaManager.AreaReservationType.Open);
        }
        else //Circular
            city.areaManager.ReserveAreasWithinThisCircle((int)centerInAreas.x, (int)centerInAreas.z, innerPerimeterAreasLong / 2, AreaManager.AreaReservationType.Open, true, AreaManager.AreaReservationType.ReservedForExtraPerimeter);

        //Generate entrance for going from the previous level to this level
        float entranceDistanceFromCenter = foundationScale.x * 0.5f;
        foundationManager.GenerateEntrancesForCardinalDirections(false, overrideDistanceFromCityCenter: entranceDistanceFromCenter, foundationLocalY: foundationLocalPosition.y + 0.02f, reserveArea: true);
    }
}
