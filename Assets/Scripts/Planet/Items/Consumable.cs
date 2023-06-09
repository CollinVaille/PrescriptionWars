using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Item
{
    public AudioClip consume;
    public Transform liquid;
    public int portions = 1;
    public List<PillEffect> effects;

    private int originalPortionCount;
    private float originalLiquidYScale = 1.0f, originalLiquidYPosition = 0.0f;

    private void Start()
    {
        originalPortionCount = portions;
        if(liquid)
        {
            originalLiquidYPosition = liquid.localPosition.y;
            originalLiquidYScale = liquid.localScale.y;
        }
    }

    public override void PrimaryAction()
    {
        if (!holder)
            return;

        StartCoroutine(ConsumePortion());
    }

    public override void PutInHand(PlanetPill newHolder)
    {
        base.PutInHand(newHolder);

        UpdatePortionCountOnUI();
    }

    public override void RetireFromHand()
    {
        DurabilityTextManager.ClearDurabilityText();

        base.RetireFromHand();
    }

    private IEnumerator ConsumePortion()
    {
        //Guard
        if (stabbing || !holder || holder.performingAction || portions <= 0)
            yield break;

        //Start
        holder.GetAudioSource().PlayOneShot(consume);
        holder.performingAction = true;

        //Rotate consumable inward to get a bite/drink/pill/portion
        int x;
        for (x = 0; x < 10 && holder; x++)
	    {
                transform.Rotate(Vector3.forward * 6.0f);
                yield return new WaitForSeconds(0.03f);
        }

        //Consume one portion
        if (holder)
            InstantlyConsumePortion();

        //Rotate consumable back to normal rotation
        for (; x > 0 && holder; x--)
        {
            transform.Rotate(Vector3.forward * -6.0f);
            yield return new WaitForSeconds(0.02f);
        }

        //Cleanup
        if (holder)
        {
            holder.performingAction = false;
            transform.localEulerAngles = Vector3.zero;
        }
    }

    private void InstantlyConsumePortion()
    {
        portions--;
        UpdatePortionCountOnUI();
        UpdateLiquidVisual();

        if (effects != null)
        {
            foreach (PillEffect effect in effects)
            {
                if (effect != null)
                    effect.ApplyEffect(holder);
            }
        }
    }

    private void UpdateLiquidVisual()
    {
        if (!liquid)
            return;

        if (portions == 0)
            liquid.gameObject.SetActive(false);
        else
        {
            liquid.gameObject.SetActive(true);

            //Calculate lerp value
            float lerpValue = (portions * 1.0f) / originalPortionCount;
            float fullyDrainedYPosition = originalLiquidYPosition - originalLiquidYScale;

            //Lerp position
            Vector3 liquidLocalPosition = liquid.localPosition;
            liquidLocalPosition.y = Mathf.Lerp(fullyDrainedYPosition, originalLiquidYPosition, lerpValue);
            liquid.localPosition = liquidLocalPosition;

            //Lerp scale
            Vector3 liquidLocalScale = liquid.localScale;
            liquidLocalScale.y = Mathf.Lerp(0.0f, originalLiquidYScale, lerpValue);
            liquid.localScale = liquidLocalScale;
        }   
    }

    private void UpdatePortionCountOnUI()
    {
        if (holderIsPlayer)
            DurabilityTextManager.SetDurabilityText(portions, originalPortionCount);
    }

    public override Vector3 GetPlaceInPlayerHand() { return new Vector3(0.5f, -0.25f, 0.5f); }
}
