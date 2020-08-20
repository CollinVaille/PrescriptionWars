using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pill : MonoBehaviour, Damageable
{
    private static string[] pillNames;

    //References
    protected Rigidbody rBody;
    protected AudioSource mainAudioSource;
    protected Spawner spawner;
    protected Collider navigationZone;
    public Squad squad;
    protected Item holding = null;
    public Voice voice = null;

    //Status variables
    public int team = 0;
    public float moveSpeed = 10;
    protected float health = 100, maxHealth;
    protected bool dead = false, touchingWater = false, controlOverride = false;
    [HideInInspector] public bool performingAction = false, onFire = false;
    [HideInInspector] public Vector3 spawnPoint = Vector3.zero;

    protected virtual void Start ()
    {
        //Set references
        rBody = GetComponent<Rigidbody>();
        mainAudioSource = GetComponent<AudioSource>();
        voice = Voice.GetVoice("Carmine");

        //Set status variables
        maxHealth = health;

        name = GetRandomPillName();
    }

    public virtual void Damage (float damage, float knockback, Vector3 from, DamageType damageType, int team)
    {
        if (dead || damage <= 0)
            return;

        if (voice)
            Say(voice.GetOof(), damageType != DamageType.Fire, Random.Range(0.2f, 0.4f));

        //Friendly fire is prevented!
        if (this.team == team)
        {
            if (damageType == DamageType.Melee)
                mainAudioSource.PlayOneShot(God.god.jab);

            return;
        }

        //Knockback, if any
        if (knockback > 0)
            rBody.AddExplosionForce(knockback, from, 1000);

        //Apply damage
        health -= damage;

        if (health <= 0)
        {
            health = 0;
            dead = true;

            Die();
        }
    }

    public virtual void Heal (float amount, bool fromExternalSource = true)
    {
        if (dead || amount <= 0)
            return;

        health += amount;

        if (health > maxHealth)
            health = maxHealth;

        if (fromExternalSource && voice && Random.Range(0, 10) == 0)
        {
            if (health > maxHealth - 1)
                Say(voice.GetImGood(), false);
            else
                Say(voice.GetThanks(), false);
        }
    }

    //Should only be called by Damage function
    protected virtual void Die ()
    {
        //Stop any override behaviours such as sleeping in bed, sitting in seat, climbing a ladder...
        controlOverride = false;

        //Report the sad news
        squad.ReportingDeparture(this);
        spawner.ReportDeath(this, true);
    }

    protected void OnCollisionEnter (Collision collision)
    {
        int layerHit = collision.GetContact(0).thisCollider.gameObject.layer;

        //Pointy (layer 8) and blunt (layer 9) objects can damage pills upon contact
        if (holding && (layerHit == 8 || layerHit == 9))
        {
            Damageable hitObject = collision.GetContact(0).otherCollider.GetComponent<Damageable>();
            Pill hitPill = collision.GetContact(0).otherCollider.GetComponent<Pill>();

            if (hitObject != null)
            {
                if (holding.IsStabbing())
                {
                    //Apply damage and knockback
                    hitObject.Damage(holding.meleeDamage, holding.meleeKnockback, transform.position, DamageType.Melee, team);

                    if(hitPill && hitPill.team != team)
                    {
                        hitPill.AlertOfAttacker(this, false);
                        AlertOfAttacker(hitPill, false);

                        //Play stab sound effect (often times pills stab each other to death at same time)
                        if (gameObject.activeInHierarchy) //... so this surpresses the warning for playing from disabled source
                            mainAudioSource.PlayOneShot(holding.stab);
                    }
                }
                else if (layerHit == 8) //Pointy objects (layer 8) can scrape when not stabbing (but not blunt objects)
                {
                    //Apply damage
                    hitObject.Damage(holding.meleeDamage / 2, 0, Vector3.zero, DamageType.Scrape, team);

                    //Play scrape sound effect
                    if (gameObject.activeInHierarchy)
                        mainAudioSource.PlayOneShot(holding.scrape);

                    if (hitPill && hitPill.team != team)
                    {
                        hitPill.AlertOfAttacker(this, false);
                        AlertOfAttacker(hitPill, false);
                    }
                }
            }
            else if (holding.IsStabbing())
            {
                if (collision.gameObject.GetComponent<Pill>()) //Hit gear of some jackwagon
                    mainAudioSource.PlayOneShot(God.god.deflection);
                else //Hit wall or something
                    mainAudioSource.PlayOneShot(God.god.genericImpact);
            }
        }
    }

    public bool IsDead () { return dead; }

    public Pill GetPill () { return this; }

    public AudioSource GetAudioSource () { return mainAudioSource; }

    public virtual void Equip (Item item, bool dropOldItem = true)
    {
        //Unequip previous item so we have room to equip new item
        if (holding)
        {
            //Relocate it
            holding.transform.Translate(Vector3.down * 0.25f, Space.Self);
            holding.transform.parent = null;

            if(dropOldItem)
            {
                //Give it a rigidbody
                holding.gameObject.AddComponent<Rigidbody>();

                //Can now grab it off ground
                holding.gameObject.layer = 10;

                //Need trigger collider again since we're putting it back on ground
                if (holding.GetComponent<Collider>())
                    holding.GetComponent<Collider>().enabled = true;
            }

            //Set status stuff (do last)
            holding.RetireFromHand();
            holding = null;
        }

        //Equip new item if we have one
        if (item)
        {
            //Set status stuff (do first)
            holding = item;
            holding.PutInHand(GetPill());

            //Remove rigidbody
            if (holding.GetComponent<Rigidbody>())
                Destroy(holding.GetComponent<Rigidbody>());

            //Can no longer grab it off ground
            holding.gameObject.layer = 0;

            //Disabling trigger collider for performance (collider used for identification when on ground)
            if (holding.GetComponent<Collider>())
                holding.GetComponent<Collider>().enabled = false;

            //Equip sound effect
            mainAudioSource.PlayOneShot(holding.equip);
        }
    }

    public virtual void Equip (Item primary, Item secondary) { }

    public bool IsGrounded (float errorTolerance)
    {
        float distance = GetComponent<Collider>().bounds.extents.y + errorTolerance;

        return Physics.Raycast(transform.position, Vector3.down, distance);
    }

    public void Jump ()
    {
        if(IsGrounded(0.1f) || touchingWater)
            rBody.AddForce(Vector3.up * 5, ForceMode.Impulse);
    }

    public virtual void AimDownSights () { }

    public virtual void RaiseShield () { }

    protected void SetShadowsRecursively (Transform root, bool shadowsOn)
    {
        Renderer theRenderer = root.GetComponent<Renderer>();

        //Set shadows for root object
        if (theRenderer)
        {
            //Set casting
            if(shadowsOn)
                theRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                theRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            //Set receiving
            theRenderer.receiveShadows = shadowsOn;
        }

        //Recursively set it for children
        foreach (Transform child in root)
            SetShadowsRecursively(child, shadowsOn);
    }

    public virtual void OnCreationFromSpawner (Spawner spawner)
    {
        this.spawner = spawner;
        team = spawner.team;
        
        //So the player can read the name tags of friendly pills
        if (team == Player.playerTeam)
        {
            if (!GetComponent<Player>())
                gameObject.layer = 14;
        }
        else
            gameObject.layer = 15;

        SnapToGround();
    }

    public virtual void BringToLife ()
    {
        performingAction = false;
        health = maxHealth;

        dead = false;

        //Extinguish any fires from a previous life, the future cannot be weighed down by burdens of the past
        if(onFire)
        {
            Destroy(transform.GetComponentInChildren<Fire>().gameObject);
            onFire = false;
        }

        gameObject.SetActive(true);
    }

    public virtual void EquipGear (GameObject gear, bool forHead) { }

    public virtual void Submerge (Collider water)
    {
        touchingWater = true;
    }

    public virtual void Emerge (Collider water)
    {
        touchingWater = false;
    }

    public virtual bool RaycastShoot (Transform from, int range, out RaycastHit hit)
    {
        return Physics.Raycast(from.position + from.forward, from.forward, out hit, range,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    public virtual void AlertOfAttacker (Pill attacker, bool softAlert) { }

    protected bool SameOrders (Squad.Orders orders)
    {
        if (squad.leader == this)
            return squad.GetOrders() != Squad.Orders.Standby;
        else
            return orders == squad.GetOrders();
    }

    protected bool SameOrdersID (int ordersID)
    {
        if (squad.leader == this)
            return squad.GetOrders() != Squad.Orders.Standby;
        else
            return ordersID == squad.GetOrdersID();
    }

    public void SetNavigationZone (Collider navigationZone) { this.navigationZone = navigationZone; }

    public void RemoveNavigationZone (Collider navigationZone)
    {
        if (this.navigationZone == navigationZone)
            this.navigationZone = null;
    }

    public virtual bool CanOverride () { return !dead && !controlOverride; }

    public virtual void OverrideControl (Interactable overrider) { controlOverride = true; }

    public virtual void ReleaseOverride () { controlOverride = false; }

    private void SnapToGround ()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10))
        {
            Vector3 adjustedSpawn = hit.point;
            adjustedSpawn.y += transform.GetComponent<Collider>().bounds.size.y / 2.0f;
            transform.position = adjustedSpawn;
        }
    }

    public Rigidbody GetRigidbody () { return rBody; }

    public Item GetItemInHand () { return holding; }

    private string GetRandomPillName ()
    {
        string pillName = "";

        //Get list of pill names
        if (pillNames == null)
        {
            pillNames = GeneralHelperMethods.GetLinesFromFile(Random.Range(0, 2) == 0 ?
                "Pill Names/Pill Names" : "Pill Names/Boring Names", false);
        }

        //Randomly choose pill name
        pillName = pillNames[Random.Range(0, pillNames.Length)];

        if (Random.Range(0, 2) == 0)
            pillName = "Pvt. " + pillName;
        else if (Random.Range(0, 2) == 0)
            pillName = "Cpl. " + pillName;
        else if (Random.Range(0, 2) == 0)
            pillName = "Sgt. " + pillName;
        else
            pillName = "Lt. " + pillName;

        return pillName;
    }

    public void Say(AudioClip dialogue, bool interruptPrevious, float delay = 0.0f)
    {
        //Deal with possibility of interrupting dialogue that is still playing
        if(mainAudioSource.isPlaying)
        {
            if (interruptPrevious)
                mainAudioSource.Stop();
            else
                return;
        }

        //Play new dialogue
        mainAudioSource.clip = dialogue;
        if (delay > 0)
            mainAudioSource.PlayDelayed(delay);
        else
            mainAudioSource.Play();
    }
}
