using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int batchId;
    public int BatchID
    {
        get { return batchId; }
        set { batchId = value; }
    }

    public float movementSpeed = 3f;
    Vector3 currentMovementDirection = Vector3.zero;
    public int spatialGroup = 0;

    int health = 10;
    int damage = 5;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }


    public void RunLogic()
    {
        // Calculate direction
        currentMovementDirection = GameController.instance.player.position - transform.position;
        currentMovementDirection.Normalize();

        // Move towards the player //TODO: Maybe have this run every frame? (no, but can definetely be more)
        transform.position += currentMovementDirection * Time.deltaTime * movementSpeed;

        // Push other nearby enemies away
        PushNearbyEnemies();

        // Update spatial group
        int newSpatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
        if (newSpatialGroup != spatialGroup)
        {
            GameController.instance.enemySpatialGroups[spatialGroup].Remove(this); // REMOVE from old spatial group

            spatialGroup = newSpatialGroup; // UPDATE current spatial group
            GameController.instance.enemySpatialGroups[spatialGroup].Add(this); // ADD to new spatial group
        }
    }

    void PushNearbyEnemies()
    {
        List<Enemy> currAreaEnemies = GameController.instance.enemySpatialGroups[spatialGroup].ToList(); // ONLY enemies in the same spatial group

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
                enemy.transform.position -= direction * Time.deltaTime * movementSpeed * 5;
            }
        }
    }


    public void ChangeHealth(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            KillEnemy();
        }
    }

    public void KillEnemy()
    {
        Debug.Log("Enemy Killed");
        GameController.instance.RemoveFromSpatialGroup(batchId, this);
        GameController.instance.enemySpatialGroups[spatialGroup].Remove(this);

      

        Destroy(gameObject);
    }
}