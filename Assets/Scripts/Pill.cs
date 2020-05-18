using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pill : MonoBehaviour
{
    //References
    protected Rigidbody rBody;
    protected AudioSource mainAudioSource;
    protected Spawner spawner;
    protected Collider navigationZone;
    public Squad squad;

    //Status variables
    public int team = 0;
    public float moveSpeed = 10;
    protected float health = 100, maxHealth;
    protected bool dead = false, touchingWater = false;
    [HideInInspector] public bool performingAction = false, controlOverride = false;
    [HideInInspector] public Vector3 spawnPoint = Vector3.zero;

    protected Item holding = null;

    protected virtual void Start ()
    {
        //Set references
        rBody = GetComponent<Rigidbody>();
        mainAudioSource = GetComponent<AudioSource>();

        //Set status variables
        maxHealth = health;

        name = GetRandomPillName();
    }

    public virtual void Damage (float amount)
    {
        if (dead || amount <= 0)
            return;

        health -= amount;

        if(health <= 0)
        {
            health = 0;
            dead = true;

            Die();
        }
    }

    protected virtual void Die ()
    {
        controlOverride = false;

        squad.ReportingDeparture(this);
    }

    public virtual void Heal (float amount)
    {
        if (dead || amount <= 0)
            return;

        health += amount;

        if (health > maxHealth)
            health = maxHealth;
    }

    protected void OnCollisionEnter (Collision collision)
    {
        int layerHit = collision.GetContact(0).thisCollider.gameObject.layer;

        //Pointy (layer 8) and blunt (layer 9) objects can damage pills upon contact
        if (holding && (layerHit == 8 || layerHit == 9))
        {
            Pill hitPill = collision.GetContact(0).otherCollider.GetComponent<Pill>();

            if (hitPill)
            {
                //Hit enemy
                if (hitPill.team != team)
                {
                    if (holding.IsStabbing())
                    {
                        //Apply damage and knockback
                        hitPill.ApplyHit(holding.meleeDamage, holding.meleeKnockback, transform.position);

                        hitPill.AlertOfAttacker(this, false);
                        AlertOfAttacker(hitPill, false);

                        //Play stab sound effect (often times pills stab each other to death at same time)
                        if (gameObject.activeInHierarchy) //... so this surpresses the warning for playing from disabled source
                            mainAudioSource.PlayOneShot(holding.stab);
                    }
                    else if (layerHit == 8) //Pointy objects (layer 8) can scrape when not stabbing (but not blunt objects)
                    {
                        //Apply damage
                        hitPill.ApplyHit(holding.meleeDamage / 2, 0, Vector3.zero);

                        hitPill.AlertOfAttacker(this, false);
                        AlertOfAttacker(hitPill, false);

                        //Play scrape sound effect
                        if (gameObject.activeInHierarchy)
                            mainAudioSource.PlayOneShot(holding.scrape);
                    }
                }
                else if (holding.IsStabbing()) //Stabbed teammate
                    mainAudioSource.PlayOneShot(God.god.jab);
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

    public void ApplyHit (float damage, float knockback, Vector3 from)
    {
        if(knockback > 0)
            rBody.AddExplosionForce(knockback, from, 1000);

        Damage(damage);
    }

    public bool IsDead () { return dead; }

    public Pill GetPill () { return this; }

    public AudioSource GetAudioSource () { return mainAudioSource; }

    public virtual void Equip (Item item)
    {
        //Unequip previous item so we have room to equip new item
        if (holding)
        {
            //Relocate it
            holding.transform.Translate(Vector3.down * 0.25f, Space.Self);
            holding.transform.parent = null;

            //Give it a rigidbody
            holding.gameObject.AddComponent<Rigidbody>();

            //Can now grab it off ground
            holding.gameObject.layer = 10;

            //Need trigger collider again since we're putting it back on ground
            if (holding.GetComponent<Collider>())
                holding.GetComponent<Collider>().enabled = true;

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

    public void OnCreationFromSpawner (Spawner spawner)
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
    }

    public virtual void BringToLife ()
    {
        performingAction = false;
        health = maxHealth;

        dead = false;

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
        return Physics.Raycast(from.position + from.forward, from.forward, out hit, range);
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

    public virtual bool CanSleep () { return !dead && !controlOverride; }

    public virtual void Sleep (Bed bed) { }

    public virtual void WakeUp () { }

    private string GetRandomPillName ()
    {
        string pillName = "";

        int picker = Random.Range(0, 62);

        switch (picker)
        {
            case 0: pillName = "Abilify"; break;
            case 1: pillName = "Accutane"; break;
            case 2: pillName = "Acetaminophen"; break;
            case 3: pillName = "Aspirin"; break;
            case 4: pillName = "Codeine"; break;
            case 5: pillName = "Tylenol"; break;
            case 6: pillName = "Cyanide"; break;
            case 7: pillName = "Acular"; break;
            case 8: pillName = "Advil"; break;
            case 9: pillName = "Adenosine"; break;
            case 10: pillName = "Epinephrine"; break;
            case 11: pillName = "Benadryl"; break;
            case 12: pillName = "Bevyxxa"; break;
            case 13: pillName = "Targretrin"; break;
            case 14: pillName = "Buprenorphine"; break;
            case 15: pillName = "Bumex"; break;
            case 16: pillName = "Sotalol"; break;
            case 17: pillName = "Betapace"; break;
            case 18: pillName = "Lotensin"; break;
            case 19: pillName = "Benazepril"; break;
            case 20: pillName = "Baraclude"; break;
            case 21: pillName = "Celebrex"; break;
            case 22: pillName = "Capoten"; break;
            case 23: pillName = "Diazepam"; break;
            case 24: pillName = "Bentyl"; break;
            case 25: pillName = "Dicyclomine"; break;
            case 26: pillName = "Methadone"; break;
            case 27: pillName = "Dolophine"; break;
            case 28: pillName = "Morphine"; break;
            case 29: pillName = "Drisdol"; break;
            case 30: pillName = "Hydrocodone"; break;
            case 31: pillName = "Hydrochlorothiazide"; break;
            case 32: pillName = "Montelukast"; break;
            case 33: pillName = "Singulair"; break;
            case 34: pillName = "Crestor"; break;
            case 35: pillName = "Vyvanse"; break;
            case 36: pillName = "Januvia"; break;
            case 37: pillName = "Adderall"; break;
            case 38: pillName = "Insulin Glargine"; break;
            case 39: pillName = "Levothyroxine"; break;
            case 40: pillName = "Synthroid"; break;
            case 41: pillName = "Decongestant"; break;
            case 42: pillName = "Ibuprofen"; break;
            case 43: pillName = "Roids"; break;
            case 44: pillName = "Viagra"; break;
            case 45: pillName = "Penicillin"; break;
            case 46: pillName = "Valium"; break;
            case 47: pillName = "EZ Nite Sleep"; break;
            case 48: pillName = "Xanax"; break;
            case 49: pillName = "Klonopin"; break;
            case 50: pillName = "Vicodin"; break;
            case 51: pillName = "Ketamine"; break;
            case 52: pillName = "Tramadol"; break;
            case 53: pillName = "Motrin"; break;
            case 54: pillName = "Antihistamine"; break;
            case 55: pillName = "Zyrtec"; break;
            case 56: pillName = "Xyzal"; break;
            case 57: pillName = "Allegra"; break;
            case 58: pillName = "Lithium"; break;
            case 59: pillName = "Diphenhydramine"; break;
            case 60: pillName = "Aller-Tec"; break;
            default: pillName = "Placebo"; break;
        }

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
}
