using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //STATIC POOLING OF PROJECTILES----------------------------------------------------------------

    private static Dictionary<string, List<Projectile>> pooledProjectiles;

    public static void SetUpPooling ()
    {
        pooledProjectiles = new Dictionary<string, List<Projectile>>();
    }

    //Retrieve projectile from pool if one exists, else return newly created one
    public static Projectile GetProjectile (string projectileName)
    {
        pooledProjectiles.TryGetValue(projectileName, out List<Projectile> projectiles);

        Projectile projectile;

        //Happy day: list exists and is not empty
        //So return pooled projectile
        if (projectiles != null && projectiles.Count != 0)
        {
            projectile = projectiles[Random.Range(0, projectiles.Count)];
            projectiles.Remove(projectile);
            return projectile;
        }

        //Sad day: either list doesn't exist or is empty
        //So return newly created projectile
        projectile = Instantiate(Resources.Load<GameObject>("Projectiles/" + projectileName)).GetComponent<Projectile>();
        projectile.InitialSetUp(); //One-time initialization on creation
        return projectile;
    }

    //Put projectile in pool unless pool is full, in which case destroy projectile
    private static void PoolProjectile (Projectile projectile)
    {
        pooledProjectiles.TryGetValue(projectile.name, out List<Projectile> projectiles);

        if (projectiles == null) //No such pool, so create pool and add projectile to it
        {
            projectiles = new List<Projectile>(); //Create pool
            projectiles.Add(projectile); //Add projectile to pool
            pooledProjectiles.Add(projectile.name, projectiles); //Add pool to list of pools
        }
        else //Found projectile pool, so see if projectile fits...
        {
            if (projectiles.Count > 30) //Too many pooled so just destroy projectile
                projectile.DestroyProjectile();
            else //There's still room in pool, so put it in there
                projectiles.Add(projectile);
        }
    }

    //PROJECTILE INSTANCE-------------------------------------------------------------------------- 

    public enum ProjectileType { Default, Laser }

    //Launch parameters
    private float damage = 34;
    private float knockback = 200;
    private float range = 300;
    private Pill launcher;

    //Customization
    public ProjectileType projectileType = ProjectileType.Default;
    public float launchSpeed = 40;
    public string explosionName;

    //References
    private Rigidbody rBody;
    private AudioSource sfxSource;

    private float distanceCovered = 0.0f;

    private void InitialSetUp()
    {
        //Get references
        rBody = GetComponent<Rigidbody>();
        sfxSource = GetComponent<AudioSource>();

        //Makes pause menu pause/resume audio appropriately
        God.god.ManageAudioSource(sfxSource);

        //Remove (Clone) from end of name (necessary for pooling to work)
        name = name.Substring(0, name.Length - 7);
    }

    public void Launch(float damage, float knockback, float range, Pill launcher)
    {
        //First, remember our mission briefing
        this.damage = damage;
        this.knockback = knockback;
        this.range = range;
        this.launcher = launcher;

        //Then, theme the projectile's appearance to the faction color
        Reskin();

        //Thereafter, randomize initial rotation to make it look cooler
        transform.Rotate(Vector3.forward * Random.Range(0, 90), Space.Self);

        //Finally, proceed with launch!
        distanceCovered = 0.0f;
        gameObject.SetActive(true);

        //Bwwaaaaahhh
        if (sfxSource)
            sfxSource.Play();

        //Raahhhhhhhh
        if (rBody)
            rBody.AddRelativeForce(Vector3.forward * launchSpeed, ForceMode.VelocityChange);
        else
            God.god.ManageProjectile(this);
    }

    //Used to update the collision detection, movement, etc...
    public void UpdateLaunchedProjectile(float stepTime)
    {
        float stepDistance = launchSpeed * stepTime;

        //Check for collision
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stepDistance, ~0, QueryTriggerInteraction.Ignore))
            Impact(hit.transform);

        //Spin for cool effect
        transform.Rotate(Vector3.forward * stepTime * 720, Space.Self);

        //Move forward
        transform.Translate(Vector3.forward * stepDistance, Space.Self);

        //Time to put to pasture?
        distanceCovered += stepDistance;
        if (distanceCovered > range)
            Decommission();
    }

    //Called when the projectile hits something
    private void Impact(Transform hit)
    {
        Damageable victim = God.GetDamageable(hit);
        if (victim != null)
        {
            victim.Damage(damage, knockback, transform.position, DamageType.Projectile, launcher.team);

            if (Player.player.GetPill() == launcher)
                Player.player.GetAudioSource().PlayOneShot(Player.player.hitMarker);
        }

        if(!explosionName.Equals(""))
        {
            Explosion explosion = Explosion.GetExplosion(explosionName);
            explosion.transform.position = transform.position;
            explosion.transform.rotation = transform.rotation;
            explosion.Explode(launcher.team);
        }

        Decommission();
    }

    //Called to deactive the projectile and either... destroy it OR put it back in reserve pool
    private void Decommission()
    {
        //Silence
        if (sfxSource)
            sfxSource.Stop();

        //Stop updating
        if (rBody)
            rBody.velocity = Vector3.zero;
        else
            God.god.UnmanageProjectile(this);

        //Hide
        gameObject.SetActive(false);

        //Either pool or destroy
        PoolProjectile(this);
    }

    //Call this instead of Object.Destroy to ensure all needed clean up is performed
    private void DestroyProjectile()
    {
        God.god.UnmanageAudioSource(sfxSource);
        Destroy(gameObject);
    }

    private void Reskin()
    {
        if(projectileType == ProjectileType.Laser)
        {
            Army army = Army.GetArmy(launcher.team);
            if(army)
            {
                transform.Find("Top Face").GetComponent<Renderer>().sharedMaterial = army.plasma1;
                transform.Find("Side Face").GetComponent<Renderer>().sharedMaterial = army.plasma2;
            }
        }
    }
}
