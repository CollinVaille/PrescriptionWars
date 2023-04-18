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
        float tilePlacementOffsetAmount = (cityLength - numberOfTilesOneWay * tileLength) * 0.5f;
        tilePlacementOffsetPosition = (-cityLength * 0.5f) + tilePlacementOffsetAmount;

        Debug.Log(numberOfTilesOneWay + "x" + numberOfTilesOneWay);

        GenerateTile(new Vector3Int(0, 0, 0), false, true);
        
        GenerateTile(new Vector3Int(0, 0, 1), true, true);

        GenerateTile(new Vector3Int(1, 0, 1), true, true);
        GenerateTile(new Vector3Int(1, 1, 1), true, true);

        GenerateTile(new Vector3Int(1, 0, 0), true, true);
        GenerateTile(new Vector3Int(1, 1, 0), true, true);
        GenerateTile(new Vector3Int(1, 2, 0), true, true);

        GenerateVerticalScalerToConnectTiles(new Vector3Int(0, 0, 0), new Vector3Int(1, 3, 0));
        GenerateVerticalScalerToConnectTiles(new Vector3Int(0, 1, 1), new Vector3Int(1, 3, 0));

        GenerateBridgeToConnectTiles(new Vector3Int(0, 3, 1), new Vector3Int(1, 3, 0));
    }

    //Helper functions---

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

    private void GenerateVerticalScalerToConnectTiles(Vector3Int lowerTileLocationIndices, Vector3Int upperTileLocationIndices)
    {
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
