using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorLights : VehiclePart
{
    public IndicatorLightPairing[] lightPairings;
    public Material onMaterial, offMaterial, unlitMaterial;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(UpdateLights());
    }

    private IEnumerator UpdateLights()
    {
        while(true)
        {
            //Wait fraction of a second
            yield return new WaitForSeconds(0.5f);

            //Update lights
            foreach (IndicatorLightPairing lightPairing in lightPairings)
                UpdateLight(lightPairing);
        }
    }

    private void UpdateLight(IndicatorLightPairing lightPairing)
    {
        if (!working || !belongsTo || !belongsTo.PoweredOn())
            SetMaterial(lightPairing.indicatorLight, unlitMaterial);
        else
        {
            float healthPercentage = lightPairing.part.GetHealthPercentage();

            if (healthPercentage > 0.75f)
                SetMaterial(lightPairing.indicatorLight, onMaterial);
            else if (healthPercentage > 0.5f)
                FlashBetweenMaterials(lightPairing.indicatorLight, onMaterial, unlitMaterial);
            else if (healthPercentage > 0.25f)
                FlashBetweenMaterials(lightPairing.indicatorLight, onMaterial, offMaterial);
            else if (healthPercentage > 0.0f)
                FlashBetweenMaterials(lightPairing.indicatorLight, offMaterial, unlitMaterial);
            else
                SetMaterial(lightPairing.indicatorLight, offMaterial);
        }
    }

    private void SetMaterial(MeshRenderer lightToSet, Material lightState) { lightToSet.sharedMaterial = lightState; }

    private void FlashBetweenMaterials(MeshRenderer lightToFlash, Material flashStateOne, Material flashStateTwo)
    {
        lightToFlash.sharedMaterial = lightToFlash.sharedMaterial == flashStateOne ? flashStateTwo : flashStateOne;
    }
}

[System.Serializable]
public class IndicatorLightPairing
{
    public VehiclePart part;
    public MeshRenderer indicatorLight;
}
