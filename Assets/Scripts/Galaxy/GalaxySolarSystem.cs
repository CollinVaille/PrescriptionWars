using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySolarSystem : MonoBehaviour
{
    [Header("Components")]

    [SerializeField, Tooltip("The particle system that creates the space dust clouds that encompass the solar system.")] private ParticleSystem spaceDustPS = null;
    [SerializeField, LabelOverride("Planetary Orbits Parent"), Tooltip("The transform of the game object that serves as the parent of all planet orbits in the solar system.")] private Transform planetaryOrbitsParentVar = null;
    [SerializeField, Tooltip("The visibility plane of the solar system that triggers needed functions whenever the solar system becomes either visible or invisibile to the main galaxy camera.")] private VisibilityPlane visibilityPlane = null;

    //Non-inspector variables and properties.

    /// <summary>
    /// Private variable that holds a reference to the star that is at the center of this solar system. Should be accessed from the publicly accessible property.
    /// </summary>
    private GalaxyStar starVar = null;
    /// <summary>
    /// Public property that should be used to access the script of the star that is at the center of the solar system.
    /// </summary>
    public GalaxyStar star { get => starVar; }

    /// <summary>
    /// Public property that should be used to acces the parent of orbits that planets in the solar system can be assigned to.
    /// </summary>
    public Transform planetaryOrbitsParent { get => planetaryOrbitsParentVar; }

    /// <summary>
    /// Private list that holds the planets that are contained within this solar system in the galaxy.
    /// </summary>
    private List<NewGalaxyPlanet> planetsVar = null;
    /// <summary>
    /// Public property that should be used in order to access the list of planets that are contained within this solar system in the galaxy.
    /// </summary>
    public List<NewGalaxyPlanet> planets { get => planetsVar; }

    /// <summary>
    /// Private variable that holds the index of the planet in the system's list of planets that is the capital planet of the system.
    /// </summary>
    private int capitalPlanetIndexVar = 0;
    /// <summary>
    /// Public property that should be used to access the index of the solar system's capital planet in the list of planets in the solar system.
    /// </summary>
    public int capitalPlanetIndex { get => capitalPlanetIndexVar; }
    /// <summary>
    /// Public property that should be used to access the planet that serves as the capital planet of the solar system.
    /// </summary>
    public NewGalaxyPlanet capitalPlanet { get => planets[capitalPlanetIndexVar]; }

    /// <summary>
    /// Public property that should be used to access the empire that controls the solar system, which is actually just the empire that controls the capital planet of the solar system.
    /// </summary>
    public NewEmpire owner { get => capitalPlanet.owner; }
    /// <summary>
    /// Public property that should be used to access the ID of the empire that controls the solar system, which is actually just the empire that controls the capital planet of the solar system, though it might be more intuitive usually to use the owner property directly.
    /// </summary>
    public int ownerID { get => capitalPlanet.ownerID; }

    /// <summary>
    /// Public property that should be used in order to access the color of the empire that owns the solar system. Returns white if no empire controls the solar system.
    /// </summary>
    public Color ownerColor { get => owner == null ? Color.white : owner.color; }

    /// <summary>
    /// Private variable that holds the ID of the solar system (index in the list of solar systems in the galaxy).
    /// </summary>
    private int IDVar = -1;
    /// <summary>
    /// Public property that should be used to access the ID of the solar system (index in the list of solar systems in the galaxy).
    /// </summary>
    public int ID { get => IDVar; }

    /// <summary>
    /// Public property that should be accessed in order to determine whether or not the solar system is the capital system of the empire.
    /// </summary>
    public bool isCapitalSystem { get => owner.capitalSystemID == ID; }

    /// <summary>
    /// Private variable that holds a boolean value that indicates whether or not the visibility plane of the solar system is visible to the main camera.
    /// </summary>
    private bool visibleVar = false;
    /// <summary>
    /// Public property that should be accessed in order to determine if the visibility plane of the solar system is visible to the main galaxy camera or not.
    /// </summary>
    public bool visible
    {
        get => visibleVar;
    }

    /// <summary>
    /// Private list that holds all of the functions that need to be executed whenever the visibility plane of the solar system becomes visible to the main galaxy camera.
    /// </summary>
    private List<Action> onBecameVisibleFunctions = null;
    /// <summary>
    /// Private list that holds all of the functions that need to be executed whenever the visibility plane of the solar system becomes invisible to the main galaxy camera.
    /// </summary>
    private List<Action> onBecameInvisibleFunctions = null;

    /// <summary>
    /// Private list that holds the ID of all of the hyperspace lanes that directly connect this solar system with another solar system.
    /// </summary>
    private List<int> hyperspaceLaneIDsVar = null;
    /// <summary>
    /// Publicly accessible property that returns a list that contains the ID of all of the hyperspace lanes that directly connect this solar system with another solar system (never returns null, will return an empty list if no connections but the underlying private list is null).
    /// </summary>
    public List<int> hyperspaceLaneIDs { get => hyperspaceLaneIDsVar == null ? new List<int>() : hyperspaceLaneIDsVar; }
    /// <summary>
    /// Publicly accessible property that contains all of the hyperspace lanes that directly connect this solar system with another solar system.
    /// </summary>
    public List<HyperspaceLane> hyperspaceLanes
    {
        get
        {
            //Initializes an empty list of hyperspace lanes.
            List<HyperspaceLane> hyperspaceLanesTemp = new List<HyperspaceLane>();
            //Loops through each hyperspace lane id and uses it to obtain the correct hyperspace lane objects from the galaxy manager (if the galaxy manager's hyperspace lanes list has been initialized) and adds said hyperspace lane objects to the list previously initialized.
            if(NewGalaxyManager.hyperspaceLanes != null)
                foreach(int hyperspaceLaneID in hyperspaceLaneIDsVar)
                    hyperspaceLanesTemp.Add(NewGalaxyManager.hyperspaceLanes[hyperspaceLaneID]);
            //Returns the list of hyperspace lanes.
            return hyperspaceLanesTemp;
        }
    }

    private void Awake()
    {
        //Informs the solar system's visibility plane of the functions that need to be executed once the visibility plane becomes either visible or invisible.
        visibilityPlane.AddOnBecameVisibleFunction(OnVisibilityPlaneBecameVisible);
        visibilityPlane.AddOnBecameInvisibleFunction(OnVisibilityPlaneBecameInvisible);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This public method should be called in order to initialize the solar system from the GenerateSolarSystems method in the galaxy generator.
    /// </summary>
    /// <param name="star"></param>
    public void InitializeFromGalaxyGenerator(GalaxyStar star, List<NewGalaxyPlanet> planets, int ID)
    {
        //Initializes the star variable.
        starVar = star;
        //Initializes the planets list variable.
        planetsVar = planets;
        //Initializes the int that indicates which planet in the solar system serves as the capital planet of the system.
        capitalPlanetIndexVar = 0;
        //Initializes the ID of the solar system.
        IDVar = ID;
    }

    /// <summary>
    /// This public method should be called by the GenerateSolarSystems method in the galaxy generator in order to initialize the solar system from save data that has been statically passed over from the load game menu.
    /// </summary>
    /// <param name="solarSystemData"></param>
    public void InitializeFromSaveData(GalaxySolarSystemData solarSystemData, GalaxyStar star, List<NewGalaxyPlanet> planets, int ID, float localYRotation)
    {
        //Loads in the local position from the solar system save data.
        transform.localPosition = new Vector3(solarSystemData.localPosition[0], solarSystemData.localPosition[1], solarSystemData.localPosition[2]);
        //Sets the star variable to the star that has already been loaded in from the solar system save data.
        starVar = star;
        //Sets the variable that indicates the index of the system's capital planet in the list of planets in the solar system.
        capitalPlanetIndexVar = solarSystemData.capitalPlanetIndex;
        //Initializes the planets list variable.
        planetsVar = planets;
        //Sets the variable that indicates the ID of the solar system.
        IDVar = ID;
        //Sets the local y rotation of the solar system.
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, localYRotation, transform.localRotation.eulerAngles.z);
        //Initializes the hyperspace lane IDs list.
        hyperspaceLaneIDsVar = solarSystemData.hyperspaceLaneIDs;
    }

    /// <summary>
    /// Public function that should be called by the script of the empire's capital planet whenever its owner changes.
    /// </summary>
    public void OnOwnerChange(int previousOwnerID)
    {
        //Removes the solar system from the list of solar systems controlled by the previous owning empire if needed.
        if(previousOwnerID >= 0 && previousOwnerID < NewGalaxyManager.empires.Count)
            NewGalaxyManager.empires[previousOwnerID].solarSystemIDs.Remove(ID);
        //Adds the solar system to the list of solar systems controlled by the new owner empire if needed.
        if (owner != null)
            owner.solarSystemIDs.Add(ID);

        //Updates the color of the solar system's space dust particle system.
        ParticleSystem.MainModule spaceDustPSMain = spaceDustPS.main;
        spaceDustPSMain.startColor = Color.Lerp(spaceDustPSMain.startColor.color, new Color(ownerColor.r, ownerColor.g, ownerColor.b, spaceDustPSMain.startColor.color.a), 0.09f);
    }

    /// <summary>
    /// Private method that should be called by the visibility plane of the solar system whenever it becomes visible to the main galaxy camera.
    /// </summary>
    private void OnVisibilityPlaneBecameVisible()
    {
        //Stores that the solar system should now be considered visible to the main galaxy camera.
        visibleVar = true;

        //Executes each function that was added to the list of functions that need to be executed whenever the visibility plane of the solar system becomes visible to the main galaxy camera.
        if(onBecameVisibleFunctions != null)
            foreach (Action onBecameVisibleFunction in onBecameVisibleFunctions)
                onBecameVisibleFunction();
    }

    /// <summary>
    /// Private method that should be called by the visibility plane of the solar system whenever it becomes invisible to the main galaxy camera.
    /// </summary>
    private void OnVisibilityPlaneBecameInvisible()
    {
        //Stores that the solar system should now be considered invisible to the main galaxy camera.
        visibleVar = false;

        //Executes each function that was added to the list of functions that need to be executed whenever the visibility plane of the solar system becomes invisible to the main galaxy camera.
        if (onBecameInvisibleFunctions != null)
            foreach (Action onBecameInvisibleFunction in onBecameInvisibleFunctions)
                onBecameInvisibleFunction();
    }

    /// <summary>
    /// Public method that should be used in order to add a function to the list of functions to be executed whenever the visibility plane of the solar system becomes visible to the main galaxy camera.
    /// </summary>
    /// <param name="onBecameVisibleFunction"></param>
    public void AddOnBecameVisibleFunction(Action onBecameVisibleFunction)
    {
        if (onBecameVisibleFunctions == null)
            onBecameVisibleFunctions = new List<Action>();
        onBecameVisibleFunctions.Add(onBecameVisibleFunction);
    }

    /// <summary>
    /// Public method that should be used in order to add a function to the list of functions to be executed whenever the visibility plane becomes invisible to the main camera.
    /// </summary>
    /// <param name="onBecameInvisibleFunction"></param>
    public void AddOnBecameInvisibleFunction(Action onBecameInvisibleFunction)
    {
        if (onBecameInvisibleFunctions == null)
            onBecameInvisibleFunctions = new List<Action>();
        onBecameInvisibleFunctions.Add(onBecameInvisibleFunction);
    }

    /// <summary>
    /// Public method that should be used in order to remove a function from the list of functions to be executed whenever the visibility plane becomes visible to the main camera.
    /// </summary>
    /// <param name="onBecameVisibleFunction"></param>
    public void RemoveOnBecameVisibleFunction(Action onBecameVisibleFunction)
    {
        if (onBecameVisibleFunctions != null && onBecameVisibleFunctions.Contains(onBecameVisibleFunction))
        {
            onBecameVisibleFunctions.Remove(onBecameVisibleFunction);
            if (onBecameVisibleFunctions.Count == 0)
                onBecameVisibleFunctions = null;
        }
    }

    /// <summary>
    /// Public method that should be used in order to remove a function from the list of functions to be executed whenever the visibility plane becomes invisible to the main camera.
    /// </summary>
    /// <param name="onBecameInvisibleFunction"></param>
    public void RemoveOnBecameInvisibleFunction(Action onBecameInvisibleFunction)
    {
        if (onBecameInvisibleFunctions != null && onBecameInvisibleFunctions.Contains(onBecameInvisibleFunction))
        {
            onBecameInvisibleFunctions.Remove(onBecameInvisibleFunction);
            if (onBecameInvisibleFunctions.Count == 0)
                onBecameInvisibleFunctions = null;
        }
    }

    /// <summary>
    /// Public method that should be called mainly by the galaxy generator in order to log in the solar system that the specified hyperspace lane ID belongs to a hyperspace lane that connects this specific solar system to another solar system directly.
    /// </summary>
    /// <param name="hyperspaceLaneID"></param>
    public void AddHyperspaceLaneIDLog(int hyperspaceLaneID)
    {
        if (hyperspaceLaneIDsVar == null)
            hyperspaceLaneIDsVar = new List<int>();
        if (!hyperspaceLaneIDsVar.Contains(hyperspaceLaneID))
            hyperspaceLaneIDsVar.Add(hyperspaceLaneID);
    }

    /// <summary>
    /// Public method that should be called in order to log in the solar system that the specified hyperspace lane ID no longer belongs to a hyperspace lane that connects this specific solar system to another solar system directly.
    /// </summary>
    /// <param name="hyperspaceLaneID"></param>
    public void RemoveHyperspaceLaneIDLog(int hyperspaceLaneID)
    {
        if (hyperspaceLaneIDsVar == null)
            return;
        if (hyperspaceLaneIDsVar.Contains(hyperspaceLaneID))
            hyperspaceLaneIDsVar.Remove(hyperspaceLaneID);
    }

    /// <summary>
    /// Public method that should be called by the owning empire whenever the solar system is no longer the capital system of the empire.
    /// </summary>
    public void OnBecameCapitalSystem()
    {
        if(capitalPlanet != null)
            capitalPlanet.OnBecameEmpireCapitalPlanet();
    }

    /// <summary>
    /// Public method that should be called by the owning empire whenever the solar system is now the capital system of the empire.
    /// </summary>
    public void OnBecameNoncapitalSystem()
    {
        if (capitalPlanet != null)
            capitalPlanet.OnBecameEmpireNoncapitalPlanet();
    }

    /// <summary>
    /// Public method that should be called by the galaxy manager in its EndTurnUpdate method and it updates the local y rotation of the solar system to be the opposite local y rotation of the galaxy in order to ensure the labels are the correct orientation.
    /// </summary>
    public void EndTurnUpdate()
    {
        transform.localRotation = new Quaternion(transform.localRotation.x, -1 * NewGalaxyManager.galaxyManager.transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
    }
}

[System.Serializable]
public class GalaxySolarSystemData
{
    public float[] localPosition = null;
    public GalaxyStarData star = null;
    public List<GalaxyPlanetData> planets = null;
    public int capitalPlanetIndex = 0;
    public List<int> hyperspaceLaneIDs = null;

    public GalaxySolarSystemData(GalaxySolarSystem solarSystem)
    {
        localPosition = new float[3];
        localPosition[0] = solarSystem.transform.localPosition.x;
        localPosition[1] = solarSystem.transform.localPosition.y;
        localPosition[2] = solarSystem.transform.localPosition.z;

        star = new GalaxyStarData(solarSystem.star);

        planets = new List<GalaxyPlanetData>();
        for(int planetIndex = 0; planetIndex < solarSystem.planets.Count; planetIndex++)
            planets.Add(new GalaxyPlanetData(solarSystem.planets[planetIndex]));

        capitalPlanetIndex = solarSystem.capitalPlanetIndex;

        hyperspaceLaneIDs = solarSystem.hyperspaceLaneIDs.Count == 0 ? null : solarSystem.hyperspaceLaneIDs;
    }
}