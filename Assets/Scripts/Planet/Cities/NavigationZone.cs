using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationZone : MonoBehaviour
{
    private Collider theCollider;

    private void Start() { theCollider = GetComponent<Collider>(); }

    private void OnTriggerEnter(Collider bot) { bot.GetComponent<Pill>().SetNavigationZone(theCollider); }

    private void OnTriggerExit(Collider bot) { bot.GetComponent<Pill>().RemoveNavigationZone(theCollider); }

    public AsyncOperation BakeNavigation(NavMeshSurface navMeshSurface, int radius, bool firstTime)
    {
        AsyncOperation op = null;

        //Scale collider as ground surface for nav zone 
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
        transform.localScale = new Vector3(radius * 2.2f, 100, radius * 2.2f);
        gameObject.layer = 13;

        return op;
    }
}
