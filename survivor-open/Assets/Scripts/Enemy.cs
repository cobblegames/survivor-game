using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, IMovable, IControllable
{
    private int batchId;

    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    [SerializeField] private EnemyData enemyData;

    [SerializeField] private float currentSpeed;
    private float currentHealth;
    private float currentDamage;

    private Vector3 currentMovementDirection = Vector3.zero;
    private int spatialGroup = 0;

    public int SpatialGroup
    {
        get { return spatialGroup; }
        set { spatialGroup = value; }
    }

    public float Damage
    {
        get { return enemyData.Damage; }
    }

    protected SpatialGroupManager spatialGroupManager;
    protected PlayerController playerController;
    protected PoolManager poolManager;
    public void Initialize(IControllable[] _injectedElements)
    {
        this.spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        this.playerController = _injectedElements[1] as PlayerController;
        this.poolManager = _injectedElements[2] as PoolManager;

        if (enemyData != null)
        {
            currentHealth = enemyData.Health;
            currentSpeed = enemyData.MovementSpeed;
            currentDamage = enemyData.Damage;
        }
        else
        {
            Debug.LogError("Enemy Scriptable Data is null");
        }
    }

    public void EveryFrameLogic()
    {
        // Calculate direction
        if (playerController != null)
        {
            currentMovementDirection = playerController.transform.position - transform.position;
            currentMovementDirection.Normalize();

            // Flip the sprite based on the x-axis direction
            if (currentMovementDirection.x != 0)
            {
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * Mathf.Sign(currentMovementDirection.x); // Flip based on direction
                transform.localScale = localScale;
            }

            transform.position += currentMovementDirection * Time.deltaTime * currentSpeed;

            int newSpatialGroup = spatialGroupManager.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
            if (newSpatialGroup != spatialGroup)
            {
                spatialGroupManager.enemySpatialGroups[spatialGroup].Remove(this); // REMOVE from old spatial group
                spatialGroup = newSpatialGroup; // UPDATE current spatial group
                spatialGroupManager.enemySpatialGroups[spatialGroup].Add(this); // ADD to new spatial group
            }
        }
        else
        {
            Debug.LogError("Player is null");
        }
    }

    public void IntervalLogic()
    {
        PushNearbyEnemies();
    }

    private void PushNearbyEnemies()
    {
        List<Enemy> currAreaEnemies = spatialGroupManager.enemySpatialGroups[spatialGroup].ToList(); // ONLY enemies in the same spatial group

        // Check each enemy, if the distance between them is less than 0.2, push it away
        foreach (Enemy enemy in currAreaEnemies) // ONLY enemies in the same spatial group
        {
            if (enemy == null) continue;
            if (enemy == this) continue;

            float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y);
            if (distance < 0.2f)
            {
                // Push this enemy away
                Vector3 direction = transform.position - enemy.transform.position;
                direction.Normalize();
                enemy.transform.position -= direction * Time.deltaTime * currentSpeed * 5;
            }
        }
    }

    public void ChangeHealth(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {       
            KillEnemy();
        }
    }

    public void KillEnemy()
    {
      
        GameObject pickupOBJ = Instantiate(poolManager.SpawnFromPool("exp1"), poolManager.PickupsHolder);
        Pickable pickup = pickupOBJ.GetComponent<Pickable>();
        pickup.transform.position = transform.position;
        pickupOBJ.SetActive(true);

        spatialGroupManager.RemoveFromSpatialGroup(batchId, this);
        spatialGroupManager.enemySpatialGroups[spatialGroup].Remove(this);

        Destroy(gameObject);
    }
}