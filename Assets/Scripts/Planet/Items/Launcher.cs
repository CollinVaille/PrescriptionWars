using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : Gun
{
    public GameObject projectilePrefab;

    protected override void EjectProjectile()
    {
        Projectile projectile = GetProjectile();

        projectile.transform.rotation = transform.rotation;
        projectile.transform.position = transform.position;
        projectile.transform.Translate(Vector3.forward, Space.Self);

        projectile.SetUp();
        projectile.Launch(bulletDamage, bulletKnockback, range, holder.team);
    }

    private Projectile GetProjectile()
    {
        return Instantiate(projectilePrefab).GetComponent<Projectile>();
    }
}
