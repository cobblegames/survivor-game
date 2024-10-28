using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool gameIsStarted;

    private SpatialGroupManager spatialGroupManager;

    // Stats
    private int health = 100;

    private float movementSpeed = 4f;

    // Spatial groups
    private int spatialGroup = -1;

    public int SpatialGroup
    { get { return spatialGroup; } }

    // Taking damage from enemy
    private int takeDamageEveryXFrames = 0;

    private int takeDamageEveryXFramesCD = 10;
    private float hitBoxRadius = 0.4f;

    // Nearest enemy position (for weapons)
    private Vector2 nearestEnemyPosition = Vector2.zero;

    public Vector2 NearestEnemyPosition
    {
        get { return nearestEnemyPosition; }
        set { nearestEnemyPosition = value; }
    }

    private bool noNearbyEnemies = false;

    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    public bool NoNearbyEnemies
    {
        get { return noNearbyEnemies; }
        set { noNearbyEnemies = value; }
    }

    public void Initialize(SpatialGroupManager manager)
    {
        this.spatialGroupManager = manager;

        spatialGroup = spatialGroupManager.GetSpatialGroup(transform.position.x, transform.position.y);
    }

    private void OnEnable()
    {
        GameEvents.OnStartGame += Handle_StartGame;
        GameEvents.OnStopGame += Handle_StopGame;
    }

    private void OnDisable()
    {
        GameEvents.OnStartGame -= Handle_StartGame;
        GameEvents.OnStopGame -= Handle_StopGame;
    }

    private void Handle_StartGame()
    {
        gameIsStarted = true;

        StartCoroutine(PlayerMainLoop());
    }

    private void Handle_StopGame()
    {
        gameIsStarted = false;
        Destroy(gameObject);
    }

    private IEnumerator PlayerMainLoop()
    {
        while (gameIsStarted)
        {
            Vector3 movementVector = Vector3.zero;

            // WASD to move around
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movementVector += Utils.V2toV3(new Vector2(0, 1));
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movementVector += Utils.V2toV3(new Vector2(-1, 0));
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movementVector += Utils.V2toV3(new Vector2(0, -1));
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movementVector += Utils.V2toV3(new Vector2(1, 0));

            transform.position += movementVector.normalized * Time.deltaTime * movementSpeed;

            // Calculate nearest enemy direction
            if (spatialGroupManager != null)
            {
                spatialGroup = spatialGroupManager.GetSpatialGroup(transform.position.x, transform.position.y); // GET spatial group
                CalculateNearestEnemyDirection();

                // Colliding with any enemy? Lose health?
                takeDamageEveryXFrames++;
                if (takeDamageEveryXFrames > takeDamageEveryXFramesCD)
                {
                    CheckCollisionWithEnemy();
                    takeDamageEveryXFrames = 0;
                }

            }
         

            yield return waitForEndOfFrame;
        }
    }

    private void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatialGroups = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup);
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllEnemiesInSpatialGroups(surroundingSpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < hitBoxRadius)
            {
                // Take damage
                ModifyHealth(-enemy.Damage);

                break;
            }
        }
    }

    private void CalculateNearestEnemyDirection()
    {
        // Just checks enemies in the same spatial group
        float minDistance = 100f;
        Vector2 closestPosition = Vector2.zero;
        bool foundATarget = false;

        List<int> spatialGroupsToSearch = new List<int>() { spatialGroup };
        spatialGroupsToSearch = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, 6);

        // Get all enemies
        List<Enemy> nearbyEnemies = spatialGroupManager.GetAllEnemiesInSpatialGroups(spatialGroupsToSearch);

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

    private void KillPlayer()
    {
        GameEvents.StopGame();
    }
}