using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTerrain : MonoBehaviour
{
    public static PlanetTerrain planetTerrain;

    public Terrain terrain;
    public GameObject flatAreaPrefab;
    public Material horizonMaterial;
    public Transform horizonTransform; //13594

    public TerrainCustomization customization;
    public TerrainOffsets offsets;

    private int areaSize = 50;
    private int[,] areaSteepness;
    private int[,] areaHeight;

    public void Awake () { planetTerrain = this; }

    public void RegenerateTerrain (TerrainCustomization customization, TerrainOffsets offsets, PlanetJSON savedPlanet)
    {
        StartCoroutine(GenerateTerrain(customization, offsets, savedPlanet));
    }

    //TERRAIN FUNCTIONS--------------------------------------------------------------------------------------------

    //Call this to perform the whole generation (heightmap, painting, trees, areas, etc.)
    public IEnumerator GenerateTerrain (
        TerrainCustomization customization, TerrainOffsets offsets = null, PlanetJSON savedPlanet = null)
    {
        //If not supplied offsets, generate new random ones
        if (offsets == null)
            offsets = new TerrainOffsets();

        //Remember parameters
        this.customization = customization;
        this.offsets = offsets;

        //Height map
        GenerateTerrainHeightmap();

        //Horizon (planes that surround terrain)
        SetHorizons();

        //Divide terrain into square areas for purposes of finding flat area to generate features on
        ComputeAllTerrainAreas();

        //Generate cities after terrain heightmap and area system have been set up
        //But also before painting so painting reflects any elevation changes made by city generation
        Planet.planet.GenerateCities(savedPlanet);
        //Temporary---------------------------------------------------------------------------------------------------
        //Transform spawn = Instantiate(spawnZonePrefab, Vector3.zero, Quaternion.identity).transform;
        //Transform spawn2 = Instantiate(badGuySpawnZonePrefab, ReserveTerrainPosition(Random.Range(0, 3),
        //    (int)seabedHeight + Random.Range(0, 4), 500, 25, true), Quaternion.identity).transform;
        //spawn2.Translate(Vector3.up * spawn2.localScale.y / 2);
        //------------------------------------------------------------------------------------------------------------

        GenerateTrees();

        //Wait a couple frames for all terrain edits to complete
        yield return null;
        yield return null;

        //Terrain textures (do after everything else to make sure painting is up to date)
        PaintTerrain();

        //ShowAreas(9999, -9999, 9999);
    }

    //TERRAIN GENERATION FUNCTIONS------------------------------------------------------------------------------------

    //Defaults: NGS- 40, AGS- 8, AP- 3, NS- 0.5, All offsets: Random.Range(0.0f, 10000.0f)
    private void GenerateTerrainHeightmap ()
    {
        TerrainData terrainData = terrain.terrainData;

        int width = customization.smallTerrain ? 1024 : 2048;
        int length = width;

        terrainData.size = new Vector3(width, 512, length);
        terrainData.heightmapResolution = (int)(terrainData.size.x + 1);

        float[,] heights = new float[length, width];

        //Terrain gets its randomness through random offsets applied to perlin noise
        float noiseOffsetX = offsets.noiseOffsetX;
        float noiseOffsetZ = offsets.noiseOffsetZ;
        float amplitudeOffsetX = offsets.amplitudeOffsetX;
        float amplitudeOffsetZ = offsets.amplitudeOffsetZ;

        //Ground scales (smaller actually kinda means larger)
        float noiseGroundScale = customization.noiseGroundScale; //Scale of small bumps in terrain
        float amplitudeGroundScale = customization.amplitudeGroundScale; //Scale of larger land features

        //Other customization
        float noiseStrength = customization.noiseStrength;
        int amplitudePower = customization.amplitudePower;
        bool terrainIgnoresHorizonHeight = !customization.horizonHeightIsCeiling;

        float boundaryHeight = 0.25f;
        int boundaryWidth = customization.smallTerrain ? 125 : 250;

        float noise, amplitude;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                //Base terrain noise (small bumps)
                noise = Mathf.PerlinNoise(noiseOffsetX + x * noiseGroundScale / width,
                                           noiseOffsetZ + z * noiseGroundScale / length) * noiseStrength;

                //Randomly amplify the noise to differentiate terrain (large land features)
                amplitude = Mathf.PerlinNoise(amplitudeOffsetX + x * amplitudeGroundScale / width,
                                              amplitudeOffsetZ + z * amplitudeGroundScale / length);

                //Terrain boundaries
                float terrainEdgePercentage = TerrainEdgePercentage(x, z, width, boundaryWidth);
                if (terrainEdgePercentage > 0)
                {
                    if (customization.lowBoundaries) //Edge of terrain goes to 0 height
                    {
                        noise -= terrainEdgePercentage * 0.6f;
                        amplitude = Mathf.Max(amplitude - terrainEdgePercentage * 2.0f, 0);
                    }
                    else //Edge of terrain is mountain range that is capped to a certain height
                    {
                        noise += terrainEdgePercentage * 0.6f;
                        amplitude += terrainEdgePercentage * 0.6f;
                    }
                }

                //Apply computation
                if (terrainEdgePercentage == 0 && terrainIgnoresHorizonHeight) //Unrestricted height
                    heights[z, x] = noise * Mathf.Pow(amplitude, amplitudePower);
                else //Height restricted to border height
                    heights[z, x] = Mathf.Min(noise * Mathf.Pow(amplitude, amplitudePower), boundaryHeight);
            }
        }

        terrainData.SetHeights(0, 0, heights);

        //Debug.Log("NGS: " + noiseGroundScale + ", AGS: " + amplitudeGroundScale + ", AP: " + amplitudePower + ", NS: " + noiseStrength);
    }

    //Returns 0 if point not within boundary width
    //Returns percentage (0-1) to very edge of map if within boundary width
    //where 0 = beginning of boundary and 1 = very edge of map
    //Used to make smooth map boundaries
    private float TerrainEdgePercentage (float x, float z, int terrainWidth, int boundaryWidth)
    {
        float[] boundaryPercentages = new float[] { 0, 0, 0, 0 };

        if (x < boundaryWidth)
            boundaryPercentages[0] = 1 - x / boundaryWidth;
        else if (terrainWidth < x + boundaryWidth)
            boundaryPercentages[2] = 1 - (terrainWidth - x) / boundaryWidth;

        if (z < boundaryWidth)
            boundaryPercentages[1] = 1 - z / boundaryWidth;
        else if (terrainWidth < z + boundaryWidth)
            boundaryPercentages[3] = 1 - (terrainWidth - z) / boundaryWidth;

        float max = boundaryPercentages[0];
        for (int boundary = 1; boundary < 4; boundary++)
        {
            if (boundaryPercentages[boundary] > max)
                max = boundaryPercentages[boundary];
        }

        return max;
    }

    private void GenerateHill (float[,] heights, int xStart, int zStart, int xLength, int zLength, float height)
    {
        //heights[z + zStart, x + xStart] = 1.0f / (Mathf.Abs(x - xLength / 2) + 1);

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLength; z++)
                heights[z + zStart, x + xStart] = Mathf.PerlinNoise(x / 500.0f, z / 500.0f);
        }
    }

    private void PaintTerrain ()
    {
        TerrainData terrainData = terrain.terrainData;

        //Create terrain layers--------------------------------------------------------------------------------------

        List<TerrainLayer> terrainTextureList = new List<TerrainLayer>();

        TerrainLayer groundLayer = new TerrainLayer();
        groundLayer.diffuseTexture = customization.groundTexture;
        groundLayer.tileSize = new Vector2(2, 2);

        TerrainLayer cliffLayer = new TerrainLayer();
        cliffLayer.diffuseTexture = customization.cliffTexture;
        cliffLayer.tileSize = new Vector2(2, 2);

        TerrainLayer seabedLayer = new TerrainLayer();
        seabedLayer.diffuseTexture = customization.seabedTexture;
        seabedLayer.tileSize = new Vector2(2, 2);

        TerrainLayerEffects(groundLayer, cliffLayer, seabedLayer);

        terrainTextureList.Add(groundLayer);
        terrainTextureList.Add(cliffLayer);
        terrainTextureList.Add(seabedLayer);

        terrainData.terrainLayers = terrainTextureList.ToArray();

        //Use terrain layers to paint terrain-----------------------------------------------------------------------

        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        int heightmapX, heightmapZ;
        for (int x = 0; x < terrainData.alphamapWidth; x++)
        {
            for (int z = 0; z < terrainData.alphamapHeight; z++)
            {
                heightmapZ = (int)((float)x * terrainData.heightmapResolution / terrainData.alphamapWidth);
                heightmapX = (int)((float)z * terrainData.heightmapResolution / terrainData.alphamapHeight);

                if (terrainData.GetHeight(heightmapX, heightmapZ) < customization.seabedHeight) //SEABED
                {
                    splatmapData[x, z, 0] = 0; //Ground
                    splatmapData[x, z, 1] = 0; //Cliff
                    splatmapData[x, z, 2] = 1; //Seabed
                }
                else
                {
                    float terrainSteepness = terrainData.GetSteepness(((float)z) / terrainData.alphamapHeight,
                                                                      ((float)x) / terrainData.alphamapWidth);

                    if (terrainSteepness > 35) //CLIFF
                    {
                        splatmapData[x, z, 0] = 0; //Ground
                        splatmapData[x, z, 1] = 1; //Cliff
                        splatmapData[x, z, 2] = 0; //Seabed
                    }
                    else //GROUND
                    {
                        splatmapData[x, z, 0] = 1; //Ground
                        splatmapData[x, z, 1] = 0; //Cliff
                        splatmapData[x, z, 2] = 0; //Seabed
                    }
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private void TerrainLayerEffects (TerrainLayer groundLayer, TerrainLayer cliffLayer, TerrainLayer seabedLayer)
    {
        //Apply customization of metallic and smoothness properties to terrain layers
        groundLayer.metallic = customization.groundMetallic;
        groundLayer.smoothness = customization.groundSmoothness;

        cliffLayer.metallic = customization.cliffMetallic;
        cliffLayer.smoothness = customization.cliffSmoothness;

        seabedLayer.metallic = customization.seabedMetallic;
        seabedLayer.smoothness = customization.seabedSmoothness;

        //Update horizon to have same effects as terrain
        if(Planet.planet.hasOcean && customization.lowBoundaries)
        {
            horizonMaterial.SetFloat("_Metallic", seabedLayer.metallic);
            horizonMaterial.SetFloat("_Glossiness", seabedLayer.smoothness);
        }
        else
        {
            horizonMaterial.SetFloat("_Metallic", groundLayer.metallic);
            horizonMaterial.SetFloat("_Glossiness", groundLayer.smoothness);
        }
    }

    private void SetHorizons ()
    {
        //Set height and texture
        if (customization.lowBoundaries)
        {
            horizonMaterial.SetTexture("_MainTex", customization.seabedTexture);
            horizonTransform.Translate(Vector3.down * 128);
        }
        else
            horizonMaterial.SetTexture("_MainTex", customization.groundTexture);

        //Move to squeeze around small terrain
        if (customization.smallTerrain)
        {
            horizonTransform.Find("Front Horizon").localPosition = new Vector3(0.0f, 127.99f, 10524.0f);
            horizonTransform.Find("Right Horizon").localPosition = new Vector3(10524.0f, 127.99f, 0.0f);
        }
    }

    //TERRAIN FEATURE FUNCTIONS------------------------------------------------------------------------------------

    private void ComputeAllTerrainAreas ()
    {
        TerrainData terrainData = terrain.terrainData;

        areaSteepness = new int[1 + terrainData.heightmapResolution / areaSize, 1 + terrainData.heightmapResolution / areaSize];
        areaHeight = new int[1 + terrainData.heightmapResolution / areaSize, 1 + terrainData.heightmapResolution / areaSize];

        //Results for one area: X coord = steepness of area, y coord = height of area
        Vector2Int results;

        //Compute all areas, one at a time (one per iteration)
        for (int x = 0; x < terrainData.heightmapResolution; x += areaSize)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z += areaSize)
            {
                //Flipped b/c terrain orientation is wack
                results = ComputeSingleTerrainArea(terrainData, x, z);

                areaSteepness[z / areaSize, x / areaSize] = results.x;
                areaHeight[z / areaSize, x / areaSize] = results.y;

                /*
                if (IsFlatArea(terrainData, x, z, brushSize, maxSteepness))
                {
                    Transform cube = Instantiate(flatAreaPrefab, new Vector3(-500, 25, -500), Quaternion.Euler(0, 0, 0)).transform;

                    cube.localScale = new Vector3(brushSize, 100, brushSize);
                    cube.position += new Vector3(z, 0, x);
                    
                }   */
            }
        }
    }

    private Vector2Int ComputeSingleTerrainArea (TerrainData terrainData, int xStart, int zStart)
    {
        //Out of bounds = no go (pretend like area is really steep and high so we never use it)
        if (xStart + areaSize > terrainData.heightmapResolution)
            return new Vector2Int(9999, 9999);
        else if (zStart + areaSize > terrainData.heightmapResolution)
            return new Vector2Int(9999, 9999);

        //Set the steepness of the area to the steepest point within the area
        //And the height of the area to the highest point within the area
        int maxSteepness = 0;
        int currentSteepness = 0;
        int maxHeight = 0;
        int currentHeight = 0;
        for (int x = xStart; x < xStart + areaSize; x++)
        {
            for (int z = zStart; z < zStart + areaSize; z++)
            {
                if (TerrainEdgePercentage(x, z, terrainData.heightmapResolution, customization.smallTerrain ? 125 : 250) > 0)
                    return new Vector2Int(9999, 9999);

                //Steepness...

                currentSteepness = (int)terrainData.GetSteepness(((float)z) / terrainData.heightmapResolution,
                                                                  ((float)x) / terrainData.heightmapResolution);

                if (currentSteepness > maxSteepness)
                    maxSteepness = currentSteepness;

                //Height...

                currentHeight = (int)terrainData.GetHeight(z, x);

                if (currentHeight > maxHeight)
                    maxHeight = currentHeight;
            }
        }

        return new Vector2Int(currentSteepness, currentHeight);
    }

    //Generate cubes over areas that have steepness < max steepness and height within range
    private void ShowAreas (int maxSteepness, int minHeight, int maxHeight)
    {
        for (int x = 0; x < areaSteepness.GetLength(0); x++)
        {
            for (int z = 0; z < areaSteepness.GetLength(1); z++)
            {
                //areaSteepness[x, z] <= maxSteepness && areaHeight[x, z] <= maxHeight && 

                if (areaHeight[x, z] >= minHeight)
                {
                    Transform cube = Instantiate(flatAreaPrefab, new Vector3(-500, 25, -500), Quaternion.Euler(0, 0, 0)).transform;

                    cube.localScale = new Vector3(areaSize, 100, areaSize);
                    cube.position += new Vector3(x * areaSize, 0, z * areaSize);
                }
            }
        }
    }

    private bool AreaAtPositionAvailable (float globalXPos, float globalZPos)
    {
        Vector2Int areaCoords = ConvertFromGlobalToAreaUnits(globalXPos, globalZPos);
        return areaHeight[areaCoords.x, areaCoords.y] < 9000;
    }

    public Vector2Int ConvertFromGlobalToAreaUnits(float globalXPos, float globalZPos)
    {
        return new Vector2Int((int)((globalXPos + 500) / areaSize), (int)((globalZPos + 500) / areaSize));
    }

    public Vector2 ConvertFromAreaToGlobalUnits(float areaXCoord, float areaZCoord)
    {
        return new Vector2((areaXCoord * areaSize) - 500.0f, (areaZCoord * areaSize) - 500.0f);
    }

    public Vector3 ReserveTerrainPosition (TerrainReservationOptions options)
    {
        int xCoord, zCoord;

        //If we have to flatten, then half of the radius is taken up by boundaries so we double radius to compensate
        if (options.flatten)
            options.radius *= 2;

        if (options.newGeneration) //New generation
        {
            //Set initial values to center of terrain so that if we don't get a match we just plop it in the middle
            xCoord = areaSteepness.GetLength(0) / 2;
            zCoord = areaSteepness.GetLength(1) / 2;
            float smallestDifference = 9999;

            //Go through all the areas and remember the one that had the closest steepness value to preferred steepness
            for (int x = 0; x < areaSteepness.GetLength(0); x++)
            {
                for (int z = 0; z < areaSteepness.GetLength(1); z++)
                {
                    //Look for candidate with best steepness value
                    if (Mathf.Abs(areaSteepness[x, z] - options.preferredSteepness) < smallestDifference)
                    {
                        //Make sure it is safe
                        if (SelectedAreasAreSafe(x, z, 0, 500, options.radius))
                        {
                            //Found new best area
                            smallestDifference = Mathf.Abs(areaSteepness[x, z] - options.preferredSteepness);
                            xCoord = x;
                            zCoord = z;
                        }
                    }
                }
            }
        }
        else //Restoration
        {
            Vector2Int areaCoords = ConvertFromGlobalToAreaUnits(options.position.x, options.position.z);
            xCoord = areaCoords.x;
            zCoord = areaCoords.y;
        }

        //Now that we have concluded which area(s) to reserve, reserve it/them...
        int minHeightForReservation = options.newGeneration ? options.heightRange.x + 1 : (int)options.position.y;

        ReserveSelectedAreas(xCoord, zCoord, minHeightForReservation, options.radius, options.flatten);

        //Compute the starting point of the area we just reserved (in world coordinates)
        Vector2 globalCoords = ConvertFromAreaToGlobalUnits(xCoord, zCoord);
        Vector3 worldPosition = new Vector3(globalCoords.x, 9999.0f, globalCoords.y);

        //Translate from starting point to center point of area
        worldPosition.x += areaSize / 2;
        worldPosition.z += areaSize / 2;

        //Compute the y value
        RaycastHit raycastHit;
        Physics.Raycast(worldPosition, Vector3.down, out raycastHit);
        worldPosition.y = raycastHit.point.y;

        //Return computed, height-adjusted, center point
        return worldPosition;
    }

    private bool SelectedAreasAreSafe (int xCoord, int zCoord, int minHeight, int maxHeight, int radius)
    {
        int areasLong = Mathf.CeilToInt(radius * 2.0f / areaSize);

        int xStart = xCoord - (areasLong / 2); //Leftmost x included
        int zStart = zCoord - (areasLong / 2); //Bottommost z included

        //Go through each included area from bottom left corner to top right corner
        for (int x = xStart; x < xStart + areasLong; x++)
        {
            for (int z = zStart; z < zStart + areasLong; z++)
            {
                //For each area we will include...

                //Check lower boundaries
                if (x < 0 || z < 0)
                    return false;

                //Check upper boundaries
                if (x >= areaHeight.Length || z >= areaHeight.Length)
                    return false;

                //Check height
                if (areaHeight[x, z] < minHeight || areaHeight[x, z] > maxHeight)
                    return false;
            }
        }

        return true;
    }

    private void ReserveSelectedAreas (int xCoord, int zCoord, int minHeight, int radius, bool flatten)
    {
        //The Mathf.Min caps the city size to that of the entire terrain
        int areasLong = Mathf.Min(Mathf.CeilToInt(radius * 2.0f / areaSize), areaSteepness.GetLength(0) - 2);

        int xStart = xCoord - (areasLong / 2); //Leftmost x included
        int zStart = zCoord - (areasLong / 2); //Bottommost z included

        //Mark selected areas as taken
        for (int x = xStart; x < xStart + areasLong; x++)
        {
            for (int z = zStart; z < zStart + areasLong; z++)
            {
                areaSteepness[x, z] = 9999;
                areaHeight[x, z] = 9999;
            }
        }

        //Flatten selected areas (all to the sample height of the center of the selection)
        if (flatten)
        {
            int selectionSize = areaSize * areasLong;

            float[,] heights = new float[selectionSize, selectionSize];

            //Figure out what height to level the selection to (sample height from middle of selection)
            float newHeight = terrain.terrainData.GetHeight((int)(xStart * areaSize + selectionSize * 0.5f),
                                                            (int)(zStart * areaSize + selectionSize * 0.5f));

            //Make sure height is above min height
            if (newHeight < minHeight)
                newHeight = minHeight;

            //Convert from world coordinates to terrain heightmap coordinates
            newHeight /= 512.0f;

            //Set every point in the area to have that height except boundary points transition back to normal terrain
            for (int x = 0; x < heights.GetLength(0); x++)
            {
                for (int z = 0; z < heights.GetLength(1); z++)
                {
                    float edgePercentage = TerrainEdgePercentage(x, z, selectionSize, selectionSize / 4);

                    if (edgePercentage == 0)
                        heights[x, z] = newHeight;
                    else
                    {
                        float oldHeight = terrain.terrainData.GetHeight(xStart * areaSize + z,
                                                                        zStart * areaSize + x)
                                                                        / 512.0f;

                        heights[x, z] = Mathf.Lerp(newHeight, oldHeight, edgePercentage);
                    }
                }
            }

            //Apply changes to terrain
            terrain.terrainData.SetHeights(xStart * areaSize, zStart * areaSize, heights);
        }
    }

    //Return the index of the texture that is painted at the specified point
    public int GetTextureIndexAtPoint (Vector3 point)
    {
        //Adjust the point from world coordinates to terrain coordinates
        point.x += 500;
        point.z += 500;
        point.x /= 4;
        point.z /= 4;

        //Return the index of the layer with an alpha value of 1...

        //Get alphamaps at point
        float[,,] alphamaps = terrain.terrainData.GetAlphamaps((int)point.x, (int)point.z, 1, 1);

        //Search through them
        for (int x = 0; x < alphamaps.GetLength(2); x++)
            if (alphamaps[0, 0, x] == 1)
                return x;

        //Default, shouldn't get here b/c all points should be painted on some layer
        return 0;
    }

    private void GenerateTrees ()
    {
        //int idealTreeCount, int maxSteepness, float seabedHeight, params string[] treeNames

        //Load tree models...
        string[] treeNames = customization.treeNames;
        TreePrototype[] treePrototypes = new TreePrototype[treeNames.Length];
        GameObject[] treePrefabs = new GameObject[treeNames.Length];
        for (int x = 0; x < treeNames.Length; x++)
        {
            TreePrototype newPrototype = new TreePrototype();

            newPrototype.prefab = Resources.Load<GameObject>("Planet/Environment/Trees/" + treeNames[x]);
            newPrototype.bendFactor = 0;

            treePrototypes[x] = newPrototype;

            treePrefabs[x] = newPrototype.prefab;
        }
        terrain.terrainData.treePrototypes = treePrototypes;

        List<TreeInstance> trees = new List<TreeInstance>();

        //Set generation parameters...

        int idealTreeCount = customization.idealTreeCount;

        int maxAttempts = idealTreeCount * 2;

        float minHeight = -10;
        if (Planet.planet.hasOcean)
            minHeight = (int)Mathf.Max(Planet.planet.oceanTransform.position.y, customization.seabedHeight);

        //Attempt to generate one tree per iteration; if attempt fails no reattempt is made, moves onto next tree
        for (int treesPlanted = 0, attempts = 0; treesPlanted < idealTreeCount && attempts < maxAttempts; attempts++)
        {
            //Generate new place to put tree
            Vector3 newTreePosition = new Vector3(Random.value, 0.0f, Random.value);

            //Choose randomly which type of tree to place
            int prototypeIndex = Random.Range(0, treeNames.Length);

            //Check if placement is ok...
            if (!CanPlaceTreeHere(newTreePosition, minHeight))
                continue;

            //If got to this point, we passed all tests so proceed with creating tree...
            treesPlanted++;

            //Create tree instance
            TreeInstance newTree = new TreeInstance();
            newTree.color = Color.white;
            newTree.lightmapColor = Color.white;
            newTree.prototypeIndex = prototypeIndex;
            newTree.widthScale = 1;
            newTree.heightScale = 1;
            newTree.rotation = 0;
            newTree.position = newTreePosition;

            //Add it to list of trees
            trees.Add(newTree);
        }

        //Debug.Log("Tree Count: " + trees.Count);

        terrain.terrainData.SetTreeInstances(trees.ToArray(), true);

        //Supposed to make all changes take effect...
        terrain.Flush();

        //But in reality, tree colliders need the terrain collider to be toggled to work...
        //(hours and hours wasted on this bug)
        terrain.GetComponent<TerrainCollider>().enabled = false;
        terrain.GetComponent<TerrainCollider>().enabled = true;

        //Finally, generate trees not on terrain, but on horizon
        GenerateHorizonTrees(treePrefabs, idealTreeCount * 2);
    }

    private void GenerateHorizonTrees (GameObject[] treePrefabs, int idealTreeCount)
    {
        //Nowhere to generate trees, so we're just gonna give up bro
        if (horizonTransform.position.y < 1 && Planet.planet.hasOcean)
            return;

        //First, create empty gameobject for containing all the trees
        Transform horizonTrees = new GameObject("Trees").transform;
        horizonTrees.parent = horizonTransform;
        horizonTrees.localPosition = Vector3.zero;
        horizonTrees.localEulerAngles = Vector3.zero;

        //Then, define some tree positioning parameters

        //EXACT BOUNDARIES: -500 to 1542 (for both x and y)
        //LOOSE BOUNDARIES: -300 to 1342 (for both x and y)
        float southPole = -300, northPole = 1342;
        float midPole = (southPole + northPole) / 2.0f;

        float treeRange = 3000, poleRange = northPole - southPole + treeRange;
        float magicHeight = horizonTransform.position.y;

        //Finally, attempt to generate horizon trees - one per iteration
        int maxAttempts = idealTreeCount * 2;
        for (int treesPlanted = 0, attempts = 0; treesPlanted < idealTreeCount && attempts < maxAttempts; attempts++)
        {
            //RANDOMLY GENERATE NEW TREE POSITION
            Vector3 treePos;
            
            if(Random.Range(0, 2) == 0)
                treePos = new Vector3(GetRandomTreePosition(treeRange, southPole, northPole), 1000,
                                      GetRandomTreePosition(poleRange, midPole, midPole));
            else
                treePos = new Vector3(GetRandomTreePosition(poleRange, midPole, midPole), 1000,
                                      GetRandomTreePosition(treeRange, southPole, northPole));

            //TEST IF IT WORKS

            //No ground to place tree on = FAILURE
            if (!Physics.Raycast(treePos, Vector3.down, out RaycastHit hitInfo))
                continue;

            //Ground is terrain, not horizon = FAILURE
            if (Mathf.Abs(magicHeight - hitInfo.point.y) > 2)
                continue;

            //PASSES TESTS, SO PLACE TREE
            treesPlanted++;
            Transform newTree = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)]).transform;

            //Parent and rotation
            newTree.parent = horizonTrees;
            newTree.localEulerAngles = Vector3.zero;

            //Position
            treePos.y = magicHeight;
            newTree.position = treePos;
        }
    }

    //Used by tree horizon generation to get random position of new tree
    private float GetRandomTreePosition (float treeRange, float southPole, float northPole)
    {
        //Random distribution from:
        //https://gamedev.stackexchange.com/questions/116832/random-number-in-a-range-biased-toward-the-low-end-of-the-range

        //Generates random # between 0 and 1 with 0 being most likely, 1 being least likely
        float treePos = Mathf.Abs(Random.Range(0.0f, 1.0f) - Random.Range(0.0f, 1.0f));

        //Scale random # by treeRange and make it deviate from either south or north pole
        if (Random.Range(0, 2) == 0)
            treePos = northPole + (treePos * treeRange);
        else
            treePos = southPole - (treePos * treeRange);

        return treePos;
    }

    //Tree position is in terrain coordinates, that is: X and Z should be between 0 and 1, Y is ignored
    private bool CanPlaceTreeHere (Vector3 treePosition, float minHeight)
    {
        TerrainData terrainData = terrain.terrainData;

        //Ground must be level enough
        if (terrainData.GetSteepness(treePosition.x, treePosition.z) > customization.maxTreeSteepness)
            return false;

        //Ground cannot be lower than sea level
        int heightmapX = (int)(treePosition.x * terrainData.heightmapResolution);
        int heightmapZ = (int)(treePosition.z * terrainData.heightmapResolution);

        float height = terrainData.GetHeight(heightmapX, heightmapZ);
        if (height < minHeight || height > 120)
            return false;

        //Ground must be clear of any obstacles (so we don't place trees inside of buildings for example)...

        //Get world position of tree
        Vector3 worldPos = treePosition;
        worldPos.x *= terrain.terrainData.size.x;
        worldPos.z *= terrain.terrainData.size.z;
        worldPos += terrain.transform.position;

        //Get the actual height of the terrain at that position in world coordinates
        //worldPos.y = terrain.SampleHeight(worldPos);

        return AreaAtPositionAvailable(worldPos.x, worldPos.z);
    }

    public float GetSeabedHeight () { return customization.seabedHeight; }

    //PLANET MANAGEMENT FUNCTIONS----------------------------------------------------------------------------------

    public void SetTreeVisibility (bool visible) { terrain.drawTreesAndFoliage = visible; }
}