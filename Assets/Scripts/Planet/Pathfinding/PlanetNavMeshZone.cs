using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlanetNavMeshZone : MonoBehaviour
{
    private Collider theCollider;

    private void Start() { theCollider = GetComponent<Collider>(); }

    private void OnTriggerEnter(Collider botCollider) { OnNavMeshZoneChange(true, botCollider); }

    private void OnTriggerExit(Collider botCollider) { OnNavMeshZoneChange(false, botCollider); }

    private void OnNavMeshZoneChange(bool triggerEnter, Collider botCollider)
    {
        PlanetBotNavMeshNavigation navMeshNavigation = botCollider.GetComponent<PlanetBotNavMeshNavigation>();

        if (navMeshNavigation)
        {
            if(triggerEnter)
                navMeshNavigation.SetNavMeshZone(theCollider);
            else //trigger exit
                navMeshNavigation.RemoveNavMeshZone(theCollider);
        }
    }

    public AsyncOperation BakeNavigation(NavMeshSurface navMeshSurface, int radius, bool firstTime)
    {
        AsyncOperation op = null;

        //Scale collider as ground surface for nav zone
        if(radius > 0)
            transform.localScale = new Vector3(radius * 2.25f, 0.1f, radius * 2.25f);

        //Bake nav zone
        if (firstTime)
            navMeshSurface.BuildNavMesh();
        else
        {
            gameObject.layer = 0;
            op = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        //Repurpose collider for agent detection
        gameObject.layer = 13;
        if (radius > 0)
            transform.localScale = new Vector3(radius * 2.2f, 10000, radius * 2.2f);

        return op;
    }
}
