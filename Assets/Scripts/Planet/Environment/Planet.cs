using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public enum TimeOfDay { Unknown, Morning, Day, Evening, Night }
    public enum Biome { Unknown, Frozen, Temperate, SandyDesert, RockyDesert, Swamp, Hell, Spirit }
    public enum BiomeSubType { Default, Forest }
    public enum OceanType { Normal, Frozen, Lava, Glowing, Murky }

    public static Planet planet;
    public static bool newPlanet = true;
    private static string jsonString = "";

    //General
    [HideInInspector] public string planetName = "";
    public Biome biome = Biome.Unknown;
    [HideInInspector] public BiomeSubType biomeSubType = BiomeSubType.Default;
    public PlanetGenerator generator;

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

    //Special effects
    public GameObject firePrefab;
    public ReflectionProbe skyboxReflectionProbe;

    //START UP STUFF------------------------------------------------------------------------------------------------

    private void Awake () { planet = this; }

    private void Start () { StartCoroutine(StartUpPlanet()); }

    private IEnumerator StartUpPlanet ()
    {
        //Get references
        ambientAudioSource = GetComponent<AudioSource>();

        //Make ambience paused when game is paused
        God.god.ManageAudioSource(ambientAudioSource);

        //Make/restore selections
        if (newPlanet)
            yield return StartCoroutine(generator.GeneratePlanet(this));
        else
            RestorePlanet();

        //ShowAreas(15, 5, 10);

        //Apply selections
        PaintSkyboxes();
        PlanetPauseMenu.pauseMenu.UpdatePlanetName(planetName);

        //Start coroutines
        StartCoroutine(DayNightCycle());
        StartCoroutine(ManagePlanetAmbience());
    }

    //BIOME FUNCTIONS------------------------------------------------------------------------------------------------

    public void LoadSkybox (bool daySkybox, params string[] skyboxNames)
    {
        if(daySkybox)
        {
            string skyboxName = skyboxNames[Random.Range(0, skyboxNames.Length)];

            this.daySkybox = Resources.Load<Material>("Planet/Environment/Skyboxes/" + skyboxName);

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
                case "AllSky_Space_AnotherPlanet":
                    cubemapName = "AllSky_Space_AnotherPlanet Equirect"; break;
                case "Deep Dusk":
                case "Brown Cloud":
                    cubemapName = "Deep Dusk Equirect"; break;
                default:
                    cubemapName = "Sky_Day_BlueSky_Equirect"; break;
            }
            LoadWaterCubemap(cubemapName);
        }
        else
            nightSkybox = Resources.Load<Material>("Planet/Environment/Skyboxes/" + skyboxNames[Random.Range(0, skyboxNames.Length)]);
    }

    public AudioClip LoadAmbience (params string[] clipNames)
    {
        return Resources.Load<AudioClip>("Planet/Environment/Ambience/" + clipNames[Random.Range(0, clipNames.Length)]);
    }

    public Texture2D LoadTexture (params string[] textureNames)
    {
        return Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + textureNames[Random.Range(0, textureNames.Length)]);
    }

    public void LoadGroundFootsteps (string stepType)
    {
        groundWalking = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + stepType + " Walking");
        groundRunning = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + stepType + " Running");
    }

    public void LoadSeabedFootsteps (string stepType)
    {
        seabedWalking = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + stepType + " Walking");
        seabedRunning = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + stepType + " Running");
    }

    public void SetUnderwaterColor (Color underwaterColor)
    {
        PlanetPauseMenu.pauseMenu.HUD.Find("Underwater").GetComponent<Image>().color = underwaterColor;
    }

    private void LoadWaterCubemap (string mapName)
    {
        oceanTransform.GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", Resources.Load<Cubemap>("Planet/Environment/Cubemaps/" + mapName));
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
                FreezeOcean("Glacier", "Ice 0041", "Ice Cracked", "Ice Caves", "Chiseled Ice");
            else //Load old ice texture
                FreezeOcean(iceTexture);
        }
        else if(oceanType == OceanType.Lava)
            oceanTransform.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Planet/Environment/Ocean/Lava");
        else if (oceanType == OceanType.Glowing)
            oceanTransform.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Planet/Environment/Ocean/Glowing Water");
        else if(oceanType == OceanType.Murky)
            oceanTransform.GetComponent<Renderer>().sharedMaterial = Resources.Load<Material>("Planet/Environment/Ocean/Murky Water");
    }

    private void FreezeOcean (params string[] textureNames)
    {
        //Solidify
        oceanTransform.GetComponent<Collider>().isTrigger = false;

        //Set sound
        oceanTransform.tag = "Ice";

        //Set visuals
        Texture2D iceTexture = Resources.Load<Texture2D>("Planet/Environment/Terrain Textures/" + textureNames[Random.Range(0, textureNames.Length)]);
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

        UpdateSkyboxReflectiveProbe();
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

            for (int cityCount = 0; cityCount < 1; cityCount++)
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

    public void UpdateSkyboxReflectiveProbe()
    {
        //Update reflective materials to reflect new skybox
        if (skyboxReflectionProbe)
            skyboxReflectionProbe.RenderProbe();
        else
            Debug.LogWarning("Skybox reflection probe not set. Reflective materials will not look right.");

        //DynamicGI.UpdateEnvironment();
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
        biomeSubType = planet.biomeSubType;

        //Sun
        sunFlare = planet.sun.GetComponent<Light>().flare.name;
        sunColor = planet.sun.GetComponent<Light>().color;
        sunIntensity = planet.sun.GetComponent<Light>().intensity;

        //Ocean
        oceanHeight = planet.oceanTransform.position.y;
        oceanType = planet.oceanType;
        underwaterColor = PlanetPauseMenu.pauseMenu.HUD.Find("Underwater").GetComponent<Image>().color;
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
        planet.biomeSubType = biomeSubType;

        //Sun
        planet.sun.GetComponent<Light>().flare = Resources.Load<Flare>("Planet/Environment/Lens Flares/" + sunFlare);
        planet.sun.GetComponent<Light>().color = sunColor;
        planet.sun.GetComponent<Light>().intensity = sunIntensity;

        //Ocean
        planet.SetOcean((int)oceanHeight, oceanType, iceTexture);
        planet.SetUnderwaterColor(underwaterColor);

        //Audio
        planet.GetComponent<AudioReverbZone>().reverbPreset = reverb;
        planet.dayAmbience = Resources.Load<AudioClip>("Planet/Environment/Ambience/" + dayAmbience);
        planet.nightAmbience = Resources.Load<AudioClip>("Planet/Environment/Ambience/" + nightAmbience);
        planet.groundWalking = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + groundWalking);
        planet.groundRunning = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + groundRunning);
        planet.seabedWalking = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + seabedWalking);
        planet.seabedRunning = Resources.Load<AudioClip>("Planet/Environment/Terrain Footsteps/" + seabedRunning);

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
    public Planet.BiomeSubType biomeSubType;

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