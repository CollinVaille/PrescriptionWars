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

        //Set up stuff for biomes
        planet.SetOcean(-10, Planet.OceanType.Normal); //By default ocean is disabled (negative height disables ocean)

        //This also generates the sun based on the biome
        GenerateBiome(planet, out TerrainCustomization terrainCustomization);

        //THEREAFTER, GENERATE TERRAIN-------------------------------------------------------------------------------

        yield return StartCoroutine(PlanetTerrain.planetTerrain.GenerateTerrain(terrainCustomization));

        //FINALLY, GIVE IT A FUCKING NAME AND SAVE THE BITCH---------------------------------------------------------

        //Temp name
        planet.planetName = GeneralHelperMethods.GetLineFromFile("Location Names/Planet Names");

        //SavePlanet();
    }

    //BIOME GENERATION----------------------------------------------------------------------------------------

    public void SelectBiome(Planet planet)
    {
        //Already set biome as test case
        if (planet.biome != Planet.Biome.Unknown)
            return;

        //Randomly select
        int picker = Random.Range(0, 6);
        switch (picker)
        {
            case 0: planet.biome = Planet.Biome.Frozen; break;
            case 1: planet.biome = Planet.Biome.Temperate; break;
            case 2: planet.biome = Planet.Biome.SandyDesert; break;
            case 3: planet.biome = Planet.Biome.Swamp; break;
            case 4: planet.biome = Planet.Biome.Hell; break;
            default: planet.biome = Planet.Biome.Spirit; break;
        }
    }

    private void GenerateBiome(Planet planet, out TerrainCustomization terrainCustomization)
    {
        terrainCustomization = new TerrainCustomization();

        if (planet.biome == Planet.Biome.Frozen) //Frozen
        {
            //Sun
            if (Random.Range(0, 3) == 0)
                GenerateSun(planet, Random.Range(0.05f, 0.5f));
            else if (Random.Range(0, 2) == 0)
                GenerateSun(planet, Random.Range(0.2f, 0.6f));
            else
                GenerateSun(planet, Random.Range(0.3f, 0.85f));

            //Terrain textures
            terrainCustomization.groundTexture = planet.LoadTexture("Snow", "Snow 0087", "Snow 0096");
            terrainCustomization.cliffTexture = planet.LoadTexture("Deep Freeze", "Snow 0126", "Age of the Canyon", "Glacier");
            terrainCustomization.seabedTexture = terrainCustomization.cliffTexture;

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 4) == 0;
            terrainCustomization.noiseGroundScale = Random.Range(30, 50);
            terrainCustomization.amplitudeGroundScale = Random.Range(5, 15);
            terrainCustomization.amplitudePower = Random.Range(2, 5);
            terrainCustomization.noiseStrength = Random.Range(0.35f, 0.75f);

            //Reverb
            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Hangar;

            //Footsteps
            planet.LoadGroundFootsteps("Snow");
            planet.seabedWalking = planet.groundWalking;
            planet.seabedRunning = planet.groundRunning;

            //Water
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, planet.sun.GetComponent<Light>().intensity) < 0.35f)
                    planet.SetOcean(Random.Range(1, 10), Planet.OceanType.Frozen);
                else
                {
                    planet.SetOcean(Random.Range(1, 10), Planet.OceanType.Normal);
                    planet.SetUnderwaterColor(new Color(0 / 255.0f, 27 / 255.0f, 108 / 255.0f, 0.5f));
                }
            }

            //Seabed height relative to water level
            terrainCustomization.seabedHeight = planet.oceanTransform.position.y - Random.Range(1.5f, 3.0f);

            FinishNormalBiomeSetUp(planet, planet.sun.GetComponent<Light>().intensity);
        }
        else if (planet.biome == Planet.Biome.Temperate) //Temperate
        {
            //Sun
            GenerateSun(planet, Random.Range(0.85f, 1.15f));

            //Terrain textures
            terrainCustomization.groundTexture = planet.LoadTexture("Grass 0043", "Grass 0103", "Common Ground", "Twisted Hills");
            terrainCustomization.cliffTexture = planet.LoadTexture("Rock Grassy 0030", "Cliffs 0120", "Age of the Canyon");

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 2) == 0;
            terrainCustomization.noiseGroundScale = Random.Range(30, 50);
            terrainCustomization.amplitudeGroundScale = Random.Range(7, 10);
            terrainCustomization.amplitudePower = Random.Range(0, 4) != 0 ? 3 : Random.Range(2, 5);
            terrainCustomization.noiseStrength = Random.Range(0.4f, 0.6f);

            //Basic sound
            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Plain;
            planet.LoadGroundFootsteps("Grass");

            //Water
            planet.SetUnderwaterColor(new Color(0 / 255.0f, 48 / 255.0f, 255 / 255.0f, 0.5f));
            if (Random.Range(0, 3) != 0)
            {
                //Set water level
                planet.SetOcean(Random.Range(1, 10), Planet.OceanType.Normal);

                //Set seabed
                if (Random.Range(0, 3) != 0) //Beach
                {
                    terrainCustomization.seabedTexture = planet.LoadTexture("Sahara", "Soil Beach 0052", "Soil Beach 0079");
                    planet.LoadSeabedFootsteps("Swamp");

                    //Set seabed height to be above water
                    terrainCustomization.seabedHeight = planet.oceanTransform.position.y + Random.Range(1.5f, 3.0f);
                }
                else //Rock shore
                {
                    terrainCustomization.seabedTexture = planet.LoadTexture("Rock Grassy 0030", "Cliffs 0120", "Age of the Canyon");
                    planet.LoadSeabedFootsteps("Rock");

                    //Set seabed height to be around water level
                    terrainCustomization.seabedHeight = planet.oceanTransform.position.y + Random.Range(-1.5f, 1.5f);
                }
            }
            else //No water so make seabed appear like ground
            {
                terrainCustomization.seabedTexture = terrainCustomization.groundTexture;
                planet.seabedWalking = planet.groundWalking;
                planet.seabedRunning = planet.groundRunning;
            }

            //Trees
            if (Random.Range(0, 1) == 0)
            {
                if (Random.Range(0, 1) == 0) //Jungle trees
                {
                    terrainCustomization.SetTreeNames("Jungle Tree");
                    terrainCustomization.maxTreeSteepness = 20;

                    if (Random.Range(0, 1) == 0) //Thick forest
                    {
                        planet.biomeSubType = Planet.BiomeSubType.Forest;

                        terrainCustomization.groundTexture = planet.LoadTexture("Common Ground", "Darkland Forest");
                        terrainCustomization.cliffTexture = planet.LoadTexture("Faulted Range", "Fractured Flow");

                        if (!planet.hasOcean)
                            terrainCustomization.seabedHeight = -10;

                        terrainCustomization.idealTreeCount = 450;
                    }
                    else //Sparse forest
                        terrainCustomization.idealTreeCount = Random.Range(100, 200);
                }
                else if (planet.hasOcean && Random.Range(0, 2) == 0) //Palm trees
                {
                    terrainCustomization.idealTreeCount = Random.Range(50, 200);
                    terrainCustomization.SetTreeNames("Palm Tree");
                }
                else
                {
                    terrainCustomization.idealTreeCount = Random.Range(100, 250);
                    terrainCustomization.SetTreeNames("Oak Tree");
                }
            }

            FinishNormalBiomeSetUp(planet, planet.sun.GetComponent<Light>().intensity);
        }
        else if (planet.biome == Planet.Biome.SandyDesert) //Sandy desert
        {
            //Sun
            if (Random.Range(0, 3) == 0)
                GenerateSun(planet, Random.Range(1.0f, 1.3f));
            else
                GenerateSun(planet, Random.Range(1.0f, 1.2f));

            //Terrain textures
            terrainCustomization.groundTexture = planet.LoadTexture("Sahara", "Soil Beach 0052", "Soil Beach 0079");
            terrainCustomization.cliffTexture = planet.LoadTexture("Rocks Arid 0038", "Age of the Canyon");
            terrainCustomization.seabedTexture = terrainCustomization.groundTexture;

            //Terrain heightmap & Water
            if(Random.Range(0, 2) == 0) //Desert canyon
            {
                terrainCustomization.lowBoundaries = false;
                terrainCustomization.horizonHeightIsCeiling = true;
                terrainCustomization.noiseGroundScale = Random.Range(3, 7);
                terrainCustomization.amplitudeGroundScale = 10;
                terrainCustomization.amplitudePower = 1;
                terrainCustomization.noiseStrength = Random.Range(0.75f, 1.75f);
            }
            else if(Random.Range(0, 2) == 0) //Large rolling dunes, i.e. "a sea of sand"
            {
                terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
                terrainCustomization.noiseGroundScale = Random.Range(5, 9);
                terrainCustomization.amplitudeGroundScale = Random.Range(5, 15);
                terrainCustomization.amplitudePower = 1;
                terrainCustomization.noiseStrength = Random.Range(0.2f, 0.35f);
            }
            else //Mountainous desert with room for good variety
            {
                terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
                terrainCustomization.noiseGroundScale = Random.Range(30, 50);
                terrainCustomization.amplitudeGroundScale = Random.Range(7, 10);
                terrainCustomization.amplitudePower = Random.Range(3, 5);
                terrainCustomization.noiseStrength = Random.Range(0.4f, 0.75f);

                if (Random.Range(0, 2) == 0)
                    planet.SetOcean(Random.Range(1, 7), Planet.OceanType.Normal);
            }

            //Reverb
            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Mountains;

            //Water
            planet.SetUnderwaterColor(new Color(0 / 255.0f, 70 / 255.0f, 115 / 255.0f, 0.5f));

            //Footsteps
            planet.LoadGroundFootsteps("Sand");
            planet.LoadSeabedFootsteps(planet.hasOcean ? "Swamp" : "Sand"); //Make sand near water sound wet

            FinishNormalBiomeSetUp(planet, planet.sun.GetComponent<Light>().intensity);
        }
        else if (planet.biome == Planet.Biome.RockyDesert) //Rocky desert
            RockyDesertBiome.GenerateBiome(planet, this, out terrainCustomization);
        else if (planet.biome == Planet.Biome.Swamp) //Swamp
        {
            GenerateSun(planet, Random.Range(1.0f, 1.2f), SunType.GetColorRGB(185, 145, 0));

            terrainCustomization.groundTexture = planet.LoadTexture("Sulfur Wasteland");
            terrainCustomization.cliffTexture = planet.LoadTexture("Rock Grassy 0030");
            terrainCustomization.seabedTexture = planet.LoadTexture("Egg Veins", "Carburetor Resin");

            if (Random.Range(0, 2) == 0)
                planet.LoadSkybox(true, "SkyMorning");
            else
                planet.LoadSkybox(true, "SkySunset");
            planet.LoadSkybox(false, "SkyEarlyDusk");

            planet.dayAmbience = planet.LoadAmbience("Dank Swamp");
            planet.nightAmbience = planet.dayAmbience;

            RenderSettings.fog = true;
            RenderSettings.fogDensity = Random.Range(0.01f, 0.03f);
            RenderSettings.fogColor = SunType.GetColorRGB(108, 100, 4);

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
            terrainCustomization.amplitudePower = 4;
            terrainCustomization.noiseStrength = Random.Range(0.55f, 0.75f);

            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Alley;

            planet.LoadGroundFootsteps("Swamp");
            planet.seabedWalking = planet.groundWalking;
            planet.seabedRunning = planet.groundRunning;

            //Change water color
            planet.SetUnderwaterColor(new Color(72 / 255.0f, 108 / 255.0f, 39 / 255.0f, 0.5f));

            planet.SetOcean(Random.Range(7, 15), Planet.OceanType.Normal);

            //Seabed height relative to water level
            terrainCustomization.seabedHeight = planet.oceanTransform.position.y - Random.Range(1.5f, 3.0f);

            //Trees
            terrainCustomization.idealTreeCount = Random.Range(250, 450);
            terrainCustomization.SetTreeNames("Jungle Tree");
            terrainCustomization.maxTreeSteepness = 20;
        }
        else if (planet.biome == Planet.Biome.Hell) //Hell
        {
            GenerateSun(planet, Random.Range(1.3f, 1.75f), SunType.GetColorRGB(255, 76, 0));

            terrainCustomization.groundTexture = planet.LoadTexture("Searing Gorge", "Pele's Lake", "Lava Cracks");
            terrainCustomization.cliffTexture = planet.LoadTexture("Slumbering Volcano");
            terrainCustomization.seabedTexture = planet.LoadTexture("Noxious Melt", "Mt Etna", "Iodine Atmosphere");

            terrainCustomization.groundMetallic = 1;
            terrainCustomization.groundSmoothness = 0.85f;

            terrainCustomization.cliffMetallic = 1;

            terrainCustomization.seabedMetallic = 1;
            terrainCustomization.seabedSmoothness = 1;

            planet.LoadSkybox(true, "AllSky_Space_AnotherPlanet", "Gloomy", "RedYellowNebular", "RedOrangeYellowNebular");
            planet.LoadSkybox(false, planet.daySkybox.name);

            planet.dayAmbience = planet.LoadAmbience("Large Eerie Reverberant Space", "Dark Empty Hissing");
            planet.nightAmbience = planet.dayAmbience;

            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.0035f;
            RenderSettings.fogColor = SunType.GetColorRGB(67, 52, 52);

            planet.SetUnderwaterColor(new Color(255 / 255.0f, 78 / 255.0f, 0 / 255.0f, 0.5f));

            //Terrain heightmap
            if (Random.Range(0, 3) == 0) //Lava ocean expanse
            {
                terrainCustomization.lowBoundaries = Random.Range(0, 3) != 0;
                terrainCustomization.amplitudePower = 5;
                terrainCustomization.amplitudeGroundScale = Random.Range(2, 5);
                terrainCustomization.noiseGroundScale = Random.Range(25, 35);
                terrainCustomization.noiseStrength = Random.Range(1.15f, 1.35f);

                planet.SetOcean(Random.Range(7, 15), Planet.OceanType.Lava);
            }
            else //Lava mountains
            {
                terrainCustomization.lowBoundaries = Random.Range(0, 3) == 0;
                terrainCustomization.amplitudePower = 3;
                terrainCustomization.amplitudeGroundScale = Random.Range(2, 5);
                terrainCustomization.noiseGroundScale = Random.Range(25, 35);
                terrainCustomization.noiseStrength = Random.Range(1.65f, 1.9f);

                planet.SetOcean(Random.Range(10, 20), Planet.OceanType.Lava);
            }

            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Arena;

            planet.LoadGroundFootsteps("Rock");
            planet.seabedWalking = planet.groundWalking;
            planet.seabedRunning = planet.groundRunning;

            //Rock is molten just at very edge of lava
            terrainCustomization.seabedHeight = planet.oceanTransform.position.y + Random.Range(0.25f, 0.5f);
        }
        else if (planet.biome == Planet.Biome.Spirit) //Spirit
        {
            if (Random.Range(0, 2) == 0) //Warm rock
            {
                GenerateSun(planet, Random.Range(1.3f, 1.5f), SunType.GetColorRGB(210, 240, 255));

                terrainCustomization.groundTexture = planet.LoadTexture("Sputnik");
                planet.LoadGroundFootsteps("Rock");
            }
            else //Cool snow
            {
                GenerateSun(planet, Random.Range(1.1f, 1.3f), SunType.GetColorRGB(210, 240, 255));

                terrainCustomization.groundTexture = planet.LoadTexture("Snow");
                planet.LoadGroundFootsteps("Snow");
            }

            terrainCustomization.cliffTexture = planet.LoadTexture("Dead Sea", "Blue Quartz");
            terrainCustomization.seabedTexture = planet.LoadTexture("Magnified Frost");

            terrainCustomization.cliffMetallic = 1;
            terrainCustomization.cliffSmoothness = 1;

            terrainCustomization.seabedMetallic = 0.2f;
            terrainCustomization.seabedSmoothness = 0.8f;

            planet.LoadSeabedFootsteps("Snow");

            planet.LoadSkybox(true, "Blue Galaxy 1", "Blue Galaxy 2");
            planet.LoadSkybox(false, planet.daySkybox.name);

            planet.dayAmbience = planet.LoadAmbience("Airy Ambience");
            planet.nightAmbience = planet.dayAmbience;

            RenderSettings.fog = false;

            planet.SetUnderwaterColor(new Color(0 / 255.0f, 136 / 255.0f, 255 / 255.0f, 0.5f));
            planet.SetOcean(Random.Range(7, 15), Planet.OceanType.Glowing);

            terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
            terrainCustomization.amplitudePower = 5;
            terrainCustomization.amplitudeGroundScale = Random.Range(4, 6);
            terrainCustomization.noiseGroundScale = Random.Range(20, 40);
            terrainCustomization.noiseStrength = Random.Range(1.1f, 1.2f);

            planet.GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Arena;

            //Seabed glows just above edge of glowing water
            terrainCustomization.seabedHeight = planet.oceanTransform.position.y + Random.Range(0.65f, 0.9f);
        }
    }

    //For Frozen, Temperate, and Desert only
    private void FinishNormalBiomeSetUp(Planet planet, float intensity)
    {
        //Fog (more likely for cold planets, impossible for hot planets)
        float fogFactor = Random.Range(0, intensity * 5);
        if (fogFactor < 1.5f && intensity < Random.Range(0.7f, 1.05f))
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            if (fogFactor < 0.5f && intensity < 0.6f)   //Heavy fog (only for frigid planets)
                RenderSettings.fogDensity = Random.Range(0.025f, 0.15f);
            else //Light fog
                RenderSettings.fogDensity = Random.Range(0.005f, 0.025f);
        }
        else
            RenderSettings.fog = false;

        //Skyboxes and ambient noise (selections depend on temperature and fog)
        SelectDaySkyboxAndAmbience(planet, intensity);
        SelectNightSkyboxAndAmbience(planet, intensity);
    }

    //For Frozen, Temperate, and Desert only
    private void SelectDaySkyboxAndAmbience(Planet planet, float intensity)
    {
        //If there's fog, fog dominates skybox and ambience choice
        if (RenderSettings.fog)
        {
            planet.LoadSkybox(true, "SkyCloudy", "SkyHaloSky", "AllSky_Overcast4_Low");
            planet.dayAmbience = planet.LoadAmbience("Howling Wind");

            return;
        }

        //Otherwise, temperature dictates skybox and ambience choice...

        if (planet.biome == Planet.Biome.Frozen) //Cold
        {
            planet.LoadSkybox(true, "Cold Sunset", "AllSky_Overcast4_Low");
            planet.dayAmbience = planet.LoadAmbience("Light Winter Wind");
        }
        else if (planet.biome == Planet.Biome.Temperate) //Temperate
        {
            planet.LoadSkybox(true, "Epic_GloriousPink", "Epic_BlueSunset", "SkyBrightMorning", "Day_BlueSky_Nothing", "Blue Cloud");

            if (planet.biomeSubType == Planet.BiomeSubType.Forest)
                planet.dayAmbience = planet.LoadAmbience("Rainforest");
            else
                planet.dayAmbience = planet.LoadAmbience("Morning Country Birds", "Quiet Lake with Birds");
        }
        else //Hot
        {
            planet.LoadSkybox(true, "SkySunset", "SkyMorning", "Desert Sky Morning", "Desert World Sky", "Brown Cloud");
            planet.dayAmbience = planet.LoadAmbience("Howling Wind");
        }
    }

    //For Frozen, Temperate, and Desert only
    private void SelectNightSkyboxAndAmbience(Planet planet, float intensity)
    {
        //Temperature dictates skybox and ambience choice...

        if (planet.biome == Planet.Biome.Frozen) //Cold
        {
            if (RenderSettings.fog)
                planet.LoadSkybox(false, "Cold Night", "Night Moon Burst");
            else
                planet.LoadSkybox(false, "SkyNight", "Cartoon Base NightSky", "BlueGreenNebular", "Blue Galaxy 1", "Blue Galaxy 2");

            //Ambience
            if (RenderSettings.fog && RenderSettings.fogDensity > 0.05f && intensity < 0.55f)
                planet.nightAmbience = planet.LoadAmbience("Blizzard");
            else
                planet.nightAmbience = planet.LoadAmbience("Howling Wind", "Light Winter Wind");
        }
        else if (planet.biome == Planet.Biome.Temperate) //Temperate
        {
            if (RenderSettings.fog)
                planet.LoadSkybox(false, "SkyEarlyDusk", "Cartoon Base NightSky");
            else
                planet.LoadSkybox(false, "SkyMidnight", "SkyNight", "SkyEarlyDusk", "Galaxy Field 1", "Galaxy Field 2");

            //Ambience (biome sub types and fog play a factor here)
            if (planet.biomeSubType == Planet.BiomeSubType.Forest)
                planet.nightAmbience = planet.LoadAmbience("Rainforest");
            else if (RenderSettings.fog)
                planet.nightAmbience = planet.LoadAmbience("Howling Wind");
            else
                planet.nightAmbience = planet.LoadAmbience("Summer Night");
        }
        else //Hot (don't worry about fog; impossible for hot planets)
        {
            if (Random.Range(0, 2) == 0)
                planet.LoadSkybox(false, "Deep Dusk");
            else
                planet.LoadSkybox(false, "Yellow Galaxy", "Spiral Galaxy");
            planet.nightAmbience = planet.LoadAmbience("Summer Night");
        }
    }

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