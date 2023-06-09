using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that acts as a controller for bot navigation in the planet view.
/// It encapsulates all the data needed to track navigation and switches between the three navigation modes (waypoint, navmesh, and primitive) as needed.
/// This makes it to where the base bot class does not have to worry about any navigation details and can simply say "SetNavigationTarget" and "PerformUpdate" in order to move around.
/// </summary>
public class PlanetBotNavigationController
{
    private enum NavigationModeIndex { Waypoint = 0, NavMesh = 1, Primitive = 2 }

    private PlanetBotNavigationMode[] navigationModes;
    private int currentNavigationModeIndex = (int)NavigationModeIndex.Primitive;

    public PlanetBotPill bot { get; private set; }
    private Rigidbody rBody { get => bot.GetRigidbody(); }
    public Transform targetTransform { get; private set; }
    public Vector3 targetGlobalPosition { get; private set; }
    /// <summary>
    /// True if navigation has already started towards the current target.
    /// </summary>
    public bool navigationHasBegun { get; private set; }
    /// <summary>
    /// True if the bot is within the arrivalRadius set at SetNavigationTarget(...) of the target. Updated at the start of every PerformUpdate() call.
    /// </summary>
    public bool atDestination { get; private set; }
    public float arrivalRadius { get; private set; }


    public PlanetBotNavigationController(PlanetBotPill bot)
    {
        this.bot = bot;

        navigationModes = new PlanetBotNavigationMode[3];
        navigationModes[(int)NavigationModeIndex.Waypoint] = bot.GetComponent<PlanetBotWaypointNavigation>();
        navigationModes[(int)NavigationModeIndex.NavMesh] = bot.GetComponent<PlanetBotNavMeshNavigation>();
        navigationModes[(int)NavigationModeIndex.Primitive] = bot.GetComponent<PlanetBotPrimitiveNavigation>();

        foreach (PlanetBotNavigationMode navigationMode in navigationModes)
            navigationMode.Initialize(this);
    }

    /// <summary>
    /// <para>
    /// Stops any previously-running navigation/movement and sets the target data needed to begin new navigation/movement.
    /// PerformUpdate will need to be called regularly afterwards to actually perform the navigation/movement.
    /// </para>
    /// <para>
    /// The target can be specified in one of two ways: as a Transform or as a Vector3.
    /// A null transform means navigation will be based solely on the Vector3.
    /// A non-null transform will override whatever is put in the Vector3.
    /// </para>
    /// </summary>
    /// <param name="targetTransform"></param>
    /// <param name="targetGlobalPosition"></param>
    public void SetNavigationTarget(Transform targetTransform, Vector3 targetGlobalPosition, float arrivalRadius)
    {
        if(navigationHasBegun)
            Stop();

        this.targetTransform = targetTransform;
        this.targetGlobalPosition = targetGlobalPosition;
        this.arrivalRadius = arrivalRadius;
    }

    /// <summary>
    /// Performs a single update to the navigation system.
    /// In the case of primitive or waypoint-driven navigation, this will cause the pill to move a "step".
    /// In the case of navmesh-driven navigation, this will ensure the navmesh agent has the most up-to-date data.
    /// </summary>
    public void PerformUpdate()
    {
        if (!navigationHasBegun)
            navigationHasBegun = true;

        if (targetTransform)
            targetGlobalPosition = targetTransform.position;

        atDestination = Vector3.Distance(rBody.position, targetGlobalPosition) < arrivalRadius;

        DetermineCurrentNavigationMode();

        navigationModes[currentNavigationModeIndex].PerformUpdate();
    }

    /// <summary>
    /// Stops navigation to the current target.
    /// </summary>
    public void Stop()
    {
        if (!navigationHasBegun)
            return;

        DeactivateCurrentNavigationMode();
        navigationHasBegun = false;
    }

    private void DetermineCurrentNavigationMode()
    {
        int x = 0;
        for (;; x++)
        {
            if (x == currentNavigationModeIndex)
            {
                if (navigationModes[x].CanContinue())
                    break;
                else
                    continue;
            }
            else if (navigationModes[x].TryToActivate() || x + 1 == navigationModes.Length)
                break;
        }

        if(x != currentNavigationModeIndex)
        {
            DeactivateCurrentNavigationMode();
            currentNavigationModeIndex = x;
        }
    }

    private void DeactivateCurrentNavigationMode()
    {
        if(currentNavigationModeIndex >= 0)
            navigationModes[currentNavigationModeIndex].Deactivate();
    }

    /// <returns>The distance between the two vectors NOT factoring in their y value (so ignoring height difference).</returns>
    public static float GroundDistance(Vector3 position1, Vector3 position2)
    {
        //a^2 + b^2 = c^2
        //so... c = sqrt(a^2 + b^2)
        return Mathf.Sqrt(Mathf.Pow(position1.x - position2.x, 2) + Mathf.Pow(position1.z - position2.z, 2));
    }
}
