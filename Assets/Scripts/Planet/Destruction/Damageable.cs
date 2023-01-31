﻿using UnityEngine;

public enum DamageType { Melee, Scrape, Projectile, Explosive, Fire, HullCompromised, ImpactSpeed }

public interface Damageable
{
    void Damage(float damage, float knockback, Vector3 from, DamageType damageType, int team);
}
