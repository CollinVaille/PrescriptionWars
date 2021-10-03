using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicWater : Water
{
    public float floorHeight = 0;

    private bool currentlyFilling = false;
    private float cubicMetersLeftToAdd = 0, cubicMeterVolume = 0;
    
    public void AddWaterGradually(float cubicMetersToAdd)
    {
        if (currentlyFilling)
            cubicMetersLeftToAdd += cubicMetersToAdd;
        else
        {
            AddWaterInstantly(0.01f); //Add a tiny drop of water just to ensure the water object is enabled. Coroutines cannot start on a disabled object.
            StartCoroutine(AddWaterGraduallyImplement(cubicMetersToAdd));
        }
    }

    private IEnumerator AddWaterGraduallyImplement(float cubicMeters)
    {
        currentlyFilling = true;

        cubicMetersLeftToAdd = cubicMeters;

        //Parameters
        float cubicMetersPerSecond = 30.0f, secondsPerIncrement = 0.1f;

        //At a fixed interval, add fixed amounts of water until we get to the new volume
        while(cubicMetersLeftToAdd > 0)
        {
            //Determine how many cubic meters we're adding this increment
            float cubicMeterIncrement = cubicMetersPerSecond * secondsPerIncrement;
            if (cubicMetersLeftToAdd <= cubicMeterIncrement)
            {
                cubicMeterIncrement = cubicMetersLeftToAdd;
                cubicMetersLeftToAdd = 0;
            }
            else
                cubicMetersLeftToAdd -= cubicMeterIncrement;

            //Update volume
            AddWaterInstantly(cubicMeterIncrement);

            //Wait
            yield return new WaitForSeconds(secondsPerIncrement);
        }

        currentlyFilling = false;
    }

    public void AddWaterInstantly(float cubicMetersToAdd) { SetNewVolume(cubicMeterVolume + cubicMetersToAdd); }

    private void SetNewVolume(float newCubicMeterVolume)
    {
        //Update status
        cubicMeterVolume = newCubicMeterVolume;

        //Determine whether to hide/show gameobject
        if (newCubicMeterVolume > 0)
            gameObject.SetActive(true);
        else
        {
            gameObject.SetActive(false);
            return; //No point in setting scale/position when not visible
        }

        //Set new vertical scale
        Vector3 newScale = transform.localScale;
        newScale.y = newCubicMeterVolume / (transform.localScale.x * transform.localScale.z);
        transform.localScale = newScale;

        //Set new vertical position
        Vector3 newPosition = transform.localPosition;
        newPosition.y = (newScale.y / 2.0f) + floorHeight;
        transform.localPosition = newPosition;
    }
}
