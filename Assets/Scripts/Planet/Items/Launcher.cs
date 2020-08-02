using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : Gun
{
    public GameObject projectilePrefab;

    protected override void EjectProjectile()
    {
        Projectile projectile = GetProjectile();

        //Put projectile in launch position
        projectile.transform.rotation = transform.rotation;
        projectile.transform.position = transform.position;
        projectile.transform.Translate(Vector3.forward, Space.Self);

        //Launch time!
        projectile.SetUp();
        projectile.Launch(bulletDamage, bulletKnockback, range, holder.team, holderIsPlayer);
    }

    private Projectile GetProjectile()
    {
        return Instantiate(projectilePrefab).GetComponent<Projectile>();
    }
}
