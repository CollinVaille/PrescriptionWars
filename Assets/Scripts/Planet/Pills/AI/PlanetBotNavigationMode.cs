using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetBotNavigationMode : MonoBehaviour
{
    protected PlanetBotNavigationController navigation { get; private set; }
    protected PlanetBotPill bot { get => navigation.bot; }
    protected Rigidbody rBody { get => bot.GetRigidbody(); }

    public virtual void Initialize(PlanetBotNavigationController navigation)
    {
        this.navigation = navigation;
    }

    /// <summary>
    /// Called when this navigation mode is not being used and any applicable higher-priority modes are not currently available.
    /// The purpose is to (a) determine if we should switch over to this mode and (b) do so if we should.
    /// </summary>
    /// <returns>True if the activation was successful.</returns>
    public abstract bool TryToActivate();
    
    /// <summary>
    /// Called when this mode is currently active to deactivate it.
    /// </summary>
    public abstract void Deactivate();
    
    /// <summary>
    /// Regular update call to the activate navigation mode. This is needed to ensure the navigation is working as intended.
    /// </summary>
    public abstract void PerformUpdate();

    /// <summary>
    /// <para>Called regularly on the active navigation mode to determine if we can continue to use this mode for navigating.
    /// If we can, then PerformUpdate() is called. Otherwise, Deactivate() is called and another navigation mode will be chosen.</para>
    /// 
    /// <para>For example, if this is the navmesh mode and the bot leaves the navmesh, this should return false.</para>
    /// 
    /// </summary>
    /// <returns>True if navigation can continue with this mode.</returns>
    public abstract bool CanContinue();
}
