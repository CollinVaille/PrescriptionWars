using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillEffect
{
    public enum PillEffectType { NoEffect, Poison, Heal, Ignite, Extinguish, Combust, LaunchUpwards, DropItem, SpeedUp, Seizure }

    public PillEffectType effectType = PillEffectType.NoEffect;
    public float magnitude = 0.0f;

    public void ApplyEffect(PlanetPill subject)
    {
        ApplyEffect(subject, effectType, magnitude);
    }

    public static void ApplyEffect(PlanetPill subject, PillEffectType effectType, float magnitude)
    {
        if (!subject)
            return;

        switch(effectType)
        {
            case PillEffectType.NoEffect:
                return;

            case PillEffectType.Poison:
                subject.Damage(magnitude, 0.0f, subject.transform.position, DamageType.Poison, -1);
                return;

            case PillEffectType.Heal:
                subject.Heal(magnitude, false);
                return;

            case PillEffectType.Ignite:
                Fire.SetOnFire(subject.transform, Vector3.zero, magnitude);
                return;

            case PillEffectType.Extinguish:
                Fire subjectFire = Fire.GetSubjectFire(subject.transform);
                if(subjectFire)
                    subjectFire.intensity *= magnitude;
                return;

            case PillEffectType.Combust:
                Explosion.CreateGenericExplosion(subject.transform.position, -1, damage: magnitude);
                return;

            case PillEffectType.LaunchUpwards:
                subject.GetRigidbody().AddForce(Vector3.up * magnitude, ForceMode.Impulse);
                return;

            case PillEffectType.DropItem:
                subject.Equip(null, dropOldItem: true);
                return;

            default:
                ApplyAsTemporalPillEffect(subject, effectType, magnitude);
                return;
        }
    }

    private static void ApplyAsTemporalPillEffect(PlanetPill subject, PillEffectType effectType, float magnitude)
    {
        TemporalPillEffect[] temporalPillEffects = subject.GetComponents<TemporalPillEffect>();

        if(temporalPillEffects != null)
        {
            TemporalPillEffect temporalPillEffect;
            for (int x = 0; x < temporalPillEffects.Length; x++)
            {
                temporalPillEffect = temporalPillEffects[x];

                if(temporalPillEffect.effectType == effectType)
                {
                    temporalPillEffect.ResetTimer(); //Keep the existing effect and just reset its timer
                    return;
                }
            }

            //Else, we do not have the effect already so add it
            temporalPillEffect = subject.gameObject.AddComponent<TemporalPillEffect>();
            temporalPillEffect.effectType = effectType;
            temporalPillEffect.magnitude = magnitude;
            temporalPillEffect.ApplyEffect(subject);
        }
    }
}
