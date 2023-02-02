using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeManager
{
    public City city;
    private List<BridgeDestination> destinations;
    private List<BridgeDestinationPairing> pairings;
    private GameObject markerPrefab, specialConnector;

    public BridgeManager(City city)
    {
        this.city = city;
        markerPrefab = Resources.Load<GameObject>("Planet/City/Bridges/Short/Simple Arch Bridge");
        specialConnector = Resources.Load<GameObject>("Planet/City/Miscellaneous/Special Connector");
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
        GenerateNewPairingsFromDestinations();
        GenerateNewBridgesFromPairings();
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
        foreach (BridgeDestinationPairing pairing in pairings)
            GenerateNewBridgeFromPairingIfApplicable(pairing, markerPrefab);
    }

    private void GenerateNewBridgeFromPairingIfApplicable(BridgeDestinationPairing pairing, GameObject bridgePrefab)
    {
        //First, determine if the distance is too short for a bridge
        float gapLength = Vector3.Distance(pairing.destination1, pairing.destination2);

        if (gapLength < 1.0f)
            return;

        //Proceed with generation...

        //Choose appearance options for the new bridge
        Material newSlabMaterial = city.wallMaterials[Random.Range(0, city.wallMaterials.Length)];
        Material newGroundMaterial = city.floorMaterials[Random.Range(0, city.floorMaterials.Length)];

        //Create prototype bridge
        Bridge bridgePrototype = GameObject.Instantiate(bridgePrefab).GetComponent<Bridge>();

        //Define some needed variables
        Vector3 bridgeRotation = Quaternion.LookRotation(pairing.destination2 - pairing.destination1).eulerAngles;
        
        float defaultBridgeLength = bridgePrototype.transform.localScale.z;
        float totalPadding = 1.0f;
        float totalBridgeLength = gapLength + totalPadding;

        //From the prototype, decide whether we want to stretch it or tile it to span the length of the gap
        int numberOfSections;
        if (bridgePrototype.ShouldStretch(gapLength, defaultBridgeLength)) //Stretch
            numberOfSections = 1;
        else //Tile
            numberOfSections = Mathf.Max(Mathf.RoundToInt(totalBridgeLength / defaultBridgeLength), 1);

        //Proceed with filling the gap with a bridge by either stretching or tiling the bridge section(s)
        float sectionLength = totalBridgeLength / numberOfSections;
        Vector3 endpoint1 = Vector3.MoveTowards(pairing.destination1, pairing.destination2, -(totalPadding / 2.0f));
        Vector3 endpoint2 = Vector3.MoveTowards(pairing.destination2, pairing.destination1, -(totalPadding / 2.0f));
        bool keepSubstructure = numberOfSections > 1;

        for(int x = 0; x < numberOfSections; x++)
        {
            Vector3 bridgePosition = Vector3.MoveTowards(endpoint1, endpoint2, sectionLength * (x + 0.5f));

            //Generate the bridge section
            Bridge bridge;
            if(x == 0)
                bridge = GenerateNewBridgeSection(bridgePrototype.gameObject, true, bridgePosition, sectionLength, bridgeRotation, newSlabMaterial, newGroundMaterial, keepSubstructure);
            else
                bridge = GenerateNewBridgeSection(bridgePrefab, false, bridgePosition, sectionLength, bridgeRotation, newSlabMaterial, newGroundMaterial, keepSubstructure);

            //For the first and last bridge sections, see if we need to generate a connector beside them
            if (x == 0)
                GenerateNewConnectorIfNeeded(pairing.destination1Colliders, bridge, true, newSlabMaterial, newGroundMaterial);
            if (x == numberOfSections - 1) //WARNING: DON'T make this else if because the first and last could be the same section
                GenerateNewConnectorIfNeeded(pairing.destination2Colliders, bridge, false, newSlabMaterial, newGroundMaterial);
        }
    }

    private Bridge GenerateNewBridgeSection(GameObject bridgeObject, bool alreadyInstantiated, Vector3 position, float zScale, Vector3 rotation, Material newSlabMaterial, Material newGroundMaterial, bool useSubstructure)
    {
        //Get the instantiated game object as a transform
        Transform bridgeTransform;
        if (alreadyInstantiated)
            bridgeTransform = bridgeObject.transform;
        else
            bridgeTransform = GameObject.Instantiate(bridgeObject).transform;

        //Parent it
        bridgeTransform.parent = city.transform;

        //Position
        bridgeTransform.position = position;

        //Scale
        Vector3 bridgeScale = bridgeTransform.localScale;
        bridgeScale.z = zScale;
        bridgeTransform.localScale = bridgeScale;

        //Rotate
        bridgeTransform.eulerAngles = rotation;

        //Reskin
        Bridge bridge = bridgeTransform.GetComponent<Bridge>();
        bridge.SetUpBridge(useSubstructure, newSlabMaterial, newGroundMaterial);

        return bridge;
    }

    private void GenerateNewConnectorIfNeeded(Collider[] collidersToHit, Bridge bridge, bool checkBack, Material newSlabMaterial, Material newGroundMaterial)
    {
        if (collidersToHit == null)
            return;

        Vector3 leftCorner = bridge.GetCorner(true, checkBack);
        Vector3 rightCorner = bridge.GetCorner(false, checkBack);

        bool leftCornerTooFarAway = IsCornerTooFarAway(leftCorner, collidersToHit);
        bool rightCornerTooFarAway = IsCornerTooFarAway(rightCorner, collidersToHit);

        if (leftCornerTooFarAway || rightCornerTooFarAway)
            GenerateNewConnector(bridge, checkBack, newSlabMaterial, newGroundMaterial);
    }

    private void GenerateNewConnector(Bridge bridge, bool onBack, Material newSlabMaterial, Material newGroundMaterial)
    {
        Transform bridgeTransform = bridge.transform;

        //Compute scale beforehand
        Vector3 connectorScale = Vector3.one;
        connectorScale.x = bridgeTransform.localScale.x;
        connectorScale.z = connectorScale.x * 3.0f;

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
        Bridge connector = GenerateNewBridgeSection(specialConnector, false, connectorPosition, connectorScale.z, connectorRotation, newSlabMaterial, newGroundMaterial, false);
        
        //Adjust scale post-creation
        connectorScale.y = connector.transform.localScale.y;
        connector.transform.localScale = connectorScale;

        //Adjust position post-creation
        connector.transform.position += Vector3.down * ((connectorScale.y / 2.0f) + 0.1f);
    }

    private bool IsCornerTooFarAway(Vector3 corner, Collider[] collidersToHit)
    {
        Vector3 closestPointOnColliders = Bridge.GetClosestPointAmongstColliders(corner, collidersToHit);
        return Vector3.Distance(corner, closestPointOnColliders) > 0.25f;
    }
}
