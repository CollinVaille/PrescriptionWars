using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockyDesertBiome
{
    public static void GenerateBiome(Planet planet, PlanetGenerator generator, out TerrainCustomization terrainCustomization)
    {
        if (Random.Range(0, 2) == 3)
            GenerateRockyValleys(planet, generator, out terrainCustomization);
        else
            GenerateJaggedSeas(planet, generator, out terrainCustomization);
    }

    private static void GenerateRockyValleys(Planet planet, PlanetGenerator generator, out TerrainCustomization terrainCustomization)
    {
        terrainCustomization = new TerrainCustomization();

        //Sun
        generator.GenerateSun(planet, Random.Range(1.0f, 1.05f));

        //Reverb
        planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Arena;

        //Dirty brown water
        planet.SetUnderwaterColor(new Color(70 / 255.0f, 55 / 255.0f, 55 / 255.0f, 0.5f));

        //Terrain textures
        terrainCustomization.groundTexture = planet.LoadTexture("Red Desert", "Mars");
        terrainCustomization.cliffTexture = planet.LoadTexture("Slumbering Volcano");
        terrainCustomization.seabedTexture = terrainCustomization.groundTexture;

        //Terrain heightmap
        terrainCustomization.lowBoundaries = false;
        terrainCustomization.horizonHeightIsCeiling = true;
        terrainCustomization.noiseGroundScale = Random.Range(7, 17);
        terrainCustomization.amplitudeGroundScale = 10;
        terrainCustomization.amplitudePower = 3;
        terrainCustomization.noiseStrength = Random.Range(1.25f, 2.25f);

        //Footsteps
        planet.LoadGroundFootsteps("Martian Dirt");
        planet.seabedWalking = planet.groundWalking;
        planet.seabedRunning = planet.groundRunning;

        //Ominous, luminating, arid fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = Random.Range(0.001f, 0.002f);
        RenderSettings.fogColor = SunType.GetColorRGB(91, 47, 33);

        //Day skybox & ambience
        planet.LoadSkybox(true, "Brown Cloud");
        planet.dayAmbience = planet.LoadAmbience("Night Of The Cacti");

        //Night skybox & ambience
        if (Random.Range(0, 2) == 0)
            planet.LoadSkybox(false, "Deep Dusk");
        else
            planet.LoadSkybox(false, "Yellow Galaxy", "Spiral Galaxy", "Galaxy Field 1", "Galaxy Field 2");
        planet.nightAmbience = planet.LoadAmbience("Night Of The Cacti");
    }

    private static void GenerateJaggedSeas(Planet planet, PlanetGenerator generator, out TerrainCustomization terrainCustomization)
    {
        terrainCustomization = new TerrainCustomization();

        //Sun
        generator.GenerateSun(planet, Random.Range(1.0f, 1.05f));

        //Reverb
        planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Arena;

        //Dirty brown water
        planet.SetUnderwaterColor(new Color(70 / 255.0f, 55 / 255.0f, 55 / 255.0f, 0.5f));

        //Set water level
        planet.SetOcean(Random.Range(25, 75), Planet.OceanType.Murky);

        //Set seabed height to be above water
        terrainCustomization.seabedHeight = planet.oceanTransform.position.y + Random.Range(1.5f, 3.0f);

        //Terrain textures
        terrainCustomization.groundTexture = planet.LoadTexture("Soil Beach 0079", "Soil Beach 0052", "Sahara");
        terrainCustomization.cliffTexture = planet.LoadTexture("Slumbering Volcano", "Cliffs 0120");
        terrainCustomization.seabedTexture = planet.LoadTexture("Soil Beach 0079", "Soil Beach 0052");

        //Terrain heightmap
        terrainCustomization.lowBoundaries = true;
        terrainCustomization.noiseGroundScale = Random.Range(7, 17);
        terrainCustomization.amplitudeGroundScale = 10;
        terrainCustomization.amplitudePower = 3;
        terrainCustomization.noiseStrength = Random.Range(1.25f, 2.25f);

        //Footsteps
        planet.LoadGroundFootsteps("Sand");
        planet.LoadSeabedFootsteps("Swamp");

        //Ominous, luminating, arid fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = Random.Range(0.001f, 0.002f);
        RenderSettings.fogColor = SunType.GetColorRGB(91, 47, 33);

        //Day skybox & ambience
        planet.LoadSkybox(true, "Brown Cloud");
        planet.dayAmbience = planet.LoadAmbience("Night Of The Cacti");

        //Night skybox & ambience
        if (Random.Range(0, 2) == 0)
            planet.LoadSkybox(false, "Deep Dusk");
        else
            planet.LoadSkybox(false, "Yellow Galaxy", "Spiral Galaxy", "Galaxy Field 1", "Galaxy Field 2");
        planet.nightAmbience = planet.LoadAmbience("Night Of The Cacti");
    }
}
