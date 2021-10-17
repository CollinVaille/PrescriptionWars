
using UnityEngine;

public class TerrainOffsets
{
    public float noiseOffsetX, noiseOffsetZ;
    public float amplitudeOffsetX, amplitudeOffsetZ;

    //Remember old terrain offsets (for purpose of regenerating terrain to be like old one)
    public TerrainOffsets(float noiseOffsetX, float noiseOffsetZ, float amplitudeOffsetX, float amplitudeOffsetZ)
    {
        this.noiseOffsetX = noiseOffsetX;
        this.noiseOffsetZ = noiseOffsetZ;

        this.amplitudeOffsetX = amplitudeOffsetX;
        this.amplitudeOffsetZ = amplitudeOffsetZ;
    }

    //Generate new offsets (for purpose of generating new random terrain)
    public TerrainOffsets()
    {
        noiseOffsetX = Random.Range(0.0f, 10000.0f);
        noiseOffsetZ = Random.Range(0.0f, 10000.0f);

        amplitudeOffsetX = Random.Range(0.0f, 10000.0f);
        amplitudeOffsetZ = Random.Range(0.0f, 10000.0f);
    }
}