using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IControllable
{
    [Header("UI Elements")]
    [SerializeField] private HealthBar healthBar;

    [Header("Drag to Inspector Elements")]
    [SerializeField] private InputActionReference move;

    [SerializeField] private PlayerData playerData;

    #region private variables

    private float currentHealth = 100;
    private float currentMovementSpeed = 4f;
    private float currentHitBoxRadius = 0.4f;

    private SpatialGroupManager spatialGroupManager;
    private int spatialGroup = -1;
    private int takeDamageEveryXFrames = 0;
    private int takeDamageEveryXFramesCD = 10;
    private bool gameIsStarted;

    // Nearest enemy position (for weapons)
    private Vector2 nearestEnemyPosition = Vector2.zero;

    private bool noNearbyEnemies = false;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    #endregion private variables

    [Header("Current Weapons")]
    [SerializeField] private Weapon[] currentWeapons;

    #region Public Getters and Setters

    public Vector2 NearestEnemyPosition
    {
        get { return nearestEnemyPosition; }
        set { nearestEnemyPosition = value; }
    }

    public int SpatialGroup
    { get { return spatialGroup; } }

    public bool NoNearbyEnemies
    {
        get { return noNearbyEnemies; }
        set { noNearbyEnemies = value; }
    }

    #endregion Public Getters and Setters

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

    public void Initialize(IControllable[] _injectedElements)
    {
        this.spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        spatialGroup = spatialGroupManager.GetSpatialGroup(transform.position.x, transform.position.y);

        if (playerData != null)
        {
            currentHealth = playerData.Health;
            currentMovementSpeed = playerData.MovementSpeed;
            currentHitBoxRadius = playerData.HitBoxRadius;

            currentWeapons = new Weapon[playerData.DefaultWeapons.Length];
            for (int i = 0; i < playerData.DefaultWeapons.Length; i++)
            {
                currentWeapons[i] = Instantiate(playerData.DefaultWeapons[i], transform);
                currentWeapons[i].Initialize(new IControllable[] { spatialGroupManager });
            }
        }
        else
        {
            Debug.LogError("Player Data is missing");
        }
    }

    private void Handle_StartGame()
    {
        gameIsStarted = true;
        healthBar.UpdateBar((float)currentHealth / (float)playerData.Health);
        StartCoroutine(PlayerMainLoop());
    }

    private void Handle_StopGame()
    {
        gameIsStarted = false;
        Destroy(gameObject);
    }

    private IEnumerator PlayerMainLoop()
    {
        float previousDirectionX = 0f; // Track the previous horizontal direction
        Vector3 movementVector = Vector3.zero;

        while (gameIsStarted)
        {
            movementVector = move.action.ReadValue<Vector2>();

            if (movementVector.x != 0 && Mathf.Sign(movementVector.x) != Mathf.Sign(previousDirectionX))
            {
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * -Mathf.Sign(movementVector.x);
                transform.localScale = localScale;
                previousDirectionX = movementVector.x;
            }

            transform.position += movementVector.normalized * Time.deltaTime * currentMovementSpeed;

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

            for (int i = 0; i < currentWeapons.Length; i++)
            {
                currentWeapons[i].Attack(transform.position, movementVector);
            }

            yield return waitForEndOfFrame;
        }
    }

    private void CheckCollisionWithEnemy()
    {
        List<int> surroundingSpatialGroups = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, Vector2.zero);
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllEnemiesInSpatialGroups(surroundingSpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < currentHitBoxRadius)
            {
                // Take damage
            //    ModifyHealth(enemy.Damage);

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

        if (!spatialGroupManager.enemySpatialGroups.ContainsKey(spatialGroup) || spatialGroupManager.enemySpatialGroups[spatialGroup].Count == 0)
        {
            // If no enemies in player's spatial group, expand search
            spatialGroupsToSearch = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, 6);
        }

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

    public void ModifyHealth(float amount)
    {
        currentHealth -= amount;
        healthBar.UpdateBar(currentHealth / playerData.Health);

        if (currentHealth <= 0)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        GameEvents.StopGame();
    }
}