using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int batchId;

    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    [SerializeField] private EnemyData enemyData;

    private float currentSpeed;
    private int currentHealth;
    private int currentDamage;

    private Vector3 currentMovementDirection = Vector3.zero;
    private int spatialGroup = 0;

    public int SpatialGroup
    {
        get { return spatialGroup; }
        set { spatialGroup = value; }
    }

    public int Damage
    {
        get { return enemyData.Damage; }
     
    }

    private SpatialGroupManager spatialGroupManager;
    private PlayerController playerController;

    // SpatialGroupManager Injection
    public void Initialize(SpatialGroupManager manager, PlayerController _playerController)
    {
        this.spatialGroupManager = manager;
        this.playerController = _playerController;

        if (enemyData != null) 
        {
            currentHealth = enemyData.Health;
            currentSpeed = enemyData.MovementSpeed;
            currentDamage = enemyData.Damage;
        }else
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
            transform.position += currentMovementDirection * Time.deltaTime * currentSpeed;
            PushNearbyEnemies();
            int newSpatialGroup = spatialGroupManager.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
            if (newSpatialGroup != spatialGroup)
            {
                spatialGroupManager.enemySpatialGroups[spatialGroup].Remove(this); // REMOVE from old spatial group

                spatialGroup = newSpatialGroup; // UPDATE current spatial group
                spatialGroupManager.enemySpatialGroups[spatialGroup].Add(this); // ADD to new spatial group
            }
        }
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

    public void ChangeHealth(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            KillEnemy();
        }
    }

    public void KillEnemy()
    {
        Debug.Log("Enemy Killed");

        spatialGroupManager.RemoveFromSpatialGroup(batchId, this);

        spatialGroupManager.enemySpatialGroups[spatialGroup].Remove(this);

        Destroy(gameObject);
    }
}