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
    public AudioClip deflection, jab, softItemImpact, hardItemImpact, genericImpact;

    //Audio Managment
    //Parallel lists for pause/resuming audio sources on pause menu
    private List<AudioSource> managedAudioSources;
    private List<bool> wasPlaying;

    //Projectile management
    private List<Projectile> managedProjectiles;

    //Corpse management
    public GameObject corpsePrefab;

    //City management
    private List<City> citiesToUpdate;

    //Camera management
    private Camera currentCamera;
    private AudioListener currentListener;

    //Initialization
    private void Awake()
    {
        god = this;

        //Loading camera
        SetActiveCamera(GetComponent<Camera>(), true);

        //Variable initialization
        managedAudioSources = new List<AudioSource>();
        wasPlaying = new List<bool>();
        managedProjectiles = new List<Projectile>();
        citiesToUpdate = new List<City>();

        //Initialize settings
        AudioSettings.LoadSettings();
        VideoSettings.LoadSettings();

        //Call for restart of all static initialization
        Vehicle.setUp = false;
        Voice.InitialSetUp();
        Projectile.SetUpPooling();
        Explosion.SetUpPooling();
        DavyJonesLocker.PrepareTheLockerForSouls();
    }

    //Delayed Initialization
    private void Start()
    {
        StartCoroutine(ManageProjectiles());
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

    private IEnumerator ManageProjectiles()
    {
        float stepTime = 0.033f;

        float lastTime = Time.timeSinceLevelLoad;
        float actualStepTime = stepTime;

        while (true)
        {
            yield return new WaitForSeconds(stepTime);

            actualStepTime = Time.timeSinceLevelLoad - lastTime;

            for (int x = 0; x < managedProjectiles.Count; x++)
            {
                //Update projectile
                Projectile original = managedProjectiles[x];
                original.UpdateLaunchedProjectile(actualStepTime);

                //If projectile was removed from list during update, adjust accordingly
                if (x == managedProjectiles.Count || original != managedProjectiles[x])
                    x--;
            }

            lastTime = Time.timeSinceLevelLoad;
        }
    }

    public void ManageProjectile(Projectile projectile) { managedProjectiles.Add(projectile); }

    public void UnmanageProjectile(Projectile projectile) { managedProjectiles.Remove(projectile); }

    //PERIODIC UPDATES--------------------------------------------------------------------------------

    //Indicates city's nav mesh needs to be updated
    public void PaintCityDirty(City city)
    {
        if (!citiesToUpdate.Contains(city))
            citiesToUpdate.Add(city);
    }

    //Performs computationally intensive operations, might cause lag if not called at right time
    //Updates all nav meshes that need updating
    private IEnumerator PerformUpdatesOnPause()
    {
        while (citiesToUpdate.Count > 0)
        {
            //Get next city to update
            City nextCity = citiesToUpdate[0];
            citiesToUpdate.Remove(nextCity);

            //Debug.Log("Updating " + nextCity.name + "'s nav mesh...");

            //Start update
            AsyncOperation navMeshUpdate = nextCity.UpdateNavMesh();

            //Wait until it's done updating
            while (!navMeshUpdate.isDone)
                yield return null;

            //Debug.Log(nextCity.name + "'s nav mesh successfully updated.");
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

    public static Damageable GetDamageable(Transform t)
    {
        if (!t)
            return null;
        else if (t.GetComponent<Damageable>() != null)
            return t.GetComponent<Damageable>();
        else
            return GetDamageable(t.parent);
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
}
