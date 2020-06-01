using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public enum TimeOfDay { Unknown, Morning, Day, Evening, Night }
    public enum Biome { Unknown, Frozen, Temperate, Desert, Swamp, Hell, Forest }
    public enum OceanType { Normal, Frozen, Lava }

    public static Planet planet;
    public static bool newPlanet = true;
    private static string jsonString = "";

    //General
    [HideInInspector] public string planetName = "";
    public Biome biome = Biome.Unknown;

    //Day/night cycle
    public Transform sun, moon;
    public float dayLength;
    private bool windingClocks = true; //Used as true on level load to skip to time of day indicated by time of day variable
    public float dayProgression = 0; //Time in seconds since the very beginning of the day
    private TimeOfDay timeOfDay; //Use this to figure out the time of day
    [HideInInspector] public Material daySkybox, nightSkybox;
    [HideInInspector] public AudioClip dayAmbience, nightAmbience;

    //Ocean
    public OceanType oceanType;
    public Transform oceanTransform;
    [HideInInspector] public bool hasOcean = true;
    public Material ice;

    //Audio
    [HideInInspector] public float ambientVolume = 1;
    private AudioSource ambientAudioSource;
    [HideInInspector] public AudioClip groundWalking, groundRunning, seabedWalking, seabedRunning;

    //Cities
    public GameObject cityPrefab;
    [HideInInspector] public List<City> cities;

    //START UP STUFF------------------------------------------------------------------------------------------------

    private void Awake () { planet = this; }

    private void Start ()
    {
        //Get references
        ambientAudioSource = GetComponents<AudioSource>()[1];

        //Make ambience paused when game is paused
        God.god.ManageAudioSource(ambientAudioSource);

        //Make/restore selections
        if (newPlanet)
            StartCoroutine(GeneratePlanet());
        else
            RestorePlanet();

        //ShowAreas(15, 5, 10);

        //Apply selections
        PaintSkyboxes();

        //Start coroutines
        StartCoroutine(DayNightCycle());
        StartCoroutine(ManagePlanetAmbience());
    }

    private IEnumerator GeneratePlanet ()
    {
        //FIRST, GENERATE SUN--------------------------------------------------------------------------------------

        //Day length
        //dayLength = Random.Range(60, 500);

        //Sun type (affects following environmental conditions...)
        SunType sunType = SunType.GetRandomType();

        //Lens flare for sun
        sun.GetComponent<Light>().flare = Resources.Load<Flare>("Planet/Lens Flares/" + sunType.flareName);

        //Sunlight color
        sun.GetComponent<Light>().color = sunType.sunlightColor;

        //Sunlight intensity
        sun.GetComponent<Light>().intensity = sunType.intensity;

        //THEN, USE SUN TO GENERATE BIOME--------------------------------------------------------------------------

        //Set up stuff for biomes
        SetOcean(-10, OceanType.Normal); //By default ocean is disabled (negative height disables ocean)

        SelectBiome(sunType.intensity);

        GenerateBiome(sunType.intensity, out TerrainCustomization terrainCustomization);

        //THEREAFTER, GENERATE TERRAIN-------------------------------------------------------------------------------

        yield return StartCoroutine(PlanetTerrain.planetTerrain.GenerateTerrain(terrainCustomization));

        //FINALLY, GIVE IT A FUCKING NAME AND SAVE THE BITCH---------------------------------------------------------

        planetName = GeneratePlanetName();

        //SavePlanet();
    }

    //BIOME FUNCTIONS------------------------------------------------------------------------------------------------

    private void SelectBiome (float intensity)
    {
        //Already set biome as test case
        if (biome != Biome.Unknown)
            return;

        //Chance for special biomes
        if (Random.Range(0, 3) == 0)
        {
            if (intensity > 1.075f) //Hot biomes
            {
                if (Random.Range(0, 2) == 0 || intensity > 1.3f)
                    biome = Biome.Hell;
                else
                    biome = Biome.Swamp;
            }
        }

        //Special biomes chance failed so just do normal biomes
        if (biome == Biome.Unknown)
        {
            if (intensity < 0.85f)
                biome = Biome.Frozen;
            else if (intensity < 1.15f)
                biome = Biome.Temperate;
            else
                biome = Biome.Desert;
        }
    }

    private void GenerateBiome (float intensity, out TerrainCustomization terrainCustomization)
    {
        terrainCustomization = new TerrainCustomization();

        if (biome == Biome.Frozen) //Frozen
        {
            biome = Biome.Frozen;

            //Terrain textures
            terrainCustomization.groundTexture = LoadTexture("Snow", "Snow 0087", "Snow 0096");
            terrainCustomization.cliffTexture = LoadTexture("Deep Freeze", "Snow 0126", "Age of the Canyon", "Glacier");
            terrainCustomization.seabedTexture = terrainCustomization.cliffTexture;

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 4) == 0;
            terrainCustomization.noiseGroundScale = Random.Range(30, 50);
            terrainCustomization.amplitudeGroundScale = Random.Range(5, 15);
            terrainCustomization.amplitudePower = Random.Range(2, 5);
            terrainCustomization.noiseStrength = Random.Range(0.35f, 0.75f);

            //Reverb
            GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Hangar;

            //Footsteps
            LoadGroundFootsteps("Snow");
            seabedWalking = groundWalking;
            seabedRunning = groundRunning;

            //Water
            if (Random.Range(0, 2) == 0)
            {
                if (Random.Range(0, intensity) < 0.35f)
                    SetOcean(Random.Range(1, 10), OceanType.Frozen);
                else
                {
                    SetOcean(Random.Range(1, 10), OceanType.Normal);
                    SetUnderwaterColor(new Color(0 / 255.0f, 27 / 255.0f, 108 / 255.0f, 0.5f));
                }
            }

            //Seabed height relative to water level
            terrainCustomization.seabedHeight = oceanTransform.position.y - Random.Range(1.5f, 3.0f);

            FinishNormalBiomeSetUp(intensity);
        }
        else if (biome == Biome.Temperate) //Temperate
        {
            biome = Biome.Temperate;

            //Terrain textures
            terrainCustomization.groundTexture = LoadTexture("Grass 0043", "Grass 0103", "Common Ground");
            terrainCustomization.cliffTexture = LoadTexture("Rock Grassy 0030", "Cliffs 0120", "Age of the Canyon");

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 2) == 0;
            terrainCustomization.noiseGroundScale = Random.Range(30, 50);
            terrainCustomization.amplitudeGroundScale = Random.Range(7, 10);
            terrainCustomization.amplitudePower = Random.Range(0, 4) != 0 ? 3 : Random.Range(2, 5);
            terrainCustomization.noiseStrength = Random.Range(0.4f, 0.6f);

            //Basic sound
            GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Plain;
            LoadGroundFootsteps("Grass");

            //Water
            SetUnderwaterColor(new Color(0 / 255.0f, 48 / 255.0f, 255 / 255.0f, 0.5f));
            if (Random.Range(0, 3) != 0)
            {
                //Set water level
                SetOcean(Random.Range(1, 10), OceanType.Normal);

                //Set seabed
                if (Random.Range(0, 3) != 0) //Beach
                {
                    terrainCustomization.seabedTexture = LoadTexture("Sahara", "Soil Beach 0052", "Soil Beach 0079");
                    LoadSeabedFootsteps("Swamp");

                    //Set seabed height to be above water
                    terrainCustomization.seabedHeight = oceanTransform.position.y + Random.Range(1.5f, 3.0f);
                }
                else //Rock shore
                {
                    terrainCustomization.seabedTexture = LoadTexture("Rock Grassy 0030", "Cliffs 0120", "Age of the Canyon");
                    LoadSeabedFootsteps("Rock");

                    //Set seabed height to be around water level
                    terrainCustomization.seabedHeight = oceanTransform.position.y + Random.Range(-1.5f, 1.5f);
                }
            }
            else //No water so make seabed appear like ground
            {
                terrainCustomization.seabedTexture = terrainCustomization.groundTexture;
                seabedWalking = groundWalking;
                seabedRunning = groundRunning;
            }

            //Trees
            if (Random.Range(0, 3) != 0)
            {
                if (hasOcean && Random.Range(0, 3) != 0) //Palm trees
                {
                    terrainCustomization.idealTreeCount = Random.Range(50, 200);
                    terrainCustomization.SetTreeNames("Palm Tree");
                }
                else if (Random.Range(0, 2) == 0) //Oak trees
                {
                    terrainCustomization.idealTreeCount = Random.Range(100, 250);
                    terrainCustomization.SetTreeNames("Oak Tree");
                }
                else //Jungle trees
                {
                    terrainCustomization.SetTreeNames("Jungle Tree");
                    terrainCustomization.maxTreeSteepness = 20;

                    if (Random.Range(0, 2) == 0) //Thick forest
                    {
                        biome = Biome.Forest;

                        terrainCustomization.groundTexture = LoadTexture("Common Ground");
                        terrainCustomization.cliffTexture = LoadTexture("Faulted Range");

                        if (!hasOcean)
                            terrainCustomization.seabedHeight = -10;

                        terrainCustomization.idealTreeCount = 450;
                    }
                    else //Sparse forest
                        terrainCustomization.idealTreeCount = Random.Range(100, 200);
                }
            }

            FinishNormalBiomeSetUp(intensity);
        }
        else if (biome == Biome.Desert) //Desert
        {
            biome = Biome.Desert;

            //Terrain textures
            terrainCustomization.groundTexture = LoadTexture("Sahara", "Soil Beach 0052", "Soil Beach 0079");
            terrainCustomization.cliffTexture = LoadTexture("Rocks Arid 0038", "Age of the Canyon");
            terrainCustomization.seabedTexture = terrainCustomization.groundTexture;

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
            terrainCustomization.noiseGroundScale = Random.Range(30, 50);
            terrainCustomization.amplitudeGroundScale = Random.Range(7, 10);
            terrainCustomization.amplitudePower = Random.Range(3, 5);
            terrainCustomization.noiseStrength = Random.Range(0.4f, 0.75f);

            //Reverb
            GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Mountains;

            //Footsteps
            LoadGroundFootsteps("Swamp");
            seabedWalking = groundWalking;
            seabedRunning = groundRunning;

            //Water
            SetUnderwaterColor(new Color(0 / 255.0f, 70 / 255.0f, 115 / 255.0f, 0.5f));
            if (Random.Range(0, 2) == 0)
                SetOcean(Random.Range(1, 7), OceanType.Normal);

            FinishNormalBiomeSetUp(intensity);
        }
        else if (biome == Biome.Swamp) //Swamp
        {
            terrainCustomization.groundTexture = LoadTexture("Sulfur Wasteland");
            terrainCustomization.cliffTexture = LoadTexture("Rock Grassy 0030");
            terrainCustomization.seabedTexture = LoadTexture("Egg Veins", "Carburetor Resin");

            if (Random.Range(0, 2) == 0)
                LoadSkybox(true, "SkyMorning");
            else
                LoadSkybox(true, "SkySunset");
            LoadSkybox(false, "SkyEarlyDusk");

            dayAmbience = LoadAmbience("Dank Swamp");
            nightAmbience = dayAmbience;

            RenderSettings.fog = true;
            RenderSettings.fogDensity = Random.Range(0.01f, 0.03f);
            RenderSettings.fogColor = SunType.GetColorRGB(108, 100, 4);

            sun.GetComponent<Light>().color = SunType.GetColorRGB(185, 145, 0);

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 4) != 0;
            terrainCustomization.amplitudePower = 4;
            terrainCustomization.noiseStrength = Random.Range(0.55f, 0.75f);

            GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Alley;

            LoadGroundFootsteps("Swamp");
            seabedWalking = groundWalking;
            seabedRunning = groundRunning;

            //Change water color
            SetUnderwaterColor(new Color(72 / 255.0f, 108 / 255.0f, 39 / 255.0f, 0.5f));

            SetOcean(Random.Range(7, 15), OceanType.Normal);

            //Seabed height relative to water level
            terrainCustomization.seabedHeight = oceanTransform.position.y - Random.Range(1.5f, 3.0f);

            //Trees
            terrainCustomization.idealTreeCount = Random.Range(250, 450);
            terrainCustomization.SetTreeNames("Jungle Tree");
            terrainCustomization.maxTreeSteepness = 20;
        }
        else if (biome == Biome.Hell) //Hell
        {
            terrainCustomization.groundTexture = LoadTexture("Searing Gorge");
            terrainCustomization.cliffTexture = LoadTexture("Slumbering Volcano");
            terrainCustomization.seabedTexture = LoadTexture("Noxious Melt");

            LoadSkybox(true, "AllSky_Space_AnotherPlanet", "RedYellowNebular", "RedOrangeYellowNebular");
            LoadSkybox(false, daySkybox.name);

            dayAmbience = LoadAmbience("Large Eerie Reverberant Space");
            nightAmbience = dayAmbience;

            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.0035f;
            RenderSettings.fogColor = SunType.GetColorRGB(67, 52, 52);

            sun.GetComponent<Light>().intensity = Random.Range(1.3f, 1.5f);
            sun.GetComponent<Light>().color = SunType.GetColorRGB(255, 76, 0);

            //Terrain heightmap
            terrainCustomization.lowBoundaries = Random.Range(0, 2) == 0;
            terrainCustomization.amplitudePower = 5;
            terrainCustomization.amplitudeGroundScale = Random.Range(2, 5);
            terrainCustomization.noiseGroundScale = Random.Range(25, 35);
            terrainCustomization.noiseStrength = Random.Range(1.15f, 1.35f);

            GetComponent<AudioReverbZone>().reverbPreset = AudioReverbPreset.Arena;

            LoadGroundFootsteps("Rock");
            seabedWalking = groundWalking;
            seabedRunning = groundRunning;

            SetUnderwaterColor(new Color(255 / 255.0f, 78 / 255.0f, 0 / 255.0f, 0.5f));
            SetOcean(Random.Range(7, 15), OceanType.Lava);

            //Rock is molten just at very edge of lava
            terrainCustomization.seabedHeight = oceanTransform.position.y + Random.Range(0.25f, 0.5f);
        }
    }

    private void FinishNormalBiomeSetUp (float intensity)
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
        SelectDaySkyboxAndAmbience(intensity);
        SelectNightSkyboxAndAmbience(intensity);
    }

    private void SelectDaySkyboxAndAmbience (float intensity)
    {
        //If there's fog, fog dominates skybox and ambience choice
        if (RenderSettings.fog)
        {
            LoadSkybox(true, "SkyCloudy", "SkyHaloSky", "AllSky_Overcast4_Low");
            dayAmbience = LoadAmbience("Howling Wind");

            return;
        }

        //Otherwise, temperature dictates skybox and ambience choice...

        if (biome == Biome.Frozen) //Cold
        {
            LoadSkybox(true, "Cold Sunset", "AllSky_Overcast4_Low");
            dayAmbience = LoadAmbience("Light Winter Wind");
        }
        else if (biome == Biome.Temperate || biome == Biome.Forest) //Temperate
        {
            LoadSkybox(true, "Epic_GloriousPink", "Epic_BlueSunset", "SkyBrightMorning", "Day_BlueSky_Nothing");

            if (biome == Biome.Forest)
                dayAmbience = LoadAmbience("Rainforest");
            else
                dayAmbience = LoadAmbience("Morning Country Birds", "Quiet Lake with Birds");
        }
        else //Hot
        {
            LoadSkybox(true, "SkySunset", "SkyMorning", "Desert Sky Morning");
            dayAmbience = LoadAmbience("Howling Wind");
        }
    }

    private void SelectNightSkyboxAndAmbience (float intensity)
    {
        //Temperature dictates skybox and ambience choice...

        if (biome == Biome.Frozen) //Cold
        {
            LoadSkybox(false, "Cold Night", "Night Moon Burst", "SkyNight", "Cartoon Base NightSky", "BlueGreenNebular");

            //Ambience
            if (RenderSettings.fog && RenderSettings.fogDensity > 0.05f && intensity < 0.55f)
                nightAmbience = LoadAmbience("Blizzard");
            else
                nightAmbience = LoadAmbience("Howling Wind", "Light Winter Wind");
        }
        else if (biome == Biome.Temperate || biome == Biome.Forest) //Temperate
        {
            LoadSkybox(false, "SkyMidnight", "SkyNight", "SkyEarlyDusk", "Cartoon Base NightSky", "BlueGreenNebular");

            //Ambience (biome and fog play a factor here)
            if (biome == Biome.Forest)
                nightAmbience = LoadAmbience("Rainforest");
            else if(RenderSettings.fog)
                nightAmbience = LoadAmbience("Howling Wind");
            else
                nightAmbience = LoadAmbience("Summer Night");
        }
        else //Hot (don't worry about fog; impossible for hot planets)
        {
            LoadSkybox(false, "Deep Dusk");
            nightAmbience = LoadAmbience("Summer Night");
        }
    }

    public void LoadSkybox (bool daySkybox, params string[] skyboxNames)
    {
        if(daySkybox)
        {
            string skyboxName = skyboxNames[Random.Range(0, skyboxNames.Length)];

            this.daySkybox = Resources.Load<Material>("Planet/Skyboxes/" + skyboxName);

            //Set water cubemap based on daytime skybox
            string cubemapName;
            switch (skyboxName)
            {
                case "AllSky_Overcast4_Low":
                case "SkyCloudy":
                case "SkyHaloSky":
                case "SkyMorning":
                case "SkySunset":
                    cubemapName = "AllSky_Overcast4_Low"; break;
                case "Cold Sunset":
                    cubemapName = "Cold Sunset Equirect"; break;
                case "Epic_BlueSunset":
                    cubemapName = "Epic_BlueSunset_EquiRect_flat"; break;
                case "Epic_GloriousPink":
                    cubemapName = "Epic_GloriousPink_EquiRect"; break;
                default:
                    cubemapName = "Sky_Day_BlueSky_Equirect"; break;
            }
            LoadWaterCubemap(cubemapName);
        }
        else
            nightSkybox = Resources.Load<Material>("Planet/Skyboxes/" + skyboxNames[Random.Range(0, skyboxNames.Length)]);
    }

    private AudioClip LoadAmbience (params string[] clipNames)
    {
        return Resources.Load<AudioClip>("Planet/Ambience/" + clipNames[Random.Range(0, clipNames.Length)]);
    }

    private Texture2D LoadTexture (params string[] textureNames)
    {
        return Resources.Load<Texture2D>("Planet/Terrain Textures/" + textureNames[Random.Range(0, textureNames.Length)]);
    }

    private void LoadGroundFootsteps (string stepType)
    {
        groundWalking = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + stepType + " Walking");
        groundRunning = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + stepType + " Running");
    }

    private void LoadSeabedFootsteps (string stepType)
    {
        seabedWalking = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + stepType + " Walking");
        seabedRunning = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + stepType + " Running");
    }

    public void SetUnderwaterColor (Color underwaterColor)
    {
        God.god.HUD.Find("Underwater").GetComponent<Image>().color = underwaterColor;
    }

    private void LoadWaterCubemap (string mapName)
    {
        oceanTransform.GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", Resources.Load<Cubemap>("Planet/Cubemaps/" + mapName));
    }

    public void SetOcean (int height, OceanType oceanType, string iceTexture = "")
    {
        this.oceanType = oceanType;

        //Set height
        Vector3 newPosition = oceanTransform.position;
        newPosition.y = height;
        oceanTransform.position = newPosition;

        //Set enabled
        hasOcean = oceanTransform.position.y > 0;
        oceanTransform.gameObject.SetActive(hasOcean);

        //Set type... default is OceanType.Normal
        if (oceanType == OceanType.Frozen)
        {
            if(iceTexture.Equals("")) //Generate new ice texture
                FreezeOcean("Glacier", "Ice 0041", "Ice Cracked", "Ice Caves");
            else //Load old ice texture
                FreezeOcean(iceTexture);
        }
        else if(oceanType == OceanType.Lava)
        {
            oceanTransform.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Planet/Misc/Lava");
        }
    }

    private void FreezeOcean (params string[] textureNames)
    {
        //Solidify
        oceanTransform.GetComponent<Collider>().isTrigger = false;

        //Set sound
        oceanTransform.tag = "Untagged";

        //Set visuals
        Texture2D iceTexture = Resources.Load<Texture2D>("Planet/Terrain Textures/" + textureNames[Random.Range(0, textureNames.Length)]);
        ice.SetTexture("_MainTex", iceTexture);
        oceanTransform.GetComponent<Renderer>().sharedMaterial = ice;

        //Otherwise ocean will be animated like it is moving water
        hasOcean = false;
    }

    //Applies the skyboxes selected (only needs to be done once at beginning of level after selections made)
    private void PaintSkyboxes ()
    {
        //Set up day skybox
        RenderSettings.skybox.SetTexture("_FrontTex", daySkybox.GetTexture("_FrontTex"));
        RenderSettings.skybox.SetTexture("_BackTex", daySkybox.GetTexture("_BackTex"));
        RenderSettings.skybox.SetTexture("_LeftTex", daySkybox.GetTexture("_LeftTex"));
        RenderSettings.skybox.SetTexture("_RightTex", daySkybox.GetTexture("_RightTex"));
        RenderSettings.skybox.SetTexture("_UpTex", daySkybox.GetTexture("_UpTex"));
        RenderSettings.skybox.SetTexture("_DownTex", daySkybox.GetTexture("_DownTex"));

        //Set up night skybox
        RenderSettings.skybox.SetTexture("_FrontTex2", nightSkybox.GetTexture("_FrontTex"));
        RenderSettings.skybox.SetTexture("_BackTex2", nightSkybox.GetTexture("_BackTex"));
        RenderSettings.skybox.SetTexture("_LeftTex2", nightSkybox.GetTexture("_LeftTex"));
        RenderSettings.skybox.SetTexture("_RightTex2", nightSkybox.GetTexture("_RightTex"));
        RenderSettings.skybox.SetTexture("_UpTex2", nightSkybox.GetTexture("_UpTex"));
        RenderSettings.skybox.SetTexture("_DownTex2", nightSkybox.GetTexture("_DownTex"));

        //Update shiny materials to reflect new skybox
        DynamicGI.UpdateEnvironment();
    }

    //CITY MANAGEMENT FUNCTIONS------------------------------------------------------------------------------------

    public void GenerateCities (PlanetJSON savedPlanet)
    {
        if(savedPlanet != null) //Restore/regenerate cities
        {
            cities = new List<City>(savedPlanet.cities.Count);
            for (int x = 0; x < savedPlanet.cities.Count; x++)
            {
                City newCity = Instantiate(cityPrefab, Vector3.zero, Quaternion.identity).GetComponent<City>();
                savedPlanet.cities[x].RestoreCity(newCity);
                cities.Add(newCity);
            }
        }
        else //Generate new cities
        {
            cities = new List<City>();

            for (int cityCount = 0; cityCount < 2; cityCount++)
            {
                City newCity = Instantiate(cityPrefab, Vector3.zero, Quaternion.identity).GetComponent<City>();
                newCity.GenerateCity();
                cities.Add(newCity);
            }
        }
    }

    //PLANET MANAGEMENT FUNCTIONS----------------------------------------------------------------------------------

    private IEnumerator DayNightCycle ()
    {
        //Must have sun and positive day length to work
        if (!sun || dayLength <= 0)
            yield break;

        Vector3 sunRotation = sun.eulerAngles;

        //Fog
        bool fog = RenderSettings.fog;
        Color dayFogColor = RenderSettings.fogColor;
        Color nightFogColor = dayFogColor / 3;

        //Ocean
        Material ocean = oceanTransform.GetComponent<Renderer>().sharedMaterial;
        Color dayOceanColor = new Color(48 / 255.0f, 60 / 255.0f, 67 / 255.0f);
        Color nightOceanColor = new Color(8 / 255.0f, 8 / 255.0f, 8 / 255.0f);
        Vector2 oceanOffset = Vector2.zero;
        float oceanAnimationSpeed = 0.025f; //Units of movement per second

        if(biome == Biome.Hell)
        {
            dayOceanColor = new Color(70 / 255.0f, 27 / 255.0f, 0 / 255.0f);
            nightOceanColor = Color.black;
        }

        //Till' the sun rises in the east and sets in the west
        while (true)
        {
            if(!windingClocks)
                dayProgression = 0;

            windingClocks = false;

            //Morning---------------------------------------------------------------------------------------------
            timeOfDay = TimeOfDay.Morning;
            for (; dayProgression < dayLength / 3; dayProgression += Time.deltaTime)
            {
                //Rotate sun
                sunRotation.x = Mathf.Lerp(-30, 30, dayProgression * 3 / dayLength);
                sun.eulerAngles = sunRotation;

                //Rotate moon
                if (moon)
                {
                    moon.rotation = sun.rotation;
                    sun.Rotate(180, 0.0f, 0.0f, Space.World);
                }

                //Transition from night skybox (1) to day skybox (0)
                RenderSettings.skybox.SetFloat("_Blend", 1 - (dayProgression * 3 / dayLength));

                //Transition fog to daytime color
                if (fog)
                    RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, dayProgression * 3 / dayLength);

                if (hasOcean)
                {
                    //Transition ocean to daytime color
                    ocean.SetColor("_ReflectColor", Color.Lerp(nightOceanColor, dayOceanColor, dayProgression * 3 / dayLength));

                    //Animate water
                    oceanOffset.x += oceanAnimationSpeed * Time.deltaTime;
                    oceanOffset.y += oceanAnimationSpeed * Time.deltaTime;
                    ocean.SetTextureOffset("_MainTex", oceanOffset);
                }

                //Wait a frame
                yield return null;
            }

            //Finalize transition from night skybox to day skybox
            RenderSettings.skybox.SetFloat("_Blend", 0.0f);

            //Finalize transition to daytime fog
            if(fog)
                RenderSettings.fogColor = dayFogColor;

            //Finalize transition to daytime ocean
            if (hasOcean)
                ocean.SetColor("_ReflectColor", dayOceanColor);

            //Day---------------------------------------------------------------------------------------------
            timeOfDay = TimeOfDay.Day;
            for (; dayProgression < dayLength; dayProgression += Time.deltaTime)
            {
                //Rotate sun
                sunRotation.x = Mathf.Lerp(-30, 330, dayProgression * 0.5f / dayLength);
                sun.eulerAngles = sunRotation;

                //Rotate moon
                if (moon)
                {
                    moon.rotation = sun.rotation;
                    sun.Rotate(180, 0.0f, 0.0f, Space.World);
                }

                if (hasOcean)
                {
                    //Animate water
                    oceanOffset.x += oceanAnimationSpeed * Time.deltaTime;
                    oceanOffset.y += oceanAnimationSpeed * Time.deltaTime;
                    ocean.SetTextureOffset("_MainTex", oceanOffset);
                }

                //Wait a frame
                yield return null;
            }

            //Evening---------------------------------------------------------------------------------------------
            timeOfDay = TimeOfDay.Evening;
            float t = 0;
            for (; dayProgression < dayLength * 4 / 3; dayProgression += Time.deltaTime)
            {
                //Rotate sun
                sunRotation.x = Mathf.Lerp(-30, 330, dayProgression * 0.5f / dayLength);
                sun.eulerAngles = sunRotation;

                //Rotate moon
                if (moon)
                {
                    moon.rotation = sun.rotation;
                    sun.Rotate(180, 0.0f, 0.0f, Space.World);
                }

                //Transition from day skybox (0) to night skybox (1)
                t = dayProgression - dayLength;
                RenderSettings.skybox.SetFloat("_Blend", t * 3 / dayLength);

                //Transition fog to nighttime color
                if (fog)
                    RenderSettings.fogColor = Color.Lerp(dayFogColor, nightFogColor, t * 3 / dayLength);

                if (hasOcean)
                {
                    //Transition ocean to nighttime color
                    ocean.SetColor("_ReflectColor", Color.Lerp(dayOceanColor, nightOceanColor, t * 3 / dayLength));

                    //Animate water
                    oceanOffset.x += oceanAnimationSpeed * Time.deltaTime;
                    oceanOffset.y += oceanAnimationSpeed * Time.deltaTime;
                    ocean.SetTextureOffset("_MainTex", oceanOffset);
                }

                //Wait a frame
                yield return null;
            }

            //Finalize transition from day skybox to night skybox
            RenderSettings.skybox.SetFloat("_Blend", 1.0f);

            //Finalize transition to nighttime fog
            if(fog)
                RenderSettings.fogColor = nightFogColor;

            //Finalize transition to nighttime ocean
            if (hasOcean)
                ocean.SetColor("_ReflectColor", nightOceanColor);

            //Night---------------------------------------------------------------------------------------------
            timeOfDay = TimeOfDay.Night;
            for (; dayProgression < dayLength * 2; dayProgression += Time.deltaTime)
            {
                //Rotate sun
                sunRotation.x = Mathf.Lerp(-30, 330, dayProgression * 0.5f / dayLength);
                sun.eulerAngles = sunRotation;

                //Rotate moon
                if (moon)
                {
                    moon.rotation = sun.rotation;
                    sun.Rotate(180, 0.0f, 0.0f, Space.World);
                }

                if (hasOcean)
                {
                    //Animate water
                    oceanOffset.x += oceanAnimationSpeed * Time.deltaTime;
                    oceanOffset.y += oceanAnimationSpeed * Time.deltaTime;
                    ocean.SetTextureOffset("_MainTex", oceanOffset);
                }

                //Wait a frame
                yield return null;
            }
        }
    }

    public TimeOfDay GetTimeOfDay () { return timeOfDay; }

    private IEnumerator ManagePlanetAmbience ()
    {
        while (true)
        {
            float phaseProgression = 0;
            float phaseStart = 0;

            //Just to eliminate any possibility of looping forever
            yield return new WaitForSeconds(0.1f);

            //Get it started on level load
            if (!ambientAudioSource.isPlaying)
            {
                ambientAudioSource.clip = nightAmbience;
                ambientAudioSource.Play();
            }

            //Early morning (fade out night time ambience)
            while (dayProgression < dayLength / 6)
            {
                UpdateAmbientVolume(1 - (dayProgression * 6 / dayLength));
                yield return new WaitForSeconds(0.1f);
            }

            //Switch to day time ambience
            ambientAudioSource.Stop();
            ambientAudioSource.clip = dayAmbience;
            ambientAudioSource.Play();

            //Late morning (fade in daytime ambience)
            phaseStart = dayLength / 6;
            while (dayProgression < dayLength / 3)
            {
                phaseProgression = dayProgression - phaseStart;
                UpdateAmbientVolume(phaseProgression * 6 / dayLength);
                yield return new WaitForSeconds(0.1f);
            }

            //Day
            while (timeOfDay == TimeOfDay.Day)
            {
                UpdateAmbientVolume(1);
                yield return new WaitForSeconds(0.1f);
            }

            //Early evening (fade out daytime ambience)
            phaseStart = dayLength;
            while (dayProgression < dayLength * 7 / 6)
            {
                phaseProgression = dayProgression - phaseStart;
                UpdateAmbientVolume(1 - (phaseProgression * 6 / dayLength));
                yield return new WaitForSeconds(0.1f);
            }

            //Switch to night time ambience
            ambientAudioSource.Stop();
            ambientAudioSource.clip = nightAmbience;
            ambientAudioSource.Play();

            //Late evening (fade in night time ambience)
            phaseStart = dayLength * 7 / 6;
            while (dayProgression < dayLength * 4 / 3)
            {
                phaseProgression = dayProgression - phaseStart;
                UpdateAmbientVolume(phaseProgression * 6 / dayLength);
                yield return new WaitForSeconds(0.1f);
            }

            //Night
            while (timeOfDay == TimeOfDay.Night)
            {
                UpdateAmbientVolume(1);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void UpdateAmbientVolume (float internalAmbientVolume)
    {
        ambientAudioSource.volume = internalAmbientVolume * ambientVolume;
    }

    private string GeneratePlanetName ()
    {
        string planetName = "";

        if (Random.Range(0, 5) != 0) //Normal random name
        {
            //Get list of planet names
            TextAsset planetNamesFile = Resources.Load<TextAsset>("Text/Planet Names");
            string[] planetNames = planetNamesFile.text.Split('\n');

            //Pick a random name
            planetName = planetNames[Random.Range(0, planetNames.Length)];
        }
        else if (Random.Range(0, 2) == 0) //Greek letter + astrological/zodiac/birth sign name
        {
            string[] greekLetters = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega"};

            string[] zodiacSigns = new string[] { "Carinae", "Tauri", "Pegasi", "Centauri", "Scuti", "Orionis", "Scorpius",
            "Geminorum"};

            planetName = greekLetters[Random.Range(0, greekLetters.Length)] + " "
                + zodiacSigns[Random.Range(0, zodiacSigns.Length)];
        }
        else //Some prefix + major or minor name
        {
            string[] prefixes = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Rho", "Omikron", "Zeta",
            "Sigma", "Omega", "Ursa", "Virgo", "Canis", "Pisces", "Saega", "Polis"};

            if (Random.Range(0, 2) == 0)
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Major";
            else
                planetName = prefixes[Random.Range(0, prefixes.Length)] + " Minor";
        }

        //Add roman numeral onto end for variation
        if (Random.Range(0, 5) == 0)
        {
            string[] numerals = new string[] { "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };
            planetName += " " + numerals[Random.Range(0, numerals.Length)];
        }

        return planetName;
    }

    //SAVE FUNCTIONS-----------------------------------------------------------------------------------------------

    public void SavePlanet ()
    {
        PlanetJSON savedPlanet = new PlanetJSON(this);
        jsonString = JsonUtility.ToJson(savedPlanet, true);
    }

    private void RestorePlanet ()
    {
        PlanetJSON savedPlanet = JsonUtility.FromJson<PlanetJSON>(jsonString);
        savedPlanet.RestorePlanet(this);
    }
}

[System.Serializable]
public class PlanetJSON
{
    public PlanetJSON (Planet planet)
    {
        //General
        planetName = planet.planetName;
        biome = planet.biome;

        //Sun
        sunFlare = planet.sun.GetComponent<Light>().flare.name;
        sunColor = planet.sun.GetComponent<Light>().color;
        sunIntensity = planet.sun.GetComponent<Light>().intensity;

        //Ocean
        oceanHeight = planet.oceanTransform.position.y;
        oceanType = planet.oceanType;
        underwaterColor = God.god.HUD.Find("Underwater").GetComponent<Image>().color;
        iceTexture = planet.oceanTransform.GetComponent<Renderer>().sharedMaterial.mainTexture.name;

        //Audio
        reverb = planet.GetComponent<AudioReverbZone>().reverbPreset;
        dayAmbience = planet.dayAmbience.name;
        nightAmbience = planet.nightAmbience.name;
        groundWalking = planet.groundWalking.name;
        groundRunning = planet.groundRunning.name;
        seabedWalking = planet.seabedWalking.name;
        seabedRunning = planet.seabedRunning.name;

        //Fog
        fog = RenderSettings.fog;
        fogDensity = RenderSettings.fogDensity;
        fogColor = RenderSettings.fogColor;

        //Skyboxes
        daySkybox = planet.daySkybox.name;
        nightSkybox = planet.nightSkybox.name;

        //Terrain
        planetTerrain = new PlanetTerrainJSON(PlanetTerrain.planetTerrain);

        //Cities
        cities = new List<CityJSON>(planet.cities.Count);
        for (int x = 0; x < planet.cities.Count; x++)
            cities.Add(new CityJSON(planet.cities[x]));
    }

    public void RestorePlanet (Planet planet)
    {
        //General
        planet.planetName = planetName;
        planet.biome = biome;

        //Sun
        planet.sun.GetComponent<Light>().flare = Resources.Load<Flare>("Planet/Lens Flares/" + sunFlare);
        planet.sun.GetComponent<Light>().color = sunColor;
        planet.sun.GetComponent<Light>().intensity = sunIntensity;

        //Ocean
        planet.SetOcean((int)oceanHeight, oceanType, iceTexture);
        planet.SetUnderwaterColor(underwaterColor);

        //Audio
        planet.GetComponent<AudioReverbZone>().reverbPreset = reverb;
        planet.dayAmbience = Resources.Load<AudioClip>("Planet/Ambience/" + dayAmbience);
        planet.nightAmbience = Resources.Load<AudioClip>("Planet/Ambience/" + nightAmbience);
        planet.groundWalking = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + groundWalking);
        planet.groundRunning = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + groundRunning);
        planet.seabedWalking = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + seabedWalking);
        planet.seabedRunning = Resources.Load<AudioClip>("Planet/Terrain Footsteps/" + seabedRunning);

        //Fog
        RenderSettings.fog = fog;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;

        //Skyboxes
        planet.LoadSkybox(true, daySkybox);
        planet.LoadSkybox(false, nightSkybox);

        //Terrain (also implicitly regenerates the cities)
        planetTerrain.RestorePlanetTerrain(PlanetTerrain.planetTerrain, this);
    }

    //General
    public string planetName;
    public Planet.Biome biome;

    //Sun
    public string sunFlare;
    public Color sunColor;
    public float sunIntensity;

    //Ocean
    public float oceanHeight;
    public Planet.OceanType oceanType;
    public Color underwaterColor;
    public string iceTexture;

    //Audio
    public AudioReverbPreset reverb;
    public string dayAmbience, nightAmbience;
    public string groundWalking, groundRunning, seabedWalking, seabedRunning;

    //Fog
    public bool fog;
    public float fogDensity;
    public Color fogColor;

    //Skyboxes
    public string daySkybox, nightSkybox;

    //Terrain
    public PlanetTerrainJSON planetTerrain;

    //Cities
    public List<CityJSON> cities;
}

class SunType
{
    public string flareName;
    public Color sunlightColor;
    public float intensity; //Measure of coldness (1 = normal temp, 0.5 = frigid, 1.5 = blazing)

    public SunType (string flareName, Color sunlightColor)
    {
        this.flareName = flareName;
        this.sunlightColor = sunlightColor;

        //Calculates intensity based on ratio of red (hot) to blue (cold)
        intensity = Mathf.Max(0.5f + sunlightColor.r - sunlightColor.b / 2.0f, 0.25f);
    }

    public static SunType GetRandomType ()
    {
        string flareName = "";
        Color sunlightColor = Color.magenta;
        int picker = Random.Range(0, 16);

        switch (picker)
        {
            case 0:
                flareName = "6 Blade Aperture";
                sunlightColor = GetRandomColor();
                break;
            case 1:
                flareName = "35mm Lens";
                sunlightColor = GetColorRGB(203, 237, 255);
                break;
            case 2:
                flareName = "85mm Lens";
                sunlightColor = GetRandomColor();
                break;
            case 3:
                flareName = "Cheap Plastic Lens";
                sunlightColor = GetColorRGB(255, 255, 255);
                break;
            case 4:
                flareName = "Cold Clear Sun";
                sunlightColor = GetColorRGB(255, 255, 255);
                break;
            case 5:
                flareName = "Concert";
                sunlightColor = GetRandomColor();
                break;
            case 6:
                flareName = "Digicam Lens";
                sunlightColor = GetColorRGB(255, 252, 223);
                break;
            case 7:
                flareName = "Digital Camera";
                sunlightColor = GetRandomColor();
                break;
            case 8:
                flareName = "Halogen Bulb";
                sunlightColor = GetColorRGB(105, 184, 255);
                break;
            case 9:
                flareName = "Laser";
                sunlightColor = GetRandomColor();
                break;
            case 10:
                flareName = "Subtle1";
                sunlightColor = GetRandomColor();
                break;
            case 11:
                flareName = "Subtle2";
                sunlightColor = GetRandomColor();
                break;
            case 12:
                flareName = "Subtle3";
                sunlightColor = GetRandomColor();
                break;
            case 13:
                flareName = "Subtle4";
                sunlightColor = GetRandomColor();
                break;
            case 14:
                flareName = "Sun (from space)";
                sunlightColor = GetColorRGB(255, 255, 255);
                break;
            default:
                flareName = "Welding";
                sunlightColor = GetColorRGB(114, 218, 255);
                break;
        }

        return new SunType(flareName, sunlightColor);
    }

    //Below are a bunch of helper functions...

    //Converts 0-255 (human readable) to 0-1 (unity format)
    public static Color GetColorRGB (int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    //Converts 0-360/0-100/0-100 (human readable) to 0-1 (unity format)
    private static Color GetColorHSV (int h, int s, int v)
    {
        return Color.HSVToRGB(h / 360.0f, s / 100.0f, v / 100.0f);
    }

    private static Color GetRandomColor ()
    {
        if (Random.Range(0, 3) == 0) //White
            return Color.white;
        else if (Random.Range(0, 5) == 0) //Whitish yellow
            return GetColorRGB(255, 255, Random.Range(200, 256));
        else if (Random.Range(0, 3) == 0) //Hot
            return GetColorHSV(Random.Range(0, 60), Random.Range(25, 60), 100);
        else if (Random.Range(0, 5) != 0) //Random light tint
            return GetColorHSV(Random.Range(0, 361), Random.Range(0, 30), 100);
        else //Random harsh tint
            return GetColorHSV(Random.Range(0, 361), Random.Range(30, 101), 100);
    }
}