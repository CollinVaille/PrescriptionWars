using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    public GameObject playerPrefab, pillPrefab, squadPrefab;

    private Squad squad = null;
    public Squad.SquadType squadType = Squad.SquadType.Mobilized;

    //Pill class variables
    public PillClass[] pillClasses;
    private float totalProbability = 0;
    
    //Spawner limitations
    public int atOnce = 5, prescriptions = 20;
    private int alive = 0;

    //Team info
    public int team = 0;

    //*Death* -> Death queue -> Respawn queue -> *Alive*
    private List<Pill> deathQueue, respawnQueue;

    //Other status variables
    private City city = null;

    private void Start ()
    {
        //Compute total probability
        for (int x = 0; x < pillClasses.Length; x++)
            totalProbability += pillClasses[x].probability;

        //Initialize other stuff
        deathQueue = new List<Pill>();
        respawnQueue = new List<Pill>();

        //Get references
        city = GetComponent<City>();

        //Boot up spawner
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop ()
    {
        //Initial spawn wave
        while(alive < atOnce && prescriptions > 0)
            yield return StartCoroutine(CreatePill(pillPrefab, GetRandomPillClass()));

        //Spawn/respawn loop after that
        while (true)
        {
            //Spawn one pill at a time
            if (alive < atOnce && prescriptions > 0)
            {
                //Try to reuse/respawn pill if we can, otherwise create new one...

                if(respawnQueue.Count > 0) //Respawn pill
                {
                    //Pop head of queue
                    Pill toRespawn = respawnQueue[0];
                    respawnQueue.RemoveAt(0);

                    RespawnPill(toRespawn);
                }
                else if(deathQueue.Count == 0) //Create pill
                    yield return StartCoroutine(CreatePill(pillPrefab, GetRandomPillClass()));

                //Else, just wait a cycle for pill in death queue to move to respawn queue
            }

            //Death queue to respawn queue
            if(deathQueue.Count > 0)
            {
                //Pop head of death queue
                Pill deadPill = deathQueue[0];
                deathQueue.RemoveAt(0);

                //Insert in back of respawn queue
                respawnQueue.Add(deadPill);
            }

            //Spawning takes some time
            yield return new WaitForSeconds(Random.Range(1.0f, 1.5f));
        }
    }

    private IEnumerator CreatePill (GameObject pillPrefab, PillClass pillClass)
    {
        alive++;
        prescriptions--;

        //Instantiate pill
        Pill pill;
        if (!Player.player && Player.playerTeam == team) //One time creation of player
        {
            pill = Instantiate(playerPrefab).GetComponent<Pill>();

            //Loading is over, player is here everybody!
            Player.player = pill.GetComponent<Player>();
            PlanetPauseMenu.pauseMenu.LoadingScreen(false, false);
        }
        else
            pill = Instantiate(pillPrefab).GetComponent<Pill>();

        //Position and rotate pill
        if (city)
            pill.spawnPoint = city.GetNewSpawnPoint();
        SetSpawnPositionAndRotation(pill);

        //Create everlasting bond with pill
        pill.OnCreationFromSpawner(this);

        //Skin pill
        //pill.GetComponent<Renderer>().sharedMaterial = pillClass.skin;

        //Name pill
        /*
        if(pill.GetComponent<Player>())
            pill.name = "Player Pill";
        else
            pill.name = "Pill " + Random.Range(0, 1000);
        */

        //Give pill items and gear...

        //Instantiate primary
        Item primary = null;
        if (pillClass.primary)
        {
            primary = Instantiate(pillClass.primary, pill.transform.position, pill.transform.rotation).GetComponent<Item>();
            primary.name = primary.name.Substring(0, primary.name.Length - 7);
        }

        //Instantiate secondary
        Item secondary = null;
        if (pillClass.secondary)
        {
            secondary = Instantiate(pillClass.secondary, pill.transform.position, pill.transform.rotation).GetComponent<Item>();
            secondary.name = secondary.name.Substring(0, secondary.name.Length - 7);
        }

        //Wait a frame for start to be called on pill and items
        yield return null;

        //Equip items
        pill.Equip(primary, secondary);

        //Head gear
        if (pillClass.headGear)
            pill.EquipGear(Instantiate(pillClass.headGear), true);

        //Body gear
        if (pillClass.bodyGear)
            pill.EquipGear(Instantiate(pillClass.bodyGear), false);

        //Assign squad (all pills must have a squad they belong to)
        SetSquad(pill);

        //Life has been given!
        pill.BringToLife();
    }

    private void RespawnPill (Pill toRespawn)
    {
        alive++;
        prescriptions--;

        //If player, spawn corpse dummy on respawn
        if (toRespawn == Player.player.GetPill())
            SpawnCorpse(God.god.corpsePrefab, toRespawn.transform);

        SetSpawnPositionAndRotation(toRespawn);

        SetSquad(toRespawn);

        toRespawn.BringToLife();
    }

    private void SetSpawnPositionAndRotation (Pill pill)
    {
        if (city)
        {
            pill.transform.position = pill.spawnPoint;
            pill.transform.eulerAngles = Vector3.zero;
        }
        else
        {
            Transform pillTransform = pill.transform;

            //Put pill in center of spawn
            pillTransform.rotation = transform.rotation;
            pillTransform.position = transform.position;

            //Temporarily bind pill to spawn
            pillTransform.parent = transform;

            //Randomize its position in the spawn zone
            Vector3 localOffset = Vector3.zero;
            localOffset.x = transform.localScale.x * Random.Range(-0.5f, 0.5f);
            localOffset.z = transform.localScale.z * Random.Range(-0.5f, 0.5f);
            pillTransform.Translate(localOffset, Space.Self);

            //Unbind pill
            pillTransform.parent = null;
        }
    }

    public PillClass GetRandomPillClass ()
    {
        if (pillClasses.Length == 0)
            return null;

        PillClass selectedClass = null;

        //Pick a random class based on probability
        for(int x = 0; x < pillClasses.Length - 1; x++)
        {
            //Picked this class
            if(Random.Range(0, totalProbability) < pillClasses[x].probability)
            {
                selectedClass = pillClasses[x];
                break;
            }
        }

        if (selectedClass != null) //Already picked an item
            return selectedClass;
        else //Haven't picked class yet so pick last class which we saved for now
            return pillClasses[pillClasses.Length - 1];
    }

    public void ReportDeath (Pill deadPill, bool addToDeathQueue)
    {
        alive--;

        if(addToDeathQueue)
            deathQueue.Add(deadPill);
    }

    public void SetSquad (Pill pill)
    {
        if (!squad)
        {
            squad = Instantiate(squadPrefab).GetComponent<Squad>();
            squad.InitializeSquad(atOnce, squadType);
        }

        squad.ReportingForDuty(pill);
    }

    public static void SpawnCorpse (GameObject corpsePrefab, Transform fromPill)
    {
        //Create corpse dummy
        GameObject newCorpse = Instantiate(corpsePrefab, fromPill.position, fromPill.rotation);

        //Make corpse dummy look like us!
        newCorpse.GetComponent<MeshRenderer>().sharedMaterial = fromPill.GetComponent<MeshRenderer>().sharedMaterial;
        newCorpse.GetComponent<Rigidbody>().velocity = fromPill.GetComponent<Rigidbody>().velocity;
    }
}