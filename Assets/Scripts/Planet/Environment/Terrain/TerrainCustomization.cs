using System.Collections.Generic;
using UnityEngine;

public class TerrainCustomization
{
    //Heightmap
    public List<TerrainSculptingLayerSelectionJSON> terrainSculptingLayers;

    //Trees
    public int idealTreeCount, maxTreeSteepness;
    public string[] treeNames;

    //Texture layers
    public Texture2D groundTexture, ground2Texture, cliffTexture, seabedTexture;
    public float groundMetallic, groundSmoothness, cliffMetallic, cliffSmoothness, seabedMetallic, seabedSmoothness;
    public float ground2TextureScale; //Inverse scale of how large patches of ground 2 texture should be

    //Misc
    public bool lowBoundaries, smallTerrain;
    public float seabedHeight;

    public TerrainCustomization() { InitializeParametersToDefaults(); }

    public void InitializeParametersToDefaults()
    {
        idealTreeCount = 0;
        maxTreeSteepness = 30;
        treeNames = new string[1];
        treeNames[0] = "Palm Tree";

        groundTexture = null;
        ground2Texture = null;
        cliffTexture = null;
        seabedTexture = null;
        groundMetallic = 0;
        groundSmoothness = 0;
        cliffMetallic = 0;
        cliffSmoothness = 0;
        seabedMetallic = 0;
        seabedSmoothness = 0;

        lowBoundaries = true;
        smallTerrain = false;
        seabedHeight = 7;
    }

    public void SetTreeNames(params string[] newNames) { treeNames = newNames; }
}

[System.Serializable]
public class TerrainSculptingLayerSelectionJSON
{
    public TerrainSculptingType sculptingType;
    public TerrainSculptingCondition sculptingCondition;

    //Main Heightmap Config
    [Tooltip("Inverse X-Z scale of noise")] public float noiseGroundScale;
    [Tooltip("Inverse X-Z scale of amplitude")] public float amplitudeGroundScale;
    [Tooltip("How dramatic the height difference is between landforms")] public int amplitudePower;
    [Tooltip("How big the hills are")] public float noiseStrength;

    //Optional Heightmap Config
    [Tooltip("Usually false. Set to true if you want terrain height to cap at horizon height.")] public bool horizonHeightIsCeiling;

    //Generate default sculpting (for purpose of generating new random terrain)
    public TerrainSculptingLayerSelectionJSON(TerrainSculptingLayerOptionsJSON terrainSculptingLayerOptionsJSON)
    {
        if (!System.Enum.TryParse<TerrainSculptingType>(terrainSculptingLayerOptionsJSON.editType, out sculptingType))
            sculptingType = TerrainSculptingType.Add;

        if (!System.Enum.TryParse<TerrainSculptingCondition>(terrainSculptingLayerOptionsJSON.condition, out sculptingCondition))
            sculptingCondition = TerrainSculptingCondition.Unconditional;

        horizonHeightIsCeiling = terrainSculptingLayerOptionsJSON.horizonHeightIsCeilingChance > Random.Range(0.0f, 1.0f);
        noiseGroundScale = GeneralHelperMethods.GetRandomValueFromRange(terrainSculptingLayerOptionsJSON.noiseGroundScaleRange, defaultValue: 40);
        amplitudeGroundScale = GeneralHelperMethods.GetRandomValueFromRange(terrainSculptingLayerOptionsJSON.amplitudeGroundScaleRange, defaultValue: 8);
        amplitudePower = GeneralHelperMethods.GetRandomValueFromRange(terrainSculptingLayerOptionsJSON.amplitudePowerRange, defaultValue: 3);
        noiseStrength = GeneralHelperMethods.GetRandomValueFromRange(terrainSculptingLayerOptionsJSON.noiseStrengthRange, defaultValue: 0.5f);
    }
}
