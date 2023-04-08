
using UnityEngine;

[System.Serializable]
public class PlanetTerrainJSON
{
    public PlanetTerrainJSON(PlanetTerrain planetTerrain)
    {
        //Customization: Main Heightmap Config
        noiseGroundScale = planetTerrain.customization.noiseGroundScale;
        amplitudeGroundScale = planetTerrain.customization.amplitudeGroundScale;
        amplitudePower = planetTerrain.customization.amplitudePower;
        noiseStrength = planetTerrain.customization.noiseStrength;

        //Customization: Optional Heightmap Config
        horizonHeightIsCeiling = planetTerrain.customization.horizonHeightIsCeiling;

        //Customization: Trees
        idealTreeCount = planetTerrain.customization.idealTreeCount;
        maxTreeSteepness = planetTerrain.customization.maxTreeSteepness;
        treeNames = planetTerrain.customization.treeNames;

        //Customization: Layers
        groundTexture = planetTerrain.customization.groundTexture.name;
        Texture2D ground2Tex = planetTerrain.customization.ground2Texture;
        ground2Texture = ground2Tex ? ground2Tex.name : null;
        cliffTexture = planetTerrain.customization.cliffTexture.name;
        seabedTexture = planetTerrain.customization.seabedTexture.name;
        groundMetallic = planetTerrain.customization.groundMetallic;
        groundSmoothness = planetTerrain.customization.groundSmoothness;
        cliffMetallic = planetTerrain.customization.cliffMetallic;
        cliffSmoothness = planetTerrain.customization.cliffSmoothness;
        seabedMetallic = planetTerrain.customization.seabedMetallic;
        seabedSmoothness = planetTerrain.customization.seabedSmoothness;
        ground2TextureScale = planetTerrain.customization.ground2TextureScale;

        //Customization: Misc
        lowBoundaries = planetTerrain.customization.lowBoundaries;
        smallTerrain = planetTerrain.customization.smallTerrain;
        seabedHeight = planetTerrain.customization.seabedHeight;

        //Offsets
        noiseOffsetX = planetTerrain.offsets.noiseOffsetX;
        noiseOffsetZ = planetTerrain.offsets.noiseOffsetZ;
        amplitudeOffsetX = planetTerrain.offsets.amplitudeOffsetX;
        amplitudeOffsetZ = planetTerrain.offsets.amplitudeOffsetZ;
        ground2OffsetX = planetTerrain.offsets.ground2OffsetX;
        ground2OffsetZ = planetTerrain.offsets.ground2OffsetZ;
    }

    public void RestorePlanetTerrain(PlanetTerrain planetTerrain, PlanetJSON savedPlanet)
    {
        //Customization: Main Heightmap Config
        TerrainCustomization customization = new TerrainCustomization(noiseGroundScale, amplitudeGroundScale,
            amplitudePower, noiseStrength);

        //Customization: Optional Heightmap Config
        customization.horizonHeightIsCeiling = horizonHeightIsCeiling;

        //Customization: Trees
        customization.idealTreeCount = idealTreeCount;
        customization.maxTreeSteepness = maxTreeSteepness;
        customization.treeNames = treeNames;

        //Customization: Layers
        customization.groundTexture = Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + groundTexture);
        if(!string.IsNullOrEmpty(ground2Texture))
            customization.ground2Texture = Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + ground2Texture);
        customization.cliffTexture = Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + cliffTexture);
        customization.seabedTexture = Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + seabedTexture);
        customization.groundMetallic = groundMetallic;
        customization.groundSmoothness = groundSmoothness;
        customization.cliffMetallic = cliffMetallic;
        customization.cliffSmoothness = cliffSmoothness;
        customization.seabedMetallic = seabedMetallic;
        customization.seabedSmoothness = seabedSmoothness;
        customization.ground2TextureScale = ground2TextureScale;

        //Customization: Misc
        customization.lowBoundaries = lowBoundaries;
        customization.smallTerrain = smallTerrain;
        customization.seabedHeight = seabedHeight;

        //Offsets
        TerrainOffsets offsets = new TerrainOffsets(noiseOffsetX, noiseOffsetZ, amplitudeOffsetX, amplitudeOffsetZ, ground2OffsetX, ground2OffsetZ);

        //Now that all terrain parameters have been restored, use them to regenerate the terrain
        planetTerrain.RegenerateTerrain(customization, offsets, savedPlanet);
    }

    //Customization: Heightmap
    public float noiseGroundScale, amplitudeGroundScale;
    public int amplitudePower;
    public float noiseStrength;
    public bool horizonHeightIsCeiling;

    //Customization: Trees
    public int idealTreeCount, maxTreeSteepness;
    public string[] treeNames;

    //Customization: Layers
    public string groundTexture, ground2Texture, cliffTexture, seabedTexture;
    public float groundMetallic, groundSmoothness, cliffMetallic, cliffSmoothness, seabedMetallic, seabedSmoothness;
    public float ground2TextureScale;

    //Customization: Misc
    public bool lowBoundaries, smallTerrain;
    public float seabedHeight;

    //Offsets
    public float noiseOffsetX, noiseOffsetZ;
    public float amplitudeOffsetX, amplitudeOffsetZ;
    public float ground2OffsetX, ground2OffsetZ;
}