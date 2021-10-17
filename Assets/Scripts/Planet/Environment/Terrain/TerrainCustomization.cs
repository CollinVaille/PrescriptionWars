
using UnityEngine;

public class TerrainCustomization
{
    //Main Heightmap Config
    [Tooltip("Inverse X-Z scale of noise")] public float noiseGroundScale;
    [Tooltip("Inverse X-Z scale of amplitude")] public float amplitudeGroundScale;
    [Tooltip("How dramatic the height difference is between landforms")] public int amplitudePower;
    [Tooltip("How big the hills are")] public float noiseStrength;

    //Optional Heightmap Config
    [Tooltip("Usually false. Set to true if you want terrain height to cap at horizon height.")] public bool horizonHeightIsCeiling;

    //Trees
    public int idealTreeCount, maxTreeSteepness;
    public string[] treeNames;

    //Layers
    public Texture2D groundTexture, cliffTexture, seabedTexture;
    public float groundMetallic, groundSmoothness, cliffMetallic, cliffSmoothness, seabedMetallic, seabedSmoothness;

    //Misc
    public bool lowBoundaries, smallTerrain;
    public float seabedHeight;

    //Remember old terrain customization (for purpose of regenerating terrain to be like old one)
    public TerrainCustomization(float noiseGroundScale, float amplitudeGroundScale, int amplitudePower, float noiseStrength)
    {
        InitializeParametersToDefaults();

        this.noiseGroundScale = noiseGroundScale;
        this.amplitudeGroundScale = amplitudeGroundScale;
        this.amplitudePower = amplitudePower;
        this.noiseStrength = noiseStrength;
    }

    //Generate default customization (for purpose of generating new random terrain)
    public TerrainCustomization() { InitializeParametersToDefaults(); }

    public void InitializeParametersToDefaults()
    {
        noiseGroundScale = 40;
        amplitudeGroundScale = 8;
        amplitudePower = 3;
        noiseStrength = 0.5f;
        horizonHeightIsCeiling = false;

        idealTreeCount = 0;
        maxTreeSteepness = 30;
        treeNames = new string[1];
        treeNames[0] = "Palm Tree";

        groundTexture = null;
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
