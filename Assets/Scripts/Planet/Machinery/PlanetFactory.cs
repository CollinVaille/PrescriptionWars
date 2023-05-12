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
        float stepDuration = 0.1f;
        while(true)
        {
            PerformFactoryUpdate(elapsedTimeForThisCycle, stepDuration);

            yield return new WaitForSeconds(stepDuration);

            elapsedTimeForThisCycle += stepDuration;
            if (elapsedTimeForThisCycle > cycleDuration)
                elapsedTimeForThisCycle = 0.0f;
        }
    }

    private void PerformFactoryUpdate(float elapsedTimeForThisCycle, float stepDuration)
    {
        if (startingMachine)
            startingMachine.PerformMachineCyleUpdate(elapsedTimeForThisCycle / cycleDuration, stepDuration);

        UpdateSingleInstanceFactoryStuff(elapsedTimeForThisCycle);
    }

    private void UpdateSingleInstanceFactoryStuff(float elapsedTimeForThisCycle)
    {
        conveyorBeltMaterial.SetTextureOffset("_MainTex", new Vector2(0.0f, elapsedTimeForThisCycle));
    }
}
