using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour, IControllable
{
    [System.Serializable]
    public class Pool
    {
        public string poolName; // Identifier for the pool
        public GameObject prefab; // Enemy prefab
    }

    [SerializeField] private List<Pool> pools;
    [SerializeField] private Transform poolObjectHolder;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<GameObject, string> activeObjects; // Tracks which pool an object belongs to

    [SerializeField] private Transform enemyHolder;

    public Transform EnemyHolder
    { get { return enemyHolder; } }

    private SpatialGroupManager spatialGroupManager;

    public void Initialize(IControllable[] _injectedElements)
    {
        spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        activeObjects = new Dictionary<GameObject, string>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Instantiate initial objects for the pool

            GameObject obj = Instantiate(pool.prefab, poolObjectHolder);
            obj.SetActive(false); // Deactivate the object initially
            objectPool.Enqueue(obj);

            poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    public GameObject SpawnFromPool(string poolName)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool with name {poolName} does not exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[poolName].Dequeue();

        //// Reactivate and position the object
        //objectToSpawn.SetActive(true);
        //objectToSpawn.transform.position = position;
        //objectToSpawn.transform.rotation = rotation;

        // Track this object as active and associate it with its pool
        activeObjects[objectToSpawn] = poolName;

        // Requeue the object back into the pool for future use
        poolDictionary[poolName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}