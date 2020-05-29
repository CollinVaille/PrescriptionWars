using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot1 : Pill
{
    //Fix these guys later
    public GameObject corpsePrefab;

    //Status variables
    private Transform target, potentialTarget;
    private bool noAmmo = false;

    private NavMeshAgent agent;

    protected override void Start ()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();

        mainAudioSource.spatialBlend = 1;

        SetShadowsRecursively(transform, false);
    }

    /* public override void Damage (float amount)
    {
        base.Damage(amount);
    }   */

    protected override void Die ()
    {
        base.Die();

        //Create corpse dummy
        GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);

        //Make corpse dummy look like us!
        newCorpse.GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        newCorpse.GetComponent<Rigidbody>().velocity = rBody.velocity;

        //We go into secret hiding
        gameObject.SetActive(false);

        //Report our death
        spawner.ReportDeath(this, true);
    }

    public override void OnCreationFromSpawner (Spawner spawner)
    {
        base.OnCreationFromSpawner(spawner);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        //Move agent onto navmesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5, NavMesh.AllAreas))
            agent.Warp(hit.position);

        //Debug.Log(agent.Warp(hit.position).ToString() + ": " + (hit.position - transform.position).ToString());

        //Bootup BIOS on agent
        agent.enabled = true;
    }

    private bool NoInterruptions ()
    {
        return !dead && !controlOverride && !target && !potentialTarget;
    }

    private IEnumerator MindlessWander ()
    {
        //Wander loop
        while (NoInterruptions() && SameOrders(Squad.Orders.Roam))
        {
            if(Random.Range(0, 5) == 0)
            {
                Vector3 newDestination = transform.position;
                newDestination.x += Random.Range(5, 40) * (Random.Range(0, 2) == 0 ? 1 : -1);
                newDestination.z += Random.Range(5, 40) * (Random.Range(0, 2) == 0 ? 1 : -1);

                float arrivalRadius = Vector3.Distance(transform.position, newDestination) * 0.75f;

                yield return StartCoroutine(GoToPosition(newDestination, arrivalRadius, squad.GetOrdersID(), null));

                if (!NoInterruptions() || !SameOrders(Squad.Orders.Roam))
                    break;
            }

            //Randomly rotate slightly
            if(Random.Range(0, 3) == 0)
                transform.Rotate(Vector3.up, Random.Range(-20, 20), Space.World);

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        //Done with wandering around... now what?
        AfterFinishingOrders();
    }

    private IEnumerator PatrolAround (Vector3 patrolPosition, float patrolRadius)
    {
        int ordersID = squad.GetOrdersID();

        Vector3 currentDestination = patrolPosition;
        bool newDestination = true;

        //Patrol loop
        while (NoInterruptions() && SameOrdersID(ordersID))
        {
            if(newDestination)
            {
                yield return StartCoroutine(GoToPosition(currentDestination, 3, ordersID, null));

                if (!NoInterruptions() || !SameOrdersID(ordersID))
                    break;

                newDestination = false;
            }
            else if (Random.Range(0, 10) == 0) //Random chance for new target (otherwise pill will just sit at old one for now)
            {
                newDestination = true;

                currentDestination = patrolPosition;
                currentDestination.x += Random.Range(-patrolRadius, patrolRadius);
                currentDestination.z += Random.Range(-patrolRadius, patrolRadius);
            }

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        AfterFinishingOrders();
    }

    private IEnumerator FollowLeader (Transform leaderTransform, float followRadius)
    {
        int ordersID = squad.GetOrdersID();

        //Follow loop
        while (NoInterruptions() && SameOrdersID(ordersID))
        {
            if (GroundDistance(transform.position, leaderTransform.position) > followRadius) //Get closer to the follow target
            {
                yield return StartCoroutine(GoToPosition(leaderTransform.position, followRadius, ordersID, leaderTransform));

                if (!NoInterruptions() || !SameOrdersID(ordersID))
                    break;

                //Rotate to the follow target
                //transform.LookAt(leaderTransform);

                //Move forward
                //if (rBody.velocity.magnitude < moveSpeed)
                //    rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);
            }
            else if (Random.Range(0, 5) == 0) //Look around occassionally when stopped
            {
                if (Random.Range(0, 2) == 0) //Look at player
                    transform.LookAt(leaderTransform);
                else //Look randomly in another direction
                    transform.eulerAngles = new Vector3(0, Random.Range(0, 359), 0);
            }

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        AfterFinishingOrders();
    }

    private IEnumerator FallInFormation ()
    {
        int ordersID = squad.GetOrdersID();
        
        //Vector3 place = squad.GetPlaceInFormation(GetPill());

        //float lastChecked = Time.timeSinceLevelLoad;
        //Vector3 lastPosition = transform.position;

        /*
        //Go to formation loop
        while (!dead && !target && !potentialTarget && SameOrdersID(ordersID))
        {
            //Loop guard continued
            if (GroundDistance(transform.position, place) < 1)
                break;

            //Time to check if stuck
            if(Time.timeSinceLevelLoad - lastChecked > 1)
            {
                //Make sure we are not stuck
                if (GroundDistance(transform.position, lastPosition) < 1)
                {
                    //Rotate slightly
                    transform.Rotate(Vector3.up, Random.Range(-45, 45), Space.World);

                    //Thrust out of the way
                    rBody.AddForce(transform.forward * moveSpeed * 3, ForceMode.Impulse);
                }
                else
                {
                    //Look at place
                    transform.LookAt(place);

                    //Move forward
                    if (rBody.velocity.magnitude < moveSpeed)
                        rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

                    //Check for enemies
                    target = LookForEnemy();
                }

                lastChecked = Time.timeSinceLevelLoad;
                lastPosition = transform.position;
            }
            else
            {
                //Look at place
                transform.LookAt(place);

                //Move forward
                if (rBody.velocity.magnitude < moveSpeed)
                    rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

                //Check for enemies
                target = LookForEnemy();
            }

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }   */

        yield return StartCoroutine(GoToPosition(squad.GetPlaceInFormation(GetPill()), 1, ordersID, null));

        //Get in formation rotation
        squad.SetRotationInFormation(transform);

        //Stay in formation loop
        while (NoInterruptions() && SameOrdersID(ordersID))
        {
            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        //Done with formation... now what?
        AfterFinishingOrders();
    }

    private IEnumerator Standby ()
    {
        //Standby loop
        while(NoInterruptions() && SameOrders(Squad.Orders.Standby))
        {
            //Occassionally look around
            if(Random.Range(0, 10) == 0)
                transform.eulerAngles = new Vector3(0, Random.Range(0, 359), 0);

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        //Done with standby... now what?
        AfterFinishingOrders();
    }

    private IEnumerator Investigate ()
    {
        //Time variables
        float startTime = Time.timeSinceLevelLoad;
        float patience = Random.Range(4.0f, 11.0f);

        //Position variables
        float errorOffset = Vector3.Distance(transform.position, potentialTarget.position) * Random.Range(-0.5f, 0.5f);
        Vector3 suspectedPosition = potentialTarget.position + Vector3.one * errorOffset;

        potentialTarget = null;

        //Walk over to investigate
        while (!dead && !target && Time.timeSinceLevelLoad < startTime + patience)
        {
            //Loop guard continued
            if (Vector3.Distance(transform.position, suspectedPosition) < 4)
                break;

            //Rotate slightly
            transform.LookAt(suspectedPosition);

            //Move forward
            if (rBody.velocity.magnitude < moveSpeed)
                rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        //Final pause before giving up
        startTime = Time.timeSinceLevelLoad;
        patience = Random.Range(2.0f, 5.0f);
        while (!dead && !target && Time.timeSinceLevelLoad < startTime + patience)
        {
            //Loop guard continued
            if (Vector3.Distance(transform.position, suspectedPosition) < 4)
                break;

            //Look around
            if(Random.Range(0, 2) == 0)
                transform.eulerAngles = new Vector3(0, Random.Range(0, 359), 0);

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        //Done with investigating... now what?
        AfterFinishingOrders();
    }

    private void AfterFinishingOrders ()
    {
        //Figure out what to do next...

        if (!dead)
        {
            if (target) //Attack
            {
                if (holding.GetComponent<Gun>())
                    StartCoroutine(RangedAttack(target.GetComponent<Pill>()));
                else
                    StartCoroutine(MeleeAttack(target.GetComponent<Pill>()));
            }
            else if (potentialTarget) //Thought I heard something...
                StartCoroutine(Investigate());
            else if(!controlOverride)
                CarryOutOrders();
        }
    }

    private IEnumerator RangedAttack (Pill targetPill)
    {
        Transform targetTransform = targetPill.transform;
        Gun gun = holding.GetComponent<Gun>();

        noAmmo = gun.loadedBullets == 0 && gun.spareClips == 0;

        //Attack loop
        while (!noAmmo && MeAndBroAlive(targetPill))
        {
            //Go away from pill (want to be far enough away not to get stabbed)
            if(MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetTransform.position) < gun.range / 10)
            {
                float startTime = Time.timeSinceLevelLoad;
                float patience = Random.Range(3.0f, 7.0f);

                do
                {
                    //If it takes too long to gain separation then give up running away and take a stand
                    if (Time.timeSinceLevelLoad - startTime > patience)
                        break;

                    //Look away
                    transform.LookAt(targetTransform);
                    transform.Rotate(Vector3.up, 180);

                    //Move forward
                    if (rBody.velocity.magnitude < moveSpeed)
                        rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

                    //Wait fraction of a second
                    yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
                }
                while (MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetTransform.position) < gun.range / 10);
            }

            //Go towards pill (have to be in range to take shot)
            while (MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetTransform.position) >= gun.range / 5)
            {
                //Look at target
                transform.LookAt(targetPill.transform);

                //Move forward
                if (rBody.velocity.magnitude < moveSpeed)
                    rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

                //Wait fraction of a second
                yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
            }
            
            //Take a stand
            while (!noAmmo && MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetTransform.position) < gun.range / 5)
            {
                //Aim at target
                AimAt(targetTransform.position, !gun.aiming);

                //Check ammo (might take a few dry fires before they realize its empty)
                if (gun.loadedBullets == 0 && Random.Range(0, 2) == 0)
                {
                    //Completely out
                    if (gun.spareClips == 0)
                    {
                        noAmmo = true;
                        break;
                    }

                    //Reload
                    yield return StartCoroutine(gun.Reload());
                }
                else if (Random.Range(0, 10) < 3) //Chance to shoot
                    gun.Shoot();
                else if (Random.Range(0, 8) == 0) //Chance to aim
                    StartCoroutine(AimDownSightsImplement(Random.Range(0.25f, 1.0f)));

                //Wait fraction of a second
                yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
            }
        }

        if (!MeAndBroAlive(targetPill)) //Done with that guy
        {
            target = null;
            potentialTarget = null;

            if (!dead)
                CarryOutOrders();
        }
        else if (noAmmo) //Getting desperate
            StartCoroutine(MeleeAttack(targetPill));
    }

    private IEnumerator MeleeAttack (Pill targetPill)
    {
        //Attack loop
        while(MeAndBroAlive(targetPill))
        {
            //Go towards pill
            while (MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetPill.transform.position) > 1.5f)
            {
                //Look at target
                transform.LookAt(targetPill.transform);

                //Move forward
                if (rBody.velocity.magnitude < moveSpeed)
                    rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);

                //Wait fraction of a second
                yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
            }

            //Stand still, look at target, and stab
            while (MeAndBroAlive(targetPill) && GroundDistance(transform.position, targetPill.transform.position) <= 1.5f)
            {
                //Look at target
                AimAt(targetPill.transform.position, true);

                //Stab
                yield return StartCoroutine(holding.CheapStab());

                //Wait fraction of a second
                yield return new WaitForSeconds(Random.Range(0.25f, 0.75f));
            }

            //Reaction delay
            yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));
        }

        //Done with that guy
        target = null;
        potentialTarget = null;

        if (!dead)
            CarryOutOrders();
    }

    private IEnumerator GoToPosition (Vector3 targetPosition, float arrivalRadius, int ordersID, Transform positionUpdater)
    {
        bool usingAgent = StartNavigationTo(targetPosition);
        Collider oldNavZone = navigationZone;
        float lastUpdateTime = 0.0f;

        //Go until reach position or we have a target
        while (NoInterruptions() && SameOrdersID(ordersID))
        {
            if (GroundDistance(transform.position, targetPosition) <= arrivalRadius)
                break;

            //Periodically update target position if have position updater
            if(positionUpdater && Time.timeSinceLevelLoad > lastUpdateTime + Random.Range(1.25f, 2.25f))
            {
                lastUpdateTime = Time.timeSinceLevelLoad;
                
                if(Vector3.Distance(targetPosition, positionUpdater.position) > arrivalRadius)
                {
                    targetPosition = positionUpdater.position;

                    if(usingAgent)
                    {
                        agent.isStopped = true;
                        usingAgent = StartNavigationTo(targetPosition);
                    }
                }
            }

            //Make sure we're still making progress to target position
            if (usingAgent) //Agent-driven navigation
            {
                //Debug.Log("has path: " + agent.hasPath + ", velocity: " + agent.velocity.sqrMagnitude + ", stopped: " + 
                //    agent.isStopped);

                //Switch to primitive-driven navigation
                if (!agent.isOnNavMesh || agent.remainingDistance <= agent.stoppingDistance)
                {
                    //Debug.Log(name + " reached destination, switching to primitive");

                    //Disable agent
                    usingAgent = false;
                    agent.isStopped = true;
                    agent.enabled = false;
                }
            }
            else //Primitive-driven navigation
            {
                if(navigationZone && navigationZone != oldNavZone) //Switch to agent-driven navigation
                {
                    //Debug.Log(name + " entered nav zone, switching to agent");

                    oldNavZone = navigationZone;
                    usingAgent = StartNavigationTo(targetPosition);
                }
                else
                {
                    //Look at target position
                    transform.LookAt(targetPosition);

                    //Move forward
                    if (rBody.velocity.magnitude < moveSpeed)
                        rBody.AddForce(transform.forward * moveSpeed, ForceMode.Impulse);
                }
            }

            //Check for enemies
            target = LookForEnemy();

            //Wait fraction of a second
            yield return new WaitForSeconds(Random.Range(0.2f, 0.25f));
        }

        if(agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;

        agent.enabled = false;

        Debug.Log(transform.position.y);
    }

    //Returns the distance between the two vectors NOT factoring in their y value (so ignoring height difference)
    private float GroundDistance (Vector3 position1, Vector3 position2)
    {
        //a^2 + b^2 = c^2
        //so... c = sqrt(a^2 + b^2)
        return Mathf.Sqrt(Mathf.Pow(position1.x - position2.x, 2) + Mathf.Pow(position1.z - position2.z, 2));
    }

    private bool MeAndBroAlive (Pill bro) { return !dead && !bro.IsDead(); }

    //Performs LookAt with offset so that weapon is centered on target instead of our view
    //Also has built-in inaccuracy so bot is not aimbot!
    private void AimAt (Vector3 aimAt, bool fromHip)
    {
        //Aim view
        transform.LookAt(aimAt);

        //Hipfire offset
        if(fromHip)
            transform.Rotate(Vector3.up, Mathf.Max(-15 / GroundDistance(transform.position, aimAt), -15));

        //Introduce inaccuary
        transform.Rotate(Vector3.up, Random.Range(-10.0f, 10.0f));
    }

    //Call this to command agent to go to destination, returns false if impossible for agent to satisfy request
    private bool StartNavigationTo (Vector3 destination)
    {
        bool usingAgent = false;

        //Wake up agent and update it's position info
        agent.enabled = true;
        agent.Warp(transform.position);

        //Determine if we can use nav mesh agent and use it if we can
        if (agent.isOnNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(destination, path);

            if (path.status == NavMeshPathStatus.PathComplete) //Pill and destination are on nav mesh
            {
                usingAgent = true;
                agent.SetPath(path);
            }
            else //Pill is on nav mesh but destination is off nav mesh
            {
                if (navigationZone)
                {
                    Vector3 closestPoint = navigationZone.ClosestPoint(destination);
                    closestPoint.y = navigationZone.transform.position.y;

                    //Go to closest point on nav mesh to destination but only if we aren't already at that point
                    if (Vector3.Distance(transform.position, closestPoint) > agent.stoppingDistance)
                    {
                        usingAgent = true;

                        path = new NavMeshPath();
                        agent.CalculatePath(closestPoint, path);
                        agent.SetPath(path);
                    }
                }
            }
        }

        if (!usingAgent)
            agent.enabled = false;

        return usingAgent;
    }

    public override void Equip (Item item)
    {
        //Do the item switching
        base.Equip(item);

        //Set up new item
        if (holding)
        {
            //Positioning
            holding.transform.parent = transform;
            holding.transform.localPosition = new Vector3(0.5f, 0.25f, 0.0f);
            holding.transform.localRotation = Quaternion.Euler(0, 0, 0);

            //Get rid of shadows for optimization
            SetShadowsRecursively(holding.transform, false);
        }
    }

    public override void Equip (Item primary, Item secondary)
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

    private IEnumerator AimDownSightsImplement (float duration)
    {
        if (performingAction || !holding || holding.IsStabbing())
            yield break;

        performingAction = true;
        holding.GetComponent<Gun>().aiming = true;

        Item aimingDown = holding;

        //Move gun to center of body (and move it forward a little bit)
        holding.transform.localPosition = new Vector3(0.0f, 0.25f, 0.5f);

        //Wait
        yield return new WaitForSeconds(duration);

        //Return to original state
        if(aimingDown == holding)
        {
            //Return to original position
            holding.transform.localPosition = new Vector3(0.5f, 0.25f, 0.0f);

            //State info reset
            performingAction = false;
            holding.GetComponent<Gun>().aiming = false;
        }
    }

    //Sphere cast in front for possible target
    private Transform LookForEnemy ()
    {
        RaycastHit hit;

        if (Physics.SphereCast(transform.position, 4, transform.forward, out hit, 10))
        {
            if (hit.transform.GetComponent<Pill>() && hit.transform.GetComponent<Pill>().team != team)
                return hit.transform;
            else
                return null;
        }
        else
            return null;
    }

    public override void BringToLife ()
    {
        base.BringToLife();

        target = null;
        potentialTarget = null;

        CarryOutOrders();
    }

    public override void EquipGear (GameObject gear, bool forHead)
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
            while(gear.transform.childCount > 0)
                gear.transform.GetChild(0).parent = transform;

            Destroy(gear);
        }
        //Otherwise, gear actually is the gear so keep it
    }

    private void CarryOutOrders ()
    {
        if(squad.leader == GetPill()) //Squad leader actions
        {
            squad.SetOrders(Squad.Orders.Follow);

            StartCoroutine(MindlessWander());
        }
        else //Squad subordinate actions
        {
            Squad.Orders orders = squad.GetOrders();

            if (orders == Squad.Orders.Standby)
                StartCoroutine(Standby());
            else if (orders == Squad.Orders.Roam)
                StartCoroutine(MindlessWander());
            else if (orders == Squad.Orders.HoldPosition)
                StartCoroutine(PatrolAround(squad.leader.transform.position, squad.members.Count + Random.Range(5, 10)));
            else if (orders == Squad.Orders.Follow)
                StartCoroutine(FollowLeader(squad.leader.transform, Random.Range(8.0f, 11.0f)));
            else
                StartCoroutine(FallInFormation());
        }
    }

    public override void AlertOfAttacker (Pill attacker, bool softAlert)
    {
        if (dead)
            return;

        //Soft alert means we think there's an attacker but we are not sure... it's a ghost in the shadows
        //Hard alert means that fucker is right in front of our eyes

        if (softAlert)
            potentialTarget = attacker.transform;
        else //Hard alert
        {
            target = attacker.transform;
            squad.AlertSquadOfAttacker(attacker, GetPill(), Random.Range(5, 9));
        }
    }

    public override bool CanSleep ()
    {
        if (base.CanSleep() && !target && !potentialTarget)
        {
            if (!squad || squad.squadType == Squad.SquadType.Mobilized)
                return false;

            if (squad.squadType == Squad.SquadType.NightGuard && Planet.planet.GetTimeOfDay() == Planet.TimeOfDay.Morning)
                return true;
            
            return Planet.planet.GetTimeOfDay() == Planet.TimeOfDay.Evening;
        }
        else
            return false;
    }

    public override void Sleep (Bed bed)
    {
        StartCoroutine(BotSleep(bed));
    }

    private IEnumerator BotSleep (Bed bed)
    {
        //Figure out when to wake up based on when you went to bed
        Planet.TimeOfDay wakeUpAt = Planet.TimeOfDay.Morning;
        if (Planet.planet.GetTimeOfDay() == Planet.TimeOfDay.Morning)
            wakeUpAt = Planet.TimeOfDay.Evening;

        do
        {
            yield return new WaitForSeconds(0.2f);
        }
        while (!dead && !target && !potentialTarget && (Planet.planet.GetTimeOfDay() != wakeUpAt || Random.Range(0, 50) != 0));

        bed.WakeUp();
    }

    public override void WakeUp ()
    {
        if(!dead)
            CarryOutOrders();
    }
}
