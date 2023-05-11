using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFactory : MonoBehaviour
{
    private float cycleDuration = 15.0f;
    public PlanetFactoryMachine startingMachine;

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
            if (startingMachine)
                startingMachine.PerformMachineCyleUpdate(elapsedTimeForThisCycle / cycleDuration);

            yield return new WaitForSeconds(stepDuration);

            elapsedTimeForThisCycle += stepDuration;
            if (elapsedTimeForThisCycle > cycleDuration)
                elapsedTimeForThisCycle = 0.0f;
        }
    }
}
