using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoundationGeneratorForGrid
{
    private City city;
    private FoundationManager foundationManager;
    private AreaManager areaManager;

    private float tileLength;
    private float tilePlacementOffsetPosition;

    //Main access functions---

    public FoundationGeneratorForGrid(FoundationManager foundationManager)
    {
        this.foundationManager = foundationManager;
        city = foundationManager.city;
        areaManager = city.areaManager;
    }

    public void GenerateNewGridFoundations()
    {
        //No outer walls for this foundation type
        city.newCitySpecifications.shouldGenerateCityPerimeterWalls = false;

        //Start with buildings being allowed to generate nowhere
        areaManager.ReserveAllAreasWithType(AreaManager.AreaReservationType.LackingRequiredFoundation, AreaManager.AreaReservationType.Open);

        //Compute the dimensions and measurements of the grid
        tileLength = city.buildingManager.GetLongestBuildingLength() + areaManager.areaSize * 2;
        float cityLength = areaManager.areaTaken.GetLength(0) * areaManager.areaSize;
        int numberOfTilesOneWay = Mathf.FloorToInt(cityLength / tileLength);

        //Figure out which grid plan to use. If there's isn't a grid plan for the dimensions we were going for, then we scale the dimensions down until we find a plan
        int newNumberOfTilesOneWay = GetPlanFolderAndName(numberOfTilesOneWay, out string planFolder, out string planName);

        //If we had to scale the dimensions down to find a plan, re-adjust calculations made based on those dimensions
        if(newNumberOfTilesOneWay != numberOfTilesOneWay)
        {
            numberOfTilesOneWay = newNumberOfTilesOneWay;
            tileLength = Mathf.FloorToInt(cityLength / numberOfTilesOneWay);
        }

        //Now that the grid dimensions are finalized, calculate the offset data based on the dimensions. This data is needed to compute the exact positioning of the tiles
        tilePlacementOffsetPosition = GetTilePlacementOffsetPosition(cityLength, tileLength, numberOfTilesOneWay);

        //Load in the grid plan
        GridFoundationPlanJSON gridFoundationPlan = GetGridFoundationPlan(planFolder, planName);

        //Finally, enact the grid plan...

        //Generate the foundations
        if (gridFoundationPlan.foundations != null)
        {
            for (int x = 0; x < gridFoundationPlan.foundations.Length; x++)
            {
                GridFoundationTileJSON tileStack = gridFoundationPlan.foundations[x];

                for (int y = 0; y < tileStack.foundationPresentPerYLevel.Length; y++)
                {
                    Vector3Int tileLocationIndices = new Vector3Int(tileStack.groundCoordinates.x, y, tileStack.groundCoordinates.y);
                    bool topTile = (y == tileStack.foundationPresentPerYLevel.Length - 1);

                    GenerateTile(tileLocationIndices, tileStack.foundationPresentPerYLevel[y], topTile && tileStack.markAsOpen);
                }
            }
        }

        //Generate the vertical scalers
        if(gridFoundationPlan.verticalScalers != null)
        {
            for (int x = 0; x < gridFoundationPlan.verticalScalers.Length; x++)
            {
                GetCoordinatesForConnector(gridFoundationPlan.verticalScalers[x], out Vector3Int coordinate1, out Vector3Int coordinate2);
                GenerateVerticalScalerToConnectTiles(coordinate1, coordinate2);
            }

        }

        //Generate the bridges
        if (gridFoundationPlan.bridges != null)
        {
            for (int x = 0; x < gridFoundationPlan.bridges.Length; x++)
            {
                GetCoordinatesForConnector(gridFoundationPlan.bridges[x], out Vector3Int coordinate1, out Vector3Int coordinate2);
                GenerateBridgeToConnectTiles(coordinate1, coordinate2);
            }
        }
    }

    //Helper functions---

    private static int GetPlanFolderAndName(int numberOfTilesOneWay, out string planFolder, out string planName)
    {
        planFolder = null;
        planName = null;
        int smallestAllowedOneWay = 2;

        if (numberOfTilesOneWay < smallestAllowedOneWay)
            numberOfTilesOneWay = smallestAllowedOneWay;

        for(; numberOfTilesOneWay >= smallestAllowedOneWay;)
        {
            planFolder = numberOfTilesOneWay + "x" + numberOfTilesOneWay;
            planName = GeneralHelperMethods.GetLineFromFile("Planet/City/Grid Foundation Plans/Plan Lists/" + planFolder + " Grid Foundations", startPathFromGeneralTextFolder: false, nullSafe: true);

            if (planName == null && numberOfTilesOneWay > smallestAllowedOneWay)
                numberOfTilesOneWay = Random.Range(smallestAllowedOneWay, numberOfTilesOneWay);
            else
                break;
        }

        return numberOfTilesOneWay;
    }

    private static GridFoundationPlanJSON GetGridFoundationPlan(string planFolder, string planName)
    {
        string foundationPlanJsonAsString = GeneralHelperMethods.GetTextAsset("Planet/City/Grid Foundation Plans/" + planFolder + "/" + planName, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<GridFoundationPlanJSON>(foundationPlanJsonAsString);
    }

    private static void GetCoordinatesForConnector(GridFoundationConnectorJSON connector, out Vector3Int coordinate1, out Vector3Int coordinate2)
    {
        coordinate1 = new Vector3Int(connector.groundCoordinates1.x, connector.heightCoordinate1, connector.groundCoordinates1.y);
        coordinate2 = new Vector3Int(connector.groundCoordinates2.x, connector.heightCoordinate2, connector.groundCoordinates2.y);
    }

    private static float GetTilePlacementOffsetPosition(float cityLength, float tileLength, int numberOfTilesOneWay)
    {
        float tilePlacementOffsetAmount = (cityLength - numberOfTilesOneWay * tileLength) * 0.5f;
        return (-cityLength * 0.5f) + (tileLength * 0.5f) + tilePlacementOffsetAmount;
    }

    private void GenerateTile(Vector3Int tileLocationIndices, bool placeFoundation, bool markAsOpen)
    {
        Vector3 localPositionOfFoundation = GetLocalPositionOfTileFoundation(tileLocationIndices);

        if (placeFoundation)
        {
            Vector3 localScaleOfFoundation = GetLocalScaleOfTileFoundation();

            foundationManager.GenerateNewFoundation(localPositionOfFoundation, localScaleOfFoundation, FoundationShape.Rectangular, false);
        }

        if(markAsOpen)
        {
            Vector3 localEdgePointOfFoundation = localPositionOfFoundation;
            localEdgePointOfFoundation.x -= tileLength * 0.5f;
            localEdgePointOfFoundation.z -= tileLength * 0.5f;
            Vector3 areaCoordsOfFoundation = areaManager.LocalCoordToAreaCoord(localEdgePointOfFoundation);

            int startX = (int)(areaCoordsOfFoundation.x) + 1;
            int startZ = (int)(areaCoordsOfFoundation.z) + 1;
            int areasLong = (int)(tileLength / areaManager.areaSize) - 2;
            areaManager.ReserveAreasRegardlessOfType(startX, startZ, areasLong, AreaManager.AreaReservationType.Open);
        }
    }

    private void GenerateVerticalScalerToConnectTiles(Vector3Int tile1LocationIndices, Vector3Int tile2LocationIndices)
    {
        //Figure out which location is the lower one and which is the upper one
        Vector3Int lowerTileLocationIndices, upperTileLocationIndices;
        if (tile1LocationIndices.y < tile2LocationIndices.y)
        {
            lowerTileLocationIndices = tile1LocationIndices;
            upperTileLocationIndices = tile2LocationIndices;
        }
        else
        {
            lowerTileLocationIndices = tile2LocationIndices;
            upperTileLocationIndices = tile1LocationIndices;
        }

        //Compute needed inputs to generate vertical scaler
        Vector3 upperTileGlobalPosition = city.transform.TransformPoint(GetLocalPositionOfTileFoundation(upperTileLocationIndices));
        Vector3 lowerTileGlobalPosition = city.transform.TransformPoint(GetLocalPositionOfTileFoundation(lowerTileLocationIndices));

        Vector3 globalEdgePoint = (upperTileGlobalPosition + lowerTileGlobalPosition) * 0.5f;

        float globalBottomLevel = lowerTileGlobalPosition.y;
        float globalTopLevel = upperTileGlobalPosition.y;

        //Finally generate the vertical scaler
        city.verticalScalerManager.GenerateVerticalScalerWithFocalPoint(upperTileGlobalPosition, globalEdgePoint, globalBottomLevel, globalTopLevel, false);

        //Reserve area around the vertical scaler so that no buildings can spawn there
        Vector3 placeInAreas = areaManager.LocalCoordToAreaCoord(city.transform.InverseTransformPoint(globalEdgePoint));
        int radiusInAreas = 20 / city.areaManager.areaSize;
        city.areaManager.ReserveAreasWithinThisCircle((int)placeInAreas.x, (int)placeInAreas.z, radiusInAreas, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.LackingRequiredFoundation);

    }

    private void GenerateBridgeToConnectTiles(Vector3Int tile1LocationIndices, Vector3Int tile2LocationIndices)
    {
        //Get the positioning of the two tiles to connect in global space
        Vector3 tile1GlobalPosition = city.transform.TransformPoint(GetLocalPositionOfTileFoundation(tile1LocationIndices));
        Vector3 tile2GlobalPosition = city.transform.TransformPoint(GetLocalPositionOfTileFoundation(tile2LocationIndices));

        //Convert the positions from the center of the tiles to the edges
        tile1GlobalPosition = Vector3.MoveTowards(tile1GlobalPosition, tile2GlobalPosition, tileLength * 0.5f);
        tile2GlobalPosition = Vector3.MoveTowards(tile2GlobalPosition, tile1GlobalPosition, tileLength * 0.5f);

        //Create destinations for both points
        BridgeDestination destination1 = new BridgeDestination(tile1GlobalPosition, 0.1f);
        BridgeDestination destination2 = new BridgeDestination(tile2GlobalPosition, 0.1f);

        //Pair the destinations together
        BridgeDestinationPairing bridgeDestinationPairing = new BridgeDestinationPairing(destination1, destination2);

        //Send the pairing request off to the bridge manager which will do the rest for us
        city.bridgeManager.AddNewDestinationPairing(bridgeDestinationPairing);

        //Reserve the areas underneath the bridge as being off-limits for building generation. That way buildings aren't clipping through bridges
        Vector3 tile1LocalPosition = city.transform.InverseTransformPoint(tile1GlobalPosition);
        Vector3 tile2LocalPosition = city.transform.InverseTransformPoint(tile2GlobalPosition);
        Vector3 localReservationHead = tile1LocalPosition;
        for(int iterationsCompleted = 0; iterationsCompleted < 500 && Vector3.Distance(localReservationHead, tile2LocalPosition) > areaManager.areaSize; iterationsCompleted++)
        {
            Vector3 areaCoords = areaManager.LocalCoordToAreaCoord(localReservationHead);
            areaManager.ReserveAreasWithinThisCircle((int)areaCoords.x, (int)areaCoords.z, 2, AreaManager.AreaReservationType.ReservedForExtraPerimeter, true, AreaManager.AreaReservationType.Open);

            //Move head to next position
            localReservationHead = Vector3.MoveTowards(localReservationHead, tile2LocalPosition, areaManager.areaSize);
        }
    }

    private Vector3 GetLocalPositionOfTileFoundation(Vector3Int tileLocationIndices)
    {
        return new Vector3(tilePlacementOffsetPosition + tileLength * tileLocationIndices.x,
                            tileLocationIndices.y * foundationManager.foundationHeight * 0.5f,
                            tilePlacementOffsetPosition + tileLength * tileLocationIndices.z);
    }

    private Vector3 GetLocalScaleOfTileFoundation()
    {
        return new Vector3(tileLength, foundationManager.foundationHeight, tileLength);
    }
}


[System.Serializable]
public class GridFoundationPlanJSON
{
    public GridFoundationTileJSON[] foundations;
    public GridFoundationConnectorJSON[] verticalScalers;
    public GridFoundationConnectorJSON[] bridges;
}


[System.Serializable]
public class GridFoundationTileJSON
{
    public Vector2Int groundCoordinates;
    public bool[] foundationPresentPerYLevel;
    public bool markAsOpen;
}


[System.Serializable]
public class GridFoundationConnectorJSON
{
    public Vector2Int groundCoordinates1, groundCoordinates2;
    public int heightCoordinate1, heightCoordinate2;
}