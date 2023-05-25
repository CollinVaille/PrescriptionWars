using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillEffect
{
    public enum PillEffectType { NoEffect, Poison, Heal, Ignite, Extinguish, Combust, LaunchUpwards }

    public PillEffectType effectType = PillEffectType.NoEffect;
    public float magnitude = 0.0f;

    public void ApplyEffect(Pill subject)
    {
        if (!subject)
            return;

        switch(effectType)
        {
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
        }
    }
}
