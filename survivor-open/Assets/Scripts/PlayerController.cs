
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // Stats
    int health = 100;
    int maxHealth = 100;
    float movementSpeed = 4f;


    // Spatial groups
    int spatialGroup = -1;
    public int SpatialGroup { get { return spatialGroup; } }

    // Taking damage from enemy
    int takeDamageEveryXFrames = 0;
    int takeDamageEveryXFramesCD = 10;
    float hitBoxRadius = 0.4f;

    // Nearest enemy position (for weapons)
    Vector2 nearestEnemyPosition = Vector2.zero;
    public Vector2 NearestEnemyPosition
    {
        get { return nearestEnemyPosition; }
        set { nearestEnemyPosition = value; }
    }

    bool noNearbyEnemies = false; // shoot randomly
    public bool NoNearbyEnemies
    {
        get { return noNearbyEnemies; }
        set { noNearbyEnemies = value; }
    }

    void Start()
    {
        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
    
    }

    void FixedUpdate()
    {
        Vector3 movementVector = Vector3.zero;

        // WASD to move around
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movementVector += Utils.V2toV3(new Vector2(0, 1));
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movementVector += Utils.V2toV3(new Vector2(-1, 0));
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movementVector += Utils.V2toV3(new Vector2(0, -1));
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movementVector += Utils.V2toV3(new Vector2(1, 0));

        transform.position += movementVector.normalized * Time.deltaTime * movementSpeed;

        // Calculate nearest enemy direction
        spatialGroup = GameController.instance.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
        CalculateNearestEnemyDirection();

        // Colliding with any enemy? Lose health?
        takeDamageEveryXFrames++;
        if (takeDamageEveryXFrames > takeDamageEveryXFramesCD)
        {
            CheckCollisionWithEnemy();
            takeDamageEveryXFrames = 0;
        }
    }

    void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatialGroups = Utils.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroundingEnemies = Utils.GetAllEnemiesInSpatialGroups(surroundingSpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        // foreach (Enemy enemy in GameController.instance.enemySpatialGroups[spatialGroup])
        {
            if (enemy == null) continue;

            // float distance = Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y);
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < hitBoxRadius)
            {
                // Take damage
                ModifyHealth(-enemy.Damage);

                break;
            }
        }
    }

    void CalculateNearestEnemyDirection()
    {
        // Just checks enemies in the same spatial group
        float minDistance = 100f;
        Vector2 closestPosition = Vector2.zero;
        bool foundATarget = false;

        List<int> spatialGroupsToSearch = new List<int>() { spatialGroup };
        spatialGroupsToSearch = Utils.GetExpandedSpatialGroups(spatialGroup, 6);

        // Get all enemies
        List<Enemy> nearbyEnemies = Utils.GetAllEnemiesInSpatialGroups(spatialGroupsToSearch);

        // No nearby enemies?
        if (nearbyEnemies.Count == 0)
        {
            noNearbyEnemies = true;
        }
        else
        {
            noNearbyEnemies = false;

            // Filter thru enemies
            foreach (Enemy enemy in nearbyEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPosition = enemy.transform.position;
                    foundATarget = true;
                }
            }

            if (!foundATarget) // Somehow no targets found? randomize
                noNearbyEnemies = true;
            else
                nearestEnemyPosition = closestPosition;
        }
    }

 
  
    public void ModifyHealth(int amount)
    {
        health += amount;

      
        if (health <= 0)
        {
            KillPlayer();
        }
    }

    void KillPlayer()
    {
     
        Destroy(gameObject);
    }
}
