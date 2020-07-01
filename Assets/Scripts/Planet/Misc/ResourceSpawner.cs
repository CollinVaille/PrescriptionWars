using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    public PossibleResource[] possibleResources;

    private void Start()
    {
        //Get random resource
        string resourceName = GetRandomResourceName();

        if (resourceName.Equals(""))
            return;

        //Spawn resource
        Transform resourceInstance = Instantiate(Resources.Load<GameObject>(resourceName)).transform;

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
