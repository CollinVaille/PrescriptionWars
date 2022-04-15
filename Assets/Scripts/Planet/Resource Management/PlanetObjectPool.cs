using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Represents pooling for one type of object (ex: Explosion). Allows pooling for this type of object to be further permutated based on prefab name (Ex: Large Plasma Explosion).
 * 
 * Example:
 * PlanetObjectPool explosionPool = new PlanetObjectPool("Explosions/");
 * Explosion explosion = explosionPool.GetGameObject("Large Plasma Explosion").GetComponent<Explosion>();
 */ 

public class PlanetObjectPool
{
    //STATIC MANAGEMENT -------------------------------------------------------------------------------------------------------------

    public static void SetUpPooling()
    {
        Explosion.explosionPool = new PlanetObjectPool("Explosions/");
        Projectile.projectilePool = new PlanetObjectPool("Projectiles/");
        ImpactEffect.impactEffectPool = new PlanetObjectPool("Impact Effects/");
    }


    //POOL INSTANCES ----------------------------------------------------------------------------------------------------------------

    private Dictionary<string, List<GameObject>> pooledGameObjects;
    private string resourcePathPrefix = "";

    public PlanetObjectPool(string resourcePathPrefix)
    {
        pooledGameObjects = new Dictionary<string, List<GameObject>>();
        this.resourcePathPrefix = resourcePathPrefix;
    }

    //Retrieve game object from pool if one exists, else return newly created one
    public GameObject GetGameObject(string gameObjectName)
    {
        pooledGameObjects.TryGetValue(gameObjectName, out List<GameObject> gameObjects);

        GameObject theGameObject;

        //Happy day: list exists and is not empty
        //So return pooled game object
        if (gameObjects != null && gameObjects.Count != 0)
        {
            theGameObject = gameObjects[Random.Range(0, gameObjects.Count)];
            gameObjects.Remove(theGameObject);
            return theGameObject;
        }

        //Sad day: either game object doesn't exist or is empty
        //So return newly game object
        theGameObject = God.god.InstantiateSomeShit(Resources.Load<GameObject>(GetResourcePath(gameObjectName)));
        theGameObject.GetComponent<PlanetPooledObject>().OneTimeSetUp();
        return theGameObject;
    }

    //Put game object in pool unless pool is full, in which case destroy game object
    public void PoolGameObject(GameObject theGameObject)
    {
        theGameObject.GetComponent<PlanetPooledObject>().CleanUp();

        pooledGameObjects.TryGetValue(theGameObject.name, out List<GameObject> gameObjects);

        if (gameObjects == null) //No such pool, so create pool and add game object to it
        {
            gameObjects = new List<GameObject>(); //Create pool
            gameObjects.Add(theGameObject); //Add game object to pool
            pooledGameObjects.Add(theGameObject.name, gameObjects); //Add pool to list of pools
        }
        else //Found game object pool, so see if game object fits...
        {
            if (gameObjects.Count > 20) //Too many pooled so just game object
                God.god.DestroySomeShit(theGameObject);
            else //There's still room in pool, so put it in there
                gameObjects.Add(theGameObject);
        }
    }

    private string GetResourcePath(string gameObjectName) { return "Planet/" + resourcePathPrefix + gameObjectName; }
}
