using UnityEngine;

public enum DamageType { Melee, Scrape, Projectile, Explosive, Fire, HullCompromised, ImpactSpeed, Poison }

public interface IDamageable
{
    void Damage(float damage, float knockback, Vector3 from, DamageType damageType, int team);
}
