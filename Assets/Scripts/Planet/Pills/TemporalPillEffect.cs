using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporalPillEffect : MonoBehaviour
{
    public PillEffect.PillEffectType effectType = PillEffect.PillEffectType.NoEffect;
    public float magnitude = 0.0f;

    private float updateInterval = 2.0f, timeLeft = 10.0f;
    private int lifeNumber;
    private bool notYetDoneFirstUpdate = true;
    private Pill subject;
    private GameObject referenceSlot;

    public void ApplyEffect(Pill subject)
    {
        this.subject = subject;

        if (subject)
            StartCoroutine(ApplyEffectLoop());
        else
            EndEffect(); //Subject is required in order to have an effect
    }

    public void ResetTimer()
    {
        timeLeft = 10.0f;
    }

    public void EndEffect()
    {
        if(!notYetDoneFirstUpdate)
            ApplyEffectUpdate(false, true);

        subject = null;
        Destroy(this);
    }

    private IEnumerator ApplyEffectLoop()
    {
        lifeNumber = subject.GetLifeNumber();

        while (timeLeft > 0.0f && subject && subject.GetLifeNumber() == lifeNumber)
        {
            ApplyEffectUpdate(notYetDoneFirstUpdate, false);
            if(notYetDoneFirstUpdate)
                notYetDoneFirstUpdate = false;

            yield return new WaitForSeconds(updateInterval);
            timeLeft -= updateInterval;
        }

        if(subject)
            EndEffect();
    }

    private void ApplyEffectUpdate(bool firstUpdate, bool lastUpdate)
    {
        switch(effectType)
        {
            case PillEffect.PillEffectType.SpeedUp:
                if (firstUpdate)
                {
                    subject.GetRigidbody().velocity *= magnitude;
                    subject.moveSpeed *= magnitude;

                    if (subject.IsPlayer)
                        subject.GetComponent<Player>().GetFeetAudioSource().pitch *= magnitude;
                }
                else if (lastUpdate)
                {
                    subject.moveSpeed /= magnitude;

                    if (subject.IsPlayer)
                        subject.GetComponent<Player>().GetFeetAudioSource().pitch /= magnitude;
                }
                return;

            case PillEffect.PillEffectType.Seizure:
                updateInterval = magnitude * Random.Range(0.75f, 1.25f);

                float rand = Random.Range(0.0f, 1.0f);
                if(rand < 0.2 && !lastUpdate)
                {
                    if(!referenceSlot)
                        referenceSlot = Instantiate(Resources.Load<GameObject>("Planet/UI/Seizure Image"), PlanetPauseMenu.pauseMenu.HUD);
                }
                else
                {
                    if (referenceSlot)
                    {
                        Destroy(referenceSlot);
                        referenceSlot = null;
                    }

                    if (rand < 0.4f)
                    {
                        Item itemInHand = subject.GetItemInHand();
                        if (itemInHand)
                        {
                            if (Random.Range(0.0f, 1.0f) < 0.5f)
                                itemInHand.PrimaryAction();
                            else
                                itemInHand.SecondaryAction();
                        }
                    }
                    else if (rand < 0.6f)
                        subject.Jump();
                    else if (rand < 0.8f)
                        subject.Equip(null, dropOldItem: true);
                    else
                        PillEffect.ApplyEffect(subject, PillEffect.PillEffectType.Poison, Random.Range(1.0f, 10.0f));
                }
                return;
        }
    }
}
