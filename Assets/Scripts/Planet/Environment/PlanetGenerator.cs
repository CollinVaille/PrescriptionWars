using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    //PLANET GENERATION----------------------------------------------------------------------------------------

    public IEnumerator GeneratePlanet(Planet planet)
    {
        //Day length
        //dayLength = Random.Range(60, 500);

        //FIRST, DETERMINE BIOME--------------------------------------------------------------------------

        SelectBiome(planet);

        //THEN, GENERATE BIOME----------------------------------------------------------------------------

        //This generates everything to do with the environment of the planet based on the biome. Things like skyboxes, ambience, the sun, terrain textures, etc.
        CustomizePlanetBasedOnBiome(planet, out TerrainCustomization terrainCustomization);

        //THEREAFTER, GENERATE TERRAIN-------------------------------------------------------------------------------

        //Generating the terrain also implicitly generates the cities. Before we generate the cities, we need to do this preparation
        Planet.planet.planetWideCityCustomization = new PlanetCityCustomization();
        Planet.planet.planetWideCityCustomization.GenerateCityCustomizationForNewPlanet();

        //Finally, actually generate the terrain
        yield return StartCoroutine(PlanetTerrain.planetTerrain.GenerateTerrain(terrainCustomization));

        //FINALLY, GIVE IT A FUCKING NAME AND SAVE THE BITCH---------------------------------------------------------

        //Temp name
        planet.planetName = PlanetNameGenerator.GeneratePlanetName();

        //SavePlanet();
    }

    //BIOME GENERATION----------------------------------------------------------------------------------------

    public void SelectBiome(Planet planet)
    {
        //Already set biome as test case
        if (planet.biome != Planet.Biome.Unknown)
            return;

        planet.biome = GetRandomBiome();
    }

    public static Planet.Biome GetRandomBiome()
    {
        //Randomly select
        int picker = Random.Range(0, 6);
        switch (picker)
        {
            case 0: return Planet.Biome.Frozen;
            case 1: return Planet.Biome.Temperate;
            case 2: return Planet.Biome.Desert;
            case 3: return Planet.Biome.Swamp;
            case 4: return Planet.Biome.Hell;
            default: return Planet.Biome.Spirit;
        }
    }

    private void CustomizePlanetBasedOnBiome(Planet planet, out TerrainCustomization terrainCustomization)
    {
        //Get biome
        string biomeName = GetBiomeName(planet.biome);

        //Get sub biome
        SubBiomeJSON subBiomeJSON;
        if (string.IsNullOrEmpty(Planet.planet.subBiome))
            subBiomeJSON = GetSubBiomeWithDegreeOfRandomness(biomeName, Random.Range(0.0f, 1.0f));
        else
            subBiomeJSON = GetSubBiome(biomeName, Planet.planet.subBiome);

        //Generate the planet based on that sub biome
        GeneratePlanetFromSubBiome(planet, subBiomeJSON, out terrainCustomization);
    }

    private static SubBiomeJSON GetSubBiomeWithDegreeOfRandomness(string biomeName, float randomness)
    {
        SubBiomeJSON targetSubBiomeJSON = GetRandomUnalteredSubBiomeOfBiome(biomeName);

        Debug.Log("Randomness: " + randomness);

        if (Mathf.Approximately(randomness, 0.0f))
            return targetSubBiomeJSON;

        SubBiomeJSON referenceSubBiomeJSON;
        if(randomness < Random.Range(0.5f, 1.0f))
            referenceSubBiomeJSON = GetRandomUnalteredSubBiomeOfBiome(biomeName);
        else
            referenceSubBiomeJSON = GetRandomUnalteredSubBiomeOfBiome(GetBiomeName(GetRandomBiome()));

        bool referenceFromSameBiome = referenceSubBiomeJSON.biomeName.Equals(targetSubBiomeJSON.biomeName);
        for (int attemptsAtRandomization = 0; attemptsAtRandomization < 100 && randomness > 0.0f; attemptsAtRandomization++)
            randomness -= PerformRandomModificationToTargetSubBiome(targetSubBiomeJSON, referenceSubBiomeJSON, randomness, referenceFromSameBiome);

        return targetSubBiomeJSON;
    }

    private static float PerformRandomModificationToTargetSubBiome(SubBiomeJSON targetSubBiomeJSON, SubBiomeJSON referenceSubBiomeJSON, float allowedRandomness, bool referenceFromSameBiome)
    {
        int changeType = Random.Range(0, 11);

        switch(changeType)
        {
            case 0:
                if(allowedRandomness > 0.15f)
                {
                    targetSubBiomeJSON.fogChance = referenceSubBiomeJSON.fogChance;
                    targetSubBiomeJSON.fogMode = referenceSubBiomeJSON.fogMode;
                    targetSubBiomeJSON.fogDensityRange = referenceSubBiomeJSON.fogDensityRange;
                    return 0.15f;
                }
                break;

            case 1:
                if (allowedRandomness > 0.2f)
                {
                    targetSubBiomeJSON.fogColor = referenceSubBiomeJSON.fogColor;
                    targetSubBiomeJSON.sunColor = referenceSubBiomeJSON.sunColor;
                    targetSubBiomeJSON.sunIntensityRange = referenceSubBiomeJSON.sunIntensityRange;
                    return 0.2f;
                }
                break;

            case 2:
                if (allowedRandomness > 0.1f)
                {
                    targetSubBiomeJSON.oceanHeightRange = referenceSubBiomeJSON.oceanHeightRange;
                    return 0.1f;
                }
                break;

            case 3:
                if (allowedRandomness > 0.25f && referenceFromSameBiome && referenceSubBiomeJSON.seabedTexture != null && referenceSubBiomeJSON.seabedTexture.Length > 0)
                {
                    targetSubBiomeJSON.underwaterColor = referenceSubBiomeJSON.underwaterColor;
                    targetSubBiomeJSON.oceanMaterial = referenceSubBiomeJSON.oceanMaterial;
                    targetSubBiomeJSON.seabedTexture = referenceSubBiomeJSON.seabedTexture;
                    targetSubBiomeJSON.seabedMetallicness = referenceSubBiomeJSON.seabedMetallicness;
                    targetSubBiomeJSON.seabedSmoothness = referenceSubBiomeJSON.seabedSmoothness;
                    targetSubBiomeJSON.seabedMaterial = referenceSubBiomeJSON.seabedMaterial;
                    targetSubBiomeJSON.wetSeabedMaterial = referenceSubBiomeJSON.wetSeabedMaterial;
                    targetSubBiomeJSON.drySeabedMaterial = referenceSubBiomeJSON.drySeabedMaterial;
                    return 0.25f;
                }
                break;

            case 4:
                if (allowedRandomness > 0.1f && referenceFromSameBiome && referenceSubBiomeJSON.cliffTexture != null && referenceSubBiomeJSON.cliffTexture.Length > 0)
                {
                    targetSubBiomeJSON.cliffTexture = referenceSubBiomeJSON.cliffTexture;
                    targetSubBiomeJSON.cliffMetallicness = referenceSubBiomeJSON.cliffMetallicness;
                    targetSubBiomeJSON.cliffSmoothness = referenceSubBiomeJSON.cliffSmoothness;
                    return 0.1f;
                }
                break;

            case 5:
                if (allowedRandomness > 0.25f && referenceFromSameBiome && referenceSubBiomeJSON.groundTexture != null && referenceSubBiomeJSON.groundTexture.Length > 0)
                {
                    targetSubBiomeJSON.groundTexture = referenceSubBiomeJSON.groundTexture;
                    targetSubBiomeJSON.ground2Texture = referenceSubBiomeJSON.ground2Texture;
                    targetSubBiomeJSON.ground2TextureScaleRange = referenceSubBiomeJSON.ground2TextureScaleRange;
                    targetSubBiomeJSON.groundMetallicness = referenceSubBiomeJSON.groundMetallicness;
                    targetSubBiomeJSON.groundSmoothness = referenceSubBiomeJSON.groundSmoothness;
                    targetSubBiomeJSON.groundMaterial = referenceSubBiomeJSON.groundMaterial;
                    return 0.25f;
                }
                break;

            case 6:
                if (allowedRandomness > 0.2f && referenceFromSameBiome)
                {
                    targetSubBiomeJSON.idealTreeCountRange = referenceSubBiomeJSON.idealTreeCountRange;
                    targetSubBiomeJSON.treeNames = referenceSubBiomeJSON.treeNames;
                    targetSubBiomeJSON.maxTreeSteepnessRange = referenceSubBiomeJSON.maxTreeSteepnessRange;
                    return 0.2f;
                }
                break;

            case 7:
            case 8:
            case 9:
            default:
                if (allowedRandomness > 0.15f)
                {
                    targetSubBiomeJSON.terrainSculpting = referenceSubBiomeJSON.terrainSculpting;
                    return 0.15f;
                }
                break;
        }

        return 0.0f;
    }

    private static SubBiomeJSON GetRandomUnalteredSubBiomeOfBiome(string biomeName)
    {
        string subBiomeName = GeneralHelperMethods.GetLineFromFile("Planet/Environment/Sub Biomes/Sub Biome Lists/" + biomeName, startPathFromGeneralTextFolder: false);
        SubBiomeJSON subBiomeJSON = GetSubBiome(biomeName, subBiomeName);

        subBiomeJSON.biomeName = biomeName;
        subBiomeJSON.subBiomeName = subBiomeName;
        
        return subBiomeJSON;
    }

    private static SubBiomeJSON GetSubBiome(string biomeName, string subBiomeName)
    {
        string subBiomeJsonAsString = GeneralHelperMethods.GetTextAsset("Planet/Environment/Sub Biomes/" + biomeName + "/" + subBiomeName, startPathFromGeneralTextFolder: false).text;
        return JsonUtility.FromJson<SubBiomeJSON>(subBiomeJsonAsString);
    }

    private void GeneratePlanetFromSubBiome(Planet planet, SubBiomeJSON subBiomeJSON, out TerrainCustomization terrainCustomization)
    {
        terrainCustomization = new TerrainCustomization();

        float sunIntensity = GetRandomValueFromRange(subBiomeJSON.sunIntensityRange);

        if (GetColorIfSpecified(subBiomeJSON.sunColor, out Color sunColor))
            GenerateSun(planet, sunIntensity, sunColor);
        else
            GenerateSun(planet, sunIntensity);

        if (System.Enum.TryParse<AudioReverbPreset>(subBiomeJSON.reverbPreset, out AudioReverbPreset audioReverbPreset))
            planet.GetComponent<AudioReverbZone>().reverbPreset = audioReverbPreset;

        if(GetColorIfSpecified(subBiomeJSON.underwaterColor, out Color underWaterColor))
            planet.SetUnderwaterColor(underWaterColor);

        if (!System.Enum.TryParse<Planet.OceanType>(subBiomeJSON.oceanType, out Planet.OceanType oceanType))
            oceanType = Planet.OceanType.Water;

        string oceanMaterialSelection = GetOneOf(subBiomeJSON.oceanMaterial);
        if (oceanMaterialSelection == null)
            oceanMaterialSelection = "Water";

        planet.SetOcean(GetRandomValueFromRange(subBiomeJSON.oceanHeightRange), oceanType, oceanMaterialSelection);

        terrainCustomization.seabedHeight = planet.oceanTransform.position.y + GetRandomValueFromRange(subBiomeJSON.seabedRelativeHeightRange, defaultValue: 7);

        terrainCustomization.groundTexture = Planet.LoadTexture(subBiomeJSON.groundTexture);

        string ground2Texture = GetOneOf(subBiomeJSON.ground2Texture);
        if(!string.IsNullOrEmpty(ground2Texture))
        {
            terrainCustomization.ground2Texture = Planet.LoadTexture(ground2Texture);
            terrainCustomization.ground2TextureScale = GetRandomValueFromRange(subBiomeJSON.ground2TextureScaleRange, 100.0f);
        }

        terrainCustomization.cliffTexture = subBiomeJSON.cliffHasSameTextureAsGround ? terrainCustomization.groundTexture : Planet.LoadTexture(subBiomeJSON.cliffTexture);

        if (subBiomeJSON.seabedHasSameTextureAsGround)
            terrainCustomization.seabedTexture = terrainCustomization.groundTexture;
        else if (subBiomeJSON.seabedHasSameTextureAsCliff)
            terrainCustomization.seabedTexture = terrainCustomization.cliffTexture;
        else
            terrainCustomization.seabedTexture = Planet.LoadTexture(subBiomeJSON.seabedTexture);

        terrainCustomization.groundMetallic = subBiomeJSON.groundMetallicness;
        terrainCustomization.groundSmoothness = subBiomeJSON.groundSmoothness;
        terrainCustomization.cliffMetallic = subBiomeJSON.cliffMetallicness;
        terrainCustomization.cliffSmoothness = subBiomeJSON.cliffSmoothness;
        terrainCustomization.seabedMetallic = subBiomeJSON.seabedMetallicness;
        terrainCustomization.seabedSmoothness = subBiomeJSON.seabedSmoothness;

        string terrainSculptingFileName = GetOneOf(subBiomeJSON.terrainSculpting);
        string terrainSculptingJSONAsString = GeneralHelperMethods.GetTextAsset("Planet/Environment/Terrain Sculpting/" + terrainSculptingFileName, startPathFromGeneralTextFolder: false).text;
        TerrainSculptingJSON terrainSculptingJSON = JsonUtility.FromJson<TerrainSculptingJSON>(terrainSculptingJSONAsString);

        terrainCustomization.lowBoundaries = terrainSculptingJSON.lowBoundariesChance > Random.Range(0.0f, 1.0f);

        terrainCustomization.terrainSculptingLayers = new List<TerrainSculptingLayerSelectionJSON>();
        for(int x = 0; x < terrainSculptingJSON.layers.Length; x++)
        {
            if(Random.Range(0.0f, 1.0f) < terrainSculptingJSON.layers[x].chance || terrainSculptingJSON.layers.Length == 1)
                terrainCustomization.terrainSculptingLayers.Add(new TerrainSculptingLayerSelectionJSON(terrainSculptingJSON.layers[x]));
        }

        if (System.Enum.TryParse<PlanetMaterialType>(GetOneOf(subBiomeJSON.groundMaterial), out PlanetMaterialType groundMaterialType))
            planet.LoadGroundMaterial(groundMaterialType);

        if(subBiomeJSON.seabedHasSameMaterialAsGround)
            planet.LoadSeabedMaterial(groundMaterialType);
        else if (subBiomeJSON.seabedMaterial != null && subBiomeJSON.seabedMaterial.Length > 0)
        {
            if (System.Enum.TryParse<PlanetMaterialType>(GetOneOf(subBiomeJSON.seabedMaterial), out PlanetMaterialType seabedMaterialType))
                planet.LoadSeabedMaterial(seabedMaterialType);
        }
        else
        {
            if(planet.hasLiquidOcean)
            {
                if (System.Enum.TryParse<PlanetMaterialType>(GetOneOf(subBiomeJSON.wetSeabedMaterial), out PlanetMaterialType seabedMaterialType))
                    planet.LoadSeabedMaterial(seabedMaterialType);
            }
            else
            {
                if (System.Enum.TryParse<PlanetMaterialType>(GetOneOf(subBiomeJSON.drySeabedMaterial), out PlanetMaterialType seabedMaterialType))
                    planet.LoadSeabedMaterial(seabedMaterialType);
            }
        }

        RenderSettings.fog = subBiomeJSON.fogChance > Random.Range(0.0f, 1.0f);

        if (System.Enum.TryParse<FogMode>(subBiomeJSON.fogMode, out FogMode fogMode))
            RenderSettings.fogMode = fogMode;

        RenderSettings.fogDensity = GetRandomValueFromRange(subBiomeJSON.fogDensityRange);
        if (GetColorIfSpecified(subBiomeJSON.fogColor, out Color fogColor))
            RenderSettings.fogColor = fogColor;

        string daySkybox = GetOneOf(subBiomeJSON.daySkybox);
        planet.LoadSkybox(true, daySkybox);
        planet.LoadSkybox(false, subBiomeJSON.nightHasSameSkyboxAsDay ? daySkybox : GetOneOf(subBiomeJSON.nightSkybox));

        planet.dayAmbience = planet.LoadAmbience(subBiomeJSON.dayAmbience);
        planet.nightAmbience = planet.LoadAmbience(subBiomeJSON.nightAmbience);

        terrainCustomization.idealTreeCount = GetRandomValueFromRange(subBiomeJSON.idealTreeCountRange);
        terrainCustomization.SetTreeNames(subBiomeJSON.treeNames != null ? subBiomeJSON.treeNames : new string[] { "Palm Tree" });
        terrainCustomization.maxTreeSteepness = GetRandomValueFromRange(subBiomeJSON.maxTreeSteepnessRange, defaultValue: 30);
    }

    public static float GetRandomValueFromRange(float[] range, float defaultValue = 0.0f)
    {
        if (range == null || range.Length == 0)
            return defaultValue;

        if (range.Length == 1)
            return range[0];

        return Random.Range(range[0], range[1]);
    }

    public static int GetRandomValueFromRange(int[] range, int defaultValue = 0)
    {
        if (range == null || range.Length == 0)
            return defaultValue;

        if (range.Length == 1)
            return range[0];

        return Random.Range(range[0], range[1]);
    }

    private static string GetOneOf(string[] options)
    {
        if (options == null || options.Length == 0)
            return null;

        return options[Random.Range(0, options.Length)];
    }

    private static bool GetColorIfSpecified(HumanFriendlyColorJSON colorJSON, out Color color)
    {
        if (colorJSON == null || (colorJSON.r == 0 && colorJSON.g == 0 && colorJSON.b == 0 && Mathf.Approximately(colorJSON.a, 0.0f)))
        {
            color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            return false;
        }

        color = new Color(colorJSON.r / 255.0f, colorJSON.g / 255.0f, colorJSON.b / 255.0f, colorJSON.a);
        return true;
    }

    private static string GetBiomeName(Planet.Biome biome) { return GeneralHelperMethods.GetEnumText(biome.ToString()); }

    //SUN GENERATION----------------------------------------------------------------------------------------

    public void GenerateSun(Planet planet, float intensity)
    {
        SunType sunType = new SunType(intensity);

        ApplySunType(planet, sunType);
    }

    public void GenerateSun(Planet planet, float intensity, Color sunColor)
    {
        SunType sunType = new SunType(intensity, sunColor);

        ApplySunType(planet, sunType);
    }

    private void ApplySunType(Planet planet, SunType sunType)
    {
        //Lens flare for sun
        planet.sun.GetComponent<Light>().flare = Resources.Load<Flare>("Planet/Environment/Lens Flares/" + sunType.flareName);

        //Sunlight color
        planet.sun.GetComponent<Light>().color = sunType.sunlightColor;

        //Sunlight intensity
        planet.sun.GetComponent<Light>().intensity = sunType.intensity;
    }
}

[System.Serializable]
public class SubBiomeJSON
{
    public string biomeName;
    public string subBiomeName;

    public float[] sunIntensityRange;
    public HumanFriendlyColorJSON sunColor;
    
    public string reverbPreset = "Arena";
    
    public HumanFriendlyColorJSON underwaterColor;
    public int[] oceanHeightRange;
    public string oceanType = "Water";
    public string[] oceanMaterial;
    public float[] seabedRelativeHeightRange;

    public string[] groundTexture;
    public string[] ground2Texture;
    public float[] ground2TextureScaleRange;
    public string[] cliffTexture;
    public string[] seabedTexture;
    public bool cliffHasSameTextureAsGround = false;
    public bool seabedHasSameTextureAsGround = false;

    public float groundMetallicness = 0.0f;
    public float groundSmoothness = 0.0f;
    public float cliffMetallicness = 0.0f;
    public float cliffSmoothness = 0.0f;
    public float seabedMetallicness = 0.0f;
    public float seabedSmoothness = 0.0f;

    public string[] terrainSculpting;

    public string[] groundMaterial;
    public string[] seabedMaterial;
    public string[] wetSeabedMaterial;
    public string[] drySeabedMaterial;
    public bool seabedHasSameMaterialAsGround = false;
    public bool seabedHasSameTextureAsCliff = false;

    public float fogChance = 0.0f;
    public string fogMode = "Exponential";
    public float[] fogDensityRange;
    public HumanFriendlyColorJSON fogColor;

    public string[] daySkybox;
    public string[] nightSkybox;
    public bool nightHasSameSkyboxAsDay = false;

    public string[] dayAmbience;
    public string[] nightAmbience;

    public int[] idealTreeCountRange;
    public string[] treeNames;
    public int[] maxTreeSteepnessRange;
}


[System.Serializable]
public class TerrainSculptingJSON
{
    public float lowBoundariesChance = 1.0f;
    public TerrainSculptingLayerOptionsJSON[] layers;
}

[System.Serializable]
public class TerrainSculptingLayerOptionsJSON
{
    public string comments;
    public string editType;
    public string condition;
    public float chance = 1.0f;

    public float horizonHeightIsCeilingChance = 0.0f;
    public float[] noiseGroundScaleRange;
    public float[] amplitudeGroundScaleRange;
    public int[] amplitudePowerRange;
    public float[] noiseStrengthRange;
}

public enum TerrainSculptingType { Overwrite, Add, Subtract }
public enum TerrainSculptingCondition { Unconditional, AboveHorizon, BelowHorizon }

[System.Serializable]
public class HumanFriendlyColorJSON
{
    public int r = 0, g = 0, b = 0; //0-255
    public float a = 0.0f; //0-1
}