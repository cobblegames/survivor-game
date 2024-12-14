using System.Collections.Generic;
using UnityEngine;

public enum PoolObjectType
{ Enemy = 0, Pickable = 1, Bullet = 2 }

public class PoolManager : MonoBehaviour, IControllable
{
    [System.Serializable]
    public class Pool
    {
        [SerializeField] private string poolName; // Identifier for the pool
        [SerializeField] private GameObject prefab; // Enemy prefab
        [SerializeField] private PoolObjectType poolObject;

        public string PoolName
        { get { return poolName; } }
        public GameObject Prefab
        { get { return prefab; } }
        public PoolObjectType PoolObject
        { get { return poolObject; } }
    }

    [SerializeField] private List<Pool> pools;
    [SerializeField] private Transform poolObjectHolder;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<GameObject, string> activeObjects; // Tracks which pool an object belongs to

    [SerializeField] private Transform enemyHolder;
    [SerializeField] private Transform pickupsHolder;

    public Transform EnemyHolder
    { get { return enemyHolder; } }
    public Transform PickupsHolder
    { get { return pickupsHolder; } }

    private SpatialGroupManager spatialGroupManager;

    private bool isInitialized = false;

    public bool IsInitialized
    { get { return isInitialized; } }

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

            GameObject obj = Instantiate(pool.Prefab, poolObjectHolder);
            obj.SetActive(false); // Deactivate the object initially
            objectPool.Enqueue(obj);

            poolDictionary.Add(pool.PoolName, objectPool);
        }

        isInitialized = true;
    }

    public GameObject SpawnFromPool(string poolName)
    {
        if (isInitialized == false)
        {
            return null;
        }

        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogError($"Pool with name {poolName} does not exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[poolName].Dequeue();

        activeObjects[objectToSpawn] = poolName;

        // Requeue the object back into the pool for future use
        poolDictionary[poolName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}