using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFactory : MonoBehaviour
{
    private float cycleDuration = 15.0f;
    public PlanetFactoryMachine startingMachine;

    public Material conveyorBeltMaterial;

    private void Start()
    {
        StartCoroutine(FactoryManagementLoop());
    }

    private IEnumerator FactoryManagementLoop()
    {
        float elapsedTimeForThisCycle = 0.0f;
        float stepDuration = 0.2f;
        while(true)
        {
            PerformFactoryUpdate(elapsedTimeForThisCycle);

            yield return new WaitForSeconds(stepDuration);

            elapsedTimeForThisCycle += stepDuration;
            if (elapsedTimeForThisCycle > cycleDuration)
                elapsedTimeForThisCycle = 0.0f;
        }
    }

    private void PerformFactoryUpdate(float elapsedTimeForThisCycle)
    {
        if (startingMachine)
            startingMachine.PerformMachineCyleUpdate(elapsedTimeForThisCycle / cycleDuration);

        UpdateSingleInstanceFactoryStuff(elapsedTimeForThisCycle);
    }

    private void UpdateSingleInstanceFactoryStuff(float elapsedTimeForThisCycle)
    {
        conveyorBeltMaterial.SetTextureOffset("_MainTex", new Vector2(0.0f, elapsedTimeForThisCycle));
    }
}
