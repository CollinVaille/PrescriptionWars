
using UnityEngine;

public class TerrainOffsets
{
    public float noiseOffsetX, noiseOffsetZ;
    public float amplitudeOffsetX, amplitudeOffsetZ;
    public float ground2OffsetX, ground2OffsetZ;

    //Remember old terrain offsets (for purpose of regenerating terrain to be like old one)
    public TerrainOffsets(float noiseOffsetX, float noiseOffsetZ, float amplitudeOffsetX, float amplitudeOffsetZ, float ground2OffsetX, float ground2OffsetZ)
    {
        this.noiseOffsetX = noiseOffsetX;
        this.noiseOffsetZ = noiseOffsetZ;

        this.amplitudeOffsetX = amplitudeOffsetX;
        this.amplitudeOffsetZ = amplitudeOffsetZ;

        this.ground2OffsetX = ground2OffsetX;
        this.ground2OffsetZ = ground2OffsetZ;
    }

    //Generate new offsets (for purpose of generating new random terrain)
    public TerrainOffsets()
    {
        noiseOffsetX = Random.Range(0.0f, 10000.0f);
        noiseOffsetZ = Random.Range(0.0f, 10000.0f);

        amplitudeOffsetX = Random.Range(0.0f, 10000.0f);
        amplitudeOffsetZ = Random.Range(0.0f, 10000.0f);

        ground2OffsetX = Random.Range(0.0f, 10000.0f);
        ground2OffsetZ = Random.Range(0.0f, 10000.0f);
    }
}