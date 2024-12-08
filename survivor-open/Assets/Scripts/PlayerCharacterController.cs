using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterController : MonoBehaviour, IControllable
{

    [Header("Drag to Inspector Elements")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private PlayerCharacterData playerData;

    #region Injected Dependencies
    private SpatialGroupManager spatialGroupManager;
    private UserInterfaceManager userInterfaceManager;
    #endregion

    #region private variables

    private float currentHealth = 100;
    private float currentMovementSpeed = 4f;
    private float currentHitBoxRadius = 0.4f;
    private float currentCollectionRadius = 1f;
    private int currentLevel = 0;
    private int currentExperience = 0;
    private bool doubleXPGrowth;    
    private int spatialGroup = -1;
    private int takeDamageEveryXFrames = 0;
    private int takeDamageEveryXFramesCD = 10;
    private bool gameIsStarted;
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

    //public int SpatialGroup
    //{ get { return spatialGroup; } }

    public bool NoNearbyEnemies  // for weapons that need enemy targetting
    {
        get { return noNearbyEnemies; }
        set { noNearbyEnemies = value; }
    }

    #endregion Public Getters and Setters

    private void OnEnable()
    {
        GameEvents.OnDoStartGame += Handle_StartGame;
        GameEvents.OnDoStopGame += Handle_StopGame;
    }

    private void OnDisable()
    {
        GameEvents.OnDoStartGame -= Handle_StartGame;
        GameEvents.OnDoStopGame -= Handle_StopGame;
    }

    public void Initialize(IControllable[] _injectedElements)
    {
        this.spatialGroupManager = _injectedElements[0] as SpatialGroupManager;
        this.userInterfaceManager = _injectedElements[1] as UserInterfaceManager;


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
        userInterfaceManager.HPValue = currentHealth / playerData.Health;
       
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

                CheckCollisionWithPickable();
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
        List<Enemy> surroundingEnemies = spatialGroupManager.GetAllItemsInSpatialGroups<Enemy>(surroundingSpatialGroups, spatialGroupManager.enemySpatialGroups);

        foreach (Enemy enemy in surroundingEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < currentHitBoxRadius)
            {
                // Take damage
                ModifyHealth(enemy.Damage);
                break;
            }
        }
    }

    private void CheckCollisionWithPickable()
    {
        List<int> surroundingSpatialGroupsBatchIDs = spatialGroupManager.GetExpandedSpatialGroups(spatialGroup, Vector2.zero);
      
        List<Pickable> surroundingPickables = spatialGroupManager.GetAllItemsInSpatialGroups<Pickable>(surroundingSpatialGroupsBatchIDs, spatialGroupManager.pickableSpatialGroups);
        Debug.Log("Found pickables in area: " + surroundingPickables.Count);

        foreach (Pickable pickable in surroundingPickables)
        {
            if (pickable == null) continue;

            float distance = Vector2.Distance(transform.position, pickable.transform.position);
            if (distance <= currentCollectionRadius)
            {
                // Pickup
                HandlePickup(pickable);
           

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
        List<Enemy> nearbyEnemies = spatialGroupManager.GetAllItemsInSpatialGroups<Enemy>(spatialGroupsToSearch, spatialGroupManager.enemySpatialGroups);


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


    private void HandlePickup (Pickable pickable)
    {
        Debug.Log("Collsion with pickable detected");

        pickable.Handle_GivePickupBonus();

        int batchID = spatialGroupManager.GetBatchIDFromSpatialGroup(spatialGroup);
        spatialGroupManager.pickableSpatialGroups[batchID].Remove(pickable);

        Destroy(pickable.gameObject);
    }

    // based on https://vampire-survivors.fandom.com/wiki/Level_up

    public void GetExp(int _point)
    {
        if (doubleXPGrowth)
            _point *= 2;

        currentExperience += _point;
        CheckForLevelUp();
    }


    private void CheckForLevelUp()
    {

        int xpRequired = GetXPRequiredForNextLevel(currentLevel);
        float expProgression = (float)currentExperience / (float) xpRequired;

        Debug.Log("current progression " + expProgression);

        userInterfaceManager.ExpValue = expProgression;
      
        if (currentExperience >= xpRequired)
        {
            currentExperience -= xpRequired;
            currentLevel++;

            userInterfaceManager.LevelValue = currentLevel.ToString();
          
            // Activate double XP growth if reaching level 20 or 40
            if (currentLevel == 20 || currentLevel == 40)
                doubleXPGrowth = true;
            else
                doubleXPGrowth = false;

            Debug.Log($"Level up! New Level: {currentLevel}, Remaining XP: {currentExperience}");
        }
    }

   
    private int GetXPRequiredForNextLevel(int level)
    {
        if (level < 20)
        {
            return 5 + (level - 1) * 10;
        }
        else if (level == 20)
        {
            return 5 + (level - 1) * 10 + 600; // Extra XP requirement for level 20
        }
        else if (level < 40)
        {
            return 600 + (15 + (level - 21) * 13); // Base XP from level 20 + 13 XP per level
        }
        else if (level == 40)
        {
            return 600 + (15 + (level - 21) * 13) + 2400; // Extra XP requirement for level 40
        }
        else
        {
            return 2400 + (16 + (level - 41) * 16); // Base XP from level 40 + 16 XP per level
        }
    }


    private void ModifyHealth(float amount)
    {
        currentHealth -= amount;
        userInterfaceManager.HPValue = currentHealth / playerData.Health;
      
        if (currentHealth <= 0)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        GameEvents.DoStopGame();
    }
}