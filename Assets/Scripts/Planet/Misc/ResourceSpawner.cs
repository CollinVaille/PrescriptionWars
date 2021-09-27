using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    private static string resourcePathPrefix = "Planet/";

    public PossibleResource[] possibleResources;
    public bool makeResourceChild = false;

    private void Start()
    {
        LoadRandomResource();
    }

    private void LoadRandomResource()
    {
        //Determine which resource to load randomly
        string resourceName = GetRandomResourceName();

        if (resourceName.Equals(""))
            return;

        //Spawn resource
        Transform resourceInstance = Instantiate(Resources.Load<GameObject>(resourcePathPrefix + resourceName)).transform;

        //Get rid of (Clone) part of name
        resourceInstance.name = resourceInstance.name.Substring(0, resourceInstance.name.Length - 7);

        //Parent resource?
        if (makeResourceChild)
            resourceInstance.parent = transform;

        //Move/rotate resource
        resourceInstance.position = transform.position;
        resourceInstance.rotation = transform.rotation;
    }

    private string GetRandomResourceName ()
    {
        if (possibleResources.Length == 0)
            return "";

        float totalProbability = 0.0f;

        foreach (PossibleResource possibleResource in possibleResources)
            totalProbability += possibleResource.spawnChance;

        float theGreatJudgement = Random.Range(0.0f, totalProbability);

        float elapsed = 0.0f;
        string resourceName = possibleResources[0].name;
        for(int resourceIndex = 0; resourceIndex < possibleResources.Length; resourceIndex++)
        {
            elapsed += possibleResources[resourceIndex].spawnChance;

            if(theGreatJudgement < elapsed)
            {
                resourceName = possibleResources[resourceIndex].name;
                break;
            }
        }

        return resourceName;
    }
}

[System.Serializable]
public class PossibleResource
{
    public string name;
    public float spawnChance;
}
