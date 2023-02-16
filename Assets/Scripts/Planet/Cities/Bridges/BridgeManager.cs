using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeManager
{
    public City city;
    public Material slabMaterial, groundMaterial;
    public List<string> bridgePrefabPaths;

    private List<BridgeDestination> destinations;
    private List<BridgeDestinationPairing> pairings;
    public List<Bridge> bridges;

    public BridgeManager(City city)
    {
        this.city = city;
    }

    public void AddNewDestination(BridgeDestination bridgeDestination)
    {
        if (destinations == null)
            destinations = new List<BridgeDestination>();
        destinations.Add(bridgeDestination);
    }

    public void AddNewDestinationPairing(BridgeDestinationPairing bridgeDestinationPairing)
    {
        if (pairings == null)
            pairings = new List<BridgeDestinationPairing>();
        pairings.Add(bridgeDestinationPairing);
    }

    public void GenerateNewBridges()
    {
        DetermineBridgeMaterials();

        GenerateNewPairingsFromDestinations();
        GenerateNewBridgesFromPairings();
    }
    
    public void DetermineBridgeMaterials()
    {
        slabMaterial = city.foundationManager.slabMaterial;
        groundMaterial = city.foundationManager.groundMaterial;
    }

    public Bridge InstantiateBridge(int prefabIndex)
    {
        string fullResourcePath = "Planet/City/Bridges/" + city.cityType.name + "/" + bridgePrefabPaths[prefabIndex];
        return GameObject.Instantiate(Resources.Load<GameObject>(fullResourcePath)).GetComponent<Bridge>();
    }

    private int GetIndexOfABridgeModel(bool specialConnector)
    {
        return specialConnector ? bridgePrefabPaths.Count - 1 : Random.Range(0, bridgePrefabPaths.Count - 1);
    }

    private void GenerateNewPairingsFromDestinations()
    {
        if (destinations == null || destinations.Count <= 1)
            return;

        //FIRST, ENSURE CONNECTEDNESS OF ALL DESTINATIONS BY CREATING A MINIMUM SPANNING TREE USING KRUSKAL'S ALGORITHM!!!

        //Get all possible pairings and their weights, i.e. distances
        PossibleBridgeDestinationPairing[] possiblePairings = new PossibleBridgeDestinationPairing[destinations.Count * destinations.Count];
        int nextWeightIndex = 0;
        for (int x = 0; x < destinations.Count; x++)
        {
            for (int y = 0; y < destinations.Count; y++)
            {
                if (x == y) //Do not allow self-loops
                    possiblePairings[nextWeightIndex++] = new PossibleBridgeDestinationPairing(x, y, Mathf.Infinity);
                else
                    possiblePairings[nextWeightIndex++] = new PossibleBridgeDestinationPairing(x, y, Vector3.Distance(destinations[x].center, destinations[y].center));
            }
        }

        //Sort possible pairings by weight, i.e. distance
        //This ensures that we try to add the shortest pairing first
        System.Array.Sort(possiblePairings);

        //Initially, each pairing is it's own tree
        List<HashSet<int>> indexTrees = new List<HashSet<int>>();
        for (int x = 0; x < destinations.Count; x++)
        {
            HashSet<int> newTree = new HashSet<int>();
            newTree.Add(x);
            indexTrees.Add(newTree);
        }

        //Add pairings until all destinations are in a single connected tree
        //But only add a pairing if it serves to connect two trees
        int possiblePairingIndex = 0;
        while (indexTrees.Count > 1)
        {
            PossibleBridgeDestinationPairing possiblePairing = possiblePairings[possiblePairingIndex++];

            //Determine which tree each destination belongs to
            int destination1Tree = -1;
            int destination2Tree = -1;
            for (int x = 0; x < indexTrees.Count; x++)
            {
                if (indexTrees[x].Contains(possiblePairing.destination1Index))
                    destination1Tree = x;

                if (indexTrees[x].Contains(possiblePairing.destination2Index))
                    destination2Tree = x;
            }

            //Disjoint trees, so include pairing
            if (destination1Tree != destination2Tree)
            {
                //Merge trees
                indexTrees[destination1Tree].UnionWith(indexTrees[destination2Tree]); //1 becomes union of 1 and 2
                indexTrees.RemoveAt(destination2Tree); //2 gets discarded

                //Include pairing
                AddNewDestinationPairing(new BridgeDestinationPairing(destinations[possiblePairing.destination1Index], destinations[possiblePairing.destination2Index]));
            }
        }
    }

    private void GenerateNewBridgesFromPairings()
    {
        if (pairings == null)
            return;

        foreach (BridgeDestinationPairing pairing in pairings)
            GenerateNewBridgeFromPairingIfApplicable(pairing);
    }

    private void GenerateNewBridgeFromPairingIfApplicable(BridgeDestinationPairing pairing)
    {
        //Determine if the distance is too short for a bridge
        float gapLength = Vector3.Distance(pairing.destination1EdgePoint, pairing.destination2EdgePoint);

        if (gapLength < 0.05f)
            return;

        //Proceed with generation...
        GenerateNewBridgeFromPairing(pairing, GetIndexOfABridgeModel(false));
    }

    private void GenerateNewBridgeFromPairing(BridgeDestinationPairing pairing, int prefabIndex)
    {
        NewBridgeSpecs specs = new NewBridgeSpecs(pairing, prefabIndex);

        //Before generating, determine if the angle is too steep
        Vector3 bridgeRotation = specs.GetBridgeRotation();
        Vector3 bridgeXZRotation = new Vector3(bridgeRotation.x, 0.0f, bridgeRotation.z);

        //If it is too steep and the distance is sufficient enough, have the lower point draw a flat line of bridge sections to the upper point's x, z location.
        //Once there, put up a vertical scaler (like a ladder) to cover the y difference.
        float unsignedBridgeXZRotation = Mathf.DeltaAngle(bridgeXZRotation.magnitude, 0.0f);
        if (unsignedBridgeXZRotation > 15.0f && Mathf.Abs(specs.point2.y - specs.point1.y) > 1.5f)
        {
            Vector3 bottomPoint, topPoint;
            Vector3 verticalScalerRotation = bridgeRotation;

            if (specs.point1.y < specs.point2.y) //Generate vertical scaler at destination 2
            {
                specs.verticalScalerAtDestination2 = true;

                bottomPoint = specs.point2;
                bottomPoint.y = specs.point1.y;
                topPoint = specs.point2;

                specs.point2 = bottomPoint;

                specs.point2Padding = 0.0f;

                verticalScalerRotation.y = verticalScalerRotation.y + 180.0f;
            }
            else //Generate vertical scaler at destination 1
            {
                specs.verticalScalerAtDestination1 = true;

                bottomPoint = specs.point1;
                bottomPoint.y = specs.point2.y;
                topPoint = specs.point1;

                specs.point1 = bottomPoint;

                specs.point1Padding = 0.0f;
            }

            GenerateNewVerticalScalerAtEndOfBridge(bottomPoint, topPoint, (int)city.transform.InverseTransformDirection(verticalScalerRotation).y);
        }
        //Otherwise, draw an angle bridge between the points

        GenerateNewBridgeSectionsBetweenPoints(specs);
    }

    //This will take the points provided as is and draw bridge sections between them.
    //This function does not check if the points are too close to each other or the angle between them is too steep.
    //Those checks should already have been done by the function(s) that call this.
    private void GenerateNewBridgeSectionsBetweenPoints(NewBridgeSpecs specs)
    {
        float gapLength = Vector3.Distance(specs.point1, specs.point2);

        //Create prototype bridge
        Bridge bridgePrototype = InstantiateBridge(specs.bridgePrefabIndex);

        //Define some needed variables
        Vector3 bridgeRotation = specs.GetBridgeRotation();

        float defaultBridgeLength = bridgePrototype.transform.localScale.z;
        float totalBridgeLength = gapLength + specs.point1Padding + specs.point2Padding;

        //From the prototype, decide whether we want to stretch it or tile it to span the length of the gap
        int numberOfSections;
        if (bridgePrototype.ShouldStretch(gapLength, defaultBridgeLength)) //Stretch
            numberOfSections = 1;
        else //Tile
            numberOfSections = Mathf.Max(Mathf.RoundToInt(totalBridgeLength / defaultBridgeLength), 1);

        //Proceed with filling the gap with a bridge by either stretching or tiling the bridge section(s)
        float sectionLength = totalBridgeLength / numberOfSections;
        bool justNeedSingleRamp = totalBridgeLength < 3.0f;
        Vector3 endpoint1 = Vector3.MoveTowards(specs.point1, specs.point2, -(specs.point1Padding / 2.0f));
        Vector3 endpoint2 = Vector3.MoveTowards(specs.point2, specs.point1, -(specs.point2Padding / 2.0f));
        bool keepSubstructure = numberOfSections > 1;

        for (int x = 0; x < numberOfSections; x++)
        {
            Vector3 bridgePosition = Vector3.MoveTowards(endpoint1, endpoint2, sectionLength * (x + 0.5f));

            //Generate the bridge section
            Bridge bridge;
            if (justNeedSingleRamp)
            {
                bridgePosition.y -= 0.1f;
                bridge = GenerateNewBridgeSection(GetIndexOfABridgeModel(true), bridgePosition, sectionLength, bridgeRotation, false, false);
                Vector3 bridgeScale = bridge.transform.localScale;
                bridgeScale.x = 5.0f;
                bridge.transform.localScale = bridgeScale;

                GameObject.Destroy(bridgePrototype.gameObject);
            }
            else
            {
                bool avoidRoof = (x == 0) && specs.verticalScalerAtDestination1 || (x == numberOfSections - 1) && specs.verticalScalerAtDestination2;

                if (x == 0)
                    bridge = GenerateNewBridgeSection(specs.bridgePrefabIndex, bridgePosition, sectionLength, bridgeRotation, keepSubstructure, !avoidRoof, bridgePrototype);
                else
                    bridge = GenerateNewBridgeSection(specs.bridgePrefabIndex, bridgePosition, sectionLength, bridgeRotation, keepSubstructure, !avoidRoof);
            }

            //For the first and last bridge sections, see if we need to generate a connector beside them
            if(!justNeedSingleRamp)
            {
                if (x == 0)
                    GenerateNewEdgeConnectorIfNeeded(specs.point1Colliders, bridge, true);
                if (x == numberOfSections - 1) //WARNING: DON'T make this "else if" because the first and last could be the same section
                    GenerateNewEdgeConnectorIfNeeded(specs.point2Colliders, bridge, false);
            }
        }
    }

    private Bridge GenerateNewBridgeSection(int prefabIndex, Vector3 position, float zScale, Vector3 rotation, bool useSubstructure, bool useRoof, Bridge prototype = null)
    {
        //Get the instantiated game object as a transform
        Bridge bridge;
        if (prototype)
            bridge = prototype;
        else
            bridge = InstantiateBridge(prefabIndex);

        bridge.SetUpBridge(prefabIndex, position, rotation, zScale, this, useSubstructure, useRoof);

        return bridge;
    }

    private void GenerateNewEdgeConnectorIfNeeded(Collider[] collidersToHit, Bridge bridge, bool checkBack)
    {
        if (collidersToHit == null)
            return;

        Vector3 leftCorner = bridge.GetCorner(true, checkBack);
        Vector3 rightCorner = bridge.GetCorner(false, checkBack);

        bool leftCornerTooFarAway = IsCornerTooFarAway(leftCorner, collidersToHit);
        bool rightCornerTooFarAway = IsCornerTooFarAway(rightCorner, collidersToHit);

        if (leftCornerTooFarAway || rightCornerTooFarAway)
            GenerateNewEdgeConnector(bridge, checkBack);
    }

    private void GenerateNewEdgeConnector(Bridge bridge, bool onBack)
    {
        Transform bridgeTransform = bridge.transform;

        //Compute scale beforehand
        Vector3 connectorScale = Vector3.one;
        connectorScale.x = bridgeTransform.localScale.x;
        connectorScale.z = connectorScale.x * 2.5f;

        //Compute position beforehand
        Vector3 edgeOfBridgeInLocal = (onBack ? Vector3.back : Vector3.forward) * 0.5f;
        Vector3 edgeOfBridge = bridgeTransform.TransformPoint(edgeOfBridgeInLocal);

        Vector3 offsetDirection = onBack ? -bridgeTransform.forward : bridgeTransform.forward;
        offsetDirection.y = 0.0f;

        Vector3 connectorPosition = edgeOfBridge + offsetDirection * (connectorScale.z / 2.0f);

        //Compute rotation beforehand
        Vector3 connectorRotation = bridgeTransform.eulerAngles;
        connectorRotation.x = 0.0f;
        connectorRotation.z = 0.0f;

        //Create the connector
        Bridge connector = GenerateNewBridgeSection(GetIndexOfABridgeModel(true), connectorPosition, connectorScale.z, connectorRotation, false, false);
        
        //Adjust scale post-creation
        connectorScale.y = connector.transform.localScale.y;
        connector.transform.localScale = connectorScale;

        //Adjust position post-creation
        connector.transform.position += Vector3.down * 0.1f;
    }

    private bool IsCornerTooFarAway(Vector3 corner, Collider[] collidersToHit)
    {
        Vector3 closestPointOnColliders = Foundation.GetClosestPointAmongstColliders(corner, collidersToHit, false);
        closestPointOnColliders.y = corner.y; //We don't want the y-value to factor into the distance. We're only looking for x-z corners

        return Vector3.Distance(corner, closestPointOnColliders) > 0.25f;
    }

    private void GenerateNewVerticalScalerAtEndOfBridge(Vector3 bottomPoint, Vector3 topPoint, int yAxisRotation)
    {
        //Instantiate the vertical scaler
        VerticalScaler newVerticalScaler = city.verticalScalerManager.InstantiateVerticalScaler(city.cityType.GetVerticalScaler(true), city.transform);
        Transform verticalScalerTransform = newVerticalScaler.transform;

        //Rotate it
        newVerticalScaler.SetYAxisRotation(yAxisRotation);

        //Position it
        verticalScalerTransform.position = bottomPoint;

        //Scale it and connect it to the entrance foundation
        newVerticalScaler.ScaleToHeightAndConnect(topPoint.y - bottomPoint.y, false);
    }
}

[System.Serializable]
public class BridgeManagerJSON
{
    public List<string> bridgePrefabPaths;
    public BridgeJSON[] bridgeJSONs;

    public BridgeManagerJSON(BridgeManager bridgeManager)
    {
        if (bridgeManager.bridges == null)
            return;

        bridgePrefabPaths = bridgeManager.bridgePrefabPaths;

        bridgeJSONs = new BridgeJSON[bridgeManager.bridges.Count];
        for(int x = 0; x < bridgeJSONs.Length; x++)
            bridgeJSONs[x] = new BridgeJSON(bridgeManager.bridges[x], bridgeManager);
    }

    public void RestoreBridgeManager(BridgeManager bridgeManager)
    {
        bridgeManager.DetermineBridgeMaterials();

        if (bridgeJSONs == null)
            return;

        bridgeManager.bridgePrefabPaths = bridgePrefabPaths;

        foreach (BridgeJSON bridgeJSON in bridgeJSONs)
            bridgeJSON.RestoreBridge(bridgeManager);
    }
}