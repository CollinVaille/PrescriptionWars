using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlanetBotNavMeshNavigation : PlanetBotNavigationMode
{
    private NavMeshAgent agent;
    private NavMeshHit hit;
    private Collider navMeshZone;
    private bool pathPending = false;
    private Vector3 globalStartPosition;

    private bool reachedEndOfPath { get => Mathf.Approximately(agent.velocity.sqrMagnitude, 0.0f) && Vector3.Distance(agent.nextPosition, globalStartPosition) > agent.stoppingDistance; }

    public override void Initialize(PlanetBotNavigationController navigation)
    {
        base.Initialize(navigation);
        
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false; //Do not automatically update the transform position. We will instead do that in PerformUpdate using the rigidbody position.
    }

    public override void Deactivate()
    {
        if(agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
            agent.isStopped = true;

        agent.enabled = false;
        enabled = false;
    }

    public override bool TryToActivate()
    {
        //Must be within a nav mesh zone
        if (!navMeshZone)
        {
            Debug.Log("not in nav mesh zone for " + name);
            return false;
        }

        //If we're already at the closest point to the destination the nav mesh can take us to then there's no point in using the nav mesh
        if (Vector3.Distance(rBody.position, navMeshZone.ClosestPoint(navigation.targetGlobalPosition)) < navigation.arrivalRadius)
        {
            Debug.Log("already at closest point for " + name);
            return false;
        }

        //Final check; this time we check directly with the NavMesh system to make sure we're close enough to snap to the mesh
        //This is needed for example when the bot is falling in the air and is inside the zone, not near the destination, but not near the nav mesh itself
        if (!NavMesh.SamplePosition(rBody.position, out hit, agent.height, NavMesh.AllAreas))
        {
            Debug.Log("invalid starting position for " + name);
            return false;
        }

        //Passed all checks; proceed with activating
        enabled = true;
        agent.enabled = true;
        globalStartPosition = hit.position;
        agent.Warp(globalStartPosition);
        agent.isStopped = false;

        agent.stoppingDistance = navigation.arrivalRadius;
        agent.speed = bot.moveSpeed;

        agent.ResetPath();
        agent.SetDestination(navMeshZone.ClosestPoint(navigation.targetGlobalPosition));
        pathPending = true;

        return true;
    }

    public override void PerformUpdate()
    {
        //agent.nextPosition = rBody.position;
    }

    private void FixedUpdate()
    {
        //Tell the rigidbody to smoothly interpolate to the latest position of the navmesh agent
        //(we told the agent to not automatically update the rigidbody position because we want to control is ourselves in order to...
        //...prevent y-position snapping and glitching)
        Vector3 newPosition = new Vector3(agent.nextPosition.x, rBody.position.y, agent.nextPosition.z);
        rBody.MovePosition(newPosition);
    }

    public override bool CanContinue()
    {
        if (pathPending)
        {
            if (agent.pathPending)
                return true;
            else
            {
                if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    Debug.Log("INVALID PATH");
                else
                    God.PlaceDebugMarker(agent.destination, markerName: name + "'s Destination", width: 0.1f);

                pathPending = false;

                if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    return false;
                else if (agent.path.corners.Length <= 1)
                    return false;
                else
                    return true;
            }
        }
        else
        {
            Debug.Log(name + ": on mesh=" + agent.isOnNavMesh + ", at end=" + reachedEndOfPath);
            foreach (Vector3 corner in agent.path.corners)
                Debug.Log(name + " path corner at " + corner + " - " + agent.path.corners.Length + " total");

            //God.PlaceDebugMarker(corner, markerName: name + "'s Path Corner", width: 0.2f, height: 0.2f);

            return agent.isOnNavMesh && !reachedEndOfPath;
        }
    }

    public void SetNavMeshZone(Collider navMeshZone)
    {
        this.navMeshZone = navMeshZone;
    }

    public void RemoveNavMeshZone(Collider navMeshZone)
    {
        if (this.navMeshZone == navMeshZone)
            this.navMeshZone = null;
    }
}
