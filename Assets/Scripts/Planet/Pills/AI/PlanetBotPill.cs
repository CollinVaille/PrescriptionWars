using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetBotPill : PlanetPill
{
    private PlanetBotNavigationController navigation;
    private IDestroyableTarget target, potentialTarget;

    protected override void Awake()
    {
        base.Awake();
        navigation = new PlanetBotNavigationController(this);
    }

    protected override void Start()
    {
        base.Start();
        mainAudioSource.spatialBlend = 1;
        SetShadowsRecursively(transform, false);
    }

    //Should only be called by Damage function
    protected override void Die()
    {
        base.Die();

        //We go into secret hiding
        gameObject.SetActive(false);

        //Bot spawns corpse dummy on death whereas player spawns his on respawn
        Spawner.SpawnCorpse(God.god.corpsePrefab, transform);
    }

    public override void BringToLife()
    {
        base.BringToLife();

        target = null;
        potentialTarget = null;

        //CarryOutOrders();
        StartCoroutine(GoToPosition(Vector3.zero, 5.0f, squad.GetOrdersID(), null));
    }

    public override void Equip(Item item, bool dropOldItem = true)
    {
        //Do the item switching
        base.Equip(item, dropOldItem);

        //Set up new item
        if (holding)
        {
            //Positioning
            holding.transform.parent = transform;
            holding.transform.localPosition = holding.GetPlaceInPlayerHand() + Vector3.up * 0.5f;
            holding.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //Get rid of shadows for optimization
            SetShadowsRecursively(holding.transform, false);
        }
    }

    public override void Equip(Item primary, Item secondary)
    {
        if (!primary)
            Equip(secondary);
        else
        {
            Equip(primary);

            if (secondary)
                Destroy(secondary.gameObject);
        }
    }

    public override void EquipGear(GameObject gear, bool forHead)
    {
        if (!gear)
            return;

        //Get rid of shadows for optimization
        SetShadowsRecursively(gear.transform, false);

        //Parenting and rotation
        gear.transform.parent = transform;
        gear.transform.localRotation = Quaternion.Euler(0, 0, 0);

        //Position
        if (forHead)
            gear.transform.localPosition = Vector3.up * 0.5f;
        else
            gear.transform.localPosition = Vector3.zero;

        //Gear is just holding a bunch of children so put them under our control and then delete gear
        if (gear.transform.childCount > 0)
        {
            while (gear.transform.childCount > 0)
                gear.transform.GetChild(0).parent = transform;

            Destroy(gear);
        }
        //Otherwise, gear actually is the gear so keep it
    }

    private bool NoInterruptions()
    {
        return !dead && !controlOverride && target == null && potentialTarget == null;
    }

    //Sphere cast in front for possible target
    private IDestroyableTarget LookForEnemy()
    {
        RaycastHit hit;

        if (Physics.SphereCast(transform.position, 4, transform.forward, out hit, 10))
        {
            PlanetPill hitPill = hit.transform.GetComponent<PlanetPill>();
            if (hitPill && hitPill.team != team)
                return hitPill;
            else
                return null;
        }
        else
            return null;
    }

    private IEnumerator GoToPosition(Vector3 targetPosition, float arrivalRadius, int ordersID, Transform positionUpdater)
    {
        navigation.SetNavigationTarget(positionUpdater, targetPosition, arrivalRadius);

        //Go until reach position or we have a target
        while (NoInterruptions() && SameOrdersID(ordersID))
        {
            //if (PlanetBotNavigationController.GroundDistance(transform.position, targetPosition) <= arrivalRadius)
            //    break;

            navigation.PerformUpdate();

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        navigation.Stop();
        //Debug.Log("!dead: " + (!dead) + ", !override: " + (!controlOverride) + ", !target: " +  (target == null) + ", !potential target: " + (potentialTarget == null) + ", same orders: " + SameOrdersID(ordersID));
    }
}
