using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour, IControllable
{
    // Singleton instance
    public static PoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string poolName; // Identifier for the pool
        public GameObject prefab; // Enemy prefab
        public int size; // Initial size of the pool
    }

    [SerializeField]
    private List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<GameObject, string> activeObjects; // Tracks which pool an object belongs to

    private Transform enemyHolder;
    public Transform EnemyHolder
    { get { return enemyHolder; } }

    SpatialGroupManager spatialGroupManager;


    public void Initialize(IControllable[] _injectedElements)
    {
        
    }


    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        activeObjects = new Dictionary<GameObject, string>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Instantiate initial objects for the pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // Deactivate the object initially
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolName, objectPool);
        }
    }

    public GameObject SpawnFromPool(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool with name {poolName} does not exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[poolName].Dequeue();

        // Reactivate and position the object
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Track this object as active and associate it with its pool
        activeObjects[objectToSpawn] = poolName;

        // Requeue the object back into the pool for future use
        poolDictionary[poolName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Attempted to return a null object to the pool.");
            return;
        }

        if (!activeObjects.ContainsKey(obj))
        {
            Debug.LogWarning($"Object {obj.name} does not belong to any pool managed by PoolManager.");
            return;
        }

        string poolName = activeObjects[obj];

        // Deactivate the object and re-enqueue it in its pool
        obj.SetActive(false);
        poolDictionary[poolName].Enqueue(obj);

        // Remove it from the active tracking dictionary
        activeObjects.Remove(obj);
    }

   
}
