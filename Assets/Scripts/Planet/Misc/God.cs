using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * God.cs
 * Used to perform singleton operations, general management, and misc helper functions on planet view.
*/

public class God : MonoBehaviour
{
    public static God god;

    //Audio
    public AudioClip deflection, jab, softItemImpact, hardItemImpact;

    //Audio Managment
    //Parallel lists for pause/resuming audio sources on pause menu
    private List<AudioSource> managedAudioSources;
    private List<bool> wasPlaying;

    //Fasting moving, high volume object management
    private List<ManagedVolatileObject> managedVolatileObjects;

    //Corpse management
    public GameObject corpsePrefab;

    //City management
    private List<INavZoneUpdater> navZonesToUpdate;

    //Camera management
    private Camera currentCamera;
    private AudioListener currentListener;

    //Altitude management
    [Tooltip("X & Y are min & max heights allowed for vehicles, respectively.")] public Vector2 altitudeLimits;

    //Initialization
    private void Awake()
    {
        god = this;

        //Loading camera
        SetActiveCamera(GetComponent<Camera>(), true);

        //Variable initialization
        managedAudioSources = new List<AudioSource>();
        wasPlaying = new List<bool>();
        managedVolatileObjects = new List<ManagedVolatileObject>();
        navZonesToUpdate = new List<INavZoneUpdater>();

        //Initialize settings
        AudioSettings.LoadSettings();
        VideoSettings.LoadSettings();

        //Call for restart of all static initialization
        Vehicle.setUp = false;
        Voice.InitialSetUp();
        PlanetObjectPool.SetUpPooling();
        DavyJonesLocker.PrepareTheLockerForSouls();
    }

    //Delayed Initialization
    private void Start()
    {
        StartCoroutine(ManageProjectilesAndDeathRays());
        StartCoroutine(PerformUpdatesPeriodically());
    }

    //COORDINATION WITH PAUSE MENU--------------------------------------------------------------------

    public void OnPause()
    {
        //Pause audio sources and keep track of which ones we paused so we can resume them later
        for (int x = 0; x < managedAudioSources.Count; x++)
        {
            //If audio source to manage has been deleted then remove it from management system
            if (!managedAudioSources[x])
            {
                managedAudioSources.RemoveAt(x);
                wasPlaying.RemoveAt(x);
                x--;
                continue;
            }

            if (managedAudioSources[x].isPlaying)
            {
                managedAudioSources[x].Pause();
                wasPlaying[x] = true;
            }
            else
                wasPlaying[x] = false;
        }

        //Perform computationally intensive updates on pause to avoid lag spikes in game
        StartCoroutine(PerformUpdatesOnPause());
    }

    public void OnResume()
    {
        //Resume audio sources that were playing before being paused
        for (int x = 0; x < managedAudioSources.Count; x++)
        {
            //If audio source to manage has been deleted then remove it from management system
            if (!managedAudioSources[x])
            {
                managedAudioSources.RemoveAt(x);
                wasPlaying.RemoveAt(x);
                x--;
                continue;
            }

            if (wasPlaying[x])
                managedAudioSources[x].Play();
        }
    }

    //POOLING AND AUDIO MANAGEMENT--------------------------------------------------------------------

    public void ManageAudioSource(AudioSource toManage)
    {
        managedAudioSources.Add(toManage);
        wasPlaying.Add(false);
    }

    public void UnmanageAudioSource(AudioSource toUnmanage)
    {
        int parallelIndex = managedAudioSources.IndexOf(toUnmanage);

        managedAudioSources.RemoveAt(parallelIndex);
        wasPlaying.RemoveAt(parallelIndex);
    }

    private IEnumerator ManageProjectilesAndDeathRays()
    {
        float stepTime = 0.033f;

        float lastTime = Time.timeSinceLevelLoad;
        float actualStepTime = stepTime;

        while (true)
        {
            yield return new WaitForSeconds(stepTime);

            actualStepTime = Time.timeSinceLevelLoad - lastTime;

            for (int x = 0; x < managedVolatileObjects.Count; x++)
            {
                //Update projectile
                ManagedVolatileObject original = managedVolatileObjects[x];
                original.UpdateActiveStatus(actualStepTime);

                //If projectile was removed from list during update, adjust accordingly
                if (x == managedVolatileObjects.Count || original != managedVolatileObjects[x])
                    x--;
            }

            lastTime = Time.timeSinceLevelLoad;
        }
    }

    public void ManageVolatileObject(ManagedVolatileObject volatileObjectToManage) { managedVolatileObjects.Add(volatileObjectToManage); }

    public void UnmanageVolatileObject(ManagedVolatileObject volatileObjectToUnmanage) { managedVolatileObjects.Remove(volatileObjectToUnmanage); }

    //PERIODIC UPDATES--------------------------------------------------------------------------------

    //Indicates city's nav mesh needs to be updated
    public void PaintNavMeshDirty(INavZoneUpdater navZoneUpdater)
    {
        if (!navZonesToUpdate.Contains(navZoneUpdater))
            navZonesToUpdate.Add(navZoneUpdater);
    }

    //Performs computationally intensive operations, might cause lag if not called at right time
    //Updates all nav meshes that need updating
    private IEnumerator PerformUpdatesOnPause()
    {
        while (navZonesToUpdate.Count > 0)
        {
            //Get next nav mesh to update
            INavZoneUpdater nextNavMesh = navZonesToUpdate[0];
            navZonesToUpdate.Remove(nextNavMesh);

            //Start update
            AsyncOperation navMeshUpdate = nextNavMesh.UpdateNavMesh();

            //Wait until it's done updating
            while (!navMeshUpdate.isDone)
                yield return null;
        }
    }

    private IEnumerator PerformUpdatesPeriodically()
    {
        while(true)
        {
            if(Planet.planet)
                Planet.planet.UpdateSkyboxReflectiveProbe();

            //Random significant stutter of time
            yield return new WaitForSeconds(Random.Range(2.0f, 5.0f));
        }
    }

    //MISC HELPER FUNCTIONS--------------------------------------------------------------------------

    public static void InitializeAudioList(List<AudioClip> audioList, string resourcesPath)
    {
        for (int x = 1; x <= 10000; x++)
        {
            AudioClip newClip = Resources.Load<AudioClip>(resourcesPath + x);

            if (newClip)
                audioList.Add(newClip);
            else
                break;
        }
    }

    public static AudioClip RandomClip(List<AudioClip> audioList)
    {
        return audioList[Random.Range(0, audioList.Count)];
    }

    public static string SpaceOutString(string toSpaceOut)
    {
        char[] original = toSpaceOut.ToCharArray();

        int newSize = original.Length;

        //Compute size of new array by incrementing it everytime we find a place to add a space character
        for (int x = 0; x < original.Length; x++)
        {
            if (x != 0 && original[x] >= 65 && original[x] <= 90)
                newSize++;
        }

        char[] modified = new char[newSize];

        //Create new string as char array
        int newIndex = 0;
        for (int oldIndex = 0; oldIndex < original.Length; oldIndex++, newIndex++)
        {
            //Add space
            if (oldIndex != 0 && original[oldIndex] >= 65 && original[oldIndex] <= 90)
            {
                modified[newIndex] = ' ';
                newIndex++;
            }

            //Copy character over
            modified[newIndex] = original[oldIndex];
        }

        return new string(modified);
    }

    public void SetActiveCamera(Camera newCamera, bool withListener)
    {
        if(currentCamera)
            currentCamera.enabled = false;
        newCamera.enabled = true;
        currentCamera = newCamera;

        if (withListener)
            SetActiveListener(currentCamera.GetComponent<AudioListener>());
    }

    private void SetActiveListener(AudioListener newListener)
    {
        if(currentListener)
            currentListener.enabled = false;
        newListener.enabled = true;
        currentListener = newListener;
    }

    public GameObject InstantiateSomeShit(GameObject somePrefab) { return Instantiate(somePrefab); }

    public void DestroySomeShit(GameObject toDestroy) { Destroy(toDestroy); }

    public static void SnapToGround(Transform transformToSnap, float startSearchFromHeight = 9000.0f, List<Collider> collidersToCheckAgainst = null)
    {
        Vector3 newTransformPosition = transformToSnap.position;
        transformToSnap.position = Vector3.one * 9000.0f;
        transformToSnap.gameObject.SetActive(false);

        Ray ray = new Ray(newTransformPosition + Vector3.up * startSearchFromHeight, Vector3.down);

        if(collidersToCheckAgainst == null) //Check against all colliders in the scene
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                newTransformPosition.y = hitInfo.point.y;
        }
        else //Check against the specific list of colliders provided
        {
            float highestContactHeight = Mathf.NegativeInfinity;
            bool hitSomething = false;
            foreach(Collider collider in collidersToCheckAgainst)
            {
                if(collider.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
                {
                    if (hitInfo.point.y > highestContactHeight)
                    {
                        hitSomething = true;
                        highestContactHeight = hitInfo.point.y;
                    }
                }
            }

            if (hitSomething)
                newTransformPosition.y = highestContactHeight;
        }

        transformToSnap.position = newTransformPosition;
        transformToSnap.gameObject.SetActive(true);
    }

    public static void CopyMaterialValues(Material copyValuesFrom, Material copyValuesTo, float xScaling, float yScaling, bool scalingIsRelative)
    {
        copyValuesTo.mainTexture = copyValuesFrom.mainTexture;
        copyValuesTo.SetTexture("_BumpMap", copyValuesFrom.GetTexture("_BumpMap"));

        float metallic = copyValuesFrom.GetFloat("_Metallic");
        float smoothness = copyValuesFrom.GetFloat("_Glossiness");

        //if (metallic > 0 || smoothness > 0)
        //    materialToUpdate.EnableKeyword("_METALLICGLOSSMAP");

        copyValuesTo.SetFloat("_Metallic", metallic);
        copyValuesTo.SetFloat("_Glossiness", smoothness);

        // if (metallic == 0 && smoothness == 0)
        //     materialToUpdate.DisableKeyword("_METALLICGLOSSMAP");

        //Scale texture
        Vector2 newTextureScale = copyValuesFrom.mainTextureScale;
        if(scalingIsRelative)
        {
            newTextureScale.x *= xScaling;
            newTextureScale.y *= yScaling;
        }
        else
        {
            newTextureScale.x = xScaling;
            newTextureScale.y = yScaling;
        }
        copyValuesTo.mainTextureScale = newTextureScale;

        //materialToUpdate.
        //DynamicGI.UpdateEnvironment();
    }
}
